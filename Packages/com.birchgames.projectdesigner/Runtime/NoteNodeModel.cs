using System;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [Serializable]
    public sealed class NoteNodeModel : BoardNodeModel
    {
        [SerializeField]
        private string _body;
        [SerializeField]
        private string _accentHex;

        public override string Category
        {
            get { return BoardNodeCategories.Reference; }
        }

        public string Body
        {
            get { return _body; }
            set { _body = value ?? string.Empty; }
        }

        public string AccentHex
        {
            get { return _accentHex; }
            set { _accentHex = string.IsNullOrEmpty(value) ? "#2B90D9" : value; }
        }

        public NoteNodeModel()
            : base(BoardNodeTypeIds.Note, "Note", new Vector2(120f, 400f), new Vector2(300f, 220f))
        {
            _body = "Drop in ideas, risks, questions, or meeting notes.";
            _accentHex = "#2B90D9";
        }

        public override BoardNodeModel Clone()
        {
            var clone = new NoteNodeModel
            {
                Body = Body,
                AccentHex = AccentHex
            };
            CopyCommonTo(clone);
            return clone;
        }

        public override string GetSearchText()
        {
            return string.Join(" ", new[] { base.GetSearchText(), Body });
        }
    }
}
