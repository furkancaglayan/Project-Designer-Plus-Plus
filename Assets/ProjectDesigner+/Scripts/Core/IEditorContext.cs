using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// Keeps track of the editor window context. One should be created per window.
    /// </summary>
    public interface IEditorContext
    {
        /// <summary>
        /// Processes the given action as <see cref="EditorContextAction"/>. This enables given actions to be registered as undo-redoable actions.
        /// </summary>
        /// <param name="action">EditorContextAction object</param>
        void ProcessAction(object action);
        /// <summary>
        /// Converts and returns a given canvas position to GUI position.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        Vector2 GetScreenPosition(Vector2 original);
        /// <summary>
        /// Drag and drop handler used in the editor.
        /// </summary>
        IDragAndDropHandler DragAndDropHandler { get; }
        /// <summary>
        /// Used to register an <see cref="IDrawable"/> for drawing on the editor.
        /// </summary>
        /// <param name="drawable">IDrawable object. For example <see cref="NodeBase"/>.</param>
        /// <param name="drawableCreationType">Creation type of the object.</param>
        void AddDrawable(IDrawable drawable, DrawableCreationType drawableCreationType = DrawableCreationType.Default);
        /// <summary>
        /// Deletes the given <see cref="IDrawable"/> from drawing list.
        /// </summary>
        /// <param name="drawable"></param>
        void DeleteDrawable(IDrawable drawable);
        /// <summary>
        /// Returns the available <see cref="ConnectionBase"/> types as <see cref="TemplateCollection.ConnectionTypeData"/>. Is used for 
        /// context menus.<br/>See <seealso cref="NodeBase.OnContextClick(IEditorContext, Vector2, GenericMenu)"/>
        /// </summary>
        /// <returns></returns>
        IEnumerable<TemplateCollection.ConnectionTypeData> GetAvailableConnectionTypes();
        /// <summary>
        /// Returns the current zoom factor used in the editor window.
        /// </summary>
        /// <returns></returns>
        float GetZoomFactor();
        /// <summary>
        /// Used for getting drawables of the given <typeparamref name="T"/> and satisfy the given <paramref name="func"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        List<T> GetDrawables<T>(Func<T, bool> func = null);
        /// <summary>
        /// Used for getting <see cref="IDrawable"/>s that satisfy the given <paramref name="func"/>.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        List<IDrawable> GetDrawables(Func<IDrawable, bool> func = null);
        /// <summary>
        /// Returns the defined <see cref="TeamMember"/>s in the project. 
        /// </summary>
        /// <returns></returns>
        ReadOnlyCollection<TeamMember> GetTeamMembers();
        /// <summary>
        /// Registers a log.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="description"></param>
        void RegisterLog(string text, string description);
        /// <summary>
        /// Registers a warning.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="description"></param>
        void RegisterWarning(string text, string description);
        /// <summary>
        /// Registers an error.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="description"></param>
        void RegisterError(string text, string description);
        /// <summary>
        /// Checks to see if given <paramref name="position"/> is inside the editor window. Can be used to check if the user is trying to get an <see cref="IDrawable"/>
        /// out of the window bounds. <br></br>
        /// <seealso cref="NodeBase.OnEndedDragging(IEditorContext, Vector2, IDrawable, IDragAndDropHandler.DragAndDropMode, object)"/>
        /// </summary>
        /// <param name="position"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        Vector2 CheckIfPositionInBounds(Rect position, out bool result);
        /// <summary>
        /// Converts screen position to "canvas" position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        Vector2 GetGUIPosition(Vector2 position);
        /// <summary>
        /// Returns the log saved to the context.
        /// </summary>
        /// <returns></returns>
        ReadOnlyCollection<EditorLogHistory.Log> GetLogs();
        /// <summary>
        /// Focuses a single <see cref="IDrawable"/>.
        /// </summary>
        /// <param name="nodeBase"></param>
        void Overview(IDrawable drawable);
        /// <summary>
        /// Use this function to show a generic menu outside of the zoom area. Showing dropdowns inside the zooming area (for example in a <see cref="MemberBase"/>), can be tricky,
        /// because <see cref="GenericMenu"/>s are shown in screen position.
        /// </summary>
        /// <param name="getGenericMenu">Menu to show.</param>
        void ShowDropdownOutsideZoomArea(Func<GenericMenu> getGenericMenu);
    }
}
