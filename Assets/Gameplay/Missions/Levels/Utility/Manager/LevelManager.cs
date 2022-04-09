using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public static FoodVendor[] FoodVendors;
    public static string sceneName;

    public LevelUI UI;
    public PlayerSpawn ActivePlayerSpawn => activePlayerSpawn;

    [SerializeField] private PlayerSpawn activePlayerSpawn;
    [SerializeField] private List<WinCondition> winConditions;

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
        
        // Initialise WinConditions
        foreach (WinCondition winCondition in winConditions)
        {
            winCondition.onComplete += OnConditionComplete;
        }

        FoodVendors = FindObjectsOfType<FoodVendor>();
    }
    
    private void OnDisable()
    {
        foreach (WinCondition winCondition in winConditions)
        {
            winCondition.onComplete -= OnConditionComplete;
        }
    }
    
    public void LoadLevel(string levelName)
    {
        SceneManager.UnloadSceneAsync(gameObject.scene);
        SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
    }

    public void RespawnPlayer()
    {
        GlobalEvents.MissionRestart();
        if (activePlayerSpawn != null)
        {
            UnitHelper.Player.data.rb.velocity = Vector2.zero;
            UnitHelper.Player.transform.position = activePlayerSpawn.transform.position;
        }
    }

    public void OnConditionComplete()
    {
        foreach (WinCondition winCondition in winConditions)
        {
            if (!winCondition.IsComplete) return;
        }
        LevelComplete();
    }

    public void LevelComplete()
    {
        GlobalEvents.MissionComplete();
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

    private void OnSlow()
    {
        Time.timeScale = Time.timeScale == 1.0f ? 0.2f : 1.0f;
    }

    #endregion
}