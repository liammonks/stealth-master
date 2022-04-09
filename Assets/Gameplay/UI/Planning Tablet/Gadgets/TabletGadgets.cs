using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Gadgets;

public class TabletGadgets : MonoBehaviour
{
    [SerializeField] private Transform gadgetsParent;
    [SerializeField] private Button gadgetButton;

    private void Start() {
        InitialiseGadgets();
    }
    
    private void InitialiseGadgets()
    {
        // Create buttons
        foreach (BaseGadget gadget in GlobalData.Gadgets)
        {
            Button button = Instantiate(gadgetButton, gadgetsParent);
            button.GetComponentInChildren<TextMeshProUGUI>().text = gadget.name;
            button.image.color = GlobalData.playerGadgets.Contains(gadget) ? Color.green : Color.red;
            button.onClick.AddListener(delegate { OnGadgetClicked(gadget, button); });
        }
    }
    
    public void OnGadgetClicked(BaseGadget gadget, Button button)
    {
        if (GlobalData.playerGadgets.Contains(gadget))
        {
            GlobalData.playerGadgets.Remove(gadget);
            button.image.color = Color.red;
        }
        else
        {
            GlobalData.playerGadgets.Add(gadget);
            button.image.color = Color.green;
        }
        GlobalData.SavePlayerGadgets();
    }
}
