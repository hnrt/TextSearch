using com.hideakin.textsearch.utility;
using System.Collections.Generic;

namespace com.hideakin.textsearch.model
{
    public class FileContents
    {
        private static readonly object mutex = new object();

        private static long serialNo = 0;

        private static Dictionary<int, (FileContents Contents, long SerialNo)> Cache { get; } = new Dictionary<int, (FileContents Contents, long SerialNo)>();

        private static Dictionary<int, string> Saved { get; } = new Dictionary<int, string>();

        public static FileContents Find(int fid)
        {
            lock(mutex)
            {
                if (Cache.TryGetValue(fid, out var value))
                {
                    return value.Contents;
                }
                else if (Saved.TryGetValue(fid, out var path))
                {
                    var path2 = GetTmpPath(fid);
                    if (System.IO.File.Exists(path2))
                    {
                        var lines = System.IO.File.ReadAllLines(path2);
                        return Store(fid, path, lines);
                    }
                }
                return null;
            }
        }

        public static FileContents Store(int fid, string path, string[] lines)
        {
            lock (mutex)
            {
                var contents = new FileContents(fid, path, lines);
                Cache[fid] = (contents, serialNo);
                serialNo++;
                if (IsOverCapacity())
                {
                    for (int count = 256; count > 0; count--)
                    {
                        Reduce();
                    }
                }
                return contents;
            }
        }

        private static bool IsOverCapacity()
        {
            return Cache.Count > 8192;
        }

        private static void Reduce()
        {
            int fid = -1;
            FileContents contents = null;
            long no = serialNo;
            foreach (var kvp in Cache)
            {
                if (kvp.Value.SerialNo < no)
                {
                    fid = kvp.Key;
                    contents = kvp.Value.Contents;
                    no = kvp.Value.SerialNo;
                }
            }
            if (fid > -1)
            {
                if (!Saved.ContainsKey(fid))
                {
                    var path2 = GetTmpPath(fid);
                    System.IO.File.WriteAllLines(path2, contents.Lines);
                    Saved[fid] = contents.Path;
                }
                Cache.Remove(fid);
            }
        }

        private static string GetTmpPath(int fid)
        {
            return System.IO.Path.Combine(AppData.DirectoryPath, string.Format("CACHE{0:D8}", fid));
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
