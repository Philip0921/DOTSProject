using Unity.Entities;
using Unity.Mathematics;

public struct ComponentData : IComponentData
{
    // Script name
}
public struct Speed : IComponentData
{
    public float Value; 
}
public struct DecisionData : IComponentData
{
    public float Remaining; // decision time
    public float2 Range; // x = min, y = max
}
public struct MoveDir : IComponentData
{
    public float2 Value; // Riktingsvektor (0,0)
}
public struct MapBounds : IComponentData
{
    public float2 Size; // (Width, Height)
}
public struct RNG : IComponentData
{
    public Unity.Mathematics.Random Value; // per-entity RNG
}
