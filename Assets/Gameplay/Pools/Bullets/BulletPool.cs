using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletPool : MonoBehaviour
{
    public class BulletHitData
    {
        
    }
    public static BulletPool Instance;

    private static IObjectPool<Bullet> bulletPool;

    [SerializeField] private Bullet bulletPrefab;
    
    public bool collectionChecks = true;
    private const int bulletCount = 10;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Two Instances of BulletPool Found");
            return;
        }
        Instance = this;
        bulletPool = new ObjectPool<Bullet>(CreateBullet, GetBullet, ReleaseBullet, DestroyBullet, collectionChecks, bulletCount);
    }

    public static Bullet Fire(Vector2 position, Vector2 direction, Vector2 parentVelocity, BulletStats stats, bool isPlayer)
    {
        Bullet bullet = bulletPool.Get();
        bullet.Fire(position, direction, parentVelocity, stats, isPlayer);
        return bullet;
    }
    
    public static void Release(Bullet bullet)
    {
        bulletPool.Release(bullet);
    }
    
    private Bullet CreateBullet()
    {
        return Instantiate(bulletPrefab, transform);
    }

    // Called when an item is taken from the pool using Get
    private void GetBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(true);
    }

    // Called when an item is returned to the pool using Release
    private void ReleaseBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    private void DestroyBullet(Bullet bullet)
    {
        Destroy(bullet.gameObject);
    }
    
}
