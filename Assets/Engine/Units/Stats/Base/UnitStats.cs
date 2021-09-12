using UnityEngine;

[CreateAssetMenu(fileName = "UnitStats", menuName = "stealth-master/UnitStats", order = 0)]
public class UnitStats : ScriptableObject {
    
    [Header("Stats")]
    public float walkSpeed = 3;
    public float runSpeed = 5;
    public float jumpForce = 6.0f;
    public Vector2 diveVelocityMultiplier = new Vector2(0.5f, 0.5f);

    [Header("Climbing")]
    public float vaultHeight = 0.5f;
    public float stepHeight = 1.0f;
    [Range(0.0f, 1.0f)]
    public float vaultRate = 0.1f, stepRate = 0.1f;
    
    [Header("Physics")]
    public Vector2 size = new Vector2(0.5f, 1.7f);
    public float feetSeperation = 0.8f;
    public float groundAuthority = 12.0f;
    public float airAuthority = 2.0f;
    public float groundDrag = 8.0f;
    public float airDrag = 1.0f;
    public float terminalVeloicty = 55.56f;
    
    private void OnValidate() {
        vaultHeight = Mathf.Max(stepHeight, vaultHeight); // Vault height should always be larger or equal to step height
        stepHeight = Mathf.Min(stepHeight, vaultHeight); // Step height should always be smaller or equal to vault height
    }
    
    public UnitStats GetInstance()
    {
        UnitStats instance = ScriptableObject.CreateInstance<UnitStats>();
        // Stats
        instance.walkSpeed = this.walkSpeed;
        instance.runSpeed = this.runSpeed;
        instance.jumpForce = this.jumpForce;
        instance.diveVelocityMultiplier = this.diveVelocityMultiplier;
        instance.vaultHeight = this.vaultHeight;
        instance.vaultRate = this.vaultRate;
        instance.stepHeight = this.stepHeight;
        instance.stepRate = this.stepRate;

        // Physics
        instance.size = this.size;
        instance.feetSeperation = this.feetSeperation;
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
        instance.diveVelocityMultiplier = Vector2.Lerp(from.diveVelocityMultiplier, to.diveVelocityMultiplier, t);
        instance.vaultHeight = Mathf.Lerp(from.vaultHeight, to.vaultHeight, t);
        instance.vaultRate = Mathf.Lerp(from.vaultRate, to.vaultRate, t);
        instance.stepHeight = Mathf.Lerp(from.stepHeight, to.stepHeight, t);
        instance.stepRate = Mathf.Lerp(from.stepRate, to.stepRate, t);
        
        // Physics
        instance.size = Vector2.Lerp(from.size, to.size, t);
        instance.feetSeperation = Mathf.Lerp(from.feetSeperation, to.feetSeperation, t);
        instance.airAuthority = Mathf.Lerp(from.airAuthority, to.airAuthority, t);
        instance.airDrag = Mathf.Lerp(from.airDrag, to.airDrag, t);
        instance.terminalVeloicty = Mathf.Lerp(from.terminalVeloicty, to.terminalVeloicty, t);

        return instance;
    }
}