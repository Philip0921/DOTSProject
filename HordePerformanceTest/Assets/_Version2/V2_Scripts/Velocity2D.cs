using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Velocity2D : IComponentData
{
    public float2 values;
}

public struct Wander2D : IComponentData
{
    public float Speed;
    public float TurnInterval;
    public float TimeSinceTurn;
}
