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

            toolbar.Add(new ToolbarButton(() => _commandStack.Undo()) { text = "Undo" });
            toolbar.Add(new ToolbarButton(() => _commandStack.Redo()) { text = "Redo" });
            toolbar.Add(new ToolbarButton(() =>
            {
                if (_canvasView != null)
                {
                    _canvasView.FrameAll();
                }
            }) { text = "Frame" });
            toolbar.Add(new ToolbarButton(DeleteSelectedNode) { text = "Delete" });
            toolbar.Add(new ToolbarButton(SaveCurrentFilter) { text = "Save Filter" });
            toolbar.Add(new ToolbarButton(ClearCurrentFilter) { text = "Clear Filter" });
            toolbar.Add(new ToolbarButton(() => _setBoard(null)) { text = "Back" });
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

            _libraryView.Add(CreateSectionLabel("Node Library"));
            foreach (IGrouping<string, IProjectDesignerNodeDefinition> group in ProjectDesignerRegistry.GetNodeDefinitions().GroupBy(definition => definition.Category))
            {
                _libraryView.Add(CreateSubsectionLabel(group.Key));
                foreach (IProjectDesignerNodeDefinition definition in group)
                {
                    var button = new Button(() => CreateNode(definition))
                    {
                        text = definition.DisplayName
                    };
                    button.AddToClassList("pd-node-library-button");
                    _libraryView.Add(button);
                }
            }

            _libraryView.Add(CreateSectionLabel("Saved Filters"));
            _libraryView.Add(_savedFiltersContainer);
            _savedFiltersContainer.Clear();

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
            Label title = new Label("Board Details");
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

            _inspectorView.Add(CreateSubsectionLabel("Templates"));
            foreach (BoardTemplateDefinition template in _boardAsset.Document.Templates)
            {
                BoardTemplateDefinition localTemplate = template;
                var button = new Button(() => ReplaceBoardWithTemplate(localTemplate))
                {
                    text = localTemplate.Name
                };
                button.AddToClassList("pd-secondary-button");
                _inspectorView.Add(button);
            }
        }

        private void BuildConnectionsInspector(BoardNodeModel selectedNode)
        {
            _inspectorView.Add(CreateSubsectionLabel("Connections"));

            List<BoardNodeModel> targets = _boardAsset.Document.Nodes
                .Where(node => node != null && node.Id != selectedNode.Id)
                .ToList();

            List<string> edgeOptions = ProjectDesignerRegistry.GetEdgeDefinitions().Select(definition => definition.DisplayName).ToList();
            List<string> targetOptions = targets.Select(node => node.Title + " (" + node.Category + ")").ToList();
            if (edgeOptions.Count == 0 || targetOptions.Count == 0)
            {
                _inspectorView.Add(new Label("Select at least two nodes to create a link."));
                return;
            }

            var edgePicker = new PopupField<string>("Edge Type", edgeOptions, 0);
            var targetPicker = new PopupField<string>("Target", targetOptions, 0);
            _inspectorView.Add(edgePicker);
            _inspectorView.Add(targetPicker);

            var createButton = new Button(() =>
            {
                IProjectDesignerEdgeDefinition edgeDefinition = ProjectDesignerRegistry.GetEdgeDefinitions()
                    .FirstOrDefault(definition => definition.DisplayName == edgePicker.value);
                BoardNodeModel targetNode = targets[targetPicker.index];
                if (edgeDefinition == null || !edgeDefinition.CanConnect(_boardAsset.Document, selectedNode, targetNode))
                {
                    EditorUtility.DisplayDialog("Project Designer+", "That edge type is not valid for this pair of nodes.", "OK");
                    return;
                }

                var edge = new BoardEdgeModel(edgeDefinition.TypeId, selectedNode.Id, targetNode.Id);
                edge.Label = edgeDefinition.GetLabel(edge, _boardAsset.Document);
                _commandStack.Execute(new CreateEdgeCommand(_boardAsset, edge));
            })
            {
                text = "Create Link"
            };
            createButton.AddToClassList("pd-primary-button");
            _inspectorView.Add(createButton);

            foreach (BoardEdgeModel edge in _boardAsset.Document.Edges.Where(item => item.SourceNodeId == selectedNode.Id || item.TargetNodeId == selectedNode.Id))
            {
                string relatedNodeId = edge.SourceNodeId == selectedNode.Id ? edge.TargetNodeId : edge.SourceNodeId;
                BoardNodeModel relatedNode = _boardAsset.Document.GetNode(relatedNodeId);
                if (relatedNode == null)
                {
                    continue;
                }

                var row = new VisualElement();
                row.AddToClassList("pd-edge-row");
                row.Add(new Label(relatedNode.Title + " [" + edge.TypeId + "]"));

                var removeButton = new Button(() => _commandStack.Execute(new DeleteEdgeCommand(_boardAsset, edge.Id)))
                {
                    text = "Remove"
                };
                removeButton.AddToClassList("pd-secondary-button");
                row.Add(removeButton);
                _inspectorView.Add(row);
            }
        }

        private void CreateNode(IProjectDesignerNodeDefinition definition)
        {
            BoardNodeModel node = definition.CreateDefaultNode(_canvasView.GetSuggestedSpawnPosition());
            _commandStack.Execute(new CreateNodeCommand(_boardAsset, node));
            OnSelectionChanged(node.Id);
        }

        private void OnSelectionChanged(string nodeId)
        {
            _boardAsset.Document.ViewState.SelectedNodeId = nodeId ?? string.Empty;
            PersistBoard();
            RefreshInspector();
            _canvasView.Refresh();
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
    }
}
