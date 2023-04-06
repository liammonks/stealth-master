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

        /// <summary>
        /// Fetch all DebugCommand sub classes
        /// </summary>
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

        public void ExecuteCommand(string input)
        {
            List<string> split = input.Split(' ').ToList();
            string command = split[0];

            split.RemoveAt(0);
            string[] args = split.ToArray();

            if (m_DebugCommands.ContainsKey(command))
            {
                m_DebugCommands[command].Execute(args);
            }
            else
            {
                switch (args.Length)
                {
                    case 0:
                        ReadFlag(command);
                        break;
                    case 1:
                        SetFlag(command, args[0]);
                        break;
                }
            }
        }

        private void SetFlag(string flag, string value)
        {
            FieldInfo flagField = null;
            Type flagType = null;

            try
            {
                flagField = typeof(DebugFlags).GetField(flag);
                flagType = flagField.FieldType;
            }
            catch
            {
                Debug.LogError(
                    $"Failed to set flag {flag} to {value}\n" +
                    $"Flag was not found"
                );
                return;
            }

            if (flagType == typeof(string))
            {
                flagField.SetValue(null, value);
            }
            if (flagType == typeof(bool))
            {
                flagField.SetValue(null, bool.Parse(value));
            }
            if (flagType == typeof(int))
            {
                flagField.SetValue(null, int.Parse(value));
            }

            Debug.Log($"Flag {flag} set to {value}");
        }

        private void ReadFlag(string flag)
        {
            try
            {
                Debug.Log(typeof(DebugFlags).GetField(flag).GetValue(null));
            }
            catch
            {
                Debug.LogError("Cannot find flag with ID " + flag);
            }
        }

    }

}