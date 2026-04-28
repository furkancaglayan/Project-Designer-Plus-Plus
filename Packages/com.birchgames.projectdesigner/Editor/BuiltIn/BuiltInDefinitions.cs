using ProjectDesigner.V2.Data;
using UnityEngine;

namespace ProjectDesigner.V2.BuiltIn
{
    internal sealed class TaskNodeDefinition : IProjectDesignerNodeDefinition
    {
        public string TypeId { get { return BoardNodeTypeIds.Task; } }
        public string DisplayName { get { return "Task"; } }
        public string Description { get { return "Track actionable work with assignees, estimates, priority, and due dates."; } }
        public string Category { get { return BoardNodeCategories.Planning; } }
        public string AccentColor { get { return "#FF8A3D"; } }
        public Vector2 DefaultSize { get { return new Vector2(320f, 220f); } }

        public BoardNodeModel CreateDefaultNode(Vector2 position)
        {
            var node = new TaskNodeModel();
            node.Position = position;
            node.Size = DefaultSize;
            return node;
        }

        public string GetPreview(BoardNodeModel node, BoardDocument document)
        {
            TaskNodeModel task = node as TaskNodeModel;
            if (task == null)
            {
                return string.Empty;
            }

            string assignee = string.IsNullOrEmpty(task.Assignee) ? "Unassigned" : task.Assignee;
            return task.Status + " | " + task.Priority + " | " + assignee;
        }
    }

    internal sealed class ProjectBriefNodeDefinition : IProjectDesignerNodeDefinition
    {
        public string TypeId { get { return BoardNodeTypeIds.ProjectBrief; } }
        public string DisplayName { get { return ProjectDesignerProductInfo.ProjectBriefTitle; } }
        public string Description { get { return "A sticky planning card for the high-level pitch, team snapshot, and shared project knowledge."; } }
        public string Category { get { return BoardNodeCategories.Planning; } }
        public string AccentColor { get { return "#F5C451"; } }
        public Vector2 DefaultSize { get { return new Vector2(360f, 280f); } }

        public BoardNodeModel CreateDefaultNode(Vector2 position)
        {
            var node = new ProjectBriefNodeModel();
            node.Position = position;
            node.Size = DefaultSize;
            node.IsPinned = true;
            return node;
        }

        public string GetPreview(BoardNodeModel node, BoardDocument document)
        {
            ProjectBriefNodeModel brief = node as ProjectBriefNodeModel;
            if (brief == null)
            {
                return string.Empty;
            }

            string team = string.IsNullOrWhiteSpace(brief.TeamSnapshot) ? "Team snapshot pending" : brief.TeamSnapshot;
            return team + " | " + brief.Overview;
        }
    }

    internal sealed class MilestoneNodeDefinition : IProjectDesignerNodeDefinition
    {
        public string TypeId { get { return BoardNodeTypeIds.Milestone; } }
        public string DisplayName { get { return "Milestone"; } }
        public string Description { get { return "Aggregate progress for a major pre-production checkpoint."; } }
        public string Category { get { return BoardNodeCategories.Planning; } }
        public string AccentColor { get { return "#F4C542"; } }
        public Vector2 DefaultSize { get { return new Vector2(320f, 200f); } }

        public BoardNodeModel CreateDefaultNode(Vector2 position)
        {
            var node = new MilestoneNodeModel();
            node.Position = position;
            node.Size = DefaultSize;
            return node;
        }

        public string GetPreview(BoardNodeModel node, BoardDocument document)
        {
            MilestoneNodeModel milestone = node as MilestoneNodeModel;
            if (milestone == null)
            {
                return string.Empty;
            }

            float progress = BoardInsights.CalculateMilestoneCompletion(document, milestone);
            return Mathf.RoundToInt(progress * 100f) + "% complete";
        }
    }

    internal sealed class NoteNodeDefinition : IProjectDesignerNodeDefinition
    {
        public string TypeId { get { return BoardNodeTypeIds.Note; } }
        public string DisplayName { get { return "Note"; } }
        public string Description { get { return "Capture ideas, questions, and meeting notes alongside the board."; } }
        public string Category { get { return BoardNodeCategories.Reference; } }
        public string AccentColor { get { return "#2B90D9"; } }
        public Vector2 DefaultSize { get { return new Vector2(300f, 220f); } }

        public BoardNodeModel CreateDefaultNode(Vector2 position)
        {
            var node = new NoteNodeModel();
            node.Position = position;
            node.Size = DefaultSize;
            return node;
        }

        public string GetPreview(BoardNodeModel node, BoardDocument document)
        {
            NoteNodeModel note = node as NoteNodeModel;
            return note == null ? string.Empty : note.Body;
        }
    }

