using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [Serializable]
    public sealed class BoardViewState
    {
        [SerializeField]
        private Vector2 _panOffset;
        [SerializeField]
        private float _zoom;
        [SerializeField]
        private string _searchQuery;
        [SerializeField]
        private string _category;
        [SerializeField]
        private string _selectedNodeId;
        [SerializeField]
        private List<string> _selectedNodeIds = new List<string>();
        [SerializeField]
        private string _activeFilterId;
        [SerializeField]
        private string _quickFilterId;
        [SerializeField]
        private bool _snapToGrid;

        public Vector2 PanOffset
        {
            get { return _panOffset; }
            set { _panOffset = value; }
        }

        public float Zoom
        {
            get { return _zoom; }
            set { _zoom = Mathf.Clamp(value, 0.35f, 2.5f); }
        }

        public string SearchQuery
        {
            get { return _searchQuery; }
            set { _searchQuery = value ?? string.Empty; }
        }

        public string Category
        {
            get { return string.IsNullOrEmpty(_category) ? BoardNodeCategories.All : _category; }
            set { _category = string.IsNullOrEmpty(value) ? BoardNodeCategories.All : value; }
        }

        public string SelectedNodeId
        {
            get { return _selectedNodeId; }
            set
            {
                _selectedNodeId = value ?? string.Empty;
                EnsureSelectionDefaults();

                if (string.IsNullOrEmpty(_selectedNodeId))
                {
                    _selectedNodeIds.Clear();
                    return;
                }

                if (_selectedNodeIds.Count == 0)
                {
                    _selectedNodeIds.Add(_selectedNodeId);
                    return;
                }

                if (_selectedNodeIds.Count == 1)
                {
                    _selectedNodeIds[0] = _selectedNodeId;
                    return;
                }

                if (_selectedNodeIds.Contains(_selectedNodeId))
                {
                    return;
                }

                _selectedNodeIds.Clear();
                _selectedNodeIds.Add(_selectedNodeId);
            }
        }

        public IReadOnlyList<string> SelectedNodeIds
        {
            get
            {
                EnsureSelectionDefaults();
                return _selectedNodeIds;
            }
        }

        public string ActiveFilterId
        {
            get { return _activeFilterId; }
            set { _activeFilterId = value ?? string.Empty; }
        }

        public bool SnapToGrid
        {
            get { return _snapToGrid; }
            set { _snapToGrid = value; }
        }

        public string QuickFilterId
        {
            get { return _quickFilterId; }
            set { _quickFilterId = value ?? string.Empty; }
        }

        public BoardViewState()
        {
            _panOffset = Vector2.zero;
            _zoom = 1f;
            _searchQuery = string.Empty;
            _category = BoardNodeCategories.All;
            _selectedNodeId = string.Empty;
            _selectedNodeIds = new List<string>();
            _activeFilterId = string.Empty;
            _quickFilterId = string.Empty;
            _snapToGrid = false;
        }

        public BoardViewState Clone()
        {
            var clone = new BoardViewState
            {
                PanOffset = PanOffset,
                Zoom = Zoom,
                SearchQuery = SearchQuery,
                Category = Category,
                SelectedNodeId = SelectedNodeId,
                ActiveFilterId = ActiveFilterId,
                QuickFilterId = QuickFilterId,
                SnapToGrid = SnapToGrid
            };
            clone.SetSelection(SelectedNodeIds, SelectedNodeId);
            return clone;
        }

        public void SetSelection(IEnumerable<string> nodeIds, string primaryNodeId = null)
        {
            EnsureSelectionDefaults();

            List<string> normalized = (nodeIds ?? Enumerable.Empty<string>())
                .Where(nodeId => !string.IsNullOrEmpty(nodeId))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            _selectedNodeIds = normalized;
            if (_selectedNodeIds.Count == 0)
            {
                _selectedNodeId = string.Empty;
                return;
            }

            if (string.IsNullOrEmpty(primaryNodeId) || !_selectedNodeIds.Contains(primaryNodeId))
            {
                _selectedNodeId = _selectedNodeIds[0];
                return;
            }

            _selectedNodeId = primaryNodeId;
        }

        public void SelectSingle(string nodeId)
        {
            SetSelection(string.IsNullOrEmpty(nodeId) ? Enumerable.Empty<string>() : new[] { nodeId }, nodeId);
        }

        public void ToggleSelection(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                return;
            }

            EnsureSelectionDefaults();
            List<string> selection = _selectedNodeIds.ToList();
            if (selection.Contains(nodeId))
            {
                selection.Remove(nodeId);
                SetSelection(selection, _selectedNodeId == nodeId ? selection.FirstOrDefault() : _selectedNodeId);
                return;
            }

            selection.Add(nodeId);
            SetSelection(selection, nodeId);
        }

        public void Deselect(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                return;
            }

            EnsureSelectionDefaults();
            if (!_selectedNodeIds.Contains(nodeId))
            {
                return;
            }

            List<string> selection = _selectedNodeIds.Where(selectedId => selectedId != nodeId).ToList();
            SetSelection(selection, _selectedNodeId == nodeId ? selection.FirstOrDefault() : _selectedNodeId);
        }

        public bool IsSelected(string nodeId)
        {
            return !string.IsNullOrEmpty(nodeId) && SelectedNodeIds.Contains(nodeId);
        }

        public void ClearSelection()
        {
            SetSelection(Enumerable.Empty<string>(), string.Empty);
        }

        private void EnsureSelectionDefaults()
        {
            _selectedNodeIds = _selectedNodeIds ?? new List<string>();

            if (string.IsNullOrEmpty(_selectedNodeId))
            {
                if (_selectedNodeIds.Count == 0)
                {
                    return;
                }

                _selectedNodeId = _selectedNodeIds[0];
                return;
            }

            if (_selectedNodeIds.Count == 0)
            {
                _selectedNodeIds.Add(_selectedNodeId);
                return;
            }

            if (!_selectedNodeIds.Contains(_selectedNodeId))
            {
                _selectedNodeIds.Insert(0, _selectedNodeId);
            }
        }
    }
}
