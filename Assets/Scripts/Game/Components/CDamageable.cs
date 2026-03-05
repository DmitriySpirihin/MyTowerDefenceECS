using Unity.Entities;

public struct CDamageable : IComponentData
{
    public ushort durability;
    public byte armor;
}
