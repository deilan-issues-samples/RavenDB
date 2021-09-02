using System;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Operations;
using Raven.Client.Http;
using Raven.Embedded;

namespace RavenDB.Issues.RavenDB_17165
{
    public static class Program
    {
        private const int DocumentsCount = 1024;

        public static async Task Main()
        {
            var documentStore = GetDocumentStore();
            await SeedDatabaseAsync(documentStore);

            EmbeddedServer.Instance.OpenStudioInBrowser();

            Console.ReadKey();

            await QueryDocumentsAsync(documentStore);
            await QueryDocumentsAsync(documentStore);

            Console.ReadKey();
        }

        private static async Task QueryDocumentsAsync(IDocumentStore documentStore)
        {
            using var session1 = documentStore.OpenAsyncSession();

            var openDocuments1 = await session1.Query<Document>()
                .Where(x => x.Status == Status.Open)
                .ToArrayAsync();

            var closedDocuments1 = await session1.Query<Document>()
                .Where(x => x.Status == Status.Closed)
                .ToArrayAsync();

            var openDocuments2 = await session1.Query<Document>()
                .Where(x => x.Status == Status.Open)
                .ToArrayAsync();

            var closedDocuments2 = await session1.Query<Document>()
                .Where(x => x.Status == Status.Closed)
                .ToArrayAsync();
        }

        private static IDocumentStore GetDocumentStore()
        {
            var serverUrl = "http://127.0.0.1:0/";
            var databaseName = "Test_1";
            var serverOptions = new ServerOptions
            {
                ServerUrl = serverUrl,
                FrameworkVersion = "3.1.x"
            };
            var conventions = new DocumentConventions
            {
                MaxNumberOfRequestsPerSession = int.MaxValue,
                AggressiveCache =
                {
                    Duration = TimeSpan.FromDays(1),
                    Mode = AggressiveCacheMode.TrackChanges
                },
            };
            var databaseOptions = new DatabaseOptions(databaseName)
            {
                Conventions = conventions
            };
            EmbeddedServer.Instance.StartServer(serverOptions);

            var documentStore = EmbeddedServer.Instance.GetDocumentStore(databaseOptions);
            documentStore.AggressivelyCache();
            documentStore.OnBeforeQuery += (sender, beforeQueryExecutedArgs) =>
            {
                beforeQueryExecutedArgs.QueryCustomization.WaitForNonStaleResults();
            };
            return documentStore;
        }

        private static async Task SeedDatabaseAsync(IDocumentStore documentStore)
        {
            using var session = documentStore.OpenAsyncSession();

            var operation = await documentStore.Operations.SendAsync(new DeleteByQueryOperation($"from {documentStore.Conventions.FindCollectionName(typeof(Document))}"));
            await operation.WaitForCompletionAsync();

            for (var i = 0; i < DocumentsCount; i++)
            {
                var document = new Document
                {
                    Id = $"{documentStore.Conventions.FindCollectionName(typeof(Document))}|",
                    Name = Guid.NewGuid().ToString(),
                    Status = i % 2 == 0 ? Status.Open : Status.Closed
                };
                await session.StoreAsync(document);
            }

            await session.SaveChangesAsync();
        }
    }
}
