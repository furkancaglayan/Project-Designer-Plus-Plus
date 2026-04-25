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

        private bool _styleLoaded;

        public static void Open(ProjectBoardAsset board = null)
        {
            ProjectDesignerV2Window window = GetWindow<ProjectDesignerV2Window>();
            window.titleContent = new GUIContent("Project Designer+");
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
            titleContent = new GUIContent("Project Designer+");
            Rebuild();
        }

        private void Rebuild()
        {
            if (rootVisualElement == null)
            {
                return;
            }

            EnsureStyles();
            rootVisualElement.Clear();

            if (_boardAsset == null)
            {
                rootVisualElement.Add(new ProjectDesignerWelcomeView(CreatePresetBoard, TryOpenSelectedBoard));
            }
            else
            {
                rootVisualElement.Add(new ProjectDesignerWorkspaceView(_boardAsset, SetBoard));
            }
        }

        private void EnsureStyles()
        {
            if (_styleLoaded)
            {
                return;
            }

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ProjectDesignerPackageInfo.StyleSheetPath);
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
                _styleLoaded = true;
            }
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
                EditorUtility.DisplayDialog("Project Designer+", "Select a Project Board asset in the Project window first.", "OK");
                return;
            }

            SetBoard(selectedBoard);
        }
    }
}
