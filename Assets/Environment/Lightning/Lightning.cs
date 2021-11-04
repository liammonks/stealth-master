using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    private static Vector3 rootPosition = new Vector3(0, 15, 20);
    private const float lightningThickness = 0.05f;
    private const float heightMin = 2.0f, heightMax = 4.0f;
    private const float angleMin = -30.0f, angleMax = 30.0f;
    private const int sectionCount = 10;
    private const float forkChance = 1.0f;
    
    private struct LightningSection
    {
        public GameObject obj;
        public Vector3 childAnchor;

        // Root constructor
        public LightningSection(GameObject obj, Vector3 spawnPosition, float height, float angle)
        {
            this.obj = obj;

            obj.transform.position = spawnPosition;
            obj.transform.localScale = new Vector3(lightningThickness, height, 1.0f);
            obj.transform.rotation = Quaternion.Euler(0, 0, angle);
            childAnchor = obj.transform.TransformPoint(Vector3.down);
        }

        // Child constructor
        public LightningSection(GameObject obj, LightningSection parent, float height, float angle, bool isForked = false)
        {
            this.obj = obj;

            obj.transform.position = parent.childAnchor;
            // Forked lightning should be shorter and thinner
            float thickness = isForked ? lightningThickness * 0.5f : lightningThickness;
            height = isForked ? height * 0.5f : height;
            obj.transform.localScale = new Vector3(thickness, height, 1.0f);
            obj.transform.rotation = Quaternion.Euler(0, 0, angle);
            childAnchor = obj.transform.TransformPoint(Vector3.down);
        }
    }
    
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private GameObject directionalLight;

    private List<LightningSection> mainSections = new List<LightningSection>();
    private List<LightningSection> forkedSections = new List<LightningSection>();
    
    void Start()
    {
        StartCoroutine(Storm());
    }
    
    private IEnumerator Storm()
    {
        Vector3 spawnPosition = rootPosition + (Vector3.right * Random.Range(-30.0f, 30.0f)) + (Vector3.right * Camera.main.transform.position.x);
        StartCoroutine(Strike(spawnPosition));
        yield return new WaitForSeconds(0.15f);
        StartCoroutine(Strike(spawnPosition));
        yield return new WaitForSeconds(Random.Range(6.0f, 12.0f));
        StartCoroutine(Storm());
    }

    private IEnumerator Strike(Vector3 spawnPosition)
    {
        // Spawn root section
        GameObject lightningObj = Instantiate(lightningPrefab, transform);
        LightningSection section = new LightningSection(lightningObj, spawnPosition, Random.Range(heightMin, heightMax), Random.Range(angleMin, angleMax));
        mainSections.Add(section);
        // Setup directional light
        directionalLight.transform.position = spawnPosition;
        directionalLight.transform.LookAt(Camera.main.transform);
        directionalLight.SetActive(true);
        // Spawn children
        float fChance = forkChance;
        for (int i = 0; i < sectionCount; ++i)
        {
            // Spawn child section
            lightningObj = Instantiate(lightningPrefab, transform);
            section = new LightningSection(lightningObj, mainSections[i], Random.Range(heightMin, heightMax), Random.Range(angleMin, angleMax));
            mainSections.Add(section);
            // Fork chance
            if(Random.Range(0.0f, 1.0f) <= fChance)
            {
                lightningObj = Instantiate(lightningPrefab, transform);
                section = new LightningSection(lightningObj, mainSections[i], Random.Range(heightMin, heightMax), Random.Range(angleMin, angleMax) * 1.5f, true);
                forkedSections.Add(section);
                // Fork chance
                if (Random.Range(0.0f, 1.0f) <= fChance * 0.75f)
                {
                    lightningObj = Instantiate(lightningPrefab, transform);
                    section = new LightningSection(lightningObj, section, Random.Range(heightMin, heightMax), Random.Range(angleMin, angleMax) * 1.75f, true);
                    forkedSections.Add(section);
                    // Fork chance
                    if (Random.Range(0.0f, 1.0f) <= fChance * 0.5f)
                    {
                        lightningObj = Instantiate(lightningPrefab, transform);
                        section = new LightningSection(lightningObj, section, Random.Range(heightMin, heightMax), Random.Range(angleMin, angleMax) * 2.0f, true);
                        forkedSections.Add(section);
                        // Fork chance
                        if (Random.Range(0.0f, 1.0f) <= fChance * 0.25f)
                        {
                            lightningObj = Instantiate(lightningPrefab, transform);
                            section = new LightningSection(lightningObj, section, Random.Range(heightMin, heightMax), Random.Range(angleMin, angleMax) * 2.25f, true);
                            forkedSections.Add(section);
                        }
                    }
                }
            }
            fChance = fChance * 0.9f;
        }
        yield return new WaitForSeconds(0.1f);
        
        // Destroy Lightning
        foreach(LightningSection ls in mainSections)
        {
            Destroy(ls.obj);
        }
        foreach (LightningSection ls in forkedSections)
        {
            Destroy(ls.obj);
        }
        mainSections.Clear();
        forkedSections.Clear();
        directionalLight.SetActive(false);
    }
}
