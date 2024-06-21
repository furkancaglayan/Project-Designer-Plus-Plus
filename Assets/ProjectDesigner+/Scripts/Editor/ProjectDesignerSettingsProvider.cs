using ProjectDesigner.Core;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectDesigner.Editor
{
    public class ProjectDesignerSettingsProvider : SettingsProvider
    {
        class Styles
        {
            public static GUIContent GridSpacing = new GUIContent("Grid Spacing");
            public static GUIContent Font = new GUIContent("Custom Font");
            public static GUIContent MembersStyle = new GUIContent("Team TeamMembers");
            public static GUIContent CustomTextures = new GUIContent("Custom Textures");
            public static GUIContent OpenOnLaunch = new GUIContent("Open How to Start Window when Unity Opens");
        }

        private SerializedObject _settings;

        public ProjectDesignerSettingsProvider(string path): base(path, SettingsScope.User) { }

        public static bool IsSettingsAvailable()
        {
            return File.Exists(ProjectDesignerSettings.SettingsPath + ".asset");
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _settings = ProjectDesignerSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            ProjectDesignerSettings settings = ProjectDesignerSettings.Instance;

            GUILayout.Label("General", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_settings.FindProperty(nameof(settings.CustomFont)), Styles.Font);
            EditorGUILayout.PropertyField(_settings.FindProperty(nameof(settings.GridSpacing)), Styles.GridSpacing);
            GUILayout.Space(15);

            GUILayout.Label("Styles", EditorStyles.boldLabel);
            SerializedProperty customTextures = _settings.FindProperty(nameof(settings.CustomTextures));
            if (customTextures != null)
            {
                EditorGUILayout.PropertyField(customTextures, Styles.CustomTextures);
            }
            GUILayout.Space(15);

            GUILayout.Label("Team TeamMembers", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_settings.FindProperty(nameof(settings.TeamMembers)), Styles.MembersStyle);

            _settings.ApplyModifiedPropertiesWithoutUndo();
        }

        [SettingsProvider]
        public static SettingsProvider CreateProjectDesignerSettingsProvider()
        {
            if (IsSettingsAvailable())
            {
                var provider = new ProjectDesignerSettingsProvider(ProjectDesigner.Core.ProjectDesigner.PreferencesPath)
                {
                    keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
                };
                return provider;
            }

            return null;
        }
    }
}
