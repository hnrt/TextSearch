using com.hideakin.textsearch.model;
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

        private const int EOF = -1;

        public List<Token> Tokens { get; } = new List<Token>();

        private int row = 0;

        private int col = 0;

        private StringBuilder buf = new StringBuilder();

        private int c;

        public Tokenizer()
        {
        }

        public void Run(TextReader tr)
        {
            c = tr.Read();
            while (c != EOF)
            {
                if (Char.IsWhiteSpace((char)c))
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
                    c = tr.Read();
                }
                else if (UnicodeClassifier.IsJapaneseLetter(c))
                {
                    int tRow = row;
                    int tCol = col;
                    buf.Length = 0;
                    buf.Append((char)c);
                    col++;
                    c = tr.Read();
                    if (UnicodeClassifier.IsJapaneseLetter(c))
                    {
                        buf.Append((char)c);
                    }
                    Tokens.Add(new Token(buf.ToString(), tRow, tCol));
                }
                else if (UnicodeClassifier.IsFullwidthAlphaNumeric(c))
                {
                    int tRow = row;
                    int tCol = col;
                    buf.Length = 0;
                    buf.Append((char)c);
                    col++;
                    c = tr.Read();
                    while (buf.Length < MAX_LEN && UnicodeClassifier.IsFullwidthAlphaNumeric(c))
                    {
                        buf.Append((char)c);
                        col++;
                        c = tr.Read();
                    }
                    Tokens.Add(new Token(buf.ToString(), tRow, tCol));
                }
                else if (Char.IsLetterOrDigit((char)c))
                {
                    int tRow = row;
                    int tCol = col;
                    buf.Length = 0;
                    buf.Append((char)c);
                    col++;
                    c = tr.Read();
                    while (buf.Length < MAX_LEN && Char.IsLetterOrDigit((char)c))
                    {
                        buf.Append((char)c);
                        col++;
                        c = tr.Read();
                    }
                    Tokens.Add(new Token(buf.ToString().ToUpperInvariant(), tRow, tCol));
                }
                else 
                {
                    col++;
                    c = tr.Read();
                }
            }
        }

        public List<string> Texts
        {
            get
            {
                var list = new List<string>(Tokens.Count);
                foreach (Token token in Tokens)
                {
                    list.Add(token.Text);
                }
                return list;
            }
        }
    }
}
