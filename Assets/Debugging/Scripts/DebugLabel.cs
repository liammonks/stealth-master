using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Debugging
{

    public class DebugLabel : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_Text;
        private string m_OriginalText;

        public void SetText(string text)
        {
            m_OriginalText = text;
            m_Text.text = text;
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        public void SetColor(Color color)
        {
            m_Text.color = color;
        }

        public void Highlight(string text, Color color)
        {
            if (!m_OriginalText.Contains(text) || text == string.Empty) { return; }
            List<string> split = m_OriginalText.Split(text).ToList();
            //if (split.Length < 3) { return; }
            //m_Text.text = $"{split[0]}<#{ColorUtility.ToHtmlStringRGB(color)}>{split[1]}</color>{split[2]}";
            string highlightText = string.Empty;
            for (int i = 0; i < split.Count; i++)
            {
                if (i == split.Count - 1)
                {
                    highlightText += split[i];
                }
                else
                {
                    highlightText += $"{split[i]}<#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
                }
            }

            m_Text.text = highlightText;
        }
    }

}