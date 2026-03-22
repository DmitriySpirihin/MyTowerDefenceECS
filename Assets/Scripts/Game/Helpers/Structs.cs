using Unity.Mathematics;
using Unity.Entities;
using System;

// for array of GO
public struct AsteroidPrefab : IBufferElementData
{
    public Entity Value;
}

// Flag component to mark entyity as randomizable
public struct CInitializationTag : IComponentData {}
public struct CPlayerTag : IComponentData {}
public struct CEnemyTag : IComponentData {}
public struct CProjectileTag : IComponentData {}
public struct CCollectibleTag : IComponentData {}
public struct CFriendly : IComponentData {}