using com.hideakin.textsearch.model;
using com.hideakin.textsearch.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.hideakin.textsearch
{
    enum CommandType
    {
        None,
        Help,
        UpdateIndex,
        DeleteIndex,
        Query
    }

    class Program
    {
        public static readonly string BAD_COMMAND_LINE_SYNTAX = "Bad command line syntax.";

        public string Name { get; }

        private CommandType commandType = CommandType.None;

        private Dictionary<CommandType, Action> CommandMap { get; }

        private string groupName = "default";

        private List<string> PathsToIndex { get; } = new List<string>();

        private List<string> DirsToExclude { get; } = new List<string>();

        private List<string> ExtsToExclude { get; } = new List<string>();

        private string queryExpression;

        private IndexService IndexSvc { get; } = new IndexService();

        public Program()
        {
            Name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            CommandMap = new Dictionary<CommandType, Action>();
            CommandMap.Add(CommandType.None, Help);
            CommandMap.Add(CommandType.Help, Help);
            CommandMap.Add(CommandType.UpdateIndex, UpdateIndex);
            CommandMap.Add(CommandType.DeleteIndex, DeleteIndex);
            CommandMap.Add(CommandType.Query, Query);
        }

        public void ParseCommandLine(string[] args)
        {
            int i = 0;
            while (i < args.Length)
            {
                string a = args[i++];
                if (a == "-help" || a == "-h" || a == "-?")
                {
                    if (commandType != CommandType.None)
                    {
                        throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                    }
                    commandType = CommandType.Help;
                }
                else if (a == "-group" || a == "-g")
                {
                    if (i == args.Length)
                    {
                        throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                    }
                    groupName = args[i++];
                }
                else if (a == "-index" || a == "-i")
                {
                    if (commandType != CommandType.None)
                    {
                        throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                    }
                    commandType = CommandType.UpdateIndex;
                    if (i == args.Length)
                    {
                        throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                    }
                    var ss = args[i++].Split(new char[] { ',', ';' });
                    foreach (string s in ss)
                    {
                        PathsToIndex.Add(s);
                    }
                }
                else if (a == "-exclude-dir")
                {
                    if (i == args.Length)
                    {
                        throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                    }
                    var ss = args[i++].Split(new char[] { ',', ';' });
                    foreach (string s in ss)
                    {
                        DirsToExclude.Add(s);
                    }
                }
                else if (a == "-exclude-ext")
                {
                    if (i == args.Length)
                    {
                        throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                    }
                    var ss = args[i++].Split(new char[] { ',', ';' });
                    foreach (string s in ss)
                    {
                        ExtsToExclude.Add(s.StartsWith(".") ? s : ("." + s));
                    }
                }
                else if (a == "-delete-index")
                {
                    if (commandType != CommandType.None)
                    {
                        throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                    }
                    commandType = CommandType.DeleteIndex;
                }
                else if (a == "-query" || a == "-q")
                {
                    if (commandType != CommandType.None)
                    {
                        throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                    }
                    commandType = CommandType.Query;
                    if (i == args.Length)
                    {
                        throw new Exception(BAD_COMMAND_LINE_SYNTAX);
                    }
                    queryExpression = args[i++];
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

        public void Help()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  {0} -group FILEGROUP -index PATH [options]", Name);
            Console.WriteLine("  {0} -group FILEGROUP -delete-index", Name);
            Console.WriteLine("  {0} -group FILEGROUP -query EXPR", Name);
            Console.WriteLine("Options:");
            Console.WriteLine("  -exclude-dir DIR1[,DIR2,...]");
            Console.WriteLine("  -exclude-ext EXT1[,EXT2,...]");
        }

        public void UpdateIndex()
        {
            Console.WriteLine("Started indexing...");
            foreach (string path in PathsToIndex)
            {
                if (Directory.Exists(path))
                {
                    if (DirsToExclude.Contains(path))
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
                    if (ExtsToExclude.Contains(Path.GetExtension(path)))
                    {
                        Console.WriteLine("Skipping {0}...", path);
                    }
                    else
                    {
                        IndexFile(path);
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
                if (DirsToExclude.Contains(dir))
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
                if (ExtsToExclude.Contains(Path.GetExtension(file)))
                {
                    Console.WriteLine("Skipping {0}...", file);
                }
                else
                {
                    IndexFile(file);
                }
            }
        }

        private void IndexFile(string path)
        {
            path = Path.GetFullPath(path);
            Console.WriteLine("{0}", path);
            if (!IndexSvc.UpdateIndex(groupName, path))
            {
                Console.WriteLine("  FAILURE");
            }
        }

        public void DeleteIndex()
        {
            Console.WriteLine("Started deleting index...");
            IndexSvc.DeleteIndex(groupName);
            Console.WriteLine("Done.");
        }

        private void Query()
        {
            var rsp = IndexSvc.FindText(groupName, queryExpression);
            foreach (PathLines pathLines in rsp)
            {
                Console.WriteLine("{0}", pathLines.Path);
                var lineTexts = File.ReadAllLines(pathLines.Path);
                foreach (int line in pathLines.Lines)
                {
                    Console.WriteLine("{0,6}: {1}", line + 1, lineTexts[line]);
                }
            }
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
