using Unity.Entities;
using Unity.Mathematics;

public struct CAsteroidSpawner : IComponentData
{
    // префаб и основная точка спавна
    public SpawnArea spawnArea;
    // параметры спавна
    public float spawnXAbsOffset;// 0 середина
    public float nextSpawnTime;
    public float spawnRate;

    // для хранения генератора
    public Random random;
    // пределы рандомных значений для спавна
    public float minMovementSpeed;
    public float maxMovementSpeed;
    public float minRotationSpeed;
    public float maxRotationSpeed;
    public float minAcceleration;
    public float maxAcceleration;
    public float minScale;
    public float maxScale;
    public ushort baseDurability;
}

public struct AsteroidPrefab : IBufferElementData
{
    public Entity Value;
}