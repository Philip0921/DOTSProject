using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Entities;

public class AIUnitAuthoring : MonoBehaviour
{
    [SerializeField] float speed = 0.2f;
    [SerializeField] Vector2 decisionTime = new Vector2(1f, 8f);
    [SerializeField] Vector2 mapSize = new Vector2(70f, 40f);

    // Startvärden (samma idé som din Start(): slumpa timer & riktning)
    [SerializeField] bool randomizeOnBake = true;

    class Baker : Baker<AIUnitAuthoring>
    {
        public override void Bake(AIUnitAuthoring a)
        {
            Entity e = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(e, new Speed { Value = a.speed });
            AddComponent(e, new DecisionData
            {
                Remaining = 0f,
                Range = new float2(a.decisionTime.x, a.decisionTime.y)
            });

            AddComponent(e, new MoveDir { Value = float2.zero });
            AddComponent(e, new MapBounds { Size = new float2(a.mapSize.x, a.mapSize.y) });

            // RNG
            uint seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);
            var rng = new Unity.Mathematics.Random(seed);
            AddComponent(e, new RNG { Value = rng });

            // Sätt initial position
            // Initiera besluttimer + initial riktning
            float renaining = math.select(
                math.lerp(a.decisionTime.x, a.decisionTime.y, 0.5f),
                a.decisionTime.x + (a.decisionTime.y - a.decisionTime.x) * rng.NextFloat(),
                a.randomizeOnBake
                );

            var firstDir = float2.zero;
            if ( a.randomizeOnBake )
            {
                // Välj slumpmässig riktning i listan av val: höger, vänster, upp, ner, tom, tom
                int pick = (int)math.floor(rng.NextFloat(0, 6));
                switch (pick)
                {
                    case 0: firstDir = new float2(1, 0); break; // höger
                    case 1: firstDir = new float2(-1, 0); break; // vänster
                    case 2: firstDir = new float2(0, 1); break; // upp
                    case 3: firstDir = new float2(0, -1); break; // ner
                    default: firstDir = float2.zero; break; // tom
                }
            }

            // Skriv tillbaka intvärden
            SetComponent(e, new DecisionData { Remaining = renaining, Range = new float2(a.decisionTime.x, a.decisionTime.y) });
            SetComponent(e, new MoveDir { Value = firstDir });
        }
    }
}
