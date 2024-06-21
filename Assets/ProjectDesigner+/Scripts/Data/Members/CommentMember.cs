using ProjectDesigner.Core;
using ProjectDesigner.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Data.Members
{
    /// <summary>
    /// A <see cref="CommentMember"/> is used for writing in nodes.
    /// </summary>
    [Serializable]
    public class CommentMember : MemberBase
    {
        private const int TextCharacterLimit = 1000;
        private const int HeaderCharacterLimit = 50;
        private const int Space = 10;

        [SerializeField]
        private string _text;
        [SerializeField]
        private string _header;

        public CommentMember() : this("Type Here")
        {

        }

        public CommentMember(string label = "Type Here", string header = "", int priority = 0) : base(priority)
        {
            _text = label;
            _header = header;
            Debug.Assert(_header.Length < HeaderCharacterLimit);
            if (_header.Length > HeaderCharacterLimit)
            {
                _header = _header.Substring(0, HeaderCharacterLimit);
            }
        }

        public CommentMember(CommentMember other) : this(other._text, other._header)
        {

        }

        public override void Draw(IEditorContext context, NodeBase parent, float width)
        {
            if (!string.IsNullOrEmpty(_header))
            {
                CustomGUILayout.Label(_header, HeaderStyle);
                CustomGUILayout.Space(Space);
            }

            CustomGUILayout.BeginVertical();
            if (parent.IsExpanded)
            {
                _text = CustomGUILayout.TextArea(_text, Id, LabelStyle);

            }
            else
            {
                CustomGUILayout.Label(_text, LabelWithRichTextStyle);
            }
            CustomGUILayout.EndVertical();

            if (_text.Length > TextCharacterLimit)
            {
                _text = _text.Substring(0, TextCharacterLimit);
            }
        }

        public override void OnAdded(NodeBase nodeBase)
        {
        }

        public override MemberBase Copy(NodeBase parent)
        {
            return new CommentMember(this);
        }

        /// <summary>
        /// Returns the actual text written in the node.
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            return _text;
        }
    }
}
