using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;

[BurstCompile]
public partial struct AIMoveSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        foreach ((RefRW<LocalTransform> lt, RefRW<Speed> speed, RefRW<MoveDir> dir, RefRW<MapBounds> bounds, RefRW<RNG> rngRef) in
        SystemAPI.Query<RefRW<LocalTransform>, RefRW<Speed>,RefRW<MoveDir>,RefRW<MapBounds>, RefRW<RNG>>())
        {
            var transform = lt.ValueRW;

            // Rörelse i XY-plan
            float2 delta = dir.ValueRO.Value * speed.ValueRO.Value * dt;
            transform.Position.xy += delta;

            // Utanför bounds?
            float halfX = bounds.ValueRO.Size.x * dt;
            float halfY = bounds.ValueRO.Size.y * dt;

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
    }
}
