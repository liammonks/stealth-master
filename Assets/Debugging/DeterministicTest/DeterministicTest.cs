using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DeterministicTest : MonoBehaviour
{
    private const int TEST_TICK = 600;

    [SerializeField]
    private Transform[] m_AgentTransforms;
    private bool m_Testing = true;

    private void Awake()
    {
        TickMachine.Register(TickOrder.DeterministicTest, OnTick);
    }

    private void Start()
    {
        //ManualTick();
    }

    private void OnDestroy()
    {
        TickMachine.Unregister(TickOrder.DeterministicTest, OnTick);
    }

    private void ManualTick()
    {
        TickMachine.AutoTick = false;
        for (int i = 0; i < TEST_TICK; ++i)
        {
            TickMachine.Tick();
        }
    }

    public void OnTick()
    {
        if (m_Testing && TickMachine.TickCount == TEST_TICK)
        {
            EndTest();
            m_Testing = false;
        }
    }

    private void EndTest()
    {
        // WRITE POSITION DATA
        string path = $"Assets/Debugging/DeterministicTest/positions.txt";

        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine($"TICK COUNT - {TickMachine.TickCount}");
        for (int i = 0; i < m_AgentTransforms.Length; i++)
        {
            if (m_AgentTransforms[i].gameObject.activeInHierarchy == false) { continue; }
            writer.WriteLine($"Agent_{i} - POSITION{(Vector2)m_AgentTransforms[i].position}");
        }
        writer.WriteLine($"-- END TEST -------------------------------------------");
        writer.WriteLine($"");
        writer.Close();

        Debug.Break();
    }

}
