using com.hideakin.textsearch.exception;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.command
{
    internal class DiagnosticsCommand : ICommand
    {
        private CancellationTokenSource cts = new CancellationTokenSource();

        public void Register(CommandLine commandLine, CommandQueue commandQueue)
        {
            commandLine
                .AddHandler("-print-id.next", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        var client = IndexApiClient.Create(cts.Token);
                        var task = client.GetIds();
                        task.Wait();
                        if (task.Result is IdStatus status)
                        {
                            Console.WriteLine("{0}", status);
                        }
                        else if (task.Result is Exception x)
                        {
                            throw x;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    });
                })
                .AddHandler("-reset-id.next", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        var client = IndexApiClient.Create(cts.Token);
                        var task = client.ResetIds();
                        task.Wait();
                        if (task.Result is IdStatus status)
                        {
                            Console.WriteLine("{0}", status);
                        }
                        else if (task.Result is Exception x)
                        {
                            throw x;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    });
                })
                .AddHandler("-print-unused-files", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        var client = IndexApiClient.Create(cts.Token);
                        var task = client.GetUnusedFiles();
                        task.Wait();
                        if (task.Result is FileInfo[] unused)
                        {
                            foreach (var info in unused)
                            {
                                Console.WriteLine("{0}", info);
                            }
                        }
                        else if (task.Result is Exception x)
                        {
                            throw x;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    });
                })
                .AddHandler("-delete-unused-files", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        var client = IndexApiClient.Create(cts.Token);
                        var task = client.DeleteUnusedFiles();
                        task.Wait();
                        if (task.Result is FileInfo[] deleted)
                        {
                            if (deleted.Length > 0)
                            {
                                Console.Write("Deleted: ");
                                foreach (var info in deleted)
                                {
                                    Console.WriteLine("{0}", info);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Unused files not found.");
                            }
                        }
                        else if (task.Result is Exception x)
                        {
                            throw x;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    });
                })
                .AddHandler("-print-unused-contents", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        var client = IndexApiClient.Create(cts.Token);
                        var task = client.GetUnusedContents();
                        task.Wait();
                        if (task.Result is int[] unused)
                        {
                            var sb = new StringBuilder();
                            foreach (int fid in unused)
                            {
                                sb.AppendFormat(" {0}", fid);
                            }
                            if (sb.Length > 0)
                            {
                                Console.WriteLine("{0}", sb.ToString(1, sb.Length - 1));
                            }
                        }
                        else if (task.Result is Exception x)
                        {
                            throw x;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    });
                })
                .AddHandler("-delete-unused-contents", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        var client = IndexApiClient.Create(cts.Token);
                        var task = client.DeleteUnusedContents();
                        task.Wait();
                        if (task.Result is int[] deleted)
                        {
                            if (deleted.Length > 0)
                            {
                                var sb = new StringBuilder();
                                foreach (int fid in deleted)
                                {
                                    sb.AppendFormat(" {0}", fid);
                                }
                                Console.WriteLine("Deleted:{0}", sb.ToString());
                            }
                            else
                            {
                                Console.WriteLine("Unused contents not found.");
                            }
                        }
                        else if (task.Result is Exception x)
                        {
                            throw x;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    });
                })
                .AddHandler("-print-index-stats", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("Group name is not specified.");
                    }
                    var groupname = (string)e.Current;
                    commandQueue.Add(() =>
                    {
                        var client = IndexApiClient.Create(cts.Token);
                        var task = client.GetIndexStats(groupname);
                        task.Wait();
                        if (task.Result is IndexStats stats)
                        {
                            Console.WriteLine("{0}", stats);
                        }
                        else if (task.Result is Exception x)
                        {
                            throw x;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    });
                })
                .AddUsageHeader("Usage <diagnostics>:")
                .AddUsage("{0} -print-id.next", Program.Name)
                .AddUsage("{0} -reset-id.next", Program.Name)
                .AddUsage("{0} -print-unused-files", Program.Name)
                .AddUsage("{0} -delete-unused-files", Program.Name)
                .AddUsage("{0} -print-unused-contents", Program.Name)
                .AddUsage("{0} -delete-unused-contents", Program.Name)
                .AddUsage("{0} -print-index-stats GROUPNAME", Program.Name);
        }
    }
}
