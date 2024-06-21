using ProjectDesigner.Core;
using ProjectDesigner.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Data.Members
{
    [Serializable]
    public class TaskStatusMember : MemberBase
    {
        public TaskStatusMember(TaskStatusMember other) : base(15)
        {
        }

        public TaskStatusMember() : base(15)
        {
        }

        public override void OnAdded(NodeBase nodeBase)
        {
        }

        public override void Draw(IEditorContext context, NodeBase parent, float width)
        {
            if (!parent.IsExpanded)
            {
                CustomGUILayout.BeginHorizontal();
            }
            CustomGUILayout.Label($"Task Status: ", LabelStyle);

            if (parent.IsExpanded)
            {
                if (CustomGUILayout.Button(parent.GetStatus().ToString(), ButtonStyle))
                {
                    context.ShowDropdownOutsideZoomArea(() => GetStatusMenu(parent));
                }
            }
            else
            {
                CustomGUILayout.ColoredLabel(parent.GetStatus().ToString(), GetStatusColor(parent.GetStatus()), LabelStyle);
            }

            if (!parent.IsExpanded)
            {
                CustomGUILayout.EndHorizontal();
            }
        }

        public override MemberBase Copy(NodeBase parent)
        {
           return new TaskStatusMember(this);
        }

        private GenericMenu GetStatusMenu(NodeBase parent)
        {
            GenericMenu genericMenu = new GenericMenu();
            for (int i = 0; i < (int)NodeBase.NodeStatus.NumStatus; i++)
            {
                NodeBase.NodeStatus nodeStatus = (NodeBase.NodeStatus)i;
                genericMenu.AddItem(new GUIContent(nodeStatus.ToString()), false, () => parent.SetStatus(nodeStatus));
            }

            return genericMenu;
        }

        private Color GetStatusColor(NodeBase.NodeStatus status)
        {
            switch (status)
            {
                case NodeBase.NodeStatus.NotStarted:
                    return Color.red;
                case NodeBase.NodeStatus.InProgress:
                    return new Color(224f / 255f, 177f / 255f, 58f / 255f);
                case NodeBase.NodeStatus.Completed:
                    return new Color(174f / 255f, 224f / 255f, 58f / 255f);
                default : return Color.white;
            }
        }
    }
}
