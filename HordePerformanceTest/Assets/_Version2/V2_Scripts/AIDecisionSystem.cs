using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Profiling;

[BurstCompile]
public partial struct AIDecisionSystem : ISystem
{
    static readonly ProfilerCategory kCat = ProfilerCategory.Scripts;
    static readonly ProfilerMarker kMarker = new ProfilerMarker(kCat, "AIDecisionSystem.Update");
    static ProfilerCounterValue<int> kDecided =
        new ProfilerCounterValue<int>(kCat, "AI Entities Decided (frame)", ProfilerMarkerDataUnit.Count);

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        using (kMarker.Auto())
        {
            float dt = SystemAPI.Time.DeltaTime;
            int processed = 0;

            foreach ((RefRW<DecisionData> decision, RefRW<MoveDir> dir, RefRW<RNG> rngRef) in
                SystemAPI.Query<RefRW<DecisionData>, RefRW<MoveDir>, RefRW<RNG>>())
            {
                processed++;
                var d = decision.ValueRW;
                d.Remaining -= dt;

                if (d.Remaining <= 0f)
                {
                    // Ny timer
                    var rng = rngRef.ValueRW.Value;
                    d.Remaining = rng.NextFloat(d.Range.x, d.Range.y);

                    // Välj ny riktning: höger, vänster, upp, ner, tom, tom
                    int pick = (int)math.floor(rng.NextFloat(0, 6));
                    float2 newDir;
                    switch (pick)
                    {
                        case 0: newDir = new float2(1, 0); break; // höger
                        case 1: newDir = new float2(-1, 0); break; // vänster
                        case 2: newDir = new float2(0, 1); break; // upp
                        case 3: newDir = new float2(0, -1); break; // ner
                        default: newDir = float2.zero; break; // tom
                    }

                    dir.ValueRW.Value = newDir;

                    // Spara tillbaka rng-state
                    rngRef.ValueRW.Value = rng;
                }

                decision.ValueRW = d;
            }

            kDecided.Value = processed;
        }
    }
}
