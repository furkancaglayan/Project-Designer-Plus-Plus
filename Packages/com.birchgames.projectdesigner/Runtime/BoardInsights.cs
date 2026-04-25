using System.Collections.Generic;
using System.Linq;

namespace ProjectDesigner.V2.Data
{
    public static class BoardInsights
    {
        public static IEnumerable<BoardNodeModel> GetVisibleNodes(BoardDocument document)
        {
            if (document == null)
            {
                return Enumerable.Empty<BoardNodeModel>();
            }

            IEnumerable<BoardNodeModel> nodes = document.Nodes.Where(node => node != null);

            BoardSavedFilter activeFilter = document.SavedFilters
                .FirstOrDefault(filter => filter != null && filter.Id == document.ViewState.ActiveFilterId);

            if (activeFilter != null)
            {
                nodes = nodes.Where(node => activeFilter.Matches(node, document.ViewState.SearchQuery));
            }
            else
            {
                if (document.ViewState.Category != BoardNodeCategories.All)
                {
                    nodes = nodes.Where(node => node.Category == document.ViewState.Category);
                }

                if (!string.IsNullOrWhiteSpace(document.ViewState.SearchQuery))
                {
                    nodes = nodes.Where(node => node.MatchesSearch(document.ViewState.SearchQuery));
                }
            }

            return nodes;
        }

        public static float CalculateMilestoneCompletion(BoardDocument document, MilestoneNodeModel milestone)
        {
            if (document == null || milestone == null)
            {
                return 0f;
            }

            var linkedTasks = document.Edges
                .Where(edge => edge.TypeId == BoardEdgeTypeIds.Milestone && edge.TargetNodeId == milestone.Id)
                .Select(edge => document.GetNode(edge.SourceNodeId) as TaskNodeModel)
                .Where(task => task != null)
                .ToList();

            if (linkedTasks.Count == 0)
            {
                return 0f;
            }

            int completed = linkedTasks.Count(task => task.Status == TaskNodeStatus.Done);
            return completed / (float)linkedTasks.Count;
        }

        public static int CountTasksByStatus(BoardDocument document, TaskNodeStatus status)
        {
            if (document == null)
            {
                return 0;
            }

            return document.Nodes.OfType<TaskNodeModel>().Count(task => task.Status == status);
        }

        public static IEnumerable<TaskNodeModel> GetTasksForAssignee(BoardDocument document, string assignee)
        {
            if (document == null || string.IsNullOrWhiteSpace(assignee))
            {
                return Enumerable.Empty<TaskNodeModel>();
            }

            return document.Nodes
                .OfType<TaskNodeModel>()
                .Where(task => string.Equals(task.Assignee, assignee, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
