using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using Unity.Profiling;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine.Profiling;
using UnityEngine;

[BurstCompile]
public partial struct AIMoveSystem : ISystem
{
    // Static marker + manual stopwatch
    static readonly ProfilerMarker MoveMarker = new ProfilerMarker("AIMoveSystem_Total");

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        float dt = SystemAPI.Time.DeltaTime;

        // Start timestamp (CPU now)
        double start = Time.realtimeSinceStartupAsDouble;

        var job = new AIMoveJob
        {
            DeltaTime = dt
        };

        // Schedule + complete for measurement
        MoveMarker.Begin();
        var handle = job.ScheduleParallel(state.Dependency);
        state.Dependency = handle;
        state.Dependency.Complete();
        MoveMarker.End();

        double end = Time.realtimeSinceStartupAsDouble;
        double elapsedMs = (end - start) * 1000.0;

        PerfSampler.RecordMoveMs((float)elapsedMs);

    }

    [BurstCompile]
    public partial struct AIMoveJob : IJobEntity
    {
        public float DeltaTime;

        void Execute(ref LocalTransform lt,
                     in Speed speed,
                     in MoveDir dir,
                     in MapBounds bounds)
        {

            // Movement in XY-plane
            float3 pos = lt.Position;
            float2 d = dir.Value;
            float v = speed.Value;

            pos.x += d.x * v * DeltaTime;
            pos.y += d.y * v * DeltaTime;

            // Bounds clamp
            float2 center = bounds.Center;
            float2 size = bounds.Size;

            float halfX = size.x * 0.5f;
            float halfY = size.y * 0.5f;

            float minX = center.x - halfX;
            float maxX = center.x + halfX;
            float minY = center.y - halfY;
            float maxY = center.y + halfY;

            pos.x = math.clamp(pos.x, minX, maxX);
            pos.y = math.clamp(pos.y, minY, maxY);

            // Write back
            lt.Position = pos;
        }
    }
}
