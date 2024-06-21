using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// Refers the objects that can be dragged such as <see cref="NodeBase"/>.
    /// </summary>
    public interface IDraggable : IDrawable, ISelectable
    {
        /// <summary>
        /// Called once when an <see cref="IDraggable"/> has began being dragged.
        /// </summary>
        /// <param name="context">Editor Context</param>
        /// <param name="startPosition">Mouse position when starting drag.</param>
        void OnStartDragging(IEditorContext context, Vector2 startPosition);
        /// <summary>
        /// Called during an <see cref="IDrawable"/> is being dragged.
        /// </summary>
        /// <param name="context">Editor Context</param>
        /// <param name="delta">Change of position - delta.</param>
        void OnBeingDragged(IEditorContext context, Vector2 delta);
        /// <summary>
        /// Called when dragging of an <see cref="IDrawable"/> is ended.
        /// </summary>
        /// <param name="context">Editor context</param>
        /// <param name="delta">Total change of position from start of the dragging to the end.</param>
        /// <param name="endedOnTop">Refers to the <see cref="IDrawable"/> that the draggins ends on top of, if there is any.</param>
        /// <param name="mode">Mode of hte dragging</param>
        /// <param name="data">user data</param>
        void OnEndDragging(IEditorContext context, Vector2 delta, IDrawable endedOnTop, DragAndDropMode mode, object data = null);
    }
}
