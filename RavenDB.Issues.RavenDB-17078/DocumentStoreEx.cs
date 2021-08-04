using Raven.Client.Documents;

namespace RavenDB.Issues.RavenDB_17078
{
    public static class DocumentStoreEx
    {
        public static string GetCollectionName<T>(this IDocumentStore documentStore)
        {
            return documentStore.Conventions.FindCollectionName(typeof(T));
        }
    }
}