using System;
using System.Collections;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// EditorContextAction class used to create an undo-redoable connection.
    /// </summary>
    [Serializable]
    public class CreateConnectionAction : EditorContextAction
    {
        [SerializeField]
        private ConnectionCreationContext _connectionContext;
        [SerializeField]
        private ConnectionBase _connection;

        //<inheritdoc>
        public CreateConnectionAction(ConnectionCreationContext connectionCreation) : base("Create new Connection", $"New {connectionCreation.GetConnectionType().Name} is created.")
        {
            _connectionContext = connectionCreation;
        }

        //<inheritdoc>
        public override void Do(IEditorContext context)
        {
            Debug.Assert(_connectionContext.From != null);
            Debug.Assert(_connectionContext.From.CanHaveConnectionOfType(_connectionContext.To, _connectionContext.GetConnectionType()));

            ConnectionBase connectionBase = _connectionContext.CreateDrawableConnectionFromContext(context);
            context.AddDrawable(connectionBase);
        }

        //<inheritdoc>
        public override void Undo(IEditorContext context)
        {
            Debug.Assert(_connectionContext.From != null);
            Debug.Assert(_connection != null);
            _connection.From.RemoveConnection(_connection);
            context.DeleteDrawable(_connection);
            _connection = null;
        }
    }

    /// <summary>
    /// EditorContextAction class used to delete a connection.
    /// </summary>
    [Serializable]
    public class DeleteConnectionAction : EditorContextAction
    {
        [SerializeField]
        private ConnectionCreationContext _connectionContext;
        [SerializeField]
        private ConnectionBase _connection;

        //<inheritdoc>
        public DeleteConnectionAction(ConnectionBase connectionBase) : base("Delete Connection", $"{connectionBase.GetType().Name} is deleted.")
        {
            _connection = connectionBase;
        }

        //<inheritdoc>
        public override void Do(IEditorContext context)
        {
            Debug.Assert(_connection != null);
            Debug.Assert(_connection.From != null);
            _connectionContext = new ConnectionCreationContext(_connection.From, _connection.To, _connection.GetType());
            _connection.From.RemoveConnection(_connection);
            _connection.Id = UniqueIdGenerator.GenerateUniqueId();
            context.DeleteDrawable(_connection);
            _connection = null;
        }

        //<inheritdoc>
        public override void Undo(IEditorContext context)
        {
            Debug.Assert(_connectionContext.From != null);
            Debug.Assert(_connectionContext.From.CanHaveConnectionOfType(_connectionContext.To, _connectionContext.GetConnectionType()));
            ConnectionBase connectionBase = _connectionContext.CreateDrawableConnectionFromContext(context);
            context.AddDrawable(connectionBase);
            _connectionContext = default(ConnectionCreationContext);
        }
    }
}
