using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class AISpawnAuthoring : MonoBehaviour
{
    [Header("Prefabs & Count")]
    public GameObject aiPrefab;

    [Header("Spawn Area")]
    public Vector2 center = Vector2.zero;
    public Vector2 size = new Vector2(70f, 40f);

    class Baker : Baker<AISpawnAuthoring>
    {
        public override void Bake(AISpawnAuthoring a)
        {
            Entity e = GetEntity(TransformUsageFlags.None);

            float2 prefabDecisionRange = float2.zero;
            if (a.aiPrefab != null)
            {
                var aiUnitAuthoring = a.aiPrefab.GetComponent<AIUnitAuthoring>();
                if (aiUnitAuthoring != null)
                {
                    prefabDecisionRange = aiUnitAuthoring.decisionRange;
                }
            }

            AddComponent(e, new AISpawnConfig
            {
                SpawnCount = 0,
                Center = a.center,
                Size = a.size,
                DecisionRange = prefabDecisionRange
            });
            AddComponent<AISpawnState>(e);
            AddComponent(e, new PrefabRef
            {
                Prefab = GetEntity(a.aiPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.lightBlue;
        Gizmos.DrawWireCube(center, new Vector3(size.x, size.y, 0));
    }
}
