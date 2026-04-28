using System;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [Serializable]
    public sealed class BoardEdgeModel
    {
        [SerializeField]
        private string _id;
        [SerializeField]
        private string _typeId;
        [SerializeField]
        private string _sourceNodeId;
        [SerializeField]
        private string _targetNodeId;
        [SerializeField]
        private string _label;

        public string Id
        {
            get
            {
                EnsureId();
                return _id;
            }
        }

        public string TypeId
        {
            get { return _typeId; }
            set { _typeId = value ?? BoardEdgeTypeIds.Dependency; }
        }

        public string SourceNodeId
        {
            get { return _sourceNodeId; }
            set { _sourceNodeId = value ?? string.Empty; }
        }

        public string TargetNodeId
        {
            get { return _targetNodeId; }
            set { _targetNodeId = value ?? string.Empty; }
        }

        public string Label
        {
            get { return _label; }
            set { _label = value ?? string.Empty; }
        }

        public BoardEdgeModel()
        {
            _typeId = BoardEdgeTypeIds.Dependency;
            _sourceNodeId = string.Empty;
            _targetNodeId = string.Empty;
            _label = string.Empty;
            EnsureId();
        }

        public BoardEdgeModel(string typeId, string sourceNodeId, string targetNodeId, string label = null)
            : this()
        {
            TypeId = typeId;
            SourceNodeId = sourceNodeId;
            TargetNodeId = targetNodeId;
            Label = label ?? string.Empty;
        }

        public BoardEdgeModel Clone()
        {
            var clone = new BoardEdgeModel(TypeId, SourceNodeId, TargetNodeId, Label);
            clone._id = Id;
            return clone;
        }

        public void RegenerateId()
        {
            _id = Guid.NewGuid().ToString("N");
        }

        private void EnsureId()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = Guid.NewGuid().ToString("N");
            }
        }
    }
}
