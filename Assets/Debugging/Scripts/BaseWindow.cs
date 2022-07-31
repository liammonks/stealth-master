using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Debugging
{
    public abstract class BaseWindow : MonoBehaviour
    {
        protected static Color backgroundColor = Color.white;
        protected static Color textColor = Color.white;

        [SerializeField] protected TextMeshProUGUI headerText;
        [SerializeField] protected RectTransform content;

        protected Component targetComponent;
        protected GameObject textPrefab;
        protected List<TextMeshProUGUI> textList = new List<TextMeshProUGUI>();

        private void Awake()
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("DebugText");
            textPrefab = handle.WaitForCompletion();
            OnAwake();
            Hide();
        }

        protected void SetText(int line, string text)
        {
            int addLines = (line + 1) - textList.Count;
            while (addLines > 0)
            {
                textList.Add(Instantiate(textPrefab, content).GetComponent<TextMeshProUGUI>());
                content.gameObject.SetActive(true);
                Canvas.ForceUpdateCanvases();
                addLines--;
            }
            textList[line].text = text;
        }

        protected void ClearText()
        {
            foreach (TextMeshProUGUI tmp in textList)
            {
                Destroy(tmp.gameObject);
            }
            textList.Clear();
        }

        public void Show(Component target)
        {
            targetComponent = target;
            headerText.text = target.transform.name + " : " + target.GetType();
            gameObject.SetActive(true);
            OnShow();
        }

        public void Hide()
        {
            ClearText();
            content.gameObject.SetActive(false);
            gameObject.SetActive(false);
            OnHide();
        }

        protected abstract void OnAwake();
        protected abstract void OnShow();
        protected abstract void OnHide();
    }
}