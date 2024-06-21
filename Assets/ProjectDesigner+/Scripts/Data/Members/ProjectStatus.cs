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
    /// <summary>
    /// <see cref="ProjectStatus"/> shows in-progress tasks of a <see cref="Project"/> object.
    /// </summary>
    public class ProjectStatus : MemberBase
    {
        public ProjectStatus(ProjectStatus other) : base()
        {
        }

        public ProjectStatus() : base()
        {
        }

        public override void OnAdded(NodeBase nodeBase)
        {
        }

        public override void Draw(IEditorContext context, NodeBase parent, float width)
        {
            ShowTasksInProgress(context, 120);
        }

        private void ShowTasksInProgress(IEditorContext context, float height)
        {
            CustomGUILayout.Label($"Tasks In Progress", LabelStyle);

            List < NodeBase > nodes = context.GetDrawables<NodeBase>(x => x.GetStatus() == NodeBase.NodeStatus.InProgress);

            if (nodes.Count == 0)
            {
                CustomGUILayout.ColoredLabel($"There are no tasks in progress", Color.red, LabelStyle);
            }
            int[] layouts = new int[] { 2, 1, 3};
            int currentLayoutIndex = 0;
            bool beginHorizontal = false;
            int layoutCount = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                int currentLayout = layouts[currentLayoutIndex];
                if (!beginHorizontal)
                {
                    CustomGUILayout.BeginHorizontal();
                    beginHorizontal = true;
                    layoutCount = 0;
                }
                DrawTask(context, height, nodes[i]);
                bool endHorizontal = layoutCount == currentLayout - 1;
                if (endHorizontal)
                {
                    currentLayoutIndex = (currentLayoutIndex + 1) % layouts.Length;
                    CustomGUILayout.EndHorizontal();
                    beginHorizontal = false;
                }
                else
                {
                    layoutCount++;
                }
            }

            if (beginHorizontal)
            {
                CustomGUILayout.EndHorizontal();
            }
        }

        private void DrawTask(IEditorContext context, float height, NodeBase nodeBase)
        {
            CustomGUILayout.BeginVertical(GUI.skin.box, h: height);
            DateTimeMember dateTimeMember = nodeBase.GetFirstMember<DateTimeMember>();

            CustomGUILayout.BeginHorizontal();
            if (dateTimeMember != null)
            {
                CustomGUILayout.Image(dateTimeMember.GetDueTimeIcon(), w: 16);
            }

        
            if (CustomGUILayout.Button(nodeBase.HeaderText, LeftAlignedLabelStyle))
            {
                context.Overview(nodeBase);
            }

            DrawAssignees(nodeBase);
            /*Texture2D assigneeIcon = GetAssigneeIcon(nodeBase);
            if (assigneeIcon != null)
            {
                CustomGUILayout.Image(assigneeIcon, w: 32);
            }*/
            CustomGUILayout.EndHorizontal();

            if (dateTimeMember != null)
            {
                CustomGUILayout.Label($"Due time: {dateTimeMember.GetDateDisplay()}", EditorStyles.miniBoldLabel);
            }

            CommentMember commentMember = nodeBase.GetFirstMember<CommentMember>();
            if (commentMember != null)
            {
                GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel);
                style.wordWrap = true;
                style.richText = true;
                CustomGUILayout.Label($"{commentMember.GetText().Replace(Environment.NewLine, string.Empty)}", style, characterLimit: 100);
            }

            CustomGUILayout.EndVertical();
        }

        private void DrawAssignees(NodeBase nodeBase)
        {
            foreach (var assignee in nodeBase.GetAssignees())
            {
                Texture2D assigneeIcon = assignee.Image;
                if (assigneeIcon != null)
                {
                    CustomGUILayout.Image(assigneeIcon, w: 32);
                }
            }
        }

        public override MemberBase Copy(NodeBase parent)
        {
            return new ProjectStatus(this);
        }
    }
}
