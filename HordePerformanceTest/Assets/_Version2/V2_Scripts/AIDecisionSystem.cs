using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Profiling;
using UnityEngine.Profiling;
using UnityEngine;

[BurstCompile]
public partial struct AIDecisionSystem : ISystem
{
    // Static marker + manual stopwatch
    static readonly ProfilerMarker DecisionMarker = new ProfilerMarker("AIDecisionSystem_Total");

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        // Start timestamp (CPU now)
        double start = Time.realtimeSinceStartupAsDouble;

        // Send dt to jobs - IJobEntity-scheduler
        var job = new AIDecisionJob
        {
            DeltaTime = dt
        };

        // Schedule + complete for measurement
        DecisionMarker.Begin();
        var handle = job.ScheduleParallel(state.Dependency);
        state.Dependency = handle;
        state.Dependency.Complete();
        DecisionMarker.End();

        double end = Time.realtimeSinceStartupAsDouble;
        double elapsedMs = (end - start) * 1000.0;

        PerfSampler.RecordDecisionMs((float)elapsedMs);

    }

    // IJobEntity generates query based on parametertypes.
    [BurstCompile]
    public partial struct AIDecisionJob : IJobEntity
    {
        public float DeltaTime;

        void Execute(ref DecisionData decision,
                     ref MoveDir dir,
                     ref RNG rngRef)
        {
            var d = decision;
            d.Remaining -= DeltaTime;

            if (d.Remaining <= 0f)
            {
                // Pick current rng state
                var rng = rngRef.Value;

                // New timer
                d.Remaining = rng.NextFloat(d.Range.x, d.Range.y);

                // Pick new direction: right, left, up, down, empty, empty
                int pick = (int)math.floor(rng.NextFloat(0, 6));
                float2 newDir;
                switch (pick)
                {
                    case 0: newDir = new float2(1, 0); break;
                    case 1: newDir = new float2(-1, 0); break;
                    case 2: newDir = new float2(0, 1); break;
                    case 3: newDir = new float2(0, -1); break;
                    default: newDir = float2.zero; break;
                }

                dir.Value = newDir;

                // Save rng-state
                rngRef.Value = rng;
            }

            decision = d;
        }
    }
}
