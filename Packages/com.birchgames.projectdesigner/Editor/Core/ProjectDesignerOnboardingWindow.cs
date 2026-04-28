using System.IO;
using ProjectDesigner.V2.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class ProjectDesignerOnboardingWindow : EditorWindow
    {
        public static void Open()
        {
            ProjectDesignerOnboardingWindow window = GetWindow<ProjectDesignerOnboardingWindow>();
            window.titleContent = new GUIContent(ProjectDesignerProductInfo.ProductName + " Onboarding");
            window.minSize = new Vector2(760f, 700f);
            window.Show();
            window.Rebuild();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent(ProjectDesignerProductInfo.ProductName + " Onboarding");
            Rebuild();
        }

        private void Rebuild()
        {
            if (rootVisualElement == null)
            {
                return;
            }

            ProjectDesignerThemeConfig.ApplyTheme(rootVisualElement);
            rootVisualElement.Clear();

            var scrollView = new ScrollView();
            scrollView.AddToClassList("pd-onboarding-window");
            rootVisualElement.Add(scrollView);

            var container = new VisualElement();
            container.AddToClassList("pd-onboarding-content");
            scrollView.Add(container);

            var welcomeView = new ProjectDesignerWelcomeView(
                CreatePresetBoard,
                OpenSelectedBoard,
                ProjectDesignerV2Menus.OpenQuickStartGuide,
                ProjectDesignerV2Menus.OpenProjectSettings,
                ProjectDesignerV2Menus.OpenPlanningBoards);
            container.Add(welcomeView);

            var footer = new VisualElement();
            footer.AddToClassList("pd-onboarding-footer");
            container.Add(footer);

            string version = ProjectDesignerPackageInfo.GetInstalledVersion();
            var versionLabel = new Label("Installed package version: " + version);
            versionLabel.AddToClassList("pd-onboarding-meta");
            footer.Add(versionLabel);

            string absoluteQuickStartPath = Path.GetFullPath(ProjectDesignerPackageInfo.QuickStartPath);
            var storageLabel = new Label("Quick start guide: " + absoluteQuickStartPath);
            storageLabel.AddToClassList("pd-onboarding-meta");
            footer.Add(storageLabel);

            var actions = new VisualElement();
            actions.AddToClassList("pd-action-row");
            footer.Add(actions);

            var continueButton = new Button(ContinueToWorkspace) { text = "Continue to Planner" };
            continueButton.AddToClassList("pd-primary-button");
            actions.Add(continueButton);

            var disableAutoShowButton = new Button(DisableAutomaticOnboarding) { text = "Don't Show Automatically" };
            disableAutoShowButton.AddToClassList("pd-secondary-button");
            actions.Add(disableAutoShowButton);
        }

        public void RefreshTheme()
        {
            Rebuild();
        }

        private void CreatePresetBoard(string presetId, string boardName)
        {
            ProjectBoardAsset board = ProjectDesignerV2Menus.CreateBoard(presetId, boardName);
            if (board != null && !ProjectDesignerSettings.instance.AutoOpenWorkspaceAfterBoardCreation)
            {
                Selection.activeObject = board;
            }

            Close();
        }

        private void OpenSelectedBoard()
        {
            ProjectBoardAsset selectedBoard = Selection.activeObject as ProjectBoardAsset;
            ProjectDesignerV2Window.Open(selectedBoard);
            Close();
        }

        private void ContinueToWorkspace()
        {
            ProjectBoardAsset selectedBoard = Selection.activeObject as ProjectBoardAsset;
            ProjectDesignerV2Window.Open(selectedBoard);
            Close();
        }

        private void DisableAutomaticOnboarding()
        {
            ProjectDesignerSettings.instance.SetShowOnboardingOnStartup(false);
            Close();
        }
    }
}
