using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] private UnitTemplate unitVariant;

    private void Awake()
    {
        unitVariant.CreateUnit(transform.position);
    }
}
