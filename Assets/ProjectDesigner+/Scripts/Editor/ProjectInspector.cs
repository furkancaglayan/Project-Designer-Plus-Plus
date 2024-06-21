using ProjectDesigner.Core;
using ProjectDesigner.Data;
using ProjectDesigner.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ProjectDesigner.Editor
{
    [CustomEditor(typeof(Project))]
    public class ProjectInspector : UnityEditor.Editor
    {
        [SerializeField]
        private Vector2 _scroll;

        public override void OnInspectorGUI()
        {
            Project project = (Project)target;
            CustomGUILayout.Label(project.name, GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.Header));

            CustomGUILayout.Space(10);
            CustomGUILayout.Label("Double click on asset to open.", GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.Label));

            CustomGUILayout.Label($"Drawable Count: {project.Drawables.Count}/{Project.DrawableLimit}");

            using (GUILayout.ScrollViewScope scope = CustomGUILayout.CreateScrollView(_scroll))
            {
                CustomGUILayout.BeginVertical(GUI.skin.box);
                _scroll = scope.scrollPosition;
                foreach (var log in project.GetLogs())
                {
                    LogLayout(log);
                }
                CustomGUILayout.EndVertical();
            }

            if (project.GetLogs().Any() && CustomGUILayout.Button("Clear History"))
            {
                project.ClearLogHistory();
            }
        }


        [OnOpenAsset]
        public static bool OpenDesign(int instanceID, int line)
        {
            Project project = EditorUtility.InstanceIDToObject(instanceID) as Project;
            if (project != null)
            {
                ProjectDesignerWindow[] windows = GetEditorWindows<ProjectDesignerWindow>();
                if (windows != null && windows.Length > 0)
                {
                    foreach (var window in windows)
                    {
                        if (window.Project == project)
                        {
                            window.ShowTab();
                            return true;
                        }
                    }
                }

                ProjectDesignerWindow.CreateWindow(project);
                return true;
            }
            return false;
        }

        private void LogLayout(EditorLogHistory.Log log)
        {
            CustomGUILayout.HelpBox($"{log.Text}\n{log.Description}", log.GetMessageType());
        }

        private static T[] GetEditorWindows<T>(Func<T, bool> func = null) where T : EditorWindow
        {
            UnityEngine.Object[] windows = Resources.FindObjectsOfTypeAll(typeof(T));
            if (func == null)
            {
                return windows as T[];
            }

            return windows.Where(x => func((T)x)) as T[];
        }
    }
}
