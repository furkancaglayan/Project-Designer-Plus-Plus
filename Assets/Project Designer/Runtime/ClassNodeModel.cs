using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [Serializable]
    public sealed class ClassNodeModel : BoardNodeModel
    {
        [SerializeField]
        private string _namespaceName;
        [SerializeField]
        private string _summary;
        [SerializeField]
        private List<BoardClassMemberData> _fields = new List<BoardClassMemberData>();
        [SerializeField]
        private List<BoardClassMemberData> _methods = new List<BoardClassMemberData>();

        public override string Category
        {
            get { return BoardNodeCategories.TechnicalDesign; }
        }

        public string NamespaceName
        {
            get { return _namespaceName; }
            set { _namespaceName = value ?? string.Empty; }
        }

        public string Summary
        {
            get { return _summary; }
            set { _summary = value ?? string.Empty; }
        }

        public IList<BoardClassMemberData> Fields
        {
            get { return _fields; }
        }

        public IList<BoardClassMemberData> Methods
        {
            get { return _methods; }
        }

        public ClassNodeModel()
            : base(BoardNodeTypeIds.Class, "Class", new Vector2(820f, 140f), new Vector2(340f, 260f))
        {
            _namespaceName = "Game";
            _summary = "Use this for technical design, ownership, and class relationships.";
            _fields = new List<BoardClassMemberData>();
            _methods = new List<BoardClassMemberData>();
        }

        public override BoardNodeModel Clone()
        {
            var clone = new ClassNodeModel
            {
                NamespaceName = NamespaceName,
                Summary = Summary
            };
            clone._fields = _fields.Select(item => item.Clone()).ToList();
            clone._methods = _methods.Select(item => item.Clone()).ToList();
            CopyCommonTo(clone);
            return clone;
        }

        public override string GetSearchText()
        {
            return string.Join(" ", new[]
            {
                base.GetSearchText(),
                NamespaceName,
                Summary,
                string.Join(" ", _fields.Select(item => item.Signature)),
                string.Join(" ", _methods.Select(item => item.Signature))
            });
        }
    }
}
