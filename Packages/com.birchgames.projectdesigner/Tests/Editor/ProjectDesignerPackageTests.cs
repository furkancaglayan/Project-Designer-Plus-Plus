using System;
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

        public bool CanImport(UnityEngine.Object asset)
        {
            TextAsset textAsset = asset as TextAsset;
            return textAsset != null && textAsset.name.IndexOf("extension-test", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public IEnumerable<BoardNodeModel> Import(UnityEngine.Object asset, Vector2 position)
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
            ProjectDesignerTeamRosterContext.SetProvider(null);
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
            ProjectDesignerTeamRosterContext.SetProvider(null);
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
        public void BoardViewState_SupportsPrimaryAndMultiSelection()
        {
            var viewState = new BoardViewState();
            viewState.SelectSingle("alpha");
            Assert.AreEqual("alpha", viewState.SelectedNodeId);
            CollectionAssert.AreEqual(new[] { "alpha" }, viewState.SelectedNodeIds);

            viewState.ToggleSelection("beta");
            Assert.AreEqual("beta", viewState.SelectedNodeId);
            CollectionAssert.AreEqual(new[] { "alpha", "beta" }, viewState.SelectedNodeIds);

            viewState.Deselect("beta");
            Assert.AreEqual("alpha", viewState.SelectedNodeId);
            CollectionAssert.AreEqual(new[] { "alpha" }, viewState.SelectedNodeIds);

            viewState.ClearSelection();
            Assert.IsEmpty(viewState.SelectedNodeIds);
            Assert.AreEqual(string.Empty, viewState.SelectedNodeId);
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
            BoardDocument redo = BoardPresetFactory.Create(BoardPresetIds.ProjectDesignerRedo, "Redo");
            BoardDocument smallTeam = BoardPresetFactory.Create(BoardPresetIds.SmallTeam, "Team");
            BoardDocument technical = BoardPresetFactory.Create(BoardPresetIds.TechnicalDesign, "Tech");

            Assert.IsTrue(redo.Templates.Count >= 9);
            Assert.IsTrue(redo.Nodes.OfType<ProjectBriefNodeModel>().Any());
            Assert.IsTrue(redo.Nodes.OfType<ClassNodeModel>().Any());
            Assert.IsTrue(redo.SavedFilters.Any(filter => filter.Name == "Launch"));
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
        public void LinkUtility_GroupsMultipleValidLinkTypesPerTarget()
        {
            BoardDocument document = BoardPresetFactory.CreateEmpty("Inline Links");
            var sourceTask = new TaskNodeModel { Title = "Source Task" };
            var targetTask = new TaskNodeModel { Title = "Target Task" };
            var milestone = new MilestoneNodeModel { Title = "Target Milestone" };

            document.AddNode(sourceTask);
            document.AddNode(targetTask);
            document.AddNode(milestone);

            Dictionary<string, List<ProjectDesignerLinkOption>> byTarget = ProjectDesignerLinkUtility.GetLinkOptionsByTarget(document, sourceTask);
            List<ProjectDesignerLinkOption> taskOptions = byTarget[targetTask.Id];
            List<ProjectDesignerLinkOption> milestoneOptions = byTarget[milestone.Id];

            Assert.AreEqual(2, taskOptions.Count);
            CollectionAssert.AreEquivalent(
                new[] { BoardEdgeTypeIds.Dependency, BoardEdgeTypeIds.Reference },
                taskOptions.Select(option => option.Definition.TypeId).ToArray());

            Assert.AreEqual(2, milestoneOptions.Count);
            CollectionAssert.AreEquivalent(
                new[] { BoardEdgeTypeIds.Milestone, BoardEdgeTypeIds.Reference },
                milestoneOptions.Select(option => option.Definition.TypeId).ToArray());
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
        public void DuplicateNodesCommand_RecreatesOnlyInternalEdgesAndSelectsCopies()
        {
            ProjectBoardAsset board = ProjectBoardAsset.CreateTransient(BoardPresetFactory.CreateEmpty("Duplicate"));
            var taskA = new TaskNodeModel { Title = "Task A", Position = new Vector2(100f, 100f) };
            var taskB = new TaskNodeModel { Title = "Task B", Position = new Vector2(320f, 100f) };
            var milestone = new MilestoneNodeModel { Title = "Milestone", Position = new Vector2(560f, 120f) };
            board.Document.AddNode(taskA);
            board.Document.AddNode(taskB);
            board.Document.AddNode(milestone);
            board.Document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Dependency, taskA.Id, taskB.Id));
            board.Document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Milestone, taskB.Id, milestone.Id));

            var command = new DuplicateNodesCommand(board, new[] { taskA.Id, taskB.Id }, new Vector2(48f, 40f), false);
            command.Execute(board);

            Assert.AreEqual(5, board.Document.Nodes.Count);
            Assert.AreEqual(3, board.Document.Edges.Count);
            Assert.AreEqual(2, board.Document.ViewState.SelectedNodeIds.Count);

            List<BoardNodeModel> duplicatedTasks = board.Document.ViewState.SelectedNodeIds
                .Select(board.Document.GetNode)
                .Where(node => node != null)
                .ToList();
            Assert.AreEqual(2, duplicatedTasks.Count);
            Assert.IsTrue(duplicatedTasks.All(node => node.Position.x >= 148f));

            List<BoardEdgeModel> internalEdges = board.Document.Edges
                .Where(edge => duplicatedTasks.Any(node => node.Id == edge.SourceNodeId) && duplicatedTasks.Any(node => node.Id == edge.TargetNodeId))
                .ToList();
            Assert.AreEqual(1, internalEdges.Count);
            Assert.AreEqual(BoardEdgeTypeIds.Dependency, internalEdges[0].TypeId);

            Assert.IsFalse(board.Document.Edges.Any(edge => duplicatedTasks.Any(node => node.Id == edge.SourceNodeId) && edge.TargetNodeId == milestone.Id));
        }

        [Test]
        public void ArrangeNodesCommand_AlignsAndDistributesSelection()
        {
            ProjectBoardAsset board = ProjectBoardAsset.CreateTransient(BoardPresetFactory.CreateEmpty("Arrange"));
            var first = new TaskNodeModel { Title = "First", Position = new Vector2(100f, 150f) };
            var second = new TaskNodeModel { Title = "Second", Position = new Vector2(300f, 220f) };
            var third = new TaskNodeModel { Title = "Third", Position = new Vector2(560f, 300f) };
            board.Document.AddNode(first);
            board.Document.AddNode(second);
            board.Document.AddNode(third);

            new ArrangeNodesCommand(board, new[] { first.Id, second.Id, third.Id }, BoardArrangeMode.AlignTop).Execute(board);
            Assert.AreEqual(150f, board.Document.GetNode(first.Id).Position.y);
            Assert.AreEqual(150f, board.Document.GetNode(second.Id).Position.y);
            Assert.AreEqual(150f, board.Document.GetNode(third.Id).Position.y);

            new ArrangeNodesCommand(board, new[] { first.Id, second.Id, third.Id }, BoardArrangeMode.DistributeHorizontal).Execute(board);
            float firstX = board.Document.GetNode(first.Id).Position.x;
            float secondX = board.Document.GetNode(second.Id).Position.x;
            float thirdX = board.Document.GetNode(third.Id).Position.x;
            Assert.Less(firstX, secondX);
            Assert.Less(secondX, thirdX);
        }

        [Test]
        public void ArrangeNodesCommand_AutoLayoutLeftToRightRespectsPlannerEdgeSemantics()
        {
            ProjectBoardAsset board = ProjectBoardAsset.CreateTransient(BoardPresetFactory.CreateEmpty("Auto Layout"));
            board.Document.ViewState.SnapToGrid = true;

            var blocker = new TaskNodeModel { Title = "Blocker", Position = new Vector2(760f, 460f) };
            var dependent = new TaskNodeModel { Title = "Dependent", Position = new Vector2(180f, 160f) };
            var milestone = new MilestoneNodeModel { Title = "Milestone", Position = new Vector2(360f, 720f) };
            var reference = new ReferenceNodeModel { Title = "Reference", Position = new Vector2(980f, 120f) };

            board.Document.AddNode(blocker);
            board.Document.AddNode(dependent);
            board.Document.AddNode(milestone);
            board.Document.AddNode(reference);

            board.Document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Dependency, dependent.Id, blocker.Id));
            board.Document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Milestone, dependent.Id, milestone.Id));
            board.Document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Reference, reference.Id, blocker.Id));

            new ArrangeNodesCommand(board, new[] { blocker.Id, dependent.Id, milestone.Id, reference.Id }, BoardArrangeMode.AutoLayoutLeftToRight).Execute(board);

            Vector2 blockerPosition = board.Document.GetNode(blocker.Id).Position;
            Vector2 dependentPosition = board.Document.GetNode(dependent.Id).Position;
            Vector2 milestonePosition = board.Document.GetNode(milestone.Id).Position;
            Vector2 referencePosition = board.Document.GetNode(reference.Id).Position;

            Assert.Less(referencePosition.x, blockerPosition.x);
            Assert.Less(blockerPosition.x, dependentPosition.x);
            Assert.Less(dependentPosition.x, milestonePosition.x);
            Assert.Less(Mathf.Abs(Mathf.Repeat(blockerPosition.x, ProjectDesignerProductInfo.GridSize)), 0.001f);
            Assert.Less(Mathf.Abs(Mathf.Repeat(dependentPosition.x, ProjectDesignerProductInfo.GridSize)), 0.001f);
            Assert.Less(Mathf.Abs(Mathf.Repeat(milestonePosition.x, ProjectDesignerProductInfo.GridSize)), 0.001f);
        }

        [Test]
        public void MoveAndArrangeCommands_RespectSnapToGrid()
        {
            ProjectBoardAsset board = ProjectBoardAsset.CreateTransient(BoardPresetFactory.CreateEmpty("Snap"));
            board.Document.ViewState.SnapToGrid = true;

            var first = new TaskNodeModel { Title = "First", Position = new Vector2(101f, 117f) };
            var second = new TaskNodeModel { Title = "Second", Position = new Vector2(297f, 139f) };
            board.Document.AddNode(first);
            board.Document.AddNode(second);

            new DuplicateNodesCommand(board, new[] { first.Id }, new Vector2(11f, 19f), true).Execute(board);
            BoardNodeModel duplicate = board.Document.Nodes.Last();
            Assert.AreEqual(BoardLayoutUtility.SnapPosition(new Vector2(112f, 136f)), duplicate.Position);

            new ArrangeNodesCommand(board, new[] { first.Id, second.Id }, BoardArrangeMode.AlignLeft).Execute(board);
            Assert.AreEqual(96f, board.Document.GetNode(first.Id).Position.x);
            Assert.AreEqual(96f, board.Document.GetNode(second.Id).Position.x);
        }

        [Test]
        public void BoardInsights_AssigneeSummariesAggregateOpenWorkload()
        {
            BoardDocument document = BoardPresetFactory.CreateEmpty("Workload");
            DateTime referenceDate = new DateTime(2026, 4, 28);

            ProjectDesignerTeamRosterAsset roster = CreateRoster(
                new ProjectDesignerTeamMemberData { Id = "aylin", DisplayName = "Aylin", Role = "Producer", AccentColor = "#55AAFF" },
                new ProjectDesignerTeamMemberData { Id = "mert", DisplayName = "Mert", Role = "Engineer", AccentColor = "#66CC88" });

            var first = new TaskNodeModel { Title = "First", AssigneeId = "aylin", EstimatePoints = 5, DueDateIso = "2026-04-27" };
            var second = new TaskNodeModel { Title = "Second", AssigneeId = "aylin", EstimatePoints = 8, Status = TaskNodeStatus.Blocked };
            var third = new TaskNodeModel { Title = "Third", AssigneeId = "mert", EstimatePoints = 2, Status = TaskNodeStatus.Done };
            document.AddNode(first);
            document.AddNode(second);
            document.AddNode(third);

            IReadOnlyList<BoardAssigneeSummary> summaries = BoardInsights.GetAssigneeSummaries(document, referenceDate, roster);
            BoardAssigneeSummary aylin = summaries.First(summary => summary.AssigneeId == "aylin");

            Assert.AreEqual(2, aylin.OpenTaskCount);
            Assert.AreEqual(13, aylin.TotalEstimatePoints);
            Assert.AreEqual(1, aylin.BlockedTaskCount);
            Assert.AreEqual(1, aylin.OverdueTaskCount);
            Assert.IsTrue(aylin.HasOverload);
            Assert.AreEqual("Aylin", aylin.DisplayName);
            Assert.AreEqual("#55AAFF", aylin.AccentColor);
        }

        [Test]
        public void BoardInsights_MilestoneHealthTracksRiskAndCompletion()
        {
            BoardDocument document = BoardPresetFactory.CreateEmpty("Milestone Health");
            DateTime referenceDate = new DateTime(2026, 4, 28);

            var milestone = new MilestoneNodeModel { Title = "Vertical Slice", TargetDateIso = "2026-05-01" };
            var taskA = new TaskNodeModel { Title = "Gameplay loop", Status = TaskNodeStatus.InProgress };
            var taskB = new TaskNodeModel { Title = "Art pass", Status = TaskNodeStatus.Blocked };
            document.AddNode(milestone);
            document.AddNode(taskA);
            document.AddNode(taskB);
            document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Milestone, taskA.Id, milestone.Id));
            document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Milestone, taskB.Id, milestone.Id));

            BoardMilestoneHealthReport report = BoardInsights.GetMilestoneHealth(document, milestone, referenceDate);
            Assert.AreEqual(BoardMilestoneHealthState.AtRisk, report.State);
            Assert.AreEqual(2, report.LinkedTaskCount);
            Assert.AreEqual(1, report.BlockedTaskCount);

            taskA.Status = TaskNodeStatus.Done;
            taskB.Status = TaskNodeStatus.Done;
            report = BoardInsights.GetMilestoneHealth(document, milestone, referenceDate);
            Assert.AreEqual(BoardMilestoneHealthState.Complete, report.State);
            Assert.AreEqual(1f, report.Completion);
        }

        [Test]
        public void BoardInsights_DependencyHelpersIdentifyWaitingAndBlockingTasks()
        {
            BoardDocument document = BoardPresetFactory.CreateEmpty("Dependencies");
            var dependent = new TaskNodeModel { Title = "Dependent", Status = TaskNodeStatus.InProgress };
            var blocker = new TaskNodeModel { Title = "Blocker", Status = TaskNodeStatus.InProgress };
            var freeTask = new TaskNodeModel { Title = "Free", Status = TaskNodeStatus.InProgress };
            document.AddNode(dependent);
            document.AddNode(blocker);
            document.AddNode(freeTask);
            document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Dependency, dependent.Id, blocker.Id));

            Assert.IsTrue(BoardInsights.HasUnresolvedDependencies(document, dependent));
            Assert.IsTrue(BoardInsights.IsTaskBlocked(document, dependent));
            Assert.AreEqual(1, BoardInsights.CountUnresolvedDependencyLinks(document));
            CollectionAssert.AreEquivalent(new[] { dependent.Id }, BoardInsights.GetBlockedTasks(document).Select(task => task.Id).ToArray());
            CollectionAssert.AreEquivalent(new[] { blocker.Id }, BoardInsights.GetTasksBlockingOthers(document).Select(task => task.Id).ToArray());
            Assert.IsFalse(BoardInsights.HasUnresolvedDependencies(document, freeTask));
        }

        [Test]
        public void BoardInsights_QuickFiltersAndVisibleNodesRespectPlannerSignals()
        {
            BoardDocument document = BoardPresetFactory.CreateEmpty("Quick Filters");
            string overdueDate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
            string soonDate = DateTime.Today.AddDays(3).ToString("yyyy-MM-dd");

            var overdueTask = new TaskNodeModel { Title = "Overdue", AssigneeId = "aylin", DueDateIso = overdueDate };
            var soonTask = new TaskNodeModel { Title = "Soon", AssigneeId = "aylin", DueDateIso = soonDate };
            var unassignedTask = new TaskNodeModel { Title = "Unassigned" };
            var riskNote = new NoteNodeModel { Title = "Risk Note" };
            riskNote.SetTagsFromCsv("risk, review");

            document.AddNode(overdueTask);
            document.AddNode(soonTask);
            document.AddNode(unassignedTask);
            document.AddNode(riskNote);

            document.ViewState.QuickFilterId = BoardQuickFilterIds.Overdue;
            CollectionAssert.AreEquivalent(new[] { overdueTask.Id }, BoardInsights.GetVisibleNodes(document).Select(node => node.Id).ToArray());

            document.ViewState.QuickFilterId = BoardQuickFilterIds.ForAssigneeId("aylin");
            CollectionAssert.AreEquivalent(new[] { overdueTask.Id, soonTask.Id }, BoardInsights.GetVisibleNodes(document).Select(node => node.Id).ToArray());

            document.ViewState.QuickFilterId = BoardQuickFilterIds.AtRisk;
            CollectionAssert.AreEquivalent(new[] { riskNote.Id }, BoardInsights.GetVisibleNodes(document).Select(node => node.Id).ToArray());
        }

        [Test]
        public void CardPresentation_TaskSignalsStayFocusedOnStatusAssigneeAndUrgency()
        {
            BoardDocument document = BoardPresetFactory.CreateEmpty("Card Signals");
            ProjectDesignerTeamRosterAsset roster = CreateRoster(
                new ProjectDesignerTeamMemberData { Id = "producer", DisplayName = "Producer", Role = "Production", AccentColor = "#55AAFF" });

            var task = new TaskNodeModel
            {
                Title = "Audience",
                Status = TaskNodeStatus.InProgress,
                Priority = TaskNodePriority.Critical,
                AssigneeId = "producer",
                DueDateIso = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd")
            };
            var blocker = new TaskNodeModel
            {
                Title = "Blocker",
                Status = TaskNodeStatus.InProgress
            };

            document.AddNode(task);
            document.AddNode(blocker);
            document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Dependency, task.Id, blocker.Id));

            IReadOnlyList<ProjectDesignerCardSignal> signals = ProjectDesignerCardPresentation.GetTaskSignals(task, document, roster);

            CollectionAssert.AreEqual(
                new[] { "In Progress", "Producer", "Blocked" },
                signals.Select(signal => signal.Text).ToArray());
        }

        [Test]
        public void CardPresentation_TagSummaryCapsVisibleTagsAndShowsOverflow()
        {
            var task = new TaskNodeModel();
            task.SetTagsFromCsv("slice, audience, pitch, audience");

            ProjectDesignerCardTagSummary summary = ProjectDesignerCardPresentation.GetTagSummary(task);

            CollectionAssert.AreEqual(new[] { "slice", "audience" }, summary.VisibleTags.ToArray());
            Assert.AreEqual(1, summary.HiddenCount);
            Assert.AreEqual("+1", ProjectDesignerCardPresentation.FormatTagOverflowLabel(summary.HiddenCount));
        }

        [Test]
        public void TaskDefinition_PreviewUsesDescriptionBeforePlannerMetadata()
        {
            var definition = new TaskNodeDefinition();
            var task = new TaskNodeModel
            {
                Title = "Audience",
                Description = "Clarify who the vertical slice is meant to impress.",
                AssigneeId = "producer",
                Status = TaskNodeStatus.InProgress,
                Priority = TaskNodePriority.High
            };

            string preview = definition.GetPreview(task, BoardPresetFactory.CreateEmpty("Preview"));

            Assert.AreEqual("Clarify who the vertical slice is meant to impress.", preview);
        }

        [Test]
        public void ReferenceDefinition_PreviewUsesFriendlySourceText()
        {
            var definition = new ReferenceNodeDefinition();
            var reference = new ReferenceNodeModel
            {
                AssetPath = "Assets/References/Mood Board.png"
            };

            string preview = definition.GetPreview(reference, BoardPresetFactory.CreateEmpty("Reference Preview"));

            Assert.AreEqual("Unity asset: Mood Board.png", preview);
        }

        [Test]
        public void TeamRosterResolver_ResolvesExistingAssignmentsByIdOrDisplayName()
        {
            ProjectDesignerTeamRosterAsset roster = CreateRoster(
                new ProjectDesignerTeamMemberData { Id = "producer", DisplayName = "Producer", Role = "Production" },
                new ProjectDesignerTeamMemberData { Id = "design-lead", DisplayName = "Design Lead", Role = "Design" });

            ProjectDesignerTeamMemberData byId = ProjectDesignerTeamRosterResolver.ResolveMember(roster, "producer");
            ProjectDesignerTeamMemberData byDisplayName = ProjectDesignerTeamRosterResolver.ResolveMember(roster, "Design Lead");

            Assert.IsNotNull(byId);
            Assert.AreEqual("Producer", byId.DisplayName);
            Assert.IsNotNull(byDisplayName);
            Assert.AreEqual("design-lead", byDisplayName.Id);
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
        public void BoardCatalog_SearchAndSortRespectsQueryAndFinderMode()
        {
            var entries = new List<ProjectDesignerBoardCatalogEntry>
            {
                new ProjectDesignerBoardCatalogEntry
                {
                    BoardName = "Alpha Board",
                    Summary = "Pitch planning board",
                    AssetPath = "Assets/Boards/Alpha.asset",
                    TeamSnapshot = "Producer, Designer",
                    NodeCount = 4,
                    EdgeCount = 2,
                    InProgressCount = 1,
                    IsPinned = false,
                    RecentIndex = 2
                },
                new ProjectDesignerBoardCatalogEntry
                {
                    BoardName = "Bravo Board",
                    Summary = "Roadmap and slice planning",
                    AssetPath = "Assets/Boards/Bravo.asset",
                    TeamSnapshot = "Engineer, Artist",
                    NodeCount = 12,
                    EdgeCount = 6,
                    InProgressCount = 5,
                    IsPinned = true,
                    RecentIndex = 5
                },
                new ProjectDesignerBoardCatalogEntry
                {
                    BoardName = "Charlie Board",
                    Summary = "Research and references",
                    AssetPath = "Assets/Boards/Charlie.asset",
                    TeamSnapshot = "Research Lead",
                    NodeCount = 8,
                    EdgeCount = 1,
                    InProgressCount = 0,
                    IsPinned = false,
                    RecentIndex = 0
                }
            };

            IReadOnlyList<ProjectDesignerBoardCatalogEntry> searchResults = ProjectDesignerBoardCatalog.SearchAndSort(entries, "research", ProjectDesignerBoardFinderSortMode.BoardName);
            Assert.AreEqual(1, searchResults.Count);
            Assert.AreEqual("Charlie Board", searchResults[0].BoardName);

            IReadOnlyList<ProjectDesignerBoardCatalogEntry> recentResults = ProjectDesignerBoardCatalog.SearchAndSort(entries, string.Empty, ProjectDesignerBoardFinderSortMode.RecentlyOpened);
            Assert.AreEqual("Bravo Board", recentResults[0].BoardName);
            Assert.AreEqual("Charlie Board", recentResults[1].BoardName);

            IReadOnlyList<ProjectDesignerBoardCatalogEntry> activityResults = ProjectDesignerBoardCatalog.SearchAndSort(entries, string.Empty, ProjectDesignerBoardFinderSortMode.MostActiveWork);
            Assert.AreEqual("Bravo Board", activityResults[0].BoardName);
            Assert.AreEqual("Alpha Board", activityResults[1].BoardName);
        }

        [Test]
        public void WorkspaceHelpers_TruncateShellLabelsForStableChrome()
        {
            string title = ProjectDesignerWorkspaceView.BuildToolbarTitle("Project Designer+ Redo Demo Board With A Very Long Name");
            string trimmed = ProjectDesignerWorkspaceView.TruncateShellLabel("This label should definitely be shortened", 18);

            Assert.LessOrEqual(title.Length, ProjectDesignerWorkspaceView.ToolbarTitleMaxLength);
            Assert.IsTrue(title.EndsWith("…"));
            Assert.AreEqual("This label should…", trimmed);
        }

        [Test]
        public void WorkspaceHelpers_SavedViewHelpersExposeClearActiveState()
        {
            var activeFilter = new BoardSavedFilter("Planning", "slice", BoardNodeCategories.Planning, string.Empty, true);
            var inactiveFilter = new BoardSavedFilter("References", string.Empty, BoardNodeCategories.Reference, string.Empty, true);

            Assert.IsTrue(ProjectDesignerWorkspaceView.IsSavedViewActive(activeFilter.Id, activeFilter));
            Assert.IsFalse(ProjectDesignerWorkspaceView.IsSavedViewActive(activeFilter.Id, inactiveFilter));
            Assert.AreEqual("Active  Planning", ProjectDesignerWorkspaceView.BuildSavedViewButtonText(activeFilter.Name, true));
            Assert.AreEqual("References", ProjectDesignerWorkspaceView.BuildSavedViewButtonText(inactiveFilter.Name, false));
        }

        [Test]
        public void WorkspaceHelpers_UseStableCollapsedShellSummaries()
        {
            var selectedNodes = new List<BoardNodeModel>
            {
                new TaskNodeModel { Title = "A very long task title that should not leak into a collapsed header" }
            };

            Assert.AreEqual(string.Empty, ProjectDesignerWorkspaceView.BuildInspectorSummaryLabel(false, selectedNodes));
            Assert.AreEqual(string.Empty, ProjectDesignerWorkspaceView.BuildLibrarySummaryLabel(false));

            string expandedSummary = ProjectDesignerWorkspaceView.BuildInspectorSummaryLabel(true, selectedNodes);
            Assert.IsTrue(expandedSummary.EndsWith("…"));
            Assert.LessOrEqual(expandedSummary.Length, ProjectDesignerWorkspaceView.InspectorSummaryMaxLength);
            Assert.AreEqual("Add cards and views", ProjectDesignerWorkspaceView.BuildLibrarySummaryLabel(true));
        }

        [Test]
        public void Package_DeclaresSamplesAndDemoBoards()
        {
            string packageJsonPath = "Packages/com.birchgames.projectdesigner/package.json";
            string showcaseReadmePath = "Packages/com.birchgames.projectdesigner/Samples~/ShowcaseBoards/README.md";
            string showcaseBoardPath = "Packages/com.birchgames.projectdesigner/Samples~/ShowcaseBoards/Project Designer+ Redo Demo Board.asset";
            string sampleCodePath = "Packages/com.birchgames.projectdesigner/Samples~/StatusReportExtension/StatusReportExtension.cs";
            string sampleReadmePath = "Packages/com.birchgames.projectdesigner/Samples~/StatusReportExtension/README.md";
            string sampleBoardPath = "Packages/com.birchgames.projectdesigner/Samples~/StatusReportExtension/Status Report Demo Board.asset";

            Assert.IsTrue(File.Exists(packageJsonPath));
            Assert.IsTrue(File.Exists(showcaseReadmePath));
            Assert.IsTrue(File.Exists(showcaseBoardPath));
            Assert.IsTrue(File.Exists(sampleCodePath));
            Assert.IsTrue(File.Exists(sampleReadmePath));
            Assert.IsTrue(File.Exists(sampleBoardPath));

            string packageJson = File.ReadAllText(packageJsonPath);
            string showcaseReadme = File.ReadAllText(showcaseReadmePath);
            string showcaseBoard = File.ReadAllText(showcaseBoardPath);
            string sampleCode = File.ReadAllText(sampleCodePath);
            string sampleReadme = File.ReadAllText(sampleReadmePath);
            string sampleBoard = File.ReadAllText(sampleBoardPath);

            StringAssert.Contains("Showcase Boards", packageJson);
            StringAssert.Contains("Project Designer+ Redo Demo Board", showcaseReadme);
            StringAssert.Contains("ProjectDesignerWorkspaceView", showcaseBoard);
            StringAssert.Contains("Project Designer+ Redo Demo Board", showcaseBoard);
            StringAssert.Contains("Status Report Extension", packageJson);
            StringAssert.Contains("StatusReportNodeModel", sampleCode);
            StringAssert.Contains("StatusReportExtensionRegistration", sampleCode);
            StringAssert.Contains("demo board", sampleReadme.ToLowerInvariant());
            StringAssert.Contains("StatusReportNodeModel", sampleBoard);
            StringAssert.Contains("Status Report Demo Board", sampleBoard);
        }

        private static ProjectDesignerTeamRosterAsset CreateRoster(params ProjectDesignerTeamMemberData[] members)
        {
            ProjectDesignerTeamRosterAsset roster = ScriptableObject.CreateInstance<ProjectDesignerTeamRosterAsset>();
            foreach (ProjectDesignerTeamMemberData member in members)
            {
                if (member != null)
                {
                    roster.Members.Add(member);
                }
            }

            roster.EnsureDefaults();
            return roster;
        }
    }
}
