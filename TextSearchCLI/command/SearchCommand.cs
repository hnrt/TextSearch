﻿using com.hideakin.textsearch.model;
using com.hideakin.textsearch.service;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace com.hideakin.textsearch.command
{
    internal class SearchCommand : ICommand
    {
        private readonly CancellationTokenSource cts;
        private readonly IndexService idx;
        private readonly FileService file;

        public SearchCommand()
        {
            cts = new CancellationTokenSource();
            idx = new IndexService(cts.Token);
            file = new FileService(cts.Token);
        }

        public void Register(CommandLine commandLine, CommandQueue commandQueue)
        {
            commandLine
                .AddHandler("-query", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("Group name is not specified.");
                    }
                    var group = (string)e.Current;
                    if (!e.MoveNext())
                    {
                        throw new Exception("Expression is not specified.");
                    }
                    var sb = new StringBuilder();
                    sb.Append((string)e.Current);
                    bool formatHTML = false;
                    while (e.MoveNext())
                    {
                        var a = (string)e.Current;
                        if (a == "-html")
                        {
                            formatHTML = true;
                            if (e.MoveNext())
                            {
                                throw new Exception(CommandLine.BAD_SYNTAX);
                            }
                            break;
                        }
                        else
                        {
                            sb.Append(' ');
                            sb.Append(a);
                        }
                    }
                    commandLine.NoMoreArg = true;
                    commandQueue.Add(() =>
                    {
                        Query(group, sb.ToString(), formatHTML);
                    });
                })
                .AddTranslation("-q", "-query")
                .AddUsageHeader("Usage <search>:")
                .AddUsage("{0} -query GROUPNAME EXPRESSION... [-html]", Program.Name);
        }

        private void Query(string group, string expression, bool formatHTML)
        {
            var rsp = idx.FindText(group, expression);
            if (formatHTML)
            {
                FormatQueryResultsInHtml(rsp);
            }
            else
            {
                FormatQueryResults(rsp);
            }
        }

        private void FormatQueryResults(IEnumerable<HitFile> results)
        {
            foreach (var f in results)
            {
                var contents = FileContents.Find(f.Fid);
                if (contents == null)
                {
                    contents = file.DownloadFile(f.Fid);
                }
                Console.WriteLine("{0}", contents.Path);
                foreach (var entry in f.Rows)
                {
                    Console.WriteLine("{0,6}: {1}", entry.Row + 1, contents.Lines[entry.Row]);
                }
            }
        }

        private void FormatQueryResultsInHtml(IEnumerable<HitFile> results)
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
                var contents = FileContents.Find(prc.Fid);
                if (contents == null)
                {
                    contents = file.DownloadFile(prc.Fid);
                }
                Console.WriteLine("<p>");
                Console.WriteLine("<font class=\"path\">{0}</font>", contents.Path);
                Console.WriteLine("<table>");
                foreach (var entry in prc.Rows)
                {
                    var line = contents.Lines[entry.Row];
                    sb.Length = 0;
                    int u = 0;
                    int v = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (u < entry.Matches.Count && i == entry.Matches[u].StartCol)
                        {
                            if (u++ == v)
                            {
                                sb.Append("<font class=\"match\">");
                            }
                        }
                        if (v < entry.Matches.Count && i == entry.Matches[v].EndCol)
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
    }
}
