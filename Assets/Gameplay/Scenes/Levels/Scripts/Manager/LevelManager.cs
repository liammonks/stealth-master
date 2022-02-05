using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    private enum WinCondition
    {
        Null,
        EnemiesEliminated
    }

    public static LevelManager Instance;
    public static string sceneName;

    public LevelUI UI;
    public PlayerSpawn ActivePlayerSpawn => activePlayerSpawn;

    [SerializeField] private PlayerSpawn activePlayerSpawn;
    [SerializeField] private WinCondition winCondition;

    private List<Enemy> enemyUnits;

    private void Awake() {
        if(Instance != null) {
            Debug.LogError("Two Instances of LevelManager Found");
            return;
        }
        Instance = this;
        sceneName = gameObject.scene.name;
        
        if (activePlayerSpawn == null)
        {
            activePlayerSpawn = FindObjectOfType<PlayerSpawn>();
        }

        enemyUnits = new List<Enemy>(FindObjectsOfType<Enemy>()).FindAll(x => x.isEnemy);
    }
    
    public void LoadLevel(string levelName)
    {
        SceneManager.UnloadSceneAsync(gameObject.scene);
        SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
    }

    public void RespawnPlayer()
    {
        if (activePlayerSpawn != null)
        {
            UnitHelper.Player.data.rb.velocity = Vector2.zero;
            UnitHelper.Player.transform.position = activePlayerSpawn.transform.position;
        }
    }

    public void OnEnemyKilled(Enemy unit)
    {
        enemyUnits.Remove(unit);
        if(winCondition == WinCondition.EnemiesEliminated && enemyUnits.Count == 0)
        {
            LevelComplete();
        }
    }

    public void LevelComplete()
    {
        SceneManager.LoadScene("Planning");
    }

    #region Input

    private void OnRespawn()
    {
        RespawnPlayer();
    }
    
    private void OnPause()
    {
        Debug.Break();
    }

    #endregion
}