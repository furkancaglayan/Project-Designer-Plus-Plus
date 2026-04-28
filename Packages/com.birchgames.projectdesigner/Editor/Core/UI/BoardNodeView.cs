using System;
using ProjectDesigner.V2.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class BoardNodeView : VisualElement
    {
        private readonly BoardNodeModel _node;
        private readonly IProjectDesignerNodeDefinition _definition;
        private readonly VisualElement _selectionFrame;
        private readonly Label _titleLabel;
        private readonly Label _categoryLabel;
        private readonly Label _previewLabel;
        private readonly Label _tagsLabel;
        private readonly Color _accentColor;

        public event Action<string> Selected;
        public event Action<string, Vector2, int> DragStarted;

        public string NodeId
        {
            get { return _node.Id; }
        }

        public BoardNodeView(BoardNodeModel node, IProjectDesignerNodeDefinition definition, BoardDocument document)
        {
            _node = node;
            _definition = definition;
            _accentColor = ProjectDesignerColorUtility.ParseOrFallback(
                _definition == null ? "#6E6E6E" : _definition.AccentColor,
                new Color(0.43f, 0.43f, 0.43f));

            AddToClassList("pd-node");
            usageHints = UsageHints.DynamicTransform;
            pickingMode = PickingMode.Position;
            style.overflow = Overflow.Hidden;
            style.position = Position.Absolute;
            style.left = _node.Position.x;
            style.top = _node.Position.y;
            style.width = _node.Size.x;
            style.height = _node.Size.y;
            style.borderLeftWidth = 4f;
            style.borderLeftColor = _accentColor;

            _selectionFrame = new VisualElement();
            _selectionFrame.AddToClassList("pd-node-selection-frame");
            _selectionFrame.pickingMode = PickingMode.Ignore;
            _selectionFrame.style.display = DisplayStyle.None;
            Add(_selectionFrame);

            _categoryLabel = new Label(_node.Category);
            _categoryLabel.AddToClassList("pd-node-category");
            _categoryLabel.AddToClassList("pd-node-category-chip");
            _categoryLabel.pickingMode = PickingMode.Ignore;
            _categoryLabel.style.backgroundColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.12f);
            _categoryLabel.style.borderTopColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.18f);
            _categoryLabel.style.borderRightColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.18f);
            _categoryLabel.style.borderBottomColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.18f);
            _categoryLabel.style.borderLeftColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.18f);
            _categoryLabel.style.color = ProjectDesignerColorUtility.Blend(_accentColor, new Color(0.18f, 0.22f, 0.26f), 0.38f);
            Add(_categoryLabel);

            _titleLabel = new Label(_node.Title);
            _titleLabel.AddToClassList("pd-node-title");
            _titleLabel.pickingMode = PickingMode.Ignore;
            _titleLabel.style.color = ProjectDesignerColorUtility.Blend(_accentColor, new Color(0.18f, 0.22f, 0.26f), 0.34f);
            Add(_titleLabel);

            _previewLabel = new Label(_definition == null ? string.Empty : _definition.GetPreview(_node, document));
            _previewLabel.AddToClassList("pd-node-preview");
            _previewLabel.pickingMode = PickingMode.Ignore;
            Add(_previewLabel);

            _tagsLabel = new Label(_node.GetTagsCsv());
            _tagsLabel.AddToClassList("pd-node-tags");
            _tagsLabel.pickingMode = PickingMode.Ignore;
            _tagsLabel.style.color = ProjectDesignerColorUtility.Blend(_accentColor, new Color(0.18f, 0.22f, 0.26f), 0.2f);
            Add(_tagsLabel);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        public void Refresh(BoardDocument document, bool isSelected)
        {
            _titleLabel.text = _node.Title;
            _categoryLabel.text = _node.Category;
            _previewLabel.text = _definition == null ? string.Empty : _definition.GetPreview(_node, document);
            _tagsLabel.text = _node.GetTagsCsv();

            style.left = _node.Position.x;
            style.top = _node.Position.y;
            style.width = _node.Size.x;
            style.height = _node.Size.y;

            SetSelected(isSelected);
        }

        public void SetSelected(bool isSelected)
        {
            EnableInClassList("pd-node-selected", isSelected);
            _selectionFrame.style.display = isSelected ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetPreviewPosition(Vector2 position)
        {
            style.left = position.x;
            style.top = position.y;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            if (Selected != null)
            {
                Selected.Invoke(_node.Id);
            }

            if (DragStarted != null)
            {
                DragStarted.Invoke(_node.Id, GetEventPosition(evt.position), evt.pointerId);
            }

            evt.StopPropagation();
        }

        private static Vector2 GetEventPosition(Vector3 position)
        {
            return new Vector2(position.x, position.y);
        }
    }
}
