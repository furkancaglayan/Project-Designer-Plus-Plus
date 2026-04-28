using ProjectDesigner.V2.Data;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ProjectDesigner.V2.Editor
{
    [CustomEditor(typeof(ProjectBoardAsset))]
    internal sealed class ProjectBoardAssetInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            ProjectBoardAsset board = (ProjectBoardAsset)target;
            EditorGUILayout.LabelField("Project Designer+", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Board Name", board.Document.BoardName);
            EditorGUILayout.LabelField("Schema", board.SchemaVersion);
            EditorGUILayout.LabelField("Nodes", board.Document.Nodes.Count.ToString());
            EditorGUILayout.LabelField("Edges", board.Document.Edges.Count.ToString());
            EditorGUILayout.HelpBox(board.Document.Summary, MessageType.Info);

            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Open Planner"))
            {
                ProjectDesignerV2Window.Open(board);
            }
        }

        [OnOpenAsset]
        public static bool OpenBoard(int instanceId, int line)
        {
            ProjectBoardAsset board = EditorUtility.InstanceIDToObject(instanceId) as ProjectBoardAsset;
            if (board == null)
            {
                return false;
            }

            ProjectDesignerV2Window.Open(board);
            return true;
        }
    }
}
