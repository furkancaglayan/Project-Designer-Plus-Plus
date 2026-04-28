using System.IO;
using ProjectDesigner.V2.Data;
using UnityEditor;

namespace ProjectDesigner.V2.Editor
{
    internal static class ProjectDesignerV2Menus
    {
        [MenuItem("Tools/Project Designer/Open Planner", priority = -10)]
        public static void OpenPlanner()
        {
            ProjectDesignerV2Window.Open();
        }

        [MenuItem("Tools/Project Designer/Project Finder", priority = -9)]
        [MenuItem("Window/Project Designer/Project Finder", priority = 2000)]
        [MenuItem("Tools/Project Designer/Open Planning Boards", priority = -9)]
        public static void OpenPlanningBoards()
        {
            ProjectDesignerBoardBrowserWindow.Open();
        }

        [MenuItem("Tools/Project Designer/Onboarding", priority = -8)]
        [MenuItem("Window/Project Designer/Onboarding", priority = 2001)]
        [MenuItem("Tools/Project Designer/Show Onboarding", priority = -8)]
        public static void ShowOnboarding()
        {
            ProjectDesignerOnboardingWindow.Open();
        }

        [MenuItem("Tools/Project Designer/New Board", priority = -7)]
        public static void CreateEmptyBoard()
        {
            CreateBoard(BoardPresetIds.Empty, ProjectDesignerProductInfo.DefaultBoardName);
        }

        [MenuItem("Tools/Project Designer/New Solo Indie Board", priority = -6)]
        public static void CreateSoloIndieBoard()
        {
            CreateBoard(BoardPresetIds.SoloIndie, ProjectDesignerProductInfo.SoloBoardName);
        }

        [MenuItem("Tools/Project Designer/New Small Team Board", priority = -5)]
        public static void CreateSmallTeamBoard()
        {
            CreateBoard(BoardPresetIds.SmallTeam, ProjectDesignerProductInfo.SmallTeamBoardName);
        }

        [MenuItem("Tools/Project Designer/New Technical Design Board", priority = -4)]
        public static void CreateTechnicalDesignBoard()
        {
            CreateBoard(BoardPresetIds.TechnicalDesign, ProjectDesignerProductInfo.TechnicalBoardName);
        }

        [MenuItem("Tools/Project Designer/New Pitch & Vision Board", priority = -3)]
        public static void CreatePitchVisionBoard()
        {
            CreateBoard(BoardPresetIds.PitchVision, ProjectDesignerProductInfo.PitchVisionBoardName);
        }

        [MenuItem("Tools/Project Designer/New Milestone Roadmap Board", priority = -2)]
        public static void CreateMilestoneRoadmapBoard()
        {
            CreateBoard(BoardPresetIds.MilestoneRoadmap, ProjectDesignerProductInfo.MilestoneRoadmapBoardName);
        }

        [MenuItem("Tools/Project Designer/New Research & Reference Board", priority = -1)]
        public static void CreateResearchReferenceBoard()
        {
            CreateBoard(BoardPresetIds.ResearchReference, ProjectDesignerProductInfo.ResearchReferenceBoardName);
        }

        [MenuItem("Tools/Project Designer/New Stakeholder Review Board", priority = 0)]
        public static void CreateStakeholderReviewBoard()
        {
            CreateBoard(BoardPresetIds.StakeholderReview, ProjectDesignerProductInfo.StakeholderReviewBoardName);
        }

        [MenuItem("Tools/Project Designer/Documentation/Quick Start", priority = 10)]
        public static void OpenQuickStartGuide()
        {
            string quickStartPath = Path.GetFullPath(ProjectDesignerPackageInfo.QuickStartPath);
            if (!File.Exists(quickStartPath))
            {
                EditorUtility.DisplayDialog("Project Designer+", "Quick start documentation could not be found in the embedded package.", "OK");
                return;
            }

            EditorUtility.OpenWithDefaultApp(quickStartPath);
        }

        [MenuItem("Tools/Project Designer/Project Settings", priority = 11)]
        public static void OpenProjectSettings()
        {
            SettingsService.OpenProjectSettings(ProjectDesignerPackageInfo.SettingsPath);
        }

        [MenuItem("Assets/Create/Project Designer/Empty Board", priority = 310)]
        public static void CreateEmptyBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.Empty, ProjectDesignerProductInfo.DefaultBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Solo Indie Board", priority = 311)]
        public static void CreateSoloIndieBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.SoloIndie, ProjectDesignerProductInfo.SoloBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Small Team Board", priority = 312)]
        public static void CreateSmallTeamBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.SmallTeam, ProjectDesignerProductInfo.SmallTeamBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Technical Design Board", priority = 313)]
        public static void CreateTechnicalDesignBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.TechnicalDesign, ProjectDesignerProductInfo.TechnicalBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Pitch & Vision Board", priority = 314)]
        public static void CreatePitchVisionBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.PitchVision, ProjectDesignerProductInfo.PitchVisionBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Milestone Roadmap Board", priority = 315)]
        public static void CreateMilestoneRoadmapBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.MilestoneRoadmap, ProjectDesignerProductInfo.MilestoneRoadmapBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Research & Reference Board", priority = 316)]
        public static void CreateResearchReferenceBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.ResearchReference, ProjectDesignerProductInfo.ResearchReferenceBoardName);
        }

        [MenuItem("Assets/Create/Project Designer/Stakeholder Review Board", priority = 317)]
        public static void CreateStakeholderReviewBoardFromAssetsMenu()
        {
            CreateBoard(BoardPresetIds.StakeholderReview, ProjectDesignerProductInfo.StakeholderReviewBoardName);
        }

        public static ProjectBoardAsset CreateBoard(string presetId, string boardName)
        {
            ProjectBoardAsset asset = ProjectDesignerBoardUtility.CreateBoardAsset(presetId, boardName);
            if (ProjectDesignerSettings.instance.AutoOpenWorkspaceAfterBoardCreation)
            {
                ProjectDesignerV2Window.Open(asset);
            }

            return asset;
        }
    }
}
