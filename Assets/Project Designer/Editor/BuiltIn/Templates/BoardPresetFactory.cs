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
                case BoardPresetIds.ProjectDesignerRedo:
                    return CreateProjectDesignerRedo(boardName);
                case BoardPresetIds.SoloIndie:
                    return CreateSoloIndie(boardName);
                case BoardPresetIds.SmallTeam:
                    return CreateSmallTeam(boardName);
                case BoardPresetIds.TechnicalDesign:
                    return CreateTechnicalDesign(boardName);
                case BoardPresetIds.PitchVision:
                    return CreatePitchVision(boardName);
                case BoardPresetIds.MilestoneRoadmap:
                    return CreateMilestoneRoadmap(boardName);
                case BoardPresetIds.ResearchReference:
                    return CreateResearchReference(boardName);
                case BoardPresetIds.StakeholderReview:
                    return CreateStakeholderReview(boardName);
                default:
                    return CreateEmpty(boardName, true);
            }
        }

        public static BoardDocument CreateEmpty(string boardName = null, bool includeProjectBrief = false)
        {
            var document = new BoardDocument(boardName ?? ProjectDesignerProductInfo.DefaultBoardName);
            document.Summary = "Plan pre-production, references, and technical direction in one Unity planning board.";
            document.TeamMembers.Clear();
            document.TeamMembers.AddRange(new[] { "Producer", "Designer", "Programmer", "Artist" });
            SetTemplates(document);
            ApplySavedFilters(document);

            if (includeProjectBrief)
            {
                ProjectBriefNodeModel brief = CreateProjectBriefNode(
                    document,
                    new Vector2(120f, 100f),
                    "Capture the pitch, team roles, and the shared planning context here.");
                document.AddNode(brief);
                document.ViewState.SelectedNodeId = brief.Id;
            }

            return document;
        }

        public static BoardDocument CreateProjectDesignerRedo(string boardName = null)
        {
            BoardDocument document = CreateEmpty(boardName ?? ProjectDesignerProductInfo.ProjectDesignerRedoBoardName, false);
            document.Summary = "Flagship showcase board for the Project Designer+ rewrite, covering planner architecture, interaction polish, docs, demo content, and launch prep.";
            document.TeamMembers.Clear();
            document.TeamMembers.AddRange(new[] { "Product Design", "Tools Engineering", "Documentation", "Launch Producer" });
            ApplySavedFilters(document,
                new BoardSavedFilter("Core Planner", string.Empty, BoardNodeCategories.All, "planner-core", true),
                new BoardSavedFilter("Launch", string.Empty, BoardNodeCategories.All, "launch", true),
                new BoardSavedFilter("Samples", string.Empty, BoardNodeCategories.All, "sample", true),
                new BoardSavedFilter("Polish", string.Empty, BoardNodeCategories.All, "polish", true));

            ProjectBriefNodeModel brief = CreateProjectBriefNode(
                document,
                new Vector2(120f, 100f),
                "Rebuild the legacy asset into a UPM-first planner that is easier to understand, easier to extend, and strong enough to demo on its own. Keep technical design present, but make planning the obvious center of gravity.");

            MilestoneNodeModel plannerCore = CreateMilestone(
                "Planner Core Stable",
                "Board interactions, package architecture, and day-to-day planning workflows feel trustworthy enough to use continuously.",
                new Vector2(760f, 100f),
                "2026-05-06",
                "planner-core, milestone");

            MilestoneNodeModel showcaseReady = CreateMilestone(
                "Showcase & Docs Ready",
                "Examples, onboarding, docs, and sample surfaces clearly show what the planner can do.",
                new Vector2(1140f, 100f),
                "2026-05-13",
                "showcase, docs, milestone");

            MilestoneNodeModel launchReady = CreateMilestone(
                "Store Submission Ready",
                "Package messaging, screenshots, demo boards, and final review prep are aligned for launch.",
                new Vector2(1520f, 100f),
                "2026-05-20",
                "launch, milestone");

            TaskNodeModel repositioning = CreateTask(
                "Reposition package as planner",
                "Rewrite the product story so teams understand this as a pre-production planner first, with technical design as a secondary workflow.",
                new Vector2(120f, 420f),
                "product-design",
                3,
                TaskNodeStatus.Done,
                TaskNodePriority.Critical,
                "2026-05-01",
                "The README, onboarding, and listing draft all lead with planning instead of UML.",
                "launch, positioning");

            TaskNodeModel packageRewrite = CreateTask(
                "Ship UPM-first package structure",
                "Move the rewrite into a package layout with clearer runtime, editor, sample, and test boundaries.",
                new Vector2(460f, 420f),
                "tools-engineering",
                5,
                TaskNodeStatus.Done,
                TaskNodePriority.Critical,
                "2026-05-03",
                "The package imports cleanly and stops depending on the legacy asset folder for v2 behavior.",
                "planner-core, package, technical");

            TaskNodeModel interactionPolish = CreateTask(
                "Polish board interactions",
                "Tighten drag, selection, connection flow, and board operations until the planner feels fast instead of fragile.",
                new Vector2(800f, 420f),
                "tools-engineering",
                8,
                TaskNodeStatus.InProgress,
                TaskNodePriority.Critical,
                "2026-05-08",
                "Daily planning actions work without needing workaround explanations in the docs.",
                "planner-core, polish");

            TaskNodeModel onboardingAndFinder = CreateTask(
                "Add onboarding and project finder",
                "Create the product shell around the planner so first-run discovery feels intentional.",
                new Vector2(1140f, 420f),
                "product-design",
                5,
                TaskNodeStatus.Done,
                TaskNodePriority.High,
                "2026-05-07",
                "The user can create, find, and reopen boards without hunting around the Project window.",
                "planner-core, discoverability");

            TaskNodeModel rosterAndInsights = CreateTask(
                "Add roster-backed workload views",
                "Move assignment into a project-wide roster and use that data for board summaries, filters, and insight cards.",
                new Vector2(800f, 700f),
                "tools-engineering",
                5,
                TaskNodeStatus.InProgress,
                TaskNodePriority.High,
                "2026-05-10",
                "Assignees come from one roster, and workload summaries stop drifting into duplicate names.",
                "planner-core, insights");

            TaskNodeModel showcaseBoards = CreateTask(
                "Produce showcase boards",
                "Seed richer examples, a flagship redo board, and clearer sample packaging so users can inspect real planning density quickly.",
                new Vector2(1140f, 700f),
                "documentation",
                5,
                TaskNodeStatus.InProgress,
                TaskNodePriority.High,
                "2026-05-12",
                "The package ships with examples that demonstrate planning depth, not just empty starter content.",
                "showcase, sample, docs");

            TaskNodeModel launchMaterials = CreateTask(
                "Finalize launch copy and media checklist",
                "Align package messaging, screenshots, and release notes around the same planner-first story.",
                new Vector2(1480f, 420f),
                "launch-producer",
                3,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.High,
                "2026-05-16",
                "The Asset Store draft, screenshots, and release checklist promise only what the shipped package really does.",
                "launch, docs, marketing");

            NoteNodeModel guardrails = CreateNote(
                "Product Guardrails",
                "- Keep planning as the headline.\n- Treat extensibility as a strong advanced path, not the default story.\n- Keep technical design useful but secondary.\n- Avoid demo boards that look like tiny tutorial scraps.",
                new Vector2(120f, 760f),
                "#F28C38",
                "decision, launch");

            NoteNodeModel openQuestions = CreateNote(
                "Open Questions",
                "- Do we need a stronger review/export path before launch?\n- Which finder polish items can wait without hurting first-run clarity?\n- Should smart left-to-right layout become the post-launch hero feature?",
                new Vector2(460f, 760f),
                "#2B90D9",
                "question, risk, launch");

            ReferenceNodeModel listingDraft = CreateReference(
                "Launch Positioning Draft",
                "Current package and store messaging centered on planner-first positioning, workflow presets, and extension hooks.",
                new Vector2(1840f, 240f),
                string.Empty,
                "Use this as the source for README copy, onboarding messaging, and store-ready language.",
                "launch, docs");

            ReferenceNodeModel uiDirection = CreateReference(
                "Planner UI Direction",
                "Notes for keeping the board readable: stronger accents, simpler chips, lighter surfaces in light mode, and a darker but not muddy dark mode.",
                new Vector2(1840f, 500f),
                string.Empty,
                "Tie the board look back to the onboarding shell so the product feels consistent.",
                "polish, ui");

            ReferenceNodeModel sampleGoals = CreateReference(
                "Sample Board Goals",
                "Examples should prove planning depth, launch readiness, extensibility, and the package-friendly architecture in one pass.",
                new Vector2(1840f, 760f),
                string.Empty,
                "Use this as a checklist when deciding whether a sample board is explanatory enough.",
                "showcase, sample");

            ClassNodeModel workspaceView = CreateClass(
                "ProjectDesignerWorkspaceView",
                "ProjectDesigner.V2.Editor",
                "Coordinates the planner shell, board canvas, inspector refresh, and overview surfaces.",
                new Vector2(1480f, 780f),
                new[]
                {
                    new BoardClassMemberData("_board: ProjectBoardAsset", "private"),
                    new BoardClassMemberData("_commandStack: ProjectDesignerCommandStack", "private")
                },
                new[]
                {
                    new BoardClassMemberData("BindBoard(ProjectBoardAsset)", "public"),
                    new BoardClassMemberData("RefreshBoard()", "public"),
                    new BoardClassMemberData("RebuildInspector()", "private")
                },
                "technical, planner-core");

            ClassNodeModel boardInsights = CreateClass(
                "BoardInsights",
                "ProjectDesigner.V2.Data",
                "Derives workload, risk, dependency, milestone, and quick-filter signals from the board document.",
                new Vector2(1140f, 980f),
                new[]
                {
                    new BoardClassMemberData("AssigneeSummaries: IReadOnlyList<BoardAssigneeSummary>", "public"),
                    new BoardClassMemberData("MilestoneReports: IReadOnlyList<BoardMilestoneHealthReport>", "public")
                },
                new[]
                {
                    new BoardClassMemberData("GetVisibleNodes(BoardDocument)", "public"),
                    new BoardClassMemberData("GetAssigneeSummaries(BoardDocument)", "public"),
                    new BoardClassMemberData("GetMilestoneHealth(BoardDocument, MilestoneNodeModel)", "public")
                },
                "technical, insights");

            document.AddNode(brief);
            document.AddNode(plannerCore);
            document.AddNode(showcaseReady);
            document.AddNode(launchReady);
            document.AddNode(repositioning);
            document.AddNode(packageRewrite);
            document.AddNode(interactionPolish);
            document.AddNode(onboardingAndFinder);
            document.AddNode(rosterAndInsights);
            document.AddNode(showcaseBoards);
            document.AddNode(launchMaterials);
            document.AddNode(guardrails);
            document.AddNode(openQuestions);
            document.AddNode(listingDraft);
            document.AddNode(uiDirection);
            document.AddNode(sampleGoals);
            document.AddNode(workspaceView);
            document.AddNode(boardInsights);

            Connect(document, BoardEdgeTypeIds.Dependency, packageRewrite, repositioning);
            Connect(document, BoardEdgeTypeIds.Dependency, interactionPolish, packageRewrite);
            Connect(document, BoardEdgeTypeIds.Dependency, interactionPolish, onboardingAndFinder);
            Connect(document, BoardEdgeTypeIds.Dependency, rosterAndInsights, interactionPolish);
            Connect(document, BoardEdgeTypeIds.Dependency, showcaseBoards, interactionPolish);
            Connect(document, BoardEdgeTypeIds.Dependency, launchMaterials, showcaseBoards);

            Connect(document, BoardEdgeTypeIds.Milestone, repositioning, plannerCore);
            Connect(document, BoardEdgeTypeIds.Milestone, packageRewrite, plannerCore);
            Connect(document, BoardEdgeTypeIds.Milestone, interactionPolish, plannerCore);
            Connect(document, BoardEdgeTypeIds.Milestone, onboardingAndFinder, plannerCore);
            Connect(document, BoardEdgeTypeIds.Milestone, rosterAndInsights, plannerCore);
            Connect(document, BoardEdgeTypeIds.Milestone, showcaseBoards, showcaseReady);
            Connect(document, BoardEdgeTypeIds.Milestone, launchMaterials, launchReady);

            Connect(document, BoardEdgeTypeIds.Reference, guardrails, interactionPolish);
            Connect(document, BoardEdgeTypeIds.Reference, openQuestions, launchMaterials);
            Connect(document, BoardEdgeTypeIds.Reference, listingDraft, repositioning);
            Connect(document, BoardEdgeTypeIds.Reference, listingDraft, launchMaterials);
            Connect(document, BoardEdgeTypeIds.Reference, uiDirection, interactionPolish);
            Connect(document, BoardEdgeTypeIds.Reference, sampleGoals, showcaseBoards);
            Connect(document, BoardEdgeTypeIds.TechnicalRelation, workspaceView, boardInsights);

            document.ViewState.SelectedNodeId = brief.Id;
            return document;
        }

        public static BoardDocument CreateSoloIndie(string boardName = null)
        {
            BoardDocument document = CreateEmpty(boardName ?? ProjectDesignerProductInfo.SoloBoardName, false);
            document.Summary = "A showcase solo-dev board for vision, slice planning, reference capture, and a few technical breadcrumbs.";
            document.TeamMembers.Clear();
            document.TeamMembers.AddRange(new[] { "Founder", "Contract Audio", "Freelance QA" });
            ApplySavedFilters(document,
                new BoardSavedFilter("Slice Work", string.Empty, BoardNodeCategories.Planning, "slice", false),
                new BoardSavedFilter("Risks", string.Empty, BoardNodeCategories.All, "risk", true),
                new BoardSavedFilter("Pitch", string.Empty, BoardNodeCategories.All, "pitch", true));

            ProjectBriefNodeModel brief = CreateProjectBriefNode(
                document,
                new Vector2(120f, 100f),
                "Player fantasy: a tense salvage run where every corridor hides a small story. Keep the slice focused on readable combat, tactile looting, and one memorable extraction.");

            MilestoneNodeModel pitchLock = CreateMilestone(
                "Pitch Lock",
                "The one-sentence pitch, mood direction, and reference stack are solid enough to share externally.",
                new Vector2(640f, 100f),
                "2026-05-12",
                "pitch, milestone");

            MilestoneNodeModel verticalSlice = CreateMilestone(
                "Vertical Slice Ready",
                "First playable loop, target atmosphere, and clear success metrics are ready for outside feedback.",
                new Vector2(1020f, 100f),
                "2026-06-01",
                "slice, milestone");

            TaskNodeModel playerPromise = CreateTask(
                "Define player promise",
                "Lock the fantasy, tension curve, and the verbs that make the scavenging run feel different.",
                new Vector2(120f, 380f),
                "Founder",
                2,
                TaskNodeStatus.Done,
                TaskNodePriority.Critical,
                "2026-05-02",
                "One-sentence pitch, three pillars, and one anti-pillar are written down.",
                "vision, pitch");

            TaskNodeModel referencePillars = CreateTask(
                "Curate reference pillars",
                "Select three visual references and three interaction references that the slice can realistically hit.",
                new Vector2(460f, 380f),
                "Founder",
                2,
                TaskNodeStatus.InProgress,
                TaskNodePriority.High,
                "2026-05-06",
                "References are tagged by mood, camera, interaction, and production risk.",
                "research, art, pitch");

            TaskNodeModel greyboxEncounter = CreateTask(
                "Greybox first encounter",
                "Prototype the first scavenging room, one enemy pressure beat, and the extraction exit.",
                new Vector2(800f, 380f),
                "Founder",
                5,
                TaskNodeStatus.InProgress,
                TaskNodePriority.Critical,
                "2026-05-17",
                "Room layout supports stealth, panic, and a clear extraction route.",
                "slice, gameplay, prototype");

            TaskNodeModel cameraPass = CreateTask(
                "Camera readability pass",
                "Tune framing, distance, and threat readability so the player can parse danger quickly.",
                new Vector2(1140f, 380f),
                "Founder",
                3,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.High,
                "2026-05-20",
                "The first encounter is readable in motion and in captured GIFs.",
                "slice, ux, combat");

            TaskNodeModel playtestPlan = CreateTask(
                "Prepare five-minute playtest",
                "Create a tiny test plan, the observation checklist, and one success metric per pillar.",
                new Vector2(800f, 660f),
                "Freelance QA",
                2,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.Medium,
                "2026-05-24",
                "Testers can finish the slice, and note-taking stays focused on the intended questions.",
                "slice, playtest");

            TaskNodeModel marketingGif = CreateTask(
                "Record pitch GIF",
                "Capture one short clip that communicates the player fantasy and target mood without explanation.",
                new Vector2(1140f, 660f),
                "Founder",
                2,
                TaskNodeStatus.Blocked,
                TaskNodePriority.Medium,
                "2026-05-27",
                "Clip shows the hook in under ten seconds and uses the current lighting target.",
                "pitch, marketing, risk");

            NoteNodeModel risks = CreateNote(
                "Current Risks",
                "- Lighting target may be too expensive for solo production.\n- Enemy readability is still muddy in close quarters.\n- The extraction payoff needs a stronger audio cue.",
                new Vector2(120f, 700f),
                "#E58C4A",
                "risk, decision");

            NoteNodeModel playtestLearnings = CreateNote(
                "Early Learnings",
                "The loop clicks when the player spots loot before danger. Keep line-of-sight decisions readable and make the extraction feel like a reward, not just an exit trigger.",
                new Vector2(460f, 700f),
                "#4C7AFF",
                "feedback, playtest");

            ReferenceNodeModel moodBoard = CreateReference(
                "Mood Board Shortlist",
                "Warm industrial interiors, high-contrast item silhouettes, and small environmental storytelling beats.",
                new Vector2(1480f, 280f),
                string.Empty,
                "Focus on color accents, clutter density, and salvage-object readability.",
                "art, research");

            ReferenceNodeModel cameraReference = CreateReference(
                "Camera Language Notes",
                "Tight framing during scavenging, wider pullback during panic, and one hero framing moment near extraction.",
                new Vector2(1480f, 540f),
                "https://www.gdcvault.com/",
                "Bookmark talks on readable action framing and environmental pressure.",
                "ux, camera, research");

            ReferenceNodeModel audioReference = CreateReference(
                "Audio Hooks",
                "Short mechanical stingers for extraction and low-fi threat buildup for the encounter.",
                new Vector2(1480f, 800f),
                string.Empty,
                "Contract audio should prioritize one extraction cue and one danger swell.",
                "audio, slice");

            document.AddNode(brief);
            document.AddNode(pitchLock);
            document.AddNode(verticalSlice);
            document.AddNode(playerPromise);
            document.AddNode(referencePillars);
            document.AddNode(greyboxEncounter);
            document.AddNode(cameraPass);
            document.AddNode(playtestPlan);
            document.AddNode(marketingGif);
            document.AddNode(risks);
            document.AddNode(playtestLearnings);
            document.AddNode(moodBoard);
            document.AddNode(cameraReference);
            document.AddNode(audioReference);

            Connect(document, BoardEdgeTypeIds.Dependency, referencePillars, playerPromise);
            Connect(document, BoardEdgeTypeIds.Dependency, greyboxEncounter, playerPromise);
            Connect(document, BoardEdgeTypeIds.Dependency, cameraPass, greyboxEncounter);
            Connect(document, BoardEdgeTypeIds.Dependency, playtestPlan, greyboxEncounter);
            Connect(document, BoardEdgeTypeIds.Dependency, marketingGif, cameraPass);

            Connect(document, BoardEdgeTypeIds.Milestone, playerPromise, pitchLock);
            Connect(document, BoardEdgeTypeIds.Milestone, referencePillars, pitchLock);
            Connect(document, BoardEdgeTypeIds.Milestone, greyboxEncounter, verticalSlice);
            Connect(document, BoardEdgeTypeIds.Milestone, cameraPass, verticalSlice);
            Connect(document, BoardEdgeTypeIds.Milestone, playtestPlan, verticalSlice);
            Connect(document, BoardEdgeTypeIds.Milestone, marketingGif, verticalSlice);

            Connect(document, BoardEdgeTypeIds.Reference, moodBoard, referencePillars);
            Connect(document, BoardEdgeTypeIds.Reference, cameraReference, greyboxEncounter);
            Connect(document, BoardEdgeTypeIds.Reference, audioReference, marketingGif);
            Connect(document, BoardEdgeTypeIds.Reference, risks, marketingGif);
            Connect(document, BoardEdgeTypeIds.Reference, playtestLearnings, cameraPass);

            document.ViewState.SelectedNodeId = brief.Id;
            return document;
        }

        public static BoardDocument CreateSmallTeam(string boardName = null)
        {
            BoardDocument document = CreateEmpty(boardName ?? ProjectDesignerProductInfo.SmallTeamBoardName, false);
            document.Summary = "A richer small-team board that shows cross-discipline planning, stakeholder prep, and reference-heavy collaboration.";
            document.TeamMembers.Clear();
            document.TeamMembers.AddRange(new[] { "Creative Director", "Producer", "Gameplay Engineer", "Level Designer", "UI Designer", "Tech Artist" });
            ApplySavedFilters(document,
                new BoardSavedFilter("Review Prep", string.Empty, BoardNodeCategories.All, "review", true),
                new BoardSavedFilter("Production Risks", string.Empty, BoardNodeCategories.All, "risk", true),
                new BoardSavedFilter("Art Research", string.Empty, BoardNodeCategories.Reference, "art", false));

            ProjectBriefNodeModel brief = CreateProjectBriefNode(
                document,
                new Vector2(120f, 100f),
                "This board should answer one question every week: are we aligned on the slice that earns stakeholder confidence without overcommitting production?");

            MilestoneNodeModel pillarsLock = CreateMilestone(
                "Pillars Signed Off",
                "Creative direction, product pillars, and tone references are locked internally.",
                new Vector2(640f, 100f),
                "2026-05-09",
                "milestone, review");

            MilestoneNodeModel scopeLock = CreateMilestone(
                "Vertical Slice Scope Approved",
                "The team agrees on the slice scope, success metrics, and known production tradeoffs.",
                new Vector2(1020f, 100f),
                "2026-05-23",
                "slice, milestone");

            MilestoneNodeModel stakeholderReview = CreateMilestone(
                "Stakeholder Review Ready",
                "Deck, playable slice, and reference stack are coherent enough for outside review.",
                new Vector2(1400f, 100f),
                "2026-06-05",
                "review, milestone");

            TaskNodeModel pillars = CreateTask(
                "Define feature pillars",
                "Agree on three product pillars and one clear anti-pillar that protects the team from scope drift.",
                new Vector2(120f, 360f),
                "Creative Director",
                3,
                TaskNodeStatus.Done,
                TaskNodePriority.Critical,
                "2026-05-03",
                "Pillars map to player promise, fantasy, and demo talking points.",
                "design, pillars, review");

            TaskNodeModel onboardingFlow = CreateTask(
                "Capture onboarding flow",
                "Storyboard the first two minutes so engineering, level design, and UI align on pacing.",
                new Vector2(420f, 360f),
                "UI Designer",
                3,
                TaskNodeStatus.InProgress,
                TaskNodePriority.High,
                "2026-05-10",
                "Flow shows first interaction, first threat, and first reward.",
                "ux, slice");

            TaskNodeModel missionGreybox = CreateTask(
                "Greybox mission path",
                "Lay out the shortest path, optional detour, and one deliberate pressure spike.",
                new Vector2(720f, 360f),
                "Level Designer",
                5,
                TaskNodeStatus.InProgress,
                TaskNodePriority.Critical,
                "2026-05-15",
                "Mission path supports the intended pillar beats without needing content that will be cut later.",
                "slice, gameplay, level");

            TaskNodeModel cameraPrototype = CreateTask(
                "Prototype combat camera",
                "Tune distance, lock-on handoff, and readability for group encounters.",
                new Vector2(1020f, 360f),
                "Gameplay Engineer",
                5,
                TaskNodeStatus.Blocked,
                TaskNodePriority.High,
                "2026-05-18",
                "Camera supports one-on-one and crowd reads without confusing priority targets.",
                "combat, risk, slice");

            TaskNodeModel lightingFrames = CreateTask(
                "Build lighting target frames",
                "Create three key frames the team can rally around while asset scope is still fluid.",
                new Vector2(1320f, 360f),
                "Tech Artist",
                3,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.Medium,
                "2026-05-19",
                "Frames are usable in deck slides and in production handoff notes.",
                "art, review");

            TaskNodeModel budgetReview = CreateTask(
                "Budget review pass",
                "Flag scope that threatens the slice budget and define the cheapest fallback for each risky area.",
                new Vector2(1620f, 360f),
                "Producer",
                2,
                TaskNodeStatus.Done,
                TaskNodePriority.High,
                "2026-05-08",
                "Top three scope threats have named owners and fallback plans.",
                "production, risk");

            TaskNodeModel deckNarrative = CreateTask(
                "Review deck narrative",
                "Make sure the pitch deck, board, and playable slice all tell the same story.",
                new Vector2(1020f, 640f),
                "Producer",
                2,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.Medium,
                "2026-05-28",
                "Slides, milestone names, and demo beats reinforce the same player promise.",
                "review, pitch");

            TaskNodeModel playtestOps = CreateTask(
                "Playtest ops checklist",
                "Confirm build handoff, questions, observers, and success metrics for the stakeholder session.",
                new Vector2(1320f, 640f),
                "Producer",
                2,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.Medium,
                "2026-06-02",
                "Test plan runs in 20 minutes and captures the questions leadership actually cares about.",
                "playtest, review");

            NoteNodeModel openQuestions = CreateNote(
                "Open Questions",
                "- Is the first mission too dialogue-heavy for the slice?\n- Do we need one more enemy archetype to sell the combat pillar?\n- Which deck slide best explains the social co-op angle?",
                new Vector2(120f, 680f),
                "#F28C38",
                "risk, stakeholder");

            NoteNodeModel stakeholderSignals = CreateNote(
                "What Stakeholders Will Look For",
                "Clarity of the fantasy, cost discipline, and one unmistakable moment that shows why this project deserves the next phase.",
                new Vector2(420f, 680f),
                "#4C7AFF",
                "review, alignment");

            ReferenceNodeModel benchmarkBoard = CreateReference(
                "Competitive Inspiration",
                "Readable combat, short mission loops, and strong mission-end reward framing.",
                new Vector2(1680f, 560f),
                string.Empty,
                "Tag each reference by combat readability, UI, pacing, and production complexity.",
                "benchmark, art, review");

            ReferenceNodeModel uiReference = CreateReference(
                "Onboarding UI Reference",
                "Reference stack for first-use HUD cadence, objective prompts, and co-op readiness.",
                new Vector2(1680f, 820f),
                "https://www.figma.com/community",
                "Capture examples where the first two minutes teach without drowning the player in chrome.",
                "ux, art");

            ReferenceNodeModel lightingLookbook = CreateReference(
                "Lighting Lookbook",
                "Three target atmospheres for the slice: briefing room, mission pressure, and extraction relief.",
                new Vector2(1680f, 1080f),
                string.Empty,
                "Each frame should communicate gameplay readability plus the mood the deck promises.",
                "art, review");

            document.AddNode(brief);
            document.AddNode(pillarsLock);
            document.AddNode(scopeLock);
            document.AddNode(stakeholderReview);
            document.AddNode(pillars);
            document.AddNode(onboardingFlow);
            document.AddNode(missionGreybox);
            document.AddNode(cameraPrototype);
            document.AddNode(lightingFrames);
            document.AddNode(budgetReview);
            document.AddNode(deckNarrative);
            document.AddNode(playtestOps);
            document.AddNode(openQuestions);
            document.AddNode(stakeholderSignals);
            document.AddNode(benchmarkBoard);
            document.AddNode(uiReference);
            document.AddNode(lightingLookbook);

            Connect(document, BoardEdgeTypeIds.Dependency, onboardingFlow, pillars);
            Connect(document, BoardEdgeTypeIds.Dependency, missionGreybox, pillars);
            Connect(document, BoardEdgeTypeIds.Dependency, cameraPrototype, missionGreybox);
            Connect(document, BoardEdgeTypeIds.Dependency, lightingFrames, missionGreybox);
            Connect(document, BoardEdgeTypeIds.Dependency, deckNarrative, pillars);
            Connect(document, BoardEdgeTypeIds.Dependency, playtestOps, deckNarrative);

            Connect(document, BoardEdgeTypeIds.Milestone, pillars, pillarsLock);
            Connect(document, BoardEdgeTypeIds.Milestone, budgetReview, pillarsLock);
            Connect(document, BoardEdgeTypeIds.Milestone, onboardingFlow, scopeLock);
            Connect(document, BoardEdgeTypeIds.Milestone, missionGreybox, scopeLock);
            Connect(document, BoardEdgeTypeIds.Milestone, cameraPrototype, scopeLock);
            Connect(document, BoardEdgeTypeIds.Milestone, lightingFrames, stakeholderReview);
            Connect(document, BoardEdgeTypeIds.Milestone, deckNarrative, stakeholderReview);
            Connect(document, BoardEdgeTypeIds.Milestone, playtestOps, stakeholderReview);

            Connect(document, BoardEdgeTypeIds.Reference, benchmarkBoard, missionGreybox);
            Connect(document, BoardEdgeTypeIds.Reference, uiReference, onboardingFlow);
            Connect(document, BoardEdgeTypeIds.Reference, lightingLookbook, lightingFrames);
            Connect(document, BoardEdgeTypeIds.Reference, openQuestions, cameraPrototype);
            Connect(document, BoardEdgeTypeIds.Reference, stakeholderSignals, deckNarrative);

            document.ViewState.SelectedNodeId = brief.Id;
            return document;
        }

        public static BoardDocument CreateTechnicalDesign(string boardName = null)
        {
            BoardDocument document = CreateEmpty(boardName ?? ProjectDesignerProductInfo.TechnicalBoardName, false);
            document.Summary = "A denser technical-design board that demonstrates architecture mapping, service relationships, and implementation notes.";
            document.TeamMembers.Clear();
            document.TeamMembers.AddRange(new[] { "Tools Engineer", "Technical Designer", "Pipeline Engineer" });
            document.ViewState.Category = BoardNodeCategories.TechnicalDesign;
            ApplySavedFilters(document,
                new BoardSavedFilter("Architecture", string.Empty, BoardNodeCategories.TechnicalDesign, string.Empty, true),
                new BoardSavedFilter("Importers", string.Empty, BoardNodeCategories.TechnicalDesign, "importer", true),
                new BoardSavedFilter("Risks", string.Empty, BoardNodeCategories.All, "risk", true));

            ProjectBriefNodeModel brief = CreateProjectBriefNode(
                document,
                new Vector2(120f, 100f),
                "Keep the product surface simple, but keep the internals modular: board data, editor shell, drag interactions, and importers should evolve independently.");

            ClassNodeModel bootstrapper = CreateClass(
                "PlannerBootstrapper",
                "ProjectDesigner.V2.Editor",
                "Creates the planner shell and wires registration before the window renders.",
                new Vector2(120f, 360f),
                new[]
                {
                    new BoardClassMemberData("_registry : ProjectDesignerRegistry", "private"),
                    new BoardClassMemberData("_window : ProjectDesignerV2Window", "private")
                },
                new[]
                {
                    new BoardClassMemberData("Initialize()", "public"),
                    new BoardClassMemberData("OpenBoard(ProjectBoardAsset board)", "public")
                },
                "entry, shell");

            ClassNodeModel documentRepository = CreateClass(
                "BoardDocumentRepository",
                "ProjectDesigner.V2.Runtime",
                "Owns serialization, cloning, and persistence boundaries for board documents.",
                new Vector2(500f, 360f),
                new[]
                {
                    new BoardClassMemberData("_serializer : EditorJsonUtility", "private"),
                    new BoardClassMemberData("_asset : ProjectBoardAsset", "private")
                },
                new[]
                {
                    new BoardClassMemberData("Load()", "public"),
                    new BoardClassMemberData("Save(BoardDocument document)", "public")
                },
                "persistence, serialization");

            ClassNodeModel canvasController = CreateClass(
                "PlannerCanvasController",
                "ProjectDesigner.V2.Editor",
                "Handles pan, zoom, drag previews, and selection state for the canvas layer.",
                new Vector2(880f, 360f),
                new[]
                {
                    new BoardClassMemberData("_previewPositions : Dictionary<string, Vector2>", "private"),
                    new BoardClassMemberData("_selection : string", "private")
                },
                new[]
                {
                    new BoardClassMemberData("BeginDrag(string nodeId)", "public"),
                    new BoardClassMemberData("UpdateDrag(Vector2 screenPosition)", "public"),
                    new BoardClassMemberData("FrameAll()", "public")
                },
                "interaction, canvas");

            ClassNodeModel linkValidation = CreateClass(
                "LinkValidationService",
                "ProjectDesigner.V2.Editor",
                "Resolves which links are valid for a selected card and powers the simplified inspector UX.",
                new Vector2(1260f, 360f),
                new[]
                {
                    new BoardClassMemberData("_edgeDefinitions : IEnumerable<IProjectDesignerEdgeDefinition>", "private")
                },
                new[]
                {
                    new BoardClassMemberData("GetValidTargets(BoardNodeModel selectedNode)", "public"),
                    new BoardClassMemberData("CreateEdge(BoardNodeModel source, BoardNodeModel target)", "public")
                },
                "links, ux");

            ClassNodeModel assetImportPipeline = CreateClass(
                "AssetImportPipeline",
                "ProjectDesigner.V2.Editor",
                "Maps Unity assets onto reference or class cards through explicit importer registration.",
                new Vector2(500f, 700f),
                new[]
                {
                    new BoardClassMemberData("_importers : List<IProjectDesignerAssetImporter>", "private")
                },
                new[]
                {
                    new BoardClassMemberData("CanImport(Object asset)", "public"),
                    new BoardClassMemberData("Import(Object asset, Vector2 position)", "public")
                },
                "importer, extensibility");

            ClassNodeModel overviewMetrics = CreateClass(
                "OverviewMetricsService",
                "ProjectDesigner.V2.Editor",
                "Computes milestone progress and status counts for the docked overview cards.",
                new Vector2(880f, 700f),
                new[]
                {
                    new BoardClassMemberData("_document : BoardDocument", "private")
                },
                new[]
                {
                    new BoardClassMemberData("CountTasksByStatus(TaskNodeStatus status)", "public"),
                    new BoardClassMemberData("CalculateMilestoneCompletion(MilestoneNodeModel milestone)", "public")
                },
                "metrics, overview");

            ClassNodeModel extensionRegistry = CreateClass(
                "ExtensionRegistryFacade",
                "ProjectDesigner.V2.Editor",
                "Provides a single extension-facing registration surface for nodes, edges, inspectors, and importers.",
                new Vector2(1260f, 700f),
                new[]
                {
                    new BoardClassMemberData("_definitions : RegistrySnapshot", "private")
                },
                new[]
                {
                    new BoardClassMemberData("RegisterNode(IProjectDesignerNodeDefinition definition)", "public"),
                    new BoardClassMemberData("RegisterImporter(IProjectDesignerAssetImporter importer)", "public")
                },
                "extensibility, importer");

            NoteNodeModel migrationNotes = CreateNote(
                "Migration Constraints",
                "- Keep the package UPM-friendly.\n- Avoid dragging legacy IMGUI assumptions into the new planner shell.\n- Preserve extension points even when the default UX gets simplified.",
                new Vector2(120f, 980f),
                "#4C7AFF",
                "migration, decision");

            NoteNodeModel technicalRisks = CreateNote(
                "Technical Risks",
                "- Drag interactions can regress silently across Unity versions.\n- Hidden technical complexity should not leak into the planning-first UX.\n- Importer ordering needs tests before more samples ship.",
                new Vector2(500f, 980f),
                "#E58C4A",
                "risk, architecture");

            ReferenceNodeModel uiToolkitReference = CreateReference(
                "UI Toolkit Notes",
                "Reference docs for event propagation, picking, and editor window composition.",
                new Vector2(1260f, 980f),
                "https://docs.unity3d.com/Manual/UIElements.html",
                "Useful when validating drag and selection behaviors in custom canvas controls.",
                "reference, ui");

            ReferenceNodeModel packagingReference = CreateReference(
                "UPM Packaging Checklist",
                "Keep runtime, editor, tests, docs, and samples separated so the asset behaves like a real package.",
                new Vector2(1640f, 980f),
                string.Empty,
                "Track asmdef boundaries, sample content, docs, and submission metadata together.",
                "reference, package");

            document.AddNode(brief);
            document.AddNode(bootstrapper);
            document.AddNode(documentRepository);
            document.AddNode(canvasController);
            document.AddNode(linkValidation);
            document.AddNode(assetImportPipeline);
            document.AddNode(overviewMetrics);
            document.AddNode(extensionRegistry);
            document.AddNode(migrationNotes);
            document.AddNode(technicalRisks);
            document.AddNode(uiToolkitReference);
            document.AddNode(packagingReference);

            Connect(document, BoardEdgeTypeIds.TechnicalRelation, canvasController, bootstrapper);
            Connect(document, BoardEdgeTypeIds.TechnicalRelation, canvasController, documentRepository);
            Connect(document, BoardEdgeTypeIds.TechnicalRelation, linkValidation, canvasController);
            Connect(document, BoardEdgeTypeIds.TechnicalRelation, assetImportPipeline, extensionRegistry);
            Connect(document, BoardEdgeTypeIds.TechnicalRelation, overviewMetrics, documentRepository);
            Connect(document, BoardEdgeTypeIds.TechnicalRelation, extensionRegistry, bootstrapper);

            Connect(document, BoardEdgeTypeIds.Reference, migrationNotes, bootstrapper);
            Connect(document, BoardEdgeTypeIds.Reference, technicalRisks, canvasController);
            Connect(document, BoardEdgeTypeIds.Reference, uiToolkitReference, canvasController);
            Connect(document, BoardEdgeTypeIds.Reference, packagingReference, extensionRegistry);

            document.ViewState.SelectedNodeId = bootstrapper.Id;
            return document;
        }

        public static BoardDocument CreatePitchVision(string boardName = null)
        {
            BoardDocument document = CreateEmpty(boardName ?? ProjectDesignerProductInfo.PitchVisionBoardName, false);
            document.Summary = "A workflow board for locking the player promise, target audience, pitch language, and visual north star.";
            document.TeamMembers.Clear();
            document.TeamMembers.AddRange(new[] { "Creative Director", "Producer", "Brand / Comms" });
            ApplySavedFilters(document,
                new BoardSavedFilter("Pitch", string.Empty, BoardNodeCategories.All, "pitch", true),
                new BoardSavedFilter("Audience", string.Empty, BoardNodeCategories.All, "audience", true),
                new BoardSavedFilter("Vision", string.Empty, BoardNodeCategories.All, "vision", true));

            ProjectBriefNodeModel brief = CreateProjectBriefNode(
                document,
                new Vector2(120f, 100f),
                "Answer the core pitch questions first: who is this for, what emotion are we promising, and what should never be in the pitch?");

            MilestoneNodeModel milestone = CreateMilestone(
                "Pitch Approved",
                "The one-pager, mood direction, and target audience framing are ready to circulate.",
                new Vector2(980f, 120f),
                "2026-05-16",
                "pitch, milestone");

            TaskNodeModel playerFantasy = CreateTask(
                "Define player fantasy",
                "Write the one-sentence promise, three pillars, and one anti-pillar for the project.",
                new Vector2(120f, 380f),
                "Creative Director",
                2,
                TaskNodeStatus.Done,
                TaskNodePriority.Critical,
                "2026-05-02",
                "The pitch can be explained in under 20 seconds without extra context.",
                "pitch, vision");

            TaskNodeModel audience = CreateTask(
                "Describe target audience",
                "Identify the player taste, genre expectations, and what existing games they will compare this against.",
                new Vector2(460f, 380f),
                "Producer",
                2,
                TaskNodeStatus.InProgress,
                TaskNodePriority.High,
                "2026-05-06",
                "Audience framing includes taste, expectations, and rejection criteria.",
                "audience, pitch");

            TaskNodeModel onePager = CreateTask(
                "Draft pitch one-pager",
                "Turn the fantasy, audience, and scope into a tight one-page narrative.",
                new Vector2(800f, 380f),
                "Brand / Comms",
                3,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.High,
                "2026-05-09",
                "One-pager is short enough for review and strong enough to reuse elsewhere.",
                "pitch, writing");

            TaskNodeModel visualHook = CreateTask(
                "Choose visual hook",
                "Select the key image and color direction that make the project feel distinct in a deck.",
                new Vector2(1140f, 380f),
                "Creative Director",
                2,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.Medium,
                "2026-05-10",
                "The hook is recognizable and aligned with realistic production scope.",
                "vision, art");

            NoteNodeModel nonGoals = CreateNote(
                "Non-Goals",
                "- Do not overpromise production scale.\n- Avoid describing this as a content-heavy RPG.\n- Keep the deck focused on one clear emotional hook.",
                new Vector2(120f, 660f),
                "#E58C4A",
                "pitch, risk");

            ReferenceNodeModel moodReference = CreateReference(
                "Mood Direction",
                "Compact spaces, readable silhouettes, and one strong hero frame that sells the premise quickly.",
                new Vector2(1460f, 300f),
                string.Empty,
                "Collect references that communicate tone before mechanics.",
                "vision, art");

            ReferenceNodeModel audienceReference = CreateReference(
                "Audience Benchmark Notes",
                "Capture the games, genres, and communities the pitch must resonate with.",
                new Vector2(1460f, 580f),
                string.Empty,
                "Tag the benchmarks by fantasy, tone, and production expectation.",
                "audience, research");

            document.AddNode(brief);
            document.AddNode(milestone);
            document.AddNode(playerFantasy);
            document.AddNode(audience);
            document.AddNode(onePager);
            document.AddNode(visualHook);
            document.AddNode(nonGoals);
            document.AddNode(moodReference);
            document.AddNode(audienceReference);

            Connect(document, BoardEdgeTypeIds.Dependency, audience, playerFantasy);
            Connect(document, BoardEdgeTypeIds.Dependency, onePager, audience);
            Connect(document, BoardEdgeTypeIds.Dependency, visualHook, playerFantasy);
            Connect(document, BoardEdgeTypeIds.Milestone, playerFantasy, milestone);
            Connect(document, BoardEdgeTypeIds.Milestone, audience, milestone);
            Connect(document, BoardEdgeTypeIds.Milestone, onePager, milestone);
            Connect(document, BoardEdgeTypeIds.Milestone, visualHook, milestone);
            Connect(document, BoardEdgeTypeIds.Reference, moodReference, visualHook);
            Connect(document, BoardEdgeTypeIds.Reference, audienceReference, audience);
            Connect(document, BoardEdgeTypeIds.Reference, nonGoals, onePager);

            document.ViewState.SelectedNodeId = brief.Id;
            return document;
        }

        public static BoardDocument CreateMilestoneRoadmap(string boardName = null)
        {
            BoardDocument document = CreateEmpty(boardName ?? ProjectDesignerProductInfo.MilestoneRoadmapBoardName, false);
            document.Summary = "A workflow board for shaping milestones, dependency chains, and scope conversations.";
            document.TeamMembers.Clear();
            document.TeamMembers.AddRange(new[] { "Producer", "Design Lead", "Engineering Lead" });
            ApplySavedFilters(document,
                new BoardSavedFilter("Roadmap", string.Empty, BoardNodeCategories.Planning, "roadmap", false),
                new BoardSavedFilter("Dependencies", string.Empty, BoardNodeCategories.All, "dependency", true),
                new BoardSavedFilter("Risks", string.Empty, BoardNodeCategories.All, "risk", true));

            ProjectBriefNodeModel brief = CreateProjectBriefNode(
                document,
                new Vector2(120f, 100f),
                "Use this board to talk about checkpoint shape and sequencing before the team spends time on low-value detail.");

            MilestoneNodeModel preproduction = CreateMilestone(
                "Pre-Production Complete",
                "Vision, slice scope, and top risks are aligned internally.",
                new Vector2(520f, 120f),
                "2026-05-20",
                "roadmap, milestone");

            MilestoneNodeModel verticalSlice = CreateMilestone(
                "Vertical Slice",
                "Core loop, pacing, and reference fidelity are solid enough for broader review.",
                new Vector2(900f, 120f),
                "2026-06-18",
                "roadmap, milestone");

            MilestoneNodeModel playtest = CreateMilestone(
                "External Playtest",
                "The slice can be handed to outside players with a focused observation plan.",
                new Vector2(1280f, 120f),
                "2026-07-02",
                "roadmap, milestone");

            TaskNodeModel scope = CreateTask(
                "Define slice scope",
                "Write the minimum content, mechanics, and visual target the slice needs.",
                new Vector2(120f, 380f),
                "Design Lead",
                3,
                TaskNodeStatus.InProgress,
                TaskNodePriority.Critical,
                "2026-05-08",
                "Scope protects the milestone from feature creep.",
                "roadmap, slice");

            TaskNodeModel backlog = CreateTask(
                "Prioritize milestone backlog",
                "Map backlog items to the pre-production, slice, and playtest checkpoints.",
                new Vector2(460f, 380f),
                "Producer",
                3,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.High,
                "2026-05-10",
                "Each major item has a milestone owner and a fallback plan.",
                "roadmap, dependency");

            TaskNodeModel budget = CreateTask(
                "Run scope budget review",
                "Identify the most fragile items and define the cheapest acceptable fallback for each.",
                new Vector2(800f, 380f),
                "Producer",
                2,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.High,
                "2026-05-13",
                "Top scope risks have named owners and backup options.",
                "risk, dependency");

            TaskNodeModel tools = CreateTask(
                "Prepare playtest tooling",
                "Confirm save flow, instrumentation, and observation support for the first outside session.",
                new Vector2(1140f, 380f),
                "Engineering Lead",
                3,
                TaskNodeStatus.Blocked,
                TaskNodePriority.Medium,
                "2026-06-24",
                "The playtest build can gather the evidence the team actually needs.",
                "roadmap, playtest");

            NoteNodeModel assumptions = CreateNote(
                "Roadmap Assumptions",
                "- Milestones should be decision checkpoints, not exhaustive task dumps.\n- The slice should stay narrow enough to finish with current staffing.\n- External playtest dates depend on stable onboarding.",
                new Vector2(120f, 680f),
                "#4C7AFF",
                "roadmap, risk");

            ReferenceNodeModel boundaryReference = CreateReference(
                "Scope Boundary Notes",
                "Keep a written list of features explicitly excluded from the next milestone.",
                new Vector2(1460f, 420f),
                string.Empty,
                "Useful during roadmap reviews when the team starts expanding the definition of done.",
                "roadmap, reference");

            document.AddNode(brief);
            document.AddNode(preproduction);
            document.AddNode(verticalSlice);
            document.AddNode(playtest);
            document.AddNode(scope);
            document.AddNode(backlog);
            document.AddNode(budget);
            document.AddNode(tools);
            document.AddNode(assumptions);
            document.AddNode(boundaryReference);

            Connect(document, BoardEdgeTypeIds.Dependency, backlog, scope);
            Connect(document, BoardEdgeTypeIds.Dependency, budget, backlog);
            Connect(document, BoardEdgeTypeIds.Dependency, tools, budget);
            Connect(document, BoardEdgeTypeIds.Milestone, scope, preproduction);
            Connect(document, BoardEdgeTypeIds.Milestone, backlog, preproduction);
            Connect(document, BoardEdgeTypeIds.Milestone, budget, verticalSlice);
            Connect(document, BoardEdgeTypeIds.Milestone, tools, playtest);
            Connect(document, BoardEdgeTypeIds.Reference, assumptions, budget);
            Connect(document, BoardEdgeTypeIds.Reference, boundaryReference, scope);

            document.ViewState.SelectedNodeId = brief.Id;
            return document;
        }

        public static BoardDocument CreateResearchReference(string boardName = null)
        {
            BoardDocument document = CreateEmpty(boardName ?? ProjectDesignerProductInfo.ResearchReferenceBoardName, false);
            document.Summary = "A workflow board for collecting inspiration, clustering research, and turning references into concrete decisions.";
            document.TeamMembers.Clear();
            document.TeamMembers.AddRange(new[] { "Research Lead", "Art Director", "UI Designer" });
            ApplySavedFilters(document,
                new BoardSavedFilter("Research Questions", string.Empty, BoardNodeCategories.All, "question", true),
                new BoardSavedFilter("Visual Research", string.Empty, BoardNodeCategories.Reference, "art", false),
                new BoardSavedFilter("Interaction Research", string.Empty, BoardNodeCategories.Reference, "interaction", false));

            ProjectBriefNodeModel brief = CreateProjectBriefNode(
                document,
                new Vector2(120f, 100f),
                "Treat this board as a synthesis tool: collect references, tag them, and turn them into fewer, stronger decisions.");

            MilestoneNodeModel referencePack = CreateMilestone(
                "Reference Pack Ready",
                "The research cluster is coherent enough to brief the team and move into execution.",
                new Vector2(1040f, 120f),
                "2026-05-14",
                "research, milestone");

            TaskNodeModel questions = CreateTask(
                "Define research questions",
                "Write the questions the research pass must answer before the team changes scope.",
                new Vector2(120f, 380f),
                "Research Lead",
                2,
                TaskNodeStatus.Done,
                TaskNodePriority.High,
                "2026-05-03",
                "Questions are specific enough to reject irrelevant references quickly.",
                "question, research");

            TaskNodeModel clustering = CreateTask(
                "Cluster references",
                "Tag references by mood, readability, pacing, UI, and production complexity.",
                new Vector2(460f, 380f),
                "Research Lead",
                3,
                TaskNodeStatus.InProgress,
                TaskNodePriority.High,
                "2026-05-07",
                "Clusters show clear patterns instead of becoming a scrapbook.",
                "research, synthesis");

            TaskNodeModel principles = CreateTask(
                "Extract design principles",
                "Write 5-7 practical principles that the team can apply to art, UI, and moment-to-moment play.",
                new Vector2(800f, 380f),
                "Art Director",
                3,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.Medium,
                "2026-05-10",
                "Principles are concise and decision-shaping, not descriptive filler.",
                "research, vision");

            TaskNodeModel uiPatterns = CreateTask(
                "Capture UI patterns",
                "Identify onboarding, objective, and information-density patterns worth borrowing.",
                new Vector2(1140f, 380f),
                "UI Designer",
                2,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.Medium,
                "2026-05-09",
                "Patterns include both what to borrow and what to avoid.",
                "research, interaction");

            NoteNodeModel synthesis = CreateNote(
                "Synthesis Notes",
                "The best references are not the prettiest ones. Prioritize examples that reveal a decision the team can apply next week.",
                new Vector2(120f, 680f),
                "#4C7AFF",
                "research, synthesis");

            ReferenceNodeModel visualTone = CreateReference(
                "Visual Tone Board",
                "Frames for lighting, clutter density, and silhouette readability.",
                new Vector2(1480f, 220f),
                string.Empty,
                "Use this to brief the team on atmosphere without needing a fully polished scene.",
                "art, research");

            ReferenceNodeModel interactionPatterns = CreateReference(
                "Interaction Patterns",
                "Examples of clean onboarding, readable prompts, and satisfying feedback loops.",
                new Vector2(1480f, 500f),
                string.Empty,
                "Tag each pattern by complexity and how reusable it is in the current project.",
                "interaction, research");

            ReferenceNodeModel cameraReadability = CreateReference(
                "Camera Readability",
                "Reference shots that balance tension, spatial awareness, and target visibility.",
                new Vector2(1480f, 780f),
                string.Empty,
                "Keep a bias toward examples the team can implement with existing camera systems.",
                "camera, research");

            document.AddNode(brief);
            document.AddNode(referencePack);
            document.AddNode(questions);
            document.AddNode(clustering);
            document.AddNode(principles);
            document.AddNode(uiPatterns);
            document.AddNode(synthesis);
            document.AddNode(visualTone);
            document.AddNode(interactionPatterns);
            document.AddNode(cameraReadability);

            Connect(document, BoardEdgeTypeIds.Dependency, clustering, questions);
            Connect(document, BoardEdgeTypeIds.Dependency, principles, clustering);
            Connect(document, BoardEdgeTypeIds.Dependency, uiPatterns, clustering);
            Connect(document, BoardEdgeTypeIds.Milestone, questions, referencePack);
            Connect(document, BoardEdgeTypeIds.Milestone, clustering, referencePack);
            Connect(document, BoardEdgeTypeIds.Milestone, principles, referencePack);
            Connect(document, BoardEdgeTypeIds.Milestone, uiPatterns, referencePack);
            Connect(document, BoardEdgeTypeIds.Reference, visualTone, clustering);
            Connect(document, BoardEdgeTypeIds.Reference, interactionPatterns, uiPatterns);
            Connect(document, BoardEdgeTypeIds.Reference, cameraReadability, principles);
            Connect(document, BoardEdgeTypeIds.Reference, synthesis, principles);

            document.ViewState.SelectedNodeId = brief.Id;
            return document;
        }

        public static BoardDocument CreateStakeholderReview(string boardName = null)
        {
            BoardDocument document = CreateEmpty(boardName ?? ProjectDesignerProductInfo.StakeholderReviewBoardName, false);
            document.Summary = "A workflow board for stakeholder review prep, review risks, and demo-readiness coordination.";
            document.TeamMembers.Clear();
            document.TeamMembers.AddRange(new[] { "Producer", "Creative Director", "Gameplay Engineer" });
            ApplySavedFilters(document,
                new BoardSavedFilter("Review Prep", string.Empty, BoardNodeCategories.All, "review", true),
                new BoardSavedFilter("Deck", string.Empty, BoardNodeCategories.All, "deck", true),
                new BoardSavedFilter("Risks", string.Empty, BoardNodeCategories.All, "risk", true));

            ProjectBriefNodeModel brief = CreateProjectBriefNode(
                document,
                new Vector2(120f, 100f),
                "The review board exists to help the team tell one coherent story: what we built, why it matters, and what decision we need from stakeholders.");

            MilestoneNodeModel reviewReady = CreateMilestone(
                "Review Ready",
                "Deck, demo flow, and risk framing are aligned enough for the review meeting.",
                new Vector2(980f, 120f),
                "2026-05-21",
                "review, milestone");

            TaskNodeModel narrative = CreateTask(
                "Align deck narrative",
                "Make sure the deck, milestone language, and demo beats reinforce the same player promise.",
                new Vector2(120f, 380f),
                "Producer",
                2,
                TaskNodeStatus.InProgress,
                TaskNodePriority.High,
                "2026-05-08",
                "Every slide advances the same argument instead of introducing new scope.",
                "review, deck");

            TaskNodeModel gameplayClip = CreateTask(
                "Capture gameplay clip",
                "Record one short clip that proves the core promise before the meeting starts.",
                new Vector2(460f, 380f),
                "Gameplay Engineer",
                2,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.High,
                "2026-05-12",
                "Clip is short, readable, and visible enough to survive presentation compression.",
                "review, demo");

            TaskNodeModel risks = CreateTask(
                "Summarize top risks",
                "Frame the production, scope, and design risks in terms stakeholders can actually react to.",
                new Vector2(800f, 380f),
                "Producer",
                2,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.Medium,
                "2026-05-13",
                "Each risk has an owner, a fallback, and a decision request if needed.",
                "review, risk");

            TaskNodeModel rehearsal = CreateTask(
                "Run review rehearsal",
                "Time the meeting flow, demo beats, and handoffs before the real session.",
                new Vector2(1140f, 380f),
                "Creative Director",
                2,
                TaskNodeStatus.NotStarted,
                TaskNodePriority.Medium,
                "2026-05-16",
                "The review can finish comfortably inside the expected meeting slot.",
                "review, demo");

            NoteNodeModel keyAsks = CreateNote(
                "Key Asks",
                "- Approve the current slice scope.\n- Confirm whether the visual target is strong enough.\n- Decide if the team should invest in a broader external playtest.",
                new Vector2(120f, 660f),
                "#4C7AFF",
                "review, decision");

            ReferenceNodeModel deckReference = CreateReference(
                "Review Deck Reference",
                "Examples of compact deck structure, evidence framing, and strong review pacing.",
                new Vector2(1460f, 300f),
                string.Empty,
                "Useful when the team starts over-explaining and burying the main ask.",
                "deck, review");

            ReferenceNodeModel benchmarkMoment = CreateReference(
                "Benchmark Hero Moment",
                "One example that shows what a memorable meeting demo beat looks like.",
                new Vector2(1460f, 580f),
                string.Empty,
                "Use this to keep the slice focused on one great moment instead of many weak ones.",
                "review, benchmark");

            document.AddNode(brief);
            document.AddNode(reviewReady);
            document.AddNode(narrative);
            document.AddNode(gameplayClip);
            document.AddNode(risks);
            document.AddNode(rehearsal);
            document.AddNode(keyAsks);
            document.AddNode(deckReference);
            document.AddNode(benchmarkMoment);

            Connect(document, BoardEdgeTypeIds.Dependency, gameplayClip, narrative);
            Connect(document, BoardEdgeTypeIds.Dependency, risks, narrative);
            Connect(document, BoardEdgeTypeIds.Dependency, rehearsal, gameplayClip);
            Connect(document, BoardEdgeTypeIds.Milestone, narrative, reviewReady);
            Connect(document, BoardEdgeTypeIds.Milestone, gameplayClip, reviewReady);
            Connect(document, BoardEdgeTypeIds.Milestone, risks, reviewReady);
            Connect(document, BoardEdgeTypeIds.Milestone, rehearsal, reviewReady);
            Connect(document, BoardEdgeTypeIds.Reference, keyAsks, narrative);
            Connect(document, BoardEdgeTypeIds.Reference, deckReference, narrative);
            Connect(document, BoardEdgeTypeIds.Reference, benchmarkMoment, gameplayClip);

            document.ViewState.SelectedNodeId = brief.Id;
            return document;
        }

        private static void SetTemplates(BoardDocument document)
        {
            document.Templates.Clear();
            document.Templates.AddRange(new List<BoardTemplateDefinition>
            {
                new BoardTemplateDefinition(BoardPresetIds.Empty, "Empty", "Start light with a pinned project brief card, then add your own structure."),
                new BoardTemplateDefinition(BoardPresetIds.ProjectDesignerRedo, "Project Designer+ Redo", "Flagship showcase board for the package rewrite, planner architecture, docs, and launch prep."),
                new BoardTemplateDefinition(BoardPresetIds.SoloIndie, "Solo Indie", "A fuller solo-dev sample with slice planning, risks, references, and milestone progress."),
                new BoardTemplateDefinition(BoardPresetIds.SmallTeam, "Small Team", "A denser collaborative sample for design, production, art, and stakeholder review prep."),
                new BoardTemplateDefinition(BoardPresetIds.TechnicalDesign, "Technical Design", "A richer architecture sample with multiple class relationships, notes, and package-thinking."),
                new BoardTemplateDefinition(BoardPresetIds.PitchVision, "Pitch & Vision", "Workflow preset for player promise, target audience, and pitch framing."),
                new BoardTemplateDefinition(BoardPresetIds.MilestoneRoadmap, "Milestone Roadmap", "Workflow preset for shaping checkpoints, dependencies, and scope."),
                new BoardTemplateDefinition(BoardPresetIds.ResearchReference, "Research & Reference", "Workflow preset for reference capture, synthesis, and research tasks."),
                new BoardTemplateDefinition(BoardPresetIds.StakeholderReview, "Stakeholder Review", "Workflow preset for demo prep, review asks, and risk framing.")
            });
        }

        private static void ApplySavedFilters(BoardDocument document, params BoardSavedFilter[] extraFilters)
        {
            document.SavedFilters.Clear();
            document.SavedFilters.Add(new BoardSavedFilter("Planning", string.Empty, BoardNodeCategories.Planning, string.Empty, false));
            document.SavedFilters.Add(new BoardSavedFilter("References", string.Empty, BoardNodeCategories.Reference, string.Empty, false));
            document.SavedFilters.Add(new BoardSavedFilter("Technical", string.Empty, BoardNodeCategories.TechnicalDesign, string.Empty, true));

            if (extraFilters == null)
            {
                return;
            }

            foreach (BoardSavedFilter filter in extraFilters)
            {
                if (filter != null)
                {
                    document.SavedFilters.Add(filter);
                }
            }
        }

        private static TaskNodeModel CreateTask(
            string title,
            string description,
            Vector2 position,
            string assignee,
            int estimatePoints,
            TaskNodeStatus status,
            TaskNodePriority priority,
            string dueDateIso,
            string acceptanceCriteria,
            string tagsCsv)
        {
            var task = new TaskNodeModel
            {
                Title = title,
                Description = description,
                Position = position,
                AssigneeId = assignee,
                EstimatePoints = estimatePoints,
                Status = status,
                Priority = priority,
                DueDateIso = dueDateIso,
                AcceptanceCriteria = acceptanceCriteria
            };
            task.SetTagsFromCsv(tagsCsv);
            return task;
        }

        private static MilestoneNodeModel CreateMilestone(
            string title,
            string summary,
            Vector2 position,
            string targetDateIso,
            string tagsCsv)
        {
            var milestone = new MilestoneNodeModel
            {
                Title = title,
                Summary = summary,
                Position = position,
                TargetDateIso = targetDateIso
            };
            milestone.SetTagsFromCsv(tagsCsv);
            return milestone;
        }

        private static NoteNodeModel CreateNote(
            string title,
            string body,
            Vector2 position,
            string accentHex,
            string tagsCsv)
        {
            var note = new NoteNodeModel
            {
                Title = title,
                Body = body,
                Position = position,
                AccentHex = accentHex
            };
            note.SetTagsFromCsv(tagsCsv);
            return note;
        }

        private static ReferenceNodeModel CreateReference(
            string title,
            string summary,
            Vector2 position,
            string externalUrl,
            string textReference,
            string tagsCsv)
        {
            var reference = new ReferenceNodeModel
            {
                Title = title,
                Summary = summary,
                Position = position,
                ExternalUrl = externalUrl,
                TextReference = textReference
            };
            reference.SetTagsFromCsv(tagsCsv);
            return reference;
        }

        private static ClassNodeModel CreateClass(
            string title,
            string namespaceName,
            string summary,
            Vector2 position,
            IEnumerable<BoardClassMemberData> fields,
            IEnumerable<BoardClassMemberData> methods,
            string tagsCsv)
        {
            var classNode = new ClassNodeModel
            {
                Title = title,
                NamespaceName = namespaceName,
                Summary = summary,
                Position = position
            };

            if (fields != null)
            {
                foreach (BoardClassMemberData field in fields)
                {
                    if (field != null)
                    {
                        classNode.Fields.Add(field);
                    }
                }
            }

            if (methods != null)
            {
                foreach (BoardClassMemberData method in methods)
                {
                    if (method != null)
                    {
                        classNode.Methods.Add(method);
                    }
                }
            }

            classNode.SetTagsFromCsv(tagsCsv);
            return classNode;
        }

        private static ProjectBriefNodeModel CreateProjectBriefNode(BoardDocument document, Vector2 position, string knowledgePrompt)
        {
            var brief = new ProjectBriefNodeModel
            {
                Title = ProjectDesignerProductInfo.ProjectBriefTitle,
                Position = position,
                Overview = document.Summary,
                TeamSnapshot = string.Join(", ", document.TeamMembers),
                ProjectKnowledge = knowledgePrompt
            };
            brief.IsPinned = true;
            brief.SetTagsFromCsv("overview, team, knowledge");
            return brief;
        }

        private static void Connect(BoardDocument document, string edgeTypeId, BoardNodeModel source, BoardNodeModel target)
        {
            if (document == null || source == null || target == null)
            {
                return;
            }

            document.AddEdge(new BoardEdgeModel(edgeTypeId, source.Id, target.Id));
        }
    }
}
