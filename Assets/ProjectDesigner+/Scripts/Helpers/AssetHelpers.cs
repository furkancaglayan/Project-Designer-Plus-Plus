using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Extensions
{
    /// <summary>
    /// Helper class for asset-related operations.
    /// </summary>
    public static class AssetHelpers
    {
        /// <summary>
        /// Creates folders for the given <paramref name="path"/> recursively. No need to include "Assets" in the path.
        /// </summary>
        /// <param name="path">A directory in the "Assets" folder.</param>
        public static void CreateFolderRecursive(string path)
        {
            string[] folders = path.Split('/');
            string parent = "Assets/";

            foreach (string folder in folders) 
            {
                if (folder == "Assets" || string.IsNullOrEmpty(folder))
                {
                    continue;
                }

                if (folder.Contains("."))
                {
                    break;
                }

                if (!AssetDatabase.IsValidFolder(parent + folder))
                {
                    AssetDatabase.CreateFolder(parent.TrimEnd('/'), folder);
                }
                parent += $"{folder}/";
            }
        }

        /// <summary>
        /// Creates a <typeparamref name="T"/> scriptable object in the given path. Path does not need "Assets" at the beginning. See: <see cref="CreateFolderRecursive"/><br></br>
        /// <paramref name="assetName"/> should be used without extension.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">A directory in the "Assets" folder.</param>
        /// <param name="assetName">Name of the asset without extension.</param>
        /// <returns></returns>
        public static T CreateAsset<T>(string path, string assetName) where T: ScriptableObject
        {
            int index = 0;
            CreateFolderRecursive(path);
            string assetNameWithIndex = assetName;
            while (AssetDatabase.LoadAssetAtPath<Object>(path + assetNameWithIndex + ".asset") != null)
            {
                // Increment the index and update the path
                index++;
                assetNameWithIndex = assetName + $"({index})";
            }

            assetName = $"{assetNameWithIndex}.asset";
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path + assetName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }
    }
}
