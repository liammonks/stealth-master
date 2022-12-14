namespace Debugging.Commands
{

    public class DrawSimulationTime : DebugCommand
    {
        public override string Command => "draw_simulationTime";

        protected override void OnExecute(params string[] args)
        {
            // Validate
            if (args.Length < 1) { return; }

            // Log type
            switch (args[0])
            {
                case "0":
                    DebugBehaviour<Behaviours.DrawSimulationTime>.Instance.Disable();
                    break;
                case "1":
                    DebugBehaviour<Behaviours.DrawSimulationTime>.Instance.Enable();
                    break;
                default:
                    // LOG ERROR
                    break;
            }
        }
    }

}