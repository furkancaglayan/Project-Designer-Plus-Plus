using System;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Data
{
    public interface IProjectDesignerInspector
    {
        string NodeTypeId { get; }
        int Priority { get; }
        VisualElement BuildInspector(ProjectBoardAsset board, BoardNodeModel node, IBoardCommandDispatcher dispatcher, Action repaint);
    }
}
