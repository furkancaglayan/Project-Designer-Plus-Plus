namespace ProjectDesigner.V2.Data
{
    public static class BoardQuickFilterIds
    {
        public const string None = "";
        public const string Tasks = "tasks";
        public const string InProgress = "status:in-progress";
        public const string Blocked = "status:blocked";
        public const string Overdue = "due:overdue";
        public const string DueSoon = "due:soon";
        public const string Unassigned = "assignee:unassigned";
        public const string AtRisk = "risk:at-risk";
        public const string Milestones = "milestones";
        private const string AssigneePrefix = "assignee:";

        public static string ForAssigneeId(string assigneeId)
        {
            return AssigneePrefix + (assigneeId ?? string.Empty).Trim();
        }

        public static bool IsAssigneeFilter(string filterId)
        {
            return !string.IsNullOrEmpty(filterId) && filterId.StartsWith(AssigneePrefix);
        }

        public static string GetAssigneeId(string filterId)
        {
            return IsAssigneeFilter(filterId) ? filterId.Substring(AssigneePrefix.Length) : string.Empty;
        }
    }
}
