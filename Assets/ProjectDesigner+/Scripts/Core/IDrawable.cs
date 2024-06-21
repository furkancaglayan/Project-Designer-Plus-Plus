using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Core
{
    public interface IDrawable
    {
        /// <summary>
        /// Draw function of any IDrawable. 
        /// </summary>
        /// <param name="context">Editor context</param>
        /// <param name="screenPosition">Screen position of the <see cref="IDrawable"/></param>
        /// <param name="mousePosition">Mouse position</param>
        void Draw(IEditorContext context, Vector2 screenPosition, Vector2 mousePosition);
        /// <summary>
        /// Rect of the <see cref="IDrawable"/>.
        /// </summary>
        Rect Rect {  get; }
        /// <summary>
        /// Unique identifier of the <see cref="IDrawable"/>.
        /// </summary>
        string Id { get; }
        /// <summary>
        /// If the IDrawable is currently hidden.
        /// </summary>
        bool IsHidden { get; }
        /// <summary>
        /// Hides the <see cref="IDrawable"/>. 
        /// </summary>
        void Hide();
        /// <summary>
        /// Shows the hidden <see cref="IDrawable"/>.
        /// </summary>
        void Show();
        /// <summary>
        /// Called after <see cref="IDrawable"/> is added to the editor context.
        /// </summary>
        /// <param name="context">Editor Context</param>
        /// <param name="drawableCreationType">Creation type of the <see cref="IDrawable"/>.</param>
        void OnAdded(IEditorContext context, DrawableCreationType drawableCreationType);
        /// <summary>
        /// Called after <see cref="IDrawable"/> is removed from the editor context.
        /// </summary>
        /// <param name="context">Editor Context</param>
        void OnRemoved(IEditorContext context);
        /// <summary>
        /// Used for matching a string with an <see cref="IDrawable"/>. Used for filtering IDrawables.
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        bool Match(string searchQuery);
    }
}
