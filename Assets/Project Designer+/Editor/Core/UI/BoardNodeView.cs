using System;
using System.Collections.Generic;
using ProjectDesigner.V2.BuiltIn;
using ProjectDesigner.V2.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Editor
{
    internal readonly struct BoardNodeSelectionRequest
    {
        public string NodeId { get; }
        public bool Toggle { get; }

        public BoardNodeSelectionRequest(string nodeId, bool toggle)
        {
            NodeId = nodeId ?? string.Empty;
            Toggle = toggle;
        }
    }

    internal sealed class BoardNodeView : VisualElement
    {
        private const float CompactHeightThreshold = 190f;
        private const float TinyHeightThreshold = 150f;
        private static readonly Color ConnectorButtonColor = new Color(0.25f, 0.39f, 0.94f);
        private static readonly Color ConnectorButtonBorderColor = new Color(0.18f, 0.29f, 0.78f);
        private readonly BoardNodeModel _node;
        private readonly IProjectDesignerNodeDefinition _definition;
        private BoardDocument _document;
        private readonly VisualElement _selectionFrame;
        private readonly VisualElement _linkFrame;
        private readonly Label _titleLabel;
        private readonly VisualElement _signalContainer;
        private readonly Label _categoryLabel;
        private readonly Label _previewLabel;
        private readonly Label _metaLabel;
        private readonly VisualElement _tagsContainer;
        private readonly Label _connectHandle;
        private readonly VisualElement _resizeHandle;
        private readonly Color _accentColor;
        private bool _isSelected;
        private bool _isHovered;
        private bool _isConnectionOrigin;
        private bool _hasSignals;
        private bool _hasTags;

        public event Action<BoardNodeSelectionRequest> Selected;
        public event Action<string, Vector2, int> DragStarted;
        public event Action<string, Vector2, int> ConnectionStarted;
        public event Action<string, Vector2, int> ResizeStarted;
        public event Action<string, bool> HoverChanged;

        public string NodeId
        {
            get { return _node.Id; }
        }

        public BoardNodeView(BoardNodeModel node, IProjectDesignerNodeDefinition definition, BoardDocument document)
        {
            _node = node;
            _definition = definition;
            _document = document;
            _accentColor = ResolveAccentColor(node, definition);

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

            _linkFrame = new VisualElement();
            _linkFrame.AddToClassList("pd-node-link-frame");
            _linkFrame.pickingMode = PickingMode.Ignore;
            _linkFrame.style.display = DisplayStyle.None;
            Add(_linkFrame);

            _connectHandle = new Label("Link +")
            {
                tooltip = "Click to choose a target or drag to create a link"
            };
            _connectHandle.AddToClassList("pd-node-connector");
            _connectHandle.pickingMode = PickingMode.Position;
            ApplyDefaultConnectorStyle();
            _connectHandle.RegisterCallback<PointerDownEvent>(OnConnectionPointerDown);
            Add(_connectHandle);

            _resizeHandle = new VisualElement
            {
                tooltip = "Drag to resize"
            };
            _resizeHandle.AddToClassList("pd-node-resize-handle");
            _resizeHandle.pickingMode = PickingMode.Position;
            _resizeHandle.RegisterCallback<PointerDownEvent>(OnResizePointerDown);
            Add(_resizeHandle);

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

            _signalContainer = new VisualElement();
            _signalContainer.AddToClassList("pd-node-signal-row");
            _signalContainer.pickingMode = PickingMode.Ignore;
            Add(_signalContainer);

            _previewLabel = new Label(ProjectDesignerCardPresentation.GetPreviewText(_node, _definition, document));
            _previewLabel.AddToClassList("pd-node-preview");
            _previewLabel.pickingMode = PickingMode.Ignore;
            Add(_previewLabel);

            _metaLabel = new Label(ProjectDesignerCardPresentation.GetSecondaryMetaRichText(_node));
            _metaLabel.AddToClassList("pd-node-meta");
            _metaLabel.pickingMode = PickingMode.Ignore;
            _metaLabel.enableRichText = true;
            _metaLabel.style.display = string.IsNullOrWhiteSpace(_metaLabel.text) ? DisplayStyle.None : DisplayStyle.Flex;
            Add(_metaLabel);

            _tagsContainer = new VisualElement();
            _tagsContainer.AddToClassList("pd-node-tag-row");
            _tagsContainer.pickingMode = PickingMode.Ignore;
            Add(_tagsContainer);

            RefreshTagChips();

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            UpdateConnectorVisibility();
        }

        public void Refresh(BoardDocument document, bool isSelected)
        {
            _document = document;
            _titleLabel.text = _node.Title;
            _categoryLabel.text = _node.Category;
            RefreshSignals(document);
            _previewLabel.text = ProjectDesignerCardPresentation.GetPreviewText(_node, _definition, document);
            _metaLabel.text = ProjectDesignerCardPresentation.GetSecondaryMetaRichText(_node);
            _metaLabel.style.display = string.IsNullOrWhiteSpace(_metaLabel.text) ? DisplayStyle.None : DisplayStyle.Flex;
            RefreshTagChips();

            style.left = _node.Position.x;
            style.top = _node.Position.y;
            style.width = _node.Size.x;
            style.height = _node.Size.y;
            UpdateContentDensity(_node.Size);

            SetSelected(isSelected);
            SetConnectionState(false, false, false);
        }

        public void SetSelected(bool isSelected)
        {
            _isSelected = isSelected;
            EnableInClassList("pd-node-selected", isSelected);
            _selectionFrame.style.display = isSelected ? DisplayStyle.Flex : DisplayStyle.None;
            UpdateConnectorVisibility();
        }

        public void SetConnectionState(bool isOrigin, bool isValidTarget, bool isHoveredTarget)
        {
            _isConnectionOrigin = isOrigin;
            bool hasLinkState = isValidTarget || isHoveredTarget;
            _linkFrame.style.display = hasLinkState ? DisplayStyle.Flex : DisplayStyle.None;
            _linkFrame.EnableInClassList("pd-node-link-valid", isValidTarget && !isHoveredTarget);
            _linkFrame.EnableInClassList("pd-node-link-hover", isHoveredTarget);
            _connectHandle.EnableInClassList("pd-node-connector-active", isOrigin);
            UpdateConnectorVisibility();

            if (isOrigin)
            {
                Color activeColor = new Color(0.3f, 0.48f, 1f);
                _connectHandle.style.backgroundColor = activeColor;
                _connectHandle.style.borderTopColor = activeColor;
                _connectHandle.style.borderRightColor = activeColor;
                _connectHandle.style.borderBottomColor = activeColor;
                _connectHandle.style.borderLeftColor = activeColor;
                _connectHandle.style.color = Color.white;
                return;
            }

            ApplyDefaultConnectorStyle();
        }

        private void ApplyDefaultConnectorStyle()
        {
            Color backgroundColor = ProjectDesignerColorUtility.Blend(ConnectorButtonColor, _accentColor, 0.12f);
            Color borderColor = ProjectDesignerColorUtility.Blend(ConnectorButtonBorderColor, _accentColor, 0.1f);
            _connectHandle.style.backgroundColor = backgroundColor;
            _connectHandle.style.borderTopColor = borderColor;
            _connectHandle.style.borderRightColor = borderColor;
            _connectHandle.style.borderBottomColor = borderColor;
            _connectHandle.style.borderLeftColor = borderColor;
            _connectHandle.style.color = Color.white;
        }

        public void SetPreviewPosition(Vector2 position)
        {
            style.left = position.x;
            style.top = position.y;
        }

        public void SetPreviewSize(Vector2 size)
        {
            style.width = size.x;
            style.height = size.y;
            _previewLabel.text = ProjectDesignerCardPresentation.GetPreviewText(_node, _definition, _document, size);
            UpdateContentDensity(size);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            if (Selected != null)
            {
                Selected.Invoke(new BoardNodeSelectionRequest(_node.Id, evt.shiftKey));
            }

            if (evt.shiftKey)
            {
                evt.StopPropagation();
                return;
            }

            if (DragStarted != null)
            {
                DragStarted.Invoke(_node.Id, GetEventPosition(evt.position), evt.pointerId);
            }

            evt.StopPropagation();
        }

        private void OnConnectionPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            if (Selected != null)
            {
                Selected.Invoke(new BoardNodeSelectionRequest(_node.Id, false));
            }

            if (ConnectionStarted != null)
            {
                ConnectionStarted.Invoke(_node.Id, GetEventPosition(evt.position), evt.pointerId);
            }

            evt.StopImmediatePropagation();
        }

        private void OnResizePointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            if (Selected != null)
            {
                Selected.Invoke(new BoardNodeSelectionRequest(_node.Id, false));
            }

            if (ResizeStarted != null)
            {
                ResizeStarted.Invoke(_node.Id, GetEventPosition(evt.position), evt.pointerId);
            }

            evt.StopPropagation();
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            _isHovered = true;
            EnableInClassList("pd-node-hovered", true);
            UpdateConnectorVisibility();
            if (HoverChanged != null)
            {
                HoverChanged.Invoke(_node.Id, true);
            }
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            _isHovered = false;
            EnableInClassList("pd-node-hovered", false);
            UpdateConnectorVisibility();
            if (HoverChanged != null)
            {
                HoverChanged.Invoke(_node.Id, false);
            }
        }

        private static Vector2 GetEventPosition(Vector3 position)
        {
            return new Vector2(position.x, position.y);
        }

        private void RefreshTagChips()
        {
            _tagsContainer.Clear();

            ProjectDesignerCardTagSummary tagSummary = ProjectDesignerCardPresentation.GetTagSummary(_node);
            _hasTags = tagSummary.VisibleTags.Count > 0 || tagSummary.HiddenCount > 0;
            if (tagSummary.VisibleTags.Count == 0 && tagSummary.HiddenCount == 0)
            {
                _tagsContainer.style.display = DisplayStyle.None;
                return;
            }

            _tagsContainer.style.display = DisplayStyle.Flex;
            foreach (string tag in tagSummary.VisibleTags)
            {
                var chip = new Label(tag);
                chip.AddToClassList("pd-node-tag-chip");
                chip.style.backgroundColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.1f);
                chip.style.borderTopColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.18f);
                chip.style.borderRightColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.18f);
                chip.style.borderBottomColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.18f);
                chip.style.borderLeftColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.18f);
                chip.style.color = ProjectDesignerColorUtility.Blend(_accentColor, new Color(0.18f, 0.22f, 0.26f), 0.28f);
                chip.pickingMode = PickingMode.Ignore;
                _tagsContainer.Add(chip);
            }

            if (tagSummary.HiddenCount > 0)
            {
                var overflowChip = new Label(ProjectDesignerCardPresentation.FormatTagOverflowLabel(tagSummary.HiddenCount));
                overflowChip.AddToClassList("pd-node-tag-chip");
                overflowChip.AddToClassList("pd-node-tag-overflow");
                overflowChip.pickingMode = PickingMode.Ignore;
                _tagsContainer.Add(overflowChip);
            }
        }

        private void RefreshSignals(BoardDocument document)
        {
            _signalContainer.Clear();
            IReadOnlyList<ProjectDesignerCardSignal> signals = ProjectDesignerCardPresentation.GetSignals(_node, document);
            _hasSignals = signals.Count > 0;
            _signalContainer.style.display = signals.Count == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            foreach (ProjectDesignerCardSignal signal in signals)
            {
                AddSignalChip(signal.Text, signal.Color);
            }
        }

        private void AddSignalChip(string text, Color color)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var chip = new Label(text.Trim());
            chip.AddToClassList("pd-node-signal-chip");
            chip.style.backgroundColor = ProjectDesignerColorUtility.WithAlpha(color, 0.12f);
            chip.style.borderTopColor = ProjectDesignerColorUtility.WithAlpha(color, 0.22f);
            chip.style.borderRightColor = ProjectDesignerColorUtility.WithAlpha(color, 0.22f);
            chip.style.borderBottomColor = ProjectDesignerColorUtility.WithAlpha(color, 0.22f);
            chip.style.borderLeftColor = ProjectDesignerColorUtility.WithAlpha(color, 0.22f);
            chip.style.color = ProjectDesignerColorUtility.Blend(color, new Color(0.18f, 0.22f, 0.26f), 0.16f);
            chip.pickingMode = PickingMode.Ignore;
            _signalContainer.Add(chip);
        }

        private void UpdateConnectorVisibility()
        {
            bool isVisible = _isSelected || _isHovered || _isConnectionOrigin;
            _connectHandle.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            _resizeHandle.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateContentDensity(Vector2 size)
        {
            bool isCompact = size.y < CompactHeightThreshold;
            bool isTiny = size.y < TinyHeightThreshold;
            EnableInClassList("pd-node-compact", isCompact);
            EnableInClassList("pd-node-tiny", isTiny);

            _signalContainer.style.display = _hasSignals && !isCompact ? DisplayStyle.Flex : DisplayStyle.None;
            _previewLabel.style.display = isTiny ? DisplayStyle.None : DisplayStyle.Flex;
            _metaLabel.style.display = !isTiny && !string.IsNullOrWhiteSpace(_metaLabel.text) ? DisplayStyle.Flex : DisplayStyle.None;
            _tagsContainer.style.display = _hasTags && !isTiny ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static Color ResolveAccentColor(BoardNodeModel node, IProjectDesignerNodeDefinition definition)
        {
            if (node is NoteNodeModel note)
            {
                return ProjectDesignerColorUtility.ParseOrFallback(note.ResolvedAccentHex, new Color(0.17f, 0.56f, 0.85f));
            }

            return ProjectDesignerColorUtility.ParseOrFallback(
                definition == null ? "#6E6E6E" : definition.AccentColor,
                new Color(0.43f, 0.43f, 0.43f));
        }

    }
}
