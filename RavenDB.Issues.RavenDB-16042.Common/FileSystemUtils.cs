using System;
using System.IO;
using System.Text.RegularExpressions;

namespace RavenDB.Issues.RavenDB_16042.Common
{
    public class FileSystemUtils
    {
        public static string GetProjectPath()
        {
            var exePath = Path.GetDirectoryName(AppContext.BaseDirectory);
            var appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            return appRoot;
        }

        public static void CopyDirectory(string source, string destination)
        {
            foreach (string dirPath in Directory.GetDirectories(source, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, destination));

            foreach (string newPath in Directory.GetFiles(source, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, destination), true);
        }
    }
}
