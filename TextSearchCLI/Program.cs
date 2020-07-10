using com.hideakin.textsearch.command;
using System;

namespace com.hideakin.textsearch
{
    class Program
    {
        public static string Name { get; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        private ICommand[] Commands { get; } =
        {
            new HelpCommand(),
            new AuthenticateCommand(),
            new UserCommand(),
            new FileGroupCommand(),
            new FileCommand(),
            new SearchCommand(),
            new ConfigurationCommand(),
            new DebugCommand()
        };

        private CommandLine CommandLine { get; } = new CommandLine();

        private CommandQueue Scheduler { get; } = new CommandQueue();

        public Program()
        {
            foreach (var command in Commands)
            {
                command.Register(CommandLine, Scheduler);
            }
        }

        public void Run(string[] args)
        {
            if (args.Length == 0)
            {
                CommandLine.PutUsage();
                return;
            }
            CommandLine.Parse(args);
            Scheduler.Run();
        }

        static void Main(string[] args)
        {
            try 
            {
                var app = new Program();
                app.Run(args);
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("ERROR: {0}", e.Message);
                for (e = e.InnerException; e != null; e = e.InnerException)
                {
                    Console.Error.WriteLine("\t{0}", e.Message);
                }
            }
            Environment.Exit(1);
        }
    }
}
