using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Profiling;
using UnityEngine;

[BurstCompile]
public partial struct AISpawnSystem : ISystem
{
    static readonly ProfilerCategory kCat = ProfilerCategory.Scripts;
    static readonly ProfilerMarker kMarker = new ProfilerMarker(kCat, "AISpawnSystem.Update");
    static ProfilerCounterValue<int> kSpawned =
        new ProfilerCounterValue<int>(kCat, "AI Entities Spawned (frame)", ProfilerMarkerDataUnit.Count);

    //private EndSimulationEntityCommandBufferSystem _endSimEcb;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AISpawnState>();
        //_endSimEcb = state.World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        using (kMarker.Auto())
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            int spawnedFrame = 0;

            foreach ((RefRO<AISpawnConfig> cfg, RefRO<PrefabRef> prefabRef, var entity) in SystemAPI.Query<RefRO<AISpawnConfig>, RefRO<PrefabRef>>().WithEntityAccess())
            {
                int n = cfg.ValueRO.SpawnCount;

                using var spawned = new NativeArray<Entity>(n, Allocator.Temp);

                ecb.Instantiate(prefabRef.ValueRO.Prefab, spawned);
                spawnedFrame += n;

                // Slumpa startpositioner
                uint batchSeed = (uint)UnityEngine.Random.Range(1, int.MaxValue);

                float halfX = cfg.ValueRO.Size.x * 0.5f;
                float halfY = cfg.ValueRO.Size.y * 0.5f;

                for (int i = 0; i < n; i++)
                {
                    Entity e = spawned[i];
                    var rng = Unity.Mathematics.Random.CreateFromIndex(batchSeed + (uint)i);
                    //ecb.SetComponent(nAlc[i], new RNG { Value = rng });

                    float x = rng.NextFloat(cfg.ValueRO.Center.x - halfX, cfg.ValueRO.Center.x + halfX);
                    float y = rng.NextFloat(cfg.ValueRO.Center.y - halfY, cfg.ValueRO.Center.y + halfY);
                    ecb.SetComponent(e, LocalTransform.FromPositionRotationScale(new float3(x, y, 0f), quaternion.identity, 1f));

                    //// RNG
                    //if (!state.EntityManager.HasComponent<RNG>(e))
                    //    ecb.AddComponent(e, new RNG { Value = rng });
                    //else
                    //    ecb.SetComponent(e, new RNG { Value = rng });

                    //// MoveDir (ge något icke-synkat startvärde)
                    //float2 dir = float2.zero;
                    //switch ((int)math.floor(rng.NextFloat(0, 6)))
                    //{
                    //    case 0: dir = new float2(1, 0); break;
                    //    case 1: dir = new float2(-1, 0); break;
                    //    case 2: dir = new float2(0, 1); break;
                    //    case 3: dir = new float2(0, -1); break;
                    //    default: dir = float2.zero; break;
                    //}
                    //if (!state.EntityManager.HasComponent<MoveDir>(e))
                    //    ecb.AddComponent(e, new MoveDir { Value = dir });
                    //else
                    //    ecb.SetComponent(e, new MoveDir { Value = dir });

                    //// DecisionData: behåll Range om den finns, randomisera Remaining
                    //if (state.EntityManager.HasComponent<DecisionData>(e))
                    //{
                    //    var dd = state.EntityManager.GetComponentData<DecisionData>(e);
                    //    float rem = math.lerp(dd.Range.x, dd.Range.y, rng.NextFloat());
                    //    ecb.SetComponent(e, new DecisionData { Remaining = rem, Range = dd.Range });
                    //}
                    //else
                    //{
                    //    // fallback om prefaben saknar den (borde finnas via AIUnitAuthoring)
                    //    float2 range = new float2(1f, 8f);
                    //    float rem = math.lerp(range.x, range.y, rng.NextFloat());
                    //    ecb.AddComponent(e, new DecisionData { Remaining = rem, Range = range });
                    //}
                }

                ecb.RemoveComponent<AISpawnState>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            kSpawned.Value = spawnedFrame; // profiler-counter
        }
    }
}
