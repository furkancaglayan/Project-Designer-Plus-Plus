using System;
using UnityEngine;

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
        [SerializeField]
        private string _assignee;
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

        public string Assignee
        {
            get { return _assignee; }
            set { _assignee = value ?? string.Empty; }
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
            _assignee = string.Empty;
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
                Assignee = Assignee,
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
                Assignee,
                DueDateIso,
                AcceptanceCriteria,
                Status.ToString(),
                Priority.ToString()
            });
        }
    }
}
