using UnityEngine;

public abstract class MoveState : ScriptableObject {
    
    public abstract MoveState Initialise(UnitData data, Animator animator);

    public abstract MoveState Execute(UnitData data, Animator animator);
    
}