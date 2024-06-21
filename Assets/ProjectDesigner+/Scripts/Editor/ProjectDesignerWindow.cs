using ProjectDesigner.Core;
using ProjectDesigner.Data;
using ProjectDesigner.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static ProjectDesigner.Core.IDragAndDropHandler;

namespace ProjectDesigner.Editor
{
    [Serializable]
    public class ProjectDesignerWindow : EditorWindow
    {
        private const int PanelSize = 10000;
        private const int ToolbarHeight = 20;
        private const float MinZoomValue = 0.25f;
        private const float MaxZoomValue = 2f;
        public Rect Position
        {
            get
            {
                return new Rect(position.x, position.y + ToolbarHeight, position.width, position.height);
            }
        }

        public static ProjectDesignerWindow Current { get; private set; }

        private ProjectDesignerSettings Settings => ProjectDesignerSettings.Instance;

        private Func<GenericMenu> _genericMenuGetter;

        protected virtual IDragAndDropHandler DragAndDrop => Context.DragAndDropHandler;
        protected virtual IEditorContext Context
        {
            get
            {
                if (_context == null || (_context.Project == null && Project != null))
                {
                    _context = new EditorContext(Project, this);
                }

                return _context;
            }
        }
        private EditorContext _context;
        private IHoverable _cachedHoveredElement;

        [field: SerializeField]
        public Project Project { get; private set; }

        public float Zoom => _zoomFactor;

        [SerializeField]
        private float _zoomFactor = 1f;
        [SerializeField]
        private string _searchQuery = "";
        [SerializeField]
        private Vector2 _topLeft = Vector2.zero;
        [SerializeField]
        private Vector2 _topLeftTarget = Vector2.zero;
        [SerializeField]
        private Color _backgroundColor = new Color(46f / 255f, 46f / 255f, 46f / 255f);
        [SerializeField]
        private Color _gridColor = new Color(119f / 255f, 118f / 255f, 99f / 255f);
        [SerializeField]
        private bool _showGrids = false;
        [SerializeField]
        private bool _debugMode = false;
        [SerializeField]
        private EditorContextHistory _history = new EditorContextHistory();
        [SerializeField]
        private List<NodeBase> _nodeBuffer = new List<NodeBase>();
        [SerializeReference]
        private List<IDrawable> _drawables;
        #region Base
        private void OnFocus()
        {
            Current = this;
            OnWindowGainedFocus();
        }

        protected virtual void OnWindowGainedFocus()
        {

        }

        protected virtual void OnLostFocus()
        {
            Current = null;
            if (_drawables != null)
            {
                ResetHoveredElements();
            }
            OnWindowLostFocus();
        }

        protected virtual void OnWindowLostFocus()
        {

        }

        [MenuItem("Tools/Project Designer/New Project Designer %l", false, priority = 0)]
        public static void CreateWindow()
        {
            Project project = Project.NewDesign("Project");
            CreateWindowInternal(project);
        }

        public static void CreateWindow(Project project)
        {
            ProjectDesignerWindow window = CreateWindowInternal(project);
            window.Overview(project.Drawables);
        }

        private static ProjectDesignerWindow CreateWindowInternal(Project project)
        {
            ProjectDesignerWindow window = CreateWindow<ProjectDesignerWindow>(new[] { typeof(ProjectDesignerWindow) });
            window.Project = project;
            window.titleContent = new GUIContent($"Designer ({project.name})", GUIStyleCollection.GetTexture("project_designer_icon"));
            window.OnWindowCreated();
            window.Show();
            return window;
        }

