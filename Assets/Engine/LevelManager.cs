using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class LevelManager : MonoBehaviour
{
    private void Awake()
    {
        if (!SceneManager.GetSceneByName("Player").IsValid())
        {
            SceneManager.LoadScene("Player", LoadSceneMode.Additive);
        }
    }
}
