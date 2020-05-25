using com.hideakin.textsearch.utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.service
{
    internal class Tokenizer
    {
        public const int MAX_LEN = 256;

        public static readonly string NEW_LINE = "\n";

        private int c;

        public Tokenizer()
        {
        }

        public List<string> Run(TextReader tr)
        {
            var texts = new List<string>();
            var sb = new StringBuilder();
            c = tr.Read();
            while (c != -1)
            {
                if (c == '\n')
                {
                    c = tr.Read();
                    texts.Add(NEW_LINE);
                }
                else if (c == '\r')
                {
                    c = tr.Read();
                    if (c == '\n')
                    {
                        c = tr.Read();
                    }
                    texts.Add(NEW_LINE);
                }
                else if (Char.IsWhiteSpace((char)c))
                {
                    c = tr.Read();
                }
                else if (UnicodeClassifier.IsJapaneseLetter(c))
                {
                    sb.Length = 0;
                    sb.Append((char)c);
                    c = tr.Read();
                    if (UnicodeClassifier.IsJapaneseLetter(c))
                    {
                        sb.Append((char)c);
                    }
                    texts.Add(sb.ToString());
                }
                else if (UnicodeClassifier.IsFullwidthAlphaNumeric(c))
                {
                    sb.Length = 0;
                    sb.Append((char)c);
                    c = tr.Read();
                    while (sb.Length < MAX_LEN && UnicodeClassifier.IsFullwidthAlphaNumeric(c))
                    {
                        sb.Append((char)c);
                        c = tr.Read();
                    }
                    texts.Add(sb.ToString());
                }
                else if (Char.IsLetterOrDigit((char)c))
                {
                    sb.Length = 0;
                    sb.Append((char)c);
                    c = tr.Read();
                    while (sb.Length < MAX_LEN && Char.IsLetterOrDigit((char)c))
                    {
                        sb.Append((char)c);
                        c = tr.Read();
                    }
                    texts.Add(sb.ToString().ToUpperInvariant());
                }
                else 
                {
                    c = tr.Read();
                }
            }
            return texts;
        }
    }
}
