using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

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

}