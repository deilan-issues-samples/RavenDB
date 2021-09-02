namespace RavenDB.Issues.RavenDB_17167
{
    public sealed class DocumentX
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Status Status { get; set; }
        public string DocumentYId { get; set; }
    }

    public sealed class DocumentY
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Status Status { get; set; }
        public string DocumentXId { get; set; }
    }

    public enum Status
    {
        Open,
        Closed
    }
}