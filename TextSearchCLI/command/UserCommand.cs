using com.hideakin.textsearch.service;
using com.hideakin.textsearch.utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace com.hideakin.textsearch.command
{
    internal class UserCommand : ICommand
    {
        private readonly CancellationTokenSource cts;
        private readonly UserService usr;

        public UserCommand()
        {
            cts = new CancellationTokenSource();
            usr = new UserService(cts.Token);
        }

        public void Register(CommandLine commandLine, CommandQueue commandQueue)
        {
            commandLine
                .AddHandler("-print-users", (e) =>
                {
                    commandQueue.Add(() =>
                    {
                        var users = usr.GetUsers();
                        foreach (var entry in users.OrderBy(x => x.Uid))
                        {
                            Console.WriteLine("{0}", entry);
                        }
                    });
                })
                .AddHandler("-print-user", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("User ID is not specified.");
                    }
                    if (Char.IsDigit(((string)e.Current)[0]))
                    {
                        int uid = int.Parse((string)e.Current);
                        commandQueue.Add(() =>
                        {
                            var entry = usr.GetUser(uid);
                            Console.WriteLine("{0}", entry);
                        });
                    }
                    else
                    {
                        var username = (string)e.Current;
                        commandQueue.Add(() =>
                        {
                            var entry = usr.GetUser(username);
                            Console.WriteLine("{0}", entry);
                        });
                    }
                })
                .AddHandler("-create-user", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("Username, password, and roles are not specified.");
                    }
                    var username = (string)e.Current;
                    if (!e.MoveNext())
                    {
                        throw new Exception("Password, and roles are not specified.");
                    }
                    var password = (string)e.Current;
                    if (!e.MoveNext())
                    {
                        throw new Exception("Role is not specified.");
                    }
                    var roles = new List<string>();
                    roles.MergeItems((string)e.Current);
                    commandQueue.Add(() =>
                    {
                        var entry = usr.CreateUser(username, password, roles.ToArray());
                        Console.WriteLine("Created. {0}", entry);
                    });
                })
                .AddHandler("-update-user", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("User ID is not specified.");
                    }
                    int uid = int.Parse((string)e.Current);
                    string username = null;
                    string password = null;
                    var roles = new List<string>();
                    var commandLine2 = new CommandLine();
                    commandLine2
                        .AddHandler("-username", (ee) =>
                        {
                            if (!ee.MoveNext())
                            {
                                throw new Exception("Username is not specified.");
                            }
                            username = (string)ee.Current;
                        })
                        .AddHandler("-password", (ee) =>
                        {
                            if (!ee.MoveNext())
                            {
                                throw new Exception("Password is not specified.");
                            }
                            password = (string)ee.Current;
                        })
                        .AddHandler("-roles", (ee) =>
                        {
                            if (!ee.MoveNext())
                            {
                                throw new Exception("Roles are not specified.");
                            }
                            roles.MergeItems((string)ee.Current);
                        })
                        .AddTranslation("-u", "-username")
                        .AddTranslation("-user", "-username")
                        .AddTranslation("-p", "-password")
                        .AddTranslation("-pass", "-password")
                        .AddTranslation("-r", "-roles")
                        .AddTranslation("-role", "-roles");
                    commandLine2.Parse(e);
                    commandQueue.Add(() =>
                    {
                        var entry = usr.UpdateUser(uid, username, password, roles.ToArray());
                        Console.WriteLine("Updated. {0}", entry);
                    });
                })
                .AddHandler("-delete-user", (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("User ID is not specified.");
                    }
                    int uid = int.Parse((string)e.Current);
                    commandQueue.Add(() =>
                    {
                        var entry = usr.DeleteUser(uid);
                        Console.WriteLine("Deleted. {0}", entry);
                    });
                })
                .AddTranslation("-pu", "-print-users")
                .AddUsageHeader("Usage <user>:")
                .AddUsage("{0} -print-users", Program.Name)
                .AddUsage("{0} -print-user UID", Program.Name)
                .AddUsage("{0} -print-user USERNAME", Program.Name)
                .AddUsage("{0} -create-user USERNAME PASSWORD [ROLE[,ROLE2...]]", Program.Name)
                .AddUsage("{0} -update-user UID [-username USERNAME] [-password PASSWORD] [-roles ROLE[,ROLE2...]]", Program.Name)
                .AddUsage("{0} -delete-user UID", Program.Name);
        }
    }
}
