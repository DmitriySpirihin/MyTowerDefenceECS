using Unity.Entities;
using UnityEngine;

public class UniversalEntitySpawnerAuthoring : MonoBehaviour
{
    [SerializeField] private SpawnConfigSO spawnConfig;
    [SerializeField] private bool spawnOnStart = true;
    
    public class Baker : Baker<UniversalEntitySpawnerAuthoring>
    {
        public override void Bake(UniversalEntitySpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            var area = authoring.spawnConfig != null ? authoring.spawnConfig.spawnArea : default;
            
            Entity prefabEntity = GetEntity(authoring.spawnConfig.prefab, TransformUsageFlags.Dynamic);
            
            
            // Add spawnerData component and copy data from config
            AddComponent(entity, new SpawnerData{
                spawnMode = authoring.spawnConfig.mode,
                spawnType = authoring.spawnConfig.type,
                spawnArea = area,
                tick = authoring.spawnConfig.tick,
                waveTick = authoring.spawnConfig.waveTick,
                isSpawnTickNow = authoring.spawnConfig.isSpawnTickNow,
                prefabEntity = prefabEntity,
                burstCount = authoring.spawnConfig.burstCount,
                spawnOnStart = authoring.spawnOnStart,
                seed = (uint)System.Guid.NewGuid().GetHashCode(),
                hasSpawned = false,
                nextSpawnTime = authoring.spawnConfig.tick,  
                nextWaveTime = authoring.spawnConfig.waveTick  

            });
        }
    }
}