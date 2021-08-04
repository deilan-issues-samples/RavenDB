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
    internal static class Program
    {
        private static void Main()
        {
            ProgramUntyped.Run();
            //ProgramTyped.Run();
        }
    }
}
