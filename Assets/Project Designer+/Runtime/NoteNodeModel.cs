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
        [SerializeField]
        private string _lastValidAccentHex;

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
            set
            {
                _accentHex = string.IsNullOrWhiteSpace(value) ? DefaultAccentHex : value.Trim();
                string normalized;
                if (TryNormalizeAccentHex(_accentHex, out normalized))
                {
                    _accentHex = normalized;
                    _lastValidAccentHex = normalized;
                }
            }
        }

        public string ResolvedAccentHex
        {
            get
            {
                EnsureDefaults();
                return _lastValidAccentHex;
            }
        }

        public const string DefaultAccentHex = "#2B90D9";

        public NoteNodeModel()
            : base(BoardNodeTypeIds.Note, "Note", new Vector2(120f, 400f), new Vector2(300f, 220f))
        {
            _body = "Drop in ideas, risks, questions, or meeting notes.";
            _accentHex = DefaultAccentHex;
            _lastValidAccentHex = DefaultAccentHex;
        }

        public override BoardNodeModel Clone()
        {
            var clone = new NoteNodeModel
            {
                Body = Body,
                AccentHex = AccentHex
            };
            clone._lastValidAccentHex = ResolvedAccentHex;
            CopyCommonTo(clone);
            return clone;
        }

        public override void EnsureDefaults()
        {
            base.EnsureDefaults();
            _body = _body ?? string.Empty;

            string normalized;
            if (TryNormalizeAccentHex(_accentHex, out normalized))
            {
                _accentHex = normalized;
                _lastValidAccentHex = normalized;
                return;
            }

            if (TryNormalizeAccentHex(_lastValidAccentHex, out normalized))
            {
                _lastValidAccentHex = normalized;
                return;
            }

            _lastValidAccentHex = DefaultAccentHex;
            if (string.IsNullOrWhiteSpace(_accentHex))
            {
                _accentHex = DefaultAccentHex;
            }
        }

        public override string GetSearchText()
        {
            return string.Join(" ", new[] { base.GetSearchText(), Body });
        }

        public static bool TryNormalizeAccentHex(string value, out string normalizedHex)
        {
            normalizedHex = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string candidate = value.Trim();
            if (!candidate.StartsWith("#", StringComparison.Ordinal))
            {
                candidate = "#" + candidate;
            }

            Color color;
            if (!ColorUtility.TryParseHtmlString(candidate, out color))
            {
                return false;
            }

            normalizedHex = "#" + ColorUtility.ToHtmlStringRGB(color);
            return true;
        }
    }
}
