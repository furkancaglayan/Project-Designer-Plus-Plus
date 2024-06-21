using ProjectDesigner.Core;
using ProjectDesigner.Data.Members;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Data.Nodes
{
    /// <summary>
    /// <see cref="ProjectDashboard"/> is a special node type that visualizes in progress tasks, team members and project history of a <see cref="Project"/> object.
    /// </summary>
    [Serializable, NodeBaseMetaData("Project Dashboard", 1, false)]
    public class ProjectDashboard : NodeBase
    {
        public override Vector2 MinSize => new Vector2(700, 500);
        public override Vector2 MaxSize => new Vector2(800, 600);
        public override string IconKey => "project_designer_icon";
        public override string TextureKeyOff => "node1";
        public override string TextureKeyOn => "node1 on";
        public override bool CanBeCopied => false;

        protected override Color ConnectionPointColor => new Color(154f / 255f, 154f / 255f, 90f/ 255f);

        public ProjectDashboard() : base()
        {
            HeaderText = "Project Dashboard";
        }

        public override NodeBase Copy()
        {
            throw new InvalidOperationException();
        }

        protected override void OnAddedInternal(IEditorContext context, DrawableCreationType drawableCreationType)
        {
            AddMember<ProjectStatus>();
            AddMember<Team>();
            AddMember<ProjectHistory>();
        }

        protected override bool CanHaveConnections()
        {
            return false;
        }
    }
}
