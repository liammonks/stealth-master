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

    public List<Unit> UnitPrefabs;
    public Dictionary<ushort, ClientUnit> ClientUnits = new Dictionary<ushort, ClientUnit>();

    public void SpawnUnit(ushort ID, ushort prefabIndex, Vector2 position)
    {
        Unit unit = Instantiate(UnitPrefabs[prefabIndex], transform);
        unit.transform.SetParent(null);
        unit.transform.position = position;
        unit.transform.name = $"Unit [{ID}]";
        ClientUnits.Add(ID, new ClientUnit(prefabIndex, unit));
    }

    public void DestroyUnit(ushort ID)
    {
        if (!ClientUnits.ContainsKey(ID)) { return; }
        Destroy(ClientUnits[ID].Unit.gameObject);
        ClientUnits.Remove(ID);
    }
}
