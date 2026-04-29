using System;
using ProjectDesigner.V2.Data;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class ProjectDesignerWelcomeView : VisualElement
    {
        public ProjectDesignerWelcomeView(
            Action<string, string> createPreset,
            Action openProjectFinder,
            Action openQuickStart,
            Action openProjectSettings)
        {
            AddToClassList("pd-welcome");

            var card = new VisualElement();
            card.AddToClassList("pd-welcome-card");
            Add(card);

            var eyebrow = new Label("Project Designer+");
            eyebrow.AddToClassList("pd-welcome-eyebrow");
            card.Add(eyebrow);

            var title = new Label("Learn the planner and start from the right board");
            title.AddToClassList("pd-welcome-title");
            card.Add(title);

            var body = new Label("Project Designer+ is split into three simple surfaces: onboarding for learning and templates, Project Finder for current boards, and the planner for actual editing.");
            body.AddToClassList("pd-welcome-body");
            card.Add(body);

            card.Add(CreateInfoPanel("1. Start Here", "Use onboarding to understand the workflow, learn the main interactions, and create a board from a template when you are starting fresh."));
            card.Add(CreateInfoPanel("2. Use Project Finder For Current Boards", "Project Finder is for reopening work that already exists in the project, pinning boards you revisit, and searching by board name, summary, or team snapshot."));
            card.Add(CreateInfoPanel("3. Work Inside The Planner", "Right-click for board actions, drag cards to arrange them, drag from Link to create relationships, and use Ctrl+D, Ctrl+A, Delete, and F for the most common edits."));

            var templateTitle = new Label("Choose A Starting Template");
            templateTitle.AddToClassList("pd-section-title");
            card.Add(templateTitle);
            card.Add(CreateMutedBodyLabel("Use an empty board when you want a clean start. Use workflow presets and showcase boards when you want stronger structure or a denser example to inspect."));

            card.Add(CreatePresetButton("New Empty Board", "Start light with a sticky project brief card and build your own structure from there.", BoardPresetIds.Empty, ProjectDesignerProductInfo.DefaultBoardName, createPreset, true));
            card.Add(CreatePresetButton("Project Designer+ Redo Board", "Flagship showcase board for this package rewrite, including planner architecture, rollout work, docs, and launch prep.", BoardPresetIds.ProjectDesignerRedo, ProjectDesignerProductInfo.ProjectDesignerRedoBoardName, createPreset, false));
            card.Add(CreatePresetButton("Solo Indie Board", "A fuller showcase board with pitch framing, slice planning, references, risks, and milestone links.", BoardPresetIds.SoloIndie, ProjectDesignerProductInfo.SoloBoardName, createPreset, false));
            card.Add(CreatePresetButton("Small Team Board", "A denser collaborative example covering design, production, art, engineering, and stakeholder review prep.", BoardPresetIds.SmallTeam, ProjectDesignerProductInfo.SmallTeamBoardName, createPreset, false));
            card.Add(CreatePresetButton("Pitch & Vision Board", "Workflow preset for player promise, target audience, and visual direction framing.", BoardPresetIds.PitchVision, ProjectDesignerProductInfo.PitchVisionBoardName, createPreset, false));
            card.Add(CreatePresetButton("Milestone Roadmap Board", "Workflow preset for shaping the path from pre-production goals to milestone checkpoints.", BoardPresetIds.MilestoneRoadmap, ProjectDesignerProductInfo.MilestoneRoadmapBoardName, createPreset, false));
            card.Add(CreatePresetButton("Research & Reference Board", "Workflow preset for references, open questions, and research capture.", BoardPresetIds.ResearchReference, ProjectDesignerProductInfo.ResearchReferenceBoardName, createPreset, false));
            card.Add(CreatePresetButton("Stakeholder Review Board", "Workflow preset for review prep, talking points, risks, and next-step alignment.", BoardPresetIds.StakeholderReview, ProjectDesignerProductInfo.StakeholderReviewBoardName, createPreset, false));

            var technicalTitle = new Label("Technical Design (Optional)");
            technicalTitle.AddToClassList("pd-section-title");
            card.Add(technicalTitle);
            card.Add(CreatePresetButton("Technical Design Board", "A richer technical map with classes, notes, references, and architecture relationships when planning needs technical depth.", BoardPresetIds.TechnicalDesign, ProjectDesignerProductInfo.TechnicalBoardName, createPreset, false));

            var openButton = new Button(openProjectFinder) { text = "Open Project Finder" };
            openButton.AddToClassList("pd-secondary-button");
            card.Add(openButton);

            var actionRow = new VisualElement();
            actionRow.AddToClassList("pd-action-row");
            card.Add(actionRow);

            var quickStartButton = new Button(openQuickStart) { text = "Quick Start" };
            quickStartButton.AddToClassList("pd-secondary-button");
            actionRow.Add(quickStartButton);

            var settingsButton = new Button(openProjectSettings) { text = "Project Settings" };
            settingsButton.AddToClassList("pd-secondary-button");
            actionRow.Add(settingsButton);

        }

        private static VisualElement CreatePresetButton(string title, string description, string presetId, string boardName, Action<string, string> createPreset, bool primary)
        {
            var row = new VisualElement();
            row.AddToClassList("pd-welcome-preset");

            var label = new Label(title);
            label.AddToClassList("pd-welcome-preset-title");
            row.Add(label);

            var descriptionLabel = new Label(description);
            descriptionLabel.AddToClassList("pd-welcome-preset-description");
            row.Add(descriptionLabel);

            var button = new Button(() => createPreset(presetId, boardName)) { text = title };
            button.AddToClassList(primary ? "pd-primary-button" : "pd-secondary-button");
            row.Add(button);
            return row;
        }

        private static VisualElement CreateInfoPanel(string title, string description)
        {
            var row = new VisualElement();
            row.AddToClassList("pd-welcome-preset");

            var label = new Label(title);
            label.AddToClassList("pd-welcome-preset-title");
            row.Add(label);

            var descriptionLabel = new Label(description);
            descriptionLabel.AddToClassList("pd-welcome-preset-description");
            row.Add(descriptionLabel);
            return row;
        }

        private static Label CreateMutedBodyLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("pd-muted-body");
            return label;
        }
    }
}
