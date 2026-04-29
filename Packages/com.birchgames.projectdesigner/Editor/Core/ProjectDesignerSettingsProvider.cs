using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ProjectDesigner.V2.Data;
using System.Linq;

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
                "Planning",
                "Roster",
                "Assignee",
                "Team"
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
            EditorGUILayout.PropertyField(
                _settingsObject.FindProperty("_showOnboardingOnStartup"),
                new GUIContent("Show On Startup", "Open the onboarding window automatically when Project Designer+ is first installed in this Unity project."));
            EditorGUILayout.PropertyField(
                _settingsObject.FindProperty("_showOnboardingForPackageUpdates"),
                new GUIContent("Reopen On Package Update", "Open onboarding again when the installed package version changes."));
            EditorGUILayout.HelpBox("The onboarding window opens once for a new install, and can optionally re-open when the package version changes.", MessageType.Info);

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Planning Board Creation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                _settingsObject.FindProperty("_autoOpenWorkspaceAfterBoardCreation"),
                new GUIContent("Auto Open Planner", "Open the planner immediately after creating a new board from onboarding or the Assets/Create menu."));
            EditorGUILayout.PropertyField(
                _settingsObject.FindProperty("_defaultBoardFolder"),
                new GUIContent("Default Board Folder", "Assets-relative fallback folder used when a new board cannot be created from the current selection."));
            EditorGUILayout.HelpBox("Use an Assets-relative folder such as 'Assets/Project Designer'. When your current selection is outside Assets, new planning boards will be created here.", MessageType.None);

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Team Roster", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                _settingsObject.FindProperty("_defaultTeamRoster"),
                new GUIContent("Default Team Roster", "Project-wide roster used by task assignee dropdowns, workload summaries, and assignee filters."));
            EditorGUILayout.HelpBox("Define assignees once for the whole project, then pick them from task dropdowns instead of typing names per card.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Create Roster", "Create a new team roster asset and assign it as the default roster for this Unity project.")))
            {
                ProjectDesignerTeamRosterEditorUtility.CreateAndAssignDefaultRoster();
                _settingsObject = new SerializedObject(ProjectDesignerSettings.instance);
            }

            using (new EditorGUI.DisabledScope(ProjectDesignerSettings.instance.DefaultTeamRoster == null))
            {
                if (GUILayout.Button(new GUIContent("Select Roster", "Select the assigned roster asset in the Project window.")))
                {
                    ProjectDesignerTeamRosterEditorUtility.SelectRoster(ProjectDesignerSettings.instance.DefaultTeamRoster);
                }

                if (GUILayout.Button(new GUIContent("Open Roster", "Focus the Project window on the assigned roster asset so you can inspect or edit it.")))
                {
                    ProjectDesignerTeamRosterEditorUtility.SelectRoster(ProjectDesignerSettings.instance.DefaultTeamRoster);
                    EditorUtility.FocusProjectWindow();
                }
            }
            EditorGUILayout.EndHorizontal();
            DrawRosterPreview(ProjectDesignerSettings.instance.DefaultTeamRoster);

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Project Finder", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                _settingsObject.FindProperty("_projectFinderSortMode"),
                new GUIContent("Preferred Sort", "Default sort mode used when Project Finder opens."));
            EditorGUILayout.HelpBox("The Project Finder remembers pinned boards, recently opened boards, and this default sort preference for the current Unity project.", MessageType.None);

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(!ProjectDesignerThemeConfig.SupportsDarkTheme))
            {
                EditorGUILayout.PropertyField(
                    _settingsObject.FindProperty("_editorTheme"),
                    new GUIContent("Editor Theme", "Choose the planner theme used by onboarding, Project Finder, and the planner window."));
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
            if (GUILayout.Button(new GUIContent("Open Onboarding", "Open the onboarding window to learn the planner and create a board from a template.")))
            {
                ProjectDesignerOnboardingWindow.Open();
            }

            if (GUILayout.Button(new GUIContent("Open Quick Start", "Open the user-facing quick start guide from the package documentation.")))
            {
                ProjectDesignerV2Menus.OpenQuickStartGuide();
            }

            if (GUILayout.Button(new GUIContent("Open Project Finder", "Open the Project Finder window to search, pin, and reopen current planning boards.")))
            {
                ProjectDesignerBoardBrowserWindow.Open();
            }
        }

        private static void DrawRosterPreview(ProjectDesignerTeamRosterAsset roster)
        {
            if (roster == null)
            {
                EditorGUILayout.HelpBox("No default team roster is assigned yet. Create one here to enable assignee dropdowns and stronger workload summaries.", MessageType.None);
                return;
            }

            roster.EnsureDefaults();
            IReadOnlyList<ProjectDesignerTeamMemberData> members = roster.Members
                .Where(member => member != null)
                .OrderBy(member => member.DisplayName)
                .ToList();

            if (members.Count == 0)
            {
                EditorGUILayout.HelpBox("The roster asset exists but has no members yet. Select it and add a few teammates in the Inspector.", MessageType.None);
                return;
            }

            EditorGUILayout.LabelField("Current Members", EditorStyles.miniBoldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                foreach (ProjectDesignerTeamMemberData member in members.Take(8))
                {
                    string detail = member.DisplayName;
                    if (!string.IsNullOrWhiteSpace(member.Role))
                    {
                        detail += " | " + member.Role;
                    }
                    else if (!string.IsNullOrWhiteSpace(member.Discipline))
                    {
                        detail += " | " + member.Discipline;
                    }

                    if (!member.IsActive)
                    {
                        detail += " | inactive";
                    }

                    EditorGUILayout.TextField(detail);
                }
            }

            if (members.Count > 8)
            {
                EditorGUILayout.LabelField("+" + (members.Count - 8) + " more members", EditorStyles.miniLabel);
            }
        }
    }
}
