using Unity.Entities;
using Unity.Mathematics;

public struct CMoveable : IComponentData
{
    public float speed;
    public float acceleration;
    public float3 direction;
}
