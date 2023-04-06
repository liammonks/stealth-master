using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class RollbackTimeline : MonoBehaviour
{
    [SerializeField]
    private RectTransform m_IndicatorPrefab;

    [SerializeField]
    private TextMeshProUGUI m_MaxTime, m_MinTime, m_CurrentTime, m_StateDataOutput;

    private RectTransform m_RectTransform;

    private int m_SelectedStateDataIndex;
    private List<float> m_StateDataTimes;
    private Dictionary<float, Image> m_Indicators;

    private void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        m_SelectedStateDataIndex = -1;
        GenerateTimeline();
    }

    private void OnDisable()
    {
        foreach (KeyValuePair<float, Image> kvp in m_Indicators)
        {
            Destroy(kvp.Value.transform.parent.gameObject);
        }
    }

    private void OnTimelineLeft()
    {
        if (m_SelectedStateDataIndex == 0)
        {
            SelectIndicator(m_StateDataTimes.Count - 1);
        }
        else
        {
            SelectIndicator(m_SelectedStateDataIndex - 1);
        }
    }

    private void OnTimelineRight()
    {
        if (m_SelectedStateDataIndex == m_StateDataTimes.Count - 1)
        {
            SelectIndicator(0);
        }
        else
        {
            SelectIndicator(m_SelectedStateDataIndex + 1);
        }
    }

    private void GenerateTimeline()
    {
        m_Indicators = new Dictionary<float, Image>();
        Time.timeScale = 0.0f;

        float maxPos = m_RectTransform.rect.size.x * 0.5f;
        float minPos = -maxPos;

        m_MaxTime.text = Simulation.Time.ToString();
        m_MinTime.text = (Simulation.Time - Simulation.Instance.StateBufferDuration).ToString();

        // Spawn indicators
        foreach (KeyValuePair<float, List<StateData>> kvp in Simulation.Instance.StateBuffer)
        {
            float stateDataTime = kvp.Key;
            List<StateData> stateData = kvp.Value;

            float dataAge = Simulation.Time - stateDataTime;
            float normalisedTime = (Simulation.Instance.StateBufferDuration - dataAge) / Simulation.Instance.StateBufferDuration;
            m_Indicators.Add(stateDataTime, SpawnIndicator(Mathf.Lerp(minPos, maxPos, normalisedTime)));
        }

        // Set latest indicator to green
        m_StateDataTimes = Simulation.Instance.StateBuffer.Keys.ToList();
        m_StateDataTimes.Sort();
        SelectIndicator(m_StateDataTimes.Count - 1);
    }

    private void SelectIndicator(int index)
    {
        float stateDataTime;

        // Deselect previous indicator
        if (m_SelectedStateDataIndex != -1)
        {
            stateDataTime = m_StateDataTimes[m_SelectedStateDataIndex];
            m_Indicators[stateDataTime].color = Color.red;
        }

        // Select new indicator
        stateDataTime = m_StateDataTimes[index];
        m_Indicators[stateDataTime].color = Color.green;

        m_SelectedStateDataIndex = index;
        m_CurrentTime.text = m_StateDataTimes[m_SelectedStateDataIndex].ToString();

        Simulation.Instance.Rollback(m_StateDataTimes[m_SelectedStateDataIndex]);
        DisplayData();
    }

    private Image SpawnIndicator(float position)
    {
        RectTransform indicator = Instantiate(m_IndicatorPrefab, m_RectTransform);
        indicator.anchoredPosition = new Vector2(position, 0);
        return indicator.GetChild(0).GetComponent<Image>();
    }

    private void DisplayData()
    {
        m_StateDataOutput.text = string.Empty;

        List<StateData> allStateData = new List<StateData>();
        allStateData.AddRange(Simulation.Instance.StateBuffer[m_StateDataTimes[m_SelectedStateDataIndex]]);
        allStateData.AddRange(Simulation.Instance.InputBuffer[m_StateDataTimes[m_SelectedStateDataIndex]]);

        foreach (StateData stateData in allStateData)
        {
            m_StateDataOutput.text += $"<b>{stateData.owner.GetType()}</b>";

            foreach (FieldInfo fieldInfo in stateData.data.GetType().GetFields())
            {
                m_StateDataOutput.text += "\n";
                m_StateDataOutput.text += $"{fieldInfo.Name} : {fieldInfo.GetValue(stateData.data)}";
            }

            m_StateDataOutput.text += "\n\n";
        }
    }
}
