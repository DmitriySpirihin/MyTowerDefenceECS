using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class AsteroidSpawnerAuthoring : MonoBehaviour
{
    public GameObject prefab;
    public SpawnRangeConfigSO spawnConfig;
    public float spawnRate;
    [Range(1f, 20f)]public float minMovementSpeed;
    [Range(2f, 20f)]public float maxMovementSpeed;
    [Range(1f, 20f)]public float minRotationSpeed;
    [Range(2f, 20f)]public float maxRotationSpeed;
    [Range(0.01f, 2f)]public float minAcceleration;
    [Range(0.02f, 2f)]public float maxAcceleration;
}

public class AsteroidSpawnerBaker : Baker<AsteroidSpawnerAuthoring>
{
    public override void Bake(AsteroidSpawnerAuthoring authoring)
    {
        if (authoring.spawnConfig == null)
        {
            Debug.LogWarning($"Add config on spawner");
            return; 
        }

        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        // Инициализируем рандом уникальным сидом. 
        uint seed = (uint)System.Guid.NewGuid().GetHashCode(); 
        
        AddComponent(entity, new CAsteroidSpawner
        {
            prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
            spawnArea = authoring.spawnConfig.spawnArea,
            nextSpawnTime = 0.0f,
            spawnRate = authoring.spawnRate,
            random = new Unity.Mathematics.Random(seed),
            
            minMovementSpeed = authoring.minMovementSpeed,
            maxMovementSpeed = authoring.maxMovementSpeed,
            minRotationSpeed = authoring.minRotationSpeed,
            maxRotationSpeed = authoring.maxRotationSpeed,
            minAcceleration = authoring.minAcceleration,
            maxAcceleration = authoring.maxAcceleration
        });
    }
}