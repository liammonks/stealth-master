using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private Transform activeSpawnPoint;

    [SerializeField]
    private bool m_IgnoreSpawnPoints = false;

    private void OnRestart()
    {
        if (m_IgnoreSpawnPoints)
        {
            PlayerManager.Instance.SpawnPlayer();
        }
        else
        {
            PlayerManager.Instance.SpawnPlayer(activeSpawnPoint.position);
        }
    }

}
