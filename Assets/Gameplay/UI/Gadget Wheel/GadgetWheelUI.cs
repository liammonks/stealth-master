using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GadgetWheelUI : MonoBehaviour
{
    [SerializeField] private Image wheelSegmentPrefab;

    private const float attemptDuration = 0.5f;
    private Coroutine selectGadgetCoroutine;

    private void Awake()
    {
        for(int i = 0; i < GlobalData.playerGadgets.Count; ++i)
        {
            Image segment = Instantiate(wheelSegmentPrefab, transform);
            segment.fillAmount = 1.0f / GlobalData.playerGadgets.Count;
            segment.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, (360 / GlobalData.playerGadgets.Count) * i));
            segment.GetComponent<Button>().onClick.AddListener(delegate { SelectGadget(i); });
            segment.transform.name = GlobalData.playerGadgets[i].name;
            segment.alphaHitTestMinimumThreshold = 0.1f;
            // 0.5 = Left
            // 0.25 = Left / Down
            // 0.0 = Down
            Vector2 offset = new Vector2(segment.fillAmount * 2, (0.5f - segment.fillAmount) * 2);
            segment.rectTransform.Translate(offset * 10);
        }
    }
    
    public void Toggle(bool active)
    {
        gameObject.SetActive(active);
    }
    
    public void SelectGadget(int index)
    {
        if (selectGadgetCoroutine != null) { StopCoroutine(selectGadgetCoroutine); }
        selectGadgetCoroutine = StartCoroutine(TrySelectGadget(index));
    }
    
    private IEnumerator TrySelectGadget(int index)
    {
        float duration = 0.0f;
        while(UnitHelper.Player.EquipGadget(GlobalData.playerGadgets[index]) == false && duration < attemptDuration)
        {
            duration += Time.deltaTime;
            yield return null;
        }
    }

}
