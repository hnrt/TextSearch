using com.hideakin.textsearch.input;
using com.hideakin.textsearch.net;
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
                    string username;
                    string password;
                    var s = (string)e.Current;
                    var pos = s.IndexOf('/');
                    if (pos > 0)
                    {
                        username = s.Substring(0, pos);
                        password = s.Substring(pos + 1);
                    }
                    else if (pos == 0 || s.Length == 0)
                    {
                        throw new Exception("Empty username is not allowed.");
                    }
                    else
                    {
                        username = s;
                        Console.Write("Password: ");
                        password = Keyboard.GetPassword();
                    }
                    commandQueue.Add(() =>
                    {
                        var response = usr.Authenticate(username, password);
                        Console.WriteLine("OK. {0}", response.AccessToken);
                    });
                })
                .AddHandler("-print-myself", (e) =>
                {
                    Console.WriteLine("{0}", IndexApiClient.Credentials.Username ?? "Not specified.");
                })
                .AddTranslation("-a", AUTHENTICATE)
                .AddTranslation("-auth", AUTHENTICATE)
                .AddUsageHeader("Usage <authentication>:")
                .AddUsage("{0} {1} USERNAME[/PASSWORD]", Program.Name, AUTHENTICATE)
                .AddUsage("{0} -print-myself", Program.Name);
        }
    }
}
