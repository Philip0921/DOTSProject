using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]

public partial struct AIWanderSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        foreach ((RefRW<LocalTransform> lt, RefRW<Velocity2D> vel, RefRW<Wander2D> wander) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRW<Velocity2D>, RefRW<Wander2D>>())
        {
            var w = wander.ValueRW;
            w.TimeSinceTurn += dt;

            if (w.TimeSinceTurn >= w.TurnInterval)
            {
                w.TimeSinceTurn = 0;

                var rnd = Unity.Mathematics.Random.CreateFromIndex((uint)(lt.GetHashCode() ^ (int)(dt * 10000)));
                float2 jitter = rnd.NextFloat2Direction() * 0.75f;
                var newDir = math.normalize(vel.ValueRW.values + jitter);
                vel.ValueRW.values = newDir * w.Speed;
            }

            var t = lt.ValueRW;
            t.Position.xy += vel.ValueRW.values * dt;
            lt.ValueRW = t;

            wander.ValueRW = w;
        }
    }
}
