using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using System;

namespace Debugging
{
    public class DebugSelector : MonoBehaviour
    {
        [HideInInspector]
        public Action<Transform> OnSelectionUpdated;
        public Transform SelectedTransform => m_SelectedCollider?.transform;

        [SerializeField]
        private List<Type> m_PriorityTypes = new List<Type>();

        private ColliderRenderer m_ColliderRenderer;

        private Collider2D m_HoveredCollider;
        private Collider2D m_SelectedCollider;

        private bool m_PointerOverUI;
        private Vector2 m_MouseScreenPosition;
        private Vector2 m_MouseWorldPosition;

        private void Awake()
        {
            m_ColliderRenderer = GetComponent<ColliderRenderer>();

            m_PriorityTypes.Add(typeof(Unit));
        }

        private void Update()
        {
            m_PointerOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

            // Get hovered collider
            Vector2 currentMouseWorldPosition = GetMouseWorldPosition(m_MouseScreenPosition);
            if (currentMouseWorldPosition != m_MouseWorldPosition)
            {
                m_MouseWorldPosition = currentMouseWorldPosition;
                m_HoveredCollider = GetOverlapCollider(m_MouseWorldPosition);
            }

            // Draw hovered collider
            if (m_HoveredCollider != null && m_HoveredCollider != m_SelectedCollider)
            {
                m_ColliderRenderer.DrawCollider(m_HoveredCollider, Color.red);
            }

            // Draw selected collider
            if (m_SelectedCollider != null)
            {
                m_ColliderRenderer.DrawCollider(m_SelectedCollider, Color.green);
            }
        }

        private void OnMouseMove(InputValue value)
        {
            m_MouseScreenPosition = value.Get<Vector2>();
        }

        private void OnMouseDown(InputValue value)
        {
            if (value.Get<float>() == 1.0f && !m_PointerOverUI)
            {
                Collider2D previouslySelectedCollider = m_SelectedCollider;
                m_SelectedCollider = m_HoveredCollider;
                if (m_SelectedCollider != previouslySelectedCollider)
                {
                    OnSelectionUpdated?.Invoke(GetPriorityTransform(m_SelectedCollider));
                }
            }
        }

        private Vector2 GetMouseWorldPosition(Vector2 mouseScreenPosition)
        {
            Vector3 screenPos = (Vector3)mouseScreenPosition + Vector3.forward;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            Vector3 direction = (worldPos - Camera.main.transform.position).normalized;

            Ray ray = Camera.main.ScreenPointToRay((Vector3)screenPos + Vector3.forward);
            Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, 0.25f));
            if (plane.Raycast(ray, out float distance))
            {
                return Camera.main.transform.position + (direction * distance);
            }
            return Vector2.zero;
        }

        private Collider2D GetOverlapCollider(Vector2 worldPosition)
        {
            Collider2D overlap = Physics2D.OverlapPoint(worldPosition);
            return overlap;
        }

        private Transform GetPriorityTransform(Collider2D collider)
        {
            if (collider == null) { return null; }

            foreach (Type type in m_PriorityTypes)
            {
                Component priorityParent = collider.GetComponentInParent(type);

                if (priorityParent != null)
                {
                    return priorityParent.transform;
                }
            }
            return collider.transform;
        }

    }
}