    internal sealed class ReferenceNodeDefinition : IProjectDesignerNodeDefinition
    {
        public string TypeId { get { return BoardNodeTypeIds.Reference; } }
        public string DisplayName { get { return "Reference"; } }
        public string Description { get { return "Keep assets, screenshots, and text references close to the plan."; } }
        public string Category { get { return BoardNodeCategories.Reference; } }
        public string AccentColor { get { return "#5EC27F"; } }
        public Vector2 DefaultSize { get { return new Vector2(320f, 220f); } }

        public BoardNodeModel CreateDefaultNode(Vector2 position)
        {
            var node = new ReferenceNodeModel();
            node.Position = position;
            node.Size = DefaultSize;
            return node;
        }

        public string GetPreview(BoardNodeModel node, BoardDocument document)
        {
            ReferenceNodeModel reference = node as ReferenceNodeModel;
            if (reference == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(reference.AssetPath))
            {
                return reference.AssetPath;
            }

            if (!string.IsNullOrEmpty(reference.ExternalUrl))
            {
                return reference.ExternalUrl;
            }

            return reference.Summary;
        }
    }

    internal sealed class ClassNodeDefinition : IProjectDesignerNodeDefinition
    {
        public string TypeId { get { return BoardNodeTypeIds.Class; } }
        public string DisplayName { get { return "Class"; } }
        public string Description { get { return "Secondary technical design support for code ownership and structure."; } }
        public string Category { get { return BoardNodeCategories.TechnicalDesign; } }
        public string AccentColor { get { return "#B883FF"; } }
        public Vector2 DefaultSize { get { return new Vector2(340f, 260f); } }

        public BoardNodeModel CreateDefaultNode(Vector2 position)
        {
            var node = new ClassNodeModel();
            node.Position = position;
            node.Size = DefaultSize;
            return node;
        }

        public string GetPreview(BoardNodeModel node, BoardDocument document)
        {
            ClassNodeModel classNode = node as ClassNodeModel;
            if (classNode == null)
            {
                return string.Empty;
            }

            return classNode.NamespaceName + " | " + classNode.Fields.Count + " fields | " + classNode.Methods.Count + " methods";
        }
    }

    internal sealed class DependencyEdgeDefinition : IProjectDesignerEdgeDefinition
    {
        public string TypeId { get { return BoardEdgeTypeIds.Dependency; } }
        public string DisplayName { get { return "Dependency"; } }
        public string AccentColor { get { return "#FF8A3D"; } }

        public bool CanConnect(BoardDocument document, BoardNodeModel source, BoardNodeModel target)
        {
            return source != null && target != null && source.Id != target.Id &&
                   source.TypeId == BoardNodeTypeIds.Task &&
                   target.TypeId == BoardNodeTypeIds.Task;
        }

        public string GetLabel(BoardEdgeModel edge, BoardDocument document)
        {
            return string.IsNullOrEmpty(edge.Label) ? "depends on" : edge.Label;
        }
    }

    internal sealed class MilestoneEdgeDefinition : IProjectDesignerEdgeDefinition
    {
        public string TypeId { get { return BoardEdgeTypeIds.Milestone; } }
        public string DisplayName { get { return "Milestone Link"; } }
        public string AccentColor { get { return "#F4C542"; } }

        public bool CanConnect(BoardDocument document, BoardNodeModel source, BoardNodeModel target)
        {
            return source != null && target != null &&
                   source.TypeId == BoardNodeTypeIds.Task &&
                   target.TypeId == BoardNodeTypeIds.Milestone;
        }

        public string GetLabel(BoardEdgeModel edge, BoardDocument document)
        {
            return string.IsNullOrEmpty(edge.Label) ? "belongs to" : edge.Label;
        }
    }

    internal sealed class ReferenceEdgeDefinition : IProjectDesignerEdgeDefinition
    {
        public string TypeId { get { return BoardEdgeTypeIds.Reference; } }
        public string DisplayName { get { return "Reference Link"; } }
        public string AccentColor { get { return "#5EC27F"; } }

        public bool CanConnect(BoardDocument document, BoardNodeModel source, BoardNodeModel target)
        {
            return source != null && target != null && source.Id != target.Id;
        }

        public string GetLabel(BoardEdgeModel edge, BoardDocument document)
        {
            return string.IsNullOrEmpty(edge.Label) ? "references" : edge.Label;
        }
    }

    internal sealed class TechnicalRelationEdgeDefinition : IProjectDesignerEdgeDefinition
    {
        public string TypeId { get { return BoardEdgeTypeIds.TechnicalRelation; } }
        public string DisplayName { get { return "Technical Relation"; } }
        public string AccentColor { get { return "#B883FF"; } }

        public bool CanConnect(BoardDocument document, BoardNodeModel source, BoardNodeModel target)
        {
            return source != null && target != null &&
                   source.TypeId == BoardNodeTypeIds.Class &&
                   target.TypeId == BoardNodeTypeIds.Class &&
                   source.Id != target.Id;
        }

        public string GetLabel(BoardEdgeModel edge, BoardDocument document)
        {
            return string.IsNullOrEmpty(edge.Label) ? "relates to" : edge.Label;
        }
    }
}
