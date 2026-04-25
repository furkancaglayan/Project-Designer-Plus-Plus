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
        private readonly Func<float> _zoomGetter;
        private readonly Label _titleLabel;
        private readonly Label _categoryLabel;
        private readonly Label _previewLabel;
        private readonly Label _tagsLabel;

        private bool _dragging;
        private Vector2 _dragStartMousePosition;
        private Vector2 _dragStartNodePosition;
        private Vector2 _dragCurrentPosition;

        public event Action<string> Selected;
        public event Action<string, Vector2> MoveCompleted;

        public string NodeId
        {
            get { return _node.Id; }
        }

        public BoardNodeView(BoardNodeModel node, IProjectDesignerNodeDefinition definition, BoardDocument document, Func<float> zoomGetter)
        {
            _node = node;
            _definition = definition;
            _zoomGetter = zoomGetter;

            AddToClassList("pd-node");
            style.position = Position.Absolute;
            style.left = _node.Position.x;
            style.top = _node.Position.y;
            style.width = _node.Size.x;
            style.height = _node.Size.y;
            style.borderLeftWidth = 4f;
            style.borderLeftColor = ParseColor(_definition == null ? "#6E6E6E" : _definition.AccentColor, new Color(0.43f, 0.43f, 0.43f));

            _categoryLabel = new Label(_node.Category);
            _categoryLabel.AddToClassList("pd-node-category");
            Add(_categoryLabel);

            _titleLabel = new Label(_node.Title);
            _titleLabel.AddToClassList("pd-node-title");
            Add(_titleLabel);

            _previewLabel = new Label(_definition == null ? string.Empty : _definition.GetPreview(_node, document));
            _previewLabel.AddToClassList("pd-node-preview");
            Add(_previewLabel);

            _tagsLabel = new Label(_node.GetTagsCsv());
            _tagsLabel.AddToClassList("pd-node-tags");
            Add(_tagsLabel);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
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

            EnableInClassList("pd-node-selected", isSelected);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            _dragging = true;
            _dragStartMousePosition = evt.position;
            _dragStartNodePosition = _node.Position;
            _dragCurrentPosition = _node.Position;
            CapturePointer(evt.pointerId);

            if (Selected != null)
            {
                Selected.Invoke(_node.Id);
            }

            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_dragging || !HasPointerCapture(evt.pointerId))
            {
                return;
            }

            float zoom = Mathf.Max(0.01f, _zoomGetter());
            Vector2 delta = (evt.position - _dragStartMousePosition) / zoom;
            _dragCurrentPosition = _dragStartNodePosition + delta;
            style.left = _dragCurrentPosition.x;
            style.top = _dragCurrentPosition.y;
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_dragging || !HasPointerCapture(evt.pointerId))
            {
                return;
            }

            ReleasePointer(evt.pointerId);
            _dragging = false;

            if ((_dragCurrentPosition - _dragStartNodePosition).sqrMagnitude > 0.01f && MoveCompleted != null)
            {
                MoveCompleted.Invoke(_node.Id, _dragCurrentPosition);
            }

            evt.StopPropagation();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            _dragging = false;
        }

        private static Color ParseColor(string htmlColor, Color fallback)
        {
            Color parsed;
            return ColorUtility.TryParseHtmlString(htmlColor, out parsed) ? parsed : fallback;
        }
    }
}
