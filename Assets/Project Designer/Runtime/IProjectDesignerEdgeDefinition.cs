namespace ProjectDesigner.V2.Data
{
    public interface IProjectDesignerEdgeDefinition
    {
        string TypeId { get; }
        string DisplayName { get; }
        string AccentColor { get; }
        bool CanConnect(BoardDocument document, BoardNodeModel source, BoardNodeModel target);
        string GetLabel(BoardEdgeModel edge, BoardDocument document);
    }
}
