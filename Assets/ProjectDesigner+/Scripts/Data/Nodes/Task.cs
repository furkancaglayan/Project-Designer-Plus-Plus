using ProjectDesigner.Helpers;
using ProjectDesigner.Core;
using ProjectDesigner.Data.Connections;
using ProjectDesigner.Data.Members;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Data.Nodes
{
    /// <summary>
    /// <see cref="Task"/> nodes used for representing actual tasks in a project that can be assigned team members, set due dates and connect to other tasks as sub-tasks.
    /// </summary>
    [Serializable, NodeBaseMetaData("Task")]
    public class Task : NodeBase
    {
        public override Vector2 MinSize => new Vector2(400, 360);
        public override Vector2 MaxSize =>  new Vector2(480, 420);
        protected override int HeaderHeight => 80;

        public override string IconKey => "task";

        public override bool CanBeCopied => true;
        public override string TextureKeyOff => "node3";
        public override string TextureKeyOn => "node3 on";
        protected override int FooterHeight => 20;

        protected override Color ConnectionPointColor => new Color(131f / 255f, 96f / 255f, 74f / 255f);

        private CommentMember _comment => GetFirstMember<CommentMember>();
        private AssigneeMember _assignees => GetFirstMember<AssigneeMember>();
        private TaskStatusMember _taskStatus => GetFirstMember<TaskStatusMember>();
        private SubTasksMember _subtasks => GetFirstMember<SubTasksMember>();
        private DateTimeMember _dueDate => GetFirstMember<DateTimeMember>();

        public Task(Task task) : base(task)
        {
            HeaderText = task.HeaderText;
        }


        public Task() : base()
        {
            HeaderText = "New Task";
        }

        public override string ToString()
        {
            return $"{HeaderText} ({GetStatus()})";
        }

        protected override void OnAddedInternal(IEditorContext context, DrawableCreationType drawableCreationType)
        {
            if (drawableCreationType == DrawableCreationType.Default)
            {
                AddMember(new TaskStatusMember());
                AddMember(new CommentMember("Type here"));
                AddMember(new DateTimeMember("Task Due Time: "));
                AddMember(new AssigneeMember(this));
                AddMember(new SubTasksMember());
            }
        }

        protected override void OnContextClick(IEditorContext context, Vector2 position, GenericMenu menu)
        {
            if (_dueDate == null)
            {
                menu.AddItem(new GUIContent("Add Due Time"), false, context.ProcessAction, new AddMemberAction(this, new DateTimeMember("Task Due Time: ")));
            }
            else
            {
                menu.AddItem(new GUIContent("Remove Due Time"), false, context.ProcessAction, new RemoveMemberAction(this, _dueDate));
            }

            if (_comment == null)
            {
                menu.AddItem(new GUIContent("Add Task Explanation"), false, context.ProcessAction, new AddMemberAction(this, new CommentMember()));
            }
            else
            {
                menu.AddItem(new GUIContent("Remove Task Explanation"), false, context.ProcessAction, new RemoveMemberAction(this, _comment));
            }

            if (_subtasks == null)
            {
                menu.AddItem(new GUIContent("Add Subtasks Field"), false, context.ProcessAction, new AddMemberAction(this, new SubTasksMember()));
            }
            else
            {
                menu.AddItem(new GUIContent("Remove Subtasks Field"), false, context.ProcessAction, new RemoveMemberAction(this, _subtasks));
            }

            if (_subtasks == null)
            {
                menu.AddItem(new GUIContent("Add Subtasks Field"), false, context.ProcessAction, new AddMemberAction(this, new SubTasksMember()));
            }
            else
            {
                menu.AddItem(new GUIContent("Remove Subtasks Field"), false, context.ProcessAction, new RemoveMemberAction(this, _subtasks));
            }

            if (_assignees == null)
            {
                menu.AddItem(new GUIContent("Add Assignee Field"), false, context.ProcessAction, new AddMemberAction(this, new AssigneeMember(this)));
            }
            else
            {
                menu.AddItem(new GUIContent("Remove Assignee Field"), false, context.ProcessAction, new RemoveMemberAction(this, _assignees));
            }

            if (_taskStatus == null)
            {
                menu.AddItem(new GUIContent("Add Task Status Field"), false, context.ProcessAction, new AddMemberAction(this, new TaskStatusMember()));
            }
            else
            {
                menu.AddItem(new GUIContent("Remove Task Status Field"), false, context.ProcessAction, new RemoveMemberAction(this, _taskStatus));
            }

            for (int i = 0; i < (int)NodeStatus.NumStatus; i++)
            {
                NodeStatus status = (NodeStatus)i;
                menu.AddMenuActionOnCondition($"Set Status/{status}", () => SetStatus(status), i != (int)GetStatus());
            }
        }


        public override Core.NodeBase Copy()
        {
           return new Task(this);
        }

        protected override bool CanHaveConnections()
        {
            return true;
        }

        protected override bool CanHaveConnectionOfType(Type connectionType)
        {
            return connectionType == typeof(SubTaskConnection) || connectionType.IsSubclassOf(typeof(RelationConnection));
        }
    }
}
