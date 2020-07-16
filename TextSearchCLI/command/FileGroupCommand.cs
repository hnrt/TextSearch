using com.hideakin.textsearch.model;
using com.hideakin.textsearch.service;
using System;
using System.Linq;
using System.Threading;

namespace com.hideakin.textsearch.command
{
    internal class FileGroupCommand : ICommand
    {
        private readonly CancellationTokenSource cts;
        private readonly FileGroupService grp;

        public FileGroupCommand()
        {
            cts = new CancellationTokenSource();
            grp = new FileGroupService(cts.Token);
        }

        public void Register(CommandLine commandLine, CommandQueue commandQueue)
        {
            commandLine
                .AddHandler("-print-groups", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        var values = grp.GetFileGroups();
                        foreach (FileGroupInfo entry in values.OrderBy(x => x.Gid))
                        {
                            Console.WriteLine("{0}", entry);
                        }
                    });
                })
                .AddHandler("-create-group", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("Group name is not specified.");
                    }
                    var groupname = (string)e.Current;
                    commandQueue.Add(() =>
                    {
                        var entry = grp.CreateFileGroup(groupname);
                        Console.WriteLine("Created. {0}", entry);
                    });
                })
                .AddHandler("-update-group", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("Group ID is not specified.");
                    }
                    int gid = int.Parse((string)e.Current);
                    if (!e.MoveNext())
                    {
                        throw new Exception("Group name is not specified.");
                    }
                    string groupname = (string)e.Current;
                    commandQueue.Add(() =>
                    {
                        var entry = grp.UpdateFileGroup(gid, groupname);
                        Console.WriteLine("Updated. {0}", entry);
                    });
                })
                .AddHandler("-delete-group", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("User ID is not specified.");
                    }
                    int gid = int.Parse((string)e.Current);
                    commandQueue.Add(() =>
                    {
                        var entry = grp.DeleteFileGroup(gid);
                        Console.WriteLine("Deleted. {0}", entry);
                    });
                })
                .AddTranslation("-pg", "-print-groups")
                .AddUsageHeader("Usage <group>:")
                .AddUsage("{0} -print-groups", Program.Name)
                .AddUsage("{0} -create-group GROUPNAME", Program.Name)
                .AddUsage("{0} -update-group GID GROUPNAME", Program.Name)
                .AddUsage("{0} -delete-group GID", Program.Name);
        }
    }
}
