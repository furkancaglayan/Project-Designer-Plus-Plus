using ProjectDesigner.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// ProjectDesignerSettings is a scriptable object class used for storing the settings of the tool.
    /// </summary>
    public class ProjectDesignerSettings : ScriptableObject
    {
        /// <summary>
        /// Name of the settings asset.
        /// </summary>
        public static string AssetName => "ProjectDesignerSettings";
        /// <summary>
        /// Path of the settings asset.
        /// </summary>
        public static string SettingsPath => ProjectDesigner.SettingsAssetFolder + AssetName;
        /// <summary>
        /// Spacing of the editor grids.
        /// </summary>
        [SerializeField]
        public int GridSpacing = 50;
        /// <summary>
        /// Default font used in the editor.
        /// </summary>
        [SerializeField, HideInInspector]
        public Font DefaultFont;
        /// <summary>
        /// Custom font to use in the editor. If it is not null, it will override <see cref="DefaultFont"/>.
        /// </summary>
        [SerializeField]
        public Font CustomFont;
        /// <summary>
        /// Style container.
        /// </summary>
        [SerializeField]
        public GUIStyleCollection Styles;
        /// <summary>
        /// Default textures used in the asset.
        /// </summary>
        [SerializeField, HideInInspector]
        public SerializableDictionary<string, Texture2D> DefaultTextures;
        /// <summary>
        /// Custom textures to use in the asset. If a texture with the same name with the one of the textures in <see cref="DefaultTextures"/> is used, it will override the one in it.
        /// </summary>
        [SerializeField]
        public SerializableDictionary<string, Texture2D> CustomTextures;
        /// <summary>
        /// Team members defined in the asset.
        /// </summary>
        [SerializeField]
        public List<TeamMember> TeamMembers;

        /// <summary>
        /// <see cref="ProjectDesignerSettings"/> static instance.
        /// </summary>
        public static ProjectDesignerSettings Instance
        {
            get
            {
                var settings = AssetDatabase.LoadAssetAtPath<ProjectDesignerSettings>(ProjectDesigner.SettingsAssetFolder + AssetName + ".asset");
                if (settings == null)
                {
                    settings = CreateNewSettings();
                }
                return settings;
            }
        }

        /// <summary>
        /// Returns the <see cref="Instance"/> as a <see cref="SerializedObject"/>.
        /// </summary>
        /// <returns></returns>
        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(Instance);
        }

        private static ProjectDesignerSettings CreateNewSettings()
        {
            ProjectDesignerSettings designer = AssetHelpers.CreateAsset<ProjectDesignerSettings>(ProjectDesigner.SettingsAssetFolder, AssetName);
            designer.DefaultTextures = GUIStyleCollection.LoadAssetsFromResources<Texture2D>($"Textures/");
            designer.CustomTextures = new SerializableDictionary<string, Texture2D>();
            designer.DefaultFont = GUIStyleCollection.LoadAssetFromResources<Font>("Fonts", "Raleway-SemiBold");
            designer.TeamMembers = new List<TeamMember>
            {
                new TeamMember("Developer 1", "Developer", GUIStyleCollection.GetTexture("script")),
                new TeamMember("Designer 1", "Designer", GUIStyleCollection.GetTexture("note")),
                new TeamMember("Animator 1", "Animator", GUIStyleCollection.GetTexture("d_Avatar Icon")),
                new TeamMember("Composer 1", "Audio", GUIStyleCollection.GetTexture("AudioClip Icon"))
            };

            designer.Styles = GUIStyleCollection.GetDefaultStyles();
            EditorUtility.SetDirty(designer);   
            return designer;
        }

        /// <summary>
        /// Checks if there is a settings asset in <see cref="ProjectDesigner.SettingsAssetFolder"/>. If it's missing, creates it.
        /// </summary>
        public static void Check()
        {
            _ = Instance;
        }

        /// <summary>
        /// Opens the Project Designer+ section in the Preferences.
        /// </summary>
        [MenuItem("Tools/Project Designer/Settings", false, priority = 10)]
        public static void OpenSettings()
        {
            Check();
            SettingsService.OpenUserPreferences(ProjectDesigner.PreferencesPath);
        }

        /// <summary>
        /// Opens a settings asset in the project browser when double clicking on it. See: <see cref="OnOpenAssetAttribute"/>.
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        [OnOpenAsset]
        public static bool OpenSettings(int instanceID, int line)
        {
            ProjectDesignerSettings settings = EditorUtility.InstanceIDToObject(instanceID) as ProjectDesignerSettings;
            if (settings != null)
            {
                OpenSettings();
                return true;
            }
            return false;
        }
    }
}
