using System;
using System.IO;

namespace com.hideakin.textsearch.utility
{
    public static class AppData
    {
        public static string DirectoryPath
        {
            get
            {
                var dir1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HNRT");
                if (!Directory.Exists(dir1))
                {
                    Directory.CreateDirectory(dir1);
                }
                var dir2 = Path.Combine(dir1, "TextSearch");
                if (!Directory.Exists(dir2))
                {
                    Directory.CreateDirectory(dir2);
                }
                return dir2;
            }
        }

        public static string GetPath(string filename)
        {
            return Path.Combine(DirectoryPath, filename);
        }
    }
}
