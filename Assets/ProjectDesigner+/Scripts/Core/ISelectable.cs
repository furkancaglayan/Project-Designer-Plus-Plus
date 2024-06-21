using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// Used for making items selectable on editor.
    /// </summary>
    public interface ISelectable : IDrawable
    {
        /// <summary>
        /// Returns true if <paramref name="position"/> is inside the selectable item.
        /// </summary>
        /// <param name="position">Position to compare</param>
        /// <returns></returns>
        bool Contains(Vector2 position);
        /// <summary>
        /// Returns true if <paramref name="rect"/> is "mostly" inside the selectable item.
        /// </summary>
        /// <param name="rect">Rect to compare</param>
        /// <returns></returns>
        bool Contains(Rect rect);
        /// <summary>
        /// Called when a selectable item is selected via clicking.
        /// </summary>
        void OnSelect();
        /// <summary>
        /// Called when a selectable item is deselected via clicking elsewhere, or losing window focus.
        /// </summary>
        void OnDeselect();
        /// <summary>
        /// If the item is currently selected?
        /// </summary>
        bool IsSelected { get; }
        /// <summary>
        /// Called when a selectable item is right-click. Useful for adding custom menu options to IDrawable elements.
        /// </summary>
        /// <param name="context">Editor context</param>
        /// <param name="position">Mouse Position</param>
        /// <param name="menu">Generic menu that will be showed</param>
        void OnContextClick(IEditorContext context, Vector2 position, GenericMenu menu);
        /// <summary>
        /// Called when a selectable item is double clicked. Can be used to detect expanding.
        /// </summary>
        void OnDoubleClick();
    }
}
