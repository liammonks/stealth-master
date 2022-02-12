using UnityEngine;
using Gadgets;

public abstract class GadgetMod : ScriptableObject
{
    public abstract void Activate(BaseGadget gadget);
}
