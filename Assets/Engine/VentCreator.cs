using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VentCreator : MonoBehaviour
{
    [SerializeField] private List<Transform> points = new List<Transform>();
    [SerializeField] private List<Transform> models = new List<Transform>();
    [SerializeField] private GameObject tubePrefab;

    [ContextMenu("Add Point")]
    private void AddPoint()
    {
        GameObject newPoint = new GameObject("point_" + points.Count);
        newPoint.transform.SetParent(GetParent("Points"));
        points.Add(newPoint.transform);
        GenerateModels();
    }

    [ContextMenu("Generate Models")]
    private void GenerateModels()
    {
        // Clear previous models
        for (int i = models.Count - 1; i >= 0; --i)
        {
            DestroyImmediate(models[i].gameObject);
        }
        models.Clear();
        for (int i = 0; i < points.Count - 1; ++i)
        {
            GameObject model = Instantiate(tubePrefab, points[i].position, points[i].rotation, GetParent("Models"));
            models.Add(model.transform);
        }
    }
    
    private void Update() {
        for (int i = 0; i < points.Count - 1; ++i)
        {
            Debug.DrawLine(points[i].position, points[i + 1].position, Color.cyan);
            models[i].position = points[i].position;
            models[i].rotation = Quaternion.LookRotation(points[i + 1].position - points[i].position, Vector3.forward);
            models[i].localScale = new Vector3(1, 1, Vector3.Distance(points[i].position, points[i + 1].position));
        }
    }
    
    private Transform GetParent(string parentName)
    {
        Transform pointsParent = transform.Find(parentName);
        if(pointsParent != null)
        {
            return pointsParent;
        }
        else
        {
            GameObject newPointParent = new GameObject(parentName);
            newPointParent.transform.SetParent(transform);
            return newPointParent.transform;
        }
    }
}
