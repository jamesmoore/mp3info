using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
