using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TabletMissions : MonoBehaviour
{
    [SerializeField] private Transform missionsParent;
    [SerializeField] private Button missionButton;

    private void Awake()
    {
        foreach (Mission mission in GlobalData.Missions)
        {
            Button button = Instantiate(missionButton, missionsParent);
            button.GetComponentInChildren<TextMeshProUGUI>().text = mission.name;
            button.onClick.AddListener(delegate { SceneManager.LoadScene(mission.scene); });
        }
    }
}
