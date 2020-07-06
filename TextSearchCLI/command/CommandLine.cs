using System;
using System.Collections.Generic;

namespace com.hideakin.textsearch.command
{
    internal class CommandLine
    {
        public static readonly string BAD_SYNTAX = "Bad command line syntax.";

        private static readonly string INDENTATION = "  ";

        private Dictionary<string, Action<System.Collections.IEnumerator>> Handlers { get; } = new Dictionary<string, Action<System.Collections.IEnumerator>>();

        private Dictionary<string, string> Translations { get; } = new Dictionary<string, string>();

        private List<string> UsageLines { get; } = new List<string>();

        private List<string> OptionLines { get; } = new List<string>();

        public bool NoMoreArg { get; set; } = false;

        public CommandLine()
        {
        }

        public CommandLine AddHandler(string key, Action<System.Collections.IEnumerator> action)
        {
            Handlers.Add(key, action);
            return this;
        }

        public CommandLine RemoveHandler(string key)
        {
            Handlers.Remove(key);
            return this;
        }

        public CommandLine AddTranslation(string key, string value)
        {
            Translations.Add(key, value);
            return this;
        }

        public CommandLine AddUsageHeader(string line)
        {
            UsageLines.Add(line);
            return this;
        }

        public CommandLine AddUsage(string line)
        {
            UsageLines.Add(INDENTATION + line);
            return this;
        }

        public CommandLine AddUsage(string format, object arg0)
        {
            UsageLines.Add(INDENTATION + string.Format(format, arg0));
            return this;
        }

        public CommandLine AddUsage(string format, object arg0, object arg1)
        {
            UsageLines.Add(INDENTATION + string.Format(format, arg0, arg1));
            return this;
        }

        public CommandLine AddUsage(string format, object arg0, object arg1, object arg2)
        {
            UsageLines.Add(INDENTATION + string.Format(format, arg0, arg1, arg2));
            return this;
        }

        public CommandLine AddUsage(string format, object arg0, object arg1, object arg2, object arg3)
        {
            UsageLines.Add(INDENTATION + string.Format(format, arg0, arg1, arg2, arg3));
            return this;
        }

        public CommandLine AddUsage(string format, object arg0, object arg1, object arg2, object arg3, object arg4)
        {
            UsageLines.Add(INDENTATION + string.Format(format, arg0, arg1, arg2, arg3, arg4));
            return this;
        }

        public CommandLine AddOption(string line)
        {
            OptionLines.Add(INDENTATION + line);
            return this;
        }

        public CommandLine AddOption(string format, object arg0)
        {
            OptionLines.Add(INDENTATION + string.Format(format, arg0));
            return this;
        }

        public CommandLine AddOption(string format, object arg0, object arg1)
        {
            OptionLines.Add(INDENTATION + string.Format(format, arg0, arg1));
            return this;
        }

        public CommandLine AddOption(string format, object arg0, object arg1, object arg2)
        {
            OptionLines.Add(INDENTATION + string.Format(format, arg0, arg1, arg2));
            return this;
        }

        public CommandLine AddOption(string format, object arg0, object arg1, object arg2, object arg3)
        {
            OptionLines.Add(INDENTATION + string.Format(format, arg0, arg1, arg2, arg3));
            return this;
        }

        public CommandLine AddOption(string format, object arg0, object arg1, object arg2, object arg3, object arg4)
        {
            OptionLines.Add(INDENTATION + string.Format(format, arg0, arg1, arg2, arg3, arg4));
            return this;
        }

        public void Parse(string[] args)
        {
            var e = Parse(args.GetEnumerator());
            if (e != null)
            {
                throw new Exception(BAD_SYNTAX);
            }
        }

        public System.Collections.IEnumerator Parse(System.Collections.IEnumerator e)
        {
            while (e.MoveNext())
            {
                string a = (string)e.Current;
                if (Translations.TryGetValue(a, out var a2))
                {
                    a = a2;
                }
                if (Handlers.TryGetValue(a, out var action))
                {
                    action(e);
                    if (NoMoreArg)
                    {
                        return null;
                    }
                }
                else
                {
                    return e;
                }
            }
            return null;
        }

        public void PutUsage()
        {
            foreach (var line in UsageLines)
            {
                Console.WriteLine("{0}", line);
            }
            Console.WriteLine("Options <common>:");
            foreach (var line in OptionLines)
            {
                Console.WriteLine("{0}", line);
            }
        }
    }
}
