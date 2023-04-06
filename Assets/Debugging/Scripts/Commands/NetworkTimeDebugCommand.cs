using Debugging.Commands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTimeDebugCommand : DebugCommand
{
    public override string Command => "draw_simulationTime";

    protected override void OnExecute(params string[] args)
    {
        if (args.Length == 0)
        {
            NetworkTimeDebugBehaviour.Instance.Toggle();
            return;
        }

        switch (args[0])
        {
            case "0":
                NetworkTimeDebugBehaviour.Instance.Disable();
                break;
            case "1":
                NetworkTimeDebugBehaviour.Instance.Enable();
                break;
        }
    }

}
