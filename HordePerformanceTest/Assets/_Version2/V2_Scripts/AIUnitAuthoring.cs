using UnityEngine;
using Unity.Entities;

public class AIUnitAuthoring : MonoBehaviour
{
    public float initialSpeed = 2f;
    public float turnInterval = 1.5f;

    class Baker : Baker<AIUnitAuthoring>
    {
        public override void Bake(AIUnitAuthoring a)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<Velocity2D>(e);
            AddComponent(e, new Wander2D
            {
                Speed = a.initialSpeed,
                TurnInterval = a.turnInterval,
                TimeSinceTurn = 0f
            });
        }
    }
}
