using System;
using System.Linq;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [Serializable]
    public sealed class BoardSavedFilter
    {
        [SerializeField]
        private string _id;
        [SerializeField]
        private string _name;
        [SerializeField]
        private string _searchQuery;
        [SerializeField]
        private string _category;
        [SerializeField]
        private string _requiredTag;
        [SerializeField]
        private bool _includeTechnicalDesign;
        [SerializeField]
        private string _quickFilterId;

        public string Id
        {
            get
            {
                EnsureId();
                return _id;
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = string.IsNullOrWhiteSpace(value) ? "Saved Filter" : value.Trim(); }
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

        public string RequiredTag
        {
            get { return _requiredTag; }
            set { _requiredTag = value ?? string.Empty; }
        }

        public bool IncludeTechnicalDesign
        {
            get { return _includeTechnicalDesign; }
            set { _includeTechnicalDesign = value; }
        }

        public string QuickFilterId
        {
            get { return _quickFilterId; }
            set { _quickFilterId = value ?? string.Empty; }
        }

        public BoardSavedFilter()
        {
            _name = "Saved Filter";
            _searchQuery = string.Empty;
            _category = BoardNodeCategories.All;
            _requiredTag = string.Empty;
            _includeTechnicalDesign = true;
            _quickFilterId = string.Empty;
            EnsureId();
        }

        public BoardSavedFilter(string name, string searchQuery, string category, string requiredTag, bool includeTechnicalDesign, string quickFilterId = "")
            : this()
        {
            Name = name;
            SearchQuery = searchQuery;
            Category = category;
            RequiredTag = requiredTag;
            IncludeTechnicalDesign = includeTechnicalDesign;
            QuickFilterId = quickFilterId;
        }

        public bool Matches(BoardNodeModel node, string searchOverride = null)
        {
            if (node == null)
            {
                return false;
            }

            if (!IncludeTechnicalDesign && node.Category == BoardNodeCategories.TechnicalDesign)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(Category) && Category != BoardNodeCategories.All && node.Category != Category)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(RequiredTag) &&
                !node.Tags.Any(tag => string.Equals(tag, RequiredTag, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            string effectiveQuery = searchOverride ?? SearchQuery;
            return node.MatchesSearch(effectiveQuery);
        }

        public BoardSavedFilter Clone()
        {
            var clone = new BoardSavedFilter(Name, SearchQuery, Category, RequiredTag, IncludeTechnicalDesign, QuickFilterId);
            clone._id = Id;
            return clone;
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
