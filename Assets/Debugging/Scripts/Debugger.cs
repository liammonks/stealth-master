using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

namespace Debugging
{
    public class Debugger : SerializedMonoBehaviour
    {
        [SerializeField]
        private Dictionary<System.Type, BaseWindow> componentWindows;
        [SerializeField]
        private Transform windowParent;

        private List<System.Type> componentPriorityList;
        private Component hoveredComponent;
        private Component debugComponent;
        private BaseWindow activeWindow;

        private bool m_PointerOverUI;
        private Vector2 m_MouseScreenPosition;

        private void Awake()
        {
            componentPriorityList = new List<System.Type>(componentWindows.Keys);
            foreach (System.Type componentType in componentPriorityList)
            {
                componentWindows[componentType] = Instantiate(componentWindows[componentType], windowParent);
            }
        }

        private void Update()
        {
            m_PointerOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            if (hoveredComponent != null && hoveredComponent != debugComponent) { DrawColliders(hoveredComponent.transform, Color.red); }
            if (debugComponent != null) { DrawColliders(debugComponent.transform, Color.green); }
            if (debugComponent == null && activeWindow != null) { activeWindow.Hide(); }
        }

        private void OnMouseMove(InputValue value)
        {
            m_MouseScreenPosition = value.Get<Vector2>();
            hoveredComponent = GetOverlapObject(GetMouseWorldPosition(m_MouseScreenPosition));
        }

        private void OnMouseDown(InputValue value)
        {
            if (value.Get<float>() == 1.0f && !m_PointerOverUI)
            {
                debugComponent = GetOverlapObject(GetMouseWorldPosition(m_MouseScreenPosition));
                OnComponentSelected();
            }
        }

        private void OnSlowTime(InputValue value)
        {
            if (value.Get<float>() != 1.0f) { return; }
            if (Time.timeScale == 1.0f) { Time.timeScale = 0.2f; }
            else { Time.timeScale = 1.0f; }
        }

        private Vector3 GetMouseWorldPosition(Vector2 mouseScreenPosition)
        {
            Vector3 screenPos = mouseScreenPosition;
            screenPos.z = 1f;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            Vector3 direction = (worldPos - Camera.main.transform.position).normalized;

            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, 0.25f));
            if (plane.Raycast(ray, out float distance))
            {
                return Camera.main.transform.position + (direction * distance);
            }
            return Vector3.zero;
        }

        private Component GetOverlapObject(Vector3 worldPosition)
        {
            Collider2D overlap = Physics2D.OverlapPoint(worldPosition);
            if (overlap)
            {
                return GetPriorityType(overlap.gameObject);
            }
            return null;
        }

        private Component GetPriorityType(GameObject gameObject)
        {
            foreach (System.Type type in componentPriorityList)
            {
                Component foundType = TypeRecursiveSearch(gameObject.transform, type);
                if (foundType != null) { return foundType; }
            }
            return null;

            Component TypeRecursiveSearch(Transform root, System.Type type)
            {
                Component foundType = root.GetComponent(type);
                if (foundType != null) { return foundType; }
                if (root.parent != null) { return TypeRecursiveSearch(root.parent, type); }
                return null;
            }
        }

        private void OnComponentSelected()
        {
            if (activeWindow != null) { activeWindow.Hide(); }
            if (debugComponent == null) { return; }
            activeWindow = componentWindows[debugComponent.GetType()];
            activeWindow.Show(debugComponent);
        }

        private void DrawColliders(Transform root, Color color)
        {
            Collider2D collider = root.GetComponent<Collider2D>();
            if (collider != null && collider.isActiveAndEnabled)
            {
                if (collider is BoxCollider2D)
                {
                    DebugExtension.DebugBounds(collider.bounds, color);
                }
                else if (collider is CircleCollider2D)
                {
                    CircleCollider2D circleCollider = collider as CircleCollider2D;
                    DebugExtension.DebugCircle(circleCollider.bounds.center, Vector3.forward, color, circleCollider.radius);
                }
            }
                
            foreach (Transform child in root)
            {
                DrawColliders(child, color);
            }
        }

    }
}