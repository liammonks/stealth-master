using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public LevelUI UI;

    [SerializeField] private PlayerSpawn activePlayerSpawn;

    private void Awake() {
        if(Instance != null) {
            Debug.LogError("Two Instances of LevelManager Found");
            return;
        }
        Instance = this;

        if (activePlayerSpawn == null)
        {
            activePlayerSpawn = FindObjectOfType<PlayerSpawn>();
        }
    }

    public void RespawnPlayer()
    {
        if (activePlayerSpawn != null)
        {
            UnitHelper.Player.transform.position = activePlayerSpawn.transform.position;
        }
    }
}