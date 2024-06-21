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
    public class Team : MemberBase
    {
        private const int BlockHeight = 64;
        private const int Padding = 20;
        public List<TeamMember> Members => ProjectDesignerSettings.Instance.TeamMembers;

        public Team(Team other) : this()
        {
        }

        public Team() : base()
        {
        }


        public override void OnAdded(NodeBase nodeBase)
        {
        }


        public override void Draw(IEditorContext context, NodeBase parent, float width)
        {
            ReadOnlyCollection<TeamMember> members = context.GetTeamMembers();
            if (members.Any())
            {
                CustomGUILayout.Label("Team TeamMembers", HeaderStyle);
                CustomGUILayout.Space(Padding);

                CustomGUILayout.BeginVertical(GUI.skin.box);

                for (int i = 0; i < members.Count; i++)
                {
                    TeamMember member = members[i];
                    CustomGUILayout.BeginHorizontal();
                    CustomGUILayout.Image(member.GetMemberImage(), ImageStyle, w: BlockHeight, h: BlockHeight);
                    CustomGUILayout.BeginVertical(h: BlockHeight);
                    CustomGUILayout.Space(10);
                    CustomGUILayout.EditableText(ref member.FullName, member.FullName, HeaderStyle, parent.IsExpanded, $"{Id}_TeamMemberName");
                    CustomGUILayout.EditableText(ref member.Role, member.Role, LabelStyle, parent.IsExpanded, $"{Id}_TeamMemberRole");
                    CustomGUILayout.EndVertical();
                    CustomGUILayout.EndHorizontal();

                }

                CustomGUILayout.EndVertical();
            }
        }

        public override MemberBase Copy(NodeBase parent)
        {
            return new Team(this);
        }
    }
}
