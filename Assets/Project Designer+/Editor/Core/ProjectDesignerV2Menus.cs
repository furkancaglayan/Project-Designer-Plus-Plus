using System.IO;
using ProjectDesigner.V2.Data;
using UnityEditor;

namespace ProjectDesigner.V2.Editor
{
    internal static class ProjectDesignerV2Menus
    {
        [MenuItem("Tools/Project Designer/Open Planner", priority = -10)]
        [MenuItem("Window/Project Designer/Open Planner", priority = 2000)]
        public static void OpenPlanner()
        {
            ProjectDesignerV2Window.Open();
        }

        [MenuItem("Tools/Project Designer/Project Finder", priority = -9)]
        [MenuItem("Window/Project Designer/Project Finder", priority = 2001)]
        public static void OpenPlanningBoards()
        {
            ProjectDesignerBoardBrowserWindow.Open();
        }

        [MenuItem("Tools/Project Designer/Onboarding", priority = -8)]
        [MenuItem("Window/Project Designer/Onboarding", priority = 2002)]
        public static void ShowOnboarding()
        {
            ProjectDesignerOnboardingWindow.Open();
        }

        [MenuItem("Tools/Project Designer/Documentation/Quick Start", priority = 10)]
        public static void OpenQuickStartGuide()
        {
            string quickStartPath = Path.GetFullPath(ProjectDesignerPackageInfo.QuickStartPath);
            if (!File.Exists(quickStartPath))
            {
                EditorUtility.DisplayDialog("Project Designer+", "Quick start documentation could not be found under Assets/Project Designer+.", "OK");
                return;
            }

            EditorUtility.OpenWithDefaultApp(quickStartPath);
        }

        [MenuItem("Tools/Project Designer/Project Settings", priority = 11)]
        public static void OpenProjectSettings()
        {
            SettingsService.OpenProjectSettings(ProjectDesignerPackageInfo.SettingsPath);
        }

        public static void OpenTeamRoster()
        {
            ProjectDesignerTeamRosterAsset roster = ProjectDesignerSettings.instance.DefaultTeamRoster;
            if (roster == null)
            {
                SettingsService.OpenProjectSettings(ProjectDesignerPackageInfo.SettingsPath);
                EditorUtility.DisplayDialog(ProjectDesignerProductInfo.ProductName, "Create or assign a default team roster in Project Settings first.", "OK");
                return;
            }

            ProjectDesignerTeamRosterEditorUtility.SelectRoster(roster);
            EditorUtility.FocusProjectWindow();
        }

        [MenuItem("Assets/Create/Project Designer/Empty Board", priority = 310)]
        public static void CreateEmptyBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.Empty, ProjectDesignerProductInfo.DefaultBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Project Designer+ Redo Board", priority = 311)]
        public static void CreateProjectDesignerRedoBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.ProjectDesignerRedo, ProjectDesignerProductInfo.ProjectDesignerRedoBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Solo Indie Board", priority = 312)]
        public static void CreateSoloIndieBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.SoloIndie, ProjectDesignerProductInfo.SoloBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Small Team Board", priority = 313)]
        public static void CreateSmallTeamBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.SmallTeam, ProjectDesignerProductInfo.SmallTeamBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Technical Design Board", priority = 314)]
        public static void CreateTechnicalDesignBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.TechnicalDesign, ProjectDesignerProductInfo.TechnicalBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Pitch & Vision Board", priority = 315)]
        public static void CreatePitchVisionBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.PitchVision, ProjectDesignerProductInfo.PitchVisionBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Milestone Roadmap Board", priority = 316)]
        public static void CreateMilestoneRoadmapBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.MilestoneRoadmap, ProjectDesignerProductInfo.MilestoneRoadmapBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Research & Reference Board", priority = 317)]
        public static void CreateResearchReferenceBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.ResearchReference, ProjectDesignerProductInfo.ResearchReferenceBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Stakeholder Review Board", priority = 318)]
        public static void CreateStakeholderReviewBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.StakeholderReview, ProjectDesignerProductInfo.StakeholderReviewBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Team Roster", priority = 319)]
        public static void CreateTeamRosterFromAssetsMenu()
        {
            ProjectDesignerTeamRosterEditorUtility.CreateAndAssignDefaultRoster();
        }

        public static ProjectBoardAsset CreateBoard(string presetId, string boardName)
        {
            ProjectBoardAsset asset = ProjectDesignerBoardUtility.CreateBoardAsset(presetId, boardName);
            string assetPath = AssetDatabase.GetAssetPath(asset);
            string boardGuid = AssetDatabase.AssetPathToGUID(assetPath);
            ProjectDesignerSettings.instance.MarkBoardOpened(boardGuid);
            ProjectDesignerBoardBrowserWindow.RefreshOpenBrowsers();
            if (ProjectDesignerSettings.instance.AutoOpenWorkspaceAfterBoardCreation)
            {
                ProjectDesignerV2Window.Open(asset);
            }

            return asset;
        }
    }
}
