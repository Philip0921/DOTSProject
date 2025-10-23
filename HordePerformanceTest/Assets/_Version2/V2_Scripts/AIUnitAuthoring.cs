using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Entities;
using UnityEditor.PackageManager;

public class VisualPrefabRef : IComponentData
{
    public GameObject Prefab;
}

public class AIUnitAuthoring : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject visualPrefab;

    [Header("Movement")]
    public float speed = 1.0f;

    [Header("Decision Interval (seconds)")]
    public float2 decisionRange = new float2(1f, 5f);

    [Header("Map")]
    public float2 mapCenter = float2.zero;
    public float2 mapSize = new float2(70f, 40f);

    class Baker : Baker<AIUnitAuthoring>
    {
        public override void Bake(AIUnitAuthoring a)
        {
            Entity e = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(e, new Speed { Value = a.speed });
            AddComponent(e, new MoveDir { Value = float2.zero });
            AddComponent(e, new RNG { Value = new Unity.Mathematics.Random(1u) });
            AddComponent(e, new DecisionData
            {
                Range = a.decisionRange,
                Remaining = a.decisionRange.x
            });

            AddComponent(e, new MapBounds
            {
                Center = a.mapCenter,
                Size = a.mapSize
            });

            if (a.visualPrefab != null)
            {
                // Den här länken låter DOTS skapa en separat GameObject-instans
                AddComponentObject(e, new VisualPrefabRef { Prefab = a.visualPrefab });
            }
        }
    }
}
