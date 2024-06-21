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
    /// An <see cref="AssigneeMember"/> is used for adding/removing assignees to nodes.
    /// </summary>
    public class AssigneeMember : MemberBase
    { 
        private const int ImageSize = 32;
        private const string NoAssigneeWarning = "There is no team members to assign.";

        public AssigneeMember(AssigneeMember other) : base()
        {
        }

        public AssigneeMember(NodeBase parent) : base()
        {
        }

        public override void OnAdded(NodeBase nodeBase)
        {
        }

        public override void Draw(IEditorContext context, NodeBase parent, float width)
        {
            ReadOnlyCollection<TeamMember> possibleMembers = context.GetTeamMembers();
            if (possibleMembers.Count == 0)
            {
                CustomGUILayout.Label(NoAssigneeWarning, LeftAlignedLabelStyle);
            }
            else
            {
                CustomGUILayout.BeginVertical();

                for (int i = parent.GetAssigneeCount() - 1; i >= 0 ; i--)
                {
                    TeamMember member = parent.GetAssignees()[i];
                    if (!context.GetTeamMembers().Contains(member))
                    {
                        parent.RemoveAssignee(member);
                        continue;
                    }

                    CustomGUILayout.BeginHorizontal();
                    CustomGUILayout.Image(member.GetMemberImage(), ImageStyle, w: ImageSize, h: ImageSize);
                    CustomGUILayout.Space(5);
                    CustomGUILayout.BeginVertical();
                    CustomGUILayout.Space(5);
                    CustomGUILayout.Label(member.GetDisplayName(), LeftAlignedLabelStyle);
                    CustomGUILayout.EndVertical();

                    if (parent.IsExpanded)
                    {
                        CustomGUILayout.BeginVertical();
                        CustomGUILayout.Space(5);
                        if (CustomGUILayout.Button("X", ButtonStyle))
                        {
                            parent.RemoveAssignee(member);
                        }
                        CustomGUILayout.EndVertical();
                    }
                 
                    CustomGUILayout.EndHorizontal();
                }

                if (parent.GetAssigneeCount() == 0)
                {
                    CustomGUILayout.Label("There are no assignees", LabelStyle);
                }
                if (parent.IsExpanded)
                {
                    if (CustomGUILayout.Button("New Assignee", ButtonStyle))
                    {
                        context.ShowDropdownOutsideZoomArea(() => ShowMemberSelection(context, parent));
                    }
                }

                CustomGUILayout.EndVertical();
            }
        }

        public override MemberBase Copy(NodeBase parent)
        {
           return new AssigneeMember(this);
        }

        private GenericMenu ShowMemberSelection(IEditorContext context, NodeBase parent)
        {
            GenericMenu menu = new GenericMenu();
            foreach (var member in context.GetTeamMembers())
            {
                menu.AddMenuActionOnCondition($"{member.GetDisplayName()}", () => AddNewAssignee(parent, member), !parent.HasAssignee(member));
            }
            return menu;
        }

        private void AddNewAssignee(NodeBase parent, TeamMember member)
        {
            Debug.Assert(!parent.HasAssignee(member));
            parent.AddAssignee(member);
        }
    }
}
