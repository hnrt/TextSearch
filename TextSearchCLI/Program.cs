using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using com.hideakin.textsearch.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace com.hideakin.textsearch
{
    enum CommandType
    {
        None,
        Help,
        PrintGroups,
        PrintFiles,
        UpdateIndex,
        DeleteIndex,
        Query,
        PrintExtensions,
        AddExtensions,
        ClearExtensions,
        PrintSkipDirs,
        AddSkipDirs,
        ClearSkipDirs
    }

    class Program
    {
        public static readonly string BAD_COMMAND_LINE_SYNTAX = "Bad command line syntax.";

        public string Name { get; }

        private Dictionary<string, Action<System.Collections.IEnumerator>> OptionMap { get; } = new Dictionary<string, Action<System.Collections.IEnumerator>>();

        private Dictionary<string, string> OptionAltMap { get; } = new Dictionary<string, string>();

        private CommandType commandType = CommandType.None;

        private Dictionary<CommandType, Action> CommandMap { get; } = new Dictionary<CommandType, Action>();

        private string groupName = "default";

        private List<string> OperandList { get; } = new List<string>();

        private FileGroupService FileGrpSvc { get; } = new FileGroupService();

        private FileService FileSvc { get; } = new FileService();

        private IndexService IndexSvc { get; } = new IndexService();

        private PreferenceService PrefSvc { get; } = new PreferenceService();

        private List<string> extensions;

        private List<string> skipDirs;

        private bool formatHTML = false;

        public static int DebugLevel { get; set; } = 0;

        public Program()
        {
            Name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            OptionMap.Add("-help", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.Help;
            });
            OptionMap.Add("-print-groups", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.PrintGroups;
            });
            OptionMap.Add("-print-files", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.PrintFiles;
            });
            OptionMap.Add("-group", (e) =>
            {
                if (!e.MoveNext())
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                groupName = (string)e.Current;
            });
            OptionMap.Add("-index", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.UpdateIndex;
                if (!e.MoveNext())
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                OperandList.Add((string)e.Current);
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-delete-index", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.DeleteIndex;
            });
            OptionMap.Add("-query", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.Query;
                if (!e.MoveNext())
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                OperandList.Add((string)e.Current);
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-print-extensions", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.PrintExtensions;
            });
            OptionMap.Add("-extensions", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.AddExtensions;
                if (!e.MoveNext())
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                OperandList.Add((string)e.Current);
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-clear-extensions", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.ClearExtensions;
            });
            OptionMap.Add("-print-skip-directories", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.PrintSkipDirs;
            });
            OptionMap.Add("-skip-directories", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.AddSkipDirs;
                if (!e.MoveNext())
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                OperandList.Add((string)e.Current);
                while (e.MoveNext())
                {
                    OperandList.Add((string)e.Current);
                }
            });
            OptionMap.Add("-clear-skip-directories", (e) =>
            {
                if (commandType != CommandType.None)
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                commandType = CommandType.ClearSkipDirs;
            });
            OptionMap.Add("-index-api", (e) =>
            {
                if (!e.MoveNext())
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
                IndexNetClient.Url = (string)e.Current;
            });
            OptionMap.Add("-debug", (e) =>
            {
                DebugLevel++;
            });

            OptionAltMap.Add("-h", "-help");
            OptionAltMap.Add("-?", "-help");
            OptionAltMap.Add("-pg", "-print-groups");
            OptionAltMap.Add("-print-grp", "-print-groups");
            OptionAltMap.Add("-pf", "-print-files");
            OptionAltMap.Add("-g", "-group");
            OptionAltMap.Add("-q", "-query");
            OptionAltMap.Add("-i", "-index");
            OptionAltMap.Add("-pe", "-print-extensions");
            OptionAltMap.Add("-print-ext", "-print-extensions");
            OptionAltMap.Add("-e", "-extensions");
            OptionAltMap.Add("-ext", "-extensions");
            OptionAltMap.Add("-ce", "-clear-extensions");
            OptionAltMap.Add("-clear-ext", "-clear-extensions");
            OptionAltMap.Add("-psd", "-print-skip-directories");
            OptionAltMap.Add("-print-skip-dir", "-print-skip-directories");
            OptionAltMap.Add("-sd", "-skip-directories");
            OptionAltMap.Add("-skip-dir", "-skip-directories");
            OptionAltMap.Add("-csd", "-clear-skip-directories");
            OptionAltMap.Add("-clear-skip-dir", "-clear-skip-directories");

            CommandMap.Add(CommandType.None, Help);
            CommandMap.Add(CommandType.Help, Help);
            CommandMap.Add(CommandType.PrintGroups, PrintGroups);
            CommandMap.Add(CommandType.PrintFiles, PrintFiles);
            CommandMap.Add(CommandType.UpdateIndex, UpdateIndex);
            CommandMap.Add(CommandType.DeleteIndex, DeleteIndex);
            CommandMap.Add(CommandType.Query, Query);
            CommandMap.Add(CommandType.PrintExtensions, PrintExtensions);
            CommandMap.Add(CommandType.AddExtensions, AddExtensions);
            CommandMap.Add(CommandType.ClearExtensions, ClearExtensions);
            CommandMap.Add(CommandType.PrintSkipDirs, PrintSkipDirs);
            CommandMap.Add(CommandType.AddSkipDirs, AddSkipDirs);
            CommandMap.Add(CommandType.ClearSkipDirs, ClearSkipDirs);
        }

        public void ParseCommandLine(string[] args)
        {
            System.Collections.IEnumerator e = args.GetEnumerator();
            while (e.MoveNext())
            {
                string a = (string)e.Current;
                if (OptionAltMap.TryGetValue(a, out var a2))
                {
                    a = a2;
                }
                if (OptionMap.TryGetValue(a, out var action))
                {
                    action(e);
                }
                else
                {
                    throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                }
            }
        }

        public void Run()
        {
            CommandMap[commandType]();
        }

        private void Help()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  {0} -group FILEGROUP -index PATH...", Name);
            Console.WriteLine("  {0} -group FILEGROUP -delete-index", Name);
            Console.WriteLine("  {0} -group FILEGROUP -query EXPR [-html]", Name);
            Console.WriteLine("  {0} -print-grp", Name);
            Console.WriteLine("  {0} -print-ext", Name);
            Console.WriteLine("  {0} -ext EXT1,EXT2,...", Name);
            Console.WriteLine("  {0} -clear-ext", Name);
            Console.WriteLine("  {0} -print-skip-dir", Name);
            Console.WriteLine("  {0} -skip-dir DIR1,DIR2,...", Name);
            Console.WriteLine("  {0} -clear-skip-dir", Name);
        }

        private void PrintGroups()
        {
            var names = FileGrpSvc.GetFileGroups();
            if (names == null)
            {
                return;
            }
            foreach (string name in names)
            {
                Console.WriteLine("{0}", name);
            }
        }

        private void PrintFiles()
        {
            var names = FileSvc.GetFiles(groupName);
            if (names == null)
            {
                return;
            }
            foreach (string name in names)
            {
                Console.WriteLine("{0}", name);
            }
        }

        private void UpdateIndex()
        {
            Console.WriteLine("Started indexing...");
            extensions = PrefSvc.GetExtensions();
            skipDirs = PrefSvc.GetSkipDirs();
            foreach (string path in OperandList)
            {
                if (Directory.Exists(path))
                {
                    if (skipDirs.Contains(Path.GetFileName(path)))
                    {
                        Console.WriteLine("Skipping {0}...", path);
                    }
                    else
                    {
                        IndexDir(path);
                    }
                }
                else if (File.Exists(path))
                {
                    if (extensions.Count == 0 || extensions.Contains(Path.GetExtension(path)))
                    {
                        IndexFile(path);
                    }
                    else
                    {
                        Console.WriteLine("Skipping {0}...", path);
                    }
                }
                else
                {
                    Console.WriteLine("Unable to find {0}...", path);
                }
            }
            Console.WriteLine("Done.");
        }

        private void IndexDir(string path)
        {
            var dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                if (skipDirs.Contains(Path.GetFileName(dir)))
                {
                    Console.WriteLine("Skipping {0}...", dir);
                }
                else
                {
                    IndexDir(dir);
                }
            }
            var files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                if (extensions.Count == 0 || extensions.Contains(Path.GetExtension(file)))
                {
                    IndexFile(file);
                }
                else
                {
                    Console.WriteLine("Skipping {0}...", file);
                }
            }
        }

        private void IndexFile(string path)
        {
            path = Path.GetFullPath(path);
            Console.WriteLine("{0}", path);
            IndexSvc.UpdateIndex(groupName, path);
        }

        public void DeleteIndex()
        {
            Console.WriteLine("Started deleting index...");
            IndexSvc.DeleteIndex(groupName);
            Console.WriteLine("Done.");
        }

        private void Query()
        {
            var sb = new StringBuilder();
            foreach (var s in OperandList)
            {
                if (s == "-html")
                {
                    formatHTML = true;
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(s);
                }
            }
            var rsp = IndexSvc.FindText(groupName, sb.ToString());
            if (formatHTML)
            {
                FormatQueryResultsInHtml(rsp);
            }
            else
            {
                FormatQueryResults(rsp);
            }
        }

        private void FormatQueryResults(PathRowColumns[] results)
        {
            foreach (var prc in results)
            {
                Console.WriteLine("{0}", prc.Path);
                var lines = File.ReadAllLines(prc.Path);
                foreach (var entry in prc.Rows)
                {
                    Console.WriteLine("{0,6}: {1}", entry.Row + 1, lines[entry.Row]);
                }
            }
        }

        private void FormatQueryResultsInHtml(PathRowColumns[] results)
        {
            var sb = new StringBuilder();
            Console.WriteLine("<!doctype html>");
            Console.WriteLine("<html>");
            Console.WriteLine("<head>");
            Console.WriteLine("<style>");
            Console.WriteLine("table { border-collapse:collapse; border-width:thin; border-style:solid; width:100%; }");
            Console.WriteLine("tr td { border-width:thin; border-style:solid; }");
            Console.WriteLine("td.lineno { width:6em; text-align:right; background-color:lightgray; }");
            Console.WriteLine("font.path { color:darkgreen; }");
            Console.WriteLine("font.match { color:red; }");
            Console.WriteLine("</style>");
            Console.WriteLine("</head>");
            Console.WriteLine("<body>");
            foreach (var prc in results)
            {
                Console.WriteLine("<p>");
                Console.WriteLine("<font class=\"path\">{0}</font>", prc.Path);
                Console.WriteLine("<table>");
                var lines = File.ReadAllLines(prc.Path);
                foreach (var entry in prc.Rows)
                {
                    var line = lines[entry.Row];
                    sb.Length = 0;
                    int u = 0;
                    int v = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (u < entry.Columns.Count && i == entry.Columns[u].Start)
                        {
                            if (u++ == v)
                            {
                                sb.Append("<font class=\"match\">");
                            }
                        }
                        if (v < entry.Columns.Count && i == entry.Columns[v].End)
                        {
                            if (++v == u)
                            {
                                sb.Append("</font>");
                            }
                        }
                        char c = line[i];
                        if (c == '&') sb.Append("&amp;");
                        else if (c == '<') sb.Append("&lt;");
                        else if (c == '>') sb.Append("&gt;");
                        else sb.Append(c);
                    }
                    if (v < u)
                    {
                        sb.Append("</font>");
                    }
                    Console.WriteLine("<tr><td class=\"lineno\">{0}</td><td>{1}</td></tr>", entry.Row + 1, sb.ToString());
                }
                Console.WriteLine("</table>");
                Console.WriteLine("</p>");
            }
            Console.WriteLine("</body>");
            Console.WriteLine("</html>");
        }

        private void PrintExtensions()
        {
            var list = PrefSvc.GetExtensions();
            foreach (string ext in list)
            {
                Console.WriteLine("{0}", ext);
            }
        }

        private void AddExtensions()
        {
            Console.WriteLine("Adding extensions setting...");
            var sb = new StringBuilder();
            foreach (string s in OperandList)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append(s);
            }
            PrefSvc.AddExtensions(sb.ToString());
            Console.WriteLine("Done.");
        }

        private void ClearExtensions()
        {
            Console.WriteLine("Clearing extensions setting...");
            PrefSvc.ClearExtensions();
            Console.WriteLine("Done.");
        }

        private void PrintSkipDirs()
        {
            var list = PrefSvc.GetSkipDirs();
            foreach (string dir in list)
            {
                Console.WriteLine("{0}", dir);
            }
        }

        private void AddSkipDirs()
        {
            Console.WriteLine("Adding skip-dirs setting...");
            var sb = new StringBuilder();
            foreach (string s in OperandList)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append(s);
            }
            PrefSvc.AddSkipDirs(sb.ToString());
            Console.WriteLine("Done.");
        }

        private void ClearSkipDirs()
        {
            Console.WriteLine("Clearing skip-dirs setting...");
            PrefSvc.ClearSkipDirs();
            Console.WriteLine("Done.");
        }

        static void Main(string[] args)
        {
            try 
            {
                var app = new Program();
                app.ParseCommandLine(args);
                app.Run();
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("ERROR: {0}", e.Message);
                for (e = e.InnerException; e != null; e = e.InnerException)
                {
                    Console.Error.WriteLine("\t{0}", e.Message);
                }
            }
            Environment.Exit(1);
        }
    }
}
