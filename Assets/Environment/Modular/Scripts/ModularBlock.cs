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

    public void UpdateBlock(ModularBlock block)
    {
        m_Block = block;
    }

    public void UpdateOptions(IEnumerable<GameObject> availableObjects)
    {
        m_AvailableObjects = availableObjects;
    }

    private void OnSelectionUpdated()
    {
        m_Block.OnSelectionUpdated(this);
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
    [OnValueChanged("OnRootChanged"), OnInspectorInit("OnInspectorInit"), AssetList(Path = "Assets/Environment/Modular")]
    [SerializeField] private ModularSettings m_Root;

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

        if (m_Root != null)
        {
            // Spawn new root object
            GameObject rootObject = Instantiate(m_Root.BaseObject, transform);
            // Remove child mesh from root
            foreach (Transform child in rootObject.transform)
            {
                foreach (Component component in child.GetComponents<Component>())
                {
                    if (!(component is Transform))
                    {
                        DestroyImmediate(component);
                    }
                }
            }
            UpdateOptions();
        }
    }

    private void OnInspectorInit()
    {
        foreach (ModularOptions options in m_Options)
        {
            options.UpdateBlock(this);
        }
        UpdateOptions();
    }

    private void UpdateOptions()
    {
        if (m_Root == null) { return; }

        foreach (ModularSocket socket in m_Root.Options)
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
