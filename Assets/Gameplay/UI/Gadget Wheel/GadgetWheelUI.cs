using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GadgetWheelUI : MonoBehaviour
{
    [SerializeField] private Image wheelSegmentPrefab;

    private void Awake()
    {
        for(int i = 0; i < GlobalData.playerGadgets.Count; ++i)
        {
            Image segment = Instantiate(wheelSegmentPrefab, transform);
            segment.fillAmount = 1.0f / GlobalData.playerGadgets.Count;
            segment.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, (360 / GlobalData.playerGadgets.Count) * i));
            // 0.5 = Left
            // 0.25 = Left / Down
            // 0.0 = Down
        }
    }

}
