using System;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// DefaultContextHandler can be used to add context menu options to the menu created by right-click on an empty space on Project Designer window.<br></br> Example: <br></br>
    /// <code>
    /// [DefaultContextHandler]
    /// static void CreateNewNode(IEditorContext context, Vector2 position, GenericMenu menu)
    /// </code>
    /// </summary>
    public class DefaultContextHandlerAttribute : Attribute
    {
        /// <summary>
        /// The function delegate of the context handler.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mousePosition"></param>
        /// <param name="menu"></param>
        public delegate void ContextMenuHandlerDelegate(IEditorContext context, Vector2 mousePosition, GenericMenu menu);
        public DefaultContextHandlerAttribute()
        {
        }
    }
}
