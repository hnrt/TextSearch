using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using com.hideakin.textsearch.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.command
{
    internal class AuthenticateCommand : ICommand
    {
        private static readonly string AUTHENTICATE = "-authenticate";

        private UserService UserSvc { get; } = new UserService();

        public AuthenticateCommand()
        {
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
                        var result = UserSvc.Authenticate(username, password);
                        if (result is AuthenticateResponse a)
                        {
                            Console.WriteLine("OK. {0}", a.AccessToken);
                        }
                        else if (result is ErrorResponse x)
                        {
                            Console.WriteLine("ERROR: {0}", x.ErrorDescription);
                        }
                    });
                })
                .AddTranslation("-a", AUTHENTICATE)
                .AddTranslation("-auth", AUTHENTICATE)
                .AddUsageHeader("Usage <authentication>:")
                .AddUsage("{0} {1} USERNAME[/PASSWORD]", Program.Name, AUTHENTICATE);
        }
    }
}
