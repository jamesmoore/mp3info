using System;
using System.IO;

namespace MP3InfoTest
{
    internal static class TestExtensions
    {
        public static string ToCurrentSystemPathFormat(this string path)
        {
            if (path.Contains('\\') && path.Contains('/'))
            {
                throw new ArgumentException(path + " contains multiple formats");
            }

            var directorySeparatorChar = Path.DirectorySeparatorChar;
            if (path.Contains('\\') && directorySeparatorChar != '\\')
            {
                return path.Replace('\\', directorySeparatorChar);
            }

            if (path.Contains('/') && directorySeparatorChar != '/')
            {
                return path.Replace('/', directorySeparatorChar);
            }

            return path;
        }
    }
}
