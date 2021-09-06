using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Operations;
using Raven.Embedded;

namespace RavenDB.Issues.RavenDB_17177
{
    public static class Program
    {
        private const int DocumentsCount = 1024;

        public static async Task Main()
        {
            var documentStore = GetDocumentStore();
            await SeedDatabaseAsync(documentStore);

            await PatchByQueryAsync(documentStore);
            PatchByQuery(documentStore);

            Console.ReadKey();
        }

        private static async Task PatchByQueryAsync(IDocumentStore documentStore)
        {
            var patchOperation = new PatchByQueryOperation($@"from {documentStore.Conventions.FindCollectionName(typeof(Issue))} update {{ delete this.Status; }}");
            var operation = await documentStore.Operations.SendAsync(patchOperation);
            var result = await operation.WaitForCompletionAsync();
        }

        private static void PatchByQuery(IDocumentStore documentStore)
        {
            var patchOperation = new PatchByQueryOperation($@"from {documentStore.Conventions.FindCollectionName(typeof(Issue))} update {{ delete this.Name; }}");
            var operation = documentStore.Operations.Send(patchOperation);
            var result = operation.WaitForCompletion();
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
                CustomizeJsonSerializer = serializer =>
                {
                    // prevent '$type' property from being saved while working with dynamic type
                    // https://stackoverflow.com/questions/61526001/ravendb-how-to-prevent-type-being-saved-to-a-dynamic-typed-property/63703559#63703559
                    serializer.TypeNameHandling = TypeNameHandling.None;
                }
            };
            var databaseOptions = new DatabaseOptions(databaseName)
            {
                Conventions = conventions
            };
            EmbeddedServer.Instance.StartServer(serverOptions);

            var documentStore = EmbeddedServer.Instance.GetDocumentStore(databaseOptions);

            documentStore.OnBeforeQuery += (sender, beforeQueryExecutedArgs) =>
            {
                beforeQueryExecutedArgs.QueryCustomization.WaitForNonStaleResults();
            };
            return documentStore;
        }

        private static async Task SeedDatabaseAsync(IDocumentStore documentStore)
        {
            using var session = documentStore.OpenAsyncSession();

            var operation = await documentStore.Operations.SendAsync(new DeleteByQueryOperation($"from {documentStore.Conventions.FindCollectionName(typeof(Issue))}"));
            await operation.WaitForCompletionAsync();

            for (var i = 0; i < DocumentsCount; i++)
            {
                var document = new Issue
                {
                    Id = $"{documentStore.Conventions.FindCollectionName(typeof(Issue))}|",
                    Name = Guid.NewGuid().ToString(),
                    Status = i % 2 == 0 ? Status.Open : Status.Closed
                };
                await session.StoreAsync(document);
            }

            await session.SaveChangesAsync();
        }
    }
}
