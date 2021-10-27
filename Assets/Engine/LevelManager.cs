using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    private void Start()
    {
        // Load the player scene if not already active
        if (!SceneManager.GetSceneByName("Player").IsValid())
        {
            SceneManager.LoadScene("Player", LoadSceneMode.Additive);
        }
    }

}