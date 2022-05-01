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
    public List<GameObject> Options = new List<GameObject>();

    public ModularSocket(string name, int index)
    {
        Name = name;
        Index = index;
    }
}

[CreateAssetMenu(fileName = "NewModularPiece", menuName = "StealthMaster/ModularPiece")]
public class ModularPiece : SerializedScriptableObject
{
    [OnValueChanged("OnRootChanged")]
    public GameObject BaseObject;

    [ListDrawerSettings(Expanded = true, ListElementLabelName = "@Name", IsReadOnly = true)] [Space(20)]
    public List<ModularSocket> Options;

    private bool m_ConfirmationRequired = false;

    private void OnRootChanged()
    {
        if (BaseObject == null)
        {
            Options.Clear();
            m_ConfirmationRequired = false;
        }
        else
        {
            ResetSockets();
        }
    }

    private void ResetSockets()
    {
        Options.Clear();
        foreach (Transform child in BaseObject.transform)
        {
            ModularSocket socket = new ModularSocket(child.name, child.GetSiblingIndex());
            Options.Add(socket);
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
