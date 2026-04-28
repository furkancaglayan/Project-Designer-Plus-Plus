using System;
using System.Collections.Generic;
using System.Linq;
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

    public sealed class DeleteNodesCommand : ProjectDesignerSnapshotCommand
    {
        public DeleteNodesCommand(ProjectBoardAsset board, IEnumerable<string> nodeIds)
            : base("Delete Cards", board.Document, BuildAfter(board.Document, nodeIds))
        {
        }

        private static BoardDocument BuildAfter(BoardDocument document, IEnumerable<string> nodeIds)
        {
            BoardDocument after = document.DeepClone();
            foreach (string nodeId in (nodeIds ?? Enumerable.Empty<string>()).Where(id => !string.IsNullOrEmpty(id)).Distinct())
            {
                after.RemoveNode(nodeId);
            }

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

    public sealed class MoveNodesCommand : ProjectDesignerSnapshotCommand
    {
        public MoveNodesCommand(ProjectBoardAsset board, IDictionary<string, Vector2> newPositions)
            : base("Move Cards", board.Document, BuildAfter(board.Document, newPositions))
        {
        }

        private static BoardDocument BuildAfter(BoardDocument document, IDictionary<string, Vector2> newPositions)
        {
            BoardDocument after = document.DeepClone();
            if (newPositions == null)
            {
                return after;
            }

            foreach (KeyValuePair<string, Vector2> pair in newPositions)
            {
                BoardNodeModel node = after.GetNode(pair.Key);
                if (node != null)
                {
                    node.Position = pair.Value;
                }
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

    public sealed class DuplicateNodesCommand : ProjectDesignerSnapshotCommand
    {
        public DuplicateNodesCommand(ProjectBoardAsset board, IEnumerable<string> nodeIds, Vector2 offset, bool snapToGrid)
            : base("Duplicate Cards", board.Document, BuildAfter(board.Document, nodeIds, offset, snapToGrid))
        {
        }

        private static BoardDocument BuildAfter(BoardDocument document, IEnumerable<string> nodeIds, Vector2 offset, bool snapToGrid)
        {
            BoardDocument after = document.DeepClone();
            List<string> orderedIds = (nodeIds ?? Enumerable.Empty<string>())
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            List<BoardNodeModel> originalNodes = orderedIds
                .Select(after.GetNode)
                .Where(node => node != null)
                .ToList();

            if (originalNodes.Count == 0)
            {
                return after;
            }

            var idMap = new Dictionary<string, string>();
            var duplicatedIds = new List<string>();
            foreach (BoardNodeModel originalNode in originalNodes)
            {
                BoardNodeModel duplicate = originalNode.Clone();
                duplicate.RegenerateId();
                duplicate.Position = snapToGrid
                    ? BoardLayoutUtility.SnapPosition(originalNode.Position + offset)
                    : originalNode.Position + offset;
                after.AddNode(duplicate);
                idMap[originalNode.Id] = duplicate.Id;
                duplicatedIds.Add(duplicate.Id);
            }

            List<BoardEdgeModel> duplicatedEdges = after.Edges
                .Where(edge => edge != null && idMap.ContainsKey(edge.SourceNodeId) && idMap.ContainsKey(edge.TargetNodeId))
                .Select(edge =>
                {
                    BoardEdgeModel duplicate = edge.Clone();
                    duplicate.RegenerateId();
                    duplicate.SourceNodeId = idMap[edge.SourceNodeId];
                    duplicate.TargetNodeId = idMap[edge.TargetNodeId];
                    return duplicate;
                })
                .ToList();

            foreach (BoardEdgeModel duplicatedEdge in duplicatedEdges)
            {
                after.AddEdge(duplicatedEdge);
            }

            after.ViewState.SetSelection(duplicatedIds, duplicatedIds[0]);
            return after;
        }
    }

    public sealed class ArrangeNodesCommand : ProjectDesignerSnapshotCommand
    {
        public ArrangeNodesCommand(ProjectBoardAsset board, IEnumerable<string> nodeIds, BoardArrangeMode arrangeMode)
            : base("Arrange Cards", board.Document, BuildAfter(board.Document, nodeIds, arrangeMode))
        {
        }

        private static BoardDocument BuildAfter(BoardDocument document, IEnumerable<string> nodeIds, BoardArrangeMode arrangeMode)
        {
            BoardDocument after = document.DeepClone();
            List<BoardNodeModel> nodes = (nodeIds ?? Enumerable.Empty<string>())
                .Select(after.GetNode)
                .Where(node => node != null)
                .ToList();

            Dictionary<string, Vector2> positions = BoardLayoutUtility.Arrange(nodes, arrangeMode, after.ViewState.SnapToGrid);
            foreach (KeyValuePair<string, Vector2> pair in positions)
            {
                BoardNodeModel node = after.GetNode(pair.Key);
                if (node != null)
                {
                    node.Position = pair.Value;
                }
            }

            return after;
        }
    }
}
