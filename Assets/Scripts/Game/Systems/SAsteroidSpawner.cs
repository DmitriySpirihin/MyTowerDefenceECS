using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Burst;


[UpdateBefore(typeof(TransformSystemGroup))]
[BurstCompile]
public partial struct SAsteroidSpawner : ISystem
{
    // Точка входа системы. Вызывается каждый кадр (или тик симуляции)
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Пытаемся найти единственную сущность
        if(!SystemAPI.TryGetSingletonEntity<CAsteroidSpawner>(out Entity entity))
        {
            return;
        }
        // Получаем доступ на чтение и запись RefRW
        var spawner = SystemAPI.GetSingletonRW<CAsteroidSpawner>();
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // Проверяем, настало ли время следующего спавна
        // SystemAPI.Time.ElapsedTime - глобальное время симуляции
        if (spawner.ValueRO.nextSpawnTime < SystemAPI.Time.ElapsedTime)
        {
            // Планируем создание новой сущности на основе префабов
            var prefabs = SystemAPI.GetBuffer<AsteroidPrefab>(entity);
            // Выбираем случайный префаб
            int index = spawner.ValueRW.random.NextInt(prefabs.Length);
            Entity prefab = prefabs[index].Value;

            var newEntity = ecb.Instantiate(prefab);
            
            // Расчет и установка позиции и размера на старте
            float3 basePosition = SystemAPI.GetComponent<LocalTransform>(entity).Position;
            float scale = spawner.ValueRW.random.NextFloat(spawner.ValueRO.minScale, spawner.ValueRO.maxScale);
            CalculateSpawnPosition(spawner.ValueRO.spawnArea, ref spawner.ValueRW.random, basePosition, out float3 result);
            ecb.SetComponent(newEntity, new LocalTransform {Position = result, Rotation = quaternion.identity, Scale = scale });
            //Генерируем характеристики через диапазоны из спавнера
            // Прочность в зависимости от размера
            ushort newDurability = (ushort)(spawner.ValueRO.baseDurability * scale);
            float4 randomColor = new float4( spawner.ValueRW.random.NextFloat(0.6f, 1.0f), spawner.ValueRW.random.NextFloat(0.6f, 1.0f),spawner.ValueRW.random.NextFloat(0.6f, 1.0f), 1.0f);
            ecb.AddComponent(newEntity, new AsteroidColorProperty { Value = randomColor });
            ecb.AddComponent(newEntity, new AsteroidTintProperty { Value = spawner.ValueRW.random.NextFloat(0.1f, 0.4f) });
            ecb.AddComponent(newEntity, new AsteroidDisplaceProperty { Value = 0.1f });
            ecb.AddComponent(newEntity, new AsteroidNoiseProperty { Value = spawner.ValueRW.random.NextFloat(0.6f, 1.2f)});
            ecb.AddComponent(newEntity, new CDamageable{
               durability = newDurability,
               armor = 0
            });
            // Движение
            ecb.AddComponent(newEntity, new CMoveable{
               speed = spawner.ValueRW.random.NextFloat(spawner.ValueRO.minMovementSpeed, spawner.ValueRO.maxMovementSpeed),
               acceleration = spawner.ValueRW.random.NextFloat(spawner.ValueRO.minAcceleration, spawner.ValueRO.maxAcceleration),
               direction = new float3(0f, 0f, -1f)
            });
            // Вращение
            ecb.AddComponent(newEntity, new CRotateable{
              speed = spawner.ValueRW.random.NextFloat(spawner.ValueRO.minRotationSpeed, spawner.ValueRO.maxRotationSpeed),
              axis = spawner.ValueRW.random.NextFloat3Direction()
            });
            // Флаг жизни
            ecb.AddComponent(newEntity, new CDestructible{
              isDestructed = false
            });
            // Обновляем время следующего спавна
            spawner.ValueRW.nextSpawnTime = (float)SystemAPI.Time.ElapsedTime + spawner.ValueRO.spawnRate;

        }
    }

    private static void CalculateSpawnPosition(in SpawnArea spawnArea, ref Random rand,in float3 startPosition, out float3 result)
    { 
        float3 spawnPosition = startPosition;
        if(math.any(spawnArea.xRange != float2.zero))
            spawnPosition.x += rand.NextFloat(spawnArea.xRange.x, spawnArea.xRange.y);
        if(math.any(spawnArea.yRange != float2.zero))
            spawnPosition.y += rand.NextFloat(spawnArea.yRange.x, spawnArea.yRange.y);
        if(math.any(spawnArea.zRange != float2.zero))
            spawnPosition.z += rand.NextFloat(spawnArea.zRange.x, spawnArea.zRange.y);
        
        result = spawnPosition;
    }
}

[MaterialProperty("_BaseColor")]
public struct AsteroidColorProperty : IComponentData { public float4 Value; }

[MaterialProperty("_TintStrength")]
public struct AsteroidTintProperty : IComponentData { public float Value; }

[MaterialProperty("_DisplaceStrength")]
public struct AsteroidDisplaceProperty : IComponentData { public float Value; }

[MaterialProperty("_NoiseScale")]
public struct AsteroidNoiseProperty : IComponentData { public float Value; }