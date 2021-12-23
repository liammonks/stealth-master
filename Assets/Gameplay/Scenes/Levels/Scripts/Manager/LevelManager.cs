using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    
    private enum WinCondition
    {
        EnemiesEliminated
    }

    public static LevelManager Instance;

    public LevelUI UI;

    [SerializeField] private PlayerSpawn activePlayerSpawn;
    [SerializeField] private WinCondition winCondition;

    private List<AIUnit> enemyUnits;

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

        enemyUnits = new List<AIUnit>(FindObjectsOfType<AIUnit>()).FindAll(x => x.isEnemy);
    }

    public void RespawnPlayer()
    {
        if (activePlayerSpawn != null)
        {
            UnitHelper.Player.transform.position = activePlayerSpawn.transform.position;
        }
    }

    public void OnEnemyKilled(AIUnit unit)
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

    #endregion
}