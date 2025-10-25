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
    // Static marker + manual stopwatch
    static readonly ProfilerMarker SpawnMarker = new ProfilerMarker("AISpawnSystem_Total");
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
        // Start timestamp (CPU now)
        double start = UnityEngine.Time.realtimeSinceStartupAsDouble;

        // ECB ParallelWriter we can use in the job
        var ecbParallel = _endSimEcb.CreateCommandBuffer().AsParallelWriter();

        uint frameSeed = (uint)UnityEngine.Random.Range(1, int.MaxValue);

        var job = new AISpawnJob
        {
            Ecb = ecbParallel,
            FrameSeed = frameSeed
        };

        // Schedule + complete for measurement
        SpawnMarker.Begin();
        var handle = job.ScheduleParallel(Dependency);
        Dependency = handle;
        Dependency.Complete();

        // Tell ECB system to play back after this job finishes
        _endSimEcb.AddJobHandleForProducer(Dependency);

        SpawnMarker.End();

        double end = UnityEngine.Time.realtimeSinceStartupAsDouble;
        double elapsedMs = (end - start) * 1000.0;

        PerfSampler.RecordSpawnMs((float)elapsedMs);

    }

    [BurstCompile]
    public partial struct AISpawnJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public uint FrameSeed;

        // Execute runs for each "spawner entity"
        void Execute([ChunkIndexInQuery] int sortKey,
                     Entity spawnerEntity,
                     in AISpawnConfig cfg,
                     in PrefabRef prefabRef,
                     in AISpawnState spawnState)
        {
            int n = cfg.SpawnCount;

            float halfX = cfg.Size.x * 0.5f;
            float halfY = cfg.Size.y * 0.5f;

            float2 decisionRange = cfg.DecisionRange;

            for (int i = 0; i < n; i++)
            {
                // RNG Instance
                var rng = Unity.Mathematics.Random.CreateFromIndex(FrameSeed + (uint)i);

                // Instantiate
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

                // Initial dir so they don't all stand still
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
                float rem = math.lerp(decisionRange.x, decisionRange.y, rng.NextFloat());

                Ecb.SetComponent(sortKey, e, new DecisionData
                {
                    Remaining = rem,
                    Range = decisionRange
                });
            }

            // Make sure this spawner doesn't run again
            Ecb.RemoveComponent<AISpawnState>(sortKey, spawnerEntity);
        }
    }
}

