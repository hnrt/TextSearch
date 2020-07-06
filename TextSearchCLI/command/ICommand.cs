using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.command
{
    internal interface ICommand
    {
        void Register(CommandLine commandLine, CommandQueue commandQueue);
    }
}
