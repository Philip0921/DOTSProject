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

        // ECB ParallelWriter we can use in the job
        var ecbParallel = _endSimEcb.CreateCommandBuffer().AsParallelWriter();

        Entity firstPrefab = Entity.Null;
        DecisionData baseDD = default;

        {
            // Just read first available prefab+DecisionData for seeding
            foreach (var (prefabRefRO, entity) in
                     SystemAPI.Query<RefRO<PrefabRef>>().WithEntityAccess())
            {
                firstPrefab = prefabRefRO.ValueRO.Prefab;
                baseDD = EntityManager.GetComponentData<DecisionData>(firstPrefab);
                break;
            }
        }

        // Vi vill ha en "seed per frame" men UnityEngine.Random kan inte användas i Burst,
        // så vi genererar ett seed här på main thread och skickar in.
        uint frameSeed = (uint)UnityEngine.Random.Range(1, int.MaxValue);

        var job = new AISpawnJob
        {
            Ecb = ecbParallel,
            FrameSeed = frameSeed
        };

        // Schedule the job over all spawner entities
        Profiler.BeginSample("SampleSpawn");
        Dependency = job.ScheduleParallel(Dependency);
        Profiler.EndSample();

        // Tell ECB system to play back after this job finishes
        _endSimEcb.AddJobHandleForProducer(Dependency);

    }

    [BurstCompile]
    public partial struct AISpawnJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public uint FrameSeed;
        public DecisionData BaseDecision;

        // Execute körs för varje "spawner entity" som har de här komponenterna
        void Execute([ChunkIndexInQuery] int sortKey,
                     Entity spawnerEntity,
                     in AISpawnConfig cfg,
                     in PrefabRef prefabRef,
                     in AISpawnState spawnState)
        {
            int n = cfg.SpawnCount;

            float halfX = cfg.Size.x * 0.5f;
            float halfY = cfg.Size.y * 0.5f;

            for (int i = 0; i < n; i++)
            {
                // Skapa rng för den här instansen
                var rng = Unity.Mathematics.Random.CreateFromIndex(FrameSeed + (uint)i);

                // Instantiera
                Entity e = Ecb.Instantiate(sortKey, prefabRef.Prefab);

                // Position
                float x = rng.NextFloat(cfg.Center.x - halfX, cfg.Center.x + halfX);
                float y = rng.NextFloat(cfg.Center.y - halfY, cfg.Center.y + halfY);

                Ecb.SetComponent(sortKey, e,
                    LocalTransform.FromPositionRotationScale(
                        new float3(x, y, 0f),
                        quaternion.identity,
                        1f));

                // RNG component init
                Ecb.SetComponent(sortKey, e, new RNG { Value = rng });

                // Start dir
                float2 dir;
                switch ((int)math.floor(rng.NextFloat(0, 6)))
                {
                    case 0: dir = new float2(1, 0); break;
                    case 1: dir = new float2(-1, 0); break;
                    case 2: dir = new float2(0, 1); break;
                    case 3: dir = new float2(0, -1); break;
                    default: dir = float2.zero; break;
                }
                Ecb.SetComponent(sortKey, e, new MoveDir { Value = dir });

                // Give them DecisionData with randomized Remaining based on prefab baseline
                float rem = math.lerp(BaseDecision.Range.x, BaseDecision.Range.y, rng.NextFloat());

                Ecb.SetComponent(sortKey, e, new DecisionData
                {
                    Remaining = rem,
                    Range = BaseDecision.Range
                });
            }

            // Make sure this spawner doesn't run again
            Ecb.RemoveComponent<AISpawnState>(sortKey, spawnerEntity);
        }
    }
}

