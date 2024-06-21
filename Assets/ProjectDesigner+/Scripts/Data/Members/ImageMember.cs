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
    /// <see cref="ImageMember"/> is simply an image attachment. Images can be selected from an object field.
    /// </summary>
    [Serializable]
    public class ImageMember : MemberBase
    {
        private const string CheckersKey = "checkers";
        [SerializeField]
        private Texture2D _image;
        private GUIStyle Style => GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.Label);

        public ImageMember(Texture2D texture = null) : base()
        {
            _image = texture;
        }

        public ImageMember(ImageMember other) : this(other._image)
        {
            _image = other._image;
        }

        public override void Draw(IEditorContext context, NodeBase parent, float width)
        {
            width -= 20;
            if (parent.IsExpanded)
            {
                CustomGUILayout.BeginVertical();
                CustomGUILayout.Label("Select Image: ", Style);
                _image = CustomGUILayout.ObjectField(_image);
                CustomGUILayout.EndVertical();
            }

            CustomGUILayout.BeginVertical();

            if (_image != null)
            {
                CustomGUILayout.Image(_image, Style, width, width);
            }
            else
            {
                Texture2D image = GUIStyleCollection.GetTexture(CheckersKey);
                if (image != null)
                {
                    CustomGUILayout.Image(image, Style, width);
                }
            }

            CustomGUILayout.EndVertical();
        }

        public override void OnAdded(NodeBase nodeBase)
        {
        }

        public override MemberBase Copy(NodeBase parent)
        {
            return new ImageMember(this);
        }
    }
}
