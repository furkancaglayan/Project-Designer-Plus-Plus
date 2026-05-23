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
        private string _estimateDurationText;
        [SerializeField, FormerlySerializedAs("_estimatePoints")]
        private int _legacyEstimateDays;
        [SerializeField, FormerlySerializedAs("_assignee")]
        private string _assigneeId;
        [SerializeField]
        private string _startDateIso;
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

        public string EstimateDurationText
        {
            get
            {
                MigrateLegacyEstimate();
                return _estimateDurationText;
            }
            set { _estimateDurationText = value ?? string.Empty; }
        }

        public string AssigneeId
        {
            get { return _assigneeId; }
            set { _assigneeId = value ?? string.Empty; }
        }

        public string StartDateIso
        {
            get { return _startDateIso; }
            set { _startDateIso = value ?? string.Empty; }
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
            _estimateDurationText = string.Empty;
            _legacyEstimateDays = 0;
            _assigneeId = string.Empty;
            _startDateIso = string.Empty;
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
                EstimateDurationText = EstimateDurationText,
                AssigneeId = AssigneeId,
                StartDateIso = StartDateIso,
                DueDateIso = DueDateIso,
                AcceptanceCriteria = AcceptanceCriteria
            };
            CopyCommonTo(clone);
            return clone;
        }

        public override void EnsureDefaults()
        {
            base.EnsureDefaults();
            _description = _description ?? string.Empty;
            _assigneeId = _assigneeId ?? string.Empty;
            _startDateIso = _startDateIso ?? string.Empty;
            _dueDateIso = _dueDateIso ?? string.Empty;
            _acceptanceCriteria = _acceptanceCriteria ?? string.Empty;
            MigrateLegacyEstimate();
        }

        public override string GetSearchText()
        {
            return string.Join(" ", new[]
            {
                base.GetSearchText(),
                Description,
                EstimateDurationText,
                AssigneeId,
                ProjectDesignerTeamRosterResolver.GetDisplayName(ProjectDesignerTeamRosterContext.CurrentRoster, AssigneeId),
                StartDateIso,
                DueDateIso,
                AcceptanceCriteria,
                Status.ToString(),
                Priority.ToString()
            });
        }

        private void MigrateLegacyEstimate()
        {
            if (string.IsNullOrWhiteSpace(_estimateDurationText) && _legacyEstimateDays > 0)
            {
                _estimateDurationText = string.Format("{0}d", Mathf.Max(0, _legacyEstimateDays));
                _legacyEstimateDays = 0;
            }

            _estimateDurationText = _estimateDurationText ?? string.Empty;
        }
    }
}
