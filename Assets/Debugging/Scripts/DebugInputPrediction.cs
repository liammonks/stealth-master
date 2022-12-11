using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Debugging
{

    public class DebugInputPrediction : MonoBehaviour
    {
        private interface IPredict
        {
            public List<string> GetPredictions(string input);
        }

        private struct TransformData : IPredict
        {
            public Dictionary<string, List<Transform>> Children;
            public Dictionary<string, List<Component>> Components;

            public TransformData(Transform transform)
            {
                Children = new Dictionary<string, List<Transform>>();
                foreach (Transform child in transform)
                {
                    if (Children.ContainsKey(child.name))
                    {
                        Children[child.name].Add(child);
                    }
                    else
                    {
                        Children.Add(child.name, new List<Transform> { child });
                    }
                }

                Components = new Dictionary<string, List<Component>>();
                Component[] components = transform.GetComponents<Component>();
                foreach (Component component in components)
                {
                    string componentName = component.GetType().Name;
                    if (Components.ContainsKey(componentName))
                    {
                        Components[componentName].Add(component);
                    }
                    else
                    {
                        Components.Add(componentName, new List<Component> { component });
                    }
                }
            }

            public List<string> GetPredictions(string input)
            {
                if (input == string.Empty) { return new List<string>(); }

                List<string> predictions = new List<string>();
                char prefix = input[0];
                input = input.Substring(1);
                Debug.Log("PREFIX : " + prefix);
                Debug.Log("INPUT : " + input);
                return predictions;
            }
        }

        private struct ComponentData : IPredict
        {
            public Dictionary<string, FieldInfo> FieldInfo;
            public Dictionary<string, PropertyInfo> PropertyInfo;

            public ComponentData(Component component)
            {
                FieldInfo = new Dictionary<string, FieldInfo>();
                foreach (FieldInfo fieldInfo in component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    FieldInfo.Add(fieldInfo.Name, fieldInfo);
                }

                PropertyInfo = new Dictionary<string, PropertyInfo>();
                foreach (PropertyInfo propertyInfo in component.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    PropertyInfo.Add(propertyInfo.Name, propertyInfo);
                }
            }

            public List<string> GetPredictions(string input)
            {
                if (input == string.Empty) { return new List<string>(); }

                char prefix = input[0];
                if (prefix != '.') { return new List<string>(); }

                List<string> predictions = new List<string>();
                predictions.AddRange(FieldInfo.Keys);
                predictions.AddRange(PropertyInfo.Keys);
                return predictions;
            }
        }

        [SerializeField]
        private Transform m_LabelParent;

        [SerializeField]
        private DebugLabel m_LabelPrefab;

        private DebugSelector m_DebugSelector;
        private DebugToggle m_DebugToggle;
        private DebugInput m_DebugInput;

        private TransformData m_CurrentTransformData;
        private ComponentData m_CurrentComponentData;


        private void Awake()
        {
            m_DebugSelector = GetComponent<DebugSelector>();
            m_DebugSelector.OnSelectionUpdated += OnTransformSelected;

            m_DebugToggle = GetComponent<DebugToggle>();
            m_DebugToggle.OnActivated += OnDebugActivated;

            m_DebugInput = GetComponent<DebugInput>();
            m_DebugInput.OnInputChanged += OnInputChanged;
        }

        private void OnDebugActivated()
        {
            OnTransformSelected(m_DebugSelector.SelectedTransform);
        }

        private void OnTransformSelected(Transform selectedTransform)
        {
            if (!m_DebugToggle.Active) { return; }
            m_CurrentTransformData = new TransformData(selectedTransform);
        }

        public void OnInputChanged(string input)
        {
            m_CurrentTransformData.GetPredictions(input);
        }

        private void DisplayPredictions(List<string> predictions, string toHighlight)
        {
            foreach (Transform child in m_LabelParent)
            {
                Destroy(child.gameObject);
            }

            foreach (string availableInput in predictions)
            {
                DebugLabel label = Instantiate(m_LabelPrefab, m_LabelParent);
                label.SetText(availableInput);
                label.Highlight(toHighlight, Color.green);
            }
        }
    }

}