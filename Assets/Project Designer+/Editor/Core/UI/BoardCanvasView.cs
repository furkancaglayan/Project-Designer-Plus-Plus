using System;
using System.Collections.Generic;
using System.Linq;
using ProjectDesigner.V2.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class BoardCanvasView : VisualElement
    {
        private sealed class ConnectionPreviewData
        {
            public string SourceNodeId;
            public Vector2 EndPosition;
            public Color AccentColor;
        }

        private sealed class EdgeLayerElement : VisualElement
        {
            private readonly ProjectBoardAsset _boardAsset;

            public IDictionary<string, Vector2> PreviewNodePositions { get; set; }
            public IDictionary<string, Vector2> PreviewNodeSizes { get; set; }
            public HashSet<string> VisibleNodeIds { get; set; }
            public HashSet<string> SelectedNodeIds { get; set; }
            public string HoveredNodeId { get; set; }
            public string HoveredEdgeId { get; set; }
            public ConnectionPreviewData PreviewConnection { get; set; }

            public EdgeLayerElement(ProjectBoardAsset boardAsset)
            {
                _boardAsset = boardAsset;
                pickingMode = PickingMode.Ignore;
                generateVisualContent += OnGenerateVisualContent;
            }

            private void OnGenerateVisualContent(MeshGenerationContext context)
            {
                if (_boardAsset == null || _boardAsset.Document == null)
                {
                    return;
                }

                var painter = context.painter2D;

                foreach (BoardEdgeModel edge in _boardAsset.Document.Edges)
                {
                    BoardNodeModel source = _boardAsset.Document.GetNode(edge.SourceNodeId);
                    BoardNodeModel target = _boardAsset.Document.GetNode(edge.TargetNodeId);
                    if (source == null || target == null)
                    {
                        continue;
                    }

                    if (VisibleNodeIds != null &&
                        (!VisibleNodeIds.Contains(source.Id) || !VisibleNodeIds.Contains(target.Id)))
                    {
                        continue;
                    }

                    IProjectDesignerEdgeDefinition definition = ProjectDesignerRegistry.GetEdgeDefinition(edge.TypeId);
                    bool isHoveredEdge = IsHoveredEdge(edge);
                    bool isHighlighted = IsHighlighted(edge);
                    painter.lineWidth = isHoveredEdge ? 3.2f : (isHighlighted ? 2.4f : 1.45f);
                    painter.strokeColor = ApplyAlpha(
                        ParseColor(definition == null ? "#9AA3AF" : definition.AccentColor, new Color(0.6f, 0.6f, 0.6f)),
                        isHoveredEdge ? 0.98f : (isHighlighted ? 0.86f : 0.2f));
                    DrawCurve(
                        painter,
                        edge,
                        source,
                        target,
                        GetNodePosition(source),
                        GetNodePosition(target),
                        GetNodeSize(source),
                        GetNodeSize(target));
                }

                if (PreviewConnection == null)
                {
                    return;
                }

                BoardNodeModel previewSource = _boardAsset.Document.GetNode(PreviewConnection.SourceNodeId);
                if (previewSource == null)
                {
                    return;
                }

                Vector2 sourcePosition = GetNodePosition(previewSource);
                Vector2 start = GetAnchorPoint(previewSource, sourcePosition, GetNodeSize(previewSource), BoardEdgeAnchor.Auto, true, PreviewConnection.EndPosition);
                Vector2 end = PreviewConnection.EndPosition;
                Vector2 startDirection = GetAnchorDirection(start, end, BoardEdgeAnchor.Auto, true);
                Vector2 endDirection = GetAnchorDirection(end, start, BoardEdgeAnchor.Auto, false);
                float tangent = Mathf.Max(80f, Vector2.Distance(start, end) * 0.35f);

                painter.strokeColor = PreviewConnection.AccentColor;
                painter.lineWidth = 3f;
                painter.BeginPath();
                painter.MoveTo(start);
                painter.BezierCurveTo(start + startDirection * tangent, end + endDirection * tangent, end);
                painter.Stroke();
            }

            private bool IsHighlighted(BoardEdgeModel edge)
            {
                if (edge == null)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(HoveredEdgeId))
                {
                    return IsHoveredEdge(edge);
                }

                if (!string.IsNullOrEmpty(HoveredNodeId) &&
                    (string.Equals(edge.SourceNodeId, HoveredNodeId, StringComparison.Ordinal) ||
                     string.Equals(edge.TargetNodeId, HoveredNodeId, StringComparison.Ordinal)))
                {
                    return true;
                }

                return SelectedNodeIds != null &&
                       (SelectedNodeIds.Contains(edge.SourceNodeId) || SelectedNodeIds.Contains(edge.TargetNodeId));
            }

            private bool IsHoveredEdge(BoardEdgeModel edge)
            {
                return edge != null && string.Equals(edge.Id, HoveredEdgeId, StringComparison.Ordinal);
            }

            private void DrawCurve(
                UnityEngine.UIElements.Painter2D painter,
                BoardEdgeModel edge,
                BoardNodeModel source,
                BoardNodeModel target,
                Vector2 sourcePosition,
                Vector2 targetPosition,
                Vector2 sourceSize,
                Vector2 targetSize)
            {
                Vector2 sourceCenter = sourcePosition + sourceSize * 0.5f;
                Vector2 targetCenter = targetPosition + targetSize * 0.5f;
                Vector2 start = GetAnchorPoint(source, sourcePosition, sourceSize, edge.SourceAnchor, true, targetCenter);
                Vector2 end = GetAnchorPoint(target, targetPosition, targetSize, edge.TargetAnchor, false, sourceCenter);
                Vector2 startDirection = GetAnchorDirection(start, end, edge.SourceAnchor, true);
                Vector2 endDirection = GetAnchorDirection(end, start, edge.TargetAnchor, false);
                float tangent = Mathf.Max(80f, Vector2.Distance(start, end) * 0.35f);

                painter.BeginPath();
                painter.MoveTo(start);
                painter.BezierCurveTo(start + startDirection * tangent, end + endDirection * tangent, end);
                painter.Stroke();
            }

            private Vector2 GetNodePosition(BoardNodeModel node)
            {
                if (node == null)
                {
                    return Vector2.zero;
                }

                if (PreviewNodePositions != null && PreviewNodePositions.TryGetValue(node.Id, out Vector2 previewPosition))
                {
                    return previewPosition;
                }

                return node.Position;
            }

            private Vector2 GetNodeSize(BoardNodeModel node)
            {
                if (node == null)
                {
                    return Vector2.zero;
                }

                if (PreviewNodeSizes != null && PreviewNodeSizes.TryGetValue(node.Id, out Vector2 previewSize))
                {
                    return previewSize;
                }

                return node.Size;
            }

            private static Vector2 GetAnchorPoint(BoardNodeModel node, Vector2 nodePosition, Vector2 nodeSize, BoardEdgeAnchor anchor, bool isSource, Vector2 otherCenter)
            {
                if (anchor == BoardEdgeAnchor.Auto)
                {
                    anchor = ResolveAutoAnchor(nodePosition, nodeSize, otherCenter, isSource);
                }

                switch (anchor)
                {
                    case BoardEdgeAnchor.Left:
                        return new Vector2(nodePosition.x, nodePosition.y + nodeSize.y * 0.5f);
                    case BoardEdgeAnchor.Right:
                        return new Vector2(nodePosition.x + nodeSize.x, nodePosition.y + nodeSize.y * 0.5f);
                    case BoardEdgeAnchor.Top:
                        return new Vector2(nodePosition.x + nodeSize.x * 0.5f, nodePosition.y);
                    case BoardEdgeAnchor.Bottom:
                        return new Vector2(nodePosition.x + nodeSize.x * 0.5f, nodePosition.y + nodeSize.y);
                    default:
                        return nodePosition + nodeSize * 0.5f;
                }
            }

            private static BoardEdgeAnchor ResolveAutoAnchor(Vector2 nodePosition, Vector2 nodeSize, Vector2 otherCenter, bool isSource)
            {
                Vector2 center = nodePosition + nodeSize * 0.5f;
                Vector2 delta = otherCenter - center;
                if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                {
                    return delta.x >= 0f ? BoardEdgeAnchor.Right : BoardEdgeAnchor.Left;
                }

                return delta.y >= 0f ? BoardEdgeAnchor.Bottom : BoardEdgeAnchor.Top;
            }

            private static Vector2 GetAnchorDirection(Vector2 anchorPoint, Vector2 otherPoint, BoardEdgeAnchor anchor, bool isSource)
            {
                if (anchor == BoardEdgeAnchor.Auto)
                {
                    Vector2 delta = otherPoint - anchorPoint;
                    if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                    {
                        return delta.x >= 0f ? Vector2.right : Vector2.left;
                    }

                    return delta.y >= 0f ? Vector2.up : Vector2.down;
                }

                switch (anchor)
                {
                    case BoardEdgeAnchor.Left:
                        return Vector2.left;
                    case BoardEdgeAnchor.Right:
                        return Vector2.right;
                    case BoardEdgeAnchor.Top:
                        return Vector2.down;
                    case BoardEdgeAnchor.Bottom:
                        return Vector2.up;
                    default:
                        return isSource ? Vector2.right : Vector2.left;
                }
            }

            private static Color ApplyAlpha(Color color, float alpha)
            {
                color.a = alpha;
                return color;
            }
        }

        private sealed class AlignmentGuideLayerElement : VisualElement
        {
            public IReadOnlyList<BoardAlignmentGuide> Guides { get; private set; }
            public float Zoom { get; set; }

            public AlignmentGuideLayerElement()
            {
                Guides = new List<BoardAlignmentGuide>();
                Zoom = 1f;
                pickingMode = PickingMode.Ignore;
                generateVisualContent += OnGenerateVisualContent;
            }

            public void SetGuides(IReadOnlyList<BoardAlignmentGuide> guides)
            {
                Guides = guides ?? new List<BoardAlignmentGuide>();
                MarkDirtyRepaint();
            }

            private void OnGenerateVisualContent(MeshGenerationContext context)
            {
                if (Guides == null || Guides.Count == 0)
                {
                    return;
                }

                var painter = context.painter2D;
                float zoom = Mathf.Max(0.01f, Zoom);
                painter.lineWidth = 1.45f / zoom;

                foreach (BoardAlignmentGuide guide in Guides)
                {
                    painter.strokeColor = new Color(0.27f, 0.51f, 1f, 0.94f);
                    Vector2 start = guide.Orientation == BoardAlignmentGuideOrientation.Vertical
                        ? new Vector2(guide.Position, guide.Start)
                        : new Vector2(guide.Start, guide.Position);
                    Vector2 end = guide.Orientation == BoardAlignmentGuideOrientation.Vertical
                        ? new Vector2(guide.Position, guide.End)
                        : new Vector2(guide.End, guide.Position);
                    DrawDottedLine(painter, start, end, 7f / zoom, 5f / zoom);
                }
            }

            private static void DrawDottedLine(UnityEngine.UIElements.Painter2D painter, Vector2 start, Vector2 end, float dashLength, float gapLength)
            {
                float distance = Vector2.Distance(start, end);
                if (distance <= 0.01f)
                {
                    return;
                }

                Vector2 direction = (end - start) / distance;
                float cursor = 0f;
                painter.BeginPath();
                while (cursor < distance)
                {
                    float dashEnd = Mathf.Min(cursor + Mathf.Max(0.1f, dashLength), distance);
                    painter.MoveTo(start + direction * cursor);
                    painter.LineTo(start + direction * dashEnd);
                    cursor = dashEnd + Mathf.Max(0.1f, gapLength);
                }

                painter.Stroke();
            }
        }

        private const string DefaultHintText = "Right-click for board actions. Drag cards to arrange them. Shift-drag empty space to multi-select. Drag from Link to create relationships. Use Ctrl+D, Ctrl+A, Delete, and F for quick editing.";
        private const float AlignmentGuideBoardTolerance = 1f;
        private const float LinkClickScreenThreshold = 8f;

        private readonly ProjectBoardAsset _boardAsset;
        private readonly IBoardCommandDispatcher _dispatcher;
        private readonly VisualElement _contentLayer;
        private readonly EdgeLayerElement _edgeLayer;
        private readonly VisualElement _nodeLayer;
        private readonly AlignmentGuideLayerElement _guideLayer;
        private readonly Label _hintLabel;
        private readonly VisualElement _marqueeElement;
        private readonly Dictionary<string, BoardNodeView> _nodeViews = new Dictionary<string, BoardNodeView>();
        private readonly Dictionary<string, Vector2> _previewNodePositions = new Dictionary<string, Vector2>();
        private readonly Dictionary<string, Vector2> _previewNodeSizes = new Dictionary<string, Vector2>();
        private readonly Dictionary<string, Vector2> _dragStartNodePositions = new Dictionary<string, Vector2>();
        private HashSet<string> _visibleNodeIds = new HashSet<string>();
        private Dictionary<string, List<ProjectDesignerLinkOption>> _connectionTargetOptions = new Dictionary<string, List<ProjectDesignerLinkOption>>();

        private int _dragPointerId = -1;
        private Vector2 _dragStartMousePosition;
        private readonly List<string> _draggingNodeIds = new List<string>();

        private string _resizingNodeId = string.Empty;
        private int _resizePointerId = -1;
        private Vector2 _resizeStartMousePosition;
        private Vector2 _resizeStartSize;

        private string _connectingNodeId = string.Empty;
        private int _connectPointerId = -1;
        private Vector2 _connectStartCanvasPosition;
        private Vector2 _connectPreviewPosition;
        private string _hoveredConnectionTargetId = string.Empty;
        private string _hoveredNodeId = string.Empty;
        private VisualElement _connectionMenu;

        private bool _panning;
        private Vector2 _panStartMousePosition;
        private Vector2 _panStartOffset;
        private bool _marqueeSelecting;
        private int _marqueePointerId = -1;
        private Vector2 _marqueeStartCanvasPosition;
        private Vector2 _marqueeCurrentCanvasPosition;
        private List<string> _marqueeBaseSelection = new List<string>();

        public event Action SelectionChanged;

        public BoardCanvasView(ProjectBoardAsset boardAsset, IBoardCommandDispatcher dispatcher)
        {
            _boardAsset = boardAsset;
            _dispatcher = dispatcher;

            AddToClassList("pd-canvas");
            focusable = true;
            style.overflow = Overflow.Hidden;
            generateVisualContent += OnGenerateGrid;

            _contentLayer = new VisualElement();
            _contentLayer.usageHints = UsageHints.DynamicTransform;
            _contentLayer.style.transformOrigin = new TransformOrigin(new Length(0f), new Length(0f), 0f);
            _contentLayer.style.position = Position.Absolute;
            _contentLayer.style.left = 0f;
            _contentLayer.style.top = 0f;
            _contentLayer.style.right = 0f;
            _contentLayer.style.bottom = 0f;
            Add(_contentLayer);

            _edgeLayer = new EdgeLayerElement(_boardAsset);
            _edgeLayer.PreviewNodePositions = _previewNodePositions;
            _edgeLayer.PreviewNodeSizes = _previewNodeSizes;
            _edgeLayer.style.position = Position.Absolute;
            _edgeLayer.style.left = 0f;
            _edgeLayer.style.top = 0f;
            _edgeLayer.style.right = 0f;
            _edgeLayer.style.bottom = 0f;
            _contentLayer.Add(_edgeLayer);

            _nodeLayer = new VisualElement();
            _nodeLayer.style.position = Position.Absolute;
            _nodeLayer.style.left = 0f;
            _nodeLayer.style.top = 0f;
            _nodeLayer.style.right = 0f;
            _nodeLayer.style.bottom = 0f;
            _contentLayer.Add(_nodeLayer);

            _guideLayer = new AlignmentGuideLayerElement();
            _guideLayer.AddToClassList("pd-alignment-guide-layer");
            _guideLayer.style.position = Position.Absolute;
            _guideLayer.style.left = 0f;
            _guideLayer.style.top = 0f;
            _guideLayer.style.right = 0f;
            _guideLayer.style.bottom = 0f;
            _contentLayer.Add(_guideLayer);

            _hintLabel = new Label(DefaultHintText);
            _hintLabel.AddToClassList("pd-canvas-hint");
            _hintLabel.pickingMode = PickingMode.Ignore;
            Add(_hintLabel);

            _marqueeElement = new VisualElement();
            _marqueeElement.AddToClassList("pd-marquee");
            _marqueeElement.pickingMode = PickingMode.Ignore;
            _marqueeElement.style.display = DisplayStyle.None;
            Add(_marqueeElement);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            RegisterCallback<WheelEvent>(OnWheel);
            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        public void Refresh()
        {
            HideConnectionMenu();
            CancelConnectionPreview();
            CancelMarqueeSelection();
            ClearAlignmentGuides();
            UpdateTransform();
            RebuildNodes();
            _edgeLayer.MarkDirtyRepaint();
            MarkDirtyRepaint();
        }

        public void RefreshSelection()
        {
            HashSet<string> selectedNodeIds = new HashSet<string>(_boardAsset.Document.ViewState.SelectedNodeIds);
            foreach (BoardNodeView nodeView in _nodeLayer.Children().OfType<BoardNodeView>())
            {
                nodeView.SetSelected(selectedNodeIds.Contains(nodeView.NodeId));
            }

            _edgeLayer.SelectedNodeIds = selectedNodeIds;
            UpdateConnectionHighlights();
            BringSelectionToFront();
            _edgeLayer.MarkDirtyRepaint();
            MarkDirtyRepaint();
        }

        public void SetHoveredEdge(string edgeId)
        {
            _edgeLayer.HoveredEdgeId = edgeId ?? string.Empty;
            _edgeLayer.MarkDirtyRepaint();
        }

        public Vector2 GetViewportCenterOnBoard()
        {
            if (layout.width < 10f || layout.height < 10f)
            {
                return new Vector2(220f, 180f);
            }

            return CanvasToBoard(new Vector2(layout.width * 0.35f, layout.height * 0.35f));
        }

        public void FrameAll()
        {
            List<BoardNodeModel> nodes = _boardAsset.Document.Nodes.Where(node => node != null).ToList();
            FrameNodes(nodes);
        }

        public void FrameSelection()
        {
            List<BoardNodeModel> nodes = _boardAsset.Document.ViewState.SelectedNodeIds
                .Select(_boardAsset.Document.GetNode)
                .Where(node => node != null)
                .ToList();
            FrameNodes(nodes);
        }

        public void FocusSelection()
        {
            List<BoardNodeModel> nodes = _boardAsset.Document.ViewState.SelectedNodeIds
                .Select(_boardAsset.Document.GetNode)
                .Where(node => node != null)
                .ToList();
            FocusNodes(nodes);
        }

        internal Vector2 GetBoardPositionFromCanvas(Vector2 canvasPosition)
        {
            return CanvasToBoard(canvasPosition);
        }

        internal string GetNodeIdFromTarget(object target)
        {
            VisualElement element = target as VisualElement;
            while (element != null)
            {
                BoardNodeView nodeView = element as BoardNodeView;
                if (nodeView != null)
                {
                    return nodeView.NodeId;
                }

                element = element.parent;
            }

            return string.Empty;
        }

        private void FrameNodes(List<BoardNodeModel> nodes)
        {
            if (nodes.Count == 0 || layout.width <= 0f || layout.height <= 0f)
            {
                return;
            }

            float minX = nodes.Min(node => node.Position.x);
            float minY = nodes.Min(node => node.Position.y);
            float maxX = nodes.Max(node => node.Position.x + node.Size.x);
            float maxY = nodes.Max(node => node.Position.y + node.Size.y);

            Rect bounds = Rect.MinMaxRect(minX, minY, maxX, maxY);
            float availableWidth = Mathf.Max(1f, layout.width - 140f);
            float availableHeight = Mathf.Max(1f, layout.height - 140f);
            float zoom = Mathf.Min(availableWidth / Mathf.Max(1f, bounds.width), availableHeight / Mathf.Max(1f, bounds.height));
            zoom = Mathf.Clamp(zoom, 0.35f, 2.5f);

            _boardAsset.Document.ViewState.Zoom = zoom;
            _boardAsset.Document.ViewState.PanOffset = new Vector2(
                (layout.width - bounds.width * zoom) * 0.5f - bounds.x * zoom,
                (layout.height - bounds.height * zoom) * 0.5f - bounds.y * zoom);

            ProjectDesignerBoardUtility.MarkDirty(_boardAsset);
            Refresh();
        }

        private void FocusNodes(List<BoardNodeModel> nodes)
        {
            if (nodes == null || nodes.Count == 0 || layout.width <= 0f || layout.height <= 0f)
            {
                return;
            }

            Rect bounds = BoardLayoutUtility.GetBounds(nodes);
            float zoom = Mathf.Clamp(_boardAsset.Document.ViewState.Zoom, 0.35f, 2.5f);
            if (nodes.Count > 1)
            {
                float availableWidth = Mathf.Max(1f, layout.width - 140f);
                float availableHeight = Mathf.Max(1f, layout.height - 140f);
                float fitZoom = Mathf.Min(availableWidth / Mathf.Max(1f, bounds.width), availableHeight / Mathf.Max(1f, bounds.height));
                zoom = Mathf.Clamp(Mathf.Min(zoom, fitZoom), 0.35f, 2.5f);
            }

            _boardAsset.Document.ViewState.Zoom = zoom;
            _boardAsset.Document.ViewState.PanOffset = new Vector2(
                layout.width * 0.5f - bounds.center.x * zoom,
                layout.height * 0.5f - bounds.center.y * zoom);

            ProjectDesignerBoardUtility.MarkDirty(_boardAsset);
            Refresh();
        }

        private void RebuildNodes()
        {
            _nodeLayer.Clear();
            _nodeViews.Clear();
            _previewNodePositions.Clear();
            _previewNodeSizes.Clear();
            HashSet<string> visibleIds = new HashSet<string>(BoardInsights.GetVisibleNodes(_boardAsset.Document).Select(node => node.Id));
            _visibleNodeIds = visibleIds;
            _edgeLayer.VisibleNodeIds = visibleIds;
            HashSet<string> selectedNodeIds = new HashSet<string>(_boardAsset.Document.ViewState.SelectedNodeIds);
            _edgeLayer.SelectedNodeIds = selectedNodeIds;
            _edgeLayer.HoveredNodeId = visibleIds.Contains(_hoveredNodeId) ? _hoveredNodeId : string.Empty;

            foreach (BoardNodeModel node in _boardAsset.Document.Nodes.OrderBy(node => node != null && selectedNodeIds.Contains(node.Id) ? 1 : 0))
            {
                if (node == null || !visibleIds.Contains(node.Id))
                {
                    continue;
                }

                IProjectDesignerNodeDefinition definition = ProjectDesignerRegistry.GetNodeDefinition(node.TypeId);
                var nodeView = new BoardNodeView(node, definition, _boardAsset.Document);
                nodeView.Refresh(_boardAsset.Document, selectedNodeIds.Contains(node.Id));
                nodeView.Selected += OnNodeSelected;
                nodeView.DragStarted += OnNodeDragStarted;
                nodeView.ConnectionStarted += OnNodeConnectionStarted;
                nodeView.ResizeStarted += OnNodeResizeStarted;
                nodeView.HoverChanged += OnNodeHoverChanged;
                _nodeLayer.Add(nodeView);
                _nodeViews[node.Id] = nodeView;
            }

            UpdateConnectionHighlights();
        }

        private void OnNodeSelected(BoardNodeSelectionRequest request)
        {
            HideConnectionMenu();
            ApplyNodeSelection(request);
            BringSelectionToFront();
            if (SelectionChanged != null)
            {
                SelectionChanged.Invoke();
            }
        }

        private void OnNodeDragStarted(string nodeId, Vector2 mousePosition, int pointerId)
        {
            if (IsDraggingNodes || !string.IsNullOrEmpty(_connectingNodeId) || !string.IsNullOrEmpty(_resizingNodeId))
            {
                return;
            }

            HideConnectionMenu();

            BoardNodeModel node = _boardAsset.Document.GetNode(nodeId);
            if (node == null)
            {
                return;
            }

            _dragPointerId = pointerId;
            _dragStartMousePosition = PanelToCanvas(mousePosition);
            _dragStartNodePositions.Clear();
            _draggingNodeIds.Clear();

            List<string> selectedIds = _boardAsset.Document.ViewState.SelectedNodeIds.ToList();
            if (selectedIds.Count > 1 && selectedIds.Contains(nodeId))
            {
                foreach (string selectedId in selectedIds)
                {
                    BoardNodeModel selectedNode = _boardAsset.Document.GetNode(selectedId);
                    if (selectedNode == null)
                    {
                        continue;
                    }

                    _draggingNodeIds.Add(selectedId);
                    _dragStartNodePositions[selectedId] = selectedNode.Position;
                }
            }
            else
            {
                _draggingNodeIds.Add(nodeId);
                _dragStartNodePositions[nodeId] = node.Position;
            }

            PointerCaptureHelper.CapturePointer(this, pointerId);
            BringSelectionToFront();
            Focus();
        }

        private void OnNodeConnectionStarted(string nodeId, Vector2 mousePosition, int pointerId)
        {
            if (IsDraggingNodes || !string.IsNullOrEmpty(_resizingNodeId))
            {
                return;
            }

            HideConnectionMenu();

            BoardNodeModel node = _boardAsset.Document.GetNode(nodeId);
            if (node == null)
            {
                return;
            }

            _connectionTargetOptions = ProjectDesignerLinkUtility.GetLinkOptionsByTarget(_boardAsset.Document, node);
            if (_connectionTargetOptions.Count == 0)
            {
                _hintLabel.text = "No valid link targets are available for this card right now.";
                ShowConnectionMessage(
                    PanelToCanvas(mousePosition),
                    "No valid link targets",
                    "Add another compatible card or use the Details panel to review existing links.");
                return;
            }

            _connectingNodeId = nodeId;
            _connectPointerId = pointerId;
            _connectStartCanvasPosition = PanelToCanvas(mousePosition);
            _connectPreviewPosition = PanelToBoard(mousePosition);
            _hoveredConnectionTargetId = string.Empty;
            _edgeLayer.PreviewConnection = new ConnectionPreviewData
            {
                SourceNodeId = nodeId,
                EndPosition = _connectPreviewPosition,
                AccentColor = GetConnectionPreviewColor(string.Empty)
            };

            PointerCaptureHelper.CapturePointer(this, pointerId);
            BringNodeToFront(nodeId);
            UpdateHoveredConnectionTarget(_connectPreviewPosition);
            UpdateConnectionHighlights();
            UpdateHintLabel();
            _edgeLayer.MarkDirtyRepaint();
            MarkDirtyRepaint();
            Focus();
        }

        private void OnNodeResizeStarted(string nodeId, Vector2 mousePosition, int pointerId)
        {
            if (IsDraggingNodes || !string.IsNullOrEmpty(_connectingNodeId) || !string.IsNullOrEmpty(_resizingNodeId))
            {
                return;
            }

            HideConnectionMenu();

            BoardNodeModel node = _boardAsset.Document.GetNode(nodeId);
            if (node == null)
            {
                return;
            }

            _resizingNodeId = nodeId;
            _resizePointerId = pointerId;
            _resizeStartMousePosition = PanelToCanvas(mousePosition);
            _resizeStartSize = node.Size;
            PointerCaptureHelper.CapturePointer(this, pointerId);
            BringNodeToFront(nodeId);
            Focus();
        }

        private void OnNodeHoverChanged(string nodeId, bool isHovered)
        {
            if (isHovered)
            {
                _hoveredNodeId = nodeId ?? string.Empty;
            }
            else if (string.Equals(_hoveredNodeId, nodeId, StringComparison.Ordinal))
            {
                _hoveredNodeId = string.Empty;
            }

            _edgeLayer.HoveredNodeId = _hoveredNodeId;
            _edgeLayer.MarkDirtyRepaint();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (IsConnectionMenuTarget(evt.target))
            {
                return;
            }

            HideConnectionMenu();

            if (IsDraggingNodes || !string.IsNullOrEmpty(_connectingNodeId) || !string.IsNullOrEmpty(_resizingNodeId) || evt.button != 0 || !IsEmptyTarget(evt.target))
            {
                return;
            }

            if (evt.shiftKey)
            {
                _marqueeSelecting = true;
                _marqueePointerId = evt.pointerId;
                _marqueeStartCanvasPosition = PanelToCanvas(GetEventPosition(evt.position));
                _marqueeCurrentCanvasPosition = _marqueeStartCanvasPosition;
                _marqueeBaseSelection = _boardAsset.Document.ViewState.SelectedNodeIds.ToList();
                UpdateMarqueeElement();
                PointerCaptureHelper.CapturePointer(this, evt.pointerId);
                Focus();
                evt.StopPropagation();
                return;
            }

            _panning = true;
            _panStartMousePosition = PanelToCanvas(GetEventPosition(evt.position));
            _panStartOffset = _boardAsset.Document.ViewState.PanOffset;
            PointerCaptureHelper.CapturePointer(this, evt.pointerId);
            ClearSelection();

            Focus();
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!string.IsNullOrEmpty(_connectingNodeId))
            {
                if (evt.pointerId != _connectPointerId)
                {
                    return;
                }

                Vector2 boardPosition = PanelToBoard(GetEventPosition(evt.position));
                _connectPreviewPosition = boardPosition;
                UpdateHoveredConnectionTarget(boardPosition);
                UpdateConnectionPreview();
                UpdateConnectionHighlights();
                UpdateHintLabel();
                _edgeLayer.MarkDirtyRepaint();
                MarkDirtyRepaint();
                evt.StopPropagation();
                return;
            }

            if (!string.IsNullOrEmpty(_resizingNodeId))
            {
                if (evt.pointerId != _resizePointerId)
                {
                    return;
                }

                float zoom = Mathf.Max(0.01f, _boardAsset.Document.ViewState.Zoom);
                Vector2 resizeDelta = (PanelToCanvas(GetEventPosition(evt.position)) - _resizeStartMousePosition) / zoom;
                Vector2 previewSize = _resizeStartSize + resizeDelta;
                if (_boardAsset.Document.ViewState.SnapToGrid)
                {
                    previewSize = SnapSize(previewSize);
                }

                BoardNodeModel node = _boardAsset.Document.GetNode(_resizingNodeId);
                if (node != null)
                {
                    previewSize = BoardNodeModel.ClampSize(previewSize);
                    _previewNodeSizes[_resizingNodeId] = previewSize;
                    if (_nodeViews.TryGetValue(_resizingNodeId, out BoardNodeView nodeView))
                    {
                        nodeView.SetPreviewSize(previewSize);
                    }

                    UpdateAlignmentGuidesForResize(node, previewSize);
                }

                _edgeLayer.MarkDirtyRepaint();
                MarkDirtyRepaint();
                evt.StopPropagation();
                return;
            }

            if (IsDraggingNodes)
            {
                if (evt.pointerId != _dragPointerId)
                {
                    return;
                }

                float zoom = Mathf.Max(0.01f, _boardAsset.Document.ViewState.Zoom);
                Vector2 deltaX = (PanelToCanvas(GetEventPosition(evt.position)) - _dragStartMousePosition) / zoom;
                foreach (KeyValuePair<string, Vector2> pair in _dragStartNodePositions)
                {
                    Vector2 previewPosition = pair.Value + deltaX;
                    if (_boardAsset.Document.ViewState.SnapToGrid)
                    {
                        previewPosition = BoardLayoutUtility.SnapPosition(previewPosition);
                    }

                    _previewNodePositions[pair.Key] = previewPosition;
                    if (_nodeViews.TryGetValue(pair.Key, out BoardNodeView nodeView))
                    {
                        nodeView.SetPreviewPosition(previewPosition);
                    }
                }

                UpdateAlignmentGuidesForDrag();
                _edgeLayer.MarkDirtyRepaint();
                MarkDirtyRepaint();
                evt.StopPropagation();
                return;
            }

            if (_marqueeSelecting)
            {
                if (evt.pointerId != _marqueePointerId)
                {
                    return;
                }

                _marqueeCurrentCanvasPosition = PanelToCanvas(GetEventPosition(evt.position));
                UpdateMarqueeElement();
                ApplyMarqueeSelection();
                evt.StopPropagation();
                return;
            }

            if (!_panning || !PointerCaptureHelper.HasPointerCapture(this, evt.pointerId))
            {
                return;
            }

            Vector2 delta = PanelToCanvas(GetEventPosition(evt.position)) - _panStartMousePosition;
            _boardAsset.Document.ViewState.PanOffset = _panStartOffset + delta;
            ProjectDesignerBoardUtility.MarkDirty(_boardAsset);
            UpdateTransform();
            MarkDirtyRepaint();
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!string.IsNullOrEmpty(_connectingNodeId))
            {
                if (evt.pointerId != _connectPointerId)
                {
                    return;
                }

                int connectPointerId = _connectPointerId;
                Vector2 panelPosition = GetEventPosition(evt.position);
                Vector2 boardPosition = PanelToBoard(panelPosition);
                CompleteConnectionDrag(boardPosition, PanelToCanvas(panelPosition));
                if (connectPointerId >= 0 && PointerCaptureHelper.HasPointerCapture(this, connectPointerId))
                {
                    PointerCaptureHelper.ReleasePointer(this, connectPointerId);
                }
                evt.StopPropagation();
                return;
            }

            if (!string.IsNullOrEmpty(_resizingNodeId))
            {
                if (evt.pointerId != _resizePointerId)
                {
                    return;
                }

                int resizePointerId = _resizePointerId;
                CompleteNodeResize();
                if (resizePointerId >= 0 && PointerCaptureHelper.HasPointerCapture(this, resizePointerId))
                {
                    PointerCaptureHelper.ReleasePointer(this, resizePointerId);
                }
                evt.StopPropagation();
                return;
            }

            if (IsDraggingNodes)
            {
                if (evt.pointerId != _dragPointerId)
                {
                    return;
                }

                int dragPointerId = _dragPointerId;
                CompleteNodeDrag();
                if (dragPointerId >= 0 && PointerCaptureHelper.HasPointerCapture(this, dragPointerId))
                {
                    PointerCaptureHelper.ReleasePointer(this, dragPointerId);
                }
                evt.StopPropagation();
                return;
            }

            if (_marqueeSelecting)
            {
                if (evt.pointerId != _marqueePointerId)
                {
                    return;
                }

                CompleteMarqueeSelection();
                if (_marqueePointerId >= 0 && PointerCaptureHelper.HasPointerCapture(this, _marqueePointerId))
                {
                    PointerCaptureHelper.ReleasePointer(this, _marqueePointerId);
                }
                evt.StopPropagation();
                return;
            }

            if (!_panning || !PointerCaptureHelper.HasPointerCapture(this, evt.pointerId))
            {
                return;
            }

            PointerCaptureHelper.ReleasePointer(this, evt.pointerId);
            _panning = false;
            evt.StopPropagation();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            CancelNodeDragPreview();
            CancelConnectionPreview();
            CancelNodeResizePreview();
            CancelMarqueeSelection();
            _panning = false;
        }

        private void OnWheel(WheelEvent evt)
        {
            HideConnectionMenu();

            Vector2 boardPositionBefore = CanvasToBoard(evt.localMousePosition);
            float oldZoom = _boardAsset.Document.ViewState.Zoom;
            float newZoom = Mathf.Clamp(oldZoom * (evt.delta.y > 0f ? 0.92f : 1.08f), 0.35f, 2.5f);
            _boardAsset.Document.ViewState.Zoom = newZoom;
            _boardAsset.Document.ViewState.PanOffset = evt.localMousePosition - boardPositionBefore * newZoom;
            ProjectDesignerBoardUtility.MarkDirty(_boardAsset);
            Refresh();
            evt.StopPropagation();
        }

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0)
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.StopPropagation();
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0)
            {
                return;
            }

            HideConnectionMenu();
            DragAndDrop.AcceptDrag();
            Vector2 startPosition = CanvasToBoard(evt.localMousePosition);
            Vector2 offset = Vector2.zero;

            foreach (UnityEngine.Object asset in DragAndDrop.objectReferences)
            {
                IProjectDesignerAssetImporter importer = ProjectDesignerRegistry.GetAssetImporters()
                    .FirstOrDefault(candidate => candidate.CanImport(asset));
                if (importer == null)
                {
                    continue;
                }

                foreach (BoardNodeModel node in importer.Import(asset, startPosition + offset))
                {
                    _dispatcher.Execute(new CreateNodeCommand(_boardAsset, node));
                    offset += new Vector2(36f, 28f);
                }
            }

            evt.StopPropagation();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Escape)
            {
                return;
            }

            bool hadConnectionState = !string.IsNullOrEmpty(_connectingNodeId) || _connectionMenu != null;
            bool hadResizeState = !string.IsNullOrEmpty(_resizingNodeId);
            HideConnectionMenu();
            CancelConnectionPreview();
            CancelNodeResizePreview();
            if (_marqueeSelecting)
            {
                CancelMarqueeSelection();
            }

            if (!hadConnectionState && !hadResizeState && _boardAsset.Document.ViewState.SelectedNodeIds.Count > 0)
            {
                ClearSelection();
                evt.StopPropagation();
                return;
            }

            if (hadConnectionState || hadResizeState)
            {
                evt.StopPropagation();
            }
        }

        private void UpdateTransform()
        {
            float zoom = _boardAsset.Document.ViewState.Zoom;
            Vector2 pan = _boardAsset.Document.ViewState.PanOffset;
            _contentLayer.transform.position = new Vector3(pan.x, pan.y, 0f);
            _contentLayer.transform.scale = new Vector3(zoom, zoom, 1f);
            _guideLayer.Zoom = zoom;
        }

        private Vector2 CanvasToBoard(Vector2 canvasPosition)
        {
            float zoom = Mathf.Max(0.01f, _boardAsset.Document.ViewState.Zoom);
            return (canvasPosition - _boardAsset.Document.ViewState.PanOffset) / zoom;
        }

        private Vector2 PanelToBoard(Vector2 panelPosition)
        {
            return CanvasToBoard(PanelToCanvas(panelPosition));
        }

        private Vector2 PanelToCanvas(Vector2 panelPosition)
        {
            return this.WorldToLocal(panelPosition);
        }

        private void OnGenerateGrid(MeshGenerationContext context)
        {
            var painter = context.painter2D;
            painter.lineWidth = 1f;
            painter.strokeColor = new Color(0.2f, 0.28f, 0.36f, 0.08f);

            float step = 120f * _boardAsset.Document.ViewState.Zoom;
            if (step <= 0f)
            {
                return;
            }

            Vector2 pan = _boardAsset.Document.ViewState.PanOffset;
            float offsetX = Mathf.Repeat(pan.x, step);
            float offsetY = Mathf.Repeat(pan.y, step);

            for (float x = offsetX; x < layout.width; x += step)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(x, 0f));
                painter.LineTo(new Vector2(x, layout.height));
                painter.Stroke();
            }

            for (float y = offsetY; y < layout.height; y += step)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(0f, y));
                painter.LineTo(new Vector2(layout.width, y));
                painter.Stroke();
            }
        }

        private void UpdateHoveredConnectionTarget(Vector2 boardPosition)
        {
            string hoveredTargetId = GetNodeIdAtBoardPosition(boardPosition, _connectingNodeId);
            if (!string.IsNullOrEmpty(hoveredTargetId) && !_connectionTargetOptions.ContainsKey(hoveredTargetId))
            {
                hoveredTargetId = string.Empty;
            }

            _hoveredConnectionTargetId = hoveredTargetId;
        }

        private void UpdateConnectionPreview()
        {
            if (string.IsNullOrEmpty(_connectingNodeId))
            {
                _edgeLayer.PreviewConnection = null;
                return;
            }

            _edgeLayer.PreviewConnection = new ConnectionPreviewData
            {
                SourceNodeId = _connectingNodeId,
                EndPosition = _connectPreviewPosition,
                AccentColor = GetConnectionPreviewColor(_hoveredConnectionTargetId)
            };
        }

        private void UpdateConnectionHighlights()
        {
            foreach (KeyValuePair<string, BoardNodeView> pair in _nodeViews)
            {
                bool isOrigin = pair.Key == _connectingNodeId;
                bool isValidTarget = !isOrigin && _connectionTargetOptions.ContainsKey(pair.Key);
                bool isHoveredTarget = isValidTarget && pair.Key == _hoveredConnectionTargetId;
                pair.Value.SetConnectionState(isOrigin, isValidTarget, isHoveredTarget);
            }
        }

        private void UpdateHintLabel()
        {
            if (_connectionMenu != null)
            {
                _hintLabel.text = "Choose a link type to finish this connection, or click empty space to cancel.";
                return;
            }

            if (!string.IsNullOrEmpty(_connectingNodeId))
            {
                if (!string.IsNullOrEmpty(_hoveredConnectionTargetId) &&
                    _connectionTargetOptions.TryGetValue(_hoveredConnectionTargetId, out List<ProjectDesignerLinkOption> options))
                {
                    _hintLabel.text = options.Count == 1
                        ? "Release to create a " + options[0].Definition.DisplayName + "."
                        : "Release to choose from " + options.Count + " valid link types.";
                    return;
                }

                _hintLabel.text = "Drag to a highlighted card. Release on empty space to cancel.";
                return;
            }

            _hintLabel.text = DefaultHintText;
        }

        private void CompleteConnectionDrag(Vector2 boardPosition, Vector2 localMousePosition)
        {
            string connectingNodeId = _connectingNodeId;
            string hoveredTargetId = _hoveredConnectionTargetId;
            Dictionary<string, List<ProjectDesignerLinkOption>> optionsByTarget = _connectionTargetOptions;

            BoardNodeModel sourceNode = _boardAsset.Document.GetNode(connectingNodeId);
            bool isClick = (localMousePosition - _connectStartCanvasPosition).sqrMagnitude <= LinkClickScreenThreshold * LinkClickScreenThreshold;
            string targetNodeId = !string.IsNullOrEmpty(hoveredTargetId)
                ? hoveredTargetId
                : GetNodeIdAtBoardPosition(boardPosition, connectingNodeId);

            CancelConnectionPreview();

            if (sourceNode == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(targetNodeId))
            {
                if (isClick)
                {
                    ShowConnectionTargetMenu(localMousePosition, sourceNode, optionsByTarget);
                }

                return;
            }

            BoardNodeModel targetNode = _boardAsset.Document.GetNode(targetNodeId);
            if (targetNode == null)
            {
                return;
            }

            if (!optionsByTarget.TryGetValue(targetNodeId, out List<ProjectDesignerLinkOption> options) || options.Count == 0)
            {
                return;
            }

            if (options.Count == 1)
            {
                CreateEdgeFromOption(options[0], sourceNode.Id);
                return;
            }

            ShowConnectionMenu(localMousePosition, sourceNode, targetNode, options);
        }

        private void ShowConnectionMenu(Vector2 localMousePosition, BoardNodeModel sourceNode, BoardNodeModel targetNode, List<ProjectDesignerLinkOption> options)
        {
            HideConnectionMenu();

            var menu = new VisualElement();
            menu.AddToClassList("pd-inline-link-menu");

            float left = Mathf.Clamp(localMousePosition.x + 12f, 12f, Mathf.Max(12f, layout.width - 280f));
            float top = Mathf.Clamp(localMousePosition.y + 12f, 12f, Mathf.Max(12f, layout.height - 260f));
            menu.style.left = left;
            menu.style.top = top;

            var title = new Label("Create link");
            title.AddToClassList("pd-inline-link-menu-title");
            menu.Add(title);

            var body = new Label(sourceNode.Title + " <-> " + targetNode.Title);
            body.AddToClassList("pd-inline-link-menu-body");
            menu.Add(body);

            foreach (ProjectDesignerLinkOption option in options)
            {
                ProjectDesignerLinkOption localOption = option;
                var button = new Button(() =>
                {
                    HideConnectionMenu();
                    CreateEdgeFromOption(localOption, sourceNode.Id);
                })
                {
                    text = ProjectDesignerLinkUtility.GetInlineActionLabel(localOption)
                };
                button.AddToClassList("pd-secondary-button");
                menu.Add(button);
            }

            var cancelButton = new Button(() => HideConnectionMenu())
            {
                text = "Cancel"
            };
            cancelButton.AddToClassList("pd-secondary-button");
            menu.Add(cancelButton);

            _connectionMenu = menu;
            Add(_connectionMenu);
            _connectionMenu.BringToFront();
            UpdateHintLabel();
        }

        private void ShowConnectionTargetMenu(Vector2 localMousePosition, BoardNodeModel sourceNode, Dictionary<string, List<ProjectDesignerLinkOption>> optionsByTarget)
        {
            List<ProjectDesignerLinkOption> options = (optionsByTarget ?? new Dictionary<string, List<ProjectDesignerLinkOption>>())
                .SelectMany(pair => pair.Value ?? new List<ProjectDesignerLinkOption>())
                .Where(option => option != null && option.Definition != null && option.OtherNode != null)
                .OrderBy(option => option.DisplayLabel)
                .ThenBy(option => option.Definition.DisplayName)
                .ToList();
            if (options.Count == 0)
            {
                return;
            }

            HideConnectionMenu();

            var menu = new VisualElement();
            menu.AddToClassList("pd-inline-link-menu");

            float left = Mathf.Clamp(localMousePosition.x + 12f, 12f, Mathf.Max(12f, layout.width - 320f));
            float top = Mathf.Clamp(localMousePosition.y + 12f, 12f, Mathf.Max(12f, layout.height - 300f));
            menu.style.left = left;
            menu.style.top = top;

            var title = new Label("Create link");
            title.AddToClassList("pd-inline-link-menu-title");
            menu.Add(title);

            var body = new Label("Choose a target for " + sourceNode.Title);
            body.AddToClassList("pd-inline-link-menu-body");
            menu.Add(body);

            foreach (ProjectDesignerLinkOption option in options)
            {
                ProjectDesignerLinkOption localOption = option;
                var button = new Button(() =>
                {
                    HideConnectionMenu();
                    CreateEdgeFromOption(localOption, sourceNode.Id);
                })
                {
                    text = option.DisplayLabel + " [" + ProjectDesignerLinkUtility.GetInlineActionLabel(option) + "]"
                };
                button.AddToClassList("pd-secondary-button");
                button.AddToClassList("pd-link-menu-button");
                menu.Add(button);
            }

            var cancelButton = new Button(() => HideConnectionMenu())
            {
                text = "Cancel"
            };
            cancelButton.AddToClassList("pd-secondary-button");
            menu.Add(cancelButton);

            _connectionMenu = menu;
            Add(_connectionMenu);
            _connectionMenu.BringToFront();
            UpdateHintLabel();
        }

        private void ShowConnectionMessage(Vector2 localMousePosition, string titleText, string bodyText)
        {
            HideConnectionMenu();

            var menu = new VisualElement();
            menu.AddToClassList("pd-inline-link-menu");

            float left = Mathf.Clamp(localMousePosition.x + 12f, 12f, Mathf.Max(12f, layout.width - 300f));
            float top = Mathf.Clamp(localMousePosition.y + 12f, 12f, Mathf.Max(12f, layout.height - 220f));
            menu.style.left = left;
            menu.style.top = top;

            var title = new Label(titleText);
            title.AddToClassList("pd-inline-link-menu-title");
            menu.Add(title);

            var body = new Label(bodyText);
            body.AddToClassList("pd-inline-link-menu-body");
            menu.Add(body);

            var closeButton = new Button(() => HideConnectionMenu())
            {
                text = "OK"
            };
            closeButton.AddToClassList("pd-secondary-button");
            menu.Add(closeButton);

            _connectionMenu = menu;
            Add(_connectionMenu);
            _connectionMenu.BringToFront();
            _hintLabel.text = bodyText;
        }

        private void HideConnectionMenu()
        {
            if (_connectionMenu == null)
            {
                return;
            }

            if (_connectionMenu.parent != null)
            {
                _connectionMenu.parent.Remove(_connectionMenu);
            }

            _connectionMenu = null;
            UpdateHintLabel();
        }

        private void CreateEdgeFromOption(ProjectDesignerLinkOption option, string selectedNodeId)
        {
            if (option == null || option.Definition == null || option.OtherNode == null || string.IsNullOrEmpty(selectedNodeId))
            {
                return;
            }

            BoardNodeModel selectedNode = _boardAsset.Document.GetNode(selectedNodeId);
            if (selectedNode == null)
            {
                return;
            }

            string sourceId = option.SelectedNodeIsSource ? selectedNode.Id : option.OtherNode.Id;
            string targetId = option.SelectedNodeIsSource ? option.OtherNode.Id : selectedNode.Id;
            var edge = new BoardEdgeModel(option.Definition.TypeId, sourceId, targetId);
            edge.Label = option.Definition.GetLabel(edge, _boardAsset.Document);
            _dispatcher.Execute(new CreateEdgeCommand(_boardAsset, edge));
        }

        private bool IsEmptyTarget(object target)
        {
            return ReferenceEquals(target, this) ||
                   ReferenceEquals(target, _contentLayer) ||
                   ReferenceEquals(target, _edgeLayer) ||
                   ReferenceEquals(target, _nodeLayer);
        }

        private bool IsConnectionMenuTarget(object target)
        {
            if (_connectionMenu == null)
            {
                return false;
            }

            VisualElement element = target as VisualElement;
            while (element != null)
            {
                if (ReferenceEquals(element, _connectionMenu))
                {
                    return true;
                }

                element = element.parent;
            }

            return false;
        }

        private string GetNodeIdAtBoardPosition(Vector2 boardPosition, string excludedNodeId)
        {
            for (int index = _nodeLayer.childCount - 1; index >= 0; index--)
            {
                BoardNodeView nodeView = _nodeLayer.hierarchy.ElementAt(index) as BoardNodeView;
                if (nodeView == null || nodeView.NodeId == excludedNodeId)
                {
                    continue;
                }

                BoardNodeModel node = _boardAsset.Document.GetNode(nodeView.NodeId);
                if (node == null)
                {
                    continue;
                }

                Rect nodeRect = new Rect(node.Position, node.Size);
                if (nodeRect.Contains(boardPosition))
                {
                    return node.Id;
                }
            }

            return string.Empty;
        }

        private Color GetConnectionPreviewColor(string hoveredTargetId)
        {
            Color fallback = new Color(0.3f, 0.48f, 1f, 0.95f);
            if (string.IsNullOrEmpty(hoveredTargetId) ||
                !_connectionTargetOptions.TryGetValue(hoveredTargetId, out List<ProjectDesignerLinkOption> options) ||
                options.Count == 0 ||
                options[0].Definition == null)
            {
                return fallback;
            }

            return ParseColor(options[0].Definition.AccentColor, fallback);
        }

        private static Color ParseColor(string htmlColor, Color fallback)
        {
            Color parsed;
            return ColorUtility.TryParseHtmlString(htmlColor, out parsed) ? parsed : fallback;
        }

        private static Vector2 GetEventPosition(Vector3 position)
        {
            return new Vector2(position.x, position.y);
        }

        private void UpdateAlignmentGuidesForDrag()
        {
            Rect activeBounds = GetPreviewBounds(_draggingNodeIds);
            if (activeBounds.width <= 0f || activeBounds.height <= 0f)
            {
                ClearAlignmentGuides();
                return;
            }

            UpdateAlignmentGuides(activeBounds, new HashSet<string>(_draggingNodeIds));
        }

        private void UpdateAlignmentGuidesForResize(BoardNodeModel node, Vector2 previewSize)
        {
            if (node == null)
            {
                ClearAlignmentGuides();
                return;
            }

            Rect activeRect = new Rect(node.Position, previewSize);
            UpdateAlignmentGuides(activeRect, new HashSet<string> { node.Id });
        }

        private void UpdateAlignmentGuides(Rect activeRect, HashSet<string> excludedNodeIds)
        {
            if (!ProjectDesignerSettings.instance.ShowAlignmentGuides)
            {
                ClearAlignmentGuides();
                return;
            }

            float zoom = Mathf.Max(0.01f, _boardAsset.Document.ViewState.Zoom);
            List<Rect> referenceRects = GetAlignmentReferenceRects(excludedNodeIds);
            _guideLayer.Zoom = zoom;
            _guideLayer.SetGuides(BoardLayoutUtility.BuildAlignmentGuides(activeRect, referenceRects, AlignmentGuideBoardTolerance));
        }

        private List<Rect> GetAlignmentReferenceRects(HashSet<string> excludedNodeIds)
        {
            var rects = new List<Rect>();
            foreach (BoardNodeModel node in _boardAsset.Document.Nodes)
            {
                if (node == null ||
                    (excludedNodeIds != null && excludedNodeIds.Contains(node.Id)) ||
                    (_visibleNodeIds.Count > 0 && !_visibleNodeIds.Contains(node.Id)))
                {
                    continue;
                }

                rects.Add(GetNodePreviewRect(node));
            }

            return rects;
        }

        private Rect GetPreviewBounds(IEnumerable<string> nodeIds)
        {
            List<Rect> rects = (nodeIds ?? Enumerable.Empty<string>())
                .Select(_boardAsset.Document.GetNode)
                .Where(node => node != null)
                .Select(GetNodePreviewRect)
                .ToList();

            if (rects.Count == 0)
            {
                return new Rect(0f, 0f, 0f, 0f);
            }

            return GetBounds(rects);
        }

        private Rect GetNodePreviewRect(BoardNodeModel node)
        {
            Vector2 position = _previewNodePositions.TryGetValue(node.Id, out Vector2 previewPosition)
                ? previewPosition
                : node.Position;
            Vector2 size = _previewNodeSizes.TryGetValue(node.Id, out Vector2 previewSize)
                ? previewSize
                : node.Size;
            return new Rect(position, size);
        }

        private static Rect GetBounds(IReadOnlyList<Rect> rects)
        {
            float minX = rects.Min(rect => rect.xMin);
            float minY = rects.Min(rect => rect.yMin);
            float maxX = rects.Max(rect => rect.xMax);
            float maxY = rects.Max(rect => rect.yMax);
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private void ClearAlignmentGuides()
        {
            if (_guideLayer == null || _guideLayer.Guides == null || _guideLayer.Guides.Count == 0)
            {
                return;
            }

            _guideLayer.SetGuides(new List<BoardAlignmentGuide>());
        }

        private bool IsDraggingNodes
        {
            get { return _draggingNodeIds.Count > 0; }
        }

        private static Vector2 SnapSize(Vector2 size)
        {
            float grid = Mathf.Max(1f, ProjectDesignerProductInfo.GridSize);
            return new Vector2(
                Mathf.Round(size.x / grid) * grid,
                Mathf.Round(size.y / grid) * grid);
        }

        private void BringNodeToFront(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                return;
            }

            if (_nodeViews.TryGetValue(nodeId, out BoardNodeView nodeView))
            {
                nodeView.BringToFront();
            }
        }

        private void BringSelectionToFront()
        {
            foreach (string nodeId in _boardAsset.Document.ViewState.SelectedNodeIds)
            {
                BringNodeToFront(nodeId);
            }
        }

        private void CompleteNodeDrag()
        {
            List<string> draggingNodeIds = _draggingNodeIds.ToList();
            _dragPointerId = -1;
            _draggingNodeIds.Clear();

            var newPositions = new Dictionary<string, Vector2>();
            foreach (string draggingNodeId in draggingNodeIds)
            {
                BoardNodeModel node = _boardAsset.Document.GetNode(draggingNodeId);
                if (node == null)
                {
                    _previewNodePositions.Remove(draggingNodeId);
                    continue;
                }

                Vector2 previewPosition = _previewNodePositions.TryGetValue(draggingNodeId, out Vector2 value)
                    ? value
                    : node.Position;
                _previewNodePositions.Remove(draggingNodeId);
                if ((previewPosition - node.Position).sqrMagnitude > 0.01f)
                {
                    newPositions[draggingNodeId] = previewPosition;
                }
                else if (_nodeViews.TryGetValue(draggingNodeId, out BoardNodeView nodeView))
                {
                    nodeView.SetPreviewPosition(node.Position);
                }
            }

            _dragStartNodePositions.Clear();
            ClearAlignmentGuides();
            if (newPositions.Count == 1)
            {
                KeyValuePair<string, Vector2> onlyMove = newPositions.First();
                _dispatcher.Execute(new MoveNodeCommand(_boardAsset, onlyMove.Key, onlyMove.Value));
                return;
            }

            if (newPositions.Count > 1)
            {
                _dispatcher.Execute(new MoveNodesCommand(_boardAsset, newPositions));
                return;
            }

            _edgeLayer.MarkDirtyRepaint();
            MarkDirtyRepaint();
        }

        private void CompleteNodeResize()
        {
            string resizingNodeId = _resizingNodeId;
            _resizingNodeId = string.Empty;
            _resizePointerId = -1;

            BoardNodeModel node = _boardAsset.Document.GetNode(resizingNodeId);
            Vector2 previewSize = node == null
                ? _resizeStartSize
                : (_previewNodeSizes.TryGetValue(resizingNodeId, out Vector2 value) ? value : node.Size);
            _previewNodeSizes.Remove(resizingNodeId);
            ClearAlignmentGuides();

            if (node == null)
            {
                return;
            }

            if ((previewSize - node.Size).sqrMagnitude > 0.01f)
            {
                _dispatcher.Execute(new ResizeNodeCommand(_boardAsset, resizingNodeId, previewSize));
                return;
            }

            if (_nodeViews.TryGetValue(resizingNodeId, out BoardNodeView nodeView))
            {
                nodeView.SetPreviewSize(node.Size);
            }

            _edgeLayer.MarkDirtyRepaint();
            MarkDirtyRepaint();
        }

        private void CancelNodeDragPreview()
        {
            if (!IsDraggingNodes)
            {
                return;
            }

            int dragPointerId = _dragPointerId;
            _dragPointerId = -1;
            foreach (string draggingNodeId in _draggingNodeIds.ToList())
            {
                _previewNodePositions.Remove(draggingNodeId);

                BoardNodeModel node = _boardAsset.Document.GetNode(draggingNodeId);
                if (node != null && _nodeViews.TryGetValue(draggingNodeId, out BoardNodeView nodeView))
                {
                    nodeView.SetPreviewPosition(node.Position);
                }
            }

            _draggingNodeIds.Clear();
            _dragStartNodePositions.Clear();
            ClearAlignmentGuides();

            if (dragPointerId >= 0 && PointerCaptureHelper.HasPointerCapture(this, dragPointerId))
            {
                PointerCaptureHelper.ReleasePointer(this, dragPointerId);
            }

            _edgeLayer.MarkDirtyRepaint();
            MarkDirtyRepaint();
        }

        private void CancelNodeResizePreview()
        {
            if (string.IsNullOrEmpty(_resizingNodeId))
            {
                return;
            }

            int resizePointerId = _resizePointerId;
            string resizingNodeId = _resizingNodeId;
            _resizingNodeId = string.Empty;
            _resizePointerId = -1;
            _previewNodeSizes.Remove(resizingNodeId);
            ClearAlignmentGuides();

            BoardNodeModel node = _boardAsset.Document.GetNode(resizingNodeId);
            if (node != null && _nodeViews.TryGetValue(resizingNodeId, out BoardNodeView nodeView))
            {
                nodeView.SetPreviewSize(node.Size);
            }

            if (resizePointerId >= 0 && PointerCaptureHelper.HasPointerCapture(this, resizePointerId))
            {
                PointerCaptureHelper.ReleasePointer(this, resizePointerId);
            }

            _edgeLayer.MarkDirtyRepaint();
            MarkDirtyRepaint();
        }

        private void CancelConnectionPreview()
        {
            if (string.IsNullOrEmpty(_connectingNodeId))
            {
                return;
            }

            int connectPointerId = _connectPointerId;
            _connectingNodeId = string.Empty;
            _connectPointerId = -1;
            _connectStartCanvasPosition = Vector2.zero;
            _hoveredConnectionTargetId = string.Empty;
            _connectionTargetOptions = new Dictionary<string, List<ProjectDesignerLinkOption>>();
            _edgeLayer.PreviewConnection = null;
            UpdateConnectionHighlights();

            if (connectPointerId >= 0 && PointerCaptureHelper.HasPointerCapture(this, connectPointerId))
            {
                PointerCaptureHelper.ReleasePointer(this, connectPointerId);
            }

            UpdateHintLabel();
            _edgeLayer.MarkDirtyRepaint();
            MarkDirtyRepaint();
        }

        private void ApplyNodeSelection(BoardNodeSelectionRequest request)
        {
            if (string.IsNullOrEmpty(request.NodeId))
            {
                return;
            }

            BoardViewState viewState = _boardAsset.Document.ViewState;
            if (request.Toggle)
            {
                viewState.ToggleSelection(request.NodeId);
                ProjectDesignerBoardUtility.MarkDirty(_boardAsset);
                return;
            }

            List<string> selection = viewState.SelectedNodeIds.ToList();
            if (selection.Count > 1 && selection.Contains(request.NodeId))
            {
                viewState.SetSelection(selection, request.NodeId);
                ProjectDesignerBoardUtility.MarkDirty(_boardAsset);
                return;
            }

            viewState.SelectSingle(request.NodeId);
            ProjectDesignerBoardUtility.MarkDirty(_boardAsset);
        }

        private void ClearSelection()
        {
            if (_boardAsset.Document.ViewState.SelectedNodeIds.Count == 0)
            {
                return;
            }

            _boardAsset.Document.ViewState.ClearSelection();
            _edgeLayer.SelectedNodeIds = new HashSet<string>();
            ProjectDesignerBoardUtility.MarkDirty(_boardAsset);
            if (SelectionChanged != null)
            {
                SelectionChanged.Invoke();
            }
        }

        private void UpdateMarqueeElement()
        {
            Rect rect = GetMarqueeRect();
            _marqueeElement.style.display = DisplayStyle.Flex;
            _marqueeElement.style.left = rect.xMin;
            _marqueeElement.style.top = rect.yMin;
            _marqueeElement.style.width = rect.width;
            _marqueeElement.style.height = rect.height;
        }

        private Rect GetMarqueeRect()
        {
            Vector2 min = Vector2.Min(_marqueeStartCanvasPosition, _marqueeCurrentCanvasPosition);
            Vector2 max = Vector2.Max(_marqueeStartCanvasPosition, _marqueeCurrentCanvasPosition);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private void ApplyMarqueeSelection()
        {
            Rect marqueeRect = GetMarqueeRect();
            Vector2 boardMin = CanvasToBoard(marqueeRect.min);
            Vector2 boardMax = CanvasToBoard(marqueeRect.max);
            Rect boardRect = Rect.MinMaxRect(
                Mathf.Min(boardMin.x, boardMax.x),
                Mathf.Min(boardMin.y, boardMax.y),
                Mathf.Max(boardMin.x, boardMax.x),
                Mathf.Max(boardMin.y, boardMax.y));
            List<string> selectedIds = _marqueeBaseSelection.ToList();

            foreach (string nodeId in GetVisibleNodeIdsWithin(boardRect))
            {
                if (!selectedIds.Contains(nodeId))
                {
                    selectedIds.Add(nodeId);
                }
            }

            _boardAsset.Document.ViewState.SetSelection(selectedIds, selectedIds.FirstOrDefault());
            ProjectDesignerBoardUtility.MarkDirty(_boardAsset);
            RefreshSelection();
            if (SelectionChanged != null)
            {
                SelectionChanged.Invoke();
            }
        }

        private IEnumerable<string> GetVisibleNodeIdsWithin(Rect boardRect)
        {
            foreach (BoardNodeModel node in BoardInsights.GetVisibleNodes(_boardAsset.Document))
            {
                Rect nodeRect = new Rect(node.Position, node.Size);
                if (boardRect.Overlaps(nodeRect, true))
                {
                    yield return node.Id;
                }
            }
        }

        private void CompleteMarqueeSelection()
        {
            _marqueeSelecting = false;
            _marqueePointerId = -1;
            _marqueeElement.style.display = DisplayStyle.None;
            _marqueeBaseSelection = new List<string>();
        }

        private void CancelMarqueeSelection()
        {
            _marqueeSelecting = false;
            _marqueePointerId = -1;
            _marqueeElement.style.display = DisplayStyle.None;
            _marqueeBaseSelection = new List<string>();
        }
    }
}
