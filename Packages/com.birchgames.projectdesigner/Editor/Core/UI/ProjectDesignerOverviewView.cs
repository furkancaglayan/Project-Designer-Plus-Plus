using System;
using System.Collections.Generic;
using System.Linq;
using ProjectDesigner.V2.Data;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class ProjectDesignerOverviewView : VisualElement
    {
        private readonly Action<string> _toggleQuickFilter;

        public ProjectDesignerOverviewView(Action<string> toggleQuickFilter)
        {
            _toggleQuickFilter = toggleQuickFilter;
            AddToClassList("pd-overview");
        }

        public void Refresh(BoardDocument document)
        {
            Clear();
            if (document == null)
            {
                return;
            }

            var metricsRow = new VisualElement();
            metricsRow.AddToClassList("pd-overview-row");
            Add(metricsRow);

            metricsRow.Add(CreateMetric("Tasks", document.Nodes.OfType<TaskNodeModel>().Count().ToString(), BoardQuickFilterIds.Tasks, document.ViewState.QuickFilterId));
            metricsRow.Add(CreateMetric("In Progress", BoardInsights.CountTasksByStatus(document, TaskNodeStatus.InProgress).ToString(), BoardQuickFilterIds.InProgress, document.ViewState.QuickFilterId));
            metricsRow.Add(CreateMetric("Blocked", BoardInsights.GetBlockedTasks(document).Count().ToString(), BoardQuickFilterIds.Blocked, document.ViewState.QuickFilterId));
            metricsRow.Add(CreateMetric("Due Soon", BoardInsights.GetDueSoonTasks(document).Count().ToString(), BoardQuickFilterIds.DueSoon, document.ViewState.QuickFilterId));
            metricsRow.Add(CreateMetric("Overdue", BoardInsights.GetOverdueTasks(document).Count().ToString(), BoardQuickFilterIds.Overdue, document.ViewState.QuickFilterId));
            metricsRow.Add(CreateMetric("Unassigned", BoardInsights.CountUnassignedOpenTasks(document).ToString(), BoardQuickFilterIds.Unassigned, document.ViewState.QuickFilterId));
            metricsRow.Add(CreateMetric("At Risk", BoardInsights.CountAtRiskNodes(document).ToString(), BoardQuickFilterIds.AtRisk, document.ViewState.QuickFilterId));
            metricsRow.Add(CreateMetric("Milestones", document.Nodes.OfType<MilestoneNodeModel>().Count().ToString(), BoardQuickFilterIds.Milestones, document.ViewState.QuickFilterId));

            IReadOnlyList<BoardAssigneeSummary> assignees = BoardInsights.GetAssigneeSummaries(document);
            if (assignees.Count == 0)
            {
                return;
            }

            var workloadRow = new VisualElement();
            workloadRow.AddToClassList("pd-overview-row");
            Add(workloadRow);

            foreach (BoardAssigneeSummary summary in assignees.Take(4))
            {
                workloadRow.Add(CreateWorkloadMetric(summary, document.ViewState.QuickFilterId));
            }
        }

        private VisualElement CreateMetric(string label, string value, string filterId, string activeFilterId)
        {
            var card = new VisualElement();
            card.AddToClassList("pd-overview-card");
            card.AddManipulator(new Clickable(() => ToggleFilter(filterId)));
            card.EnableInClassList("pd-overview-card-active", string.Equals(filterId, activeFilterId, StringComparison.Ordinal));

            var valueLabel = new Label(value);
            valueLabel.AddToClassList("pd-overview-value");
            card.Add(valueLabel);

            var titleLabel = new Label(label);
            titleLabel.AddToClassList("pd-overview-label");
            card.Add(titleLabel);

            return card;
        }

        private VisualElement CreateWorkloadMetric(BoardAssigneeSummary summary, string activeFilterId)
        {
            string filterId = BoardQuickFilterIds.ForAssignee(summary.Assignee);
            var card = new VisualElement();
            card.AddToClassList("pd-overview-card");
            card.AddToClassList("pd-overview-workload-card");
            card.AddManipulator(new Clickable(() => ToggleFilter(filterId)));
            card.EnableInClassList("pd-overview-card-active", string.Equals(filterId, activeFilterId, StringComparison.Ordinal));

            var valueLabel = new Label(summary.OpenTaskCount.ToString());
            valueLabel.AddToClassList("pd-overview-value");
            card.Add(valueLabel);

            var titleLabel = new Label(summary.Assignee);
            titleLabel.AddToClassList("pd-overview-label");
            card.Add(titleLabel);

            var detailLabel = new Label(summary.TotalEstimatePoints + " pts" + (summary.HasOverload ? " | Heavy" : string.Empty));
            detailLabel.AddToClassList("pd-overview-detail");
            card.Add(detailLabel);

            return card;
        }

        private void ToggleFilter(string filterId)
        {
            if (_toggleQuickFilter != null)
            {
                _toggleQuickFilter.Invoke(filterId);
            }
        }
    }
}
