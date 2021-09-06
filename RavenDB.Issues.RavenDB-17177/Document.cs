namespace RavenDB.Issues.RavenDB_17177
{
    public sealed class Issue
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Status Status { get; set; }
    }

    public enum Status
    {
        Open,
        Closed
    }
}