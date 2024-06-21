using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// A wrapper struct to be used when creation <see cref="NodeBase"/> objects.
    /// </summary>
    [Serializable]
    public struct NodeCreationContext
    {
        /// <summary>
        /// Canvas position of the node to be created.
        /// </summary>
        [SerializeField]
        public Vector2 Position;
        [SerializeField]
        private string _assemblyQualifiedTypeName;

        public NodeCreationContext(Vector2 position, Type type)
        {
            _assemblyQualifiedTypeName = type.AssemblyQualifiedName;
            Position = position;
        }

        /// <summary>
        /// Creates a <see cref="NodeBase"/> from the given type in constructor.
        /// </summary>
        /// <param name="editorContext">Editor context</param>
        /// <returns></returns>
        public NodeBase CreateDrawableNodeFromContext(IEditorContext editorContext)
        {
            Type type = GetNodeType();
            if (type == null)
            {
                editorContext.RegisterError("Could not find type to create node", $"Assembly qualified type name: {_assemblyQualifiedTypeName}");
                return null;
            }

            NodeBase node;
            try
            {
                node = Activator.CreateInstance(type) as NodeBase;
            }
            catch (Exception ex)
            {
                editorContext.RegisterError($"Could not create node with type {type}", ex.Message);
                return null;
            }
            node.SetPosition(Position);
            node.SetTargetSize(node.MinSize);
            return node;
        }

        /// <summary>
        /// Returns the node type of the context.
        /// </summary>
        /// <returns></returns>
        public Type GetNodeType()
        {
            return Type.GetType(_assemblyQualifiedTypeName);
        }
    }
}
