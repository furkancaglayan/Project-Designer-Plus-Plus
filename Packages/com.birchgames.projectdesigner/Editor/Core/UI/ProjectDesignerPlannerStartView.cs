using System;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class ProjectDesignerPlannerStartView : VisualElement
    {
        public ProjectDesignerPlannerStartView(
            Action openProjectFinder,
            Action openOnboarding,
            Action openQuickStart,
            Action openProjectSettings,
            Action openSelectedBoard)
        {
            AddToClassList("pd-welcome");

            var card = new VisualElement();
            card.AddToClassList("pd-welcome-card");
            Add(card);

            var eyebrow = new Label("Planner");
            eyebrow.AddToClassList("pd-welcome-eyebrow");
            card.Add(eyebrow);

            var title = new Label("Open a planning board");
            title.AddToClassList("pd-welcome-title");
            card.Add(title);

            var body = new Label("Use Project Finder to reopen current boards, or use Onboarding when you want help choosing a template and learning how the planner works.");
            body.AddToClassList("pd-welcome-body");
            card.Add(body);

            card.Add(CreatePanel("Project Finder", "Browse current planning boards, search by name, reopen recent work, and pin the boards you revisit most."));
            card.Add(CreatePanel("Onboarding", "Choose a starter template, understand the planner surfaces, and learn the core board interactions before you begin editing."));
            card.Add(CreatePanel("Planner", "Edit one planning board at a time with cards, links, saved views, overview metrics, and details on demand."));

            var primaryRow = new VisualElement();
            primaryRow.AddToClassList("pd-action-row");
            card.Add(primaryRow);

            var finderButton = new Button(openProjectFinder) { text = "Open Project Finder" };
            finderButton.AddToClassList("pd-primary-button");
            primaryRow.Add(finderButton);

            var onboardingButton = new Button(openOnboarding) { text = "Open Onboarding" };
            onboardingButton.AddToClassList("pd-secondary-button");
            primaryRow.Add(onboardingButton);

            var selectedBoardButton = new Button(openSelectedBoard) { text = "Open Selected Board" };
            selectedBoardButton.AddToClassList("pd-secondary-button");
            primaryRow.Add(selectedBoardButton);

            var supportRow = new VisualElement();
            supportRow.AddToClassList("pd-action-row");
            card.Add(supportRow);

            var quickStartButton = new Button(openQuickStart) { text = "Quick Start" };
            quickStartButton.AddToClassList("pd-secondary-button");
            supportRow.Add(quickStartButton);

            var settingsButton = new Button(openProjectSettings) { text = "Project Settings" };
            settingsButton.AddToClassList("pd-secondary-button");
            supportRow.Add(settingsButton);
        }

        private static VisualElement CreatePanel(string title, string body)
        {
            var panel = new VisualElement();
            panel.AddToClassList("pd-welcome-preset");

            var titleLabel = new Label(title);
            titleLabel.AddToClassList("pd-welcome-preset-title");
            panel.Add(titleLabel);

            var bodyLabel = new Label(body);
            bodyLabel.AddToClassList("pd-welcome-preset-description");
            panel.Add(bodyLabel);
            return panel;
        }
    }
}
