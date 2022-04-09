using UnityEngine;

namespace Gadgets.GrapplingHook
{
    [CreateAssetMenu(fileName = "IncreaseProjectileSpeed", menuName = "GadgetMods/GrapplingHook/IncreaseProjectileSpeed", order = 0)]
    public class IncreaseProjectileSpeed : GadgetMod
    {
        public float add = 0.0f;
        public float scalar = 2.0f;
        private GrappleHook grapplingHook;

        public override void Activate(BaseGadget gadget)
        {
            grapplingHook = gadget as GrappleHook;
            grapplingHook.bulletStats.speed += add;
            grapplingHook.bulletStats.speed *= scalar;
        }
    }
}