using System;
using UnityEditor;
using UnityEngine;
using ProjectDesigner.Helpers;
using System.Collections.Generic;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// A base class to create a connection type. ConnectionBase implements IDrawable and ISelectable.
    /// When right clicking on a NodeBase, the types which are available for connection are chosen from all classes that implement <see cref="ConnectionBase"/> through <see cref="NodeBase.CanHaveConnectionOfType(NodeBase, Type)"/> and <see cref="NodeBase.CanHaveConnectionOfType(Type)"/> 
    /// To change the display name of the connection type in context menu, one can implement <seealso cref="ConnectionBaseMetaDataAttribute"/>.
    /// Be sure to use <see cref="SerializableAttribute"/> on your connections if you want to save variables.
    /// <code>
    /// [Serializable, ConnectionBaseMetaData("Inherit")]
    /// </code>
    /// See also: 
    /// <seealso cref="ConnectionBaseMetaDataAttribute"/>, 
    /// <seealso cref="NodeBase"/>
    /// </summary>
    [Serializable]
    public abstract class ConnectionBase : IDrawable, ISelectable
    {
        [SerializeReference]
        private NodeBase _to;
        [SerializeReference]
        private NodeBase _from;
        /// <summary>
        /// The NodeBase where the connections starts.
        /// </summary>
        public NodeBase From => _from;
        /// <summary>
        /// The NodeBase where the connection ends.
        /// </summary>
        public NodeBase To => _to;
        /// <summary>
        /// This is invalid.
        /// </summary>
        public Rect Rect { get; }
        //<inheritdoc>
        public bool IsSelected { get; private set; }
        //<inheritdoc>
        public bool IsHidden
        {
            get
            {
                return From == null || To == null || From.IsHidden || To.IsHidden;
            }
        }

        //<inheritdoc>
        [field: SerializeField]
        public string Id { get; set; }
        //<inheritdoc>
        public void Draw(IEditorContext context, Vector2 perceivedPosition, Vector2 mousePosition)
        {
            if (!IsHidden && From != null && To != null)
            {
                Color color = IsSelected ? Color.blue : Color.white;

                DrawConnection(context, context.GetScreenPosition(From.OutputPoint),
                                        context.GetScreenPosition(To.InputPoint),
                                        context.GetScreenPosition(From.Rect.center),
                                        context.GetScreenPosition(To.Rect.center),
                                        color);
            }
        }

        /// <summary>
        /// Draws the connection from <paramref name="fromOutputScreenPos"/> to <paramref name="toInputScreenPos"/>. The base implementation uses a curved connection with a triangle at the end to show the direction.
        /// See also:
        /// <seealso cref="GUIUtilities"/>
        /// </summary>
        /// <param name="context">The editor context </param>
        /// <param name="fromOutputScreenPos">Screen position of the start position</param>
        /// <param name="toInputScreenPos">Screen position of the end position</param>
        /// <param name="fromCenterScreenPos">Center of the starting node</param>
        /// <param name="toCenterScreenPos">Center of the end node</param>
        /// <param name="color"></param>
        protected virtual void DrawConnection(IEditorContext context, Vector2 fromOutputScreenPos, Vector2 toInputScreenPos, Vector2 fromCenterScreenPos, Vector2 toCenterScreenPos, Color color)
        {
            Vector2 startPoint = new Vector2(fromOutputScreenPos.x + 10, fromOutputScreenPos.y);
            Vector2 endPoint = new Vector2(toInputScreenPos.x - 22.5f, toInputScreenPos.y);
            Vector3[] points = GUIUtilities.GetCurvedPoints(startPoint, endPoint, 100f);
            GUIUtilities.DrawSolidLineArray(points, color, thickness: 5f);
            GUIUtilities.DrawTriangle(endPoint, 15f, color, Vector2.right);
        }

        /// <summary>
        /// Sets the parent node of the connection.
        /// </summary>
        /// <param name="node"></param>
        public void SetParent(NodeBase node)
        {
            _from = node;
        }

        /// <summary>
        /// Sets the final position of the connection.
        /// </summary>
        /// <param name="node"></param>
        public void Attach(NodeBase node)
        {
            _to = node;
        }

        //<inheritdoc>
        void IDrawable.OnAdded(IEditorContext context, DrawableCreationType drawableCreationType)
        {
        }

        //<inheritdoc>
        void IDrawable.OnRemoved(IEditorContext context)
        {
        }

        //<inheritdoc>
        void ISelectable.OnSelect()
        {
            IsSelected = true;
        }

        //<inheritdoc>
        void ISelectable.OnDeselect()
        {
            IsSelected = false;
        }

        //<inheritdoc>
        void ISelectable.OnContextClick(IEditorContext context, Vector2 position, GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Delete Connection"), false, context.ProcessAction, new DeleteConnectionAction(this));
        }

        //<inheritdoc>
        void ISelectable.OnDoubleClick()
        {
        }

        //<inheritdoc>
        public virtual bool Contains(Rect rect)
        {
            return ((ISelectable)this).Contains(rect.center);
        }

        //<inheritdoc>
        public virtual bool Contains(Vector2 position)
        {
            return HandleUtility.DistancePointLine(position, From.OutputPoint, To.InputPoint) < 16;
        }

        //<inheritdoc>
        void IDrawable.Hide()
        {
           
        }

        //<inheritdoc>
        void IDrawable.Show()
        {
          
        }

        //<inheritdoc>
        public bool Match(string searchQuery)
        {
            return true;
        }
    }
}
