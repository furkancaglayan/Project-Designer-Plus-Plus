using System;
using ProjectDesigner.V2.Data;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class ProjectDesignerWelcomeView : VisualElement
    {
        public ProjectDesignerWelcomeView(
            Action<string, string> createPreset,
            Action openSelectedBoard,
            Action openQuickStart,
            Action openProjectSettings,
            Action openBoardBrowser)
        {
            AddToClassList("pd-welcome");

            var card = new VisualElement();
            card.AddToClassList("pd-welcome-card");
            Add(card);

            var eyebrow = new Label("Project Designer+");
            eyebrow.AddToClassList("pd-welcome-eyebrow");
            card.Add(eyebrow);

            var title = new Label("Node-based pre-production planning for indie Unity teams");
            title.AddToClassList("pd-welcome-title");
            card.Add(title);

            var body = new Label("Use the planner to arrange planning boards, capture references from Unity assets, map milestones and tasks, and keep technical design as a secondary workflow instead of the main event.");
            body.AddToClassList("pd-welcome-body");
            card.Add(body);

            card.Add(CreatePresetButton("New Empty Board", "Start light with a sticky project brief card and build your own structure from there.", BoardPresetIds.Empty, ProjectDesignerProductInfo.DefaultBoardName, createPreset));
            card.Add(CreatePresetButton("Project Designer+ Redo Board", "Flagship showcase board for this package rewrite, including planner architecture, rollout work, docs, and launch prep.", BoardPresetIds.ProjectDesignerRedo, ProjectDesignerProductInfo.ProjectDesignerRedoBoardName, createPreset));
            card.Add(CreatePresetButton("Solo Indie Board", "A fuller showcase board with pitch framing, slice planning, references, risks, and milestone links.", BoardPresetIds.SoloIndie, ProjectDesignerProductInfo.SoloBoardName, createPreset));
            card.Add(CreatePresetButton("Small Team Board", "A denser collaborative example covering design, production, art, engineering, and stakeholder review prep.", BoardPresetIds.SmallTeam, ProjectDesignerProductInfo.SmallTeamBoardName, createPreset));
            card.Add(CreatePresetButton("Technical Design Board", "A richer technical map with multiple classes, notes, references, and explicit architecture relationships.", BoardPresetIds.TechnicalDesign, ProjectDesignerProductInfo.TechnicalBoardName, createPreset));

            var openButton = new Button(openSelectedBoard) { text = "Open Selected Planning Board" };
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

            var browserButton = new Button(openBoardBrowser) { text = "Browse Boards" };
            browserButton.AddToClassList("pd-secondary-button");
            actionRow.Add(browserButton);
        }

        private static VisualElement CreatePresetButton(string title, string description, string presetId, string boardName, Action<string, string> createPreset)
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
            button.AddToClassList("pd-primary-button");
            row.Add(button);
            return row;
        }
    }
}
