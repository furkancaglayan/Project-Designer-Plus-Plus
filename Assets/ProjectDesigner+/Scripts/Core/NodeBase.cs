using System;
using System.Collections.Generic;
using ProjectDesigner.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// The base element which nodes are created from. Implements <see cref="IDrawable"/>, <see cref="ISelectable"/>, <see cref="IDraggable"/>, <see cref="IHoverable"/> and <see cref="ISerialized"/>.<br></br>
    /// <see cref="NodeBase"/> is a very flexible and customizable visual element. NodeBase types are collected via reflection in the editor.<br></br>
    /// See also: <seealso cref="ConnectionBase"/>, <seealso cref="MemberBase"/>, <seealso cref="NodeBaseMetaDataAttribute"/>.
    /// </summary>
    [Serializable]
    public abstract class NodeBase : IDrawable, ISelectable, IDraggable, IHoverable, ISerialized
    {
        /// <summary>
        /// Status of the task progress.
        /// </summary>
        public enum NodeStatus
        {
            /// <summary>
            /// Denotes a task that is not started yet.
            /// </summary>
            NotStarted,
            /// <summary>
            /// Denotes a task that is in progress.
            /// </summary>
            InProgress,
            /// <summary>
            /// Denotes a task that is completed.
            /// </summary>
            Completed,
            /// <summary>
            /// Number of statuses.
            /// </summary>
            NumStatus,
        }

        private const string ConnectionTextureId = "dot";
        private const string ConnectionOutlineTextureId = "dot_outline";
        public const int HeaderTextCharacterLimit = 40;
        private float NodeSizeUpdateSpeed => Time.deltaTime / 4;

        /// <summary>
        /// Rect (position and the size) of the <see cref="NodeBase"/>.
        /// </summary>
        public Rect Rect
        {
            get
            {
                if (IsDragged)
                {
                    return new Rect(_tempPosition, _rect.size);
                }

                return _rect;
            }
        }

        [SerializeField]
        private Rect _rect;
        [NonSerialized]
        private Vector2 _tempPosition;
        [SerializeField]
        private Vector2 _targetSize;
        [SerializeReference]
        private List<MemberBase> _members = new List<MemberBase>();
        [SerializeReference]
        private List<TeamMember> _assignees = new List<TeamMember>();
        [SerializeField]
        private NodeStatus _status = NodeStatus.NotStarted;

        /// <summary>
        /// Read only collection of members in the node.
        /// </summary>
        public ReadOnlyCollection<MemberBase> Members => new ReadOnlyCollection<MemberBase>(_members);

        [SerializeField]
        private Vector2 _scroll;
        /// <summary>
        /// Minimum size of the <see cref="NodeBase"/>. Used when the node is not expanded. See: <see cref="IsExpanded"/>.
        /// </summary>
        public abstract Vector2 MinSize { get; }
        /// <summary>
        /// Maximum size of the <see cref="NodeBase"/>. Used when the node is expanded. See: <see cref="IsExpanded"/>.
        /// </summary>
        public abstract Vector2 MaxSize { get; }
        /// <summary>
        /// IconKey is the name of the <see cref="Texture2D"/> asset to use as a <see cref="NodeBase"/> banner/logo on the editor. The resulting texture is fetched via <see cref="GUIStyleCollection.GetTexture(string)"/>.
        /// If left empty, or a texture could not be found, only <see cref="HeaderText"/> is shown without an icon. Default icons can also be changed with <see cref="ProjectDesignerSettings.CustomTextures"/>.
        /// </summary>
        public abstract string IconKey { get; }
        /// <summary>
        /// Width and height of the icon to show with <see cref="IconKey"/>. May also need a proper header area height, <see cref="HeaderHeight"/>.
        /// </summary>
        public virtual float IconSize => 64;
        /// <summary>
        /// Text to show as node header. See: <see cref="HeaderTextCharacterLimit"/>.
        /// </summary>
        public string HeaderText
        {
            get
            {
                return _headerText;
            }
            protected set
            {
                Debug.Assert(value != null);
                _headerText = value;
                Debug.Assert(_headerText.Length < HeaderTextCharacterLimit);
                if (_headerText.Length > HeaderTextCharacterLimit)
                {
                    _headerText = _headerText.Substring(0, HeaderTextCharacterLimit);
                }
            }
        }
        [SerializeField]
        private string _headerText;
        /// <summary>
        /// Texture on key is used for getting the texture when the <see cref="NodeBase"/> is "highlighted". This usually means being hovered on and being selected.
        /// Texture is fetched via <see cref="GUIStyleCollection.GetTexture(string)"/>. The texture to get must be similar to the texture from <see cref="TextureKeyOff"/>.
        /// It is best to make the only difference from <see cref="TextureKeyOff"/> is borders.
        /// </summary>
        public virtual string TextureKeyOn => "node0 on";
        /// <summary>
        /// Texture off key is used for getting the texture when the <see cref="NodeBase"/> is not "highlighted". This usually means being hovered on and being selected.
        /// Texture is fetched via <see cref="GUIStyleCollection.GetTexture(string)"/>. The texture to get must be similar to the texture from <see cref="TextureKeyOn"/>.
        /// It is best to make the only difference from <see cref="TextureKeyOn"/> is lack of borders.
        /// </summary>
        public virtual string TextureKeyOff => "node0";
        //inheritdoc
        public bool IsSelected => _isSelected;
        /// <summary>
        /// Defines where the incoming connection will end up. Also used for drawing connection input point.
        /// </summary>
        public virtual Vector2 InputPoint => new Vector2(Rect.center.x, Rect.position.y);
        /// <summary>
        /// Defines where the outgoing connection will end up. Also used for drawing connection output point.
        /// </summary>
        public virtual Vector2 OutputPoint => new Vector2(Rect.center.x, Rect.yMax);

        [SerializeReference]
        private SerializableConnectionDictionary _connections;

        private bool _isSelected { get; set; }
        private bool _isHovered { get; set; }
        /// <summary>
        /// Is the node expanded? This usually means the node uses <see cref="MaxSize"/> and is editable.
        /// </summary>
        public bool IsExpanded { get; private set; }
        /// <summary>
        /// Is the node being dragged?
        /// </summary>
        public bool IsDragged { get; private set; }
        //<inheritdoc>
        public bool IsHidden { get; private set; }
        /// <summary>
        /// Color of the connection points (Input-output points).
        /// </summary>
        protected virtual Color ConnectionPointColor => new Color(58f / 255f, 76f / 255f, 93f / 255f);
        /// <summary>
        /// Radius of the connection points (Input-output points).
        /// </summary>
        protected virtual int ConnectionPointRadius => 12;
        /// <summary>
        /// Height of the header area.
        /// </summary>
        protected virtual int HeaderHeight => 90;
        /// <summary>
        /// Height of the footer area.
        /// </summary>
        protected virtual int FooterHeight => 40;
        /// <summary>
        /// Can the node be copied/duplicated?
        /// </summary>
        public abstract bool CanBeCopied { get; }
        /// <summary>
        /// Unique identifier. See: <see cref="UniqueIdGenerator.GenerateUniqueId"/>
        /// </summary>
        [field: SerializeField]
        public string Id { get; private set; }
        /// <summary>
        /// Creates a new <see cref="NodeBase"/> with a unique Id.
        /// </summary>
        public NodeBase()
        {
            _connections = new SerializableConnectionDictionary();
            IsHidden = false;
            Id = UniqueIdGenerator.GenerateUniqueId();
        }
        /// <summary>
        /// Copies a node and creates a new ones from it. Header, status, members and assignees are also copied.
        /// </summary>
        /// <param name="nodeBase"></param>
        public NodeBase(NodeBase nodeBase) : this()
        {
            _rect = nodeBase._rect;
            SetPosition(_rect.position + new Vector2(30, 30));
            _members = new List<MemberBase>();
            _headerText = nodeBase._headerText;
            foreach (var member in nodeBase._members)
            {
                AddMember(member.Copy(this));
            }

            foreach (var member in nodeBase._assignees)
            {
                AddAssignee(member);
            }
            _scroll = nodeBase._scroll;
            IsExpanded = nodeBase.IsExpanded;
            _status = nodeBase._status;
        }
        /// <summary>
        /// Abstract copy method to copy nodes. The constructor used in this function must also use <see cref="NodeBase(NodeBase)"/> constructor.
        /// </summary>
        /// <returns></returns>
        public abstract NodeBase Copy();

        /// <summary>
        /// Draws the node.
        /// </summary>
        /// <param name="context">Editor context</param>
        /// <param name="screenPosition">Screen position</param>
        /// <param name="mousePosition">Mouse position</param>
        public void Draw(IEditorContext context, Vector2 screenPosition, Vector2 mousePosition)
        {
            GUIStyle style = GetStyle();
            RectOffset padding = style?.padding;

            Rect nodeRect = new Rect(screenPosition, Rect.size);
            Rect headerArea = new Rect(padding.left, padding.top, Rect.width - padding.right - padding.left, HeaderHeight - padding.top);
            Rect memberArea = new Rect(padding.left, HeaderHeight + 10, Rect.width - padding.left - padding.right, Rect.height - HeaderHeight - FooterHeight - 10);
            Rect footer = new Rect(padding.left, Rect.height - FooterHeight - padding.bottom, Rect.width - padding.left - padding.right, FooterHeight);
            Rect headerAreaExtended = new Rect(0, 0, Rect.width, HeaderHeight);
            Rect footerAreaExtended = new Rect(0, footer.y + padding.bottom, Rect.width, FooterHeight);

            BeginArea(nodeRect);
            DrawBackground(headerAreaExtended);
            DrawHeader(headerArea, padding);
            DrawMembers(context, memberArea, padding, mousePosition);
            DrawFooter(context, footer, padding);
            DrawBackground(footerAreaExtended);
            EndArea();
            DrawConnectionPoints(context);
            SetTargetSize(IsExpanded ? MaxSize : MinSize);
            UpdateSize();
        }

        private void DrawConnectionPoints(IEditorContext context)
        {
            if (CanHaveConnections())
            {
                GUIUtilities.DrawTexture(context.GetScreenPosition(InputPoint), ConnectionPointColor, GUIStyleCollection.GetTexture(ConnectionTextureId), ConnectionPointRadius);
                GUIUtilities.DrawTexture(context.GetScreenPosition(InputPoint), Color.white, GUIStyleCollection.GetTexture(ConnectionOutlineTextureId), ConnectionPointRadius);

                GUIUtilities.DrawTexture(context.GetScreenPosition(OutputPoint), ConnectionPointColor, GUIStyleCollection.GetTexture(ConnectionTextureId), ConnectionPointRadius);
                GUIUtilities.DrawTexture(context.GetScreenPosition(OutputPoint), Color.white, GUIStyleCollection.GetTexture(ConnectionOutlineTextureId), ConnectionPointRadius);
            }
        }

        private void DrawFooter(IEditorContext context, Rect rect, RectOffset padding)
        {
            GUILayout.BeginArea(rect);
            OnDrawFooter(context, rect, padding);
            GUILayout.EndArea();
        }

        protected virtual void OnDrawFooter(IEditorContext context, Rect rect, RectOffset padding)
        {

        }

        private void DrawMembers(IEditorContext context, Rect rect, RectOffset padding, Vector2 mousePosition)
        {
            const float SpaceBetweenTwoMembers = 20;
            float memberAreaHeight = Rect.height - HeaderHeight - FooterHeight - padding.bottom; // Maximum height before adding scroll bar
            CustomGUILayout.Space(EditorGUIUtility.singleLineHeight);
            CustomGUILayout.BeginArea(rect);
            using (GUILayout.ScrollViewScope scrollView = CustomGUILayout.CreateScrollView(_scroll, w: rect.width, h: memberAreaHeight))
            {
                _scroll = scrollView.scrollPosition;
                DrawMembersInternal(context, rect, SpaceBetweenTwoMembers);
            }
            CustomGUILayout.EndArea();
        }

        private void DrawMembersInternal(IEditorContext context, Rect rect, float spaceBetweenTwoMembers)
        {
            for (int i = 0; i < _members.Count; i++)
            {
                MemberBase member = _members[i];
                float width = rect.width;
                CustomGUILayout.Space(spaceBetweenTwoMembers / 2);
                member.Draw(context, this, width);
                CustomGUILayout.Space(spaceBetweenTwoMembers / 2);
            }
        }

        /// <summary>
        /// Checks if the node has any <see cref="MemberBase"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasMember<T>() where T : MemberBase
        {
            return GetFirstMember<T>() != null;
        }

        /// <summary>
        /// Gets the first <see cref="MemberBase"/> of type <typeparamref name="T"/> if it has any. Otherwise it returns null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetFirstMember<T>() where T : MemberBase
        {
            foreach (MemberBase member in _members)
            {
                if (member is T t)
                {
                    return t;
                }
            }

            return null;
        }

        private void BeginArea(in Rect rect)
        {
            GUIContent tooltip = new GUIContent(string.Empty, HeaderText);

            GUIStyle style = GetStyle();
            if (style != null)
            {
                GUI.DrawTexture(rect, GetBackground());
                GUILayout.BeginArea(rect, tooltip, style);
            }
            else
            {
                GUILayout.BeginArea(rect, tooltip);
            }
        }


        private void EndArea()
        {
            GUILayout.EndArea();
        }

        /// <summary>
        /// Sets node position.
        /// </summary>
        /// <param name="position">New node position.</param>
        public void SetPosition(Vector2 position)
        {
            _rect.position = position;
        }

        /// <summary>
        /// Sets target size of the rect so the real size can smoothly lerp into the target. 
        /// </summary>
        /// <param name="size">Target size.</param>
        public void SetTargetSize(Vector2 size)
        {
            size.x = Mathf.Clamp(size.x, MinSize.x, MaxSize.x);
            size.y = Mathf.Clamp(size.y, MinSize.y, MaxSize.y);
            _targetSize = size;
        }

        private void UpdateSize()
        {
            if (Vector2.Distance(_rect.size, _targetSize) > 0.01f)
            {
                _rect.size = Vector2.Lerp(_rect.size, _targetSize, NodeSizeUpdateSpeed);
            }
            else
            {
                _rect.size = _targetSize;
            }
        }

        /// <summary>
        /// Adds and returns a new <see cref="MemberBase"/>.
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public MemberBase AddMember(MemberBase member)
        {
            if (_members.Count < MemberBase.MemberLimit)
            {
                _members.Add(member);
                member.OnAdded(this);
                OnMemberAdded(member);
                SortMembers();
            }
            else
            {
                Debug.Assert(false, "Member limit is reached.");
            }

            return member;
        }

        /// <summary>
        /// Adds and returns a new <see cref="MemberBase"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddMember<T>() where T : MemberBase, new()
        {
            return (T)AddMember(Activator.CreateInstance<T>());
        }

        private void SortMembers()
        {
            _members = _members.OrderByDescending(x => x.Priority).ToList();
        }

        /// <summary>
        /// Removes a <see cref="MemberBase"/> from the node.
        /// </summary>
        /// <param name="member"></param>
        public void RemoveMember(MemberBase member)
        {
            bool result = _members.Remove(member);
            Debug.Assert(result);
        }

        /// <summary>
        /// Clears all members in the node.
        /// </summary>
        public void ClearMembers()
        {
            _members.Clear();
        }

        /// <summary>
        /// Called when a new member is added to the node.
        /// </summary>
        /// <param name="member">Added <see cref="MemberBase"/>.</param>
        protected virtual void OnMemberAdded(MemberBase member)
        {

        }

        /// <summary>
        /// Adds a new assignee to the node.
        /// </summary>
        /// <param name="member"></param>
        public void AddAssignee(TeamMember member)
        {
            if (!_assignees.Contains(member))
            {
                _assignees.Add(member);
            }
        }

        /// <summary>
        /// Removes an assignee from the node.
        /// </summary>
        /// <param name="member"></param>
        public void RemoveAssignee(TeamMember member)
        {
            _assignees.Remove(member);
        }

        /// <summary>
        /// Returns true if the node has <paramref name="member"/>.
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public bool HasAssignee(TeamMember member)
        {
            return _assignees.Contains(member);
        }

        /// <summary>
        /// Returns the assignee list of the node.
        /// </summary>
        /// <returns></returns>
        public List<TeamMember> GetAssignees()
        {
            return _assignees;
        }


        /// <summary>
        /// Returns the assignee count of the node.
        /// </summary>
        /// <returns></returns>
        public int GetAssigneeCount()
        {
            return _assignees.Count;
        }

        /// <summary>
        /// Clears the assignees in the node.
        /// </summary>
        public void ClearAssignees()
        {
            _assignees.Clear();
        }

        /// <summary>
        /// Sets the status of the node.
        /// </summary>
        /// <param name="status"></param>
        public void SetStatus(NodeStatus status)
        {
            _status = status;
        }

        /// <summary>
        /// Returns the status of the node.
        /// </summary>
        /// <returns></returns>
        public NodeStatus GetStatus()
        {
            return _status;
        }

        private GUIStyle GetStyle()
        {
            return _isSelected ? GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.NodeOn) :
                                 GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.NodeOff);
        }

        private GUIStyle GetLabelStyle()
        {
            return GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.Label);
        }

        private GUIStyle GetImageStyle()
        {
            return GUIStyleCollection.GetDefaultStyleForCategory(GUIStyleCollection.StyleCategory.Image);
        }

        private Texture2D GetNodeIcon()
        {
            if (string.IsNullOrEmpty(IconKey))
            {
                return null;
            }

            return GUIStyleCollection.GetTexture(IconKey);
        }

        private Texture2D GetBackground()
        {
            if (_isSelected || IsDragged || _isHovered)
            {
                return GUIStyleCollection.GetTexture(TextureKeyOn);
            }
            else
            {
                return GUIStyleCollection.GetTexture(TextureKeyOff);
            }
        }

        //<inheritdoc>
        void IDrawable.OnRemoved(IEditorContext context)
        {
            OnRemovedInternal(context);
        }

        /// <summary>
        /// Called after a <see cref="NodeBase"/> is removed from the editor. See: also <seealso cref="IDrawable.OnRemoved(IEditorContext)"/>.
        /// </summary>
        /// <param name="context"></param>
        protected virtual void OnRemovedInternal(IEditorContext context)
        {

        }

        //<inheritdoc>
        void IDrawable.OnAdded(IEditorContext context, DrawableCreationType drawableCreationType)
        {
            OnAddedInternal(context, drawableCreationType);
        }

        /// <summary>
        /// Called after a <see cref="NodeBase"/> is added to the editor. See: also <seealso cref="IDrawable.OnAdded(IEditorContext, DrawableCreationType)"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="drawableCreationType"></param>
        protected virtual void OnAddedInternal(IEditorContext context, DrawableCreationType drawableCreationType)
        {

        }

        private void DrawBackground(in Rect area)
        {
            EditorGUI.DrawRect(area, new Color(0.2f, 0.2f, 0.2f, 0.4f));
        }

        private void DrawHeader(Rect headerArea, RectOffset padding, int spaceOnTop = 10, int spaceOnBottom = 16)
        {
            Texture2D icon = GetNodeIcon();
            CustomGUILayout.BeginArea(headerArea);
            if (icon == null)
            {
                CustomGUILayout.EditableText(ref _headerText, HeaderText, GetLabelStyle(), IsExpanded, $"{Id}_Header", characterLimit: HeaderTextCharacterLimit);
            }
            else
            {
                Rect iconArea = new Rect(headerArea.x, 0, headerArea.width / 4f, headerArea.height);
                Rect textArea = new Rect(iconArea.xMax, 0, headerArea.width - iconArea.width, headerArea.height);
                CustomGUILayout.BeginArea(iconArea);
                CustomGUILayout.Image(icon, GetImageStyle(), IconSize, IconSize);
                CustomGUILayout.EndArea();

                CustomGUILayout.BeginArea(textArea);
                CustomGUILayout.EditableText(ref _headerText, HeaderText, GetLabelStyle(), IsExpanded, $"{Id}_Header", characterLimit: HeaderTextCharacterLimit);
                CustomGUILayout.EndArea();
            }

            CustomGUILayout.EndArea();
        }

        //<inheritdoc>
        void ISelectable.OnSelect()
        {
            _isSelected = true;
            OnSelected();
        }

        //<inheritdoc>
        void ISelectable.OnDeselect()
        {
            _isSelected = false;
            IsExpanded = false;
            OnDeselected();
        }

        //<inheritdoc>
        void ISelectable.OnContextClick(IEditorContext context, Vector2 position, GenericMenu menu)
        {
            _isSelected = true;
            menu.AddItem(new GUIContent("Delete"), false, context.ProcessAction, new DeleteNodeAction(this));
            if (CanBeCopied)
            {
                menu.AddItem(new GUIContent("Duplicate Node"), false, context.ProcessAction, new DuplicateNodesAction(this));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Duplicate Node"));
            }

            if (!IsHidden)
            {
                menu.AddItem(new GUIContent("Hide"), false, context.ProcessAction, new HideNodeAction(this));
            }

            if (CanHaveConnections())
            {
                foreach (var data in context.GetAvailableConnectionTypes())
                {
                    bool canHaveConnection = CanHaveConnectionOfType(data.Type);
                    if (canHaveConnection)
                    {
                        menu.AddItem(new GUIContent($"New Connection/{data.DisplayName}"), false, () => { StartNewConnection(context, this, data.Type); });
                    }
                }
            }

            if (_assignees.Count > 0)
            {
                menu.AddItem(new GUIContent("Clear Assignees"), false, context.ProcessAction, new ClearAssigneesAction(this));
            }

            if (_members.Count > 0)
            {
                menu.AddItem(new GUIContent("Clear Members"), false, context.ProcessAction, new ClearMembersAction(this));
            }
            OnContextClick(context, position, menu);
        }

        public override string ToString()
        {
            return $"{GetType().Name} ({Id})";
        }

        /// <summary>
        /// Returns all outgoing connections the node has.
        /// </summary>
        /// <returns></returns>
        public List<ConnectionBase> GetConnections()
        {
            return _connections.Values.ToList();
        }

        /// <summary>
        /// Returns the connection from this node to other. If it doesn't exist it returns null.
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public ConnectionBase GetConnectionTo(NodeBase to)
        {
            if (_connections.TryGetValue(to, out ConnectionBase connection))
            {
                return connection;
            }

            return null;
        }

        /// <summary>
        /// Adds a new connection <paramref name="connection"/> from this node to other.
        /// </summary>
        /// <param name="connection">Connection to add.</param>
        /// <param name="to">Where the connection will end up?</param>
        public void AddConnection(ConnectionBase connection, NodeBase to)
        {
            Debug.Assert(CanHaveConnectionOfType(to, connection.GetType()));
            _connections[to] = connection;
            connection.SetParent(this);
            connection.Attach(to);
        }

        /// <summary>
        /// Removes a connection from the node.
        /// </summary>
        /// <param name="connection"></param>
        public void RemoveConnection(ConnectionBase connection)
        {
            _connections.Remove(connection.To);
            connection.SetParent(null);
            connection.Attach(null);
        }

        //<inheritdoc>
        void ISelectable.OnDoubleClick()
        {
            IsExpanded = true;
            OnDoubleClicked();
        }

        //<inheritdoc>
        void IDraggable.OnStartDragging(IEditorContext context, Vector2 startPosition)
        {
            _tempPosition = _rect.position;
            IsDragged = true;
            OnStartedDragging(context, startPosition);
        }

        //<inheritdoc>
        void IDraggable.OnBeingDragged(IEditorContext context, Vector2 delta)
        {
            _tempPosition += delta;
            OnDragging(context, delta);
        }

        //<inheritdoc>
        void IDraggable.OnEndDragging(IEditorContext context, Vector2 delta, IDrawable n, DragAndDropMode mode, object data)
        {
            IsDragged = false;
            _tempPosition = Vector2.zero;
            if (mode != DragAndDropMode.Connection && delta.magnitude > 0.1f)
            {
                Rect rect = new Rect(_rect.position + delta, _rect.size);
                Vector2 position = context.CheckIfPositionInBounds(rect, out bool result);
                context.ProcessAction(new MoveNodeAction(this, position));
            }
            else
            {
                if (n != null && n is NodeBase nodeBase && data != null && CanHaveConnectionOfType(nodeBase, (Type)data) && nodeBase.CanHaveConnectionOfType((Type)data))
                {
                    ConnectionCreationContext connection = new ConnectionCreationContext(this, nodeBase, (Type)data);
                    context.ProcessAction(new CreateConnectionAction(connection));
                }
            }
            OnEndedDragging(context, delta, n, mode, data);
        }

        //<inheritdoc>
        void IHoverable.OnHoverEnter()
        {
            _isHovered = true;
            OnHoverEntered();
        }

        //<inheritdoc>
        void IHoverable.OnHoverExit()
        {
            _isHovered = false;
            OnHoverExited();
        }

        /// <summary>
        /// A callback to be called when the node is selected.
        /// </summary>
        protected virtual void OnSelected()
        {

        }

        /// <summary>
        /// A callback to be called when the node is deselected.
        /// </summary>
        protected virtual void OnDeselected()
        {

        }

        /// <summary>
        /// A callback to be called when the user started to drag the node.
        /// </summary>
        protected virtual void OnStartedDragging(IEditorContext context, Vector2 startPosition)
        {

        }

        /// <summary>
        /// A callback to be called when the user drags the node.
        /// </summary>
        protected virtual void OnDragging(IEditorContext context, Vector2 delta)
        {

        }

        /// <summary>
        /// A callback to be called when the user ended dragging the node.
        /// </summary>
        protected virtual void OnEndedDragging(IEditorContext context, Vector2 delta, IDrawable n, DragAndDropMode mode, object data)
        {
        }

        /// <summary>
        /// A callback to be called when the user right-clicked on the node.
        /// </summary>
        protected virtual void OnContextClick(IEditorContext context, Vector2 position, GenericMenu menu)
        {

        }

        /// <summary>
        /// A callback to be called when the user double-clicked on the node.
        /// </summary>
        protected virtual void OnDoubleClicked()
        {

        }

        /// <summary>
        /// A callback to be called when the mouse started hovering on the node.
        /// </summary>
        protected virtual void OnHoverEntered()
        {

        }

        /// <summary>
        /// A callback to be called when the mouse ended hovering on the node.
        /// </summary>
        protected virtual void OnHoverExited()
        {

        }

        private void StartNewConnection(IEditorContext context, NodeBase nodeBase, Type type)
        {
            context.DragAndDropHandler.StartConnection(context, nodeBase, type);
        }

        //<inheritdoc>
        public void Serialize(List<ISerialized> serializeds)
        {
            serializeds.Add(this);
        }

        //<inheritdoc>
        public void Deserialize(List<IDrawable> drawables)
        {
            drawables.Add(this);
            foreach (var connection in GetConnections())
            {
                drawables.Add(connection);
            }
        }

        //<inheritdoc>
        bool ISelectable.Contains(Vector2 position)
        {
            return Rect.Contains(position);
        }

        //<inheritdoc>
        bool ISelectable.Contains(Rect rect)
        {
            return Rect.Contains(rect);
        }

        //<inheritdoc>
        public void Hide()
        {
            IsHidden = true;
        }

        //<inheritdoc>
        public void Show()
        {
            IsHidden = false;
        }

        /// <summary>
        /// Returns if a connnection of type <paramref name="connectionType"/> can be established from this node to other. Works together with <see cref="CanHaveConnectionOfType(Type)"/> and 
        /// <see cref="CanHaveConnections"/>.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="connectionType"></param>
        /// <returns></returns>
        public bool CanHaveConnectionOfType(NodeBase to, Type connectionType)
        {
            if (to == null || to == this || !CanHaveConnections())
            {
                return false;
            }

            return !_connections.ContainsKey(to) && CanHaveConnectionOfType(connectionType);
        }

        /// <summary>
        /// Returns if a connection of type <paramref name="connectionType"/> can be established for node. See also: <seealso cref="CanHaveConnections"/>, <seealso cref="CanHaveConnectionOfType(NodeBase, Type)"/>.
        /// </summary>
        /// <param name="connectionType"></param>
        /// <returns></returns>
        protected virtual bool CanHaveConnectionOfType(Type connectionType)
        {
            return CanHaveConnections();
        }

        /// <summary>
        /// Returns if the node can have any connections at all. If false, connections points aren't drawn.
        /// </summary>
        /// <returns></returns>
        protected abstract bool CanHaveConnections();

        //<inheritdoc>
        public bool Match(string searchQuery)
        {
            foreach (var member in _assignees)
            {
                if (string.Equals(member.FullName, searchQuery, StringComparison.OrdinalIgnoreCase) || string.Equals(member.Role, searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return HeaderText.Equals(searchQuery, StringComparison.OrdinalIgnoreCase) || Id.Equals(searchQuery, StringComparison.OrdinalIgnoreCase);
        }
    }
}
