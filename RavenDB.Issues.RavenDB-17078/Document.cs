namespace RavenDB.Issues.RavenDB_17078
{
    public sealed class Document
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
