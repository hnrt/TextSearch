using System;

namespace com.hideakin.textsearch.command
{
    internal class HelpCommand : ICommand
    {
        private static readonly string HELP = "-help";

        public void Register(CommandLine commandLine, CommandQueue commandQueue)
        {
            commandLine
                .AddHandler(HELP, (e) =>
                {
                    commandLine.PutUsage();
                    Environment.Exit(0);
                })
                .AddTranslation("-h", HELP)
                .AddTranslation("-?", HELP);
        }
    }
}
