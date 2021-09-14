using UnityEngine;

[System.Serializable]
public class UnitColliderData
{
    [Header("Physics")]
    public Vector2 size = new Vector2(0.5f, 1.7f);
    public float feetSeperation = 0.4f;

    [Header("Climbing")]
    public float vaultHeight = 0.5f;
    public float stepHeight = 1.0f;
    [Range(0.0f, 1.0f)]
    public float vaultRate = 0.1f, stepRate = 0.1f, collisionRate = 0.3f;
    
    public UnitColliderData CreateInstance()
    {
        UnitColliderData instance = new UnitColliderData();
        instance.size = size;
        instance.feetSeperation = feetSeperation;
        instance.vaultHeight = vaultHeight;
        instance.stepHeight = stepHeight;
        instance.vaultRate = vaultRate;
        instance.stepRate = stepRate;
        instance.collisionRate = collisionRate;
        return instance;
    }
}

[CreateAssetMenu(fileName = "UnitCollider", menuName = "stealth-master/UnitCollider", order = 0)]
public class UnitCollider : ScriptableObject
{

    [SerializeField] private UnitColliderData standing, crawling;
    private UnitColliderData activeData;

    public Vector2 size { get { return activeData.size; } }
    public float feetSeperation { get { return activeData.feetSeperation; } }

    public float vaultHeight { get { return activeData.vaultHeight; } }
    public float stepHeight { get { return activeData.stepHeight; } }
    public float vaultRate { get { return activeData.vaultRate; } }
    public float stepRate { get { return activeData.stepRate; } }
    public float collisionRate { get { return activeData.collisionRate; } }

    private float lerp = 0.0f;

    private void OnValidate()
    {
        // Default to standing data
        if (standing != null)
        {
            activeData = standing.CreateInstance();
        }
    }
    
    public void SetStanding()
    {
        activeData = standing.CreateInstance();
    }
    public void SetCrawling()
    {
        activeData = crawling.CreateInstance();
    }

    public void LerpToCrawl(float rate)
    {
        if (lerp >= 1.0f) { return; }
        lerp += Time.deltaTime * rate;
        
        activeData.size = Vector2.Lerp(standing.size, crawling.size, lerp);
        activeData.feetSeperation = Mathf.Lerp(standing.feetSeperation, crawling.feetSeperation, lerp);
        activeData.vaultHeight = Mathf.Lerp(standing.vaultHeight, crawling.vaultHeight, lerp);
        activeData.vaultRate = Mathf.Lerp(standing.vaultRate, crawling.vaultRate, lerp);
        activeData.stepHeight = Mathf.Lerp(standing.stepHeight, crawling.stepHeight, lerp);
        activeData.stepRate = Mathf.Lerp(standing.stepRate, crawling.stepRate, lerp);
    }
    public void LerpToStand(float rate)
    {
        if (lerp <= 0.0f) { return; }
        lerp -= Time.deltaTime * rate;
        
        activeData.size = Vector2.Lerp(standing.size, crawling.size, lerp);
        activeData.feetSeperation = Mathf.Lerp(standing.feetSeperation, crawling.feetSeperation, lerp);
        activeData.vaultHeight = Mathf.Lerp(standing.vaultHeight, crawling.vaultHeight, lerp);
        activeData.vaultRate = Mathf.Lerp(standing.vaultRate, crawling.vaultRate, lerp);
        activeData.stepHeight = Mathf.Lerp(standing.stepHeight, crawling.stepHeight, lerp);
        activeData.stepRate = Mathf.Lerp(standing.stepRate, crawling.stepRate, lerp);
    }
}