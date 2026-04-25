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
            string targetFolder = GetTargetFolder();
            string safeName = string.IsNullOrWhiteSpace(boardName) ? "Project Board" : boardName.Trim();
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
            string defaultPath = "Assets";
            Object selectedObject = Selection.activeObject;
            if (selectedObject == null)
            {
                return defaultPath;
            }

            string path = AssetDatabase.GetAssetPath(selectedObject);
            if (string.IsNullOrEmpty(path))
            {
                return defaultPath;
            }

            if (File.Exists(path))
            {
                string directoryName = Path.GetDirectoryName(path);
                return string.IsNullOrEmpty(directoryName) ? defaultPath : directoryName.Replace("\\", "/");
            }

            return path.Replace("\\", "/");
        }
    }
}
