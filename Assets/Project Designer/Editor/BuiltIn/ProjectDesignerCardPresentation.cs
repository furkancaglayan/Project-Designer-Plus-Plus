using System;
using System.Collections.Generic;
using System.IO;
using ProjectDesigner.V2.Data;
using UnityEngine;

namespace ProjectDesigner.V2.BuiltIn
{
    internal readonly struct ProjectDesignerCardSignal
    {
        public string Text { get; }
        public Color Color { get; }

        public ProjectDesignerCardSignal(string text, Color color)
        {
            Text = string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim();
            Color = color;
        }
    }

    internal readonly struct ProjectDesignerCardTagSummary
    {
        public IReadOnlyList<string> VisibleTags { get; }
        public int HiddenCount { get; }

        public ProjectDesignerCardTagSummary(IReadOnlyList<string> visibleTags, int hiddenCount)
        {
            VisibleTags = visibleTags ?? Array.Empty<string>();
            HiddenCount = Mathf.Max(0, hiddenCount);
        }
    }

    internal static class ProjectDesignerCardPresentation
    {
        internal const int MaxVisibleTags = 2;

        public static string GetPreviewText(BoardNodeModel node, IProjectDesignerNodeDefinition definition, BoardDocument document)
        {
            string preview = definition == null ? string.Empty : definition.GetPreview(node, document);
            return Truncate(NormalizeWhitespace(preview), GetPreviewCharacterLimit(node));
        }

        public static IReadOnlyList<ProjectDesignerCardSignal> GetSignals(BoardNodeModel node, BoardDocument document)
        {
            if (node is TaskNodeModel task)
            {
                return GetTaskSignals(task, document, ProjectDesignerTeamRosterContext.CurrentRoster);
            }

            if (node is MilestoneNodeModel milestone)
            {
                return GetMilestoneSignals(milestone, document);
            }

            if (node is ReferenceNodeModel reference)
            {
                return GetReferenceSignals(reference);
            }

            return Array.Empty<ProjectDesignerCardSignal>();
        }

        public static string GetSecondaryMetaText(BoardNodeModel node)
        {
            if (node is TaskNodeModel task)
            {
                return GetTaskSecondaryMetaText(task, false);
            }

            return string.Empty;
        }

        public static string GetSecondaryMetaRichText(BoardNodeModel node)
        {
            if (node is TaskNodeModel task)
            {
                return GetTaskSecondaryMetaText(task, true);
            }

            return GetSecondaryMetaText(node);
        }

        public static IReadOnlyList<ProjectDesignerCardSignal> GetTaskSignals(TaskNodeModel task, BoardDocument document, ProjectDesignerTeamRosterAsset roster)
        {
            if (task == null)
            {
                return Array.Empty<ProjectDesignerCardSignal>();
            }

            var signals = new List<ProjectDesignerCardSignal>
            {
                new ProjectDesignerCardSignal(GetTaskStatusLabel(task.Status), GetTaskStatusColor(task.Status))
            };

            if (string.IsNullOrWhiteSpace(task.AssigneeId))
            {
                signals.Add(new ProjectDesignerCardSignal("Unassigned", new Color(0.53f, 0.59f, 0.66f)));
            }
            else
            {
                ProjectDesignerTeamMemberData member = ProjectDesignerTeamRosterResolver.ResolveMember(roster, task.AssigneeId);
                if (member != null)
                {
                    Color assigneeColor = ParseColorOrFallback(member.AccentColor, new Color(0.2f, 0.52f, 0.88f));
                    signals.Add(new ProjectDesignerCardSignal(member.DisplayName, assigneeColor));
                }
                else
                {
                    signals.Add(new ProjectDesignerCardSignal("Missing Assignee", new Color(0.9f, 0.58f, 0.24f)));
                }
            }

            if (BoardInsights.IsTaskBlocked(document, task))
            {
                signals.Add(new ProjectDesignerCardSignal("Blocked", new Color(0.88f, 0.37f, 0.26f)));
            }
            else if (BoardInsights.IsTaskOverdue(task))
            {
                signals.Add(new ProjectDesignerCardSignal("Overdue", new Color(0.84f, 0.25f, 0.27f)));
            }
            else if (BoardInsights.IsTaskDueSoon(task))
            {
                signals.Add(new ProjectDesignerCardSignal("Due Soon", new Color(0.92f, 0.67f, 0.22f)));
            }

            return signals;
        }

