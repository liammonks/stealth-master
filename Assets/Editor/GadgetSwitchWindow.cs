// --------------------------------
// <copyright file="SceneSwitchWindow.cs" company="Rumor Games">
//     Copyright (C) Rumor Games, LLC.  All rights reserved.
// </copyright>
// --------------------------------

using System.IO;
using Gadgets;
using UnityEditor;
using UnityEngine;

/// <summary>
/// SceneSwitchWindow class.
/// </summary>
public class GadgetSwitchWindow : EditorWindow
{
    /// <summary>
    /// Tracks scroll position.
    /// </summary>
    private Vector2 scrollPos;

    /// <summary>
    /// Initialize window state.
    /// </summary>
    [MenuItem("Tools/Gadget Switch Window")]
    internal static void Init()
    {
        // EditorWindow.GetWindow() will return the open instance of the specified window or create a new
        // instance if it can't find one. The second parameter is a flag for creating the window as a
        // Utility window; Utility windows cannot be docked like the Scene and Game view windows.
        var window = (GadgetSwitchWindow)GetWindow(typeof(GadgetSwitchWindow), false, "Gadget Switch");
        window.position = new Rect(window.position.xMin + 100f, window.position.yMin + 100f, 200f, 400f);
    }

    /// <summary>
    /// Called on GUI events.
    /// </summary>
    internal void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, false, false);

        GUILayout.Label("Available Gadgets", EditorStyles.boldLabel);
        for (int i = 0; i < GlobalData.Gadgets.Count; ++i)
        {
            BaseGadget gadget = GlobalData.Gadgets[i];
            bool gadgetEquipped = GlobalData.playerGadgets.Contains(gadget);
            bool gadgetChecked = GUILayout.Toggle(GlobalData.playerGadgets.Contains(gadget), i + ": " + gadget.name, GUI.skin.toggle);
            
            if(!gadgetEquipped && gadgetChecked)
            {
                GlobalData.playerGadgets.Add(gadget);
                GlobalData.SavePlayerGadgets();
            }
            if(gadgetEquipped && !gadgetChecked)
            {
                GlobalData.playerGadgets.Remove(gadget);
                GlobalData.SavePlayerGadgets();
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
}