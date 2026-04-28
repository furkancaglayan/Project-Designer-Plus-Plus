namespace ProjectDesigner.V2.Data
{
    public sealed class BoardAssigneeSummary
    {
        public string Assignee { get; set; }
        public int OpenTaskCount { get; set; }
        public int TotalEstimatePoints { get; set; }
        public int BlockedTaskCount { get; set; }
        public int OverdueTaskCount { get; set; }
        public bool HasOverload { get; set; }
    }
}