        public static ProjectDesignerCardTagSummary GetTagSummary(BoardNodeModel node)
        {
            if (node == null || node.Tags == null || node.Tags.Count == 0)
            {
                return new ProjectDesignerCardTagSummary(Array.Empty<string>(), 0);
            }

            var tags = new List<string>();
            foreach (string tag in node.Tags)
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    continue;
                }

                string trimmed = tag.Trim();
                bool exists = false;
                foreach (string existing in tags)
                {
                    if (string.Equals(existing, trimmed, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    tags.Add(trimmed);
                }
            }

            int hiddenCount = Mathf.Max(0, tags.Count - MaxVisibleTags);
            return new ProjectDesignerCardTagSummary(tags.GetRange(0, Mathf.Min(MaxVisibleTags, tags.Count)), hiddenCount);
        }

        public static string GetProjectBriefPreview(ProjectBriefNodeModel brief)
        {
            if (brief == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(brief.Overview))
            {
                return brief.Overview.Trim();
            }

            if (!string.IsNullOrWhiteSpace(brief.ProjectKnowledge))
            {
                return brief.ProjectKnowledge.Trim();
            }

            return brief.TeamSnapshot;
        }

        public static string GetMilestonePreview(MilestoneNodeModel milestone, BoardDocument document)
        {
            if (milestone == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(milestone.Summary))
            {
                return milestone.Summary.Trim();
            }

            BoardMilestoneHealthReport health = BoardInsights.GetMilestoneHealth(document, milestone);
            return GetMilestoneHealthLabel(health.State) + " | " + Mathf.RoundToInt(health.Completion * 100f) + "% complete";
        }

        public static string GetReferencePreview(ReferenceNodeModel reference)
        {
            if (reference == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(reference.AssetPath))
            {
                return "Unity asset: " + Path.GetFileName(reference.AssetPath);
            }

            if (!string.IsNullOrWhiteSpace(reference.ExternalUrl))
            {
                if (Uri.TryCreate(reference.ExternalUrl, UriKind.Absolute, out Uri uri) && !string.IsNullOrWhiteSpace(uri.Host))
                {
                    string host = uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
                        ? uri.Host.Substring(4)
                        : uri.Host;
                    return "External link: " + host;
                }

                return reference.ExternalUrl.Trim();
            }

            if (!string.IsNullOrWhiteSpace(reference.TextReference))
            {
                return reference.TextReference.Trim();
            }

            if (!string.IsNullOrWhiteSpace(reference.Summary))
            {
                return reference.Summary.Trim();
            }

            return string.Empty;
        }

        public static string GetClassPreview(ClassNodeModel classNode)
        {
            if (classNode == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(classNode.Summary))
            {
                return classNode.Summary.Trim();
            }

            string namespaceName = string.IsNullOrWhiteSpace(classNode.NamespaceName)
                ? "No namespace"
                : classNode.NamespaceName.Trim() + " namespace";
            return namespaceName + " | " + classNode.Fields.Count + " fields | " + classNode.Methods.Count + " methods";
        }

        public static string FormatTagOverflowLabel(int hiddenCount)
        {
            return hiddenCount <= 0 ? string.Empty : "+" + hiddenCount;
        }

        public static string GetTaskSecondaryMetaText(TaskNodeModel task)
        {
            return GetTaskSecondaryMetaText(task, false);
        }

        private static string GetTaskSecondaryMetaText(TaskNodeModel task, bool richText)
        {
            if (task == null)
            {
                return string.Empty;
            }

            var parts = new List<string>();
            string priorityLabel = GetVisibleTaskPriorityLabel(task.Priority, richText);
            if (!string.IsNullOrWhiteSpace(priorityLabel))
            {
                parts.Add(priorityLabel);
            }

            if (!string.IsNullOrWhiteSpace(task.DueDateIso))
            {
                parts.Add("Due " + task.DueDateIso.Trim());
            }

            if (task.EstimatePoints > 0)
            {
                parts.Add(task.EstimatePoints + " pts");
            }

            return string.Join(" | ", parts);
        }

        private static IReadOnlyList<ProjectDesignerCardSignal> GetMilestoneSignals(MilestoneNodeModel milestone, BoardDocument document)
        {
            var signals = new List<ProjectDesignerCardSignal>();
            BoardMilestoneHealthReport health = BoardInsights.GetMilestoneHealth(document, milestone);
            signals.Add(new ProjectDesignerCardSignal(GetMilestoneHealthLabel(health.State), GetMilestoneHealthColor(health.State)));

            if (!string.IsNullOrWhiteSpace(milestone.TargetDateIso))
            {
                signals.Add(new ProjectDesignerCardSignal(milestone.TargetDateIso, new Color(0.32f, 0.45f, 0.82f)));
            }

            return signals;
        }

