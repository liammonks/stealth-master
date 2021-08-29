using UnityEngine;

[CreateAssetMenu(fileName = "UnitStats", menuName = "stealth-master/UnitStats", order = 0)]
public class UnitStats : ScriptableObject {
    
    [Header("Stats")]
    public float walkSpeed = 3;
    public float runSpeed = 5;
    public float climbHeight = 0.5f;
    [Range(0.0f, 1.0f)] public float climbRate = 0.1f;

    [Header("Physics")]
    public Vector2 size = new Vector2(0.5f, 1.7f);
    public float feetSeperation = 0.8f;
    public float groundAuthority = 12.0f;
    public float airAuthority = 2.0f;
    public float jumpForce = 6.0f;
    public float groundDrag = 8.0f;
    public float airDrag = 1.0f;
    public float terminalVeloicty = 55.56f;
    
    public UnitStats GetInstance()
    {
        UnitStats instance = ScriptableObject.CreateInstance<UnitStats>();
        // Stats
        instance.walkSpeed = this.walkSpeed;
        instance.runSpeed = this.runSpeed;
        instance.climbHeight = this.climbHeight;
        // Physics
        instance.size = this.size;
        instance.feetSeperation = this.feetSeperation;
        instance.climbRate = this.climbRate;
        instance.airAuthority = this.airAuthority;
        instance.jumpForce = this.jumpForce;
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
        instance.climbHeight = Mathf.Lerp(from.climbHeight, to.climbHeight, t);
        // Physics
        instance.size = Vector2.Lerp(from.size, to.size, t);
        instance.feetSeperation = Mathf.Lerp(from.feetSeperation, to.feetSeperation, t);
        instance.climbRate = Mathf.Lerp(from.climbRate, to.climbRate, t);
        instance.airAuthority = Mathf.Lerp(from.airAuthority, to.airAuthority, t);
        instance.jumpForce = Mathf.Lerp(from.jumpForce, to.jumpForce, t);
        instance.airDrag = Mathf.Lerp(from.airDrag, to.airDrag, t);
        instance.terminalVeloicty = Mathf.Lerp(from.terminalVeloicty, to.terminalVeloicty, t);

        return instance;
    }
}