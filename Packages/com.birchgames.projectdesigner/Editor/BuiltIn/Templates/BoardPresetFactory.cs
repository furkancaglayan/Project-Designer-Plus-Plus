using System.Collections.Generic;
using ProjectDesigner.V2.Data;
using UnityEngine;

namespace ProjectDesigner.V2.BuiltIn
{
    public static class BoardPresetFactory
    {
        public static BoardDocument Create(string presetId, string boardName = null)
        {
            switch (presetId)
            {
                case BoardPresetIds.SoloIndie:
                    return CreateSoloIndie(boardName);
                case BoardPresetIds.SmallTeam:
                    return CreateSmallTeam(boardName);
                case BoardPresetIds.TechnicalDesign:
                    return CreateTechnicalDesign(boardName);
                default:
                    return CreateEmpty(boardName);
            }
        }

        public static BoardDocument CreateEmpty(string boardName = null)
        {
            var document = new BoardDocument(boardName ?? "Project Board");
            document.Summary = "Plan pre-production, references, and technical direction in one Unity workspace.";
            document.TeamMembers.Clear();
            document.TeamMembers.AddRange(new[] { "Producer", "Designer", "Programmer", "Artist" });
            SetTemplates(document);
            document.SavedFilters.Clear();
            document.SavedFilters.Add(new BoardSavedFilter("Planning", string.Empty, BoardNodeCategories.Planning, string.Empty, true));
            document.SavedFilters.Add(new BoardSavedFilter("References", string.Empty, BoardNodeCategories.Reference, string.Empty, true));
            document.SavedFilters.Add(new BoardSavedFilter("Technical", string.Empty, BoardNodeCategories.TechnicalDesign, string.Empty, true));
            return document;
        }

