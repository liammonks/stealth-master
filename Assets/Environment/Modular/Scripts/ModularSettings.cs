using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ModularSocket
{
    [HideInInspector] public string Name;
    [HideInInspector] public int Index;

    [ListDrawerSettings(Expanded = false, DraggableItems = false)]
    public List<GameObject> Options = new List<GameObject>();

    public ModularSocket(string name, int index)
    {
        Name = name;
        Index = index;
    }
}

[CreateAssetMenu(fileName = "NewModularSettings", menuName = "Environment/ModularSettings")]
public class ModularSettings : SerializedScriptableObject
{
    [SerializeField, OnValueChanged("OnRootChanged"), OnInspectorInit("OnInspectorInit")]
    private GameObject m_BaseObject;
    public GameObject BaseObject => m_BaseObject;

    [SerializeField]
    private Vector3 m_RootOffset;
    public Vector3 RootOffset => m_RootOffset;

    [Space(20), SerializeField, ListDrawerSettings(Expanded = true, DraggableItems = false, ListElementLabelName = "@Name", IsReadOnly = true)]
    private List<ModularSocket> m_Sockets;
    public List<ModularSocket> Sockets => m_Sockets;

    private bool m_ConfirmationRequired = false;

    private void OnRootChanged()
    {
        if (BaseObject == null)
        {
            Sockets.Clear();
            m_ConfirmationRequired = false;
        }
        else
        {
            ResetSockets();
        }
    }

    private void OnInspectorInit()
    {
        if (BaseObject == null)
        {
            Sockets.Clear();
        }
    }

    private void ResetSockets()
    {
        Sockets.Clear();
        foreach (Transform child in BaseObject.transform)
        {
            ModularSocket socket = new ModularSocket(child.name, child.GetSiblingIndex());
            Sockets.Add(socket);
        }
    }

    [DisableIf("m_ConfirmationRequired")] [Button("Reset Pieces")] [PropertySpace(20)]
    private void ResetButton()
    {
        if (BaseObject == null) return;
        m_ConfirmationRequired = true;
    }

    [ShowIf("m_ConfirmationRequired"), HorizontalGroup("Split", 0.5f), Button("Confirm")]
    private void ConfirmButton()
    {
        ResetSockets();
        m_ConfirmationRequired = false;
    }

    [ShowIf("m_ConfirmationRequired"), VerticalGroup("Split/right"), Button("Cancel")]
    private void CancelButton()
    {
        m_ConfirmationRequired = false;
    }

}
