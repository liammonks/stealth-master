using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInput : MonoBehaviour
{
    public static readonly float PreTolerance = 0.2f;
    public static readonly float PostTolerance = 0.2f;

    public int Movement => m_Movement;
    public bool Running => m_Running;
    public Vector2 MouseDelta => m_MouseDelta;
    public Vector2 MousePosition => m_MousePosition;
    public bool Jumping => m_JumpRequestTime >= (Time.unscaledTime - PreTolerance);
    public bool Crawling => m_CrawlRequestTime >= (Time.unscaledTime - PreTolerance);
    public bool Melee => m_MeleeRequestTime >= (Time.unscaledTime - PreTolerance);

    private int m_Movement;
    private bool m_Running;
    private Vector2 m_MouseDelta;
    private Vector2 m_MousePosition;
    private float m_JumpRequestTime;
    private float m_CrawlRequestTime;
    private float m_MeleeRequestTime;
}