        public static BoardDocument CreateSoloIndie(string boardName = null)
        {
            BoardDocument document = CreateEmpty(boardName ?? "Solo Indie Board");
            document.Summary = "A focused solo-dev pre-production board for vision, slice planning, and reference gathering.";
            document.TeamMembers.Clear();
            document.TeamMembers.Add("Founder");

            var milestone = new MilestoneNodeModel
            {
                Title = "Vertical Slice Ready",
                Summary = "Core loop validated, target mood established, and first playable flow designed.",
                Position = new Vector2(740f, 120f),
                TargetDateIso = "2026-06-01"
            };

            var taskOne = new TaskNodeModel
            {
                Title = "Define fantasy and loop",
                Description = "Document the player fantasy, verbs, and the one-sentence pitch.",
                Position = new Vector2(120f, 120f),
                Assignee = "Founder",
                EstimatePoints = 2
            };
            taskOne.SetTagsFromCsv("vision, pitch");

            var taskTwo = new TaskNodeModel
            {
                Title = "Greybox first slice",
                Description = "Prototype the first five minutes and validate pacing.",
                Position = new Vector2(120f, 380f),
                Assignee = "Founder",
                EstimatePoints = 5,
                Status = TaskNodeStatus.InProgress
            };
            taskTwo.SetTagsFromCsv("prototype, gameplay");

            var note = new NoteNodeModel
            {
                Title = "Mood Notes",
                Body = "Compact spaces, readable silhouettes, tactile interactions, and warm industrial color accents.",
                Position = new Vector2(430f, 400f)
            };
            note.SetTagsFromCsv("art, direction");

            document.AddNode(taskOne);
            document.AddNode(taskTwo);
            document.AddNode(milestone);
            document.AddNode(note);
            document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Dependency, taskTwo.Id, taskOne.Id));
            document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Milestone, taskOne.Id, milestone.Id));
            document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Milestone, taskTwo.Id, milestone.Id));
            document.ViewState.SelectedNodeId = taskOne.Id;
            return document;
        }

        public static BoardDocument CreateSmallTeam(string boardName = null)
        {
            BoardDocument document = CreateEmpty(boardName ?? "Small Team Board");
            document.Summary = "A collaborative board for feature planning, milestone tracking, and shared references.";
            document.TeamMembers.Clear();
            document.TeamMembers.AddRange(new[] { "Design", "Engineering", "Art", "Production" });

            var milestone = new MilestoneNodeModel
            {
                Title = "Pitch Deck Lock",
                Summary = "Shared player fantasy, top-level roadmap, and polished slice references for stakeholder review.",
                Position = new Vector2(760f, 120f)
            };

            var taskOne = new TaskNodeModel
            {
                Title = "Feature pillars",
                Description = "Agree on three product pillars and map each to a player promise.",
                Position = new Vector2(120f, 120f),
                Assignee = "Design",
                EstimatePoints = 3
            };
            taskOne.SetTagsFromCsv("design, pillars");

            var taskTwo = new TaskNodeModel
            {
                Title = "Reference board",
                Description = "Collect visual, UI, and camera references that support the pitch direction.",
                Position = new Vector2(120f, 360f),
                Assignee = "Art",
                EstimatePoints = 3
            };
            taskTwo.SetTagsFromCsv("art, reference");

            var reference = new ReferenceNodeModel
            {
                Title = "Competitive inspiration",
                Summary = "Tactical readability, compact mission structure, and layered goals.",
                Position = new Vector2(460f, 340f)
            };
            reference.SetTagsFromCsv("benchmark, pitch");

            document.AddNode(taskOne);
            document.AddNode(taskTwo);
            document.AddNode(milestone);
            document.AddNode(reference);
            document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Milestone, taskOne.Id, milestone.Id));
            document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Milestone, taskTwo.Id, milestone.Id));
            document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Reference, reference.Id, taskTwo.Id));
            document.ViewState.SelectedNodeId = taskTwo.Id;
            return document;
        }

        public static BoardDocument CreateTechnicalDesign(string boardName = null)
        {
            BoardDocument document = CreateEmpty(boardName ?? "Technical Design Board");
            document.Summary = "A technical design board for class structure, ownership, and implementation notes.";
            document.ViewState.Category = BoardNodeCategories.TechnicalDesign;

            var classOne = new ClassNodeModel
            {
                Title = "BoardBootstrapper",
                NamespaceName = "Game.Tools",
                Summary = "Loads data and coordinates the editor-facing services.",
                Position = new Vector2(140f, 140f)
            };
            classOne.Fields.Add(new BoardClassMemberData("_document : BoardDocument", "private"));
            classOne.Methods.Add(new BoardClassMemberData("Load()", "public"));

            var classTwo = new ClassNodeModel
            {
                Title = "TaskPlannerService",
                NamespaceName = "Game.Planning",
                Summary = "Maps backlog data into milestone and slice views.",
                Position = new Vector2(560f, 180f)
            };
            classTwo.Fields.Add(new BoardClassMemberData("_repository : TaskRepository", "private"));
            classTwo.Methods.Add(new BoardClassMemberData("BuildBoard()", "public"));

            var note = new NoteNodeModel
            {
                Title = "Implementation Notes",
                Body = "Keep technical nodes secondary in the product UI, but first-class in the extension model.",
                Position = new Vector2(420f, 460f)
            };

            document.AddNode(classOne);
            document.AddNode(classTwo);
            document.AddNode(note);
            document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.TechnicalRelation, classTwo.Id, classOne.Id));
            document.AddEdge(new BoardEdgeModel(BoardEdgeTypeIds.Reference, note.Id, classTwo.Id));
            document.ViewState.SelectedNodeId = classTwo.Id;
            return document;
        }

        private static void SetTemplates(BoardDocument document)
        {
            document.Templates.Clear();
            document.Templates.AddRange(new List<BoardTemplateDefinition>
            {
                new BoardTemplateDefinition(BoardPresetIds.Empty, "Empty", "Start with a blank board and add your own structure."),
                new BoardTemplateDefinition(BoardPresetIds.SoloIndie, "Solo Indie", "Vision, slice planning, and reference capture for a solo project."),
                new BoardTemplateDefinition(BoardPresetIds.SmallTeam, "Small Team", "A collaborative planning board for design, art, engineering, and production."),
                new BoardTemplateDefinition(BoardPresetIds.TechnicalDesign, "Technical Design", "A secondary workflow for class maps and implementation planning.")
            });
        }
    }
}
