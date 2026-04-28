using System;
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
        private readonly BoardNodeModel _node;
        private readonly IProjectDesignerNodeDefinition _definition;
        private readonly VisualElement _selectionFrame;
        private readonly VisualElement _linkFrame;
        private readonly Label _titleLabel;
        private readonly VisualElement _signalContainer;
        private readonly Label _categoryLabel;
        private readonly Label _previewLabel;
        private readonly VisualElement _tagsContainer;
        private readonly Label _connectHandle;
        private readonly Color _accentColor;

        public event Action<BoardNodeSelectionRequest> Selected;
        public event Action<string, Vector2, int> DragStarted;
        public event Action<string, Vector2, int> ConnectionStarted;

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

            _linkFrame = new VisualElement();
            _linkFrame.AddToClassList("pd-node-link-frame");
            _linkFrame.pickingMode = PickingMode.Ignore;
            _linkFrame.style.display = DisplayStyle.None;
            Add(_linkFrame);

            _connectHandle = new Label("Link");
            _connectHandle.AddToClassList("pd-node-connector");
            _connectHandle.pickingMode = PickingMode.Position;
            _connectHandle.style.backgroundColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.12f);
            _connectHandle.style.borderTopColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.24f);
            _connectHandle.style.borderRightColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.24f);
            _connectHandle.style.borderBottomColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.24f);
            _connectHandle.style.borderLeftColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.24f);
            _connectHandle.style.color = ProjectDesignerColorUtility.Blend(_accentColor, new Color(0.18f, 0.22f, 0.26f), 0.28f);
            _connectHandle.RegisterCallback<PointerDownEvent>(OnConnectionPointerDown);
            Add(_connectHandle);

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

            _previewLabel = new Label(_definition == null ? string.Empty : _definition.GetPreview(_node, document));
            _previewLabel.AddToClassList("pd-node-preview");
            _previewLabel.pickingMode = PickingMode.Ignore;
            Add(_previewLabel);

            _tagsContainer = new VisualElement();
            _tagsContainer.AddToClassList("pd-node-tag-row");
            _tagsContainer.pickingMode = PickingMode.Ignore;
            Add(_tagsContainer);

            RefreshTagChips();

            RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        public void Refresh(BoardDocument document, bool isSelected)
        {
            _titleLabel.text = _node.Title;
            _categoryLabel.text = _node.Category;
            RefreshSignals(document);
            _previewLabel.text = _definition == null ? string.Empty : _definition.GetPreview(_node, document);
            RefreshTagChips();

            style.left = _node.Position.x;
            style.top = _node.Position.y;
            style.width = _node.Size.x;
            style.height = _node.Size.y;

            SetSelected(isSelected);
            SetConnectionState(false, false, false);
        }

        public void SetSelected(bool isSelected)
        {
            EnableInClassList("pd-node-selected", isSelected);
            _selectionFrame.style.display = isSelected ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetConnectionState(bool isOrigin, bool isValidTarget, bool isHoveredTarget)
        {
            bool hasLinkState = isValidTarget || isHoveredTarget;
            _linkFrame.style.display = hasLinkState ? DisplayStyle.Flex : DisplayStyle.None;
            _linkFrame.EnableInClassList("pd-node-link-valid", isValidTarget && !isHoveredTarget);
            _linkFrame.EnableInClassList("pd-node-link-hover", isHoveredTarget);
            _connectHandle.EnableInClassList("pd-node-connector-active", isOrigin);

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

            _connectHandle.style.backgroundColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.12f);
            _connectHandle.style.borderTopColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.24f);
            _connectHandle.style.borderRightColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.24f);
            _connectHandle.style.borderBottomColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.24f);
            _connectHandle.style.borderLeftColor = ProjectDesignerColorUtility.WithAlpha(_accentColor, 0.24f);
            _connectHandle.style.color = ProjectDesignerColorUtility.Blend(_accentColor, new Color(0.18f, 0.22f, 0.26f), 0.28f);
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

            evt.StopPropagation();
        }

        private static Vector2 GetEventPosition(Vector3 position)
        {
            return new Vector2(position.x, position.y);
        }

        private void RefreshTagChips()
        {
            _tagsContainer.Clear();

            if (_node.Tags == null || _node.Tags.Count == 0)
            {
                _tagsContainer.style.display = DisplayStyle.None;
                return;
            }

            _tagsContainer.style.display = DisplayStyle.Flex;
            foreach (string tag in _node.Tags)
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    continue;
                }

                var chip = new Label(tag.Trim());
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
        }

        private void RefreshSignals(BoardDocument document)
        {
            _signalContainer.Clear();

            TaskNodeModel task = _node as TaskNodeModel;
            if (task != null)
            {
                ProjectDesignerTeamRosterAsset roster = ProjectDesignerTeamRosterContext.CurrentRoster;
                AddSignalChip(task.Status.ToString(), GetTaskStatusColor(task.Status));
                AddSignalChip(task.Priority.ToString(), GetTaskPriorityColor(task.Priority));

                if (!string.IsNullOrWhiteSpace(task.AssigneeId))
                {
                    ProjectDesignerTeamMemberData member = ProjectDesignerTeamRosterResolver.ResolveMember(roster, task.AssigneeId);
                    if (member != null)
                    {
                        Color assigneeColor = ProjectDesignerColorUtility.ParseOrFallback(member.AccentColor, new Color(0.2f, 0.52f, 0.88f));
                        AddSignalChip(member.DisplayName, assigneeColor);
                    }
                    else
                    {
                        AddSignalChip("Unmapped Assignee", new Color(0.9f, 0.58f, 0.24f));
                    }
                }

                if (BoardInsights.IsTaskBlocked(document, task))
                {
                    AddSignalChip("Blocked", new Color(0.88f, 0.37f, 0.26f));
                }

                if (BoardInsights.IsTaskOverdue(task))
                {
                    AddSignalChip("Overdue", new Color(0.84f, 0.25f, 0.27f));
                }
                else if (BoardInsights.IsTaskDueSoon(task))
                {
                    AddSignalChip("Due Soon", new Color(0.92f, 0.67f, 0.22f));
                }

                return;
            }

            MilestoneNodeModel milestone = _node as MilestoneNodeModel;
            if (milestone != null)
            {
                BoardMilestoneHealthReport health = BoardInsights.GetMilestoneHealth(document, milestone);
                AddSignalChip(GetMilestoneHealthLabel(health.State), GetMilestoneHealthColor(health.State));
                if (!string.IsNullOrWhiteSpace(milestone.TargetDateIso))
                {
                    AddSignalChip(milestone.TargetDateIso, new Color(0.32f, 0.45f, 0.82f));
                }

                return;
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

        private static Color GetTaskStatusColor(TaskNodeStatus status)
        {
            switch (status)
            {
                case TaskNodeStatus.InProgress:
                    return new Color(0.24f, 0.55f, 0.88f);
                case TaskNodeStatus.Blocked:
                    return new Color(0.88f, 0.37f, 0.26f);
                case TaskNodeStatus.Done:
                    return new Color(0.28f, 0.66f, 0.4f);
                default:
                    return new Color(0.53f, 0.59f, 0.66f);
            }
        }

        private static Color GetTaskPriorityColor(TaskNodePriority priority)
        {
            switch (priority)
            {
                case TaskNodePriority.Critical:
                    return new Color(0.83f, 0.25f, 0.3f);
                case TaskNodePriority.High:
                    return new Color(0.95f, 0.58f, 0.22f);
                case TaskNodePriority.Medium:
                    return new Color(0.33f, 0.53f, 0.92f);
                default:
                    return new Color(0.47f, 0.67f, 0.41f);
            }
        }

        private static string GetMilestoneHealthLabel(BoardMilestoneHealthState state)
        {
            switch (state)
            {
                case BoardMilestoneHealthState.Complete:
                    return "Complete";
                case BoardMilestoneHealthState.OffTrack:
                    return "Off Track";
                case BoardMilestoneHealthState.AtRisk:
                    return "At Risk";
                case BoardMilestoneHealthState.NoLinkedTasks:
                    return "Needs Tasks";
                default:
                    return "On Track";
            }
        }

        private static Color GetMilestoneHealthColor(BoardMilestoneHealthState state)
        {
            switch (state)
            {
                case BoardMilestoneHealthState.Complete:
                    return new Color(0.28f, 0.66f, 0.4f);
                case BoardMilestoneHealthState.OffTrack:
                    return new Color(0.83f, 0.25f, 0.3f);
                case BoardMilestoneHealthState.AtRisk:
                    return new Color(0.95f, 0.58f, 0.22f);
                case BoardMilestoneHealthState.NoLinkedTasks:
                    return new Color(0.53f, 0.59f, 0.66f);
                default:
                    return new Color(0.24f, 0.55f, 0.88f);
            }
        }
    }
}
