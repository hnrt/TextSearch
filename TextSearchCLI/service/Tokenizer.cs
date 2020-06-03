using com.hideakin.textsearch.utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace com.hideakin.textsearch.service
{
    internal class Tokenizer
    {
        public const int MAX_LEN = 256;

        public static readonly string NEW_LINE = "\n";

        private int c;

        private int line;

        public List<string> Texts { get; } = new List<string>();

        public List<int> Lines { get; } = new List<int>();

        public Tokenizer()
        {
        }

        public void Run(TextReader tr)
        {
            Texts.Clear();
            Lines.Clear();
            var sb = new StringBuilder();
            c = tr.Read();
            line = 0;
            while (c != -1)
            {
                if (Char.IsWhiteSpace((char)c))
                {
                    if (c == '\n')
                    {
                        line++;
                    }
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
                    Texts.Add(sb.ToString());
                    Lines.Add(line);
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
                    Texts.Add(sb.ToString());
                    Lines.Add(line);
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
                    Texts.Add(sb.ToString().ToUpperInvariant());
                    Lines.Add(line);
                }
                else 
                {
                    c = tr.Read();
                }
            }
        }
    }
}
