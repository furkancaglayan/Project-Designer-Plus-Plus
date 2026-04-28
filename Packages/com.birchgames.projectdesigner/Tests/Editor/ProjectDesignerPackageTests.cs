using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectDesigner.V2.BuiltIn;
using ProjectDesigner.V2.Data;
using ProjectDesigner.V2.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Tests
{
    public enum TestExtensionHealth
    {
        Good,
        Warning,
        Critical
    }

    [System.Serializable]
    public sealed class TestExtensionNodeModel : BoardNodeModel
    {
        [SerializeField]
        private string _summary;
        [SerializeField]
        private TestExtensionHealth _health;

        public override string Category
        {
            get { return BoardNodeCategories.Planning; }
        }

        public string Summary
        {
            get { return _summary; }
            set { _summary = value ?? string.Empty; }
        }

        public TestExtensionHealth Health
        {
            get { return _health; }
            set { _health = value; }
        }

        public TestExtensionNodeModel()
            : base("tests.extension.node", "Test Extension", new Vector2(100f, 100f), new Vector2(280f, 180f))
        {
            _summary = "Test node summary";
            _health = TestExtensionHealth.Good;
        }

        public override BoardNodeModel Clone()
        {
            var clone = new TestExtensionNodeModel
            {
                Summary = Summary,
                Health = Health
            };
            CopyCommonTo(clone);
            return clone;
        }
    }

    internal sealed class TestExtensionNodeDefinition : IProjectDesignerNodeDefinition
    {
        public string TypeId { get { return "tests.extension.node"; } }
        public string DisplayName { get { return "Test Extension"; } }
        public string Description { get { return "Test extension node."; } }
        public string Category { get { return BoardNodeCategories.Planning; } }
        public string AccentColor { get { return "#55D6BE"; } }
        public Vector2 DefaultSize { get { return new Vector2(280f, 180f); } }

        public BoardNodeModel CreateDefaultNode(Vector2 position)
        {
            var node = new TestExtensionNodeModel();
            node.Position = position;
            return node;
        }

        public string GetPreview(BoardNodeModel node, BoardDocument document)
        {
            TestExtensionNodeModel extensionNode = node as TestExtensionNodeModel;
            return extensionNode == null ? string.Empty : extensionNode.Health + " | " + extensionNode.Summary;
        }
    }

    internal sealed class TestExtensionInspector : IProjectDesignerInspector
    {
        public string NodeTypeId { get { return "tests.extension.node"; } }
        public int Priority { get { return 100; } }

        public VisualElement BuildInspector(ProjectBoardAsset board, BoardNodeModel node, IBoardCommandDispatcher dispatcher, System.Action repaint)
        {
            return new VisualElement();
        }
    }

    internal sealed class TestExtensionImporter : IProjectDesignerAssetImporter
    {
        public int Priority { get { return 777; } }

        public bool CanImport(Object asset)
        {
            TextAsset textAsset = asset as TextAsset;
            return textAsset != null && textAsset.name.IndexOf("extension-test", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public IEnumerable<BoardNodeModel> Import(Object asset, Vector2 position)
        {
            TextAsset textAsset = asset as TextAsset;
            if (textAsset == null)
            {
                yield break;
            }

            var node = new TestExtensionNodeModel
            {
                Title = textAsset.name,
                Summary = textAsset.text,
                Position = position
            };
            yield return node;
        }
    }

    public sealed class ProjectDesignerPackageTests
    {
        private readonly List<string> _temporaryAssets = new List<string>();

        [SetUp]
        public void SetUp()
        {
            ProjectDesignerRegistry.ResetForTests();
            ProjectDesignerBuiltInRegistration.Register();
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

            Assert.IsTrue(smallTeam.Templates.Count >= 8);
            Assert.IsTrue(smallTeam.Nodes.OfType<TaskNodeModel>().Any());
            Assert.IsTrue(smallTeam.Nodes.OfType<ReferenceNodeModel>().Any());
            Assert.IsTrue(technical.Nodes.OfType<ClassNodeModel>().Any());
            Assert.AreEqual(BoardNodeCategories.TechnicalDesign, technical.ViewState.Category);
        }

        [Test]
        public void BoardPresetFactory_CreatesWorkflowPresetBoardsWithSavedViews()
        {
            BoardDocument pitchVision = BoardPresetFactory.Create(BoardPresetIds.PitchVision, "Pitch");
            BoardDocument milestoneRoadmap = BoardPresetFactory.Create(BoardPresetIds.MilestoneRoadmap, "Roadmap");
            BoardDocument researchReference = BoardPresetFactory.Create(BoardPresetIds.ResearchReference, "Research");
            BoardDocument stakeholderReview = BoardPresetFactory.Create(BoardPresetIds.StakeholderReview, "Review");

            Assert.IsTrue(pitchVision.Nodes.OfType<ProjectBriefNodeModel>().Any());
            Assert.IsTrue(pitchVision.Nodes.OfType<TaskNodeModel>().Any());
            Assert.IsTrue(pitchVision.Nodes.OfType<MilestoneNodeModel>().Any());
            Assert.IsTrue(pitchVision.SavedFilters.Any(filter => filter.Name == "Audience"));

            Assert.IsTrue(milestoneRoadmap.Nodes.OfType<MilestoneNodeModel>().Count() >= 3);
            Assert.IsTrue(milestoneRoadmap.Edges.Count > 0);
            Assert.IsTrue(milestoneRoadmap.SavedFilters.Any(filter => filter.Name == "Roadmap"));

            Assert.IsTrue(researchReference.Nodes.OfType<ReferenceNodeModel>().Count() >= 3);
            Assert.IsTrue(researchReference.SavedFilters.Any(filter => filter.Name == "Research Questions"));

            Assert.IsTrue(stakeholderReview.Nodes.OfType<NoteNodeModel>().Any());
            Assert.IsTrue(stakeholderReview.SavedFilters.Any(filter => filter.Name == "Review Prep"));
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
            MonoScript scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>("Packages/com.birchgames.projectdesigner/Runtime/TaskNodeModel.cs");

            IProjectDesignerAssetImporter textImporter = ProjectDesignerRegistry.GetAssetImporters().First(importer => importer.CanImport(textAsset));
            IProjectDesignerAssetImporter scriptImporter = ProjectDesignerRegistry.GetAssetImporters().First(importer => importer.CanImport(scriptAsset));

            BoardNodeModel textNode = textImporter.Import(textAsset, Vector2.zero).First();
            BoardNodeModel classNode = scriptImporter.Import(scriptAsset, Vector2.zero).First();

            Assert.IsInstanceOf<ReferenceNodeModel>(textNode);
            Assert.IsInstanceOf<ClassNodeModel>(classNode);
        }

        [Test]
        public void Registry_AllowsCustomExtensionNodeInspectorAndImporter()
        {
            ProjectDesignerRegistry.RegisterNodeDefinition(new TestExtensionNodeDefinition());
            ProjectDesignerRegistry.RegisterInspector(new TestExtensionInspector());
            ProjectDesignerRegistry.RegisterAssetImporter(new TestExtensionImporter());

            IProjectDesignerNodeDefinition definition = ProjectDesignerRegistry.GetNodeDefinition("tests.extension.node");
            IProjectDesignerInspector inspector = ProjectDesignerRegistry.GetInspector("tests.extension.node");
            bool hasImporter = ProjectDesignerRegistry.GetAssetImporters().Any(importer => importer.GetType() == typeof(TestExtensionImporter));

            Assert.IsNotNull(definition);
            Assert.IsNotNull(inspector);
            Assert.IsTrue(hasImporter);
        }

        [Test]
        public void LinkUtility_DisambiguatesDuplicateCardTitlesByNodeId()
        {
            BoardDocument document = BoardPresetFactory.CreateEmpty("Links");
            var selected = new TaskNodeModel { Title = "Shared Task" };
            var duplicateA = new TaskNodeModel { Title = "Shared Task" };
            var duplicateB = new TaskNodeModel { Title = "Shared Task" };

            document.AddNode(selected);
            document.AddNode(duplicateA);
            document.AddNode(duplicateB);

            List<ProjectDesignerLinkOption> options = ProjectDesignerLinkUtility.GetLinkOptions(document, selected)
                .Where(option => option.Definition.TypeId == BoardEdgeTypeIds.Dependency && option.SelectedNodeIsSource)
                .ToList();

            Assert.AreEqual(2, options.Count);
            Assert.AreNotEqual(options[0].OtherNode.Id, options[1].OtherNode.Id);
            Assert.AreNotEqual(options[0].DisplayLabel, options[1].DisplayLabel);
            StringAssert.Contains("#1", options[0].DisplayLabel + options[1].DisplayLabel);
            StringAssert.Contains("#2", options[0].DisplayLabel + options[1].DisplayLabel);
        }

        [Test]
        public void SpawnUtility_StaggersRepeatedManualCreatePositions()
        {
            Vector2 center = new Vector2(640f, 360f);
            Vector2 first = ProjectDesignerSpawnUtility.GetCreatePosition(center, null, 0);
            Vector2 second = ProjectDesignerSpawnUtility.GetCreatePosition(center, null, 1);
            Vector2 third = ProjectDesignerSpawnUtility.GetCreatePosition(center, null, 2);

            Assert.AreNotEqual(first, second);
            Assert.AreNotEqual(second, third);

            var selected = new TaskNodeModel { Position = new Vector2(120f, 220f) };
            Vector2 fromSelection = ProjectDesignerSpawnUtility.GetCreatePosition(center, selected, 99);
            Assert.AreEqual(new Vector2(166f, 258f), fromSelection);
        }

        [Test]
        public void BoardCatalog_FindsAllProjectBoardAssets()
        {
            const string rootFolder = "Assets/__ProjectDesignerTemp";
            if (!AssetDatabase.IsValidFolder(rootFolder))
            {
                AssetDatabase.CreateFolder("Assets", "__ProjectDesignerTemp");
            }

            string firstPath = AssetDatabase.GenerateUniqueAssetPath(rootFolder + "/Catalog Board A.asset");
            string secondPath = AssetDatabase.GenerateUniqueAssetPath(rootFolder + "/Catalog Board B.asset");

            ProjectBoardAsset firstBoard = ScriptableObject.CreateInstance<ProjectBoardAsset>();
            firstBoard.ResetDocument(BoardPresetFactory.CreateEmpty("Catalog Board A", true));
            AssetDatabase.CreateAsset(firstBoard, firstPath);
            _temporaryAssets.Add(firstPath);

            ProjectBoardAsset secondBoard = ScriptableObject.CreateInstance<ProjectBoardAsset>();
            secondBoard.ResetDocument(BoardPresetFactory.Create(BoardPresetIds.PitchVision, "Catalog Board B"));
            AssetDatabase.CreateAsset(secondBoard, secondPath);
            _temporaryAssets.Add(secondPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            IReadOnlyList<ProjectBoardAsset> boards = ProjectDesignerBoardCatalog.GetBoards();
            List<string> boardNames = boards.Select(board => board.Document.BoardName).ToList();

            CollectionAssert.Contains(boardNames, "Catalog Board A");
            CollectionAssert.Contains(boardNames, "Catalog Board B");
        }

        [Test]
        public void Package_DeclaresStatusReportSampleAndDemoBoard()
        {
            string packageJsonPath = "Packages/com.birchgames.projectdesigner/package.json";
            string sampleCodePath = "Packages/com.birchgames.projectdesigner/Samples~/StatusReportExtension/StatusReportExtension.cs";
            string sampleReadmePath = "Packages/com.birchgames.projectdesigner/Samples~/StatusReportExtension/README.md";
            string sampleBoardPath = "Packages/com.birchgames.projectdesigner/Samples~/StatusReportExtension/Status Report Demo Board.asset";

            Assert.IsTrue(File.Exists(packageJsonPath));
            Assert.IsTrue(File.Exists(sampleCodePath));
            Assert.IsTrue(File.Exists(sampleReadmePath));
            Assert.IsTrue(File.Exists(sampleBoardPath));

            string packageJson = File.ReadAllText(packageJsonPath);
            string sampleCode = File.ReadAllText(sampleCodePath);
            string sampleReadme = File.ReadAllText(sampleReadmePath);
            string sampleBoard = File.ReadAllText(sampleBoardPath);

            StringAssert.Contains("Status Report Extension", packageJson);
            StringAssert.Contains("StatusReportNodeModel", sampleCode);
            StringAssert.Contains("StatusReportExtensionRegistration", sampleCode);
            StringAssert.Contains("demo board", sampleReadme.ToLowerInvariant());
            StringAssert.Contains("StatusReportNodeModel", sampleBoard);
            StringAssert.Contains("Status Report Demo Board", sampleBoard);
        }
    }
}
