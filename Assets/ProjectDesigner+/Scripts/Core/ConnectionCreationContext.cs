using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// ConnectionCreationContext is a wrapper class used when creating <see cref="ConnectionBase"/>s.
    /// </summary>
    [Serializable]
    public struct ConnectionCreationContext
    {
        [SerializeField]
        public NodeBase From;
        [SerializeField]
        public NodeBase To;
        [SerializeField]
        private string _assemblyQualifiedTypeName;

        /// <summary>
        /// Creates a new ConnectionCreationContext from <paramref name="from"/> to <paramref name="to"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="to"></param>
        /// <param name="connectionType"></param>
        public ConnectionCreationContext(NodeBase from, NodeBase to, Type connectionType)
        {
            From = from;
            To = to;
            _assemblyQualifiedTypeName = connectionType.AssemblyQualifiedName;
        }


        /// <summary>
        /// Creates a <see cref="ConnectionBase"/> from the given type in constructor.
        /// </summary>
        /// <param name="editorContext">Editor context</param>
        /// <returns></returns>
        public ConnectionBase CreateDrawableConnectionFromContext(IEditorContext editorContext)
        {
            Type type = GetConnectionType();
            if (type == null)
            {
                editorContext.RegisterError("Could not find type to create connection", $"Assembly qualified type name: {_assemblyQualifiedTypeName}");
                return null;
            }

            ConnectionBase connection;
            try
            {
                connection = (ConnectionBase)Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                editorContext.RegisterError($"Could not create connection with type {type}", ex.Message);
                return null;
            }

            From.AddConnection(connection, To);

            return connection;
        }

        /// <summary>
        /// Returns the connection type of the context.
        /// </summary>
        /// <returns></returns>
        public Type GetConnectionType()
        {
            return Type.GetType(_assemblyQualifiedTypeName);
        }
    }
}
