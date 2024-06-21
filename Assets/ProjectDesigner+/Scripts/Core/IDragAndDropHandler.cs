using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Core
{

    /// <summary>
    /// Used for when assets are moved from project browser to the editor window and released in <paramref name="position"/>.
    /// </summary>
    /// <param name="objects"></param>
    /// <param name="position"></param>
    public delegate void DragObjectsDelegate(UnityEngine.Object[] objects, Vector2 position);

    /// <summary>
    /// Dragging related event manager used in the editor.
    /// </summary>
    public interface IDragAndDropHandler
    {
        /// <summary>
        /// Used for registering a <see cref="DragObjectsDelegate"/> to the <see cref="IDragAndDropHandler"/>.
        /// </summary>
        /// <param name="dragObjectsDelegate"></param>
        void SetDragObjectsDelegate(DragObjectsDelegate dragObjectsDelegate);
        /// <summary>
        /// Keeps a rect starting from the drag start point to the mouse position.
        /// </summary>
        Rect SelectionRect { get; }
        /// <summary>
        /// OnGUI function is used to render drag & drop related gui.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="current"></param>
        void OnGUI(IEditorContext context, Event current);
        /// <summary>
        /// Current dragging mode.
        /// </summary>
        DragAndDropMode Mode { get; }
        /// <summary>
        /// Is currently dragging?
        /// </summary>
        bool IsDragging { get; }
        /// <summary>
        /// Called during dragging.
        /// </summary>
        /// <param name="context">Editor context</param>
        /// <param name="current">Current event</param>
        void OnDragging(IEditorContext context, Event current);
        /// <summary>
        /// Used to start dragging multiple <see cref="IDraggable"/> items. See also <seealso cref="DragAndDropMode.MultipleItems"/>
        /// </summary>
        /// <param name="context">Editor context</param>
        /// <param name="items"><see cref="IDraggable"/> items to drag.</param>
        /// <param name="position">Position where the dragging starts</param>
        void StartDragging(IEditorContext context, IEnumerable<IDraggable> items, Vector2 position);
        /// <summary>
        /// Used to start dragging single <see cref="IDraggable"/> item. See also <seealso cref="DragAndDropMode.MultipleItems"/>
        /// </summary>
        /// <param name="context">Editor context</param>
        /// <param name="item"><see cref="IDraggable"/> item to drag.</param>
        /// <param name="position">Position where the dragging starts</param>
        void StartDragging(IEditorContext context, IDraggable item, Vector2 position);
        /// <summary>
        /// Used to start dragging on empty mode. See also <seealso cref="DragAndDropMode.Empty"/>
        /// </summary>
        /// <param name="context">Editor context</param>
        /// <param name="position">Position where the dragging starts</param>
        void StartDraggingEmpty(IEditorContext context, Vector2 position);
        /// <summary>
        /// Used for starting a selection rect via dragging. See also <seealso cref="DragAndDropMode.Selection"/>
        /// </summary>
        /// <param name="context">Editor context</param>
        /// <param name="position">Position where the dragging starts</param>
        void StartDraggingSelection(IEditorContext context, Vector2 position);
        /// <summary>
        /// Used for when trying to connect a <see cref="ConnectionBase"/>. See also <seealso cref="DragAndDropMode.Connection"/>
        /// </summary>
        /// <param name="context">Editor context</param>
        /// <param name="nodeBase">The nodebase where the connection starts from.</param>
        /// <param name="type">Type of the connection</param>
        void StartConnection(IEditorContext context, NodeBase nodeBase, Type type);
        /// <summary>
        /// Ends the current dragging action.
        /// </summary>
        /// <param name="context">Editor context</param>
        /// <param name="current">Current event</param>
        /// <param name="top">The <see cref="IDrawable"/> where the selection ends on top of.</param>
        void EndDragging(IEditorContext context, Event current, IDrawable top);

    }
}
