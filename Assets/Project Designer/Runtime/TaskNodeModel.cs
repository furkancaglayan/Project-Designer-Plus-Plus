using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectDesigner.V2.Data
{
    public enum TaskNodeStatus
    {
        NotStarted,
        InProgress,
        Blocked,
        Done
    }

    public enum TaskNodePriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    [Serializable]
    public sealed class TaskNodeModel : BoardNodeModel
    {
        [SerializeField]
        private string _description;
        [SerializeField]
        private TaskNodeStatus _status;
        [SerializeField]
        private TaskNodePriority _priority;
        [SerializeField]
        private int _estimatePoints;
        [SerializeField, FormerlySerializedAs("_assignee")]
        private string _assigneeId;
        [SerializeField]
        private string _dueDateIso;
        [SerializeField]
        private string _acceptanceCriteria;

        public override string Category
        {
            get { return BoardNodeCategories.Planning; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value ?? string.Empty; }
        }

        public TaskNodeStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public TaskNodePriority Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public int EstimatePoints
        {
            get { return _estimatePoints; }
            set { _estimatePoints = Mathf.Max(0, value); }
        }

        public string AssigneeId
        {
            get { return _assigneeId; }
            set { _assigneeId = value ?? string.Empty; }
        }

        public string DueDateIso
        {
            get { return _dueDateIso; }
            set { _dueDateIso = value ?? string.Empty; }
        }

        public string AcceptanceCriteria
        {
            get { return _acceptanceCriteria; }
            set { _acceptanceCriteria = value ?? string.Empty; }
        }

        public TaskNodeModel()
            : base(BoardNodeTypeIds.Task, "New Task", new Vector2(120f, 120f), new Vector2(320f, 220f))
        {
            _description = "Describe the user value and acceptance criteria.";
            _status = TaskNodeStatus.NotStarted;
            _priority = TaskNodePriority.Medium;
            _estimatePoints = 3;
            _assigneeId = string.Empty;
            _dueDateIso = string.Empty;
            _acceptanceCriteria = string.Empty;
        }

        public override BoardNodeModel Clone()
        {
            var clone = new TaskNodeModel
            {
                Description = Description,
                Status = Status,
                Priority = Priority,
                EstimatePoints = EstimatePoints,
                AssigneeId = AssigneeId,
                DueDateIso = DueDateIso,
                AcceptanceCriteria = AcceptanceCriteria
            };
            CopyCommonTo(clone);
            return clone;
        }

        public override string GetSearchText()
        {
            return string.Join(" ", new[]
            {
                base.GetSearchText(),
                Description,
                AssigneeId,
                ProjectDesignerTeamRosterResolver.GetDisplayName(ProjectDesignerTeamRosterContext.CurrentRoster, AssigneeId),
                DueDateIso,
                AcceptanceCriteria,
                Status.ToString(),
                Priority.ToString()
            });
        }
    }
}
