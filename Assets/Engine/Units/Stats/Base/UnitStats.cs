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
    public float airAuthority = 2.0f;
    public float jumpForce = 6.0f;
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
}