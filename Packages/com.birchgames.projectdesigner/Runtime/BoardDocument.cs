using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [Serializable]
    public sealed class BoardDocument
    {
        [SerializeField]
        private string _boardName;
        [SerializeField]
        private string _summary;
        [SerializeField]
        private List<string> _teamMembers = new List<string>();
        [SerializeField]
        private BoardViewState _viewState = new BoardViewState();
        [SerializeField]
        private List<BoardSavedFilter> _savedFilters = new List<BoardSavedFilter>();
        [SerializeField]
        private List<BoardTemplateDefinition> _templates = new List<BoardTemplateDefinition>();
        [SerializeField]
        private List<BoardEdgeModel> _edges = new List<BoardEdgeModel>();
        [SerializeReference]
        private List<BoardNodeModel> _nodes = new List<BoardNodeModel>();

        public string BoardName
        {
            get { return string.IsNullOrWhiteSpace(_boardName) ? "Project Board" : _boardName; }
            set { _boardName = string.IsNullOrWhiteSpace(value) ? "Project Board" : value.Trim(); }
        }

        public string Summary
        {
            get { return _summary; }
            set { _summary = value ?? string.Empty; }
        }

        public List<string> TeamMembers
        {
            get { return _teamMembers; }
        }

        public List<BoardNodeModel> Nodes
        {
            get { return _nodes; }
        }

        public List<BoardEdgeModel> Edges
        {
            get { return _edges; }
        }

        public List<BoardSavedFilter> SavedFilters
        {
            get { return _savedFilters; }
        }

        public List<BoardTemplateDefinition> Templates
        {
            get { return _templates; }
        }

        public BoardViewState ViewState
        {
            get { return _viewState; }
            set { _viewState = value ?? new BoardViewState(); }
        }

        public BoardDocument()
        {
            _boardName = "Project Board";
            _summary = "A node-based pre-production workspace for Unity teams.";
            EnsureDefaults();
        }

        public BoardDocument(string boardName) : this()
        {
            BoardName = boardName;
        }

        public void EnsureDefaults()
        {
            _boardName = BoardName;
            _summary = Summary;
            _teamMembers = _teamMembers ?? new List<string>();
            _nodes = _nodes ?? new List<BoardNodeModel>();
            _edges = _edges ?? new List<BoardEdgeModel>();
            _savedFilters = _savedFilters ?? new List<BoardSavedFilter>();
            _templates = _templates ?? new List<BoardTemplateDefinition>();
            _viewState = _viewState ?? new BoardViewState();
        }

        public BoardNodeModel GetNode(string nodeId)
        {
            return _nodes.FirstOrDefault(node => node != null && node.Id == nodeId);
        }

        public IEnumerable<BoardEdgeModel> GetOutgoingEdges(string nodeId)
        {
            return _edges.Where(edge => edge.SourceNodeId == nodeId);
        }

        public IEnumerable<BoardEdgeModel> GetIncomingEdges(string nodeId)
        {
            return _edges.Where(edge => edge.TargetNodeId == nodeId);
        }

        public void AddNode(BoardNodeModel node)
        {
            if (node == null)
            {
                return;
            }

            if (_nodes.All(existing => existing.Id != node.Id))
            {
                _nodes.Add(node);
            }
        }

        public void ReplaceNode(BoardNodeModel node)
        {
            if (node == null)
            {
                return;
            }

            int index = _nodes.FindIndex(existing => existing.Id == node.Id);
            if (index >= 0)
            {
                _nodes[index] = node;
            }
            else
            {
                _nodes.Add(node);
            }
        }

        public void RemoveNode(string nodeId)
        {
            _nodes.RemoveAll(node => node != null && node.Id == nodeId);
            _edges.RemoveAll(edge => edge.SourceNodeId == nodeId || edge.TargetNodeId == nodeId);
            if (ViewState.SelectedNodeId == nodeId)
            {
                ViewState.SelectedNodeId = string.Empty;
            }
        }

        public void AddEdge(BoardEdgeModel edge)
        {
            if (edge == null)
            {
                return;
            }

            bool alreadyExists = _edges.Any(existing =>
                existing.SourceNodeId == edge.SourceNodeId &&
                existing.TargetNodeId == edge.TargetNodeId &&
                existing.TypeId == edge.TypeId);

            if (!alreadyExists)
            {
                _edges.Add(edge);
            }
        }

        public void RemoveEdge(string edgeId)
        {
            _edges.RemoveAll(edge => edge.Id == edgeId);
        }

        public void UpsertFilter(BoardSavedFilter filter)
        {
            if (filter == null)
            {
                return;
            }

            int index = _savedFilters.FindIndex(existing => existing.Id == filter.Id);
            if (index >= 0)
            {
                _savedFilters[index] = filter;
            }
            else
            {
                _savedFilters.Add(filter);
            }
        }

        public BoardDocument DeepClone()
        {
            var clone = new BoardDocument(BoardName)
            {
                Summary = Summary,
                ViewState = ViewState.Clone()
            };

            clone._teamMembers = new List<string>(_teamMembers);
            clone._nodes = _nodes.Select(node => node == null ? null : node.Clone()).ToList();
            clone._edges = _edges.Select(edge => edge == null ? null : edge.Clone()).ToList();
            clone._savedFilters = _savedFilters.Select(filter => filter == null ? null : filter.Clone()).ToList();
            clone._templates = _templates.Select(template => template == null ? null : template.Clone()).ToList();
            clone.EnsureDefaults();
            return clone;
        }

        public void CopyFrom(BoardDocument other)
        {
            if (other == null)
            {
                return;
            }

            BoardName = other.BoardName;
            Summary = other.Summary;
            _teamMembers = new List<string>(other.TeamMembers);
            _nodes = other.Nodes.Select(node => node == null ? null : node.Clone()).ToList();
            _edges = other.Edges.Select(edge => edge == null ? null : edge.Clone()).ToList();
            _savedFilters = other.SavedFilters.Select(filter => filter == null ? null : filter.Clone()).ToList();
            _templates = other.Templates.Select(template => template == null ? null : template.Clone()).ToList();
            _viewState = other.ViewState == null ? new BoardViewState() : other.ViewState.Clone();
            EnsureDefaults();
        }
    }
}
