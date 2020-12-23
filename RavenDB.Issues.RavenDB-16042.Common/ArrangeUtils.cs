using System;
using System.IO;

namespace RavenDB.Issues.RavenDB_16042.Common
{
    public static class ArrangeUtils
    {
        public static string PrepareDataDirectory()
        {
            var ravenDbSourcePath = Path.Combine(FileSystemUtils.GetProjectPath(), "RavenDB");
            var ravenDbDestinationPath = Path.Combine(AppContext.BaseDirectory, "RavenDB");
            if (Directory.Exists(ravenDbDestinationPath))
            {
                Directory.Delete(ravenDbDestinationPath, true);
            }
            FileSystemUtils.CopyDirectory(ravenDbSourcePath, ravenDbDestinationPath);
            return ravenDbDestinationPath;
        }
    }
}