using com.hideakin.textsearch.utility;

namespace com.hideakin.textsearch.command
{
    internal class DebugCommand : ICommand
    {
        public void Register(CommandLine commandLine, CommandQueue commandQueue)
        {
            commandLine
                .AddHandler("-debug", (e) =>
                {
                    Debug.Level++;
                })
                .AddOption("-debug")
#if DEBUG
                .AddHandler("-debugger", (e) =>
                {
                    System.Diagnostics.Debugger.Launch();
                })
                .AddOption("-debugger")
#endif
                ;
        }
    }
}
