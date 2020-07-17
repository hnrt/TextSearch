using com.hideakin.textsearch.net;
using com.hideakin.textsearch.service;
using System;
using System.Threading;

namespace com.hideakin.textsearch.command
{
    internal class ConfigurationCommand : ICommand
    {
        private readonly CancellationTokenSource cts;
        private readonly PreferenceService pref;

        public ConfigurationCommand()
        {
            cts = new CancellationTokenSource();
            pref = new PreferenceService(cts.Token);
        }

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
                    IndexApiClient.Url = a;
                })
                .AddHandler("-username", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("Username is not specified.");
                    }
                    IndexApiClient.ChangeUser((string)e.Current);
                })
                .AddHandler("-print-extensions", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        var list = pref.GetExtensions();
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
                        var res = pref.AddExtensions(exts);
                        Console.WriteLine("{0}", res);
                    });
                })
                .AddHandler("-clear-extensions", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        Console.WriteLine("Clearing extensions setting...");
                        pref.ClearExtensions();
                        Console.WriteLine("Done.");
                    });
                })
                .AddHandler("-print-skip-dirs", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        var list = pref.GetSkipDirs();
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
                        pref.AddSkipDirs(dirs);
                        Console.WriteLine("Done.");
                    });
                })
                .AddHandler("-clear-skip-dirs", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        Console.WriteLine("Clearing skip-dirs setting...");
                        pref.ClearSkipDirs();
                        Console.WriteLine("Done.");
                    });
                })
                .AddTranslation("-u", "-username")
                .AddTranslation("-pe", "-print-extensions")
                .AddTranslation("-print-ext", "-print-extensions")
                .AddTranslation("-ext", "-add-extensions")
                .AddTranslation("-add-ext", "-add-extensions")
                .AddTranslation("-pd", "-print-skip-dirs")
                .AddTranslation("-skip", "-add-skip-dirs")
                .AddTranslation("-skip-dirs", "-add-skip-dirs")
                .AddUsageHeader("Usage <configuration>:")
                .AddUsage("{0} -print-extensions", Program.Name)
                .AddUsage("{0} -add-extensions EXT[,EXT2...]", Program.Name)
                .AddUsage("{0} -clear-extensions", Program.Name)
                .AddUsage("{0} -print-skip-dirs", Program.Name)
                .AddUsage("{0} -add-skip-dirs DIR[,DIR2...]", Program.Name)
                .AddUsage("{0} -clear-skip-dirs", Program.Name)
                .AddOption("-index-api URL")
                .AddOption("-username USERNAME");
        }
    }
}
