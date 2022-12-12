using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
        private DebugSelector m_DebugSelector;
        private DebugCommands m_Commands;

        private void Awake()
        {
            m_InputField.onValueChanged.AddListener(OnValueChanged);
            m_InputField.onSubmit.AddListener(OnSubmit);
            m_InputField.onEndEdit.AddListener(OnEndEdit);

            m_DebugSelector = GetComponent<DebugSelector>();
            m_DebugSelector.OnSelectionUpdated += OnTransformSelected;

            m_DebugToggle = GetComponent<DebugToggle>();
            m_DebugToggle.OnActivated += OnDebugActivated;
            m_DebugToggle.OnDeactivated += OnDebugDeactivated;

            m_Commands = GetComponent<DebugCommands>();
        }

        private void OnDebugActivated()
        {
            m_InputField.Select();
            m_InputField.ActivateInputField();

            OnTransformSelected(m_DebugSelector.SelectedTransform);
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
            m_Commands.ExecuteCommand(input);
        }

        private void OnEndEdit(string input)
        {
            if (m_DebugToggle.Active)
            {
                m_InputField.Select();
                m_InputField.ActivateInputField();
            }
        }

        private void OnTransformSelected(Transform selectedTransform)
        {

        }


        private IEnumerator MoveCaretToEnd()
        {
            yield return new WaitForEndOfFrame();
            m_InputField.MoveTextEnd(false);
        }

    }

}