using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabletGadgets : MonoBehaviour
{
    [SerializeField] private Transform gadgetsParent;
    [SerializeField] private Button gadgetButton;

    private void Awake() {
        foreach(Gadget gadget in GlobalData.Gadgets)
        {
            Button button = Instantiate(gadgetButton, gadgetsParent);
            button.GetComponentInChildren<TextMeshProUGUI>().text = gadget.name;
            button.image.color = Color.red;
            button.onClick.AddListener(delegate { OnGadgetClicked(gadget, button); });
        }
    }
    
    public void OnGadgetClicked(Gadget gadget, Button button)
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
    }
}
