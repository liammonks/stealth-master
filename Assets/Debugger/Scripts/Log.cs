using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public static class Log {

    public static void UnitState(UnitState state, float duration) {
        if (DebugWindowManager.Instance == null) { return; }
        DebugWindow window = DebugWindowManager.Instance.GetOrCreateWindow(DebugWindowType.UnitState);
        string currentText = window.GetCurrentText();
        // Get first word of text
        string currentState = Regex.Replace(currentText.Split()[0], @"[^0-9a-zA-Z\ ]+", "");
        if(currentState != state.ToString() && currentText != string.Empty) {
            window.Append();
        }
        
        string durationDashes = string.Empty;
        int dashCount = Mathf.RoundToInt(duration * 10);
        if(dashCount < 50) {
            for (int i = 0; i < dashCount; ++i) {
                durationDashes += '-';
            }
        } else {
            durationDashes = "----------" + "----------" + "----------" + "----------" + "----------";
        }

        window.SetText(state.ToString() + " - " + duration + " " + durationDashes);
    }
    
}
