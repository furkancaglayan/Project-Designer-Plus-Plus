using ProjectDesigner.V2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.V2.Editor
{
    [FilePath("ProjectSettings/ProjectDesignerSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class ProjectDesignerSettings : ScriptableSingleton<ProjectDesignerSettings>
    {
        [SerializeField]
        private bool _showOnboardingOnStartup = true;

        [SerializeField]
        private bool _showOnboardingForPackageUpdates = true;

        [SerializeField]
        private bool _autoOpenWorkspaceAfterBoardCreation = true;

        [SerializeField]
        private string _defaultBoardFolder = "Assets/Project Designer Boards";

        [SerializeField]
        private string _lastSeenOnboardingVersion = string.Empty;

        [SerializeField]
        private ProjectDesignerThemeMode _editorTheme = ProjectDesignerThemeConfig.DefaultTheme;
        [SerializeField]
        private ProjectDesignerTeamRosterAsset _defaultTeamRoster;
        [SerializeField]
        private ProjectDesignerBoardFinderSortMode _projectFinderSortMode = ProjectDesignerBoardFinderSortMode.RecentlyOpened;
        [SerializeField]
        private List<string> _pinnedBoardGuids = new List<string>();
        [SerializeField]
        private List<string> _recentBoardGuids = new List<string>();

        public bool ShowOnboardingOnStartup
        {
            get { return _showOnboardingOnStartup; }
        }

        public bool ShowOnboardingForPackageUpdates
        {
            get { return _showOnboardingForPackageUpdates; }
        }

        public bool AutoOpenWorkspaceAfterBoardCreation
        {
            get { return _autoOpenWorkspaceAfterBoardCreation; }
        }

        public string DefaultBoardFolder
        {
            get { return NormalizeAssetFolder(_defaultBoardFolder); }
        }

        public string LastSeenOnboardingVersion
        {
            get { return _lastSeenOnboardingVersion ?? string.Empty; }
        }

        public ProjectDesignerThemeMode EditorTheme
        {
            get { return ProjectDesignerThemeConfig.ResolveTheme(_editorTheme); }
        }

        public ProjectDesignerTeamRosterAsset DefaultTeamRoster
        {
            get { return _defaultTeamRoster; }
        }

        public ProjectDesignerBoardFinderSortMode ProjectFinderSortMode
        {
            get { return _projectFinderSortMode; }
        }

        public IReadOnlyList<string> PinnedBoardGuids
        {
            get
            {
                EnsureFinderDefaults();
                return _pinnedBoardGuids;
            }
        }

        public IReadOnlyList<string> RecentBoardGuids
        {
            get
            {
                EnsureFinderDefaults();
                return _recentBoardGuids;
            }
        }

        public bool ShouldShowOnboarding(string currentVersion)
        {
            if (!_showOnboardingOnStartup)
            {
                return false;
            }

            if (string.IsNullOrEmpty(_lastSeenOnboardingVersion))
            {
                return true;
            }

            if (string.Equals(_lastSeenOnboardingVersion, currentVersion, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return _showOnboardingForPackageUpdates;
        }

        public void MarkOnboardingSeen(string currentVersion)
        {
            _lastSeenOnboardingVersion = currentVersion ?? string.Empty;
            Save(true);
        }

        public void SetShowOnboardingOnStartup(bool value)
        {
            _showOnboardingOnStartup = value;
            Save(true);
        }

        public void SetEditorTheme(ProjectDesignerThemeMode value)
        {
            _editorTheme = ProjectDesignerThemeConfig.ResolveTheme(value);
            Save(true);
        }

        public void SetDefaultTeamRoster(ProjectDesignerTeamRosterAsset value)
        {
            _defaultTeamRoster = value;
            Save(true);
        }

        public void SetProjectFinderSortMode(ProjectDesignerBoardFinderSortMode value)
        {
            _projectFinderSortMode = value;
            Save(true);
        }

        public bool IsBoardPinned(string boardGuid)
        {
            EnsureFinderDefaults();
            return !string.IsNullOrWhiteSpace(boardGuid) && _pinnedBoardGuids.Contains(boardGuid, StringComparer.OrdinalIgnoreCase);
        }

        public bool TogglePinnedBoard(string boardGuid)
        {
            EnsureFinderDefaults();
            string normalizedGuid = NormalizeGuid(boardGuid);
            if (string.IsNullOrEmpty(normalizedGuid))
            {
                return false;
            }

            int index = _pinnedBoardGuids.FindIndex(existing => string.Equals(existing, normalizedGuid, StringComparison.OrdinalIgnoreCase));
            bool isPinned;
            if (index >= 0)
            {
                _pinnedBoardGuids.RemoveAt(index);
                isPinned = false;
            }
            else
            {
                _pinnedBoardGuids.Insert(0, normalizedGuid);
                isPinned = true;
            }

            Save(true);
            return isPinned;
        }

        public void MarkBoardOpened(string boardGuid)
        {
            EnsureFinderDefaults();
            string normalizedGuid = NormalizeGuid(boardGuid);
            if (string.IsNullOrEmpty(normalizedGuid))
            {
                return;
            }

            _recentBoardGuids.RemoveAll(existing => string.Equals(existing, normalizedGuid, StringComparison.OrdinalIgnoreCase));
            _recentBoardGuids.Insert(0, normalizedGuid);
            while (_recentBoardGuids.Count > 12)
            {
                _recentBoardGuids.RemoveAt(_recentBoardGuids.Count - 1);
            }

            Save(true);
        }

        public void RemoveBoardTracking(string boardGuid)
        {
            EnsureFinderDefaults();
            string normalizedGuid = NormalizeGuid(boardGuid);
            if (string.IsNullOrEmpty(normalizedGuid))
            {
                return;
            }

            _recentBoardGuids.RemoveAll(existing => string.Equals(existing, normalizedGuid, StringComparison.OrdinalIgnoreCase));
            _pinnedBoardGuids.RemoveAll(existing => string.Equals(existing, normalizedGuid, StringComparison.OrdinalIgnoreCase));
            Save(true);
        }

        public void SaveSettings()
        {
            Save(true);
        }

        private void EnsureFinderDefaults()
        {
            _pinnedBoardGuids = NormalizeGuidList(_pinnedBoardGuids);
            _recentBoardGuids = NormalizeGuidList(_recentBoardGuids);
        }

        private static string NormalizeGuid(string guid)
        {
            return string.IsNullOrWhiteSpace(guid) ? string.Empty : guid.Trim();
        }

        private static List<string> NormalizeGuidList(List<string> guids)
        {
            List<string> source = guids ?? new List<string>();
            var normalized = new List<string>(source.Count);
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string guid in source)
            {
                string normalizedGuid = NormalizeGuid(guid);
                if (string.IsNullOrEmpty(normalizedGuid) || !seen.Add(normalizedGuid))
                {
                    continue;
                }

                normalized.Add(normalizedGuid);
            }

            return normalized;
        }

        public static string NormalizeAssetFolder(string path)
        {
            string normalized = string.IsNullOrWhiteSpace(path) ? "Assets" : path.Trim().Replace("\\", "/");
            if (!normalized.StartsWith("Assets", StringComparison.Ordinal))
            {
                return "Assets";
            }

            while (normalized.Contains("//"))
            {
                normalized = normalized.Replace("//", "/");
            }

            normalized = normalized.TrimEnd('/');
            return normalized;
        }
    }
}
