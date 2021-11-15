using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalData : MonoBehaviour
{
    #region Gadgets
    
    // All Gadgets
    [SerializeField] private List<Gadget> gadgets;
    private static List<Gadget> _gadgets;
    public static ReadOnlyCollection<Gadget> Gadgets
    {
        get { return _gadgets.AsReadOnly(); }
    }

    public static List<Gadget> playerGadgets = new List<Gadget>();

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
        _missions = missions;

        initialised = true;
    }
}
