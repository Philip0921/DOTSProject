using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

[BurstCompile]
public partial class AISpawnSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimEcb;

    [BurstCompile]
    protected override void OnCreate()
    {
        RequireForUpdate<AISpawnState>();
        _endSimEcb = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {

        var ecb = _endSimEcb.CreateCommandBuffer();

        Profiler.BeginSample("SampleSpawn");

        foreach ((RefRO<AISpawnConfig> cfg, RefRO<PrefabRef> prefabRef, Entity entity) in SystemAPI.Query<RefRO<AISpawnConfig>, RefRO<PrefabRef>>().WithEntityAccess())
        {
            Entity prefab = prefabRef.ValueRO.Prefab;

            int n = cfg.ValueRO.SpawnCount;

            using var spawned = new NativeArray<Entity>(n, Allocator.Temp);

            ecb.Instantiate(prefabRef.ValueRO.Prefab, spawned);

            // Slumpa startpositioner
            uint batchSeed = (uint)UnityEngine.Random.Range(1, int.MaxValue);

            float halfX = cfg.ValueRO.Size.x * 0.5f;
            float halfY = cfg.ValueRO.Size.y * 0.5f;

            for (int i = 0; i < n; i++)
            {
                Entity e = spawned[i];
                var rng = Unity.Mathematics.Random.CreateFromIndex(batchSeed + (uint)i);

                float x = rng.NextFloat(cfg.ValueRO.Center.x - halfX, cfg.ValueRO.Center.x + halfX);
                float y = rng.NextFloat(cfg.ValueRO.Center.y - halfY, cfg.ValueRO.Center.y + halfY);
                ecb.SetComponent(e, LocalTransform.FromPositionRotationScale(new float3(x, y, 0f), quaternion.identity, 1f));

                ecb.SetComponent(e, new RNG { Value = rng });

                // Startdir (desynka start)
                float2 dir;
                switch ((int)math.floor(rng.NextFloat(0, 6)))
                {
                    case 0: dir = new float2(1, 0); break;
                    case 1: dir = new float2(-1, 0); break;
                    case 2: dir = new float2(0, 1); break;
                    case 3: dir = new float2(0, -1); break;
                    default: dir = float2.zero; break;
                }
                ecb.SetComponent(e, new MoveDir { Value = dir });

                var baseDD = EntityManager.GetComponentData<DecisionData>(prefab);
                float rem = math.lerp(baseDD.Range.x, baseDD.Range.y, rng.NextFloat());
                ecb.SetComponent(e, new DecisionData { Remaining = rem, Range = baseDD.Range });

            }
            ecb.RemoveComponent<AISpawnState>(entity);
        }
        Profiler.EndSample();

    }
}

