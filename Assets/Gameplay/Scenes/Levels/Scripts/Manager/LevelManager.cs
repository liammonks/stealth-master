using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private PlayerSpawn activePlayerSpawn;

    private void Awake() {
        if(Instance != null) {
            Debug.LogError("Two Instances of LevelManager Found");
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 300;
        QualitySettings.vSyncCount = 0;
        
        // Load the player scene if not already active
        if (!SceneManager.GetSceneByName("Player").IsValid())
        {
            SceneManager.LoadScene("Player", LoadSceneMode.Additive);
        }
    }

    public void RespawnPlayer()
    {
        if (activePlayerSpawn != null)
        {
            UnitHelper.Instance.GetPlayerUnit().transform.position = activePlayerSpawn.transform.position;
        }
    }
}