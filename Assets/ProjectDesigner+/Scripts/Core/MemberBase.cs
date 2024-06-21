using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// A <see cref="MemberBase"/> is used to create visuals, texts and informations on a <see cref="NodeBase"/>.
    /// </summary>
    [Serializable]
    public abstract class MemberBase
    {
        /// <summary>
        /// Maximum number of members in a node.
        /// </summary>
        public const int MemberLimit = 40;
        /// <summary>
        /// Priority is used when sorting members in a node.
        /// </summary>
        [field: SerializeField]
        public int Priority { get; private set; }
        /// <summary>
        /// Unique identifier of the <see cref="MemberBase"/>.
        /// </summary>
        [field: SerializeField]
        public string Id { get; private set; }  

        /// <summary>
        /// Creates a <see cref="MemberBase"/> with 0 priority and a unique id.
        /// </summary>
        public MemberBase()
        {
            Priority = 0;
            Id = UniqueIdGenerator.GenerateUniqueId();
        }

        /// <summary>
        /// Creates a <see cref="MemberBase"/> with given <paramref name="priority"/> and a unique id.
        /// </summary>
        public MemberBase(int priority)
        {
            Priority = priority;
            Id = UniqueIdGenerator.GenerateUniqueId();
        }

        public override string ToString()
        {
            return $"{GetType().Name}({Id})";
        }

        /// <summary>
        /// Called after a <see cref="MemberBase"/> is added to a <paramref name="nodeBase"/>.
        /// </summary>
        /// <param name="nodeBase">Parent node</param>
        public abstract void OnAdded(NodeBase nodeBase);

        /// <summary>
        /// Draws the member.
        /// </summary>
        /// <param name="context">Editor context</param>
        /// <param name="parent">Parent node</param>
        /// <param name="width">Width of the member area of the node</param>
        public abstract void Draw(IEditorContext context, NodeBase parent, float width);
        /// <summary>
        /// Copies the <see cref="MemberBase"/>. As <see cref="NodeBase"/> can be copied, <see cref="MemberBase"/> must also be able to be copied.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public abstract MemberBase Copy(NodeBase parent);

        /// <summary>
        /// Default label style. See: <see cref="GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory)"/>
        /// </summary>
        protected GUIStyle LabelStyle => GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.Label);
        /// <summary>
        /// Default rich-text enabled, label style. See: <see cref="GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory)"/>
        /// </summary>
        protected GUIStyle LabelWithRichTextStyle => GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.LabelRichText);
        /// <summary>
        /// Default left-aligned label style. See: <see cref="GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory)"/>
        /// </summary>
        protected GUIStyle LeftAlignedLabelStyle => GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.LabelLeftAligned);
        /// <summary>
        /// Default left-aligned header style. See: <see cref="GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory)"/>
        /// </summary>
        protected GUIStyle LeftAlignedHeaderStyle => GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.HeaderLeftAligned);
        /// <summary>
        /// Default header style. See: <see cref="GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory)"/>
        /// </summary>
        protected GUIStyle HeaderStyle => GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.Header);
        /// <summary>
        /// Default image style. See: <see cref="GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory)"/>
        /// </summary>
        protected GUIStyle ImageStyle => GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.Image);
        /// <summary>
        /// Default foldout style. See: <see cref="GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory)"/>
        /// </summary>
        protected GUIStyle FoldoutStyle => GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.Foldout);
        /// <summary>
        /// Default button style. Contrary to others, this isn't fetched from <see cref="GUIStyleCollection"/> and instead uses <see cref="EditorStyles.toolbarButton"/>.
        /// </summary>
        protected GUIStyle ButtonStyle => EditorStyles.toolbarButton;
    }
}
