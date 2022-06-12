using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCollider : MonoBehaviour
{
    public Dictionary<BodyState, UnitColliderInfo> Info => m_TemplateInfo;

    private Unit m_Unit;
    private Animator m_Animator;
    private BodyState m_CurrentState;
    private Dictionary<BodyState, Transform> m_Templates = new Dictionary<BodyState, Transform>();
    private Dictionary<BodyState, Collider2D[]> m_TemplateColliders = new Dictionary<BodyState, Collider2D[]>();
    private Dictionary<BodyState, UnitColliderInfo> m_TemplateInfo = new Dictionary<BodyState, UnitColliderInfo>();
    private ContactFilter2D m_OverlapFilter = new ContactFilter2D();

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_Unit = transform.parent.GetComponent<Unit>();
        m_Unit.OnBodyStateChanged += SetState;
        m_OverlapFilter.SetLayerMask(LayerMask.GetMask("Environment"));
    }

    private void Start()
    {
        CreateTemplates();
        SetState(BodyState.Standing);
    }

    private void CreateTemplates()
    {
        m_Templates.Clear();
        m_TemplateColliders.Clear();
        m_TemplateInfo.Clear();

        // Set collider to each state and duplicate the colliders into a template gameobject
        foreach (BodyState bodyState in (BodyState[])Enum.GetValues(typeof(BodyState)))
        {
            SetState(bodyState);
            Transform template = new GameObject("Template_" + bodyState.ToString()).transform;
            m_Templates.Add(bodyState, template);
            foreach (Transform child in transform)
            {
                Instantiate(child, template, false);
            }
        }

        // Create a parent for the new templates
        Transform templateParent = new GameObject("Templates").transform;
        templateParent.SetParent(transform);
        templateParent.localPosition = Vector3.zero;

        // Move each template under the parent, zero its position and disable it
        foreach (KeyValuePair<BodyState, Transform> pair in m_Templates)
        {
            pair.Value.SetParent(templateParent);
            pair.Value.localPosition = Vector3.zero;
            pair.Value.gameObject.SetActive(false);

            // Store the colliders
            Collider2D[] templateColliders = pair.Value.GetComponentsInChildren<Collider2D>();
            m_TemplateColliders.Add(pair.Key, templateColliders);

            // Generate ColliderInfo
            UnitColliderInfo info = new UnitColliderInfo(templateColliders);
            m_TemplateInfo.Add(pair.Key, info);
        }
    }

    private void SetState(BodyState state, float duration = 0.0f)
    {
        if (state == m_CurrentState) { return; }
        m_CurrentState = state;
        if (duration == 0.0f)
        {
            m_Animator.Play(state.ToString(), 0, 1.0f);
            m_Animator.Update(0);
        }
        else
        {
            m_Animator.speed = 1.0f / duration;
            m_Animator.SetTrigger(state.ToString());
        }
    }

    public bool Overlap(BodyState state, Vector2 localOffset, bool debug = false)
    {
        Transform template = m_Templates[state];
        template.localPosition = localOffset;
        template.localScale = Vector3.one;
        template.gameObject.SetActive(true);

        Collider2D[] templateColliders = m_TemplateColliders[state];
        foreach (Collider2D collider in templateColliders)
        {
            Collider2D[] contacts = new Collider2D[1];
            int contactCount = collider.OverlapCollider(m_OverlapFilter, contacts);
            bool overlap = contactCount > 0;
            if (debug)
            {
                DebugExtension.DebugColliders(templateColliders, Vector2.zero, overlap ? Color.green : Color.red);
            }
            if (overlap)
            {
                template.gameObject.SetActive(false);
                return true;
            }
        }

        template.gameObject.SetActive(false);
        return false;
    }

    public bool Overlap(BodyState state, Vector2 localOffset, Vector2 scale, bool debug = false)
    {
        Transform template = m_Templates[state];
        template.localPosition = localOffset;
        template.localScale = new Vector3(scale.x, scale.y, 1);
        template.gameObject.SetActive(true);

        Collider2D[] templateColliders = m_TemplateColliders[state];
        foreach (Collider2D collider in templateColliders)
        {
            Collider2D[] contacts = new Collider2D[1];
            int contactCount = collider.OverlapCollider(m_OverlapFilter, contacts);
            bool overlap = contactCount > 0;
            if (debug)
            {
                DebugExtension.DebugColliders(templateColliders, Vector2.zero, overlap ? Color.green : Color.red);
            }
            if (overlap)
            {
                template.gameObject.SetActive(false);
                return true;
            }
        }

        template.gameObject.SetActive(false);
        return false;
    }

}
