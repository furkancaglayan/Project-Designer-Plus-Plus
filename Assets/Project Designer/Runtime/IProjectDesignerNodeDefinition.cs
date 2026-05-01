using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    public interface IProjectDesignerNodeDefinition
    {
        string TypeId { get; }
        string DisplayName { get; }
        string Description { get; }
        string Category { get; }
        string AccentColor { get; }
        Vector2 DefaultSize { get; }
        BoardNodeModel CreateDefaultNode(Vector2 position);
        string GetPreview(BoardNodeModel node, BoardDocument document);
    }
}