        protected virtual void OnWindowCreated()
        {
            foreach (var data in TemplateCollection.GetDrawableNodeTypes())
            {
                if (data.AddOnCreation && Context.GetDrawables<NodeBase>(x => x.GetType() == data.Type).Count == 0)
                {
                    Rect viewPort = GetCurrentViewport();
                    Vector2 position = viewPort.GetRandomPosition();
                    NodeBase nodeBase = new NodeCreationContext(position, data.Type).CreateDrawableNodeFromContext(Context);
                    if (nodeBase != null)
                    {
                        Context.AddDrawable(nodeBase, DrawableCreationType.Default);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            _cachedHoveredElement = null;
            OnWindowDestroyed();
        }

        protected virtual void OnWindowDestroyed()
        {

        }

        private void OnEnable()
        {
            OnWindowEnabled();
        }

        protected virtual void OnWindowEnabled()
        {

        }

        private void OnDisable()
        {
            OnWindowDisabled();
        }

        protected virtual void OnWindowDisabled()
        {

        }

        private void OnGUI()
        {
            if (Project != null)
            {
                _drawables = Context.GetDrawables(x => !x.IsHidden);
            }
            else
            {
                _drawables = null;
            }

            EditorZoom.Begin(_zoomFactor, new Rect(0, 0, PanelSize, PanelSize));
            Event current = Event.current;

            DrawPanel();
            if (Project != null && _showGrids)
            {
                DrawGrids();
            }

            if (Project != null)
            {
                DrawElements(current.mousePosition);

                if (ShouldHandleEvents(current))
                {
                    if (this != mouseOverWindow && DragAndDrop.IsDragging)
                    {
                        DragAndDrop.EndDragging(Context, current, null);
                    }
                    HandleEvents(current);
                    if (this == mouseOverWindow)
                    {
                        DragAndDrop.OnGUI(Context, current);
                    }
                }
            }
            EditorZoom.End();

            if (_genericMenuGetter != null)
            {
                GenericMenu menu = _genericMenuGetter.Invoke();
                menu.ShowAsContext();
                _genericMenuGetter = null;
            }
            if (Project == null)
            {
                HandleProjectSelection();
            }
            else
            {
                DrawToolBar();
            }

            if (_debugMode)
            {
                DebugNormal();
            }
        }

        private void Update()
        {
            if (DragAndDrop != null)
            {
                if (DragAndDrop.IsDragging)
                {
                    Repaint();
                }
            }

            UpdateTopLeftTarget();
            OnUpdate();
        }

        protected virtual void OnUpdate() { }
        #endregion

        #region Helpers
        private bool IsMouseInsideOfPanel(Event current)
        {
            return current.mousePosition.y > ToolbarHeight + 21;
        }

        private void HandleProjectSelection()
        {
            const int sizeX = 400;
            const int yStart = 50;
            const int sizeY = 500;
            CustomGUILayout.BeginArea(new Rect(position.width / 2 - sizeX / 2, yStart, sizeX, sizeY));
            CustomGUILayout.HelpBox("Oops. It seems like the Project asset is missing.", MessageType.Warning);

            CustomGUILayout.BeginVertical(EditorStyles.helpBox);
            CustomGUILayout.BeginHorizontal();
            Project = CustomGUILayout.ObjectField(Project, "Project");
            if (CustomGUILayout.Button("New"))
            {
                Project = Project.NewDesign("New Project");
            }
            CustomGUILayout.EndHorizontal();

            Texture2D icon = GUIStyleCollection.GetTexture("project_designer_icon");
            if (icon != null)
            {
                CustomGUILayout.BeginHorizontal();
                CustomGUILayout.Space(64);
                CustomGUILayout.Image(icon, GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.Image), 256, 256);
                CustomGUILayout.EndHorizontal();

            }
            CustomGUILayout.EndVertical();

            CustomGUILayout.EndArea();
        }

        private bool ShouldHandleEvents(Event current)
        {
            bool result = true;
            if (current.isMouse)
            {
                result = focusedWindow == this && IsMouseInsideOfPanel(current);
            }
            else
            {
                result = !CustomGUIUtility.IsEditingTextField();
            }

            return result;
        }

        private Vector2 GetPerceivedScreenPosition(IDrawable drawable)
        {
            return drawable.Rect.position - GetCurrentViewport().position;
        }


        public Vector2 GetPerceivedScreenPosition(Vector2 position)
        {
            return position - GetCurrentViewport().position;
        }

        public Vector2 GetGUIPosition(Vector2 position)
        {
            return GetCurrentViewport().position + position;
        }


        private Rect GetCurrentViewport()
        {
            Rect viewPort = new Rect(_topLeft, Position.size / _zoomFactor);
            return viewPort;
        }

        private float GetZoomFactorToReachSize(Rect rect)
        {
            return Mathf.Clamp(Mathf.Min(Position.width / rect.width, Position.height / rect.height), MinZoomValue, MaxZoomValue);
        }

        #endregion
        #region Events
        private void HandleEvents(Event current)
        {
            switch (current.type)
            {
                case EventType.ContextClick:
                    HandleContextClick(current.mousePosition);
                    break;
                case EventType.MouseDown:
                    HandleMouseDown(current);
                    break;
                case EventType.MouseUp:
                    HandleMouseUp(current);
                    break;
                case EventType.KeyUp:
                    HandleKeyUp(current);
                    break;
                case EventType.KeyDown:
                    HandleKeyDown(current);
                    break;
                case EventType.MouseDrag:
                    HandleDrag(current);
                    break;
                case EventType.ScrollWheel:
                    HandleMouseWheel(current);
                    break;
                default:
                    DefaultEventHandler(current);
                    break;
            }
        }

        private void DefaultEventHandler(Event current)
        {
            if (!DragAndDrop.IsDragging)
            {
                SetHoveredElements(current);
                Repaint();
            }
        }

        private void HandleDrag(Event current)
        {
            if (DragAndDrop.IsDragging)
            {
                DragAndDrop.OnDragging(Context, current);

                if (DragAndDrop.Mode == DragAndDropMode.Empty)
                {
                    UpdateTopLeft(-current.delta);
                }
                Repaint();
            }
        }

        private void HandleKeyUp(Event current)
        {
            if (current.control)// Control + KeyCode commands here
            {
                switch (current.keyCode)
                {
                    case KeyCode.Z:
                        _history.Undo(Context);
                        break;
                    case KeyCode.Y:
                        _history.Redo(Context);
                        break;
                    case KeyCode.A:
                        SelectAllNodes();
                        break;
                    case KeyCode.C:
                        CopySelectedNodes();
                        break;
                    case KeyCode.V:
                        PasteSelectedNodes();
                        break;
                    case KeyCode.D:
                        DuplicateSelectedNodes();
                        break;
                    case KeyCode.H:
                        ShowHiddenNodes();
                        break;
                    case KeyCode.F:
                        ResetUserPreferences();
                        break;
                }
            }
            else
            {
                switch (current.keyCode)
                {
                    case KeyCode.Delete:
                        DeleteSelectedNodes();
                        break;
                    case KeyCode.H:
                        HideSelectedNodes();
                        break;
                }
            }

            current.Use();
        }

        private void HandleKeyDown(Event current)
        {
            current.Use();
        }

        private void HandleMouseDown(Event current)
        {
            if (!DragAndDrop.IsDragging)
            {
                ISelectable selectable = GetSelectedElementOnPosition(current.mousePosition);

                if (selectable == null)
                {
                    CustomGUIUtility.ResetControl();
                }

                if (!current.control && current.button != 2)
                {
                    DeselectAll(selectable);
                }

                if (current.button == 0)
                {
                    selectable?.OnSelect();

                    if (current.clickCount >= 2 && selectable != null)
                    {
                        selectable.OnDoubleClick();
                    }
                    else if (selectable != null && selectable is IDraggable draggable)
                    {
                        DragAndDrop.StartDragging(Context, draggable, current.mousePosition);
                    }
                    else
                    {
                        DeselectAll();
                        DragAndDrop.StartDraggingSelection(Context, current.mousePosition);
                    }
                }
                else if (current.button == 2)
                {
                    List<IDraggable> selectedItems = Context.GetDrawables<IDraggable>(x => x.IsSelected);

                    if (selectedItems.Count == 0)
                    {
                        DragAndDrop.StartDraggingEmpty(Context, current.mousePosition);
                    }
                    else
                    {
                        DragAndDrop.StartDragging(Context, selectedItems, current.mousePosition);
                    }
                }

                Repaint();
            }
        }

        private void HandleMouseUp(Event current)
        {
            if (DragAndDrop.IsDragging)
            {
                if (DragAndDrop.Mode == DragAndDropMode.Selection)
                {
                    Rect selection = DragAndDrop.SelectionRect;
                    selection.position = GetGUIPosition(selection.position);

                    for (int i = _drawables.Count - 1; i >= 0; i--)
                    {
                        IDrawable element = _drawables[i];
                        if (element is ISelectable selectable && selection.Contains(element.Rect))
                        {
                            Select(selectable);
                        }
                    }
                }

                DragAndDrop.EndDragging(Context, current, GetSelectedElementOnPosition(current.mousePosition));
                Repaint();
            }
            else
            {
                ISelectable selectable = GetSelectedElementOnPosition(current.mousePosition);
                if (selectable == null)
                {
                    CustomGUIUtility.ResetControl();
                }
            }
        }

        private void SetHoveredElements(Event current)
        {
            _cachedHoveredElement = null;
            for (int i = _drawables.Count - 1; i >= 0; i--)
            {
                IDrawable element = _drawables[i];
                if (element is IHoverable hoverable)
                {
                    Vector2 canvas = GetGUIPosition(current.mousePosition);
                    if (hoverable.Rect.Contains(canvas))
                    {
                        _cachedHoveredElement = hoverable;
                        _cachedHoveredElement.OnHoverEnter();
                        break;
                    }
                }
            }
        }

        private void HandleMouseWheel(Event current)
        {
            foreach (var element in _drawables)
            {
                Vector2 canvas = GetGUIPosition(current.mousePosition);
                if (element.Rect.Contains(canvas))
                {
                    return;
                }
            }

            SetZoomFactor(_zoomFactor - current.delta.y / 50f);
        }


        private void HandleContextClick(Vector2 position)
        {
            GenericMenu menu = new GenericMenu();
            bool addDefaultMenuOptions = true;
            Vector2 canvas = GetGUIPosition(position);

            ISelectable selectableOnClick = GetSelectedElementOnPosition(position);

            for (int i = _drawables.Count - 1; i >= 0; i--)
            {
                IDrawable element = _drawables[i];
                if (element is ISelectable selectable && selectable.Contains(canvas))
                {
                    OnContextClick(selectable, position, menu);
                    addDefaultMenuOptions = false;
                    break;
                }
            }

            CustomGUIUtility.ResetControl();
            DeselectAll(selectableOnClick);
            if (addDefaultMenuOptions)
            {
                AddDefaultContextMenu(position, menu);
            }

            menu.ShowAsContext();
        }

        private ISelectable GetSelectedElementOnPosition(Vector2 position)
        {
            Vector2 canvas = GetGUIPosition(position);

            for (int i = _drawables.Count - 1; i >= 0; i--)
            {
                IDrawable element = _drawables[i];
                if (element is ISelectable selectable && selectable.Contains(canvas))
                {
                    return selectable;
                }
            }

            return null;
        }

        private void OnContextClick(ISelectable selectable, Vector2 position, GenericMenu menu)
        {
            selectable.OnContextClick(Context, position, menu);
        }


        private void AddDefaultContextMenu(Vector2 position, GenericMenu menu)
        {
            foreach (var handler in TemplateCollection.GetDefaultContextHandlers())
            {
                handler.Invoke(Context, position, menu);
            }
        }

        #endregion
        #region Other
        private void SetZoomFactor(float magnitude)
        {
            _zoomFactor = Mathf.Clamp(magnitude, MinZoomValue, MaxZoomValue);
            Repaint();
        }


        private void ResetUserPreferences()
        {
            SetTopLeftTarget(Vector2.zero);
            SetZoomFactor(1f);
        }


        private void DeleteSelectedNodes()
        {
            List<NodeBase> selectedNodes = GetNodes(x => x.IsSelected);
            if (selectedNodes.Count > 0)
            {
                Context.ProcessAction(GetDeleteNodes(selectedNodes));
            }
        }

        private void HideSelectedNodes()
        {
            List<NodeBase> selectedNodes = GetNodes(x => x.IsSelected);
            if (selectedNodes.Count > 0)
            {
                Context.ProcessAction(GetHideNodes(selectedNodes));
            }
        }

        private void ShowHiddenNodes()
        {
            List<NodeBase> hiddenNotes = GetNodes(x => x.IsHidden);
            if (hiddenNotes.Count > 0)
            {
                Context.ProcessAction(GetShowNodes(hiddenNotes));
            }
        }

        private void CopySelectedNodes()
        {
            _nodeBuffer = GetNodes(x => x.IsSelected && x.CanBeCopied);
        }

        private void PasteSelectedNodes()
        {
            if (_nodeBuffer != null && _nodeBuffer.Count > 0)
            {
                ProcessAction(new DuplicateNodesAction(_nodeBuffer));
            }
        }

        public void ProcessAction(EditorContextAction action)
        {
            _history.Do(action, Context);
        }

        private void DuplicateSelectedNodes()
        {
            List<NodeBase> selectedNodes = GetNodes(x => x.IsSelected && x.CanBeCopied);
            if (selectedNodes.Any())
            {
                Context.ProcessAction(GetDuplicateNodesNode(selectedNodes));
            }
        }

        private void UpdateTopLeft(Vector2 delta)
        {
            _topLeft.x = Mathf.Clamp(_topLeft.x + delta.x, 0, PanelSize - Position.width / _zoomFactor);
            _topLeft.y = Mathf.Clamp(_topLeft.y + delta.y, 0, PanelSize - Position.height / _zoomFactor);
            _topLeftTarget = _topLeft;
        }

        private void UpdateTopLeftTarget()
        {
            if (Vector2.Distance(_topLeft, _topLeftTarget) > 0.01f)
            {
                _topLeft = Vector2.Lerp(_topLeft, _topLeftTarget, Time.deltaTime / 2);
                Repaint();
            }
            else
            {
                _topLeftTarget = _topLeft;
            }
        }

        private void SetTopLeftTarget(Vector2 position)
        {
            _topLeftTarget.x = Mathf.Clamp(position.x, 0, PanelSize - Position.width / _zoomFactor);
            _topLeftTarget.y = Mathf.Clamp(position.y, 0, PanelSize - Position.height / _zoomFactor);
        }

        private void ResetHoveredElements()
        {
            _cachedHoveredElement = null;
            for (int i = _drawables.Count - 1; i >= 0; i--)
            {
                IDrawable element = _drawables[i];
                if (element is IHoverable hoverable)
                {
                    hoverable.OnHoverExit();
                }
            }

            Repaint();
        }

        private void DeselectAll(ISelectable except = null)
        {
            for (int i = _drawables.Count - 1; i >= 0; i--)
            {
                IDrawable element = _drawables[i];
                if (element is ISelectable selectable && (except == null || except != selectable))
                {
                    Deselect(selectable);
                }
            }

            Repaint();
        }

        private void SelectAllNodes()
        {
            foreach (var element in _drawables)
            {
                if (!element.IsHidden && element is NodeBase n)
                {
                    Select(n);
                }
            }
        }

        private void Deselect(ISelectable selectable)
        {
            Debug.Assert(selectable != null && !selectable.IsHidden);
            selectable.OnDeselect();
        }

        private void Select(ISelectable selectable)
        {
            Debug.Assert(selectable != null && !selectable.IsHidden);
            selectable.OnSelect();
            Repaint();
        }
        #endregion
        #region Actions

        private DeleteNodesAction GetDeleteNodes(List<NodeBase> nodes)
        {
            return new DeleteNodesAction(nodes);
        }

        private HideNodesAction GetHideNodes(List<NodeBase> nodes)
        {
            return new HideNodesAction(nodes);
        }

        private ShowNodesAction GetShowNodes(List<NodeBase> nodes)
        {
            return new ShowNodesAction(nodes);
        }

        private DuplicateNodesAction GetDuplicateNodesNode(List<NodeBase> nodes)
        {
            return new DuplicateNodesAction(nodes);
        }
        #endregion
        #region OnGUI

        private void DrawPanel()
        {
            Rect graphPanel = new Rect(0, 0, PanelSize, PanelSize);
            GUI.DrawTexture(graphPanel, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, _backgroundColor, Vector4.zero, 0f);
        }

        private void DrawGrids()
        {
            Rect rect = new Rect(0, 0, PanelSize, PanelSize);

            int widthDivs = Mathf.CeilToInt(rect.width / Settings.GridSpacing) + 1;
            int heightDivs = Mathf.CeilToInt(rect.height / Settings.GridSpacing) + 1;

            Color handlesColor = Handles.color;
            Handles.color = _gridColor;
            for (int i = 0; i < widthDivs; i++)
            {
                Vector2 v1 = rect.position + new Vector2(i * Settings.GridSpacing, 0f);
                Vector2 v2 = rect.position + new Vector2(i * Settings.GridSpacing, rect.height);

                Handles.DrawLine(GetPerceivedScreenPosition(v1), GetPerceivedScreenPosition(v2));

                for (int j = 0; j < heightDivs; j++)
                {
                    if (i == widthDivs - 1)
                    {
                        v1 = GetPerceivedScreenPosition(rect.position + new Vector2(0f, j * Settings.GridSpacing));
                        v2 = GetPerceivedScreenPosition(rect.position + new Vector2(rect.width, j * Settings.GridSpacing));
                        Handles.DrawLine(v1, v2);
                    }
                }
            }

            //Borders
            //top left -> top right
            Handles.DrawLine(GetPerceivedScreenPosition(rect.position), GetPerceivedScreenPosition(new Vector2(rect.position.x + rect.size.x, rect.position.y)));
            //top left -> bottom left
            Handles.DrawLine(GetPerceivedScreenPosition(rect.position), GetPerceivedScreenPosition(new Vector2(rect.position.x, rect.position.y + rect.size.y)));
            //bottom right -> top right
            Handles.DrawLine(GetPerceivedScreenPosition(rect.position + rect.size), GetPerceivedScreenPosition(new Vector2(rect.position.x + rect.size.x, rect.position.y)));
            //bottom right -> bottom left
            Handles.DrawLine(GetPerceivedScreenPosition(rect.position + rect.size), GetPerceivedScreenPosition(new Vector2(rect.position.x, rect.position.y + rect.size.y)));

            Handles.color = handlesColor;
        }


        private void DebugNormal()
        {
            Rect rect = Position;
            rect.position = new Vector2(5, rect.height - 18);
            GUIStyle style = EditorStyles.miniLabel;
            CustomGUILayout.BeginArea(rect);
            int selectedCount = GetNodes(x => x.IsSelected).Count;
            CustomGUILayout.BeginHorizontal();
            CustomGUILayout.Label($"Mouse position: {Event.current.mousePosition}", style);
            CustomGUILayout.Label($"Selected Node Count: {selectedCount}", style);
            string isEditing = CustomGUIUtility.IsEditingTextField() ? "Yes" : "No";

            CustomGUILayout.Label($"Is editing: {isEditing}", style);
            CustomGUILayout.Label($"Control: {GUI.GetNameOfFocusedControl()}", style);
            CustomGUILayout.EndHorizontal();

            CustomGUILayout.EndArea();
        }

        private void DrawElements(Vector2 mousePosition)
        {
            for (int i = 0; i < _drawables.Count; i++)
            {
                IDrawable element = _drawables[i];
                DrawElement(element, mousePosition);
            }
        }

        private void DrawElement(IDrawable element, Vector2 mousePosition)
        {
            Color oldColor = GUI.color;
            if (!string.IsNullOrEmpty(_searchQuery) && !element.Match(_searchQuery))
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f);
            }
            else
            {
                GUI.color = oldColor;
            }
            Vector2 perceivedPosition = GetPerceivedScreenPosition(element);
            element.Draw(Context, perceivedPosition, mousePosition);

            if (element is IHoverable hoverable)
            {
                hoverable.OnHoverExit();
            }
            GUI.color = oldColor;
        }
        private List<NodeBase> GetNodes(Func<NodeBase, bool> func = null)
        {
            List<NodeBase> nodes = new List<NodeBase>();
            foreach (var element in _drawables)
            {
                if (element is NodeBase n && (func == null || func(n)))
                {
                    nodes.Add(n);
                }
            }
            return nodes;
        }

