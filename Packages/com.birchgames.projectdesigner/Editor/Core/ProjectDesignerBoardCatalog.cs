using System.Collections.Generic;
using System.Linq;
using ProjectDesigner.V2.Data;
using UnityEditor;

namespace ProjectDesigner.V2.Editor
{
    internal static class ProjectDesignerBoardCatalog
    {
        public static IReadOnlyList<ProjectBoardAsset> GetBoards()
        {
            return GetBoardEntries().Select(entry => entry.Board).Where(board => board != null).ToList();
        }

        public static IReadOnlyList<ProjectDesignerBoardCatalogEntry> GetBoardEntries()
        {
            string[] guids = AssetDatabase.FindAssets("t:ProjectBoardAsset");
            if (guids == null || guids.Length == 0)
            {
                return new List<ProjectDesignerBoardCatalogEntry>();
            }

            ProjectDesignerSettings settings = ProjectDesignerSettings.instance;
            IReadOnlyList<string> pinnedGuids = settings.PinnedBoardGuids;
            IReadOnlyList<string> recentGuids = settings.RecentBoardGuids;

            return guids
                .Select(guid => CreateEntry(guid, pinnedGuids, recentGuids))
                .Where(entry => entry != null && entry.Board != null)
                .OrderBy(entry => entry.BoardName)
                .ToList();
        }

        public static IReadOnlyList<ProjectDesignerBoardCatalogEntry> SearchAndSort(IReadOnlyList<ProjectDesignerBoardCatalogEntry> entries, string searchQuery, ProjectDesignerBoardFinderSortMode sortMode)
        {
            IEnumerable<ProjectDesignerBoardCatalogEntry> filteredEntries = entries ?? new List<ProjectDesignerBoardCatalogEntry>();
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                string normalizedQuery = searchQuery.Trim().ToLowerInvariant();
                filteredEntries = filteredEntries.Where(entry => MatchesSearch(entry, normalizedQuery));
            }

            switch (sortMode)
            {
                case ProjectDesignerBoardFinderSortMode.BoardName:
                    return filteredEntries
                        .OrderByDescending(entry => entry.IsPinned)
                        .ThenBy(entry => entry.BoardName)
                        .ToList();

                case ProjectDesignerBoardFinderSortMode.MostCards:
                    return filteredEntries
                        .OrderByDescending(entry => entry.IsPinned)
                        .ThenByDescending(entry => entry.NodeCount)
                        .ThenByDescending(entry => entry.EdgeCount)
                        .ThenBy(entry => entry.BoardName)
                        .ToList();

                case ProjectDesignerBoardFinderSortMode.MostActiveWork:
                    return filteredEntries
                        .OrderByDescending(entry => entry.IsPinned)
                        .ThenByDescending(entry => entry.InProgressCount)
                        .ThenByDescending(entry => entry.NodeCount)
                        .ThenBy(entry => entry.BoardName)
                        .ToList();

                default:
                    return filteredEntries
                        .OrderByDescending(entry => entry.IsPinned)
                        .ThenBy(entry => entry.RecentIndex < 0 ? int.MaxValue : entry.RecentIndex)
                        .ThenBy(entry => entry.BoardName)
                        .ToList();
            }
        }

        private static ProjectDesignerBoardCatalogEntry CreateEntry(string guid, IReadOnlyList<string> pinnedGuids, IReadOnlyList<string> recentGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ProjectBoardAsset board = AssetDatabase.LoadAssetAtPath<ProjectBoardAsset>(path);
            if (board == null)
            {
                return null;
            }

            return new ProjectDesignerBoardCatalogEntry
            {
                Board = board,
                Guid = guid,
                AssetPath = path,
                BoardName = board.Document.BoardName,
                Summary = board.Document.Summary,
                TeamSnapshot = string.Join(", ", board.Document.TeamMembers),
                NodeCount = board.Document.Nodes.Count,
                EdgeCount = board.Document.Edges.Count,
                InProgressCount = BoardInsights.CountTasksByStatus(board.Document, TaskNodeStatus.InProgress),
                IsPinned = pinnedGuids.Any(existingGuid => string.Equals(existingGuid, guid, System.StringComparison.OrdinalIgnoreCase)),
                RecentIndex = GetRecentIndex(guid, recentGuids)
            };
        }

        private static bool MatchesSearch(ProjectDesignerBoardCatalogEntry entry, string normalizedQuery)
        {
            if (entry == null || string.IsNullOrEmpty(normalizedQuery))
            {
                return true;
            }

            return (entry.BoardName ?? string.Empty).ToLowerInvariant().Contains(normalizedQuery) ||
                   (entry.Summary ?? string.Empty).ToLowerInvariant().Contains(normalizedQuery) ||
                   (entry.AssetPath ?? string.Empty).ToLowerInvariant().Contains(normalizedQuery) ||
                   (entry.TeamSnapshot ?? string.Empty).ToLowerInvariant().Contains(normalizedQuery);
        }

        private static int GetRecentIndex(string guid, IReadOnlyList<string> recentGuids)
        {
            for (int i = 0; i < recentGuids.Count; i++)
            {
                if (string.Equals(recentGuids[i], guid, System.StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
