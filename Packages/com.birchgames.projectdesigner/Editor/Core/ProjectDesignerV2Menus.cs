using ProjectDesigner.V2.Data;
using UnityEditor;

namespace ProjectDesigner.V2.Editor
{
    internal static class ProjectDesignerV2Menus
    {
        [MenuItem("Tools/Project Designer/Open Workspace", priority = -10)]
        public static void OpenWorkspace()
        {
            ProjectDesignerV2Window.Open();
        }

        [MenuItem("Tools/Project Designer/New Board", priority = -9)]
        public static void CreateEmptyBoard()
        {
            OpenPreset(BoardPresetIds.Empty, "Project Board");
        }

        [MenuItem("Tools/Project Designer/New Solo Indie Board", priority = -8)]
        public static void CreateSoloIndieBoard()
        {
            OpenPreset(BoardPresetIds.SoloIndie, "Solo Indie Board");
        }

        [MenuItem("Tools/Project Designer/New Small Team Board", priority = -7)]
        public static void CreateSmallTeamBoard()
        {
            OpenPreset(BoardPresetIds.SmallTeam, "Small Team Board");
        }

        [MenuItem("Tools/Project Designer/New Technical Design Board", priority = -6)]
        public static void CreateTechnicalDesignBoard()
        {
            OpenPreset(BoardPresetIds.TechnicalDesign, "Technical Design Board");
        }

        private static void OpenPreset(string presetId, string boardName)
        {
            ProjectBoardAsset asset = ProjectDesignerBoardUtility.CreateBoardAsset(presetId, boardName);
            ProjectDesignerV2Window.Open(asset);
        }
    }
}
