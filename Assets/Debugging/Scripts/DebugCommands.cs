using Debugging.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Debugging
{

    public class DebugCommands : MonoBehaviour
    {
        public List<string> Commands => m_DebugCommands.Keys.ToList();

        private Dictionary<string, DebugCommand> m_DebugCommands = new Dictionary<string, DebugCommand>();

        private void Awake()
        {
            Type[] debugCommandTypes = Assembly.GetAssembly(typeof(DebugCommand)).GetTypes()
                .Where(x => x.IsSubclassOf(typeof(DebugCommand))).ToArray();

            foreach (Type type in debugCommandTypes)
            {
                DebugCommand debugCommand = Activator.CreateInstance(type) as DebugCommand;
                m_DebugCommands.Add(debugCommand.Command, debugCommand);
            }
        }

        public void ExecuteCommand(string command)
        {
            if (!m_DebugCommands.ContainsKey(command)) { return; }
            m_DebugCommands[command].Execute();
        }


    }

}