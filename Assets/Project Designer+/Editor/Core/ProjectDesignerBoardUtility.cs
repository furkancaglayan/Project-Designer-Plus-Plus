using System.IO;
using ProjectDesigner.V2.BuiltIn;
using ProjectDesigner.V2.Data;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.V2.Editor
{
    internal static class ProjectDesignerBoardUtility
    {
        public static ProjectBoardAsset CreateBoardAsset(string presetId, string boardName)
        {
            string targetFolder = EnsureAssetFolderExists(GetTargetFolder());
            string safeName = string.IsNullOrWhiteSpace(boardName) ? ProjectDesignerProductInfo.DefaultBoardName : boardName.Trim();
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(targetFolder + "/" + safeName + ".asset");

            ProjectBoardAsset asset = ScriptableObject.CreateInstance<ProjectBoardAsset>();
            asset.name = Path.GetFileNameWithoutExtension(assetPath);
            asset.ResetDocument(BoardPresetFactory.Create(presetId, asset.name));

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
            MarkDirty(asset);
            return asset;
        }

        public static void MarkDirty(ProjectBoardAsset board)
        {
            if (board == null)
            {
                return;
            }

            EditorUtility.SetDirty(board);
        }

        public static string GetTargetFolder()
        {
            string defaultPath = ProjectDesignerSettings.instance.DefaultBoardFolder;
            Object selectedObject = Selection.activeObject;
            if (selectedObject == null)
            {
                return defaultPath;
            }

            string path = AssetDatabase.GetAssetPath(selectedObject);
            return ResolveBoardCreationFolder(path, AssetDatabase.IsValidFolder(path), defaultPath);
        }

        internal static string ResolveBoardCreationFolder(string selectedAssetPath, bool selectedPathIsFolder, string fallbackFolder)
        {
            string fallback = ProjectDesignerSettings.NormalizeAssetFolder(fallbackFolder);
            if (!selectedPathIsFolder || string.IsNullOrWhiteSpace(selectedAssetPath))
            {
                return fallback;
            }

            string normalizedSelection = selectedAssetPath.Trim().Replace("\\", "/");
            if (!IsAssetsFolderPath(normalizedSelection))
            {
                return fallback;
            }

            return ProjectDesignerSettings.NormalizeAssetFolder(normalizedSelection);
        }

        private static bool IsAssetsFolderPath(string path)
        {
            return string.Equals(path, "Assets", System.StringComparison.Ordinal) ||
                   path.StartsWith("Assets/", System.StringComparison.Ordinal);
        }

        internal static string EnsureAssetFolderExists(string assetFolder)
        {
            string normalizedPath = ProjectDesignerSettings.NormalizeAssetFolder(assetFolder);
            if (AssetDatabase.IsValidFolder(normalizedPath))
            {
                return normalizedPath;
            }

            string[] segments = normalizedPath.Split('/');
            string currentPath = "Assets";
            for (int i = 1; i < segments.Length; i++)
            {
                string nextPath = currentPath + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segments[i]);
                }

                currentPath = nextPath;
            }

            return currentPath;
        }
    }
}
