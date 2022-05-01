using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ModularOptions
{
    [HideInInspector] public string Name;
    [HideInInspector] public int Index;

    [ValueDropdown("m_AvailableObjects")] [HideLabel] [InlineButton("RemoveSelection", "Remove")] [OnValueChanged("OnSelectionUpdated")]
    public GameObject SelectedObject;

    private IEnumerable<GameObject> m_AvailableObjects;
    private ModularBlock m_Block;

    public ModularOptions(ModularBlock block, string name, int index, IEnumerable<GameObject> availableObjects)
    {
        m_Block = block;
        Name = name;
        Index = index;
        m_AvailableObjects = availableObjects;
    }

    private void OnSelectionUpdated()
    {
        m_Block.OnSelectionUpdated(this);
    }

    public void UpdateOptions(IEnumerable<GameObject> availableObjects)
    {
        m_AvailableObjects = availableObjects;
    }

    private void RemoveSelection()
    {
        SelectedObject = null;
        OnSelectionUpdated();
    }
}

[ExecuteInEditMode]
public class ModularBlock : MonoBehaviour
{
    [OnValueChanged("OnRootChanged"), OnInspectorInit("OnInspectorInit")]
    [SerializeField] private ModularPiece m_RootPiece;

    [ListDrawerSettings(Expanded = true, ListElementLabelName = "@Name", IsReadOnly = true)]
    [SerializeField] private List<ModularOptions> m_Options = new List<ModularOptions>();

    private void OnRootChanged()
    {
        m_Options.Clear();

        // Clear previous objects
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        if (m_RootPiece != null)
        {
            // Spawn new root object
            Instantiate(m_RootPiece.BaseObject, transform);
            UpdateOptions();
        }
    }

    private void OnInspectorInit()
    {
        UpdateOptions();
    }

    private void UpdateOptions()
    {
        if (m_RootPiece == null) { return; }

        foreach (ModularSocket socket in m_RootPiece.Options)
        {
            ModularOptions existingOptions = m_Options.Find(x => x.Name == socket.Name);
            if (existingOptions != null)
            {
                // Update existing options
                existingOptions.UpdateOptions(socket.Options);
            }
            else
            {
                // Create new options
                m_Options.Add(new ModularOptions(this, socket.Name, socket.Index, socket.Options));
            }
        }
    }

    public void OnSelectionUpdated(ModularOptions options)
    {
        // Get relevant child
        Transform socket = transform.GetChild(0).GetChild(options.Index);
        // Clear previously socketed modules
        foreach (Transform child in socket)
        {
            DestroyImmediate(child.gameObject);
        }

        if (options.SelectedObject != null)
        {
            GameObject obj = Instantiate(options.SelectedObject, socket);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
        }
    }

}
