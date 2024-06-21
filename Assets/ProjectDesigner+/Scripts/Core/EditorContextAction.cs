using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// EditorContextAction class is used to create undo-redoable Editor Window actions in Project Designer. It is advised to use this class to change the state of the editor.
    /// Inheriting classes must be serializable to support undo-redo operations. 
    /// Should be used with <see cref="IEditorContext.ProcessAction(object)"/> function to work.
    /// </summary>
    [Serializable]
    public abstract class EditorContextAction
    {
        [SerializeField]
        private string _title;
        [SerializeField]
        private string _description;

        /// <summary>
        /// Creates a context action given title and description. Parameters are used to show the context history.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        public EditorContextAction(string title, string description)
        {
            _title = title;
            _description = description;
        }

        /// <summary>
        /// Does the defined action. Used in <see cref="EditorContextHistory.Do(EditorContextAction, IEditorContext)"/> and <see cref="EditorContextHistory.Redo(IEditorContext)"/>.
        /// </summary>
        /// <param name="context"></param>
        public abstract void Do(IEditorContext context);

        /// <summary>
        /// Undoes the defined action. Used in <see cref="EditorContextHistory.Undo(IEditorContext)"/>.
        /// </summary>
        /// <param name="context"></param>
        public abstract void Undo(IEditorContext context);

        /// <summary>
        /// Gets action description.
        /// </summary>
        /// <returns></returns>
        public string GetDescription()
        {
           return _description;
        }

        /// <summary>
        /// Gets action title.
        /// </summary>
        /// <returns></returns>
        public string GetTitle()
        {
            return _title;
        }
    }
}
