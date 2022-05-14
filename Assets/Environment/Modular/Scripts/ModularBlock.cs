using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ModularOption
{
    [HideInInspector] public string Name;
    [HideInInspector] public int Index;

    [ValueDropdown("AvailableObjects")] [HideLabel] [InlineButton("ClearSelection", "Remove")] [OnValueChanged("@OnSelectionUpdated?.Invoke(this)")]
    public GameObject SelectedObject;
    public IEnumerable<GameObject> AvailableObjects;
    public Action<ModularOption> OnSelectionUpdated;

    public ModularOption(string name, int index, IEnumerable<GameObject> availableObjects)
    {
        Name = name;
        Index = index;
        AvailableObjects = availableObjects;
    }

    public void SetSelection(GameObject selection)
    {
        SelectedObject = selection;
        OnSelectionUpdated?.Invoke(this);
    }

    private void ClearSelection()
    {
        SelectedObject = null;
        OnSelectionUpdated?.Invoke(this);
    }

}

[ExecuteInEditMode]
public class ModularBlock : MonoBehaviour
{
    public Transform ModularRoot => transform.childCount > 0 ? transform.GetChild(0) : null;

    [OnInspectorInit("OnInspectorInit"), OnValueChanged("OnRootChanged"), AssetList(Path = "Assets/Environment/Modular")]
    [SerializeField] private ModularSettings m_ModularSettings;

    [ListDrawerSettings(Expanded = true, ListElementLabelName = "@Name", IsReadOnly = true)]
    [SerializeField] private List<ModularOption> m_Options = new List<ModularOption>();

    private void OnInspectorInit()
    {
        if (m_ModularSettings == null || ModularRoot == null) { return; }

        // Ensure options are setup correctly
        foreach (ModularSocket socket in m_ModularSettings.Sockets)
        {
            ModularOption existingOption = m_Options.Find(x => x.Name == socket.Name);
            ModularOption newOption = new ModularOption(socket.Name, socket.Index, socket.Options);
            newOption.OnSelectionUpdated += OptionSelectionUpdated;

            // If an option for this socket already exists, fetch its currently selected object
            if (existingOption != null)
            {
                newOption.SetSelection(existingOption.SelectedObject);
                m_Options.Remove(existingOption);
            }
            else
            {
                // Check if an object already exists in the socket
                Transform socketTransform = ModularRoot.GetChild(socket.Index);
                if (socketTransform.childCount > 0)
                {
                    GameObject foundObject = socket.Options.Find(x => x.name == socketTransform.GetChild(0).name);
                    newOption.SetSelection(foundObject);
                }
            }
            m_Options.Add(newOption);
        }
    }

    private void OnRootChanged()
    {
        m_Options.Clear();

        // Clear previous objects
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        if (m_ModularSettings != null)
        {
            // Spawn new root object
            GameObject rootObject = Instantiate(m_ModularSettings.BaseObject, transform);
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
        }
        OnInspectorInit();
    }

    private void OptionSelectionUpdated(ModularOption option)
    {
        Transform socket = ModularRoot.GetChild(option.Index);
        // Clear previous children
        foreach (Transform child in socket)
        {
            DestroyImmediate(child.gameObject);
        }

        if (option.SelectedObject == null) return;

        GameObject modularPiece = Instantiate(option.SelectedObject, socket);
        modularPiece.transform.localPosition = Vector3.zero;
        modularPiece.transform.localRotation = Quaternion.identity;
        modularPiece.transform.localScale = Vector3.one;
    }

}