        #endregion
        #region Toolbar
        private void DrawToolBar()
        {
            CustomGUILayout.BeginHorizontal(EditorStyles.toolbar, w: position.width, h: ToolbarHeight);
            {
                CustomGUILayout.Label($"Canvas: {GetCurrentViewport()}", EditorStyles.miniBoldLabel, w: 320);
                CustomGUILayout.Label($"Drawable Count: {_drawables.Count}", EditorStyles.miniBoldLabel, w: 120);

                // Left-aligned elements
                CustomGUILayout.Label("Zoom", EditorStyles.miniBoldLabel, w: 50);
                _zoomFactor = CustomGUILayout.Slider(_zoomFactor, MinZoomValue, MaxZoomValue, w: 200);

                CustomGUILayout.Label("Background Color", EditorStyles.miniBoldLabel, w: 100);
                _backgroundColor = CustomGUILayout.ColorField(_backgroundColor);

                CustomGUILayout.Label("Grid Color", EditorStyles.miniBoldLabel, w: 60);
                _gridColor = CustomGUILayout.ColorField(_gridColor);

                // Add a flexible space to push the next elements to the right side

                // Right-aligned elements
                _debugMode = CustomGUILayout.Toggle(_debugMode, "Debug Mode", EditorStyles.toolbarButton);
                _showGrids = CustomGUILayout.Toggle(_showGrids, "Show Grids", EditorStyles.toolbarButton);

                if (CustomGUILayout.Button("Overview", EditorStyles.toolbarButton))
                {
                    Overview();
                }


                if (CustomGUILayout.Button("Select All", EditorStyles.toolbarButton))
                {
                    SelectAllNodes();
                }

                if (CustomGUILayout.Button("Delete", EditorStyles.toolbarButton))
                {
                    DeleteSelectedNodes();
                }

                if (CustomGUILayout.Button("Settings", EditorStyles.toolbarButton))
                {
                    ProjectDesignerSettings.OpenSettings();
                }

                SearchToolbar();
                /*if (GUILayout.Button("Reset", EditorStyles.toolbarButton))
                {
                    ResetUserPreferences();
                }*/
            }
            CustomGUILayout.EndHorizontal();

        }

