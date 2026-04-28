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
        private sealed class EdgeLayerElement : VisualElement
        {
            private readonly ProjectBoardAsset _boardAsset;
            public IDictionary<string, Vector2> PreviewNodePositions { get; set; }

            public HashSet<string> VisibleNodeIds { get; set; }

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
                painter.lineWidth = 3f;

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
                    painter.strokeColor = ParseColor(definition == null ? "#9AA3AF" : definition.AccentColor, new Color(0.6f, 0.6f, 0.6f));

                    Vector2 sourcePosition = GetNodePosition(source);
                    Vector2 targetPosition = GetNodePosition(target);

                    Vector2 start = new Vector2(sourcePosition.x + source.Size.x, sourcePosition.y + source.Size.y * 0.5f);
                    Vector2 end = new Vector2(targetPosition.x, targetPosition.y + target.Size.y * 0.5f);
                    float tangent = Mathf.Max(80f, Mathf.Abs(end.x - start.x) * 0.35f);

                    painter.BeginPath();
                    painter.MoveTo(start);
                    painter.BezierCurveTo(start + Vector2.right * tangent, end + Vector2.left * tangent, end);
                    painter.Stroke();
                }
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
        }

        private readonly ProjectBoardAsset _boardAsset;
        private readonly IBoardCommandDispatcher _dispatcher;
        private readonly VisualElement _contentLayer;
        private readonly EdgeLayerElement _edgeLayer;
        private readonly VisualElement _nodeLayer;
        private readonly Label _hintLabel;
        private readonly Dictionary<string, BoardNodeView> _nodeViews = new Dictionary<string, BoardNodeView>();
        private readonly Dictionary<string, Vector2> _previewNodePositions = new Dictionary<string, Vector2>();

        private string _draggingNodeId = string.Empty;
        private int _dragPointerId = -1;
        private Vector2 _dragStartMousePosition;
        private Vector2 _dragStartNodePosition;
        private Vector2 _dragCurrentNodePosition;
        private bool _panning;
        private Vector2 _panStartMousePosition;
        private Vector2 _panStartOffset;

        public event Action<string> SelectionChanged;

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
            _contentLayer.style.position = Position.Absolute;
            _contentLayer.style.left = 0f;
            _contentLayer.style.top = 0f;
            _contentLayer.style.right = 0f;
            _contentLayer.style.bottom = 0f;
            Add(_contentLayer);

            _edgeLayer = new EdgeLayerElement(_boardAsset);
            _edgeLayer.PreviewNodePositions = _previewNodePositions;
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

            _hintLabel = new Label("Drag cards to arrange them. Drag empty space to pan. Drop Unity assets here to create nodes.");
            _hintLabel.AddToClassList("pd-canvas-hint");
            _hintLabel.pickingMode = PickingMode.Ignore;
            Add(_hintLabel);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            RegisterCallback<WheelEvent>(OnWheel);
            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);
        }

        public void Refresh()
        {
            UpdateTransform();
            RebuildNodes();
            _edgeLayer.MarkDirtyRepaint();
            MarkDirtyRepaint();
        }

        public void RefreshSelection()
        {
            string selectedNodeId = _boardAsset.Document.ViewState.SelectedNodeId;
            foreach (BoardNodeView nodeView in _nodeLayer.Children().OfType<BoardNodeView>())
            {
                nodeView.SetSelected(nodeView.NodeId == selectedNodeId);
            }

            BringNodeToFront(selectedNodeId);
            _edgeLayer.MarkDirtyRepaint();
            MarkDirtyRepaint();
        }

        public Vector2 GetViewportCenterOnBoard()
        {
            if (layout.width < 10f || layout.height < 10f)
            {
                return new Vector2(220f, 180f);
            }

            return ScreenToBoard(new Vector2(layout.width * 0.35f, layout.height * 0.35f));
        }

        public void FrameAll()
        {
            List<BoardNodeModel> nodes = _boardAsset.Document.Nodes.Where(node => node != null).ToList();
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

        private void RebuildNodes()
        {
            _nodeLayer.Clear();
            _nodeViews.Clear();
            _previewNodePositions.Clear();
            HashSet<string> visibleIds = new HashSet<string>(BoardInsights.GetVisibleNodes(_boardAsset.Document).Select(node => node.Id));
            _edgeLayer.VisibleNodeIds = visibleIds;
            string selectedNodeId = _boardAsset.Document.ViewState.SelectedNodeId;

            foreach (BoardNodeModel node in _boardAsset.Document.Nodes.OrderBy(node => node != null && node.Id == selectedNodeId ? 1 : 0))
            {
                if (node == null || !visibleIds.Contains(node.Id))
                {
                    continue;
                }

                IProjectDesignerNodeDefinition definition = ProjectDesignerRegistry.GetNodeDefinition(node.TypeId);
                var nodeView = new BoardNodeView(node, definition, _boardAsset.Document);
                nodeView.Refresh(_boardAsset.Document, selectedNodeId == node.Id);
                nodeView.Selected += OnNodeSelected;
                nodeView.DragStarted += OnNodeDragStarted;
                _nodeLayer.Add(nodeView);
                _nodeViews[node.Id] = nodeView;
            }
        }

        private void OnNodeSelected(string nodeId)
        {
            BringNodeToFront(nodeId);
            if (SelectionChanged != null)
            {
                SelectionChanged.Invoke(nodeId);
            }
        }

        private void OnNodeDragStarted(string nodeId, Vector2 mousePosition, int pointerId)
        {
            BoardNodeModel node = _boardAsset.Document.GetNode(nodeId);
            if (node == null)
            {
                return;
            }

            _draggingNodeId = nodeId;
            _dragPointerId = pointerId;
            _dragStartMousePosition = mousePosition;
            _dragStartNodePosition = node.Position;
            _dragCurrentNodePosition = node.Position;
            PointerCaptureHelper.CapturePointer(this, pointerId);
            BringNodeToFront(nodeId);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!string.IsNullOrEmpty(_draggingNodeId) || evt.button != 0 || !IsEmptyTarget(evt.target))
            {
                return;
            }

            _panning = true;
            _panStartMousePosition = GetEventPosition(evt.position);
            _panStartOffset = _boardAsset.Document.ViewState.PanOffset;
            PointerCaptureHelper.CapturePointer(this, evt.pointerId);

            if (SelectionChanged != null)
            {
                SelectionChanged.Invoke(string.Empty);
            }

            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!string.IsNullOrEmpty(_draggingNodeId))
            {
                float zoom = Mathf.Max(0.01f, _boardAsset.Document.ViewState.Zoom);
                Vector2 deltaX = (GetEventPosition(evt.position) - _dragStartMousePosition) / zoom;
                _dragCurrentNodePosition = _dragStartNodePosition + deltaX;
                _previewNodePositions[_draggingNodeId] = _dragCurrentNodePosition;

                if (_nodeViews.TryGetValue(_draggingNodeId, out BoardNodeView nodeView))
                {
                    nodeView.SetPreviewPosition(_dragCurrentNodePosition);
                }

                _edgeLayer.MarkDirtyRepaint();
                MarkDirtyRepaint();
                evt.StopPropagation();
                return;
            }

            if (!_panning || !PointerCaptureHelper.HasPointerCapture(this, evt.pointerId))
            {
                return;
            }

            Vector2 delta = GetEventPosition(evt.position) - _panStartMousePosition;
            _boardAsset.Document.ViewState.PanOffset = _panStartOffset + delta;
            ProjectDesignerBoardUtility.MarkDirty(_boardAsset);
            UpdateTransform();
            MarkDirtyRepaint();
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!string.IsNullOrEmpty(_draggingNodeId))
            {
                int dragPointerId = _dragPointerId;
                CompleteNodeDrag();
                if (dragPointerId >= 0 && PointerCaptureHelper.HasPointerCapture(this, dragPointerId))
                {
                    PointerCaptureHelper.ReleasePointer(this, dragPointerId);
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
            _panning = false;
        }

        private void OnWheel(WheelEvent evt)
        {
            Vector2 boardPositionBefore = ScreenToBoard(evt.localMousePosition);
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

            DragAndDrop.AcceptDrag();
            Vector2 startPosition = ScreenToBoard(evt.localMousePosition);
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

        private void UpdateTransform()
        {
            float zoom = _boardAsset.Document.ViewState.Zoom;
            Vector2 pan = _boardAsset.Document.ViewState.PanOffset;
            _contentLayer.transform.position = new Vector3(pan.x, pan.y, 0f);
            _contentLayer.transform.scale = new Vector3(zoom, zoom, 1f);
        }

        private Vector2 ScreenToBoard(Vector2 screenPosition)
        {
            float zoom = Mathf.Max(0.01f, _boardAsset.Document.ViewState.Zoom);
            return (screenPosition - _boardAsset.Document.ViewState.PanOffset) / zoom;
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

        private bool IsEmptyTarget(object target)
        {
            return ReferenceEquals(target, this) ||
                   ReferenceEquals(target, _contentLayer) ||
                   ReferenceEquals(target, _edgeLayer) ||
                   ReferenceEquals(target, _nodeLayer);
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

        private void CompleteNodeDrag()
        {
            string draggingNodeId = _draggingNodeId;
            _draggingNodeId = string.Empty;
            _dragPointerId = -1;

            BoardNodeModel node = _boardAsset.Document.GetNode(draggingNodeId);
            _previewNodePositions.Remove(draggingNodeId);

            if (node == null)
            {
                _edgeLayer.MarkDirtyRepaint();
                MarkDirtyRepaint();
                return;
            }

            if ((_dragCurrentNodePosition - node.Position).sqrMagnitude <= 0.01f)
            {
                if (_nodeViews.TryGetValue(draggingNodeId, out BoardNodeView nodeView))
                {
                    nodeView.SetPreviewPosition(node.Position);
                }

                _edgeLayer.MarkDirtyRepaint();
                MarkDirtyRepaint();
                return;
            }

            _dispatcher.Execute(new MoveNodeCommand(_boardAsset, draggingNodeId, _dragCurrentNodePosition));
        }

        private void CancelNodeDragPreview()
        {
            if (string.IsNullOrEmpty(_draggingNodeId))
            {
                return;
            }

            string draggingNodeId = _draggingNodeId;
            int dragPointerId = _dragPointerId;
            _draggingNodeId = string.Empty;
            _dragPointerId = -1;
            _previewNodePositions.Remove(draggingNodeId);

            BoardNodeModel node = _boardAsset.Document.GetNode(draggingNodeId);
            if (node != null && _nodeViews.TryGetValue(draggingNodeId, out BoardNodeView nodeView))
            {
                nodeView.SetPreviewPosition(node.Position);
            }

            if (dragPointerId >= 0 && PointerCaptureHelper.HasPointerCapture(this, dragPointerId))
            {
                PointerCaptureHelper.ReleasePointer(this, dragPointerId);
            }

            _edgeLayer.MarkDirtyRepaint();
            MarkDirtyRepaint();
        }

    }
}
