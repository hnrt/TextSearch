using com.hideakin.textsearch.service;
using System;
using System.Threading;

namespace com.hideakin.textsearch.command
{
    internal class AuthenticateCommand : ICommand
    {
        private static readonly string AUTHENTICATE = "-authenticate";

        private readonly CancellationTokenSource cts;
        private readonly UserService usr;

        public AuthenticateCommand()
        {
            cts = new CancellationTokenSource();
            usr = new UserService(cts.Token);
        }

        public void Register(CommandLine commandLine, CommandQueue commandQueue)
        {
            commandLine
                .AddHandler(AUTHENTICATE, (e) =>
                {
                    if (!e.MoveNext())
                    {
                        throw new Exception("Username/Password are not specified.");
                    }
                    var s = (string)e.Current;
                    var pos = s.IndexOf('/');
                    if (pos < 0)
                    {
                        throw new Exception("Username/Password are not specified.");
                    }
                    var username = s.Substring(0, pos);
                    var password = s.Substring(pos + 1);
                    commandQueue.Add(() =>
                    {
                        var response = usr.Authenticate(username, password);
                        Console.WriteLine("OK. {0}", response.AccessToken);
                    });
                })
                .AddTranslation("-a", AUTHENTICATE)
                .AddTranslation("-auth", AUTHENTICATE)
                .AddUsageHeader("Usage <authentication>:")
                .AddUsage("{0} {1} USERNAME[/PASSWORD]", Program.Name, AUTHENTICATE);
        }
    }
}
