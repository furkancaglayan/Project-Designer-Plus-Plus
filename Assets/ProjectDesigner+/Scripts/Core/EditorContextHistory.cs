using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// Responsible for doing and undoing of <see cref="EditorContextAction"/>s.
    /// </summary>
    [Serializable]
    public class EditorContextHistory
    {
        /// <summary>
        /// Max length of action lists.
        /// </summary>
        private const int MaxHistoryLength = 100;

        [SerializeReference]
        private readonly List<EditorContextAction> _undoList = new List<EditorContextAction>();
        [SerializeReference]
        private readonly List<EditorContextAction> _redoList = new List<EditorContextAction>();

        /// <summary>
        /// Applies the given action and adds it the "undo" list to use later.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="context"></param>
        public void Do(EditorContextAction action, IEditorContext context)
        {
            _undoList.Add(action);
            _redoList.Clear();
            TrimHistory(_undoList);
            action.Do(context);
            context.RegisterLog($"Do Context Action: {action.GetTitle()}", action.GetDescription());
        }

        /// <summary>
        /// Undoes last "done" action and add is to a "redo" list to use later.
        /// </summary>
        /// <param name="context"></param>
        public void Undo(IEditorContext context)
        {
            if (_undoList.Count > 0)
            {
                EditorContextAction action = _undoList[_undoList.Count - 1];
                _undoList.RemoveAt(_undoList.Count - 1);
                action.Undo(context);
                context.RegisterLog($"Undo Context Action: {action.GetTitle()}", action.GetDescription());
                _redoList.Add(action);
            }
        }


        /// <summary>
        /// Redoes the last "undone" action.
        /// </summary>
        /// <param name="context"></param>
        public void Redo(IEditorContext context)
        {
            if (_redoList.Count > 0)
            {
                EditorContextAction action = _redoList[_redoList.Count - 1];
                _redoList.RemoveAt(_redoList.Count - 1);
                action.Do(context);
                _undoList.Add(action);
            }
        }

        private void TrimHistory(List<EditorContextAction> historyList)
        {
            while (historyList.Count > MaxHistoryLength)
            {
                historyList.RemoveAt(0);
            }
        }
    }
}