        private static IReadOnlyList<ProjectDesignerCardSignal> GetReferenceSignals(ReferenceNodeModel reference)
        {
            if (reference == null)
            {
                return Array.Empty<ProjectDesignerCardSignal>();
            }

            if (!string.IsNullOrWhiteSpace(reference.AssetPath))
            {
                return new[]
                {
                    new ProjectDesignerCardSignal("Unity Asset", new Color(0.24f, 0.55f, 0.88f))
                };
            }

            if (!string.IsNullOrWhiteSpace(reference.ExternalUrl))
            {
                return new[]
                {
                    new ProjectDesignerCardSignal("External Link", new Color(0.28f, 0.66f, 0.4f))
                };
            }

            if (!string.IsNullOrWhiteSpace(reference.TextReference))
            {
                return new[]
                {
                    new ProjectDesignerCardSignal("Excerpt", new Color(0.53f, 0.59f, 0.66f))
                };
            }

            return Array.Empty<ProjectDesignerCardSignal>();
        }

        private static int GetPreviewCharacterLimit(BoardNodeModel node)
        {
            if (node is ProjectBriefNodeModel)
            {
                return 128;
            }

            if (node is ClassNodeModel)
            {
                return 100;
            }

            if (node is ReferenceNodeModel)
            {
                return 110;
            }

            return 120;
        }

        private static string NormalizeWhitespace(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return string.Join(" ", value.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength || maxLength < 4)
            {
                return value ?? string.Empty;
            }

            return value.Substring(0, maxLength - 3).TrimEnd() + "...";
        }

        private static Color ParseColorOrFallback(string htmlColor, Color fallback)
        {
            if (!string.IsNullOrWhiteSpace(htmlColor) && ColorUtility.TryParseHtmlString(htmlColor, out Color parsed))
            {
                return parsed;
            }

            return fallback;
        }

        private static string GetTaskStatusLabel(TaskNodeStatus status)
        {
            switch (status)
            {
                case TaskNodeStatus.NotStarted:
                    return "Not Started";
                case TaskNodeStatus.InProgress:
                    return "In Progress";
                default:
                    return status.ToString();
            }
        }

        private static string GetMilestoneHealthLabel(BoardMilestoneHealthState state)
        {
            switch (state)
            {
                case BoardMilestoneHealthState.Complete:
                    return "Complete";
                case BoardMilestoneHealthState.OffTrack:
                    return "Off Track";
                case BoardMilestoneHealthState.AtRisk:
                    return "At Risk";
                case BoardMilestoneHealthState.NoLinkedTasks:
                    return "Needs Tasks";
                default:
                    return "On Track";
            }
        }

        private static Color GetTaskStatusColor(TaskNodeStatus status)
        {
            switch (status)
            {
                case TaskNodeStatus.InProgress:
                    return new Color(0.24f, 0.55f, 0.88f);
                case TaskNodeStatus.Blocked:
                    return new Color(0.88f, 0.37f, 0.26f);
                case TaskNodeStatus.Done:
                    return new Color(0.28f, 0.66f, 0.4f);
                default:
                    return new Color(0.53f, 0.59f, 0.66f);
            }
        }

        private static Color GetMilestoneHealthColor(BoardMilestoneHealthState state)
        {
            switch (state)
            {
                case BoardMilestoneHealthState.Complete:
                    return new Color(0.28f, 0.66f, 0.4f);
                case BoardMilestoneHealthState.OffTrack:
                    return new Color(0.83f, 0.25f, 0.3f);
                case BoardMilestoneHealthState.AtRisk:
                    return new Color(0.95f, 0.58f, 0.22f);
                case BoardMilestoneHealthState.NoLinkedTasks:
                    return new Color(0.53f, 0.59f, 0.66f);
                default:
                    return new Color(0.24f, 0.55f, 0.88f);
            }
        }

        private static string GetVisibleTaskPriorityLabel(TaskNodePriority priority, bool richText)
        {
            switch (priority)
            {
                case TaskNodePriority.Critical:
                    return richText ? "<color=#E0565B>Critical</color>" : "Critical";
                case TaskNodePriority.High:
                    return richText ? "<color=#F0A84A>High</color>" : "High";
                default:
                    return string.Empty;
            }
        }
    }
}
