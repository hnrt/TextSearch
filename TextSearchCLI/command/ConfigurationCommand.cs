using com.hideakin.textsearch.net;
using com.hideakin.textsearch.service;
using System;
using System.Collections.Generic;

namespace com.hideakin.textsearch.command
{
    internal class ConfigurationCommand : ICommand
    {
        private PreferenceService PrefSvc { get; } = new PreferenceService();

        public void Register(CommandLine commandLine, CommandQueue commandQueue)
        {
            commandLine
                .AddHandler("-index-api", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("URL is not specified.");
                    }
                    var a = (string)e.Current;
                    if (!a.StartsWith("http://") && !a.StartsWith("https://"))
                    {
                        throw new Exception("URL does not look valid.");
                    }
                    PrefSvc.Url = a;
                })
                .AddHandler("-print-extensions", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        var list = PrefSvc.GetExtensions();
                        foreach (string ext in list)
                        {
                            Console.WriteLine("{0}", ext);
                        }
                    });
                })
                .AddHandler("-add-extensions", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("Extensions are not specified.");
                    }
                    var exts = (string)e.Current;
                    commandQueue.Add(() =>
                    {
                        Console.WriteLine("Adding extensions setting...");
                        var res = PrefSvc.AddExtensions(exts);
                        Console.WriteLine("{0}", res);
                    });
                })
                .AddHandler("-clear-extensions", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        Console.WriteLine("Clearing extensions setting...");
                        PrefSvc.ClearExtensions();
                        Console.WriteLine("Done.");
                    });
                })
                .AddHandler("-print-skip-dirs", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        var list = PrefSvc.GetSkipDirs();
                        foreach (string dir in list)
                        {
                            Console.WriteLine("{0}", dir);
                        }
                    });
                })
                .AddHandler("-add-skip-dirs", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("Directories are not specified.");
                    }
                    var dirs = (string)e.Current;
                    commandQueue.Add(() =>
                    {
                        Console.WriteLine("Adding skip-dirs setting...");
                        PrefSvc.AddSkipDirs(dirs);
                        Console.WriteLine("Done.");
                    });
                })
                .AddHandler("-clear-skip-dirs", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        Console.WriteLine("Clearing skip-dirs setting...");
                        PrefSvc.ClearSkipDirs();
                        Console.WriteLine("Done.");
                    });
                })
                .AddTranslation("-pe", "-print-extensions")
                .AddTranslation("-print-ext", "-print-extensions")
                .AddTranslation("-ext", "-add-extensions")
                .AddTranslation("-add-ext", "-add-extensions")
                .AddTranslation("-pd", "-print-skip-dirs")
                .AddTranslation("-skip", "-add-skip-dirs")
                .AddTranslation("-skip-dirs", "-add-skip-dirs")
                .AddTranslation("-u", "-username")
                .AddTranslation("-user", "-username")
                .AddTranslation("-p", "-password")
                .AddTranslation("-pass", "-password")
                .AddUsageHeader("Usage <configuration>:")
                .AddUsage("{0} -print-extensions", Program.Name)
                .AddUsage("{0} -add-extensions EXT[,EXT2...]", Program.Name)
                .AddUsage("{0} -clear-extensions", Program.Name)
                .AddUsage("{0} -print-skip-dirs", Program.Name)
                .AddUsage("{0} -add-skip-dirs DIR[,DIR2...]", Program.Name)
                .AddUsage("{0} -clear-skip-dirs", Program.Name)
                .AddOption("-index-api URL")
                .AddOption("-username USERNAME")
                .AddOption("-password PASSWORD");
        }
    }
}
