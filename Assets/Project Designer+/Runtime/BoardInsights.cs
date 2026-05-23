using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProjectDesigner.V2.Data
{
    public static class BoardInsights
    {
        public const int WorkMinutesPerHour = 60;
        public const int WorkHoursPerDay = 8;
        public const int WorkDaysPerWeek = 5;
        public const int WorkWeeksPerMonth = 4;
        public const int DueVerySoonWindowDays = 3;
        public const int DueSoonWindowDays = 7;

        private const int HeavyLoadWorkdays = 13;

        private static readonly Regex DurationTokenPattern = new Regex(
            @"(?<value>\d+(?:\.\d+)?)\s*(?<unit>min|m|h|d|w)",
            RegexOptions.IgnoreCase);

        private static readonly string[] SupportedDateFormats =
        {
            "yyyy-MM-dd",
            "yyyy/MM/dd",
            "MM/dd/yyyy",
            "dd/MM/yyyy"
        };

        public static IEnumerable<BoardNodeModel> GetVisibleNodes(BoardDocument document)
        {
            if (document == null)
            {
                return Enumerable.Empty<BoardNodeModel>();
            }

            IEnumerable<BoardNodeModel> nodes = document.Nodes.Where(node => node != null);

            BoardSavedFilter activeFilter = document.SavedFilters
                .FirstOrDefault(filter => filter != null && filter.Id == document.ViewState.ActiveFilterId);

            if (activeFilter != null)
            {
                nodes = nodes.Where(node => activeFilter.Matches(node, document.ViewState.SearchQuery));
            }
            else
            {
                if (document.ViewState.Category != BoardNodeCategories.All)
                {
                    nodes = nodes.Where(node => node.Category == document.ViewState.Category);
                }

                if (!string.IsNullOrWhiteSpace(document.ViewState.SearchQuery))
                {
                    nodes = nodes.Where(node => node.MatchesSearch(document.ViewState.SearchQuery));
                }
            }

            if (!string.IsNullOrWhiteSpace(document.ViewState.QuickFilterId))
            {
                nodes = nodes.Where(node => MatchesQuickFilter(document, node, document.ViewState.QuickFilterId));
            }

            return nodes;
        }

        public static IEnumerable<TaskNodeModel> GetTasks(BoardDocument document)
        {
            return document == null
                ? Enumerable.Empty<TaskNodeModel>()
                : document.Nodes.OfType<TaskNodeModel>();
        }

        public static IEnumerable<TaskNodeModel> GetOpenTasks(BoardDocument document)
        {
            return GetTasks(document).Where(task => task.Status != TaskNodeStatus.Done);
        }

        public static IEnumerable<TaskNodeModel> GetTasksForAssignee(BoardDocument document, string assigneeId)
        {
            if (document == null)
            {
                return Enumerable.Empty<TaskNodeModel>();
            }

            if (string.IsNullOrWhiteSpace(assigneeId))
            {
                return GetTasks(document).Where(task => string.IsNullOrWhiteSpace(task.AssigneeId));
            }

            return document.Nodes
                .OfType<TaskNodeModel>()
                .Where(task => string.Equals(task.AssigneeId, assigneeId, StringComparison.OrdinalIgnoreCase));
        }

        public static int CountTasksByStatus(BoardDocument document, TaskNodeStatus status)
        {
            return GetTasks(document).Count(task => task.Status == status);
        }

        public static bool HasDueDate(TaskNodeModel task, out DateTime dueDate)
        {
            dueDate = default(DateTime);
            return task != null && TryParseDate(task.DueDateIso, out dueDate);
        }

        public static bool HasStartDate(TaskNodeModel task, out DateTime startDate)
        {
            startDate = default(DateTime);
            return task != null && TryParseDate(task.StartDateIso, out startDate);
        }

        public static bool HasTargetDate(MilestoneNodeModel milestone, out DateTime targetDate)
        {
            targetDate = default(DateTime);
            return milestone != null && TryParseDate(milestone.TargetDateIso, out targetDate);
        }

        public static string FormatDateLong(DateTime date)
        {
            return date.Date.ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture);
        }

        public static bool TryFormatDateLong(string dateText, out string formattedDate)
        {
            DateTime date;
            if (TryParseDate(dateText, out date))
            {
                formattedDate = FormatDateLong(date);
                return true;
            }

            formattedDate = string.Empty;
            return false;
        }

        public static bool TryParseDate(string dateText, out DateTime date)
        {
            if (string.IsNullOrWhiteSpace(dateText))
            {
                date = default(DateTime);
                return false;
            }

            if (DateTime.TryParseExact(dateText.Trim(), SupportedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out date))
            {
                date = date.Date;
                return true;
            }

            if (DateTime.TryParse(dateText.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out date))
            {
                date = date.Date;
                return true;
            }

            if (DateTime.TryParse(dateText.Trim(), out date))
            {
                date = date.Date;
                return true;
            }

            return false;
        }

        public static bool TryParseDuration(string durationText, out int totalMinutes)
        {
            totalMinutes = 0;
            if (string.IsNullOrWhiteSpace(durationText))
            {
                return false;
            }

            string text = durationText.Trim();
            if (text.IndexOf('-', StringComparison.Ordinal) >= 0)
            {
                return false;
            }

            int cursor = 0;
            bool matchedAny = false;
            foreach (Match match in DurationTokenPattern.Matches(text))
            {
                if (!IsOnlyWhitespace(text, cursor, match.Index - cursor))
                {
                    totalMinutes = 0;
                    return false;
                }

                if (!double.TryParse(match.Groups["value"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ||
                    value < 0d)
                {
                    totalMinutes = 0;
                    return false;
                }

                int unitMinutes = GetDurationUnitMinutes(match.Groups["unit"].Value);
                if (unitMinutes <= 0)
                {
                    totalMinutes = 0;
                    return false;
                }

                double tokenMinutes = value * unitMinutes;
                if (tokenMinutes > int.MaxValue - totalMinutes)
                {
                    totalMinutes = 0;
                    return false;
                }

                totalMinutes += (int)Math.Round(tokenMinutes, MidpointRounding.AwayFromZero);

                matchedAny = true;
                cursor = match.Index + match.Length;
            }

            if (!matchedAny || !IsOnlyWhitespace(text, cursor, text.Length - cursor))
            {
                totalMinutes = 0;
                return false;
            }

            return totalMinutes > 0;
        }

        public static int GetTaskEstimateMinutes(TaskNodeModel task)
        {
            int totalMinutes;
            return task != null && TryParseDuration(task.EstimateDurationText, out totalMinutes) ? totalMinutes : 0;
        }

        public static string FormatDuration(int totalMinutes)
        {
            if (totalMinutes <= 0)
            {
                return "0min";
            }

            int monthMinutes = WorkWeeksPerMonth * WorkDaysPerWeek * WorkHoursPerDay * WorkMinutesPerHour;
            int weekMinutes = WorkDaysPerWeek * WorkHoursPerDay * WorkMinutesPerHour;
            int dayMinutes = WorkHoursPerDay * WorkMinutesPerHour;
            List<string> parts = new List<string>();

            AppendDurationPart(parts, ref totalMinutes, monthMinutes, "m");
            AppendDurationPart(parts, ref totalMinutes, weekMinutes, "w");
            AppendDurationPart(parts, ref totalMinutes, dayMinutes, "d");
            AppendDurationPart(parts, ref totalMinutes, WorkMinutesPerHour, "h");

            if (totalMinutes > 0)
            {
                parts.Add(string.Format("{0}min", totalMinutes));
            }

            return string.Join(" ", parts);
        }

        public static string DescribeDuration(int totalMinutes)
        {
            if (totalMinutes <= 0)
            {
                return "No estimate set.";
            }

            double hours = totalMinutes / (double)WorkMinutesPerHour;
            double workdays = hours / WorkHoursPerDay;
            return string.Format(CultureInfo.InvariantCulture, "{0} = {1:0.##}h / {2:0.##} workdays", FormatDuration(totalMinutes), hours, workdays);
        }

        public static bool IsTaskOverdue(TaskNodeModel task, DateTime? referenceDate = null)
        {
            DateTime dueDate;
            if (task == null || task.Status == TaskNodeStatus.Done || !HasDueDate(task, out dueDate))
            {
                return false;
            }

            return dueDate < ResolveReferenceDate(referenceDate);
        }

        public static bool IsTaskDueSoon(TaskNodeModel task, DateTime? referenceDate = null, int windowDays = DueSoonWindowDays)
        {
            DateTime dueDate;
            if (task == null || task.Status == TaskNodeStatus.Done || !HasDueDate(task, out dueDate))
            {
                return false;
            }

            DateTime today = ResolveReferenceDate(referenceDate);
            return dueDate >= today && dueDate <= today.AddDays(windowDays);
        }

        public static bool IsTaskDueVerySoon(TaskNodeModel task, DateTime? referenceDate = null)
        {
            return IsTaskDueSoon(task, referenceDate, DueVerySoonWindowDays);
        }

        public static bool HasUnresolvedDependencies(BoardDocument document, TaskNodeModel task)
        {
            if (document == null || task == null)
            {
                return false;
            }

            return document.Edges
                .Where(edge => edge != null && edge.TypeId == BoardEdgeTypeIds.Dependency && edge.SourceNodeId == task.Id)
                .Select(edge => document.GetNode(edge.TargetNodeId) as TaskNodeModel)
                .Any(targetTask => targetTask != null && targetTask.Status != TaskNodeStatus.Done);
        }

        public static bool IsTaskBlocked(BoardDocument document, TaskNodeModel task)
        {
            return task != null && (task.Status == TaskNodeStatus.Blocked || HasUnresolvedDependencies(document, task));
        }

        public static IEnumerable<TaskNodeModel> GetBlockedTasks(BoardDocument document)
        {
            return GetOpenTasks(document).Where(task => IsTaskBlocked(document, task));
        }

        public static IEnumerable<TaskNodeModel> GetOverdueTasks(BoardDocument document, DateTime? referenceDate = null)
        {
            return GetOpenTasks(document).Where(task => IsTaskOverdue(task, referenceDate));
        }

        public static IEnumerable<TaskNodeModel> GetDueVerySoonTasks(BoardDocument document, DateTime? referenceDate = null)
        {
            return GetOpenTasks(document).Where(task => IsTaskDueVerySoon(task, referenceDate));
        }

        public static IEnumerable<TaskNodeModel> GetDueSoonTasks(BoardDocument document, DateTime? referenceDate = null, int windowDays = DueSoonWindowDays)
        {
            return GetOpenTasks(document).Where(task => IsTaskDueSoon(task, referenceDate, windowDays));
        }

        public static IEnumerable<TaskNodeModel> GetUnassignedTasks(BoardDocument document)
        {
            return GetOpenTasks(document).Where(task => string.IsNullOrWhiteSpace(task.AssigneeId));
        }

        public static IEnumerable<TaskNodeModel> GetTasksWithRiskTags(BoardDocument document)
        {
            return GetTasks(document).Where(task => HasRiskTag(task));
        }

        public static IEnumerable<TaskNodeModel> GetTasksBlockingOthers(BoardDocument document)
        {
            if (document == null)
            {
                return Enumerable.Empty<TaskNodeModel>();
            }

            HashSet<string> blockerIds = new HashSet<string>(document.Edges
                .Where(edge => edge != null && edge.TypeId == BoardEdgeTypeIds.Dependency)
                .Where(edge =>
                {
                    TaskNodeModel dependent = document.GetNode(edge.SourceNodeId) as TaskNodeModel;
                    TaskNodeModel blocker = document.GetNode(edge.TargetNodeId) as TaskNodeModel;
                    return dependent != null && blocker != null && dependent.Status != TaskNodeStatus.Done && blocker.Status != TaskNodeStatus.Done;
                })
                .Select(edge => edge.TargetNodeId));

            return blockerIds.Select(id => document.GetNode(id) as TaskNodeModel).Where(task => task != null);
        }

        public static int CountUnresolvedDependencyLinks(BoardDocument document)
        {
            if (document == null)
            {
                return 0;
            }

            return document.Edges.Count(edge =>
            {
                if (edge == null || edge.TypeId != BoardEdgeTypeIds.Dependency)
                {
                    return false;
                }

                TaskNodeModel dependent = document.GetNode(edge.SourceNodeId) as TaskNodeModel;
                TaskNodeModel blocker = document.GetNode(edge.TargetNodeId) as TaskNodeModel;
                return dependent != null && blocker != null && dependent.Status != TaskNodeStatus.Done && blocker.Status != TaskNodeStatus.Done;
            });
        }

        public static IEnumerable<TaskNodeModel> GetLinkedTasks(BoardDocument document, MilestoneNodeModel milestone)
        {
            if (document == null || milestone == null)
            {
                return Enumerable.Empty<TaskNodeModel>();
            }

            return document.Edges
                .Where(edge => edge != null && edge.TypeId == BoardEdgeTypeIds.Milestone && edge.TargetNodeId == milestone.Id)
                .Select(edge => document.GetNode(edge.SourceNodeId) as TaskNodeModel)
                .Where(task => task != null);
        }

        public static float CalculateMilestoneCompletion(BoardDocument document, MilestoneNodeModel milestone)
        {
            List<TaskNodeModel> linkedTasks = GetLinkedTasks(document, milestone).ToList();
            if (linkedTasks.Count == 0)
            {
                return 0f;
            }

            int completed = linkedTasks.Count(task => task.Status == TaskNodeStatus.Done);
            return completed / (float)linkedTasks.Count;
        }

        public static BoardMilestoneHealthReport GetMilestoneHealth(BoardDocument document, MilestoneNodeModel milestone, DateTime? referenceDate = null)
        {
            List<TaskNodeModel> linkedTasks = GetLinkedTasks(document, milestone).ToList();
            var report = new BoardMilestoneHealthReport
            {
                Milestone = milestone,
                Completion = CalculateMilestoneCompletion(document, milestone),
                LinkedTaskCount = linkedTasks.Count,
                BlockedTaskCount = linkedTasks.Count(task => IsTaskBlocked(document, task)),
                OverdueTaskCount = linkedTasks.Count(task => IsTaskOverdue(task, referenceDate)),
                DueSoonTaskCount = linkedTasks.Count(task => IsTaskDueSoon(task, referenceDate))
            };

            if (milestone == null)
            {
                report.State = BoardMilestoneHealthState.NoLinkedTasks;
                return report;
            }

            if (report.LinkedTaskCount == 0)
            {
                report.State = BoardMilestoneHealthState.NoLinkedTasks;
                return report;
            }

            if (report.Completion >= 0.999f)
            {
                report.State = BoardMilestoneHealthState.Complete;
                return report;
            }

            DateTime targetDate;
            DateTime today = ResolveReferenceDate(referenceDate);
            bool hasTargetDate = HasTargetDate(milestone, out targetDate);

            if (hasTargetDate && targetDate < today)
            {
                report.State = BoardMilestoneHealthState.OffTrack;
                return report;
            }

            if (report.BlockedTaskCount > 0 || report.OverdueTaskCount > 0)
            {
                report.State = BoardMilestoneHealthState.AtRisk;
                return report;
            }

            if (hasTargetDate && targetDate <= today.AddDays(7) && report.Completion < 0.75f)
            {
                report.State = BoardMilestoneHealthState.AtRisk;
                return report;
            }

            report.State = BoardMilestoneHealthState.OnTrack;
            return report;
        }

        public static IEnumerable<BoardMilestoneHealthReport> GetMilestoneHealthReports(BoardDocument document, DateTime? referenceDate = null)
        {
            return document == null
                ? Enumerable.Empty<BoardMilestoneHealthReport>()
                : document.Nodes
                    .OfType<MilestoneNodeModel>()
                    .Select(milestone => GetMilestoneHealth(document, milestone, referenceDate));
        }

        public static IEnumerable<BoardMilestoneHealthReport> GetAtRiskMilestones(BoardDocument document, DateTime? referenceDate = null)
        {
            return GetMilestoneHealthReports(document, referenceDate)
                .Where(report => report.State == BoardMilestoneHealthState.AtRisk || report.State == BoardMilestoneHealthState.OffTrack);
        }

        public static IReadOnlyList<BoardAssigneeSummary> GetAssigneeSummaries(BoardDocument document, DateTime? referenceDate = null, ProjectDesignerTeamRosterAsset roster = null)
        {
            if (document == null)
            {
                return Array.Empty<BoardAssigneeSummary>();
            }

            ProjectDesignerTeamRosterAsset resolvedRoster = roster ?? ProjectDesignerTeamRosterContext.CurrentRoster;
            return GetOpenTasks(document)
                .Where(task => !string.IsNullOrWhiteSpace(task.AssigneeId))
                .GroupBy(task => task.AssigneeId.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    List<TaskNodeModel> tasks = group.ToList();
                    string assigneeId = group.Key;
                    return new BoardAssigneeSummary
                    {
                        AssigneeId = assigneeId,
                        DisplayName = ProjectDesignerTeamRosterResolver.GetDisplayName(resolvedRoster, assigneeId),
                        AccentColor = ProjectDesignerTeamRosterResolver.GetAccentColor(resolvedRoster, assigneeId),
                        OpenTaskCount = tasks.Count,
                        TotalEstimateMinutes = tasks.Sum(GetTaskEstimateMinutes),
                        BlockedTaskCount = tasks.Count(task => IsTaskBlocked(document, task)),
                        OverdueTaskCount = tasks.Count(task => IsTaskOverdue(task, referenceDate)),
                        HasOverload = tasks.Count >= 5 || tasks.Sum(GetTaskEstimateMinutes) >= HeavyLoadWorkdays * WorkHoursPerDay * WorkMinutesPerHour
                    };
                })
                .OrderByDescending(summary => summary.OpenTaskCount)
                .ThenByDescending(summary => summary.TotalEstimateMinutes)
                .ThenBy(summary => summary.DisplayName)
                .ToList();
        }

        public static int CountUnassignedOpenTasks(BoardDocument document)
        {
            return GetUnassignedTasks(document).Count();
        }

        public static int CountAtRiskNodes(BoardDocument document, DateTime? referenceDate = null)
        {
            return GetAtRiskNodes(document, referenceDate).Count();
        }

        public static IEnumerable<BoardNodeModel> GetAtRiskNodes(BoardDocument document, DateTime? referenceDate = null)
        {
            if (document == null)
            {
                return Enumerable.Empty<BoardNodeModel>();
            }

            HashSet<string> addedIds = new HashSet<string>();
            List<BoardNodeModel> results = new List<BoardNodeModel>();

            foreach (BoardNodeModel node in document.Nodes.Where(node => node != null && HasRiskTag(node)))
            {
                if (addedIds.Add(node.Id))
                {
                    results.Add(node);
                }
            }

            foreach (BoardMilestoneHealthReport report in GetAtRiskMilestones(document, referenceDate))
            {
                if (report.Milestone != null && addedIds.Add(report.Milestone.Id))
                {
                    results.Add(report.Milestone);
                }
            }

            return results;
        }

        public static bool HasRiskTag(BoardNodeModel node)
        {
            if (node == null || node.Tags == null)
            {
                return false;
            }

            return node.Tags.Any(tag =>
                !string.IsNullOrWhiteSpace(tag) &&
                (tag.IndexOf("risk", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 tag.IndexOf("blocker", StringComparison.OrdinalIgnoreCase) >= 0));
        }

        public static bool MatchesQuickFilter(BoardDocument document, BoardNodeModel node, string quickFilterId, DateTime? referenceDate = null)
        {
            if (document == null || node == null || string.IsNullOrWhiteSpace(quickFilterId))
            {
                return true;
            }

            TaskNodeModel task = node as TaskNodeModel;
            MilestoneNodeModel milestone = node as MilestoneNodeModel;

            switch (quickFilterId)
            {
                case BoardQuickFilterIds.Tasks:
                    return task != null;

                case BoardQuickFilterIds.InProgress:
                    return task != null && task.Status == TaskNodeStatus.InProgress;

                case BoardQuickFilterIds.Blocked:
                    return task != null && IsTaskBlocked(document, task);

                case BoardQuickFilterIds.Overdue:
                    return task != null && IsTaskOverdue(task, referenceDate);

                case BoardQuickFilterIds.DueSoon:
                    return task != null && IsTaskDueSoon(task, referenceDate);

                case BoardQuickFilterIds.Unassigned:
                    return task != null && string.IsNullOrWhiteSpace(task.AssigneeId) && task.Status != TaskNodeStatus.Done;

                case BoardQuickFilterIds.AtRisk:
                    if (HasRiskTag(node))
                    {
                        return true;
                    }

                    if (milestone == null)
                    {
                        return false;
                    }

                    BoardMilestoneHealthState milestoneState = GetMilestoneHealth(document, milestone, referenceDate).State;
                    return milestoneState != BoardMilestoneHealthState.OnTrack &&
                           milestoneState != BoardMilestoneHealthState.Complete &&
                           milestoneState != BoardMilestoneHealthState.NoLinkedTasks;

                case BoardQuickFilterIds.Milestones:
                    return milestone != null;
            }

            if (BoardQuickFilterIds.IsAssigneeFilter(quickFilterId))
            {
                string assigneeId = BoardQuickFilterIds.GetAssigneeId(quickFilterId);
                if (string.Equals(assigneeId, "unassigned", StringComparison.OrdinalIgnoreCase))
                {
                    return task != null && string.IsNullOrWhiteSpace(task.AssigneeId) && task.Status != TaskNodeStatus.Done;
                }

                return task != null && string.Equals(task.AssigneeId, assigneeId, StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        private static int GetDurationUnitMinutes(string unit)
        {
            switch ((unit ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "min":
                    return 1;
                case "h":
                    return WorkMinutesPerHour;
                case "d":
                    return WorkHoursPerDay * WorkMinutesPerHour;
                case "w":
                    return WorkDaysPerWeek * WorkHoursPerDay * WorkMinutesPerHour;
                case "m":
                    return WorkWeeksPerMonth * WorkDaysPerWeek * WorkHoursPerDay * WorkMinutesPerHour;
                default:
                    return 0;
            }
        }

        private static bool IsOnlyWhitespace(string text, int startIndex, int length)
        {
            if (length < 0)
            {
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                if (!char.IsWhiteSpace(text[startIndex + i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static void AppendDurationPart(List<string> parts, ref int totalMinutes, int unitMinutes, string suffix)
        {
            int count = totalMinutes / unitMinutes;
            if (count <= 0)
            {
                return;
            }

            parts.Add(string.Format("{0}{1}", count, suffix));
            totalMinutes -= count * unitMinutes;
        }

        private static DateTime ResolveReferenceDate(DateTime? referenceDate)
        {
            return (referenceDate ?? DateTime.Today).Date;
        }
    }
}
