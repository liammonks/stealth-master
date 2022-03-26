using UnityEngine;

public interface ITakeDamage
{
    public void TakeDamage(float damage);
    public void TakeDamage(Vector2 impact);
}