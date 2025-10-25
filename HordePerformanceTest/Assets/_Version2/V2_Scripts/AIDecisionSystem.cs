using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Profiling;
using UnityEngine.Profiling;

[BurstCompile]
public partial struct AIDecisionSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;


        // Skicka in dt till jobben via IJobEntity-scheduler
        var job = new AIDecisionJob
        {
            DeltaTime = dt
        };

        Profiler.BeginSample("SampleDecision");
        job.ScheduleParallel();
        Profiler.EndSample();

    }

    // IJobEntity genererar queryn åt oss baserat på parametertyperna.
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
                // Plocka ut current rng state
                var rng = rngRef.Value;

                // Ny timer
                d.Remaining = rng.NextFloat(d.Range.x, d.Range.y);

                // Välj ny riktning: höger, vänster, upp, ner, tom, tom
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

                // Spara tillbaka rng-state
                rngRef.Value = rng;
            }

            decision = d;
        }
    }
}
