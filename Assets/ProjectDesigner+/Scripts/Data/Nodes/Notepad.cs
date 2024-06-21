using JetBrains.Annotations;
using ProjectDesigner.Core;
using ProjectDesigner.Data.Connections;
using ProjectDesigner.Data.Members;
using System;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Data.Nodes
{
    /// <summary>
    /// <see cref="Notepad"/> represents a node that allows users to capture ideas, take notes, and attach images.
    /// </summary>
    [Serializable, NodeBaseMetaData("Note")]
    public class Notepad : NodeBase
    {
        public override Vector2 MinSize => new Vector2(440, 440);
        public override Vector2 MaxSize =>  new Vector2(440, 720);

        public override string IconKey => "note";
        public override bool CanBeCopied => true;

        protected override int FooterHeight => 10;

        protected override Color ConnectionPointColor => new Color(131f / 255f, 96f / 255f, 74f / 255f);

        public Notepad() : base()
        {
            HeaderText = "New Note";
        }

        public Notepad(Notepad simpleCard) : base(simpleCard)
        {
            HeaderText = simpleCard.HeaderText;
        }


        public Notepad(string header) : base()
        {
            HeaderText = header;
        }

        protected override void OnAddedInternal(IEditorContext context, DrawableCreationType drawableCreationType)
        {
            if (drawableCreationType == DrawableCreationType.Default)
            {
                AddMember<CommentMember>();
            }
        }

        protected override void OnContextClick(IEditorContext context, Vector2 position, GenericMenu menu)
        {
            menu.AddItem(new GUIContent("New Comment"), false, context.ProcessAction, new AddMemberAction(this, new CommentMember()));
            menu.AddItem(new GUIContent("New Image"), false, context.ProcessAction, new AddMemberAction(this, new ImageMember()));
        }


        protected override void OnDrawFooter(IEditorContext context, Rect rect, RectOffset padding)
        {
          
        }

        public override Core.NodeBase Copy()
        {
           return new Notepad(this);
        }

        protected override bool CanHaveConnections()
        {
            return true;
        }

        protected override bool CanHaveConnectionOfType(Type connectionType)
        {
            return connectionType.IsSubclassOf(typeof(RelationConnection));
        }


        [NodeBaseAssetMap(typeof(TextAsset))]
        private static NodeBase CreateFromTextAsset(UnityEngine.Object textAsset)
        {
            TextAsset text = (TextAsset)textAsset;
            Notepad notepad = new Notepad();
            notepad.AddMember(new CommentMember(text.text));
            return notepad;
        }

        [NodeBaseAssetMap(typeof(Texture2D))]
        private static NodeBase CreateFromTexture2DAsset(UnityEngine.Object textureAsset)
        {
            Texture2D texture = (Texture2D)textureAsset;
            Notepad notepad = new Notepad();
            notepad.AddMember(new ImageMember(texture));
            return notepad;
        }

        [NodeBaseAssetMap(typeof(GameObject))]
        private static NodeBase CreateFromGameObject(UnityEngine.Object asset)
        {
            Notepad notepad = new Notepad();
            Texture2D preview = AssetPreview.GetAssetPreview(asset);
            if (preview != null)
            {
                notepad.AddMember(new ImageMember(preview));
                return notepad;
            }
            else
            {
                return null;
            }
        }
    }
}
