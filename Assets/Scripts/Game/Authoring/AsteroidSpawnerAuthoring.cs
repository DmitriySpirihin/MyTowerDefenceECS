using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class AsteroidSpawnerAuthoring : MonoBehaviour
{
    public GameObject[] prefab = new GameObject[2];
    public SpawnRangeConfigSO spawnConfig;
    public float spawnRate;
    [Range(1f, 20f)]public float minMovementSpeed;
    [Range(2f, 20f)]public float maxMovementSpeed;
    [Range(0.1f, 20f)]public float minRotationSpeed;
    [Range(2f, 20f)]public float maxRotationSpeed;
    [Range(0.01f, 2f)]public float minAcceleration;
    [Range(0.02f, 2f)]public float maxAcceleration;
    [Range(0.1f, 1.5f)]public float minScale;
    [Range(1.5f, 3f)]public float maxScale;
    [Range(1, 10)]public ushort baseDurability;
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
        var prefabBuffer = AddBuffer<AsteroidPrefab>(entity);
        foreach (var en in authoring.prefab)
        {
         if (en != null) prefabBuffer.Add(new AsteroidPrefab { Value = GetEntity(en, TransformUsageFlags.Dynamic) });
        }

        // Инициализируем рандом уникальным сидом. 
        uint seed = (uint)System.Guid.NewGuid().GetHashCode(); 
        
        AddComponent(entity, new CAsteroidSpawner
        {
            spawnArea = authoring.spawnConfig.spawnArea,
            nextSpawnTime = 0.0f,
            spawnRate = authoring.spawnRate,
            random = new Unity.Mathematics.Random(seed),
            
            minMovementSpeed = authoring.minMovementSpeed,
            maxMovementSpeed = authoring.maxMovementSpeed,
            minRotationSpeed = authoring.minRotationSpeed,
            maxRotationSpeed = authoring.maxRotationSpeed,
            minAcceleration = authoring.minAcceleration,
            maxAcceleration = authoring.maxAcceleration,
            minScale = authoring.minScale,
            maxScale = authoring.maxScale,
            baseDurability = authoring.baseDurability
        });
    }
}