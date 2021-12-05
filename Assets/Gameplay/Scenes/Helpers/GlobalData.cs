using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Gadgets;
using UnityEngine.SceneManagement;

public class GlobalData : MonoBehaviour
{
    #region Gadgets

    [SerializeField] private BaseGadget defaultGadget;
    private static BaseGadget _defaultGadget;
    public static BaseGadget DefaultGadget { get { return _defaultGadget; } }

    [SerializeField] private List<BaseGadget> gadgets;
    private static List<BaseGadget> _gadgets;
    public static ReadOnlyCollection<BaseGadget> Gadgets { get { return _gadgets.AsReadOnly(); } }

    public static List<BaseGadget> playerGadgets = new List<BaseGadget>();

    public static void SavePlayerGadgets()
    {
        int data = 0;
        for (int i = 0; i < _gadgets.Count; ++i)
        {
            if(playerGadgets.Contains(_gadgets[i]))
            {
                data += Mathf.RoundToInt(Mathf.Pow(2, i));
            }
        }
        PlayerPrefs.SetInt("PlayerGadgets", data);
    }
    
    public static void LoadPlayerGadgets()
    {
        int data = PlayerPrefs.GetInt("PlayerGadgets", 1);
        for (int i = 0; i < _gadgets.Count; ++i)
        {
            int bin = Mathf.RoundToInt(Mathf.Pow(2, i));
            if ((data & bin) == bin)
            {
                playerGadgets.Add(_gadgets[i]);
            }
        }
    }

    #endregion

    #region Missions

    [SerializeField] private List<Mission> missions;
    private static List<Mission> _missions;
    public static ReadOnlyCollection<Mission> Missions
    {
        get { return _missions.AsReadOnly(); }
    }

    #endregion

    private static bool initialised = false;

    private void Awake() {
        if (initialised) { return; }

        _gadgets = gadgets;
        _defaultGadget = defaultGadget;
        _missions = missions;
        LoadPlayerGadgets();

        initialised = true;
    }
}
