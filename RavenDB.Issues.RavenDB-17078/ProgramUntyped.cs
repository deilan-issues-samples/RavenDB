using System;
using System.Dynamic;
using System.Linq;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Operations;
using Raven.Embedded;

namespace RavenDB.Issues.RavenDB_17078
{
    internal static class ProgramUntyped
    {
        private const string CollectionName = "Documents";
        private const int DocumentsCount = 1024;

        public static void Run()
        {
            var documentStore = GetDocumentStore();
            SeedDatabase(documentStore);
            using var session = documentStore.OpenSession();
            var query = session.Advanced.RawQuery<ExpandoObject>($@"
                from
                    {CollectionName} as d
                where
                    d.Status == 'Open'
            ");

            var pageSize = 1;
            var pageNumber = 1;
            dynamic[] documents;
            do
            {
                Console.WriteLine($@"pageNumber: {pageNumber}");
                // ReSharper disable once CoVariantArrayConversion
                // ObjectDisposedException: Cannot access a disposed object. Object name: 'blittable object has been disposed'.
                // on the second iteration (pageNumber == 2)
                documents = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToArray();
                Console.WriteLine($@"documents.Length: {documents.Length}");
                foreach (var document in documents)
                {
                    Console.WriteLine(document.Id);
                    document.Status = "Closed";
                }

                session.SaveChanges();
                pageNumber++;
            } while (documents.Length == pageSize);

            EmbeddedServer.Instance.OpenStudioInBrowser();

            Console.ReadKey();
        }

        private static IDocumentStore GetDocumentStore()
        {
            var serverUrl = "http://127.0.0.1:8080/";
            var databaseName = "Test_1";
            var serverOptions = new ServerOptions
            {
                ServerUrl = serverUrl,
                FrameworkVersion = "3.1.x"
            };
            var conventions = new DocumentConventions();
            var databaseOptions = new DatabaseOptions(databaseName)
            {
                Conventions = conventions
            };
            EmbeddedServer.Instance.StartServer(serverOptions);

            var documentStore = EmbeddedServer.Instance.GetDocumentStore(databaseOptions);
            return documentStore;
        }

        private static void SeedDatabase(IDocumentStore documentStore)
        {
            using var session = documentStore.OpenSession();

            documentStore.Operations.Send(new DeleteByQueryOperation($"from {CollectionName}"))
                .WaitForCompletion();

            for (var i = 0; i < DocumentsCount; i++)
            {
                dynamic document = new ExpandoObject();
                document.Id = $"{CollectionName}|";
                document.Name = Guid.NewGuid().ToString();
                document.Status = i % 2 == 0 ? "Open" : "Closed";
                session.Store(document);
                var metadata = session.Advanced.GetMetadataFor(document);
                metadata[Constants.Documents.Metadata.Collection] = CollectionName;
            }

            session.SaveChanges();
        }
    }
}
