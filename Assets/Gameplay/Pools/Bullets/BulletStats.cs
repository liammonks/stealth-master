using UnityEngine;

[CreateAssetMenu(fileName = "BulletStats", menuName = "stealth-master/BulletStats", order = 0)]
public class BulletStats : ScriptableObject {
    public float speed;
    public float maxDistance;
    public float damage;
}