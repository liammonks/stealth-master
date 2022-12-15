using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkUnitData : MonoBehaviour
{
    public class ClientUnit
    {
        public ushort PrefabIndex;
        public Unit Unit;

        public ClientUnit(ushort prefabIndex, Unit unit)
        {
            PrefabIndex = prefabIndex;
            Unit = unit;
        }
    }

    public Action<Unit> OnUnitSpawned;
    public Action<Unit> OnUnitDestroyed;

    public List<Unit> UnitPrefabs;
    public Dictionary<ushort, ClientUnit> ClientUnits = new Dictionary<ushort, ClientUnit>();

    public void SpawnUnit(ushort ID, ushort prefabIndex, Vector2 position)
    {
        Unit unit = Instantiate(UnitPrefabs[prefabIndex], transform);
        unit.transform.SetParent(null);
        unit.transform.position = position;
        unit.transform.name = $"Unit [{ID}]";
        ClientUnits.Add(ID, new ClientUnit(prefabIndex, unit));
        OnUnitSpawned?.Invoke(unit);
    }

    public void DestroyUnit(ushort ID)
    {
        if (!ClientUnits.ContainsKey(ID)) { return; }
        OnUnitDestroyed?.Invoke(ClientUnits[ID].Unit);
        Destroy(ClientUnits[ID].Unit.gameObject);
        ClientUnits.Remove(ID);
    }
}
