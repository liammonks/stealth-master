namespace Debugging.Commands
{

    public class DrawSimulationTimeVariation : DebugCommand
    {
        public override string Command => "draw_simulationTimeVariation";

        protected override void OnExecute(params string[] args)
        {
            // Validate
            if (args.Length < 1) { return; }

            // Log type
            switch (args[0])
            {
                case "0":
                    DebugBehaviour<Behaviours.DrawSimulationTimeVariation>.Instance.Disable();
                    DebugBehaviour<Behaviours.DrawSimulationTimeVariationGraph>.Instance.Disable();
                    break;
                case "1":
                    DebugBehaviour<Behaviours.DrawSimulationTimeVariation>.Instance.Enable();
                    DebugBehaviour<Behaviours.DrawSimulationTimeVariationGraph>.Instance.Disable();
                    break;
                case "2":
                    DebugBehaviour<Behaviours.DrawSimulationTimeVariation>.Instance.Disable();
                    DebugBehaviour<Behaviours.DrawSimulationTimeVariationGraph>.Instance.Enable();
                    break;
                default:
                    // LOG ERROR
                    break;
            }
        }
    }

}