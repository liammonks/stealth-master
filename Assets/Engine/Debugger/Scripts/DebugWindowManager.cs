using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum DebugWindowType
{
    Null,
    UnitState,
}

public class DebugWindowManager : MonoBehaviour
{
    public static DebugWindowManager Instance;
    public static float toolbarThickness = 20;

    [SerializeField] private DebugWindow debugWindowPrefab;
    [SerializeField] private DebugWindowToolbarButton debugWindowToolbarButton;

    private Dictionary<DebugWindowType, DebugWindow> debugWindows = new Dictionary<DebugWindowType, DebugWindow>();
    private Dictionary<DebugWindowType, DebugWindowToolbarButton> debugWindowToolbarButtons = new Dictionary<DebugWindowType, DebugWindowToolbarButton>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Two Instances of DebugWindowManager Found");
            return;
        }
        Instance = this;

        GetComponent<RectTransform>().offsetMin = new Vector2(0, Screen.height - toolbarThickness);
        GetComponent<RectTransform>().offsetMax = Vector2.zero;
    }
    
    public DebugWindow GetOrCreateWindow(DebugWindowType debugWindowType) {
        if (debugWindows.ContainsKey(debugWindowType)) { return debugWindows[debugWindowType]; }
        
        // Create new window
        DebugWindow window = Instantiate(debugWindowPrefab, transform.parent);
        window.Initialise(debugWindowType.ToString());
        debugWindows.Add(debugWindowType, window);

        // Set window offsets
        string path = $"Assets/Engine/Debugger/Data/{debugWindowType}.txt";
        StreamReader reader = new StreamReader(path);
        window.SetMinOffset(StringToVector2(reader.ReadLine()));
        window.SetMaxOffset(StringToVector2(reader.ReadLine()));
        reader.Close();
        
        // Create toolbar button for window
        DebugWindowToolbarButton toolbarButton = Instantiate(debugWindowToolbarButton, transform);
        toolbarButton.Initialise(debugWindowType);
        debugWindowToolbarButtons.Add(debugWindowType, toolbarButton);

        return window;
    }

    public void OpenWindow(DebugWindowType debugWindowType) {
        if (debugWindows.ContainsKey(debugWindowType)) { debugWindows[debugWindowType].gameObject.SetActive(true); }
    }
    
    public void CloseWindow(DebugWindowType debugWindowType) {
        if (!debugWindows.ContainsKey(debugWindowType)) { return; }
        Destroy(debugWindows[debugWindowType].gameObject);
        debugWindows.Remove(debugWindowType);
        Destroy(debugWindowToolbarButtons[debugWindowType].gameObject);
        debugWindowToolbarButtons.Remove(debugWindowType);
    }

    public Vector2 StringToVector2(string rString)
    {
        string[] temp = rString.Substring(1, rString.Length - 2).Split(',');
        float x = float.Parse(temp[0]);
        float y = float.Parse(temp[1]);
        return new Vector2(x, y);
    }

    private void OnDestroy() {
        foreach(KeyValuePair<DebugWindowType, DebugWindow> pair in debugWindows) {
            string path = $"Assets/Engine/Debugger/Data/{pair.Key}.txt";
            File.Delete(path);
            //Write some text to the test.txt file
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine(pair.Value.GetMinOffset());
            writer.WriteLine(pair.Value.GetMaxOffset());
            writer.Close();
        }
    }
}
