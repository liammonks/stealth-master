using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using static UnityEngine.Playables.FrameData;

namespace Debugging
{

    public class DebugInput : MonoBehaviour
    {
        public Action<string> OnInputChanged;

        [SerializeField]
        private TMP_InputField m_InputField;

        private DebugToggle m_DebugToggle;
        private DebugCommands m_Commands;
        private DebugInputPrediction m_Prediction;

        private List<string> m_PreviousCommands = new List<string>();
        private int m_SelectionIndex;

        private void Awake()
        {
            m_InputField.onValueChanged.AddListener(OnValueChanged);
            m_InputField.onSubmit.AddListener(OnSubmit);
            m_InputField.onEndEdit.AddListener(OnEndEdit);

            m_DebugToggle = GetComponent<DebugToggle>();
            m_DebugToggle.OnActivated += OnDebugActivated;
            m_DebugToggle.OnDeactivated += OnDebugDeactivated;

            m_Commands = GetComponent<DebugCommands>();
            m_Prediction = GetComponent<DebugInputPrediction>();
        }

        private void OnDebugActivated()
        {
            m_SelectionIndex = 0;
            m_InputField.Select();
            m_InputField.ActivateInputField();
        }

        private void OnDebugDeactivated()
        {
            m_InputField.text = string.Empty;
        }

        private void OnValueChanged(string input)
        {
            OnInputChanged?.Invoke(input);
        }

        private void OnSubmit(string input)
        {
            if (input == string.Empty) { return; }

            if (m_PreviousCommands.Count == 0 || m_PreviousCommands.Last() != input)
            {
                m_PreviousCommands.Add(input);
            }

            m_Commands.ExecuteCommand(input);
            m_DebugToggle.Deactivate();
        }

        private void OnEndEdit(string input)
        {
            if (m_DebugToggle.Active)
            {
                m_InputField.Select();
                m_InputField.ActivateInputField();
            }
        }

        private void OnSelectionUp(InputValue value)
        {
            if (!m_DebugToggle.Active) { return; }
            if (value.Get<float>() == 1.0f)
            {
                m_SelectionIndex = Mathf.Min(m_SelectionIndex + 1, m_PreviousCommands.Count);
            }
            UpdateSelection();
        }

        private void OnSelectionDown(InputValue value)
        {
            if (!m_DebugToggle.Active) { return; }
            if (value.Get<float>() == 1.0f)
            {
                m_SelectionIndex = Mathf.Max(m_SelectionIndex - 1, -m_Prediction.Predictions.Count);
            }
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            if (m_SelectionIndex > 0)
            {
                // Selection up, get previous commands
                m_InputField.SetTextWithoutNotify(m_PreviousCommands[m_PreviousCommands.Count - m_SelectionIndex]);
            }
            if (m_SelectionIndex < 0)
            {
                // Selection down, get predicted commands
                m_InputField.SetTextWithoutNotify(m_Prediction.Predictions[Mathf.Abs(m_SelectionIndex) - 1]);
            }
            StartCoroutine(MoveCaretToEnd());
        }

        private IEnumerator MoveCaretToEnd()
        {
            yield return new WaitForEndOfFrame();
            m_InputField.MoveTextEnd(false);
        }

    }

}