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
    
}

[CreateAssetMenu(fileName = "UnitCollider", menuName = "stealth-master/UnitCollider", order = 0)]
public class UnitCollider : ScriptableObject
{

    [SerializeField] private UnitColliderData standing, crawling;
    private UnitColliderData activeData, targetData;

    public Vector2 size { get { return activeData.size; } }
    public float feetSeperation { get { return activeData.feetSeperation; } }

    public float vaultHeight { get { return activeData.vaultHeight; } }
    public float stepHeight { get { return activeData.stepHeight; } }
    public float vaultRate { get { return activeData.vaultRate; } }
    public float stepRate { get { return activeData.stepRate; } }
    public float collisionRate { get { return activeData.collisionRate; } }

    private const float lerpRate = 4.0f;
    private float lerp = 0.0f;
    
    private void OnValidate() 
    {
        targetData = standing;
        activeData.size = standing.size;
        activeData.feetSeperation = standing.feetSeperation;
        activeData.vaultHeight = standing.vaultHeight;
        activeData.vaultRate = standing.vaultRate;
        activeData.stepHeight = standing.stepHeight;
        activeData.stepRate = standing.stepRate;
        activeData.collisionRate = standing.collisionRate;
    }
    
    public void SetStanding()
    {
        targetData = standing;
    }
    public void SetCrawling()
    {
        targetData = crawling;
    }

    public void LerpToTarget()
    {
        if(targetData == standing)
        {
            LerpToStand();
        }
        if(targetData == crawling)
        {
            LerpToCrawl();
        }
    }

    private void LerpToCrawl()
    {
        if (lerp >= 1.0f) { return; }
        lerp = Mathf.Min(lerp + (Time.deltaTime * lerpRate), 1.0f);

        activeData.size = Vector2.Lerp(standing.size, crawling.size, lerp);
        activeData.feetSeperation = Mathf.Lerp(standing.feetSeperation, crawling.feetSeperation, lerp);
        activeData.vaultHeight = Mathf.Lerp(standing.vaultHeight, crawling.vaultHeight, lerp);
        activeData.vaultRate = Mathf.Lerp(standing.vaultRate, crawling.vaultRate, lerp);
        activeData.stepHeight = Mathf.Lerp(standing.stepHeight, crawling.stepHeight, lerp);
        activeData.stepRate = Mathf.Lerp(standing.stepRate, crawling.stepRate, lerp);
        activeData.collisionRate = Mathf.Lerp(standing.collisionRate, crawling.collisionRate, lerp);
    }
    private void LerpToStand()
    {
        if (lerp <= 0.0f) { return; }
        lerp = Mathf.Max(0.0f, lerp - (Time.deltaTime * lerpRate));

        activeData.size = Vector2.Lerp(standing.size, crawling.size, lerp);
        activeData.feetSeperation = Mathf.Lerp(standing.feetSeperation, crawling.feetSeperation, lerp);
        activeData.vaultHeight = Mathf.Lerp(standing.vaultHeight, crawling.vaultHeight, lerp);
        activeData.vaultRate = Mathf.Lerp(standing.vaultRate, crawling.vaultRate, lerp);
        activeData.stepHeight = Mathf.Lerp(standing.stepHeight, crawling.stepHeight, lerp);
        activeData.stepRate = Mathf.Lerp(standing.stepRate, crawling.stepRate, lerp);
        activeData.collisionRate = Mathf.Lerp(standing.collisionRate, crawling.collisionRate, lerp);
    }
}