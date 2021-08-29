using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private void Awake()
    {
        if (SceneManager.GetSceneByName("Player") == null)
        {
            SceneManager.LoadScene("Player", LoadSceneMode.Additive);
        }
    }
}
