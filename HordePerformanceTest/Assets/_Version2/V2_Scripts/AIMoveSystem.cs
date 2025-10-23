using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using Unity.Profiling;

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

                // Rörelse i XY-plan
                float2 delta = dir.ValueRO.Value * speed.ValueRO.Value * dt;
                transform.Position.xy += delta;

                // Utanför bounds?
                float halfX = bounds.ValueRO.Size.x * 0.5f;
                float halfY = bounds.ValueRO.Size.y * 0.5f;

                if (transform.Position.x >= halfX || transform.Position.x <= -halfX ||
                    transform.Position.y >= halfY || transform.Position.y <= -halfY)
                {
                    // Teleportera till slumpad punkt inom bounds.
                    var rng = rngRef.ValueRW.Value;
                    float x = rng.NextFloat(-halfX, halfX);
                    float y = rng.NextFloat(-halfY, halfY);
                    transform.Position = new float3(x, y, 0f);
                    rngRef.ValueRW.Value = rng; // Spara rng-state
                }

                lt.ValueRW = transform;
            }

            kMoved.Value = processed;
        }
    }
}
