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
        private readonly ProjectBoardAsset _boardAsset;
        private readonly Action<ProjectBoardAsset> _setBoard;
        private readonly ProjectDesignerCommandStack _commandStack;
        private readonly BoardCanvasView _canvasView;
        private readonly ScrollView _libraryView;
        private readonly ScrollView _inspectorView;
        private readonly VisualElement _savedFiltersContainer;
        private readonly ProjectDesignerOverviewView _overviewView;
        private readonly ToolbarSearchField _searchField;
        private readonly PopupField<string> _categoryField;
        private readonly ToolbarButton _snapButton;
        private Label _toolbarTitle;
        private int _createSequence;

        public ProjectDesignerWorkspaceView(ProjectBoardAsset boardAsset, Action<ProjectBoardAsset> setBoard)
        {
            _boardAsset = boardAsset;
            _setBoard = setBoard;

            ProjectDesignerBuiltInRegistration.Register();
            _commandStack = new ProjectDesignerCommandStack(_boardAsset, PersistBoard);
            _commandStack.Changed += RefreshAll;
            _snapButton = CreateToolbarButton("Snap Off", ToggleSnapToGrid);

            AddToClassList("pd-workspace");

            Toolbar toolbar = BuildToolbar();
            Add(toolbar);

            var body = new VisualElement();
            body.AddToClassList("pd-workspace-body");
            Add(body);

            _libraryView = new ScrollView();
            _libraryView.AddToClassList("pd-sidebar");
            body.Add(_libraryView);

            var centerColumn = new VisualElement();
            centerColumn.AddToClassList("pd-center-column");
            body.Add(centerColumn);

            _canvasView = new BoardCanvasView(_boardAsset, _commandStack);
            _canvasView.SelectionChanged += OnSelectionChanged;
            centerColumn.Add(_canvasView);

            _overviewView = new ProjectDesignerOverviewView(ToggleQuickFilter);
            centerColumn.Add(_overviewView);

            _inspectorView = new ScrollView();
            _inspectorView.AddToClassList("pd-inspector");
            body.Add(_inspectorView);

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

            toolbar.Add(CreateToolbarButton("Undo", () => _commandStack.Undo()));
            toolbar.Add(CreateToolbarButton("Redo", () => _commandStack.Redo()));
            toolbar.Add(CreateToolbarButton("Duplicate", DuplicateSelection));
            toolbar.Add(CreateToolbarButton("Frame Sel", FrameSelection));
            toolbar.Add(CreateToolbarButton("Delete", DeleteSelectedNode));
            toolbar.Add(CreateArrangeMenu());
            toolbar.Add(_snapButton);
            toolbar.Add(CreateToolbarButton("Save View", SaveCurrentFilter));
            toolbar.Add(CreateToolbarButton("Clear View", ClearCurrentFilter));
            toolbar.Add(CreateToolbarButton("All Boards", () => _setBoard(null)));
            return toolbar;
        }

        private void RefreshAll()
        {
            _toolbarTitle.text = _boardAsset.Document.BoardName;
            _snapButton.text = _boardAsset.Document.ViewState.SnapToGrid ? "Snap On" : "Snap Off";
            RefreshLibrary();
            RefreshInspector();
            _canvasView.Refresh();
            _overviewView.Refresh(_boardAsset.Document);
        }

        private void RefreshLibrary()
        {
            _libraryView.Clear();

            _libraryView.Add(CreateSectionLabel("Add Cards"));
            _libraryView.Add(CreateMutedBodyLabel("Start with planning cards, add references as you gather material, and only dip into technical design when you need it."));

            foreach (IGrouping<string, IProjectDesignerNodeDefinition> group in ProjectDesignerRegistry.GetNodeDefinitions().GroupBy(definition => definition.Category))
            {
                _libraryView.Add(CreateLibraryGroup(group));
            }

            _libraryView.Add(CreateSectionLabel("Saved Views"));
            _libraryView.Add(_savedFiltersContainer);
            _savedFiltersContainer.Clear();

            if (_boardAsset.Document.SavedFilters.Count == 0)
            {
                _savedFiltersContainer.Add(CreateMutedBodyLabel("Save the current search and category filters to jump back to a view later."));
                return;
            }

            foreach (BoardSavedFilter filter in _boardAsset.Document.SavedFilters)
            {
                BoardSavedFilter localFilter = filter;
                var button = new Button(() => ApplySavedFilter(localFilter))
                {
                    text = localFilter.Name
                };
                button.AddToClassList("pd-filter-button");
                if (string.Equals(localFilter.Category, BoardNodeCategories.TechnicalDesign, StringComparison.Ordinal))
                {
                    button.AddToClassList("pd-filter-button-technical");
                }

                _savedFiltersContainer.Add(button);
            }
        }

        private void RefreshInspector()
        {
            _inspectorView.Clear();

            List<BoardNodeModel> selectedNodes = GetSelectedNodes();
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

            IProjectDesignerInspector inspector = ProjectDesignerRegistry.GetInspector(selectedNode.TypeId);
            if (inspector != null)
            {
                _inspectorView.Add(inspector.BuildInspector(_boardAsset, selectedNode, _commandStack, RefreshAll));
            }

            BuildConnectionsInspector(selectedNode);
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

            Foldout templatesFoldout = CreateFoldout("Starter Layouts", false);
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
            BuildPlannerInsights();
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
            firstRow.Add(CreateInspectorButton("Frame", FrameSelection));
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
            Foldout connectionsFoldout = CreateFoldout("Links", false);
            connectionsFoldout.Add(CreateMutedBodyLabel("Drag from the Link handle on a card for the fastest path, or create relationships manually here when you need more control."));
            _inspectorView.Add(connectionsFoldout);

            List<ProjectDesignerLinkOption> options = ProjectDesignerLinkUtility.GetLinkOptions(_boardAsset.Document, selectedNode);
            if (options.Count == 0)
            {
                connectionsFoldout.Add(new Label("No valid links are available for this card right now."));
                return;
            }

            List<IProjectDesignerEdgeDefinition> availableDefinitions = options
                .Select(option => option.Definition)
                .GroupBy(definition => definition.TypeId)
                .Select(group => group.First())
                .ToList();

            var edgePicker = new PopupField<string>("Link Type", availableDefinitions.Select(definition => definition.DisplayName).ToList(), 0);
            var targetPicker = new PopupField<string>("Linked Card", new List<string>(), 0);
            var directionHint = CreateMutedBodyLabel(string.Empty);
            connectionsFoldout.Add(edgePicker);
            connectionsFoldout.Add(targetPicker);
            connectionsFoldout.Add(directionHint);

            List<ProjectDesignerLinkOption> activeOptions = new List<ProjectDesignerLinkOption>();
            Action refreshTargetPicker = () =>
            {
                IProjectDesignerEdgeDefinition selectedDefinition = availableDefinitions
                    .FirstOrDefault(definition => definition.DisplayName == edgePicker.value);
                activeOptions = options
                    .Where(option => option.Definition.TypeId == (selectedDefinition == null ? string.Empty : selectedDefinition.TypeId))
                    .ToList();

                List<string> choiceLabels = activeOptions.Select(option => option.DisplayLabel).ToList();
                targetPicker.choices = choiceLabels;
                if (choiceLabels.Count > 0)
                {
                    targetPicker.index = 0;
                    targetPicker.SetValueWithoutNotify(choiceLabels[0]);
                }

                bool pointsIntoSelectedNode = activeOptions.Any(option => !option.SelectedNodeIsSource);
                directionHint.text = pointsIntoSelectedNode
                    ? "Some link choices will point into the selected card instead of away from it."
                    : "Only valid link targets are shown here.";
            };

            edgePicker.RegisterValueChangedCallback(evt => refreshTargetPicker());
            refreshTargetPicker();

            var createButton = new Button(() =>
            {
                if (activeOptions.Count == 0 || targetPicker.index < 0 || targetPicker.index >= activeOptions.Count)
                {
                    return;
                }

                ProjectDesignerLinkOption option = activeOptions[targetPicker.index];
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
            connectionsFoldout.Add(createButton);

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
                row.Add(new Label(relatedNode.Title + " [" + edgeLabel + "]"));

                var removeButton = new Button(() => _commandStack.Execute(new DeleteEdgeCommand(_boardAsset, edge.Id)))
                {
                    text = "Remove"
                };
                removeButton.AddToClassList("pd-secondary-button");
                row.Add(removeButton);
                connectionsFoldout.Add(row);
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

        private void OnSelectionChanged()
        {
            PersistBoard();
            RefreshInspector();
            _canvasView.RefreshSelection();
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
            int filterCount = _boardAsset.Document.SavedFilters.Count + 1;
            var filter = new BoardSavedFilter(
                "Filter " + filterCount,
                _boardAsset.Document.ViewState.SearchQuery,
                _boardAsset.Document.ViewState.Category,
                string.Empty,
                true);

            _commandStack.Execute(new SaveFilterCommand(_boardAsset, filter));
            _commandStack.Execute(new SetFilterStateCommand(_boardAsset, new BoardViewState
            {
                PanOffset = _boardAsset.Document.ViewState.PanOffset,
                Zoom = _boardAsset.Document.ViewState.Zoom,
                SearchQuery = filter.SearchQuery,
                Category = filter.Category,
                SelectedNodeId = _boardAsset.Document.ViewState.SelectedNodeId,
                ActiveFilterId = filter.Id
            }));
        }

        private void ApplySavedFilter(BoardSavedFilter filter)
        {
            var viewState = _boardAsset.Document.ViewState.Clone();
            viewState.ActiveFilterId = filter.Id;
            viewState.SearchQuery = filter.SearchQuery;
            viewState.Category = filter.Category;
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
                FrameSelection();
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
            Foldout workloadFoldout = CreateFoldout("Workload", true);
            IReadOnlyList<BoardAssigneeSummary> assigneeSummaries = BoardInsights.GetAssigneeSummaries(_boardAsset.Document);
            if (assigneeSummaries.Count == 0)
            {
                workloadFoldout.Add(CreateMutedBodyLabel("Assign tasks from the project-wide team roster to unlock workload summaries."));
            }
            else
            {
                foreach (BoardAssigneeSummary summary in assigneeSummaries.Take(5))
                {
                    string detail = summary.DisplayName + ": " + summary.OpenTaskCount + " open tasks | " + summary.TotalEstimatePoints + " pts";
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

            Foldout riskFoldout = CreateFoldout("Timeline & Risk", false);
            riskFoldout.Add(CreateMutedBodyLabel(BoardInsights.GetOverdueTasks(_boardAsset.Document).Count() + " overdue tasks"));
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

            Foldout dependencyFoldout = CreateFoldout("Dependencies", false);
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
