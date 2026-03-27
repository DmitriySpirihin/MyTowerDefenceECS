using Unity.Entities;
using Unity.Mathematics;
using GameEnums;

public struct SpawnerData : IComponentData
{
    public Entity prefabEntity;
    public bool spawnOnStart;
    public SpawnType spawnType;      // OneTime, Endless, Wave, PlayerFire
    public SpawnMode spawnMode;      // Random, Burst, Grid, Curve
    public SpawnArea spawnArea;      // Defines spawn bounds
    public uint seed;
    
    // Timing parameters
    public float tick;               // Spawn interval for Endless
    public float waveTick;           // Wave interval for Wave mode
    public int burstCount;           // Entities per burst
    
    // Runtime state (updated by system)
    public bool hasSpawned;          // For OneTime mode
    public float nextSpawnTime;      // Next allowed spawn time (Endless)
    public float nextWaveTime;       // Next wave trigger time (Wave)
    public bool isSpawnTickNow;      // Flag: active wave frame
}

[System.Serializable]
public struct SpawnArea
{
    public float2 xRange; 
    public float2 yRange;
    public float2 zRange;

    public float3 GetRandomPoint(ref Random random) => new float3(
        random.NextFloat(xRange.x, xRange.y),
        random.NextFloat(yRange.x, yRange.y),
        random.NextFloat(zRange.x, zRange.y)
    );
}

public struct SpawnRequest : IComponentData
{
    public Entity prefab;
    public float3 position;
    public quaternion rotation;
    public float3 scale;
    public uint seed; 
}

public struct SpawnedEntityTag : IComponentData
{
}