using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct AISpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AISpawnConfig>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach ((RefRO<AISpawnConfig> cfg, RefRO<PrefabRef> prefabRef, var entity) in SystemAPI.Query<RefRO<AISpawnConfig>, RefRO<PrefabRef>>().WithEntityAccess())
        {
            var count = cfg.ValueRO.SpawnCount;

            var center = cfg.ValueRO.Center;
            var size = cfg.ValueRO.Size;

            for (int spawned = 0; spawned < count; spawned++)
            {
                int n = math.min(count, count - spawned);

                var instances = ecb.Instantiate(prefabRef.ValueRO.Prefab, Allocator.Temp);

                for (int i = 0; i < n; i++)
                {
                    float x = Unity.Mathematics.Random.CreateFromIndex((uint)(spawned + i + 1)).NextFloat(center.x - size.x * 0.5f, center.x + size.x * 0.5f);
                    float y = Unity.Mathematics.Random.CreateFromIndex((uint)(spawned + i + 17)).NextFloat(center.y - size.y * 0.5f, center.y + size.y * 0.5f);

                    ecb.SetComponent(instances[spawned], LocalTransform.FromPositionRotationScale(
                        new float3(x, y, 0f), quaternion.identity, 1f));
                }
            }

            ecb.RemoveComponent<AISpawnConfig>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
