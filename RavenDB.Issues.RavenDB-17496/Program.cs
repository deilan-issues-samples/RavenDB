using System;
using System.Collections.Generic;
using System.IO;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.ServerWide;
using Raven.Embedded;

namespace RavenDB.Issues.RavenDB_17496
{
    class Program
    {
        static void Main(string[] args)
        {
            // for the first launch
            string licensePath = null;
            // for the second launch
            //string licensePath = "raven-license.json";

            var documentStore = GetDocumentStore(licensePath, fromPath: true);

            EmbeddedServer.Instance.OpenStudioInBrowser();

            Console.ReadKey();
        }

        private static IDocumentStore GetDocumentStore(string licensePath = null, bool fromPath = false)
        {
            var serverOptions = new ServerOptions
            {
                FrameworkVersion = "3.1.x",
                CommandLineArgs = new List<string>
                {
                    "--Logs.Mode=Information"
                }
            };
            var databaseRecord = new DatabaseRecord()
            {
                DatabaseName = "Test"
            };

            if (licensePath != null)
            {
                var absoluteLicensePath = Path.GetFullPath(licensePath);
                var license = File.ReadAllText(absoluteLicensePath);

                if (fromPath)
                {
                    serverOptions.CommandLineArgs.Add($"--License.Path={absoluteLicensePath}");
                }
                else
                {
                    serverOptions.CommandLineArgs.Add($"--License={license}");
                }
            }

            var conventions = new DocumentConventions();

            var databaseOptions = new DatabaseOptions(databaseRecord)
            {
                Conventions = conventions
            };
            EmbeddedServer.Instance.StartServer(serverOptions);

            var documentStore = EmbeddedServer.Instance.GetDocumentStore(databaseOptions);

            return documentStore;
        }
    }
}
