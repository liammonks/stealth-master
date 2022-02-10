using UnityEngine;

[CreateAssetMenu(fileName = "BulletStats", menuName = "stealth-master/BulletStats", order = 0)]
public class BulletStats : ScriptableObject {
    
    public float speed;
    public float range;
    public float damage;
    
    public static BulletStats Create(float a_speed, float a_maxDistance, float a_damage) 
    {
        BulletStats stats = ScriptableObject.CreateInstance(typeof(BulletStats)) as BulletStats;
        stats.speed = a_speed;
        stats.range = a_maxDistance;
        stats.damage = a_damage;
        return stats;
    }
}