using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitHelper : MonoBehaviour
{
    public static UnitHelper Instance;
    public static Player Player;
    public static Interactable[] Interactables;

    [SerializeField] private ParticleSystem groundParticlePrefab;

    private Queue<ParticleSystem> groundParticles;

    private void Awake() {
        if(Instance != null)
        {
            Debug.LogError("Two instances of UnitHelper found");
            return;
        }
        Instance = this;

        // Spawn ground particle pool
        int particleCount = Mathf.CeilToInt((groundParticlePrefab.main.duration + groundParticlePrefab.main.startLifetime.constant) / Time.fixedDeltaTime) + 1;
        groundParticles = new Queue<ParticleSystem>(particleCount);
        for (int i = 0; i < particleCount; ++i)
        {
            groundParticles.Enqueue(Instantiate(groundParticlePrefab, transform));
        }
        Player = FindObjectOfType<Player>();
        Interactables = FindObjectsOfType<Interactable>();
    }
    
    public void EmitGroundParticles(Vector3 position, Vector3 direction) 
    {
        if (groundParticles.Count == 0) { return; }
        ParticleSystem ps = groundParticles.Dequeue();
        ps.transform.position = position;
        ps.transform.rotation = Quaternion.LookRotation(direction, Vector3.forward * Vector3.Dot(direction, Vector3.right));
        ps.gameObject.SetActive(true);
        ps.Play();
        StartCoroutine(EnqueueParticleSystem(groundParticles, ps));
    }
    
    private IEnumerator EnqueueParticleSystem(Queue<ParticleSystem> psQueue, ParticleSystem ps)
    {
        yield return new WaitForSeconds(groundParticlePrefab.main.duration + groundParticlePrefab.main.startLifetime.constant);
        ps.gameObject.SetActive(false);
        psQueue.Enqueue(ps);
    }

    private static uint availableUnitID = 0;
    public static uint GetUnitID() {
        return availableUnitID++;
    }
}
