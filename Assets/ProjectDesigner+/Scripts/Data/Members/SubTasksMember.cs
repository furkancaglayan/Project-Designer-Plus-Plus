using ProjectDesigner.Core;
using ProjectDesigner.Data.Connections;
using ProjectDesigner.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Data.Members
{
    public class SubTasksMember : MemberBase
    {
        private const int ButtonWidth = 130;
        private const int Space = 10;
     
        public SubTasksMember(SubTasksMember other) : base()
        {
        }

        public SubTasksMember() : base()
        {
        }


        public override void OnAdded(NodeBase nodeBase)
        {
        }


        public override void Draw(IEditorContext context, NodeBase parent, float width)
        {
            List<ConnectionBase> connections = parent.GetConnections();
            if (connections.Count == 0)
            {
                CustomGUILayout.Label("There are no subtasks", LabelStyle);
            }
            else
            {
                CustomGUILayout.Label("Subtasks", HeaderStyle);

                for (int i = connections.Count - 1; i >= 0; i--) 
                {
                    ConnectionBase subTaskConnection = connections[i];

                    if (subTaskConnection is SubTaskConnection)
                    {
                        CustomGUILayout.BeginHorizontal();
                        CustomGUILayout.Label($"• {subTaskConnection.To}", LeftAlignedLabelStyle, characterLimit: parent.IsExpanded ? 30 : 40);
                        if (parent.IsExpanded)
                        {
                            if (CustomGUILayout.Button("Remove Subtask", ButtonStyle, w: ButtonWidth))
                            {
                                context.ProcessAction(new DeleteConnectionAction(subTaskConnection));
                            }
                        }
                        CustomGUILayout.EndHorizontal();
                    }
                }
            }

            CustomGUILayout.Space(Space);
            if (parent.IsExpanded)
            {
                if (CustomGUILayout.Button("Add New Subtask", EditorStyles.toolbarButton))
                {
                    context.DragAndDropHandler.StartConnection(context, parent, typeof(SubTaskConnection));
                }
            }
        }

        public override MemberBase Copy(NodeBase parent)
        {
            return new SubTasksMember(this);
        }
    }
}
