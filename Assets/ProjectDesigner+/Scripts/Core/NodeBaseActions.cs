using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using UnityEditor.MemoryProfiler;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// EditorContextAction class used to duplicate a node.
    /// </summary>
    [Serializable]
    public class DuplicateNodesAction : EditorContextAction
    {
        [SerializeReference]
        private List<NodeBase> _nodes;
        [SerializeReference]
        private List<NodeBase> _copiedNodes;

        public DuplicateNodesAction(List<NodeBase> nodes) : base("Duplicate Nodes", $"{nodes.Count} nodes are duplicated.")
        {
            Debug.Assert(nodes.Count > 0);
            _nodes = nodes;
            _copiedNodes = new List<NodeBase>();
        }

        public DuplicateNodesAction(NodeBase node) : base("Duplicate Nodes", $"{node} is duplicated.")
        {
            _nodes = new List<NodeBase> { node };
            _copiedNodes = new List<NodeBase>();
        }

        public override void Do(IEditorContext context)
        {
            _copiedNodes.Clear();
            foreach (var node in _nodes) 
            {
                Debug.Assert(node.CanBeCopied);
                NodeBase copiedNode = node.Copy();
                context.AddDrawable(copiedNode, DrawableCreationType.FromCopy);

                _copiedNodes.Add(copiedNode);
            }
        }

        public override void Undo(IEditorContext context)
        {
            Debug.Assert(_copiedNodes.Count > 0);
            for (int i = _copiedNodes.Count - 1; i >= 0; i--)
            {
                context.DeleteDrawable(_copiedNodes[i]);
            }
            _copiedNodes.Clear();
        }
    }

    /// <summary>
    /// EditorContextAction class used to hide a node.
    /// </summary>
    [Serializable]
    public class HideNodeAction : EditorContextAction
    {
        [SerializeReference]
        private NodeBase _nodeBase;

        public HideNodeAction(NodeBase node) : base("Hide Nodes", $"{node} is hidden.")
        {
            Debug.Assert(node != null);
            _nodeBase = node;
        }

        public override void Do(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null);
            _nodeBase.Hide();
        }

        public override void Undo(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null);
            _nodeBase.Show();
        }
    }

    /// <summary>
    /// EditorContextAction class used to create a node.
    /// </summary>
    [Serializable]
    public class CreateNodeAction : EditorContextAction
    {
        [SerializeField]
        private NodeCreationContext _nodeCreationContext;
        [SerializeReference]
        private NodeBase _nodeBase;

        public CreateNodeAction(NodeCreationContext nodeCreationContext) : base("Create Node", $"New {nodeCreationContext.GetNodeType().Name} is created.")
        {
            _nodeCreationContext = nodeCreationContext;
        }

        public override void Do(IEditorContext context)
        {
            Type type = _nodeCreationContext.GetNodeType();
            if (type != null)
            {
                _nodeBase = _nodeCreationContext.CreateDrawableNodeFromContext(context);
                if (_nodeBase != null)
                {
                    context.AddDrawable(_nodeBase, DrawableCreationType.Default);
                }
            }
            else
            {
                throw new InvalidOperationException("Can not find type.");
            }
        }

        public override void Undo(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null);
            context.DeleteDrawable(_nodeBase);
            _nodeBase = null;
        }
    }

    /// <summary>
    /// EditorContextAction class used to create a node from an asset.
    /// </summary>
    [Serializable]
    public class CreateNodeFromAssetAction : EditorContextAction
    {
        [SerializeReference]
        private NodeBase _nodeBase;

        public CreateNodeFromAssetAction(NodeBase node, UnityEngine.Object asset) : base("Create Node From Asset", $"{node} is created from {asset.name}.")
        {
            _nodeBase = node;
        }

        public override void Do(IEditorContext context)
        {
            context.AddDrawable(_nodeBase, DrawableCreationType.FromAsset);
        }

        public override void Undo(IEditorContext context)
        {
            context.DeleteDrawable(_nodeBase);
        }
    }

    /// <summary>
    /// EditorContextAction class used to delete a node.
    /// </summary>
    [Serializable]
    public class DeleteNodeAction : EditorContextAction
    {
        [SerializeReference]
        private NodeBase _nodeBase;
        [SerializeField]
        private SerializableConnectionDictionary _incomingConnections = new SerializableConnectionDictionary();
        [SerializeField]
        private SerializableConnectionDictionary _outgoingConnections = new SerializableConnectionDictionary();

        public DeleteNodeAction(NodeBase node) : base("Delete Node", $"{node} is deleted.")
        {
            Debug.Assert(node != null);
            _nodeBase = node;
        }

        public override void Do(IEditorContext context)
        {
            DoDeleteConnections(context);
            DoDeleteNodeBase(context);
        }

        internal void DoDeleteConnections(IEditorContext context)
        {
            List<ConnectionBase> connections = _nodeBase.GetConnections();

            for (int i = 0; i < connections.Count; i++)
            {
                _outgoingConnections.Add(connections[i].To, connections[i]);
                _nodeBase.RemoveConnection(connections[i]);
            }

            List<NodeBase> nodes = context.GetDrawables<NodeBase>();
            for (int i = 0; i < nodes.Count; i++)
            {
                NodeBase node = nodes[i];
                if (node != _nodeBase)
                {
                    ConnectionBase connectionTo = node.GetConnectionTo(_nodeBase);
                    if (connectionTo != null)
                    {
                        _incomingConnections.Add(node, connectionTo);
                        node.RemoveConnection(connectionTo);
                    }
                }
            }
        }

        internal void DoDeleteNodeBase(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null);
            context.DeleteDrawable(_nodeBase);
        }

        public override void Undo(IEditorContext context)
        {
            UndoCreateNodeBase(context);
            UndoCreateConnections(context);
        }

        internal void UndoCreateConnections(IEditorContext context)
        {
            foreach (var node in _outgoingConnections.Keys)
            {
                ConnectionBase connection = _outgoingConnections[node];
                _nodeBase.AddConnection(connection, node);

            }

            foreach (var node in _incomingConnections.Keys)
            {
                ConnectionBase connection = _incomingConnections[node];
                node.AddConnection(connection, _nodeBase);
            }
        }

        internal void UndoCreateNodeBase(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null);
            context.AddDrawable(_nodeBase, DrawableCreationType.RevertDelete);
        }
    }

    /// <summary>
    /// EditorContextAction class used to hide node(s).
    /// </summary>
    [Serializable]
    public class HideNodesAction : EditorContextAction
    {
        [SerializeField]
        private List<HideNodeAction> _hideActions = new List<HideNodeAction>();

        public HideNodesAction(List<NodeBase> nodes) : base("Hide Nodes", $"{nodes.Count} nodes are hidden.")
        {
            foreach (var node in nodes)
            {
                _hideActions.Add(new HideNodeAction(node));
            }
        }

        public override void Do(IEditorContext context)
        {
            foreach (var action in _hideActions)
            {
                action.Do(context);
            }
        }

        public override void Undo(IEditorContext context)
        {
            foreach (var action in _hideActions)
            {
                action.Undo(context);
            }
        }
    }


    /// <summary>
    /// EditorContextAction class used to show node(s).
    /// </summary>
    [Serializable]
    public class ShowNodesAction : HideNodesAction
    {
        public ShowNodesAction(List<NodeBase> nodes) : base(nodes)
        {
        }

        public override void Do(IEditorContext context)
        {
            base.Undo(context);
        }

        public override void Undo(IEditorContext context)
        {
            base.Do(context);
        }
    }


    /// <summary>
    /// EditorContextAction class used to delete node(s).
    /// </summary>
    [Serializable]
    public class DeleteNodesAction : EditorContextAction
    {
        [SerializeField]
        private List<DeleteNodeAction> _deleteActions = new List<DeleteNodeAction>();

        public DeleteNodesAction(List<NodeBase> nodes) : base("Delete Nodes", $"{nodes.Count} nodes are deleted.")
        {
            Debug.Assert(nodes != null && nodes.Count > 0);
            foreach (var node in nodes)
            {
                _deleteActions.Add(new DeleteNodeAction(node));
            }
        }

        public override void Do(IEditorContext context)
        {
            foreach (var action in _deleteActions)
            {
                action.DoDeleteConnections(context);
            }

            foreach (var action in _deleteActions)
            {
                action.DoDeleteNodeBase(context);
            }
        }

        public override void Undo(IEditorContext context)
        {
            foreach (var action in _deleteActions)
            {
                action.UndoCreateNodeBase(context);
            }

            foreach (var action in _deleteActions)
            {
                action.UndoCreateConnections(context);
            }
        }
    }

    /// <summary>
    /// EditorContextAction class used to move node(s).
    /// </summary>

    [Serializable]
    public class MoveNodeAction : EditorContextAction
    {
        [SerializeReference]
        private NodeBase _nodeBase;
        [SerializeField]
        private Vector2 _originalPosition;
        [SerializeField]
        private Vector2 _targetPosition;

        public MoveNodeAction(NodeBase node, Vector2 targetPosition) : base("Move Node", $"{node} is moved to {targetPosition}.")
        {
            Debug.Assert(node != null);
            _nodeBase = node;
            _originalPosition = node.Rect.position;
            _targetPosition = targetPosition;
        }

        public override void Do(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null);
            _nodeBase.SetPosition(_targetPosition);
        }

        public override void Undo(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null);
            _nodeBase.SetPosition(_originalPosition);
        }
    }

    /// <summary>
    /// EditorContextAction class used to add a member to a node.
    /// </summary>
    [Serializable]
    public class AddMemberAction : EditorContextAction
    {
        [SerializeReference]
        private NodeBase _nodeBase;
        [SerializeReference]
        private MemberBase _memberBase;

        public AddMemberAction(NodeBase node, MemberBase memberBase) : base("Add New Member", $"{memberBase} is added to {node}.")
        {
            Debug.Assert(node != null);
            _nodeBase = node;
            _memberBase = memberBase;
        }

        public override void Do(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null);
            _nodeBase.AddMember(_memberBase);
        }

        public override void Undo(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null);
            _nodeBase.RemoveMember(_memberBase);
        }
    }

    /// <summary>
    /// EditorContextAction class used to clear all members of a node.
    /// </summary>
    [Serializable]
    public class ClearMembersAction : EditorContextAction
    {
        [SerializeReference]
        private NodeBase _nodeBase;
        [SerializeReference]
        private List<MemberBase> _members;

        public ClearMembersAction(NodeBase node) : base("Clear Members", $"{node} members are cleared.")
        {
            _nodeBase = node;
        }

        public override void Do(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null && _nodeBase.Members.Count > 0);
            _members = new List<MemberBase>(_nodeBase.Members);
            _nodeBase.ClearMembers();
        }

        public override void Undo(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null);
            foreach (var member in _members)
            {
                _nodeBase.AddMember(member);
            }
        }
    }

    /// <summary>
    /// EditorContextAction class used to clear all assignees of a node.
    /// </summary>
    [Serializable]
    public class ClearAssigneesAction : EditorContextAction
    {
        [SerializeReference]
        private NodeBase _nodeBase;
        [SerializeReference]
        private List<TeamMember> _assignees;

        public ClearAssigneesAction(NodeBase node) : base("Clear Assignees", $"{node} assignees are cleared.")
        {
            _nodeBase = node;
        }

        public override void Do(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null && _nodeBase.Members.Count > 0);
            _assignees = new List<TeamMember>(_nodeBase.GetAssignees());
            _nodeBase.ClearAssignees();
        }

        public override void Undo(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null);
            foreach (var member in _assignees)
            {
                _nodeBase.AddAssignee(member);
            }
            _assignees = null;
        }
    }

    /// <summary>
    /// EditorContextAction class used to remove a member from a node.
    /// </summary>
    [Serializable]
    public class RemoveMemberAction : EditorContextAction
    {
        [SerializeReference]
        private NodeBase _nodeBase;
        [SerializeReference]
        private MemberBase _memberBase;

        public RemoveMemberAction(NodeBase node, MemberBase memberBase) : base("Remove Member", $"{memberBase} is removed from {node}.")
        {
            Debug.Assert(node != null);
            _nodeBase = node;
            _memberBase = memberBase;
        }

        public override void Do(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null);
            _nodeBase.RemoveMember(_memberBase);
        }

        public override void Undo(IEditorContext context)
        {
            Debug.Assert(_nodeBase != null);
            _nodeBase.AddMember(_memberBase);
        }
    }
}
