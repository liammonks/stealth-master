using UnityEngine;

[CreateAssetMenu(fileName = "AIStats", menuName = "stealth-master/AIStats", order = 0)]
public class AIStats : ScriptableObject {
    
    [Header("Variable")]
    public float fear = 0;
    public float hunger = 50;
    public float thirst = 100;
    
    [Header("Const")]
    public float maxFallDistance = 1.0f;
    
    public AIStats CloneVariation(float variation)
    {
        AIStats clone = Instantiate(this);
        clone.fear += Random.Range(-5, 5) * variation;
        clone.hunger += Random.Range(-10, 10) * variation;
        clone.thirst += Random.Range(-20, 20) * variation;
        
        clone.maxFallDistance += Random.Range(-0.5f, 0.5f) * variation;
        return clone;
    }
}