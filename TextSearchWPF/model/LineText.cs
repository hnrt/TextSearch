using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.model
{
    internal class LineText
    {
        public int Line { get; set; }

        public string Text { get; set; }

        public List<(int Start, int End)> Matches { get; set; }

        public LineText(int line, string text, List<(int Start, int End)> matches)
        {
            Line = line;
            Text = text;
            Matches = matches;
        }
    }
}
