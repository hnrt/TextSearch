using com.hideakin.textsearch.model;
using com.hideakin.textsearch.service;
using com.hideakin.textsearch.utility;
using System;
using System.Collections.Generic;

namespace com.hideakin.textsearch.command
{
    internal class FileGroupCommand : ICommand
    {
        private FileGroupService FileGrpSvc { get; } = new FileGroupService();

        public void Register(CommandLine commandLine, CommandQueue commandQueue)
        {
            commandLine
                .AddHandler("-print-groups", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        var values = FileGrpSvc.GetFileGroups();
                        if (values != null)
                        {
                            foreach (FileGroupInfo entry in values)
                            {
                                Console.WriteLine("{0}", entry);
                            }
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
                    if (!e.MoveNext())
                    {
                        throw new Exception("Owner is not specified.");
                    }
                    var ownedBy = new List<string>();
                    ownedBy.MergeItems((string)e.Current);
                    commandQueue.Add(() =>
                    {
                        var entry = FileGrpSvc.CreateFileGroup(groupname, ownedBy.ToArray());
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
                    string groupname = null;
                    List<string> ownedBy = null;
                    var commandLine2 = new CommandLine();
                    commandLine2
                        .AddHandler("-groupname", (ee) =>
                        {
                            if (!ee.MoveNext())
                            {
                                throw new Exception("Group name is not specified.");
                            }
                            groupname = (string)ee.Current;
                        })
                        .AddHandler("-owned-by", (ee) =>
                        {
                            if (!ee.MoveNext())
                            {
                                throw new Exception("Roles are not specified.");
                            }
                            if (ownedBy == null)
                            {
                                ownedBy = new List<string>();
                            }
                            ownedBy.MergeItems((string)ee.Current);
                        })
                        .AddTranslation("-g", "-groupname")
                        .AddTranslation("-group", "-groupname")
                        .AddTranslation("-o", "-owned-by")
                        .AddTranslation("-owned", "-owned-by");
                    commandLine2.Parse(e);
                    commandQueue.Add(() =>
                    {
                        var entry = FileGrpSvc.UpdateFileGroup(gid, groupname, ownedBy != null ? ownedBy.ToArray() : null);
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
                        var entry = FileGrpSvc.DeleteFileGroup(gid);
                        Console.WriteLine("Deleted. {0}", entry);
                    });
                })
                .AddTranslation("-pg", "-print-groups")
                .AddUsageHeader("Usage <group>:")
                .AddUsage("{0} -print-groups", Program.Name)
                .AddUsage("{0} -create-group GROUPNAME OWNER[,OWNER2...]", Program.Name)
                .AddUsage("{0} -update-group GID [-groupname GROUPNAME] [-owned-by OWNER[,OWNER2...]]", Program.Name)
                .AddUsage("{0} -delete-group GID", Program.Name);
        }
    }
}
