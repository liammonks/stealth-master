using UnityEngine;

[CreateAssetMenu(fileName = "UnitStats", menuName = "stealth-master/UnitStats", order = 0)]
public class UnitStats : ScriptableObject {
    
    [Header("Stats")]
    public float walkSpeed = 3;
    public float runSpeed = 5;
    public float jumpForce = 6.0f;
    public float diveVelocityMultiplier = 0.75f;
    public float slideVelocityMultiplier = 0.25f;

    [Header("Physics")]
    public float groundAuthority = 12.0f;
    public float airAuthority = 2.0f;
    public float groundDrag = 8.0f;
    public float airDrag = 1.0f;
    public float terminalVeloicty = 55.56f;
    

    public UnitStats GetInstance()
    {
        UnitStats instance = ScriptableObject.CreateInstance<UnitStats>();
        // Stats
        instance.walkSpeed = this.walkSpeed;
        instance.runSpeed = this.runSpeed;
        instance.jumpForce = this.jumpForce;
        instance.diveVelocityMultiplier = this.diveVelocityMultiplier;

        // Physics
        instance.airAuthority = this.airAuthority;
        instance.airDrag = this.airDrag;
        instance.terminalVeloicty = this.terminalVeloicty;

        return instance;
    }
    
    public static UnitStats Interpolate(UnitStats from, UnitStats to, float t)
    {
        UnitStats instance = ScriptableObject.CreateInstance<UnitStats>();
        // Stats
        instance.walkSpeed = Mathf.Lerp(from.walkSpeed, to.walkSpeed, t);
        instance.runSpeed = Mathf.Lerp(from.runSpeed, to.runSpeed, t);
        instance.jumpForce = Mathf.Lerp(from.jumpForce, to.jumpForce, t);
        instance.diveVelocityMultiplier = Mathf.Lerp(from.diveVelocityMultiplier, to.diveVelocityMultiplier, t);

        // Physics
        instance.airAuthority = Mathf.Lerp(from.airAuthority, to.airAuthority, t);
        instance.airDrag = Mathf.Lerp(from.airDrag, to.airDrag, t);
        instance.terminalVeloicty = Mathf.Lerp(from.terminalVeloicty, to.terminalVeloicty, t);

        return instance;
    }
}