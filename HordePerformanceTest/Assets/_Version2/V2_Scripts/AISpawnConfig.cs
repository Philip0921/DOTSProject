using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct AISpawnConfig : IComponentData
{
    public int SpawnCount;
    public float2 Center;
    public float2 Size;
    public float2 DecisionRange;
}

public struct AISpawnState : IComponentData { } // tom markör

public struct PrefabRef : IComponentData
{
    public Entity Prefab;
}
