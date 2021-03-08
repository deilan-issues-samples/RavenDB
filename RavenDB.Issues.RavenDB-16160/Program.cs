using System;
using System.Collections.Generic;
using System.Dynamic;
using Raven.Client;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Operations;
using Raven.Embedded;

namespace RavenDB.Issues.RavenDB_16160
{
    internal class Program
    {
        private const string CollectionName = "Documents";

        private static void Main()
        {
            // arrange
            var serverUrl = "http://127.0.0.1:8080/";
            var databaseName = "Test_1";
            var serverOptions = new ServerOptions
            {
                ServerUrl = serverUrl,
                CommandLineArgs = new List<string> { "Features.Availability=Experimental" }
            };
            var conventions = new DocumentConventions();
            var databaseOptions = new DatabaseOptions(databaseName)
            {
                Conventions = conventions
            };
            EmbeddedServer.Instance.StartServer(serverOptions);

            var documentStore = EmbeddedServer.Instance.GetDocumentStore(databaseOptions);
            var session = documentStore.OpenSession();

            documentStore.Operations.Send(new DeleteByQueryOperation($"from {CollectionName}"))
                .WaitForCompletion();

            for (var i = 0; i < 1024; i++)
            {
                dynamic document = new ExpandoObject();
                document.Id = $"{CollectionName}|";
                document.Name = Guid.NewGuid().ToString();
                session.Store(document);
                var metadata = session.Advanced.GetMetadataFor(document);
                metadata[Constants.Documents.Metadata.Collection] = CollectionName;
            }

            session.SaveChanges();

            EmbeddedServer.Instance.OpenStudioInBrowser();

            // issue a query:
            // 'match (Documents)'
            // Expected results count == 1024
            // Actual results count == 101

            Console.ReadKey();
        }
    }
}
