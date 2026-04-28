using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProjectDesigner.V2.Data;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class ProjectDesignerSettingsProvider : SettingsProvider
    {
        private SerializedObject _settingsObject;

        private ProjectDesignerSettingsProvider(string path, SettingsScope scope)
            : base(path, scope)
        {
            keywords = new HashSet<string>(new[]
            {
                "Project Designer",
                "Onboarding",
                "Board",
                "Planner",
                "Settings",
                "Planning"
            });
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new ProjectDesignerSettingsProvider(ProjectDesignerPackageInfo.SettingsPath, SettingsScope.Project);
        }

        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            _settingsObject = new SerializedObject(ProjectDesignerSettings.instance);
        }

        public override void OnGUI(string searchContext)
        {
            if (_settingsObject == null)
            {
                _settingsObject = new SerializedObject(ProjectDesignerSettings.instance);
            }

            _settingsObject.Update();

            EditorGUILayout.LabelField("Onboarding", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_settingsObject.FindProperty("_showOnboardingOnStartup"), new GUIContent("Show On Startup"));
            EditorGUILayout.PropertyField(_settingsObject.FindProperty("_showOnboardingForPackageUpdates"), new GUIContent("Reopen On Package Update"));
            EditorGUILayout.HelpBox("The onboarding window opens once for a new install, and can optionally re-open when the package version changes.", MessageType.Info);

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Planning Board Creation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_settingsObject.FindProperty("_autoOpenWorkspaceAfterBoardCreation"), new GUIContent("Auto Open Planner"));
            EditorGUILayout.PropertyField(_settingsObject.FindProperty("_defaultBoardFolder"), new GUIContent("Default Board Folder"));
            EditorGUILayout.HelpBox("Use an Assets-relative folder such as 'Assets/Project Designer'. When your current selection is outside Assets, new planning boards will be created here.", MessageType.None);

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(!ProjectDesignerThemeConfig.SupportsDarkTheme))
            {
                EditorGUILayout.PropertyField(_settingsObject.FindProperty("_editorTheme"), new GUIContent("Editor Theme"));
            }
            EditorGUILayout.HelpBox(
                ProjectDesignerThemeConfig.SupportsDarkTheme
                    ? "Light is the default. Dark is available as an optional editor theme."
                    : "The package is currently locked to light mode.",
                MessageType.None);

            bool changed = _settingsObject.ApplyModifiedProperties();
            if (changed)
            {
                ProjectDesignerSettings.instance.SaveSettings();
                ProjectDesignerThemeConfig.RefreshOpenWindows();
            }

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Package", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Installed Version", ProjectDesignerPackageInfo.GetInstalledVersion());
                EditorGUILayout.TextField("Last Onboarding Version", ProjectDesignerSettings.instance.LastSeenOnboardingVersion);
            }

            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Open Onboarding"))
            {
                ProjectDesignerOnboardingWindow.Open();
            }

            if (GUILayout.Button("Open Quick Start"))
            {
                ProjectDesignerV2Menus.OpenQuickStartGuide();
            }

            if (GUILayout.Button("Open Planning Boards"))
            {
                ProjectDesignerBoardBrowserWindow.Open();
            }

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Current Planning Boards", EditorStyles.boldLabel);
            DrawCurrentBoards();
        }

        private static void DrawCurrentBoards()
        {
            IReadOnlyList<ProjectBoardAsset> boards = ProjectDesignerBoardCatalog.GetBoards();
            if (boards.Count == 0)
            {
                EditorGUILayout.HelpBox("No planning boards were found in this project yet.", MessageType.None);
                return;
            }

            foreach (ProjectBoardAsset board in boards)
            {
                string path = AssetDatabase.GetAssetPath(board);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField("Board Asset", board, typeof(ProjectBoardAsset), false);
                }
                EditorGUILayout.LabelField("Board Name", board.Document.BoardName);
                EditorGUILayout.LabelField(path, EditorStyles.miniLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select"))
                {
                    Selection.activeObject = board;
                    EditorGUIUtility.PingObject(board);
                }

                if (GUILayout.Button("Open"))
                {
                    ProjectDesignerV2Window.Open(board);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }
    }
}
