using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ProjectDesigner.V2.Data;

namespace ProjectDesigner.V2.Editor
{
    public sealed class ProjectDesignerV2Window : EditorWindow
    {
        [SerializeField]
        private ProjectBoardAsset _boardAsset;

        public static void Open(ProjectBoardAsset board = null)
        {
            ProjectDesignerV2Window window = GetWindow<ProjectDesignerV2Window>();
            window.titleContent = new GUIContent(ProjectDesignerProductInfo.PlannerWindowTitle);
            if (board != null)
            {
                window._boardAsset = board;
            }

            window.minSize = new Vector2(1080f, 720f);
            window.Show();
            window.Rebuild();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent(ProjectDesignerProductInfo.PlannerWindowTitle);
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

            if (_boardAsset == null)
            {
                rootVisualElement.Add(new ProjectDesignerWelcomeView(
                    CreatePresetBoard,
                    TryOpenSelectedBoard,
                    ProjectDesignerV2Menus.OpenQuickStartGuide,
                    ProjectDesignerV2Menus.OpenProjectSettings,
                    ProjectDesignerV2Menus.OpenPlanningBoards));
            }
            else
            {
                rootVisualElement.Add(new ProjectDesignerWorkspaceView(_boardAsset, SetBoard));
            }
        }

        public void RefreshTheme()
        {
            Rebuild();
        }

        private void SetBoard(ProjectBoardAsset boardAsset)
        {
            _boardAsset = boardAsset;
            Rebuild();
        }

        private void CreatePresetBoard(string presetId, string boardName)
        {
            SetBoard(ProjectDesignerBoardUtility.CreateBoardAsset(presetId, boardName));
        }

        private void TryOpenSelectedBoard()
        {
            ProjectBoardAsset selectedBoard = Selection.activeObject as ProjectBoardAsset;
            if (selectedBoard == null)
            {
                EditorUtility.DisplayDialog(ProjectDesignerProductInfo.ProductName, "Select a Planning Board asset in the Project window first.", "OK");
                return;
            }

            SetBoard(selectedBoard);
        }
    }
}
