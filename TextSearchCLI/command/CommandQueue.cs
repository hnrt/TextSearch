using System;
using System.Collections.Generic;

namespace com.hideakin.textsearch.command
{
    internal class CommandQueue
    {
        private List<Action> Actions { get; } = new List<Action>();

        public CommandQueue()
        {
        }

        public CommandQueue Add(Action action)
        {
            Actions.Add(action);
            return this;
        }

        public void Run()
        {
            foreach (var action in Actions)
            {
                action();
            }
        }
    }
}
