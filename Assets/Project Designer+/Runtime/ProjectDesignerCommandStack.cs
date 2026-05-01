using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    public sealed class ProjectDesignerCommandStack : IBoardCommandDispatcher
    {
        private const int MaxHistory = 128;

        private readonly Stack<IProjectDesignerCommand> _undo = new Stack<IProjectDesignerCommand>();
        private readonly Stack<IProjectDesignerCommand> _redo = new Stack<IProjectDesignerCommand>();
        private readonly Action _afterChange;

        public ProjectBoardAsset BoardAsset { get; private set; }

        public bool CanUndo
        {
            get { return _undo.Count > 0; }
        }

        public bool CanRedo
        {
            get { return _redo.Count > 0; }
        }

        public event Action Changed;

        public ProjectDesignerCommandStack(ProjectBoardAsset boardAsset, Action afterChange = null)
        {
            BoardAsset = boardAsset;
            _afterChange = afterChange;
        }

        public void Execute(IProjectDesignerCommand command)
        {
            if (command == null || BoardAsset == null)
            {
                return;
            }

            command.Execute(BoardAsset);
            _undo.Push(command);
            _redo.Clear();
            TrimUndoHistory();
            RaiseChanged();
        }

        public void Undo()
        {
            if (!CanUndo || BoardAsset == null)
            {
                return;
            }

            IProjectDesignerCommand command = _undo.Pop();
            command.Undo(BoardAsset);
            _redo.Push(command);
            RaiseChanged();
        }

        public void Redo()
        {
            if (!CanRedo || BoardAsset == null)
            {
                return;
            }

            IProjectDesignerCommand command = _redo.Pop();
            command.Execute(BoardAsset);
            _undo.Push(command);
            RaiseChanged();
        }

        private void TrimUndoHistory()
        {
            if (_undo.Count <= MaxHistory)
            {
                return;
            }

            IProjectDesignerCommand[] commands = _undo.ToArray();
            Array.Reverse(commands);
            _undo.Clear();
            int startIndex = Mathf.Max(0, commands.Length - MaxHistory);
            for (int index = startIndex; index < commands.Length; index++)
            {
                _undo.Push(commands[index]);
            }
        }

        private void RaiseChanged()
        {
            if (_afterChange != null)
            {
                _afterChange.Invoke();
            }

            if (Changed != null)
            {
                Changed.Invoke();
            }
        }
    }
}
