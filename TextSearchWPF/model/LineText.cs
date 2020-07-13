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

        public LineText(int line, string text)
        {
            Line = line;
            Text = text;
        }
    }
}
