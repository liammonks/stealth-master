using UnityEngine;

namespace Gadgets.GrapplingHook
{
    [CreateAssetMenu(fileName = "IncreaseReelRate", menuName = "GadgetMods/GrapplingHook/IncreaseReelRate", order = 0)]
    public class IncreaseReelRate : GadgetMod
    {
        public float add = 0.0f;
        public float scalar = 2.0f;
        private GrappleHook grapplingHook;

        public override void Activate(BaseGadget gadget)
        {
            grapplingHook = gadget as GrappleHook;
            grapplingHook.reelRate += add;
            grapplingHook.reelRate *= scalar;
        }
    }
}