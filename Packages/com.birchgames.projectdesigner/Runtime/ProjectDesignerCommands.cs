using System;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    public abstract class ProjectDesignerSnapshotCommand : IProjectDesignerCommand
    {
        private readonly BoardDocument _before;
        private readonly BoardDocument _after;

        public string DisplayName { get; private set; }

        protected ProjectDesignerSnapshotCommand(string displayName, BoardDocument before, BoardDocument after)
        {
            DisplayName = displayName;
            _before = before == null ? new BoardDocument(ProjectDesignerProductInfo.DefaultBoardName) : before.DeepClone();
            _after = after == null ? new BoardDocument(ProjectDesignerProductInfo.DefaultBoardName) : after.DeepClone();
        }

        public void Execute(ProjectBoardAsset board)
        {
            if (board != null)
            {
                board.ResetDocument(_after);
            }
        }

        public void Undo(ProjectBoardAsset board)
        {
            if (board != null)
            {
                board.ResetDocument(_before);
            }
        }
    }

    public sealed class BoardMutationCommand : ProjectDesignerSnapshotCommand
    {
        public BoardMutationCommand(string displayName, BoardDocument before, BoardDocument after)
            : base(displayName, before, after)
        {
        }

        public static BoardMutationCommand Create(ProjectBoardAsset board, string displayName, Action<BoardDocument> mutate)
        {
            BoardDocument before = board.Document.DeepClone();
            BoardDocument after = before.DeepClone();
            if (mutate != null)
            {
                mutate(after);
            }

            return new BoardMutationCommand(displayName, before, after);
        }
    }

    public sealed class CreateNodeCommand : ProjectDesignerSnapshotCommand
    {
        public CreateNodeCommand(ProjectBoardAsset board, BoardNodeModel node)
            : base("Create Node", board.Document, BuildAfter(board.Document, node))
        {
        }

        private static BoardDocument BuildAfter(BoardDocument document, BoardNodeModel node)
        {
            BoardDocument after = document.DeepClone();
            after.AddNode(node == null ? null : node.Clone());
            return after;
        }
    }

    public sealed class UpdateNodeCommand : ProjectDesignerSnapshotCommand
    {
        public UpdateNodeCommand(ProjectBoardAsset board, BoardNodeModel node)
            : base("Update Node", board.Document, BuildAfter(board.Document, node))
        {
        }

        private static BoardDocument BuildAfter(BoardDocument document, BoardNodeModel node)
        {
            BoardDocument after = document.DeepClone();
            after.ReplaceNode(node == null ? null : node.Clone());
            return after;
        }
    }

    public sealed class DeleteNodeCommand : ProjectDesignerSnapshotCommand
    {
        public DeleteNodeCommand(ProjectBoardAsset board, string nodeId)
            : base("Delete Node", board.Document, BuildAfter(board.Document, nodeId))
        {
        }

        private static BoardDocument BuildAfter(BoardDocument document, string nodeId)
        {
            BoardDocument after = document.DeepClone();
            after.RemoveNode(nodeId);
            return after;
        }
    }

    public sealed class MoveNodeCommand : ProjectDesignerSnapshotCommand
    {
        public MoveNodeCommand(ProjectBoardAsset board, string nodeId, Vector2 newPosition)
            : base("Move Node", board.Document, BuildAfter(board.Document, nodeId, newPosition))
        {
        }

        private static BoardDocument BuildAfter(BoardDocument document, string nodeId, Vector2 newPosition)
        {
            BoardDocument after = document.DeepClone();
            BoardNodeModel node = after.GetNode(nodeId);
            if (node != null)
            {
                node.Position = newPosition;
            }

            return after;
        }
    }

    public sealed class CreateEdgeCommand : ProjectDesignerSnapshotCommand
    {
        public CreateEdgeCommand(ProjectBoardAsset board, BoardEdgeModel edge)
            : base("Create Edge", board.Document, BuildAfter(board.Document, edge))
        {
        }

        private static BoardDocument BuildAfter(BoardDocument document, BoardEdgeModel edge)
        {
            BoardDocument after = document.DeepClone();
            after.AddEdge(edge == null ? null : edge.Clone());
            return after;
        }
    }

    public sealed class DeleteEdgeCommand : ProjectDesignerSnapshotCommand
    {
        public DeleteEdgeCommand(ProjectBoardAsset board, string edgeId)
            : base("Delete Edge", board.Document, BuildAfter(board.Document, edgeId))
        {
        }

        private static BoardDocument BuildAfter(BoardDocument document, string edgeId)
        {
            BoardDocument after = document.DeepClone();
            after.RemoveEdge(edgeId);
            return after;
        }
    }

    public sealed class SetFilterStateCommand : ProjectDesignerSnapshotCommand
    {
        public SetFilterStateCommand(ProjectBoardAsset board, BoardViewState viewState)
            : base("Set Filter State", board.Document, BuildAfter(board.Document, viewState))
        {
        }

        private static BoardDocument BuildAfter(BoardDocument document, BoardViewState viewState)
        {
            BoardDocument after = document.DeepClone();
            after.ViewState = viewState == null ? new BoardViewState() : viewState.Clone();
            return after;
        }
    }

    public sealed class SaveFilterCommand : ProjectDesignerSnapshotCommand
    {
        public SaveFilterCommand(ProjectBoardAsset board, BoardSavedFilter filter)
            : base("Save Filter", board.Document, BuildAfter(board.Document, filter))
        {
        }

        private static BoardDocument BuildAfter(BoardDocument document, BoardSavedFilter filter)
        {
            BoardDocument after = document.DeepClone();
            after.UpsertFilter(filter == null ? null : filter.Clone());
            return after;
        }
    }
}
