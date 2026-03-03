using Unity.Entities;
using Unity.Mathematics;

public struct CRotatable : IComponentData
{
    public float speed;
    public float3 axis;
}