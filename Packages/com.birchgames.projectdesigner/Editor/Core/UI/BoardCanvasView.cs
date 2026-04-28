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
            public HashSet<string> VisibleNodeIds { get; set; }
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
                    DrawCurve(painter, source, target, GetNodePosition(source), GetNodePosition(target));
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
                Vector2 start = GetOutputAnchor(previewSource, sourcePosition);
                Vector2 end = PreviewConnection.EndPosition;
                float tangent = Mathf.Max(80f, Mathf.Abs(end.x - start.x) * 0.35f);

                painter.strokeColor = PreviewConnection.AccentColor;
                painter.lineWidth = 3f;
                painter.BeginPath();
                painter.MoveTo(start);
                painter.BezierCurveTo(start + Vector2.right * tangent, end + Vector2.left * tangent, end);
                painter.Stroke();
            }

            private void DrawCurve(UnityEngine.UIElements.Painter2D painter, BoardNodeModel source, BoardNodeModel target, Vector2 sourcePosition, Vector2 targetPosition)
            {
                Vector2 start = GetOutputAnchor(source, sourcePosition);
                Vector2 end = GetInputAnchor(target, targetPosition);
                float tangent = Mathf.Max(80f, Mathf.Abs(end.x - start.x) * 0.35f);

                painter.BeginPath();
                painter.MoveTo(start);
                painter.BezierCurveTo(start + Vector2.right * tangent, end + Vector2.left * tangent, end);
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

            private static Vector2 GetOutputAnchor(BoardNodeModel node, Vector2 nodePosition)
            {
                return new Vector2(nodePosition.x + node.Size.x, nodePosition.y + node.Size.y * 0.5f);
            }

            private static Vector2 GetInputAnchor(BoardNodeModel node, Vector2 nodePosition)
            {
                return new Vector2(nodePosition.x, nodePosition.y + node.Size.y * 0.5f);
            }
        }

        private const string DefaultHintText = "Drag cards to arrange them. Drag empty space to pan. Drag from Link to create relationships.";

        private readonly ProjectBoardAsset _boardAsset;
        private readonly IBoardCommandDispatcher _dispatcher;
        private readonly VisualElement _contentLayer;
        private readonly EdgeLayerElement _edgeLayer;
        private readonly VisualElement _nodeLayer;
        private readonly Label _hintLabel;
        private readonly Dictionary<string, BoardNodeView> _nodeViews = new Dictionary<string, BoardNodeView>();
        private readonly Dictionary<string, Vector2> _previewNodePositions = new Dictionary<string, Vector2>();
        private Dictionary<string, List<ProjectDesignerLinkOption>> _connectionTargetOptions = new Dictionary<string, List<ProjectDesignerLinkOption>>();

        private string _draggingNodeId = string.Empty;
        private int _dragPointerId = -1;
        private Vector2 _dragStartMousePosition;
        private Vector2 _dragStartNodePosition;
        private Vector2 _dragCurrentNodePosition;

        private string _connectingNodeId = string.Empty;
        private int _connectPointerId = -1;
        private Vector2 _connectPreviewPosition;
        private string _hoveredConnectionTargetId = string.Empty;
        private VisualElement _connectionMenu;

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

            _hintLabel = new Label(DefaultHintText);
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
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        public void Refresh()
        {
            HideConnectionMenu();
            CancelConnectionPreview();
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

            UpdateConnectionHighlights();
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
                nodeView.ConnectionStarted += OnNodeConnectionStarted;
                _nodeLayer.Add(nodeView);
                _nodeViews[node.Id] = nodeView;
            }

            UpdateConnectionHighlights();
        }

        private void OnNodeSelected(string nodeId)
        {
            HideConnectionMenu();
            BringNodeToFront(nodeId);
            if (SelectionChanged != null)
            {
                SelectionChanged.Invoke(nodeId);
            }
        }

        private void OnNodeDragStarted(string nodeId, Vector2 mousePosition, int pointerId)
        {
            if (!string.IsNullOrEmpty(_connectingNodeId))
            {
                return;
            }

            HideConnectionMenu();

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
            Focus();
        }

        private void OnNodeConnectionStarted(string nodeId, Vector2 mousePosition, int pointerId)
        {
            if (!string.IsNullOrEmpty(_draggingNodeId))
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
                UpdateHintLabel();
                return;
            }

            _connectingNodeId = nodeId;
            _connectPointerId = pointerId;
            _connectPreviewPosition = ScreenToBoard(mousePosition);
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

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (IsConnectionMenuTarget(evt.target))
            {
                return;
            }

            HideConnectionMenu();

            if (!string.IsNullOrEmpty(_draggingNodeId) || !string.IsNullOrEmpty(_connectingNodeId) || evt.button != 0 || !IsEmptyTarget(evt.target))
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

                Vector2 boardPosition = ScreenToBoard(GetEventPosition(evt.position));
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

            if (!string.IsNullOrEmpty(_draggingNodeId))
            {
                if (evt.pointerId != _dragPointerId)
                {
                    return;
                }

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
            if (!string.IsNullOrEmpty(_connectingNodeId))
            {
                if (evt.pointerId != _connectPointerId)
                {
                    return;
                }

                int connectPointerId = _connectPointerId;
                Vector2 boardPosition = ScreenToBoard(GetEventPosition(evt.position));
                CompleteConnectionDrag(boardPosition, GetEventPosition(evt.position));
                if (connectPointerId >= 0 && PointerCaptureHelper.HasPointerCapture(this, connectPointerId))
                {
                    PointerCaptureHelper.ReleasePointer(this, connectPointerId);
                }
                evt.StopPropagation();
                return;
            }

            if (!string.IsNullOrEmpty(_draggingNodeId))
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
            _panning = false;
        }

        private void OnWheel(WheelEvent evt)
        {
            HideConnectionMenu();

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

            HideConnectionMenu();
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

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Escape)
            {
                return;
            }

            bool hadConnectionState = !string.IsNullOrEmpty(_connectingNodeId) || _connectionMenu != null;
            HideConnectionMenu();
            CancelConnectionPreview();
            if (hadConnectionState)
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

            Vector2 endPosition = _connectPreviewPosition;
            if (!string.IsNullOrEmpty(_hoveredConnectionTargetId))
            {
                BoardNodeModel targetNode = _boardAsset.Document.GetNode(_hoveredConnectionTargetId);
                if (targetNode != null)
                {
                    endPosition = new Vector2(targetNode.Position.x, targetNode.Position.y + targetNode.Size.y * 0.5f);
                }
            }

            _edgeLayer.PreviewConnection = new ConnectionPreviewData
            {
                SourceNodeId = _connectingNodeId,
                EndPosition = endPosition,
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
            string targetNodeId = !string.IsNullOrEmpty(hoveredTargetId)
                ? hoveredTargetId
                : GetNodeIdAtBoardPosition(boardPosition, connectingNodeId);

            CancelConnectionPreview();

            if (sourceNode == null || string.IsNullOrEmpty(targetNodeId))
            {
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

        private void CancelConnectionPreview()
        {
            if (string.IsNullOrEmpty(_connectingNodeId))
            {
                return;
            }

            int connectPointerId = _connectPointerId;
            _connectingNodeId = string.Empty;
            _connectPointerId = -1;
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
    }
}
