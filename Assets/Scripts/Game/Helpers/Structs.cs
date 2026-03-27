using Unity.Mathematics;
using Unity.Entities;
using System;
using Unity.VisualScripting;

// for array of GO, not use now , made randomization via shader
/*
public struct AsteroidPrefab : IBufferElementData
{
    public Entity Value;
}
*/

// Flag component to mark entyity as randomizable
public struct CInitializationTag : IComponentData, IEnableableComponent {}
public struct CPlayerTag : IComponentData, ISingleton {}
public struct CEnemyTag : IComponentData {}
public struct CProjectileTag : IComponentData {}
public struct CCollectibleTag : IComponentData {}
public struct CFriendly : IComponentData {}

// Structs to store global data
public struct CGlobalPlayerState : IComponentData, ISingleton
{
    public bool IsFiring;
}

public struct CGlobalStatistics : IComponentData, ISingleton
{
    public int EnemyDefeated;
    public int EnemyInScene;
}