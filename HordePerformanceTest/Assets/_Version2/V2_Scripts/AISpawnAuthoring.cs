using UnityEngine;
using Unity.Entities;

public class AISpawnAuthoring : MonoBehaviour
{
    [Header("Prefabs & Count")]
    public GameObject aiPrefab;
    public int spawnCount = 100;

    [Header("Spawn Area")]
    public Vector2 center = Vector2.zero;
    public Vector2 size = new Vector2(70f, 40f);

    class Baker : Baker<AISpawnAuthoring>
    {
        public override void Bake(AISpawnAuthoring a)
        {
            Entity e = GetEntity(TransformUsageFlags.None);

            AddComponent(e, new AISpawnConfig
            {
                SpawnCount = a.spawnCount,
                Center = a.center,
                Size = a.size,
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
