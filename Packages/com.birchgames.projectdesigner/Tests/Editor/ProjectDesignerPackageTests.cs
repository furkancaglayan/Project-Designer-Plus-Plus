using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectDesigner.V2.BuiltIn;
using ProjectDesigner.V2.Data;
using ProjectDesigner.V2.SampleExtension;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.V2.Tests
{
    public sealed class ProjectDesignerPackageTests
    {
        private readonly List<string> _temporaryAssets = new List<string>();

        [SetUp]
        public void SetUp()
        {
            ProjectDesignerRegistry.ResetForTests();
            ProjectDesignerBuiltInRegistration.Register();
            StatusReportExtensionRegistration.Register();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (string assetPath in _temporaryAssets)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            _temporaryAssets.Clear();
            AssetDatabase.Refresh();
        }

        [Test]
        public void BoardDocument_DeepClonePreservesTypedNodesAndEdges()
        {
            BoardDocument document = BoardPresetFactory.Create(BoardPresetIds.SoloIndie, "Clone Test");
            BoardDocument clone = document.DeepClone();

            Assert.AreEqual(document.Nodes.Count, clone.Nodes.Count);
            Assert.AreEqual(document.Edges.Count, clone.Edges.Count);
            Assert.IsTrue(clone.Nodes.OfType<TaskNodeModel>().Any());
            Assert.IsTrue(clone.Nodes.OfType<MilestoneNodeModel>().Any());
            Assert.AreEqual(document.Nodes[0].TypeId, clone.Nodes[0].TypeId);
        }

        [Test]
        public void ProjectBoardAsset_EditorJsonUtilityRoundTripsDocumentState()
        {
            ProjectBoardAsset board = ProjectBoardAsset.CreateTransient(BoardPresetFactory.Create(BoardPresetIds.SmallTeam, "Serialize Me"));
            string json = EditorJsonUtility.ToJson(board);

            ProjectBoardAsset restored = ProjectBoardAsset.CreateTransient();
            EditorJsonUtility.FromJsonOverwrite(json, restored);

            Assert.AreEqual(board.Document.Nodes.Count, restored.Document.Nodes.Count);
            Assert.AreEqual(board.Document.Edges.Count, restored.Document.Edges.Count);
            Assert.AreEqual(board.Document.BoardName, restored.Document.BoardName);
        }

        [Test]
        public void CommandStack_UndoRedoRestoresBoardChanges()
        {
            ProjectBoardAsset board = ProjectBoardAsset.CreateTransient(BoardPresetFactory.CreateEmpty("Undo Board"));
            ProjectDesignerCommandStack stack = new ProjectDesignerCommandStack(board);

            var task = new TaskNodeModel { Title = "Test Task", Position = new Vector2(100f, 100f) };
            stack.Execute(new CreateNodeCommand(board, task));
            Assert.AreEqual(1, board.Document.Nodes.Count);

            stack.Execute(new MoveNodeCommand(board, board.Document.Nodes[0].Id, new Vector2(320f, 180f)));
            Assert.AreEqual(new Vector2(320f, 180f), board.Document.Nodes[0].Position);

            stack.Undo();
            Assert.AreEqual(new Vector2(100f, 100f), board.Document.Nodes[0].Position);

            stack.Undo();
            Assert.AreEqual(0, board.Document.Nodes.Count);

            stack.Redo();
            stack.Redo();
            Assert.AreEqual(1, board.Document.Nodes.Count);
            Assert.AreEqual(new Vector2(320f, 180f), board.Document.Nodes[0].Position);
        }

        [Test]
        public void SavedFilter_MatchesNodesByCategoryAndSearchQuery()
        {
            BoardDocument document = BoardPresetFactory.CreateEmpty("Filter Test");
            var task = new TaskNodeModel { Title = "Prototype combat loop" };
            var note = new NoteNodeModel { Title = "Mood board references" };
            document.AddNode(task);
            document.AddNode(note);

            var filter = new BoardSavedFilter("Tasks", "prototype", BoardNodeCategories.Planning, string.Empty, true);
            document.UpsertFilter(filter);
            document.ViewState.ActiveFilterId = filter.Id;
            document.ViewState.SearchQuery = "prototype";

            List<BoardNodeModel> visible = BoardInsights.GetVisibleNodes(document).ToList();
            Assert.AreEqual(1, visible.Count);
            Assert.AreEqual(task.Id, visible[0].Id);
        }

        [Test]
        public void EdgeDefinitions_EnforcePlanningAndTechnicalRules()
        {
            BoardDocument document = BoardPresetFactory.CreateEmpty("Rules");
            var task = new TaskNodeModel();
            var secondTask = new TaskNodeModel();
            var milestone = new MilestoneNodeModel();
            var note = new NoteNodeModel();
            var classNode = new ClassNodeModel();

            document.AddNode(task);
            document.AddNode(secondTask);
            document.AddNode(milestone);
            document.AddNode(note);
            document.AddNode(classNode);

            IProjectDesignerEdgeDefinition dependency = ProjectDesignerRegistry.GetEdgeDefinition(BoardEdgeTypeIds.Dependency);
            IProjectDesignerEdgeDefinition milestoneEdge = ProjectDesignerRegistry.GetEdgeDefinition(BoardEdgeTypeIds.Milestone);
            IProjectDesignerEdgeDefinition technical = ProjectDesignerRegistry.GetEdgeDefinition(BoardEdgeTypeIds.TechnicalRelation);

            Assert.IsTrue(dependency.CanConnect(document, task, secondTask));
            Assert.IsFalse(dependency.CanConnect(document, task, note));
            Assert.IsTrue(milestoneEdge.CanConnect(document, task, milestone));
            Assert.IsFalse(milestoneEdge.CanConnect(document, milestone, task));
            Assert.IsFalse(technical.CanConnect(document, task, classNode));
            Assert.IsTrue(technical.CanConnect(document, classNode, new ClassNodeModel()));
        }

        [Test]
        public void BoardPresetFactory_CreatesPresetBoardsWithTemplates()
        {
            BoardDocument smallTeam = BoardPresetFactory.Create(BoardPresetIds.SmallTeam, "Team");
            BoardDocument technical = BoardPresetFactory.Create(BoardPresetIds.TechnicalDesign, "Tech");

            Assert.IsTrue(smallTeam.Templates.Count >= 4);
            Assert.IsTrue(smallTeam.Nodes.OfType<TaskNodeModel>().Any());
            Assert.IsTrue(smallTeam.Nodes.OfType<ReferenceNodeModel>().Any());
            Assert.IsTrue(technical.Nodes.OfType<ClassNodeModel>().Any());
            Assert.AreEqual(BoardNodeCategories.TechnicalDesign, technical.ViewState.Category);
        }

        [Test]
        public void AssetImporters_CreateReferenceAndClassNodesFromAssets()
        {
            string textPath = "Assets/__ProjectDesignerTemp/status_update.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(textPath));
            File.WriteAllText(textPath, "Status: vertical slice pacing needs tuning.");
            AssetDatabase.ImportAsset(textPath);
            _temporaryAssets.Add(textPath);

            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(textPath);
            MonoScript scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/ProjectDesigner+/Scripts/Data/Nodes/Task.cs");

            IProjectDesignerAssetImporter textImporter = ProjectDesignerRegistry.GetAssetImporters().First(importer => importer.CanImport(textAsset));
            IProjectDesignerAssetImporter scriptImporter = ProjectDesignerRegistry.GetAssetImporters().First(importer => importer.CanImport(scriptAsset));

            BoardNodeModel textNode = textImporter.Import(textAsset, Vector2.zero).First();
            BoardNodeModel classNode = scriptImporter.Import(scriptAsset, Vector2.zero).First();

            Assert.IsInstanceOf<StatusReportNodeModel>(textNode);
            Assert.IsInstanceOf<ClassNodeModel>(classNode);
        }

        [Test]
        public void SampleExtension_RegistersCustomNodeInspectorAndImporter()
        {
            IProjectDesignerNodeDefinition definition = ProjectDesignerRegistry.GetNodeDefinition(BoardNodeTypeIds.StatusReport);
            IProjectDesignerInspector inspector = ProjectDesignerRegistry.GetInspector(BoardNodeTypeIds.StatusReport);
            bool hasImporter = ProjectDesignerRegistry.GetAssetImporters().Any(importer => importer.GetType().Name == "StatusReportTextImporter");

            Assert.IsNotNull(definition);
            Assert.IsNotNull(inspector);
            Assert.IsTrue(hasImporter);
        }
    }
}
