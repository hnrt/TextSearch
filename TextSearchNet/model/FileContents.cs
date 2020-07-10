using System.Collections.Generic;

namespace com.hideakin.textsearch.model
{
    public class FileContents
    {
        private static readonly object mutex = new object();

        private static Dictionary<int, FileContents> Cache { get; } = new Dictionary<int, FileContents>();

        public static FileContents Find(int fid)
        {
            lock(mutex)
            {
                return Cache.TryGetValue(fid, out var contents) ? contents : null;
            }
        }

        public static FileContents Store(int fid, string path, string[] lines)
        {
            lock (mutex)
            {
                var contents = new FileContents(fid, path, lines);
                Cache[fid] = contents;
                return contents;
            }
        }

        public int Fid { get; }

        public string Path { get; }

        public string[] Lines { get; }

        private FileContents(int fid, string path, string[] lines)
        {
            Fid = fid;
            Path = path;
            Lines = lines;
        }
    }
}
