using UnityEditor;
using ProjectDesigner.V2.Data;

namespace ProjectDesigner.V2.BuiltIn
{
    [InitializeOnLoad]
    public static class ProjectDesignerBuiltInRegistration
    {
        static ProjectDesignerBuiltInRegistration()
        {
            Register();
        }

        public static void Register()
        {
            ProjectDesignerRegistry.RegisterNodeDefinition(new TaskNodeDefinition());
            ProjectDesignerRegistry.RegisterNodeDefinition(new MilestoneNodeDefinition());
            ProjectDesignerRegistry.RegisterNodeDefinition(new NoteNodeDefinition());
            ProjectDesignerRegistry.RegisterNodeDefinition(new ReferenceNodeDefinition());
            ProjectDesignerRegistry.RegisterNodeDefinition(new ClassNodeDefinition());

            ProjectDesignerRegistry.RegisterEdgeDefinition(new DependencyEdgeDefinition());
            ProjectDesignerRegistry.RegisterEdgeDefinition(new MilestoneEdgeDefinition());
            ProjectDesignerRegistry.RegisterEdgeDefinition(new ReferenceEdgeDefinition());
            ProjectDesignerRegistry.RegisterEdgeDefinition(new TechnicalRelationEdgeDefinition());

            ProjectDesignerRegistry.RegisterInspector(new TaskNodeInspector());
            ProjectDesignerRegistry.RegisterInspector(new MilestoneNodeInspector());
            ProjectDesignerRegistry.RegisterInspector(new NoteNodeInspector());
            ProjectDesignerRegistry.RegisterInspector(new ReferenceNodeInspector());
            ProjectDesignerRegistry.RegisterInspector(new ClassNodeInspector());

            ProjectDesignerRegistry.RegisterAssetImporter(new MonoScriptClassImporter());
            ProjectDesignerRegistry.RegisterAssetImporter(new TextureReferenceImporter());
            ProjectDesignerRegistry.RegisterAssetImporter(new TextAssetReferenceImporter());
            ProjectDesignerRegistry.RegisterAssetImporter(new GenericObjectReferenceImporter());
        }
    }
}
