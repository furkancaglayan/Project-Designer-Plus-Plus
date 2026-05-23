using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [Serializable]
    public abstract class BoardNodeModel
    {
        public const float MinimumWidth = 180f;
        public const float MinimumHeight = 180f;

        [SerializeField]
        private string _id;
        [SerializeField]
        private string _typeId;
        [SerializeField]
        private string _title;
        [SerializeField]
        private Vector2 _position;
        [SerializeField]
        private Vector2 _size;
        [SerializeField]
        private List<string> _tags = new List<string>();
        [SerializeField]
        private bool _isPinned;

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
            protected set { _typeId = value ?? string.Empty; }
        }

        public string Title
        {
            get { return _title; }
            set { _title = string.IsNullOrWhiteSpace(value) ? "Untitled" : value.Trim(); }
        }

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Vector2 Size
        {
            get { return _size; }
            set { _size = ClampSize(value); }
        }

        public IReadOnlyList<string> Tags
        {
            get { return _tags; }
        }

        public bool IsPinned
        {
            get { return _isPinned; }
            set { _isPinned = value; }
        }

        public abstract string Category { get; }

        protected BoardNodeModel()
        {
            EnsureId();
            _title = "Untitled";
            _position = Vector2.zero;
            _size = new Vector2(280f, 180f);
        }

        protected BoardNodeModel(string typeId, string title, Vector2 position, Vector2 size) : this()
        {
            TypeId = typeId;
            Title = title;
            Position = position;
            Size = size;
        }

        protected void CopyCommonTo(BoardNodeModel other)
        {
            other._id = Id;
            other._typeId = TypeId;
            other._title = Title;
            other._position = Position;
            other._size = Size;
            other._tags = new List<string>(_tags);
            other._isPinned = IsPinned;
        }

        public string GetTagsCsv()
        {
            return string.Join(", ", _tags);
        }

        public void SetTagsFromCsv(string csv)
        {
            _tags = (csv ?? string.Empty)
                .Split(',')
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrEmpty(tag))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public virtual bool MatchesSearch(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return true;
            }

            return GetSearchText().IndexOf(searchQuery.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public virtual string GetSearchText()
        {
            return string.Join(" ", new[] { Title, string.Join(" ", _tags) });
        }

        public virtual void EnsureDefaults()
        {
            _size = ClampSize(_size);
        }

        public abstract BoardNodeModel Clone();

        public static Vector2 ClampSize(Vector2 size)
        {
            return new Vector2(Mathf.Max(MinimumWidth, size.x), Mathf.Max(MinimumHeight, size.y));
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
