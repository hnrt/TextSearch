using com.hideakin.textsearch.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace com.hideakin.textsearch.utility
{
    internal class TextTokenizer
    {
        public const int DEFAULT_MODE = 0;

        public const int QUERY_MODE = 1;

        public const int MAX_LEN = 255;

        public static readonly string NEW_LINE = "\n";

        private const int EOF = -1;

        public List<TextToken> Tokens { get; }

        private int mode;

        private StringBuilder buf;

        private int row;

        private int col;

        private TextReader input;

        private int c;

        private int tRow;

        private int tCol;

        public TextTokenizer(int mode = DEFAULT_MODE)
        {
            this.mode = mode;
            Tokens = new List<TextToken>();
            buf = new StringBuilder();
            row = 0;
            col = 0;
        }

        public string[] Texts => Tokens.Select(x => x.Text).ToArray();

        public void Run(string[] lines)
        {
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                foreach (var line in lines)
                {
                    sw.WriteLine(line);
                }
                sw.Flush();
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    Run(sr);
                }
            }
        }

        public void Run(TextReader input)
        {
            Initialize(input);
            while (c != EOF)
            {
                if (Char.IsWhiteSpace((char)c))
                {
                    Read();
                }
                else if (UnicodeClassifier.IsJapaneseLetter(c))
                {
                    Start();
                    Read();
                    if (UnicodeClassifier.IsJapaneseLetter(c))
                    {
                        Append();
                    }
                    EndAsIs();
                }
                else if (UnicodeClassifier.IsFullwidthAlphaNumeric(c))
                {
                    Start();
                    Read();
                    while (buf.Length < MAX_LEN && UnicodeClassifier.IsFullwidthAlphaNumeric(c))
                    {
                        Append();
                        Read();
                    }
                    EndAsIs();
                }
                else if (Char.IsLetterOrDigit((char)c) || c == '_')
                {
                    Start();
                    Read();
                    while (buf.Length < MAX_LEN && (Char.IsLetterOrDigit((char)c) || c == '_'))
                    {
                        Append();
                        Read();
                    }
                    EndAsUpper();
                }
                else if (mode == QUERY_MODE && (c == '&' || c == '|' || c == '(' || c == ')'))
                {
                    Start();
                    Read();
                    EndAsIs();
                }
                else 
                {
                    Read();
                }
            }
        }

        private void Initialize(TextReader input)
        {
            this.input = input;
            c = input.Read();
        }

        private void Read()
        {
            if (c == '\n')
            {
                row++;
                col = 0;
            }
            else
            {
                col++;
            }
            c = input.Read();
        }

        private void Start()
        {
            tRow = row;
            tCol = col;
            buf.Length = 0;
            buf.Append((char)c);
        }

        private void Append()
        {
            buf.Append((char)c);
        }

        private void EndAsIs()
        {
            Tokens.Add(new TextToken(buf.ToString(), tRow, tCol));
        }

        private void EndAsUpper()
        {
            Tokens.Add(new TextToken(buf.ToString().ToUpperInvariant(), tRow, tCol));
        }
    }
}
