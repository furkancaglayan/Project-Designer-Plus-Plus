using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// Used for making items hoverable in editor.
    /// </summary>
    public interface IHoverable : IDrawable
    {
        /// <summary>
        /// Called when mouse starts hovering over the item.
        /// </summary>
        void OnHoverEnter();
        /// <summary>
        /// Called when mouse exits the hovered item.
        /// </summary>
        void OnHoverExit();
    }
}
