using System;

namespace ProjectDesigner.V2.Data
{
    public interface IBoardCommandDispatcher
    {
        ProjectBoardAsset BoardAsset { get; }
        bool CanUndo { get; }
        bool CanRedo { get; }
        event Action Changed;
        void Execute(IProjectDesignerCommand command);
        void Undo();
        void Redo();
    }
}
