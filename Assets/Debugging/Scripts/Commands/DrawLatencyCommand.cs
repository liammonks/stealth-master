using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debugging.Behaviours;

namespace Debugging.Commands
{

    public class DrawLatencyCommand : DebugCommand
    {
        public override string Command => "draw_latency";

        public override void Execute()
        {
            DrawLatencyBehaviour.Instance.Toggle();
        }
    }

}