using System.Collections.Generic;
using System.Linq;
using ProjectDesigner.V2.Data;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class ProjectDesignerLinkOption
    {
        public IProjectDesignerEdgeDefinition Definition;
        public BoardNodeModel OtherNode;
        public bool SelectedNodeIsSource;
        public string DisplayLabel;
    }

    internal static class ProjectDesignerLinkUtility
    {
        public static List<ProjectDesignerLinkOption> GetLinkOptions(BoardDocument document, BoardNodeModel selectedNode)
        {
            var options = new List<ProjectDesignerLinkOption>();
            if (document == null || selectedNode == null)
            {
                return options;
            }

            foreach (BoardNodeModel otherNode in document.Nodes.Where(node => node != null && node.Id != selectedNode.Id))
            {
                foreach (IProjectDesignerEdgeDefinition definition in ProjectDesignerRegistry.GetEdgeDefinitions())
                {
                    if (definition == null)
                    {
                        continue;
                    }

                    if (definition.CanConnect(document, selectedNode, otherNode))
                    {
                        options.Add(new ProjectDesignerLinkOption
                        {
                            Definition = definition,
                            OtherNode = otherNode,
                            SelectedNodeIsSource = true
                        });
                        continue;
                    }

                    if (definition.CanConnect(document, otherNode, selectedNode))
                    {
                        options.Add(new ProjectDesignerLinkOption
                        {
                            Definition = definition,
                            OtherNode = otherNode,
                            SelectedNodeIsSource = false
                        });
                    }
                }
            }

            options = options
                .GroupBy(option => option.Definition.TypeId + "|" + option.OtherNode.Id + "|" + option.SelectedNodeIsSource)
                .Select(group => group.First())
                .OrderBy(option => option.Definition.DisplayName)
                .ThenBy(option => option.OtherNode.Title)
                .ThenBy(option => option.OtherNode.Id)
                .ToList();

            foreach (IGrouping<string, ProjectDesignerLinkOption> group in options.GroupBy(GetBaseLabel))
            {
                int count = 0;
                bool needsDisambiguation = group.Count() > 1;
                foreach (ProjectDesignerLinkOption option in group)
                {
                    count++;
                    option.DisplayLabel = needsDisambiguation
                        ? GetBaseLabel(option) + " | #" + count
                        : GetBaseLabel(option);
                }
            }

            return options;
        }

        private static string GetBaseLabel(ProjectDesignerLinkOption option)
        {
            if (option == null || option.OtherNode == null)
            {
                return string.Empty;
            }

            string label = option.OtherNode.Title + " (" + option.OtherNode.Category + ")";
            if (!option.SelectedNodeIsSource)
            {
                label += " | incoming";
            }

            return label;
        }
    }
}
