using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Debugging.Commands
{

    public abstract class DebugCommand
    {
        public abstract string Command { get; }

        public abstract void Execute();
    }

}