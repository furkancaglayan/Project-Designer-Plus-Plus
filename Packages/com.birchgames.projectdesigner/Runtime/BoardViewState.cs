using System;
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
        private string _activeFilterId;

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
            set { _selectedNodeId = value ?? string.Empty; }
        }

        public string ActiveFilterId
        {
            get { return _activeFilterId; }
            set { _activeFilterId = value ?? string.Empty; }
        }

        public BoardViewState()
        {
            _panOffset = Vector2.zero;
            _zoom = 1f;
            _searchQuery = string.Empty;
            _category = BoardNodeCategories.All;
            _selectedNodeId = string.Empty;
            _activeFilterId = string.Empty;
        }

        public BoardViewState Clone()
        {
            return new BoardViewState
            {
                PanOffset = PanOffset,
                Zoom = Zoom,
                SearchQuery = SearchQuery,
                Category = Category,
                SelectedNodeId = SelectedNodeId,
                ActiveFilterId = ActiveFilterId
            };
        }
    }
}