        private void SearchToolbar()
        {
            CustomGUILayout.BeginHorizontal(EditorStyles.toolbar);

            _searchQuery = CustomGUILayout.TextField(_searchQuery, nameof(ProjectDesignerWindow), EditorStyles.toolbarSearchField);

            if (CustomGUILayout.Button("X", EditorStyles.toolbarButton, w: 20))
            {
                SetSearchFilter(string.Empty);
            }
            // Dropdown button
            if (CustomGUILayout.Button(string.Empty, EditorStyles.toolbarDropDown, w: 20))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var member in Context.GetTeamMembers())
                {
                    menu.AddItem(new GUIContent(member.FullName), false, () => _searchQuery = member.FullName);
                }

                menu.ShowAsContext();
            }

            CustomGUILayout.EndHorizontal();
        }

        private void SetSearchFilter(string filter)
        {
            _searchQuery = filter;
            EditorGUI.FocusTextInControl(null);
            Repaint();
        }


        private void Overview()
        {
            List<NodeBase> selectedNodes = GetNodes(x => x.IsSelected);
            Overview(selectedNodes.Count == 0 ? GetNodes() : selectedNodes);
        }

        private Rect CalculateBoundingBox(IEnumerable<IDrawable> drawables, float padding = 0f)
        {
            float xMax = 0, yMax = 0, xMin = int.MaxValue, yMin = int.MaxValue;

            // Calculate the bounds of all nodes
            foreach (var drawable in drawables)
            {
                if (drawable.Rect.x <= xMin)
                {
                    xMin = drawable.Rect.x;
                }
                if (drawable.Rect.y <= yMin)
                {
                    yMin = drawable.Rect.y;
                }

                if (drawable.Rect.xMax >= xMax)
                {
                    xMax = drawable.Rect.xMax;
                }
                if (drawable.Rect.yMax >= yMax)
                {
                    yMax = drawable.Rect.yMax;
                }
            }

            xMin = Mathf.Clamp(xMin - padding, 0, PanelSize);
            yMin = Mathf.Clamp(yMin - padding, 0, PanelSize);

            xMax = Mathf.Clamp(xMax + padding, 0, PanelSize);
            yMax = Mathf.Clamp(yMax + padding, 0, PanelSize);

            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public void Overview(IEnumerable<IDrawable> drawables, float margin = 50)
        {
            if (drawables.Any())
            {
                Rect boundingBox = CalculateBoundingBox(drawables, margin);
                float zoom = GetZoomFactorToReachSize(boundingBox);
                SetZoomFactor(zoom);

                //try to fit viewport in the center
                Rect viewPort = GetCurrentViewport();

                float canvasPaddingX = (viewPort.width - boundingBox.width) / 2;
                float canvasPaddingY = (viewPort.height - boundingBox.height) / 2;

                Vector2 drawablesPosition = new Vector2(Mathf.Clamp(boundingBox.x - canvasPaddingX, 0, PanelSize), Mathf.Clamp(boundingBox.y - canvasPaddingY, 0, PanelSize));
                SetTopLeftTarget(drawablesPosition);

            }
        }

        public Vector2 CheckIfRectInBounds(Rect position, out bool result)
        {
            result = position.xMin > 0 && position.yMin > 0 && position.xMax < PanelSize && position.yMax < PanelSize;
            position.x = Mathf.Clamp(position.x, 0, PanelSize);
            position.y = Mathf.Clamp(position.y, ToolbarHeight, PanelSize);

            if (position.xMax > PanelSize)
            {
                position.x = Mathf.Clamp(position.x - (position.xMax - PanelSize), 0, PanelSize);
            }

            if (position.yMax > PanelSize)
            {
                position.y = Mathf.Clamp(position.y - (position.yMax - PanelSize), 0, PanelSize);
            }
            return position.position;
        }

        public void ShowDropdownOutsideZoomArea(Func<GenericMenu> getGenericMenu)
        {
            _genericMenuGetter = getGenericMenu;
        }

        #endregion
    }
}
