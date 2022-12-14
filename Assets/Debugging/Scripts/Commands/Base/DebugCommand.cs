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
            try
            {
                OnExecute(args);
            }
            catch
            {
                LogError();
            }
        }

        protected abstract void OnExecute(params string[] args);

        protected virtual void LogError()
        {
            Debug.LogError($"Error while executing command // {Command}");
        }
    }

}