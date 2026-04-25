using System.Collections.Generic;
using System.Linq;

namespace ProjectDesigner.V2.Data
{
    public static class ProjectDesignerRegistry
    {
        private static readonly Dictionary<string, IProjectDesignerNodeDefinition> NodeDefinitions = new Dictionary<string, IProjectDesignerNodeDefinition>();
        private static readonly Dictionary<string, IProjectDesignerEdgeDefinition> EdgeDefinitions = new Dictionary<string, IProjectDesignerEdgeDefinition>();
        private static readonly List<IProjectDesignerInspector> Inspectors = new List<IProjectDesignerInspector>();
        private static readonly List<IProjectDesignerAssetImporter> AssetImporters = new List<IProjectDesignerAssetImporter>();

        public static IEnumerable<IProjectDesignerNodeDefinition> GetNodeDefinitions()
        {
            return NodeDefinitions.Values.OrderBy(definition => definition.Category).ThenBy(definition => definition.DisplayName);
        }

        public static IEnumerable<IProjectDesignerEdgeDefinition> GetEdgeDefinitions()
        {
            return EdgeDefinitions.Values.OrderBy(definition => definition.DisplayName);
        }

        public static IEnumerable<IProjectDesignerAssetImporter> GetAssetImporters()
        {
            return AssetImporters.OrderByDescending(importer => importer.Priority);
        }

        public static void RegisterNodeDefinition(IProjectDesignerNodeDefinition definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.TypeId))
            {
                return;
            }

            NodeDefinitions[definition.TypeId] = definition;
        }

        public static void RegisterEdgeDefinition(IProjectDesignerEdgeDefinition definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.TypeId))
            {
                return;
            }

            EdgeDefinitions[definition.TypeId] = definition;
        }

        public static void RegisterInspector(IProjectDesignerInspector inspector)
        {
            if (inspector == null)
            {
                return;
            }

            Inspectors.RemoveAll(existing =>
                existing.GetType() == inspector.GetType() ||
                (existing.NodeTypeId == inspector.NodeTypeId && existing.Priority <= inspector.Priority));
            Inspectors.Add(inspector);
        }

        public static void RegisterAssetImporter(IProjectDesignerAssetImporter importer)
        {
            if (importer == null)
            {
                return;
            }

            AssetImporters.RemoveAll(existing => existing.GetType() == importer.GetType());
            AssetImporters.Add(importer);
        }

        public static IProjectDesignerNodeDefinition GetNodeDefinition(string typeId)
        {
            IProjectDesignerNodeDefinition definition;
            NodeDefinitions.TryGetValue(typeId, out definition);
            return definition;
        }

        public static IProjectDesignerEdgeDefinition GetEdgeDefinition(string typeId)
        {
            IProjectDesignerEdgeDefinition definition;
            EdgeDefinitions.TryGetValue(typeId, out definition);
            return definition;
        }

        public static IProjectDesignerInspector GetInspector(string nodeTypeId)
        {
            return Inspectors
                .Where(inspector => inspector.NodeTypeId == nodeTypeId)
                .OrderByDescending(inspector => inspector.Priority)
                .FirstOrDefault();
        }

        public static void ResetForTests()
        {
            NodeDefinitions.Clear();
            EdgeDefinitions.Clear();
            Inspectors.Clear();
            AssetImporters.Clear();
        }
    }
}
