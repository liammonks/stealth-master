namespace Debugging.Commands
{

    public class DrawLatency : DebugCommand
    {
        public override string Command => "draw_latency";

        protected override void OnExecute(params string[] args)
        {
            // Validate
            if (args.Length < 1) { return; }

            // Log type
            switch (args[0])
            {
                case "0":
                    DebugBehaviour<Behaviours.DrawLatency>.Instance.Disable();
                    DebugBehaviour<Behaviours.DrawLatencyGraph>.Instance.Disable();
                    break;
                case "1":
                    DebugBehaviour<Behaviours.DrawLatency>.Instance.Enable();
                    DebugBehaviour<Behaviours.DrawLatencyGraph>.Instance.Disable();
                    break;
                case "2":
                    DebugBehaviour<Behaviours.DrawLatency>.Instance.Disable();
                    DebugBehaviour<Behaviours.DrawLatencyGraph>.Instance.Enable();
                    break;
                default:
                    // LOG ERROR
                    break;
            }
        }
    }

}