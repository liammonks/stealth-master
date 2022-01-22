using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugText : MonoBehaviour
{
    public static DebugText Instance;

    [SerializeField] private TextMeshProUGUI textPrefab;

    private Dictionary<string, TextMeshProUGUI> textDictionary = new Dictionary<string, TextMeshProUGUI>();
    private Dictionary<string, Coroutine> textDestroyCoroutineDictionary = new Dictionary<string, Coroutine>();

    private void Awake() {
        if(Instance != null) {
            Debug.LogError("Two instances of DebugText found");
            return;
        }
        Instance = this;
    }
    
    public void PrintText(string key, string value, Vector2 screenPosition, Color color, bool permanent = false)
    {
        TextMeshProUGUI textObj;
        if (textDictionary.ContainsKey(key))
        {
            textObj = textDictionary[key];
        }
        else
        {
            textObj = Instantiate(textPrefab, transform);
            textDictionary.Add(key, textObj);
        }
        
        if(textDestroyCoroutineDictionary.ContainsKey(key))
        {
            StopCoroutine(textDestroyCoroutineDictionary[key]);
            textDestroyCoroutineDictionary.Remove(key);
        }
        
        textObj.rectTransform.anchoredPosition = screenPosition;
        textObj.text = value;
        textObj.color = color;
        textObj.faceColor = color;

        if (!permanent)
        {
            //textObj.GetComponent<Animator>().Play("Fade");
            textDestroyCoroutineDictionary.Add(key, StartCoroutine(DestroyText(key)));
        }
    }
    
    private IEnumerator DestroyText(string key)
    {
        yield return new WaitForSeconds(1);
        TextMeshProUGUI text = textDictionary[key];
        textDictionary.Remove(key);
        textDestroyCoroutineDictionary.Remove(key);
        Destroy(text.gameObject);
    }
}
