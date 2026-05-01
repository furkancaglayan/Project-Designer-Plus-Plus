namespace ProjectDesigner.V2.Data
{
    public interface IProjectDesignerCommand
    {
        string DisplayName { get; }
        void Execute(ProjectBoardAsset board);
        void Undo(ProjectBoardAsset board);
    }
}
