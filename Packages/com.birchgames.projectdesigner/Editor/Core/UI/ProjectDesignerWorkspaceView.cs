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
        private Label _toolbarTitle;
        private int _createSequence;

        public ProjectDesignerWorkspaceView(ProjectBoardAsset boardAsset, Action<ProjectBoardAsset> setBoard)
        {
            _boardAsset = boardAsset;
            _setBoard = setBoard;

            ProjectDesignerBuiltInRegistration.Register();
            _commandStack = new ProjectDesignerCommandStack(_boardAsset, PersistBoard);
            _commandStack.Changed += RefreshAll;

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

            _overviewView = new ProjectDesignerOverviewView();
            centerColumn.Add(_overviewView);

            _inspectorView = new ScrollView();
            _inspectorView.AddToClassList("pd-inspector");
            body.Add(_inspectorView);

            _savedFiltersContainer = new VisualElement();
            _savedFiltersContainer.AddToClassList("pd-filter-list");

            _searchField = new ToolbarSearchField();
            _searchField.value = _boardAsset.Document.ViewState.SearchQuery;
            _searchField.tooltip = "Search titles, previews, and tags across the current board.";
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
            _categoryField.tooltip = "Focus the board on a specific category.";
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
            toolbar.Add(CreateToolbarButton("Frame", () =>
            {
                if (_canvasView != null)
                {
                    _canvasView.FrameAll();
                }
            }));
            toolbar.Add(CreateToolbarButton("Delete", DeleteSelectedNode));
            toolbar.Add(CreateToolbarButton("Save View", SaveCurrentFilter));
            toolbar.Add(CreateToolbarButton("Clear View", ClearCurrentFilter));
            toolbar.Add(CreateToolbarButton("All Boards", () => _setBoard(null)));
            return toolbar;
        }

        private void RefreshAll()
        {
            _toolbarTitle.text = _boardAsset.Document.BoardName;
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
                _savedFiltersContainer.Add(button);
            }
        }

        private void RefreshInspector()
        {
            _inspectorView.Clear();

            string selectedNodeId = _boardAsset.Document.ViewState.SelectedNodeId;
            BoardNodeModel selectedNode = _boardAsset.Document.GetNode(selectedNodeId);
            if (selectedNode == null)
            {
                BuildBoardInspector();
                return;
            }

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

            var teamField = new TextField("Team Directory");
            teamField.value = string.Join(", ", _boardAsset.Document.TeamMembers);
            teamField.isDelayed = true;
            teamField.RegisterValueChangedCallback(evt =>
            {
                _commandStack.Execute(BoardMutationCommand.Create(_boardAsset, "Update Team Directory", document =>
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
        }

        private void BuildConnectionsInspector(BoardNodeModel selectedNode)
        {
            Foldout connectionsFoldout = CreateFoldout("Links", false);
            connectionsFoldout.Add(CreateMutedBodyLabel("Create relationships only when they help the board tell a clearer story."));
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
            OnSelectionChanged(node.Id);
        }

        private void OnSelectionChanged(string nodeId)
        {
            _boardAsset.Document.ViewState.SelectedNodeId = nodeId ?? string.Empty;
            PersistBoard();
            RefreshInspector();
            _canvasView.RefreshSelection();
        }

        private void DeleteSelectedNode()
        {
            if (string.IsNullOrEmpty(_boardAsset.Document.ViewState.SelectedNodeId))
            {
                return;
            }

            string selectedNodeId = _boardAsset.Document.ViewState.SelectedNodeId;
            _boardAsset.Document.ViewState.SelectedNodeId = string.Empty;
            _commandStack.Execute(new DeleteNodeCommand(_boardAsset, selectedNodeId));
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

            if ((evt.ctrlKey && evt.keyCode == KeyCode.Y) || (evt.ctrlKey && evt.shiftKey && evt.keyCode == KeyCode.Z))
            {
                _commandStack.Redo();
                evt.StopPropagation();
                return;
            }

            if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace)
            {
                DeleteSelectedNode();
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
            return button;
        }
    }
}
