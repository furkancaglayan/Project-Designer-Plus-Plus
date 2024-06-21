using ProjectDesigner.Core;
using ProjectDesigner.Extensions;
using ProjectDesigner.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Editor
{
    [InitializeOnLoad]
    public class HowToWindow : UnityEditor.EditorWindow
    {
        private const float Width = 600;
        private const float Height = 600;
        private const float Padding = 15;
        private const float LabelWidth = 45;
        private const int SubjectCharacterMinLimit = 8;
        private const int SubjectCharacterMaxLimit = 40;
        private const int MessageCharacterMinLimit = 60;
        private const int MessageCharacterMaxLimit = 1000;
        private const int NameCharacterMinLimit = 10;
        private const int NameCharacterMaxLimit = 60;
        private Vector2 _scroll;

        [MenuItem("Tools/Project Designer/How to Start?", false, priority = 112)]
        public static void CreateWindow()
        {
            HowToWindow window = CreateWindow<HowToWindow>();
            window.maxSize = new Vector2(Width, Height);
            window.minSize = window.maxSize;
            window.titleContent = new GUIContent("How to Start?");
            window.Show();
        }

        static HowToWindow()
        {
            EditorApplication.update += ShowHowToWindow;
        }

        static void ShowHowToWindow()
        {
            bool launchedBefore = EditorPrefs.GetBool("ProjectDesigner_howto");
            if (!launchedBefore)
            {
                EditorPrefs.SetBool("ProjectDesigner_howto", true);
                CreateWindow(); 
            }

            EditorApplication.update -= ShowHowToWindow;
        }


        private void OnGUI()
        {
            CustomGUILayout.BeginArea(new Rect(Padding, Padding, Width - 2 * Padding, Height - 2 * Padding));

            using (GUILayout.ScrollViewScope scope = CustomGUILayout.CreateScrollView(_scroll, w: Width - 2 * Padding, h: Height))
            {
                _scroll = scope.scrollPosition;
                CustomGUILayout.BeginVertical(w: Width - 2 * Padding);
                GUIStyle label = new GUIStyle(EditorStyles.miniLabel);
                label.richText = true;
                label.wordWrap = true;
                CustomGUILayout.BeginHorizontal();
                Texture2D icon = GUIStyleCollection.GetTexture("project_designer_icon");
                if (icon != null)
                {
                    CustomGUILayout.Image(icon, w: 128, h: 128);
                }
                CustomGUILayout.BeginVertical();
                CustomGUILayout.Space(35);
                CustomGUILayout.Label("Project Designer+", EditorStyles.largeLabel);
                CustomGUILayout.EndVertical();
                CustomGUILayout.EndHorizontal();

                CustomGUILayout.Label("<color=orange>Project Designer</color> is a <color=cyan>versatile</color> node editor built in Unity, designed to simplify project management and planning.", label);
                CustomGUILayout.Label("<color=orange>Project Designer</color> also offers <color=cyan>modular</color> capabilities, allowing users to extend its functionality and tailor it to their unique project requirements.", label);
                CustomGUILayout.Label("<color=orange>Project Designer</color> is easy-to-use, easy-to-create and works <color=red>out of the box</color>.", label);
                CustomGUILayout.Space(10);
                ShowExampleAssets(label, icon);

                CustomGUILayout.Space(15);
                CustomGUILayout.Label("How to Open a New Project?", EditorStyles.largeLabel);
                CustomGUILayout.Label("To create a new <color=lightblue>Project</color>:\r\n\r\n1. Go under Tools/Project Designer\r\n\r\n2. Right click on project browser and hit Create/Project Designer/New Project\r\n\r\nor\r\n\r\n3. Hit Control + L.", label);
                if (CustomGUILayout.Button("Create a New Project", EditorStyles.miniButton))
                {
                    ProjectDesignerWindow.CreateWindow();
                }
                CustomGUILayout.Space(15);

                CustomGUILayout.Label("Controls", EditorStyles.largeLabel);
                CustomGUILayout.Label("Control + Z: Undo", label);
                CustomGUILayout.Label("Control + Y: Redo", label);
                CustomGUILayout.Label("Control + A: Select All", label);
                CustomGUILayout.Label("Control + C: Copy Selected Nodes", label);
                CustomGUILayout.Label("Control + V: Paste Selected Nodes", label);
                CustomGUILayout.Label("Control + D: Duplicate Selected Nodes", label);
                CustomGUILayout.Label("Control + H: Show Hidden Nodes", label);
                CustomGUILayout.Label("Control + F: Resets Editor View", label);
                CustomGUILayout.Label("H: Hides Selected Nodes", label);
                CustomGUILayout.Label("Delete: Deletes Selected Nodes", label);


                CustomGUILayout.Space(15);
                CustomGUILayout.Label("Team Members", EditorStyles.largeLabel);
                CustomGUILayout.Label("To add team members, use the settings button on top right corner of the editor which will take you under Preferences/Project Designer.", label);
                if (CustomGUILayout.Button("Go to Settings", EditorStyles.miniButton))
                {
                    ProjectDesignerSettings.OpenSettings();
                }
                CustomGUILayout.EndVertical();
                CustomGUILayout.Space(20);

            }
            CustomGUILayout.EndArea();
        }

        private List<Project> GetExampleAssets()
        {
            List<Project> assets = new List<Project>();
            string[] assetGUIDs = AssetDatabase.FindAssets($"t:{nameof(Project)}", new string[] { ProjectDesigner.Core.ProjectDesigner.ExampleAssetFolder });
            foreach (string guid in assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                if (asset != null && asset is Project p)
                {
                    assets.Add(p);
                }
            }

            return assets;
        }

        private void ShowExampleAssets(GUIStyle label, Texture2D icon)
        {
            Texture2D assetIcon = EditorGUIUtility.IconContent("ScriptableObject icon")?.image as Texture2D ?? icon;
            List<Project> projects = GetExampleAssets();
            if (projects.Count == 0)
            {
                return;
            }
            CustomGUILayout.Label("Here are some examples:", label);

            bool shouldEndHorizontal = false;
            for (int i = 0; i < projects.Count; i++)
            {
                if (i % 2 == 0)
                {
                    CustomGUILayout.BeginHorizontal();
                    shouldEndHorizontal = true;
                }

                if (CustomGUILayout.Button(new GUIContent(projects[i].name, assetIcon, "Start Asset")))
                {
                    if (projects[i] != null)
                    {
                        ProjectDesignerWindow.CreateWindow(projects[i]);
                    }
                }


                if (i % 2 == 1)
                {
                    CustomGUILayout.EndHorizontal();
                    shouldEndHorizontal = false;
                }
            }

            if (shouldEndHorizontal)
            {
                CustomGUILayout.EndHorizontal();
            }
        }
    }
}
