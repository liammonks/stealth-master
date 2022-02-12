using UnityEngine;

namespace Gadgets.GrapplingHook
{
    [CreateAssetMenu(fileName = "IncreaseProjectileRange", menuName = "GadgetMods/GrapplingHook/IncreaseProjectileRange", order = 0)]
    public class IncreaseProjectileRange : GadgetMod
    {
        public float add = 5.0f;
        public float scale = 1.0f;
        private GrappleHook grapplingHook;

        public override void Activate(BaseGadget gadget)
        {
            grapplingHook = gadget as GrappleHook;
            grapplingHook.bulletStats.range += add;
            grapplingHook.bulletStats.range *= scale;
        }
    }
}