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
            string[] guids = AssetDatabase.FindAssets("t:ProjectBoardAsset");
            if (guids == null || guids.Length == 0)
            {
                return new List<ProjectBoardAsset>();
            }

            return guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<ProjectBoardAsset>(path))
                .Where(board => board != null)
                .OrderBy(board => board.Document.BoardName)
                .ToList();
        }
    }
}
