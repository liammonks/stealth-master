using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkUnitData : MonoBehaviour
{
    public Action<Unit> OnUnitSpawned;
    public Action<Unit> OnUnitDestroyed;

    [SerializeField]
    private List<Unit> m_UnitPrefabs;

    public Dictionary<ushort, Unit> ClientUnits = new Dictionary<ushort, Unit>();
    public Dictionary<ushort, ushort> ClientUnitPrefabIndicies = new Dictionary<ushort, ushort>();

    public void SpawnUnit(ushort ID, ushort prefabIndex, Vector2 position)
    {
        Unit unit = Instantiate(m_UnitPrefabs[prefabIndex], transform);
        unit.transform.SetParent(null);
        unit.transform.position = position;
        unit.transform.name = $"Unit [{ID}]";
        ClientUnits.Add(ID, unit);
        ClientUnitPrefabIndicies.Add(ID, prefabIndex);
        OnUnitSpawned?.Invoke(unit);
    }

    public void DestroyUnit(ushort ID)
    {
        if (!ClientUnits.ContainsKey(ID)) { return; }
        OnUnitDestroyed?.Invoke(ClientUnits[ID]);
        Destroy(ClientUnits[ID].gameObject);
        ClientUnits.Remove(ID);
        ClientUnitPrefabIndicies.Remove(ID);
    }
}
