using System;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    public enum BoardEdgeAnchor
    {
        Auto,
        Left,
        Right,
        Top,
        Bottom
    }

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
        [SerializeField]
        private BoardEdgeAnchor _sourceAnchor;
        [SerializeField]
        private BoardEdgeAnchor _targetAnchor;

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

        public BoardEdgeAnchor SourceAnchor
        {
            get { return _sourceAnchor; }
            set { _sourceAnchor = IsDefined(value) ? value : BoardEdgeAnchor.Auto; }
        }

        public BoardEdgeAnchor TargetAnchor
        {
            get { return _targetAnchor; }
            set { _targetAnchor = IsDefined(value) ? value : BoardEdgeAnchor.Auto; }
        }

        public BoardEdgeModel()
        {
            _typeId = BoardEdgeTypeIds.Dependency;
            _sourceNodeId = string.Empty;
            _targetNodeId = string.Empty;
            _label = string.Empty;
            _sourceAnchor = BoardEdgeAnchor.Auto;
            _targetAnchor = BoardEdgeAnchor.Auto;
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
            clone.SourceAnchor = SourceAnchor;
            clone.TargetAnchor = TargetAnchor;
            return clone;
        }

        public void EnsureDefaults()
        {
            _typeId = string.IsNullOrWhiteSpace(_typeId) ? BoardEdgeTypeIds.Dependency : _typeId;
            _sourceNodeId = _sourceNodeId ?? string.Empty;
            _targetNodeId = _targetNodeId ?? string.Empty;
            _label = _label ?? string.Empty;
            SourceAnchor = SourceAnchor;
            TargetAnchor = TargetAnchor;
            EnsureId();
        }

        public void RegenerateId()
        {
            _id = Guid.NewGuid().ToString("N");
        }

        private static bool IsDefined(BoardEdgeAnchor anchor)
        {
            return anchor == BoardEdgeAnchor.Auto ||
                   anchor == BoardEdgeAnchor.Left ||
                   anchor == BoardEdgeAnchor.Right ||
                   anchor == BoardEdgeAnchor.Top ||
                   anchor == BoardEdgeAnchor.Bottom;
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
