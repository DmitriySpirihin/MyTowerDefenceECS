using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;

//указывает компилятору трансформировать этот код высокооптимизированный код.
[UpdateBefore(typeof(TransformSystemGroup))]
[BurstCompile]
public partial struct SSpawner : ISystem
{
    // Точка входа системы. Вызывается каждый кадр (или тик симуляции)
    // RefRW и SystemAPI — это часть нового API Entities 1.0+, более безопасного и явного
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Пытаемся найти единственную сущность
        // Если сущность найдена запишем Entity в переменную
        if(!SystemAPI.TryGetSingletonEntity<CAsteroidSpawner>(out Entity entity))
        {
            return;
        }
        // Получаем доступ на чтение и запись RefRW
        RefRW<CAsteroidSpawner> spawner = SystemAPI.GetComponentRW<CAsteroidSpawner>(entity);

        // Allocator.Temp - память выделена временно и должна быть освобождена вручную
        using EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        // Проверяем, настало ли время следующего спавна
        // SystemAPI.Time.ElapsedTime — глобальное время симуляции
        if (spawner.ValueRO.nextSpawnTime < SystemAPI.Time.ElapsedTime)
        {
            // Планируем создание новой сущности на основе префаба
            Entity newEntity = ecb.Instantiate(spawner.ValueRO.prefab);
            
            float3 basePosition = SystemAPI.GetComponent<LocalTransform>(entity).Position;
            CalculateSpawnPosition(spawner.ValueRO.spawnArea, ref spawner.ValueRW.random, basePosition, out float3 result);
            
            ecb.SetComponent(newEntity, new LocalTransform {Position = result, Rotation = quaternion.identity, Scale = 1f });
            //Генерируем характеристики через диапазоны из спавнера
            ecb.AddComponent(newEntity, new CMoveable{
               speed = spawner.ValueRW.random.NextFloat(spawner.ValueRO.minMovementSpeed, spawner.ValueRO.maxMovementSpeed),
               acceleration = spawner.ValueRW.random.NextFloat(spawner.ValueRO.minAcceleration, spawner.ValueRO.maxAcceleration),
               direction = new float3(0.0f, 0.0f, -1f)
            });
            ecb.AddComponent(newEntity, new CRotatable{
              speed = spawner.ValueRW.random.NextFloat(spawner.ValueRO.minRotationSpeed, spawner.ValueRO.maxRotationSpeed), axis = spawner.ValueRW.random.NextFloat3Direction()
            });
            ecb.AddComponent(newEntity, new CDestructible{
              isDestructed = false
            });
            // Обновляем время следующего спавна
            spawner.ValueRW.nextSpawnTime = (float)SystemAPI.Time.ElapsedTime + spawner.ValueRO.spawnRate;

            // Применяем накопленные команды буфера к EntityManager
            // Это синхронизирует изменения с основным миром
            ecb.Playback(state.EntityManager);
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