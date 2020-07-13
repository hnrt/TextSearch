using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.model
{
    internal class HitItem
    {
        public int Fid { get; set; }

        public string Path { get; set; }

        public string FileName { get; set; }

        public int Line { get; set; }

        public string Text { get; set; }

        public HitItem(int fid, string path, int line, string text)
        {
            Fid = fid;
            Path = path;
            FileName = System.IO.Path.GetFileName(path);
            Line = line;
            Text = text;
        }
    }
}
