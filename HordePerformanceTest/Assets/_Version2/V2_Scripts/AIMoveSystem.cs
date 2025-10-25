using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using Unity.Profiling;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine.Profiling;

[BurstCompile]
public partial struct AIMoveSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        float dt = SystemAPI.Time.DeltaTime;

        var job = new AIMoveJob
        {
            DeltaTime = dt
        };

        Profiler.BeginSample("SampleMove");
        job.ScheduleParallel();
        Profiler.EndSample();

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

            // Rörelse i XY-plan
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

            // Skriv tillbaka
            lt.Position = pos;
        }
    }
}
