using System;
using System.Collections.Generic;
using System.Linq;
using ProjectDesigner.V2.BuiltIn;
using ProjectDesigner.V2.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class ProjectDesignerWorkspaceView : VisualElement
    {
        private const string LibraryExpandedSessionKey = "ProjectDesigner.V2.LibraryExpanded";
        private const string InspectorExpandedSessionKey = "ProjectDesigner.V2.InspectorExpanded";
        private const string InspectorWidthSessionKey = "ProjectDesigner.V2.InspectorWidth";
        private const string InspectorFoldoutSessionKeyPrefix = "ProjectDesigner.V2.InspectorFoldout.";
        private const float InspectorDefaultWidth = 380f;
        private const float InspectorMinWidth = 280f;
        private const float InspectorMaxWidth = 520f;
        private const float InspectorCollapsedWidth = 154f;
        internal const int ToolbarTitleMaxLength = 40;
        internal const int InspectorSummaryMaxLength = 22;

        private readonly ProjectBoardAsset _boardAsset;
        private readonly Action<ProjectBoardAsset> _setBoard;
        private readonly ProjectDesignerCommandStack _commandStack;
        private readonly BoardCanvasView _canvasView;
        private readonly VisualElement _libraryPanel;
        private readonly ScrollView _libraryView;
        private readonly Label _librarySummaryLabel;
        private readonly Button _libraryToggleButton;
        private readonly VisualElement _inspectorPanel;
        private readonly VisualElement _inspectorSplitter;
        private readonly ScrollView _inspectorView;
        private readonly Label _inspectorSummaryLabel;
        private readonly Button _inspectorToggleButton;
        private readonly VisualElement _savedFiltersContainer;
        private readonly ProjectDesignerOverviewView _overviewView;
        private readonly ToolbarSearchField _searchField;
        private readonly PopupField<string> _categoryField;
        private Label _toolbarTitle;
        private bool _libraryExpanded;
        private bool _inspectorExpanded;
        private bool _resizingInspector;
        private int _inspectorResizePointerId = -1;
        private float _inspectorResizeStartMouseX;
        private float _inspectorResizeStartWidth;
        private float _inspectorWidth;
        private int _createSequence;

        public ProjectDesignerWorkspaceView(ProjectBoardAsset boardAsset, Action<ProjectBoardAsset> setBoard)
        {
            _boardAsset = boardAsset;
            _setBoard = setBoard;

            ProjectDesignerBuiltInRegistration.Register();
            _commandStack = new ProjectDesignerCommandStack(_boardAsset, PersistBoard);
            _commandStack.Changed += RefreshAll;
            _libraryExpanded = SessionState.GetBool(LibraryExpandedSessionKey, true);
            _inspectorExpanded = SessionState.GetBool(InspectorExpandedSessionKey, false);
            _inspectorWidth = Mathf.Clamp(SessionState.GetFloat(InspectorWidthSessionKey, InspectorDefaultWidth), InspectorMinWidth, InspectorMaxWidth);

            AddToClassList("pd-workspace");

            Toolbar toolbar = BuildToolbar();
            Add(toolbar);

            var body = new VisualElement();
            body.AddToClassList("pd-workspace-body");
            Add(body);

            _libraryPanel = new VisualElement();
            _libraryPanel.AddToClassList("pd-sidebar-panel");
            body.Add(_libraryPanel);

            var libraryHeader = new VisualElement();
            libraryHeader.AddToClassList("pd-sidebar-header");
            _libraryPanel.Add(libraryHeader);

            var libraryTitle = new Label("Library");
            libraryTitle.AddToClassList("pd-sidebar-header-title");
            libraryHeader.Add(libraryTitle);

            _librarySummaryLabel = new Label();
            _librarySummaryLabel.AddToClassList("pd-sidebar-summary");
            libraryHeader.Add(_librarySummaryLabel);

            _libraryToggleButton = new Button(() => ToggleLibraryExpanded(true));
            _libraryToggleButton.AddToClassList("pd-sidebar-toggle");
            libraryHeader.Add(_libraryToggleButton);

            _libraryView = new ScrollView();
            _libraryView.AddToClassList("pd-sidebar");
            _libraryPanel.Add(_libraryView);

            var centerColumn = new VisualElement();
            centerColumn.AddToClassList("pd-center-column");
            body.Add(centerColumn);

            _canvasView = new BoardCanvasView(_boardAsset, _commandStack);
            _canvasView.SelectionChanged += OnSelectionChanged;
            _canvasView.AddManipulator(new ContextualMenuManipulator(BuildCanvasContextMenu));
            centerColumn.Add(_canvasView);

            _overviewView = new ProjectDesignerOverviewView(ToggleQuickFilter);
            centerColumn.Add(_overviewView);

            _inspectorSplitter = new VisualElement();
            _inspectorSplitter.AddToClassList("pd-inspector-splitter");
            _inspectorSplitter.RegisterCallback<PointerDownEvent>(OnInspectorSplitterPointerDown);
            _inspectorSplitter.RegisterCallback<PointerMoveEvent>(OnInspectorSplitterPointerMove);
            _inspectorSplitter.RegisterCallback<PointerUpEvent>(OnInspectorSplitterPointerUp);
            _inspectorSplitter.RegisterCallback<PointerCaptureOutEvent>(OnInspectorSplitterCaptureOut);
            body.Add(_inspectorSplitter);

            _inspectorPanel = new VisualElement();
            _inspectorPanel.AddToClassList("pd-inspector-panel");
            body.Add(_inspectorPanel);

            var inspectorHeader = new VisualElement();
            inspectorHeader.AddToClassList("pd-inspector-header");
            _inspectorPanel.Add(inspectorHeader);

            var inspectorTitle = new Label("Details");
            inspectorTitle.AddToClassList("pd-inspector-header-title");
            inspectorHeader.Add(inspectorTitle);

            _inspectorSummaryLabel = new Label("Board");
            _inspectorSummaryLabel.AddToClassList("pd-inspector-summary");
            inspectorHeader.Add(_inspectorSummaryLabel);

            _inspectorToggleButton = new Button(() => ToggleInspectorExpanded(true));
            _inspectorToggleButton.AddToClassList("pd-inspector-toggle");
            inspectorHeader.Add(_inspectorToggleButton);

            _inspectorView = new ScrollView();
            _inspectorView.AddToClassList("pd-inspector");
            _inspectorPanel.Add(_inspectorView);

            _savedFiltersContainer = new VisualElement();
            _savedFiltersContainer.AddToClassList("pd-filter-list");

            _searchField = new ToolbarSearchField();
            _searchField.AddToClassList("pd-toolbar-search");
            _searchField.value = _boardAsset.Document.ViewState.SearchQuery;
            _searchField.tooltip = "Search titles, previews, and tags across the current board.";
            _searchField.style.width = 340f;
            _searchField.style.minHeight = 28f;
            _searchField.style.height = 28f;
            _searchField.style.maxHeight = 28f;
            _searchField.style.marginTop = 5f;
            _searchField.style.marginBottom = 5f;
            _searchField.style.alignSelf = Align.Center;
            _searchField.RegisterValueChangedCallback(evt =>
            {
                _boardAsset.Document.ViewState.SearchQuery = evt.newValue;
                PersistBoard();
                RefreshAll();
            });

            _categoryField = new PopupField<string>(new List<string>
            {
                BoardNodeCategories.All,
                BoardNodeCategories.Planning,
                BoardNodeCategories.Reference,
                BoardNodeCategories.TechnicalDesign
            }, _boardAsset.Document.ViewState.Category);
            _categoryField.AddToClassList("pd-toolbar-category");
            _categoryField.tooltip = "Focus the board on a specific category.";
            _categoryField.style.width = 150f;
            _categoryField.style.minHeight = 28f;
            _categoryField.style.height = 28f;
            _categoryField.style.maxHeight = 28f;
            _categoryField.style.marginTop = 5f;
            _categoryField.style.marginBottom = 5f;
            _categoryField.style.marginLeft = 6f;
            _categoryField.style.alignSelf = Align.Center;
            _categoryField.RegisterValueChangedCallback(evt =>
            {
                _boardAsset.Document.ViewState.Category = evt.newValue;
                _boardAsset.Document.ViewState.ActiveFilterId = string.Empty;
                PersistBoard();
                RefreshAll();
            });

            toolbar.Add(new ToolbarSpacer());
            toolbar.Add(_searchField);
            toolbar.Add(_categoryField);

            RefreshAll();
            RegisterCallback<KeyDownEvent>(HandleKeyDown);
        }

        private Toolbar BuildToolbar()
        {
            var toolbar = new Toolbar();
            toolbar.AddToClassList("pd-toolbar");

            _toolbarTitle = new Label(_boardAsset.Document.BoardName);
            _toolbarTitle.AddToClassList("pd-toolbar-title");
            toolbar.Add(_toolbarTitle);

            toolbar.Add(CreateArrangeMenu());
            toolbar.Add(CreateToolbarButton("All Boards", () => _setBoard(null)));
            return toolbar;
        }

        private void RefreshAll()
        {
            _toolbarTitle.text = BuildToolbarTitle(_boardAsset.Document.BoardName);
            _toolbarTitle.tooltip = _boardAsset.Document.BoardName;
            RefreshLibrary();
            RefreshInspector();
            _canvasView.Refresh();
            _overviewView.Refresh(_boardAsset.Document);
        }

        private void RefreshLibrary()
        {
            UpdateLibraryPanelState();

            if (!_libraryExpanded)
            {
                _libraryView.Clear();
                _libraryView.style.display = DisplayStyle.None;
                return;
            }

            _libraryView.style.display = DisplayStyle.Flex;
            _libraryView.Clear();

            _libraryView.Add(CreateSectionLabel("Add Cards"));
            _libraryView.Add(CreateMutedBodyLabel("Start with planning cards. Add references as you gather material."));

            foreach (IGrouping<string, IProjectDesignerNodeDefinition> group in ProjectDesignerRegistry.GetNodeDefinitions().GroupBy(definition => definition.Category))
            {
                _libraryView.Add(CreateLibraryGroup(group));
            }

            _libraryView.Add(CreateSectionLabel("Saved Views"));

            BoardSavedFilter activeSavedFilter = GetActiveSavedFilter();
            if (activeSavedFilter != null)
            {
                _libraryView.Add(CreateActiveViewLabel(activeSavedFilter.Name));
            }

            var savedViewActions = CreateActionRow();
            savedViewActions.Add(CreateLibraryActionButton("Save Current View", SaveCurrentFilter, true));

            Button updateCurrentViewButton = CreateLibraryActionButton("Update Current View", () => UpdateSavedFilterFromCurrentView(activeSavedFilter), false);
            updateCurrentViewButton.tooltip = "Overwrite the active saved view with the current search, category, and quick filter.";
            updateCurrentViewButton.SetEnabled(activeSavedFilter != null);
            savedViewActions.Add(updateCurrentViewButton);

            Button clearViewButton = CreateLibraryActionButton("Clear View", ClearCurrentFilter, false);
            clearViewButton.tooltip = "Reset the active saved view, search, category, and quick filters.";
            clearViewButton.SetEnabled(CanClearCurrentFilter());
            savedViewActions.Add(clearViewButton);
            _libraryView.Add(savedViewActions);

            _libraryView.Add(_savedFiltersContainer);
            var shortcutInfo = CreateMutedBodyLabel("Shortcuts: F focus selected, Delete remove selected, Ctrl+D duplicate, Ctrl+A select visible, Esc cancel or clear.");
            shortcutInfo.AddToClassList("pd-shortcut-info");
            _libraryView.Add(shortcutInfo);
            _savedFiltersContainer.Clear();

            if (_boardAsset.Document.SavedFilters.Count == 0)
            {
                _savedFiltersContainer.Add(CreateMutedBodyLabel("Save the current search, category, and quick-filter state to jump back to a view later."));
            }
            else
            {
                foreach (BoardSavedFilter filter in _boardAsset.Document.SavedFilters)
                {
                    BoardSavedFilter localFilter = filter;
                    bool isActive = IsSavedViewActive(_boardAsset.Document.ViewState.ActiveFilterId, localFilter);
                    var button = new Button(() => ApplySavedFilter(localFilter))
                    {
                        text = BuildSavedViewButtonText(localFilter.Name, isActive)
                    };
                    button.AddToClassList("pd-filter-button");
                    button.tooltip = BuildSavedViewTooltip(localFilter, isActive);
                    if (string.Equals(localFilter.Category, BoardNodeCategories.TechnicalDesign, StringComparison.Ordinal))
                    {
                        button.AddToClassList("pd-filter-button-technical");
                    }

                    if (isActive)
                    {
                        button.AddToClassList("pd-filter-button-active");
                    }

                    _savedFiltersContainer.Add(CreateSavedViewEntry(localFilter, button));
                }
            }
        }

        private void RefreshInspector()
        {
            _canvasView.SetHoveredEdge(string.Empty);
            _inspectorView.Clear();

            List<BoardNodeModel> selectedNodes = GetSelectedNodes();
            UpdateInspectorPanelState(selectedNodes);

            if (!_inspectorExpanded)
            {
                _inspectorView.style.display = DisplayStyle.None;
                return;
            }

            _inspectorView.style.display = DisplayStyle.Flex;
            if (selectedNodes.Count == 0)
            {
                BuildBoardInspector();
                return;
            }

            if (selectedNodes.Count > 1)
            {
                BuildMultiSelectionInspector(selectedNodes);
                return;
            }

            BoardNodeModel selectedNode = selectedNodes[0];

            Label title = new Label(selectedNode.Title);
            title.AddToClassList("pd-inspector-title");
            _inspectorView.Add(title);

            var actionRow = CreateActionRow();
            actionRow.Add(CreateInspectorButton("Duplicate", DuplicateSelection));
            actionRow.Add(CreateInspectorButton("Delete", DeleteSelectedNode));
            actionRow.Add(CreateInspectorButton("Focus", FocusSelection));
            _inspectorView.Add(actionRow);

            IProjectDesignerInspector inspector = ProjectDesignerRegistry.GetInspector(selectedNode.TypeId);
            if (inspector != null)
            {
                _inspectorView.Add(inspector.BuildInspector(_boardAsset, selectedNode, _commandStack, RefreshCanvasOnly));
            }

            BuildConnectionsInspector(selectedNode);
        }

        private void RefreshCanvasOnly()
        {
            _canvasView.Refresh();
            _overviewView.Refresh(_boardAsset.Document);
        }

        private void BuildBoardInspector()
        {
            Label title = new Label("Planning Board");
            title.AddToClassList("pd-inspector-title");
            _inspectorView.Add(title);

            if (!string.IsNullOrWhiteSpace(_boardAsset.Document.ViewState.QuickFilterId))
            {
                _inspectorView.Add(CreateMutedBodyLabel("Quick filter: " + DescribeQuickFilter(_boardAsset.Document.ViewState.QuickFilterId)));
            }

            var nameField = new TextField("Board Name");
            nameField.value = _boardAsset.Document.BoardName;
            nameField.isDelayed = true;
            nameField.RegisterValueChangedCallback(evt =>
            {
                _commandStack.Execute(BoardMutationCommand.Create(_boardAsset, "Rename Board", document =>
                {
                    document.BoardName = evt.newValue;
                }));
            });
            _inspectorView.Add(nameField);

            var summaryField = new TextField("Summary");
            summaryField.value = _boardAsset.Document.Summary;
            summaryField.multiline = true;
            summaryField.isDelayed = true;
            summaryField.style.minHeight = 72f;
            summaryField.RegisterValueChangedCallback(evt =>
            {
                _commandStack.Execute(BoardMutationCommand.Create(_boardAsset, "Update Summary", document =>
                {
                    document.Summary = evt.newValue;
                }));
            });
            _inspectorView.Add(summaryField);

            var teamField = new TextField("Board Team Snapshot");
            teamField.value = string.Join(", ", _boardAsset.Document.TeamMembers);
            teamField.isDelayed = true;
            teamField.RegisterValueChangedCallback(evt =>
            {
                _commandStack.Execute(BoardMutationCommand.Create(_boardAsset, "Update Board Team Snapshot", document =>
                {
                    document.TeamMembers.Clear();
                    foreach (string entry in evt.newValue.Split(','))
                    {
                        string trimmed = entry.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                        {
                            document.TeamMembers.Add(trimmed);
                        }
                    }
                }));
            });
            _inspectorView.Add(teamField);
            _inspectorView.Add(CreateMutedBodyLabel("Use this as board context and shared knowledge. Task assignees come from the project-wide team roster in Project Settings."));

            Foldout templatesFoldout = CreatePersistentInspectorFoldout("Board.StarterLayouts", "Starter Layouts", false);
            templatesFoldout.Add(CreateMutedBodyLabel("Swap the current board structure for a curated layout."));

            foreach (BoardTemplateDefinition template in _boardAsset.Document.Templates)
            {
                BoardTemplateDefinition localTemplate = template;
                var button = new Button(() => ReplaceBoardWithTemplate(localTemplate))
                {
                    text = localTemplate.Name
                };
                button.AddToClassList("pd-secondary-button");
                templatesFoldout.Add(button);
            }

            _inspectorView.Add(templatesFoldout);
            BuildSavedViewsInspector();
            BuildPlannerInsights();
        }

        private void BuildSavedViewsInspector()
        {
            Foldout savedViewsFoldout = CreatePersistentInspectorFoldout("Board.SavedViews", "Saved Views", false);
            savedViewsFoldout.Add(CreateMutedBodyLabel("Save reusable board searches, categories, and quick filters, then rename or refresh them from here."));

            var actions = CreateActionRow();
            actions.Add(CreateInspectorButton("Save Current View", SaveCurrentFilter));

            BoardSavedFilter activeSavedFilter = GetActiveSavedFilter();
            Button updateCurrentButton = CreateInspectorButton("Update Current View", () => UpdateSavedFilterFromCurrentView(activeSavedFilter));
            updateCurrentButton.SetEnabled(activeSavedFilter != null);
            actions.Add(updateCurrentButton);
            savedViewsFoldout.Add(actions);

            if (_boardAsset.Document.SavedFilters.Count == 0)
            {
                savedViewsFoldout.Add(CreateMutedBodyLabel("No saved views yet. Save the current board state to capture your current workflow."));
                _inspectorView.Add(savedViewsFoldout);
                return;
            }

            foreach (BoardSavedFilter filter in _boardAsset.Document.SavedFilters)
            {
                savedViewsFoldout.Add(CreateSavedViewInspectorCard(filter));
            }

            _inspectorView.Add(savedViewsFoldout);
        }

        private VisualElement CreateSavedViewInspectorCard(BoardSavedFilter filter)
        {
            bool isActive = IsSavedViewActive(_boardAsset.Document.ViewState.ActiveFilterId, filter);
            int matchCount = GetSavedViewMatchCount(filter);

            var card = new VisualElement();
            card.AddToClassList("pd-welcome-preset");

            if (isActive)
            {
                card.Add(CreateActiveViewLabel(filter.Name));
            }

            var nameField = new TextField("Name");
            nameField.value = filter.Name;
            nameField.isDelayed = true;
            nameField.RegisterValueChangedCallback(evt => RenameSavedFilter(filter, evt.newValue));
            card.Add(nameField);

            card.Add(CreateMutedBodyLabel(BuildSavedViewSummaryText(filter, matchCount)));

            var actions = CreateActionRow();
            actions.Add(CreateInspectorButton("Apply", () => ApplySavedFilter(filter)));
            actions.Add(CreateInspectorButton("Update From Current", () => UpdateSavedFilterFromCurrentView(filter)));
            actions.Add(CreateInspectorButton("Delete", () => DeleteSavedFilter(filter)));
            card.Add(actions);

            return card;
        }

        private void BuildMultiSelectionInspector(List<BoardNodeModel> selectedNodes)
        {
            Label title = new Label(selectedNodes.Count + " Cards Selected");
            title.AddToClassList("pd-inspector-title");
            _inspectorView.Add(title);

            string categories = string.Join(", ", selectedNodes
                .Select(node => node.Category)
                .Distinct()
                .OrderBy(category => category));
            _inspectorView.Add(CreateMutedBodyLabel("Categories: " + categories));
            _inspectorView.Add(CreateMutedBodyLabel("Primary card: " + (_boardAsset.Document.ViewState.SelectedNodeId ?? string.Empty)));

            VisualElement firstRow = CreateActionRow();
            firstRow.Add(CreateInspectorButton("Duplicate", DuplicateSelection));
            firstRow.Add(CreateInspectorButton("Delete", DeleteSelectedNode));
            firstRow.Add(CreateInspectorButton("Focus", FocusSelection));
            _inspectorView.Add(firstRow);

            VisualElement secondRow = CreateActionRow();
            secondRow.Add(CreateInspectorButton("Auto Layout", AutoLayoutSelectionOrVisible));
            secondRow.Add(CreateInspectorButton("Align Left", () => ArrangeSelection(BoardArrangeMode.AlignLeft)));
            secondRow.Add(CreateInspectorButton("Align Top", () => ArrangeSelection(BoardArrangeMode.AlignTop)));
            secondRow.Add(CreateInspectorButton("Distribute H", () => ArrangeSelection(BoardArrangeMode.DistributeHorizontal)));
            _inspectorView.Add(secondRow);

            VisualElement thirdRow = CreateActionRow();
            thirdRow.Add(CreateInspectorButton("Align Center", () => ArrangeSelection(BoardArrangeMode.AlignCenter)));
            thirdRow.Add(CreateInspectorButton("Align Middle", () => ArrangeSelection(BoardArrangeMode.AlignMiddle)));
            thirdRow.Add(CreateInspectorButton("Distribute V", () => ArrangeSelection(BoardArrangeMode.DistributeVertical)));
            _inspectorView.Add(thirdRow);
        }

        private void BuildConnectionsInspector(BoardNodeModel selectedNode)
        {
            Foldout connectionsFoldout = CreatePersistentInspectorFoldout(selectedNode.Id + ".Links", "Links", false);
            connectionsFoldout.Add(CreateMutedBodyLabel("Drag from the Link handle on a card for the fastest path, or create relationships manually here when you need more control."));
            _inspectorView.Add(connectionsFoldout);

            List<ProjectDesignerLinkOption> options = ProjectDesignerLinkUtility.GetLinkOptions(_boardAsset.Document, selectedNode);
            if (options.Count == 0)
            {
                connectionsFoldout.Add(new Label("No valid links are available for this card right now."));
            }
            else
            {
                List<IProjectDesignerEdgeDefinition> availableDefinitions = options
                    .Select(option => option.Definition)
                    .GroupBy(definition => definition.TypeId)
                    .Select(group => group.First())
                    .ToList();

                var edgePicker = new PopupField<string>("Link Type", availableDefinitions.Select(definition => definition.DisplayName).ToList(), 0);
                var targetPicker = new PopupField<ProjectDesignerLinkTargetChoice>(
                    "Linked Card",
                    new List<ProjectDesignerLinkTargetChoice>(),
                    0,
                    choice => choice == null ? string.Empty : choice.DisplayLabel,
                    choice => choice == null ? string.Empty : choice.DisplayLabel);
                var directionHint = CreateMutedBodyLabel(string.Empty);
                Label linkDescription = CreateMutedBodyLabel(GetLinkDescription(availableDefinitions[0].TypeId));
                if (availableDefinitions.Count > 1)
                {
                    connectionsFoldout.Add(edgePicker);
                }
                else
                {
                    connectionsFoldout.Add(CreateMutedBodyLabel("Link Type: " + availableDefinitions[0].DisplayName));
                }

                connectionsFoldout.Add(linkDescription);
                connectionsFoldout.Add(targetPicker);
                connectionsFoldout.Add(directionHint);

                List<ProjectDesignerLinkOption> activeOptions = new List<ProjectDesignerLinkOption>();
                List<ProjectDesignerLinkTargetChoice> activeTargetChoices = new List<ProjectDesignerLinkTargetChoice>();
                Action refreshTargetPicker = () =>
                {
                    IProjectDesignerEdgeDefinition selectedDefinition = availableDefinitions
                        .FirstOrDefault(definition => definition.DisplayName == edgePicker.value);
                    activeOptions = options
                        .Where(option => option.Definition.TypeId == (selectedDefinition == null ? string.Empty : selectedDefinition.TypeId))
                        .ToList();

                    activeTargetChoices = ProjectDesignerLinkUtility.GetTargetChoicesForDefinition(
                        _boardAsset.Document,
                        selectedNode,
                        selectedDefinition == null ? string.Empty : selectedDefinition.TypeId);

                    targetPicker.choices = activeTargetChoices;
                    if (activeTargetChoices.Count > 0)
                    {
                        targetPicker.index = 0;
                        targetPicker.SetValueWithoutNotify(activeTargetChoices[0]);
                    }
                    else
                    {
                        targetPicker.index = -1;
                    }

                    if (linkDescription != null)
                    {
                        linkDescription.text = GetLinkDescription(selectedDefinition == null ? string.Empty : selectedDefinition.TypeId);
                    }

                    bool linksIntoSelectedNode = activeOptions.Any(option => !option.SelectedNodeIsSource);
                    directionHint.text = linksIntoSelectedNode
                        ? "Some link choices will point into the selected card instead of away from it."
                        : "Only valid link targets are shown here.";
                };

                edgePicker.RegisterValueChangedCallback(evt => refreshTargetPicker());
                refreshTargetPicker();

                var createButton = new Button(() =>
                {
                    if (activeTargetChoices.Count == 0 || targetPicker.index < 0 || targetPicker.index >= activeTargetChoices.Count)
                    {
                        return;
                    }

                    ProjectDesignerLinkTargetChoice selectedChoice = targetPicker.value ?? activeTargetChoices[targetPicker.index];
                    ProjectDesignerLinkOption option = selectedChoice == null ? null : selectedChoice.Option;
                    if (option == null)
                    {
                        return;
                    }

                    string sourceId = option.SelectedNodeIsSource ? selectedNode.Id : option.OtherNode.Id;
                    string targetId = option.SelectedNodeIsSource ? option.OtherNode.Id : selectedNode.Id;

                    var edge = new BoardEdgeModel(option.Definition.TypeId, sourceId, targetId);
                    edge.Label = option.Definition.GetLabel(edge, _boardAsset.Document);
                    _commandStack.Execute(new CreateEdgeCommand(_boardAsset, edge));
                })
                {
                    text = "Create Link"
                };
                createButton.AddToClassList("pd-primary-button");
                createButton.AddToClassList("pd-link-create-button");
                connectionsFoldout.Add(createButton);
            }

            foreach (BoardEdgeModel edge in _boardAsset.Document.Edges.Where(item => item.SourceNodeId == selectedNode.Id || item.TargetNodeId == selectedNode.Id))
            {
                string relatedNodeId = edge.SourceNodeId == selectedNode.Id ? edge.TargetNodeId : edge.SourceNodeId;
                BoardNodeModel relatedNode = _boardAsset.Document.GetNode(relatedNodeId);
                if (relatedNode == null)
                {
                    continue;
                }

                IProjectDesignerEdgeDefinition edgeDefinition = ProjectDesignerRegistry.GetEdgeDefinition(edge.TypeId);
                string edgeLabel = edgeDefinition == null ? edge.TypeId : edgeDefinition.DisplayName;
                var row = new VisualElement();
                row.AddToClassList("pd-edge-row");
                string edgeId = edge.Id;
                row.RegisterCallback<MouseEnterEvent>(_ =>
                {
                    row.EnableInClassList("pd-edge-row-hovered", true);
                    _canvasView.SetHoveredEdge(edgeId);
                });
                row.RegisterCallback<MouseLeaveEvent>(_ =>
                {
                    row.EnableInClassList("pd-edge-row-hovered", false);
                    _canvasView.SetHoveredEdge(string.Empty);
                });
                row.Add(new Label(relatedNode.Title + " [" + edgeLabel + "]"));

                var anchorRow = new VisualElement();
                anchorRow.AddToClassList("pd-edge-anchor-row");
                anchorRow.Add(CreateAnchorPopup("Source", edge.SourceAnchor, value => SetEdgeAnchors(edge.Id, value, edge.TargetAnchor)));
                anchorRow.Add(CreateAnchorPopup("Target", edge.TargetAnchor, value => SetEdgeAnchors(edge.Id, edge.SourceAnchor, value)));
                row.Add(anchorRow);

                var removeButton = new Button(() => _commandStack.Execute(new DeleteEdgeCommand(_boardAsset, edge.Id)))
                {
                    text = "Remove"
                };
                removeButton.AddToClassList("pd-secondary-button");
                row.Add(removeButton);
                connectionsFoldout.Add(row);
            }
        }

        private PopupField<BoardEdgeAnchor> CreateAnchorPopup(string label, BoardEdgeAnchor anchor, Action<BoardEdgeAnchor> onChanged)
        {
            var popup = new PopupField<BoardEdgeAnchor>(
                label,
                Enum.GetValues(typeof(BoardEdgeAnchor)).Cast<BoardEdgeAnchor>().ToList(),
                anchor);
            popup.tooltip = label + " edge anchor";
            popup.AddToClassList("pd-edge-anchor-field");
            popup.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
            return popup;
        }

        private void SetEdgeAnchors(string edgeId, BoardEdgeAnchor sourceAnchor, BoardEdgeAnchor targetAnchor)
        {
            _commandStack.Execute(new SetEdgeAnchorsCommand(_boardAsset, edgeId, sourceAnchor, targetAnchor));
        }

        private static string GetLinkDescription(string typeId)
        {
            switch (typeId)
            {
                case BoardEdgeTypeIds.Dependency:
                    return "Dependency: task A needs task B to finish first.";
                case BoardEdgeTypeIds.Milestone:
                    return "Milestone Link: counts a task under a milestone or deliverable for milestone progress.";
                case BoardEdgeTypeIds.Reference:
                    return "Reference Link: connects supporting context, source material, or notes without affecting progress.";
                case BoardEdgeTypeIds.TechnicalRelation:
                    return "Technical Relation: class or architecture cards are related.";
                default:
                    return "Create a relationship between two cards.";
            }
        }

        private void CreateNode(IProjectDesignerNodeDefinition definition)
        {
            BoardNodeModel selectedNode = _boardAsset.Document.GetNode(_boardAsset.Document.ViewState.SelectedNodeId);
            Vector2 spawnPosition = ProjectDesignerSpawnUtility.GetCreatePosition(
                _canvasView.GetViewportCenterOnBoard(),
                selectedNode,
                _createSequence++);
            BoardNodeModel node = definition.CreateDefaultNode(spawnPosition);
            _commandStack.Execute(new CreateNodeCommand(_boardAsset, node));
            _boardAsset.Document.ViewState.SelectSingle(node.Id);
            PersistBoard();
            RefreshInspector();
            _canvasView.RefreshSelection();
        }

        private void CreateNodeAtPosition(IProjectDesignerNodeDefinition definition, Vector2 boardPosition)
        {
            if (definition == null)
            {
                return;
            }

            Vector2 spawnPosition = _boardAsset.Document.ViewState.SnapToGrid
                ? BoardLayoutUtility.SnapPosition(boardPosition)
                : boardPosition;
            BoardNodeModel node = definition.CreateDefaultNode(spawnPosition);
            _commandStack.Execute(new CreateNodeCommand(_boardAsset, node));
            _boardAsset.Document.ViewState.SelectSingle(node.Id);
            PersistBoard();
            RefreshInspector();
            _canvasView.RefreshSelection();
        }

        private void OnSelectionChanged()
        {
            PersistBoard();
            RefreshInspector();
            _canvasView.RefreshSelection();
        }

        private void UpdateInspectorPanelState(List<BoardNodeModel> selectedNodes)
        {
            string expandedSummary = BuildFullInspectorSummary(selectedNodes);
            _inspectorSummaryLabel.text = BuildInspectorSummaryLabel(_inspectorExpanded, selectedNodes);
            _inspectorSummaryLabel.tooltip = _inspectorExpanded ? expandedSummary : "Details panel";
            _inspectorPanel.EnableInClassList("pd-inspector-panel-collapsed", !_inspectorExpanded);
            _inspectorSplitter.style.display = _inspectorExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            _inspectorToggleButton.text = _inspectorExpanded ? "Hide" : "Show";
            UpdateInspectorPanelWidth();
        }

        private void UpdateInspectorPanelWidth()
        {
            float width = _inspectorExpanded ? _inspectorWidth : InspectorCollapsedWidth;
            _inspectorPanel.style.width = width;
            _inspectorPanel.style.minWidth = width;
            _inspectorPanel.style.maxWidth = width;
        }

        private void OnInspectorSplitterPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0 || !_inspectorExpanded)
            {
                return;
            }

            _resizingInspector = true;
            _inspectorResizePointerId = evt.pointerId;
            _inspectorResizeStartMouseX = evt.position.x;
            _inspectorResizeStartWidth = _inspectorWidth;
            PointerCaptureHelper.CapturePointer(_inspectorSplitter, evt.pointerId);
            evt.StopPropagation();
        }

        private void OnInspectorSplitterPointerMove(PointerMoveEvent evt)
        {
            if (!_resizingInspector || evt.pointerId != _inspectorResizePointerId)
            {
                return;
            }

            float delta = evt.position.x - _inspectorResizeStartMouseX;
            _inspectorWidth = Mathf.Clamp(_inspectorResizeStartWidth - delta, InspectorMinWidth, InspectorMaxWidth);
            UpdateInspectorPanelWidth();
            evt.StopPropagation();
        }

        private void OnInspectorSplitterPointerUp(PointerUpEvent evt)
        {
            if (!_resizingInspector || evt.pointerId != _inspectorResizePointerId)
            {
                return;
            }

            FinishInspectorResize();
            evt.StopPropagation();
        }

        private void OnInspectorSplitterCaptureOut(PointerCaptureOutEvent evt)
        {
            FinishInspectorResize();
        }

        private void FinishInspectorResize()
        {
            if (!_resizingInspector)
            {
                return;
            }

            int pointerId = _inspectorResizePointerId;
            _resizingInspector = false;
            _inspectorResizePointerId = -1;
            SessionState.SetFloat(InspectorWidthSessionKey, _inspectorWidth);
            if (pointerId >= 0 && PointerCaptureHelper.HasPointerCapture(_inspectorSplitter, pointerId))
            {
                PointerCaptureHelper.ReleasePointer(_inspectorSplitter, pointerId);
            }
        }

        private void ToggleInspectorExpanded(bool persist)
        {
            SetInspectorExpanded(!_inspectorExpanded, persist);
            RefreshInspector();
        }

        private void SetInspectorExpanded(bool expanded, bool persist)
        {
            _inspectorExpanded = expanded;
            if (persist)
            {
                SessionState.SetBool(InspectorExpandedSessionKey, expanded);
            }
        }

        private static string BuildInspectorSummary(List<BoardNodeModel> selectedNodes)
        {
            return TruncateShellLabel(BuildFullInspectorSummary(selectedNodes), InspectorSummaryMaxLength);
        }

        private static string BuildFullInspectorSummary(List<BoardNodeModel> selectedNodes)
        {
            if (selectedNodes == null || selectedNodes.Count == 0)
            {
                return "Board";
            }

            if (selectedNodes.Count == 1)
            {
                return selectedNodes[0].Title;
            }

            return selectedNodes.Count + " cards";
        }

        private void UpdateLibraryPanelState()
        {
            _librarySummaryLabel.text = BuildLibrarySummaryLabel(_libraryExpanded);
            _librarySummaryLabel.tooltip = _libraryExpanded ? "Add cards and saved views" : "Library";
            _libraryPanel.EnableInClassList("pd-sidebar-panel-collapsed", !_libraryExpanded);
            _libraryToggleButton.text = _libraryExpanded ? "Hide" : "Show";
        }

        private void ToggleLibraryExpanded(bool persist)
        {
            _libraryExpanded = !_libraryExpanded;
            if (persist)
            {
                SessionState.SetBool(LibraryExpandedSessionKey, _libraryExpanded);
            }

            RefreshLibrary();
        }

        internal void ToggleLibraryPanel()
        {
            ToggleLibraryExpanded(true);
        }

        internal void ToggleDetailsPanel()
        {
            ToggleInspectorExpanded(true);
        }

        internal void UndoAction()
        {
            _commandStack.Undo();
        }

        internal void RedoAction()
        {
            _commandStack.Redo();
        }

        internal void SaveCurrentView()
        {
            SaveCurrentFilter();
        }

        internal void ClearCurrentView()
        {
            ClearCurrentFilter();
        }

        internal void FrameAll()
        {
            _canvasView.FrameAll();
        }

        internal void DeleteSelectedNode()
        {
            List<string> selectedNodeIds = _boardAsset.Document.ViewState.SelectedNodeIds.ToList();
            if (selectedNodeIds.Count == 0)
            {
                return;
            }

            _boardAsset.Document.ViewState.ClearSelection();
            if (selectedNodeIds.Count == 1)
            {
                _commandStack.Execute(new DeleteNodeCommand(_boardAsset, selectedNodeIds[0]));
                return;
            }

            _commandStack.Execute(new DeleteNodesCommand(_boardAsset, selectedNodeIds));
        }

        private void SaveCurrentFilter()
        {
            BoardSavedFilter filter = CreateSavedFilterFromCurrentView(BuildDefaultSavedViewName(), null);

            _commandStack.Execute(new SaveFilterCommand(_boardAsset, filter));
            ApplySavedFilter(filter);
        }

        private void ApplySavedFilter(BoardSavedFilter filter)
        {
            if (filter == null)
            {
                return;
            }

            var viewState = _boardAsset.Document.ViewState.Clone();
            viewState.ActiveFilterId = filter.Id;
            viewState.SearchQuery = filter.SearchQuery;
            viewState.Category = filter.Category;
            viewState.QuickFilterId = filter.QuickFilterId;
            _commandStack.Execute(new SetFilterStateCommand(_boardAsset, viewState));
        }

        private void ClearCurrentFilter()
        {
            _boardAsset.Document.ViewState.ActiveFilterId = string.Empty;
            _boardAsset.Document.ViewState.SearchQuery = string.Empty;
            _boardAsset.Document.ViewState.Category = BoardNodeCategories.All;
            _boardAsset.Document.ViewState.QuickFilterId = string.Empty;
            _searchField.value = string.Empty;
            _categoryField.value = BoardNodeCategories.All;
            PersistBoard();
            RefreshAll();
        }

        private bool CanClearCurrentFilter()
        {
            BoardViewState viewState = _boardAsset.Document.ViewState;
            return !string.IsNullOrWhiteSpace(viewState.ActiveFilterId) ||
                   !string.IsNullOrWhiteSpace(viewState.SearchQuery) ||
                   !string.Equals(viewState.Category, BoardNodeCategories.All, StringComparison.Ordinal) ||
                   !string.IsNullOrWhiteSpace(viewState.QuickFilterId);
        }

        private void ReplaceBoardWithTemplate(BoardTemplateDefinition template)
        {
            _boardAsset.ResetDocument(BoardPresetFactory.Create(template.Id, _boardAsset.Document.BoardName));
            PersistBoard();
            RefreshAll();
        }

        private void PersistBoard()
        {
            ProjectDesignerBoardUtility.MarkDirty(_boardAsset);
        }

        private void HandleKeyDown(KeyDownEvent evt)
        {
            if (evt.ctrlKey && evt.keyCode == KeyCode.Z)
            {
                _commandStack.Undo();
                evt.StopPropagation();
                return;
            }

            if (evt.ctrlKey && evt.keyCode == KeyCode.A)
            {
                SelectAllVisibleNodes();
                evt.StopPropagation();
                return;
            }

            if (evt.ctrlKey && evt.keyCode == KeyCode.D)
            {
                DuplicateSelection();
                evt.StopPropagation();
                return;
            }

            if ((evt.ctrlKey && evt.keyCode == KeyCode.Y) || (evt.ctrlKey && evt.shiftKey && evt.keyCode == KeyCode.Z))
            {
                _commandStack.Redo();
                evt.StopPropagation();
                return;
            }

            if (!evt.ctrlKey && evt.keyCode == KeyCode.F)
            {
                FocusSelection();
                evt.StopPropagation();
                return;
            }

            if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace)
            {
                DeleteSelectedNode();
                evt.StopPropagation();
                return;
            }

            if (evt.keyCode == KeyCode.Escape && _boardAsset.Document.ViewState.SelectedNodeIds.Count > 0)
            {
                _boardAsset.Document.ViewState.ClearSelection();
                PersistBoard();
                RefreshInspector();
                _canvasView.RefreshSelection();
                evt.StopPropagation();
            }
        }

        private static Label CreateSectionLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("pd-section-title");
            return label;
        }

        private static Label CreateSubsectionLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("pd-subsection-title");
            return label;
        }

        private static Label CreateMutedBodyLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("pd-muted-body");
            return label;
        }

        private static Label CreateActiveViewLabel(string activeViewName)
        {
            string safeName = string.IsNullOrWhiteSpace(activeViewName) ? "Saved View" : activeViewName.Trim();
            var label = new Label("Active View: " + safeName);
            label.AddToClassList("pd-filter-active-label");
            label.tooltip = safeName;
            return label;
        }

        private static Button CreateLibraryActionButton(string text, Action onClick, bool primary)
        {
            var button = new Button(onClick)
            {
                text = text
            };
            button.AddToClassList(primary ? "pd-primary-button" : "pd-secondary-button");
            return button;
        }

        private VisualElement CreateSavedViewEntry(BoardSavedFilter filter, Button applyButton)
        {
            int matchCount = GetSavedViewMatchCount(filter);

            var entry = new VisualElement();
            entry.Add(applyButton);
            entry.Add(CreateMutedBodyLabel(BuildSavedViewSummaryText(filter, matchCount)));
            return entry;
        }

        private VisualElement CreateLibraryGroup(IGrouping<string, IProjectDesignerNodeDefinition> group)
        {
            bool defaultOpen = group.Key != BoardNodeCategories.TechnicalDesign;
            Foldout foldout = CreateFoldout(group.Key, defaultOpen);
            if (string.Equals(group.Key, BoardNodeCategories.TechnicalDesign, StringComparison.Ordinal))
            {
                foldout.AddToClassList("pd-library-group-technical");
            }

            foreach (IProjectDesignerNodeDefinition definition in group)
            {
                foldout.Add(CreateNodeLibraryButton(definition));
            }

            return foldout;
        }

        private Button CreateNodeLibraryButton(IProjectDesignerNodeDefinition definition)
        {
            var button = new Button(() => CreateNode(definition))
            {
                text = definition.DisplayName
            };
            button.AddToClassList("pd-node-library-button");

            Color accent = ProjectDesignerColorUtility.ParseOrFallback(definition.AccentColor, new Color(0.43f, 0.43f, 0.43f));
            button.style.backgroundColor = ProjectDesignerColorUtility.WithAlpha(accent, 0.08f);
            button.style.borderLeftWidth = 4f;
            button.style.borderLeftColor = accent;
            button.style.color = ProjectDesignerColorUtility.Blend(accent, new Color(0.18f, 0.22f, 0.26f), 0.36f);
            return button;
        }

        private static Foldout CreateFoldout(string text, bool defaultOpen)
        {
            var foldout = new Foldout
            {
                text = text,
                value = defaultOpen
            };
            foldout.AddToClassList("pd-foldout");
            return foldout;
        }

        private Foldout CreatePersistentInspectorFoldout(string stateKeySuffix, string text, bool defaultOpen)
        {
            string sessionKey = InspectorFoldoutSessionKeyPrefix + stateKeySuffix;
            Foldout foldout = CreateFoldout(text, SessionState.GetBool(sessionKey, defaultOpen));
            foldout.RegisterValueChangedCallback(evt => SessionState.SetBool(sessionKey, evt.newValue));
            return foldout;
        }

        private static ToolbarButton CreateToolbarButton(string text, Action onClick)
        {
            var button = new ToolbarButton(onClick)
            {
                text = text
            };
            button.AddToClassList("pd-toolbar-button");
            button.style.width = ProjectDesignerProductInfo.ToolbarButtonWidth;
            button.style.minHeight = 28f;
            button.style.height = 28f;
            button.style.maxHeight = 28f;
            button.style.marginTop = 5f;
            button.style.marginBottom = 5f;
            button.style.alignSelf = Align.Center;
            return button;
        }

        private ToolbarMenu CreateArrangeMenu()
        {
            var menu = new ToolbarMenu
            {
                text = "Arrange"
            };
            menu.AddToClassList("pd-toolbar-button");
            menu.AddToClassList("pd-toolbar-menu");
            menu.style.width = ProjectDesignerProductInfo.ToolbarButtonWidth;
            menu.style.minHeight = 28f;
            menu.style.height = 28f;
            menu.style.maxHeight = 28f;
            menu.style.marginTop = 5f;
            menu.style.marginBottom = 5f;
            menu.style.alignSelf = Align.Center;

            menu.menu.AppendAction("Auto Layout Left To Right", _ => AutoLayoutSelectionOrVisible());
            menu.menu.AppendSeparator();
            menu.menu.AppendAction("Align Left", _ => ArrangeSelection(BoardArrangeMode.AlignLeft));
            menu.menu.AppendAction("Align Center", _ => ArrangeSelection(BoardArrangeMode.AlignCenter));
            menu.menu.AppendAction("Align Right", _ => ArrangeSelection(BoardArrangeMode.AlignRight));
            menu.menu.AppendSeparator();
            menu.menu.AppendAction("Align Top", _ => ArrangeSelection(BoardArrangeMode.AlignTop));
            menu.menu.AppendAction("Align Middle", _ => ArrangeSelection(BoardArrangeMode.AlignMiddle));
            menu.menu.AppendAction("Align Bottom", _ => ArrangeSelection(BoardArrangeMode.AlignBottom));
            menu.menu.AppendSeparator();
            menu.menu.AppendAction("Distribute Horizontal", _ => ArrangeSelection(BoardArrangeMode.DistributeHorizontal));
            menu.menu.AppendAction("Distribute Vertical", _ => ArrangeSelection(BoardArrangeMode.DistributeVertical));
            menu.menu.AppendSeparator();
            menu.menu.AppendAction("Frame All", _ => _canvasView.FrameAll());
            return menu;
        }

        private void BuildCanvasContextMenu(ContextualMenuPopulateEvent evt)
        {
            string nodeId = _canvasView.GetNodeIdFromTarget(evt.target);
            if (!string.IsNullOrEmpty(nodeId))
            {
                EnsureContextNodeSelection(nodeId);
                PopulateNodeContextMenu(evt.menu, nodeId);
                return;
            }

            PopulateCanvasContextMenu(evt.menu, _canvasView.GetBoardPositionFromCanvas(evt.localMousePosition));
        }

        private void EnsureContextNodeSelection(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                return;
            }

            if (_boardAsset.Document.ViewState.SelectedNodeIds.Contains(nodeId))
            {
                return;
            }

            _boardAsset.Document.ViewState.SelectSingle(nodeId);
            PersistBoard();
            RefreshInspector();
            _canvasView.RefreshSelection();
        }

        private void PopulateNodeContextMenu(DropdownMenu menu, string contextNodeId)
        {
            AppendAction(menu, "Duplicate", _ => DuplicateSelection(), HasSelection());
            AppendAction(menu, "Delete Selected", _ => DeleteSelectedNode(), HasSelection());
            AppendAction(menu, "Focus Selected", _ => FocusSelection(), HasSelection());
            AppendAction(menu, "Frame Selection", _ => FrameSelection(), HasSelection());

            menu.AppendSeparator();
            AppendCreateLinkActions(menu, _boardAsset.Document.GetNode(contextNodeId));
            AppendReferenceContextActions(menu, contextNodeId);
            AppendAction(menu, "Arrange/Auto Layout Left To Right", _ => AutoLayoutSelectionOrVisible(), HasSelection());
            AppendArrangeActions(menu, GetSelectedNodes().Count > 1);
            menu.AppendSeparator();
            AppendAction(menu, _libraryExpanded ? "Hide Library" : "Show Library", _ => ToggleLibraryExpanded(true), true);
            AppendAction(menu, _inspectorExpanded ? "Hide Details" : "Show Details", _ => ToggleInspectorExpanded(true), true);
        }

        private void AppendCreateLinkActions(DropdownMenu menu, BoardNodeModel selectedNode)
        {
            if (selectedNode == null)
            {
                return;
            }

            List<ProjectDesignerLinkOption> options = ProjectDesignerLinkUtility.GetLinkOptions(_boardAsset.Document, selectedNode);
            if (options.Count == 0)
            {
                AppendAction(menu, "Create Link/No valid targets", _ => { }, false);
                return;
            }

            foreach (ProjectDesignerLinkOption option in options)
            {
                ProjectDesignerLinkOption localOption = option;
                string targetTitle = option.OtherNode == null ? "Card" : SanitizeMenuPathPart(option.OtherNode.Title);
                string path = "Create Link/" + SanitizeMenuPathPart(option.Definition.DisplayName) + "/" + targetTitle;
                AppendAction(menu, path, _ => CreateEdgeFromLinkOption(localOption, selectedNode.Id), true);
            }
        }

        private void CreateEdgeFromLinkOption(ProjectDesignerLinkOption option, string selectedNodeId)
        {
            if (option == null || option.Definition == null || option.OtherNode == null)
            {
                return;
            }

            string sourceId = option.SelectedNodeIsSource ? selectedNodeId : option.OtherNode.Id;
            string targetId = option.SelectedNodeIsSource ? option.OtherNode.Id : selectedNodeId;
            var edge = new BoardEdgeModel(option.Definition.TypeId, sourceId, targetId);
            edge.Label = option.Definition.GetLabel(edge, _boardAsset.Document);
            _commandStack.Execute(new CreateEdgeCommand(_boardAsset, edge));
        }

        private static string SanitizeMenuPathPart(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "Card"
                : value.Trim().Replace("/", "-").Replace("\\", "-");
        }

        private void AppendReferenceContextActions(DropdownMenu menu, string contextNodeId)
        {
            ReferenceNodeModel referenceNode = GetContextReferenceNode(contextNodeId);
            if (referenceNode == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(referenceNode.ExternalUrl))
            {
                string url = referenceNode.ExternalUrl.Trim();
                AppendAction(menu, "Reference/Open Link", _ => Application.OpenURL(url), true);
            }

            string assetPath = GetReferenceAssetPath(referenceNode);
            UnityEngine.Object asset = string.IsNullOrWhiteSpace(assetPath)
                ? null
                : AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (asset != null)
            {
                AppendAction(menu, "Reference/Select Asset", _ => SelectReferenceAsset(asset), true);
            }
        }

        private ReferenceNodeModel GetContextReferenceNode(string contextNodeId)
        {
            ReferenceNodeModel referenceNode = _boardAsset.Document.GetNode(contextNodeId) as ReferenceNodeModel;
            if (referenceNode != null)
            {
                return referenceNode;
            }

            List<BoardNodeModel> selectedNodes = GetSelectedNodes();
            if (selectedNodes.Count == 1)
            {
                return selectedNodes[0] as ReferenceNodeModel;
            }

            return null;
        }

        private static string GetReferenceAssetPath(ReferenceNodeModel referenceNode)
        {
            if (referenceNode == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(referenceNode.AssetPath))
            {
                return referenceNode.AssetPath.Trim();
            }

            if (!string.IsNullOrWhiteSpace(referenceNode.ImageAssetPath))
            {
                return referenceNode.ImageAssetPath.Trim();
            }

            return string.Empty;
        }

        private static void SelectReferenceAsset(UnityEngine.Object asset)
        {
            if (asset == null)
            {
                return;
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private void PopulateCanvasContextMenu(DropdownMenu menu, Vector2 boardPosition)
        {
            AppendReferenceContextActions(menu, string.Empty);

            foreach (IGrouping<string, IProjectDesignerNodeDefinition> group in ProjectDesignerRegistry.GetNodeDefinitions()
                         .OrderBy(definition => GetCategorySortOrder(definition.Category))
                         .ThenBy(definition => definition.DisplayName)
                         .GroupBy(definition => definition.Category))
            {
                foreach (IProjectDesignerNodeDefinition definition in group)
                {
                    IProjectDesignerNodeDefinition localDefinition = definition;
                    AppendAction(menu,
                        "Add Card/" + group.Key + "/" + localDefinition.DisplayName,
                        _ => CreateNodeAtPosition(localDefinition, boardPosition),
                        true);
                }
            }

            menu.AppendSeparator();
            AppendAction(menu, "Undo", _ => _commandStack.Undo(), _commandStack.CanUndo);
            AppendAction(menu, "Redo", _ => _commandStack.Redo(), _commandStack.CanRedo);
            AppendAction(menu, "Select All Visible", _ => SelectAllVisibleNodes(), true);
            AppendAction(menu, "Delete Selected", _ => DeleteSelectedNode(), HasSelection());
            AppendAction(menu, "Focus Selected", _ => FocusSelection(), HasSelection());
            AppendAction(menu, "Frame All", _ => _canvasView.FrameAll(), true);
            if (_boardAsset.Document.ViewState.SelectedNodeIds.Count == 1)
            {
                AppendCreateLinkActions(menu, _boardAsset.Document.GetNode(_boardAsset.Document.ViewState.SelectedNodeId));
            }
            AppendAction(menu, "Arrange/Auto Layout Left To Right", _ => AutoLayoutSelectionOrVisible(), _boardAsset.Document.Nodes.Count > 1);
            AppendAction(menu,
                _boardAsset.Document.ViewState.SnapToGrid ? "Snap To Grid/Turn Off" : "Snap To Grid/Turn On",
                _ => ToggleSnapToGrid(),
                true);
            menu.AppendSeparator();
            AppendAction(menu, "Save View", _ => SaveCurrentFilter(), true);
            AppendAction(menu, "Clear View", _ => ClearCurrentFilter(), true);
            AppendAction(menu, _libraryExpanded ? "Hide Library" : "Show Library", _ => ToggleLibraryExpanded(true), true);
            AppendAction(menu, _inspectorExpanded ? "Hide Details" : "Show Details", _ => ToggleInspectorExpanded(true), true);
        }

        private void AppendArrangeActions(DropdownMenu menu, bool enabled)
        {
            AppendAction(menu, "Arrange/Align Left", _ => ArrangeSelection(BoardArrangeMode.AlignLeft), enabled);
            AppendAction(menu, "Arrange/Align Center", _ => ArrangeSelection(BoardArrangeMode.AlignCenter), enabled);
            AppendAction(menu, "Arrange/Align Right", _ => ArrangeSelection(BoardArrangeMode.AlignRight), enabled);
            AppendAction(menu, "Arrange/Align Top", _ => ArrangeSelection(BoardArrangeMode.AlignTop), enabled);
            AppendAction(menu, "Arrange/Align Middle", _ => ArrangeSelection(BoardArrangeMode.AlignMiddle), enabled);
            AppendAction(menu, "Arrange/Align Bottom", _ => ArrangeSelection(BoardArrangeMode.AlignBottom), enabled);
            AppendAction(menu, "Arrange/Distribute Horizontal", _ => ArrangeSelection(BoardArrangeMode.DistributeHorizontal), enabled);
            AppendAction(menu, "Arrange/Distribute Vertical", _ => ArrangeSelection(BoardArrangeMode.DistributeVertical), enabled);
        }

        private static void AppendAction(DropdownMenu menu, string path, Action<DropdownMenuAction> action, bool enabled)
        {
            menu.AppendAction(
                path,
                action,
                _ => enabled ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
        }

        private static int GetCategorySortOrder(string category)
        {
            if (string.Equals(category, BoardNodeCategories.Planning, StringComparison.Ordinal))
            {
                return 0;
            }

            if (string.Equals(category, BoardNodeCategories.Reference, StringComparison.Ordinal))
            {
                return 1;
            }

            if (string.Equals(category, BoardNodeCategories.TechnicalDesign, StringComparison.Ordinal))
            {
                return 2;
            }

            return 3;
        }

        internal static string BuildToolbarTitle(string boardName)
        {
            return TruncateShellLabel(string.IsNullOrWhiteSpace(boardName) ? "Planning Board" : boardName, ToolbarTitleMaxLength);
        }

        internal static bool IsSavedViewActive(string activeFilterId, BoardSavedFilter filter)
        {
            return filter != null &&
                   !string.IsNullOrWhiteSpace(activeFilterId) &&
                   string.Equals(activeFilterId, filter.Id, StringComparison.Ordinal);
        }

        internal static string BuildSavedViewButtonText(string filterName, bool isActive)
        {
            string name = string.IsNullOrWhiteSpace(filterName) ? "Saved View" : filterName.Trim();
            return name;
        }

        private string BuildSavedViewTooltip(BoardSavedFilter filter, bool isActive)
        {
            if (filter == null)
            {
                return string.Empty;
            }

            string tooltip = filter.Name;
            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                tooltip += "\nSearch: " + filter.SearchQuery;
            }

            if (!string.IsNullOrWhiteSpace(filter.Category) &&
                !string.Equals(filter.Category, BoardNodeCategories.All, StringComparison.Ordinal))
            {
                tooltip += "\nCategory: " + filter.Category;
            }

            if (!string.IsNullOrWhiteSpace(filter.QuickFilterId))
            {
                tooltip += "\nQuick Filter: " + DescribeQuickFilter(filter.QuickFilterId);
            }

            if (isActive)
            {
                tooltip += "\nCurrently applied";
            }

            return tooltip;
        }

        internal static string BuildInspectorSummaryLabel(bool expanded, List<BoardNodeModel> selectedNodes)
        {
            return expanded ? BuildInspectorSummary(selectedNodes) : string.Empty;
        }

        internal static string BuildLibrarySummaryLabel(bool expanded)
        {
            return expanded ? "Add cards and views" : string.Empty;
        }

        internal static string TruncateShellLabel(string text, int maxLength)
        {
            string value = string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim();
            if (maxLength < 4 || value.Length <= maxLength)
            {
                return value;
            }

            return value.Substring(0, maxLength - 1) + "…";
        }

        private int GetSavedViewMatchCount(BoardSavedFilter filter)
        {
            if (filter == null)
            {
                return 0;
            }

            IEnumerable<BoardNodeModel> nodes = _boardAsset.Document.Nodes.Where(node => filter.Matches(node));
            if (!string.IsNullOrWhiteSpace(filter.QuickFilterId))
            {
                nodes = nodes.Where(node => BoardInsights.MatchesQuickFilter(_boardAsset.Document, node, filter.QuickFilterId));
            }

            return nodes.Count();
        }

        private string BuildSavedViewSummaryText(BoardSavedFilter filter, int matchCount)
        {
            if (filter == null)
            {
                return "0 matching cards";
            }

            var parts = new List<string>
            {
                matchCount + " matching card" + (matchCount == 1 ? string.Empty : "s")
            };

            if (!string.IsNullOrWhiteSpace(filter.Category) &&
                !string.Equals(filter.Category, BoardNodeCategories.All, StringComparison.Ordinal))
            {
                parts.Add(filter.Category);
            }

            if (!string.IsNullOrWhiteSpace(filter.QuickFilterId))
            {
                parts.Add("Quick filter: " + DescribeQuickFilter(filter.QuickFilterId));
            }

            if (!string.IsNullOrWhiteSpace(filter.RequiredTag))
            {
                parts.Add("Tag: " + filter.RequiredTag.Trim());
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                parts.Add("Search: " + filter.SearchQuery.Trim());
            }

            return string.Join(" | ", parts);
        }

        private string BuildDefaultSavedViewName()
        {
            var parts = new List<string>();
            BoardViewState viewState = _boardAsset.Document.ViewState;

            if (!string.IsNullOrWhiteSpace(viewState.QuickFilterId))
            {
                parts.Add(DescribeQuickFilter(viewState.QuickFilterId));
            }

            if (!string.IsNullOrWhiteSpace(viewState.Category) &&
                !string.Equals(viewState.Category, BoardNodeCategories.All, StringComparison.Ordinal))
            {
                parts.Add(viewState.Category);
            }

            if (!string.IsNullOrWhiteSpace(viewState.SearchQuery))
            {
                parts.Add("Search: " + TruncateShellLabel(viewState.SearchQuery.Trim(), 20));
            }

            string baseName = parts.Count == 0
                ? "Saved View"
                : string.Join(" | ", parts.Take(2).ToArray());

            return EnsureUniqueSavedViewName(baseName);
        }

        private string EnsureUniqueSavedViewName(string candidate)
        {
            string baseName = string.IsNullOrWhiteSpace(candidate) ? "Saved View" : candidate.Trim();
            HashSet<string> existingNames = new HashSet<string>(
                _boardAsset.Document.SavedFilters
                    .Where(filter => filter != null)
                    .Select(filter => filter.Name),
                StringComparer.OrdinalIgnoreCase);

            if (!existingNames.Contains(baseName))
            {
                return baseName;
            }

            int suffix = 2;
            while (existingNames.Contains(baseName + " " + suffix))
            {
                suffix++;
            }

            return baseName + " " + suffix;
        }

        private BoardSavedFilter CreateSavedFilterFromCurrentView(string name, BoardSavedFilter existingFilter)
        {
            BoardViewState viewState = _boardAsset.Document.ViewState;
            BoardSavedFilter filter = existingFilter == null ? new BoardSavedFilter() : existingFilter.Clone();
            filter.Name = string.IsNullOrWhiteSpace(name) ? BuildDefaultSavedViewName() : name.Trim();
            filter.SearchQuery = viewState.SearchQuery;
            filter.Category = viewState.Category;
            filter.QuickFilterId = viewState.QuickFilterId;

            if (existingFilter == null)
            {
                filter.RequiredTag = string.Empty;
                filter.IncludeTechnicalDesign = true;
            }

            return filter;
        }

        private void UpdateSavedFilterFromCurrentView(BoardSavedFilter filter)
        {
            if (filter == null)
            {
                return;
            }

            BoardSavedFilter updated = CreateSavedFilterFromCurrentView(filter.Name, filter);
            _commandStack.Execute(new SaveFilterCommand(_boardAsset, updated));
            ApplySavedFilter(updated);
        }

        private void RenameSavedFilter(BoardSavedFilter filter, string newName)
        {
            if (filter == null)
            {
                return;
            }

            string trimmedName = string.IsNullOrWhiteSpace(newName) ? filter.Name : newName.Trim();
            if (string.Equals(trimmedName, filter.Name, StringComparison.Ordinal))
            {
                return;
            }

            BoardSavedFilter updated = filter.Clone();
            updated.Name = trimmedName;
            _commandStack.Execute(new SaveFilterCommand(_boardAsset, updated));
        }

        private void DeleteSavedFilter(BoardSavedFilter filter)
        {
            if (filter == null)
            {
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                ProjectDesignerProductInfo.ProductName,
                "Delete the saved view \"" + filter.Name + "\"?",
                "Delete",
                "Cancel");
            if (!confirmed)
            {
                return;
            }

            _commandStack.Execute(BoardMutationCommand.Create(_boardAsset, "Delete Saved View", document =>
            {
                document.RemoveFilter(filter.Id);
                if (string.Equals(document.ViewState.ActiveFilterId, filter.Id, StringComparison.Ordinal))
                {
                    document.ViewState.ActiveFilterId = string.Empty;
                }
            }));
        }

        private BoardSavedFilter GetActiveSavedFilter()
        {
            string activeFilterId = _boardAsset.Document.ViewState.ActiveFilterId;
            if (string.IsNullOrWhiteSpace(activeFilterId))
            {
                return null;
            }

            return _boardAsset.Document.SavedFilters
                .FirstOrDefault(filter => IsSavedViewActive(activeFilterId, filter));
        }

        private List<BoardNodeModel> GetSelectedNodes()
        {
            return _boardAsset.Document.ViewState.SelectedNodeIds
                .Select(_boardAsset.Document.GetNode)
                .Where(node => node != null)
                .ToList();
        }

        internal bool HasSelection()
        {
            return _boardAsset.Document.ViewState.SelectedNodeIds.Count > 0;
        }

        internal void SelectAllVisibleNodes()
        {
            List<string> visibleNodeIds = BoardInsights.GetVisibleNodes(_boardAsset.Document)
                .Select(node => node.Id)
                .ToList();

            _boardAsset.Document.ViewState.SetSelection(visibleNodeIds, visibleNodeIds.FirstOrDefault());
            PersistBoard();
            RefreshInspector();
            _canvasView.RefreshSelection();
        }

        internal void DuplicateSelection()
        {
            List<string> selectedIds = _boardAsset.Document.ViewState.SelectedNodeIds.ToList();
            if (selectedIds.Count == 0)
            {
                return;
            }

            Vector2 offset = new Vector2(48f, 40f);
            _commandStack.Execute(new DuplicateNodesCommand(_boardAsset, selectedIds, offset, _boardAsset.Document.ViewState.SnapToGrid));
        }

        internal void FrameSelection()
        {
            if (_boardAsset.Document.ViewState.SelectedNodeIds.Count == 0)
            {
                _canvasView.FrameAll();
                return;
            }

            _canvasView.FrameSelection();
        }

        internal void FocusSelection()
        {
            if (_boardAsset.Document.ViewState.SelectedNodeIds.Count == 0)
            {
                return;
            }

            _canvasView.FocusSelection();
        }

        internal void ArrangeSelection(BoardArrangeMode arrangeMode)
        {
            List<string> selectedIds = _boardAsset.Document.ViewState.SelectedNodeIds.ToList();
            if (selectedIds.Count < 2)
            {
                return;
            }

            _commandStack.Execute(new ArrangeNodesCommand(_boardAsset, selectedIds, arrangeMode));
        }

        internal void AutoLayoutSelectionOrVisible()
        {
            List<string> targetIds = _boardAsset.Document.ViewState.SelectedNodeIds
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            if (targetIds.Count < 2)
            {
                targetIds = BoardInsights.GetVisibleNodes(_boardAsset.Document)
                    .Select(node => node.Id)
                    .Distinct()
                    .ToList();
            }

            if (targetIds.Count < 2)
            {
                return;
            }

            _commandStack.Execute(new ArrangeNodesCommand(_boardAsset, targetIds, BoardArrangeMode.AutoLayoutLeftToRight));
        }

        internal void ToggleSnapToGrid()
        {
            _boardAsset.Document.ViewState.SnapToGrid = !_boardAsset.Document.ViewState.SnapToGrid;
            PersistBoard();
            RefreshAll();
        }

        internal void ToggleQuickFilter(string filterId)
        {
            string currentFilter = _boardAsset.Document.ViewState.QuickFilterId;
            _boardAsset.Document.ViewState.QuickFilterId = string.Equals(currentFilter, filterId, StringComparison.Ordinal)
                ? string.Empty
                : (filterId ?? string.Empty);
            PersistBoard();
            RefreshAll();
        }

        private static VisualElement CreateActionRow()
        {
            var row = new VisualElement();
            row.AddToClassList("pd-action-row");
            return row;
        }

        private static Button CreateInspectorButton(string text, Action onClick)
        {
            var button = new Button(onClick)
            {
                text = text
            };
            button.AddToClassList("pd-secondary-button");
            return button;
        }

        private void BuildPlannerInsights()
        {
            Foldout workloadFoldout = CreatePersistentInspectorFoldout("Board.Workload", "Workload", false);
            IReadOnlyList<BoardAssigneeSummary> assigneeSummaries = BoardInsights.GetAssigneeSummaries(_boardAsset.Document);
            if (assigneeSummaries.Count == 0)
            {
                workloadFoldout.Add(CreateMutedBodyLabel("Assign tasks from the project-wide team roster to unlock workload summaries."));
            }
            else
            {
                foreach (BoardAssigneeSummary summary in assigneeSummaries.Take(5))
                {
                    string detail = summary.DisplayName + ": " + summary.OpenTaskCount + " open tasks | " + BoardInsights.FormatDuration(summary.TotalEstimateMinutes);
                    if (summary.BlockedTaskCount > 0)
                    {
                        detail += " | " + summary.BlockedTaskCount + " blocked";
                    }

                    if (summary.OverdueTaskCount > 0)
                    {
                        detail += " | " + summary.OverdueTaskCount + " overdue";
                    }

                    if (summary.HasOverload)
                    {
                        detail += " | heavy load";
                    }

                    workloadFoldout.Add(CreateMutedBodyLabel(detail));
                }

                VisualElement assigneeButtons = CreateActionRow();
                foreach (BoardAssigneeSummary summary in assigneeSummaries.Take(4))
                {
                    assigneeButtons.Add(CreateQuickFilterButton(summary.DisplayName, BoardQuickFilterIds.ForAssigneeId(summary.AssigneeId)));
                }

                workloadFoldout.Add(assigneeButtons);
            }

            workloadFoldout.Add(CreateMutedBodyLabel(BoardInsights.CountUnassignedOpenTasks(_boardAsset.Document) + " unassigned open tasks"));
            _inspectorView.Add(workloadFoldout);

            Foldout riskFoldout = CreatePersistentInspectorFoldout("Board.TimelineAndRisk", "Timeline & Risk", false);
            riskFoldout.Add(CreateMutedBodyLabel(BoardInsights.GetOverdueTasks(_boardAsset.Document).Count() + " overdue tasks"));
            riskFoldout.Add(CreateMutedBodyLabel(BoardInsights.GetDueVerySoonTasks(_boardAsset.Document).Count() + " due very soon tasks"));
            riskFoldout.Add(CreateMutedBodyLabel(BoardInsights.GetDueSoonTasks(_boardAsset.Document).Count() + " due soon tasks"));
            riskFoldout.Add(CreateMutedBodyLabel(BoardInsights.CountAtRiskNodes(_boardAsset.Document) + " at-risk cards"));
            foreach (BoardMilestoneHealthReport report in BoardInsights.GetAtRiskMilestones(_boardAsset.Document).Take(3))
            {
                riskFoldout.Add(CreateMutedBodyLabel(report.Milestone.Title + ": " + DescribeMilestoneHealth(report)));
            }

            VisualElement riskButtons = CreateActionRow();
            riskButtons.Add(CreateQuickFilterButton("Blocked", BoardQuickFilterIds.Blocked));
            riskButtons.Add(CreateQuickFilterButton("Due Soon", BoardQuickFilterIds.DueSoon));
            riskButtons.Add(CreateQuickFilterButton("Overdue", BoardQuickFilterIds.Overdue));
            riskButtons.Add(CreateQuickFilterButton("At Risk", BoardQuickFilterIds.AtRisk));
            riskFoldout.Add(riskButtons);
            _inspectorView.Add(riskFoldout);

            Foldout dependencyFoldout = CreatePersistentInspectorFoldout("Board.Dependencies", "Dependencies", false);
            dependencyFoldout.Add(CreateMutedBodyLabel(BoardInsights.CountUnresolvedDependencyLinks(_boardAsset.Document) + " unresolved dependency links"));
            dependencyFoldout.Add(CreateMutedBodyLabel(BoardInsights.GetBlockedTasks(_boardAsset.Document).Count(task => BoardInsights.HasUnresolvedDependencies(_boardAsset.Document, task)) + " tasks are waiting on other tasks"));
            dependencyFoldout.Add(CreateMutedBodyLabel(BoardInsights.GetTasksBlockingOthers(_boardAsset.Document).Count() + " tasks are blocking downstream work"));
            _inspectorView.Add(dependencyFoldout);
        }

        private Button CreateQuickFilterButton(string text, string filterId)
        {
            Button button = CreateInspectorButton(text, () => ToggleQuickFilter(filterId));
            if (string.Equals(_boardAsset.Document.ViewState.QuickFilterId, filterId, StringComparison.Ordinal))
            {
                button.AddToClassList("pd-overview-card-active");
            }

            return button;
        }

        private string DescribeQuickFilter(string filterId)
        {
            switch (filterId)
            {
                case BoardQuickFilterIds.Tasks:
                    return "Tasks";
                case BoardQuickFilterIds.InProgress:
                    return "In Progress";
                case BoardQuickFilterIds.Blocked:
                    return "Blocked";
                case BoardQuickFilterIds.Overdue:
                    return "Overdue";
                case BoardQuickFilterIds.DueSoon:
                    return "Due Soon";
                case BoardQuickFilterIds.Unassigned:
                    return "Unassigned";
                case BoardQuickFilterIds.AtRisk:
                    return "At Risk";
                case BoardQuickFilterIds.Milestones:
                    return "Milestones";
            }

            if (BoardQuickFilterIds.IsAssigneeFilter(filterId))
            {
                string assigneeId = BoardQuickFilterIds.GetAssigneeId(filterId);
                string displayName = ProjectDesignerTeamRosterResolver.GetDisplayName(ProjectDesignerTeamRosterContext.CurrentRoster, assigneeId);
                return "Assignee: " + displayName;
            }

            return filterId;
        }

        private static string DescribeMilestoneHealth(BoardMilestoneHealthReport report)
        {
            if (report == null)
            {
                return string.Empty;
            }

            switch (report.State)
            {
                case BoardMilestoneHealthState.OffTrack:
                    return "Off track";
                case BoardMilestoneHealthState.AtRisk:
                    return "At risk";
                case BoardMilestoneHealthState.Complete:
                    return "Complete";
                case BoardMilestoneHealthState.NoLinkedTasks:
                    return "Needs linked tasks";
                default:
                    return "On track";
            }
        }
    }
}
