using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Http;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Raven.Embedded;

namespace RavenDB.Issues.RavenDB_17167
{
    public static class Program
    {
        private const int DocumentsCount = int.MaxValue;

        public static async Task Main()
        {
            var documentStore = GetDocumentStore();
            await ClearDatabase(documentStore);
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

            var openDocuments1 = await session1.Query<DocumentX>()
                .ToArrayAsync();

            var closedDocuments1 = await session1.Query<DocumentY>()
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

        private static async Task ClearDatabase(IDocumentStore documentStore)
        {
            var deleteOperation = new DeleteDatabasesOperation(documentStore.Database, true);
            await documentStore.Maintenance.Server.SendAsync(deleteOperation);

            var createDatabaseOperation = new CreateDatabaseOperation(new DatabaseRecord(documentStore.Database));
            await documentStore.Maintenance.Server.SendAsync(createDatabaseOperation);
        }

        private static async Task SeedDatabaseAsync(IDocumentStore documentStore)
        {
            using var session = documentStore.OpenAsyncSession();

            const int pageSize = 1024;
            var pageNumber = 1;

            var listX = new List<DocumentX>(pageSize);
            var listY = new List<DocumentY>(pageSize);

            while (pageSize * (pageNumber - 1) < DocumentsCount)
            {
                var count = Math.Min(pageSize, DocumentsCount - pageSize * (pageNumber - 1));
                for (var i = 0; i < count; i++)
                {
                    var documentX = new DocumentX
                    {
                        Id = $"{documentStore.Conventions.FindCollectionName(typeof(DocumentX))}|",
                        Name = Guid.NewGuid().ToString(),
                        Status = i % 2 == 0 ? Status.Open : Status.Closed
                    };
                    await session.StoreAsync(documentX);

                    var documentY = new DocumentY
                    {
                        Id = $"{documentStore.Conventions.FindCollectionName(typeof(DocumentY))}|",
                        Name = Guid.NewGuid().ToString(),
                        Status = i % 2 == 0 ? Status.Open : Status.Closed
                    };
                    await session.StoreAsync(documentY);

                    documentX.DocumentYId = documentY.Id;
                    documentY.DocumentXId = documentX.Id;

                    listX.Add(documentX);
                    listY.Add(documentY);
                }

                await session.SaveChangesAsync();

                // attempt to free resources
                for (var i = count - 1; i >= 0; i--)
                {
                    session.Advanced.Evict(listX[i]);
                    listX.RemoveAt(i);
                    session.Advanced.Evict(listY[i]);
                    listY.RemoveAt(i);
                }

                pageNumber++;
            }
        }
    }
}
