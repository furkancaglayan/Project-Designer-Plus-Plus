using System.Linq;
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
        private ProjectDesignerWorkspaceView _workspaceView;

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
            _workspaceView = null;

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
                _workspaceView = new ProjectDesignerWorkspaceView(_boardAsset, SetBoard);
                rootVisualElement.Add(_workspaceView);
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

        internal ProjectDesignerWorkspaceView GetWorkspaceView()
        {
            return _workspaceView;
        }

        internal static ProjectDesignerWorkspaceView TryGetOpenWorkspace()
        {
            ProjectDesignerV2Window[] windows = Resources.FindObjectsOfTypeAll<ProjectDesignerV2Window>();
            ProjectDesignerV2Window window = windows.FirstOrDefault(candidate => candidate != null && candidate.hasFocus && candidate._workspaceView != null);
            if (window != null)
            {
                return window._workspaceView;
            }

            return windows
                .Where(candidate => candidate != null)
                .Select(candidate => candidate._workspaceView)
                .FirstOrDefault(workspace => workspace != null);
        }
    }
}
