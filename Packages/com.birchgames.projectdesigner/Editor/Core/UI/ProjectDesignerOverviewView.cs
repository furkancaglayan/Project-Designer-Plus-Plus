using System.Linq;
using ProjectDesigner.V2.Data;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class ProjectDesignerOverviewView : VisualElement
    {
        public ProjectDesignerOverviewView()
        {
            AddToClassList("pd-overview");
        }

        public void Refresh(BoardDocument document)
        {
            Clear();
            if (document == null)
            {
                return;
            }

            Add(CreateMetric("Tasks", document.Nodes.OfType<TaskNodeModel>().Count().ToString()));
            Add(CreateMetric("In Progress", BoardInsights.CountTasksByStatus(document, TaskNodeStatus.InProgress).ToString()));
            Add(CreateMetric("Done", BoardInsights.CountTasksByStatus(document, TaskNodeStatus.Done).ToString()));
            Add(CreateMetric("Milestones", document.Nodes.OfType<MilestoneNodeModel>().Count().ToString()));
            Add(CreateMetric("References", document.Nodes.OfType<ReferenceNodeModel>().Count().ToString()));
            Add(CreateMetric("Technical", document.Nodes.OfType<ClassNodeModel>().Count().ToString()));
        }

        private static VisualElement CreateMetric(string label, string value)
        {
            var card = new VisualElement();
            card.AddToClassList("pd-overview-card");

            var valueLabel = new Label(value);
            valueLabel.AddToClassList("pd-overview-value");
            card.Add(valueLabel);

            var titleLabel = new Label(label);
            titleLabel.AddToClassList("pd-overview-label");
            card.Add(titleLabel);

            return card;
        }
    }
}
