using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Debugging
{
    public class TabGroup : MonoBehaviour
    {
        [SerializeField]
        private Button[] tabs;

        public Action<int> OnTabSelected;

        private void Awake()
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                int temp = i;
                tabs[i].onClick.AddListener(delegate { OnTabClicked(temp); });
            }
        }

        private void OnTabClicked(int index, bool fireEvent = true)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i].interactable = true;
            }
            tabs[index].interactable = false;

            if (fireEvent)
            {
                OnTabSelected?.Invoke(index);
            }
        }

        public void SetTabIndex(int index)
        {
            OnTabClicked(index, false);
        }
    }
}