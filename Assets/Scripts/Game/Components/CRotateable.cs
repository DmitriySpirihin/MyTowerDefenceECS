using Unity.Entities;
using Unity.Mathematics;

public struct CRotateable : IComponentData
{
    public float speed;
    public float3 axis;
}