using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Debugging.Commands
{

    public abstract class DebugCommand
    {
        public abstract string Command { get; }

        public void Execute(params string[] args)
        {
            OnExecute(args);
        }

        protected abstract void OnExecute(params string[] args);

    }

}