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
        state.RequireForUpdate<AISpawnState>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach ((RefRO<AISpawnConfig> cfg, RefRO<PrefabRef> prefabRef, var entity) in SystemAPI.Query<RefRO<AISpawnConfig>, RefRO<PrefabRef>>().WithEntityAccess())
        {
            int n = cfg.ValueRO.SpawnCount;

            using var nAlc = new NativeArray<Entity>(n, Allocator.Temp);

            ecb.Instantiate(prefabRef.ValueRO.Prefab, nAlc);

            // Slumpa startpositioner
            uint seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);
            var rng = new Unity.Mathematics.Random(seed);
            float halfX = cfg.ValueRO.Size.x * 0.5f;
            float halfY = cfg.ValueRO.Size.y * 0.5f;

            for (int i = 0; i < n; i++)
            {
                float x = rng.NextFloat(cfg.ValueRO.Center.x - halfX, cfg.ValueRO.Center.x + halfX);
                float y = rng.NextFloat(cfg.ValueRO.Center.y - halfY, cfg.ValueRO.Center.y + halfY);
                ecb.SetComponent(nAlc[i], LocalTransform.FromPositionRotationScale(new float3(x, y, 0f), quaternion.identity, 1f));
            }

            ecb.RemoveComponent<AISpawnState>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
