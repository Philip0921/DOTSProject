using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using Unity.Profiling;
using Unity.VisualScripting.Dependencies.Sqlite;

[BurstCompile]
public partial struct AIMoveSystem : ISystem
{
    static readonly ProfilerCategory kCat = ProfilerCategory.Scripts;
    static readonly ProfilerMarker kMarker = new ProfilerMarker(kCat, "AIMoveSystem.Update");
    static ProfilerCounterValue<int> kMoved =
        new ProfilerCounterValue<int>(kCat, "AI Entities Moved (frame)", ProfilerMarkerDataUnit.Count);

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        using (kMarker.Auto())
        {
            float dt = SystemAPI.Time.DeltaTime;
            int processed = 0;


            foreach ((RefRW<LocalTransform> lt, RefRW<Speed> speed, RefRW<MoveDir> dir, RefRW<MapBounds> bounds, RefRW<RNG> rngRef) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRW<Speed>, RefRW<MoveDir>, RefRW<MapBounds>, RefRW<RNG>>())
            {
                processed++;
                var transform = lt.ValueRW;

                float2 center = bounds.ValueRO.Center;
                float2 size = bounds.ValueRO.Size;

                // Rörelse i XY-plan
                // Rörelse: pos += dir * speed * dt
                float3 pos = transform.Position;
                float2 d = dir.ValueRO.Value;
                float v = speed.ValueRO.Value;

                pos.x += d.x * v * dt;
                pos.y += d.y * v * dt;


                // Utanför bounds?
                float halfX = bounds.ValueRO.Size.x * 0.5f;
                float halfY = bounds.ValueRO.Size.y * 0.5f;

                float minX = center.x - halfX;
                float maxX = center.x + halfX;
                float minY = center.y - halfY;
                float maxY = center.y + halfY;

                pos.x = math.clamp(pos.x, minX, maxX);
                pos.y = math.clamp(pos.y, minY, maxY);

                lt.ValueRW.Position = pos;
            }

            kMoved.Value = processed;
        }
    }
}
