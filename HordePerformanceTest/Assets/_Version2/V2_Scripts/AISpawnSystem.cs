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

                var instances = new Unity.Collections.NativeArray<Entity>(n, Allocator.Temp);
                    ecb.Instantiate(prefabRef.ValueRO.Prefab, instances);

                for (int i = 0; i < n; i++)
                {
                    var rngPos = Unity.Mathematics.Random.CreateFromIndex((uint)(spawned + i + 1));
                    float x = rngPos.NextFloat(center.x - size.x * 0.5f, center.x + size.x * 0.5f);
                    float y = rngPos.NextFloat(center.y - size.y * 0.5f, center.y + size.y * 0.5f);

                    ecb.SetComponent(instances[i], LocalTransform.FromPositionRotationScale(
                        new float3(x, y, 0f), quaternion.identity, 1f));

                    var rngDir = Unity.Mathematics.Random.CreateFromIndex((uint)(spawned + i + 31));
                    float2 dir = math.normalize(rngDir.NextFloat2Direction());

                    ecb.AddComponent(instances[i], new Velocity2D { values = dir * cfg.ValueRO.Speed });
                    ecb.AddComponent(instances[i], new Wander2D
                    {
                        Speed = cfg.ValueRO.Speed,
                        TurnInterval = cfg.ValueRO.TurnInterval,
                        TimeSinceTurn = 0f
                    });
                }
            }

            ecb.RemoveComponent<AISpawnConfig>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
