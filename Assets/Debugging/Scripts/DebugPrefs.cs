using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Debugging
{

    public class DebugPrefs : MonoBehaviour
    {
        [SerializeField]
        private TextAsset m_AutoExec;

        private DebugCommands m_Commands;

        private void Start()
        {
            m_Commands = GetComponent<DebugCommands>();

            string[] lines = m_AutoExec.text.Split('\n');
            foreach (string line in lines)
            {
                if (line == string.Empty) { continue; }
                m_Commands.ExecuteCommand(line);
            }
        }
    }

}