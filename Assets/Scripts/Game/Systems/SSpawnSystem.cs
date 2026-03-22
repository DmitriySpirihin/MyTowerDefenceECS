using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using GameEnums;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
public partial struct SSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnerData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        float elapsedTime = (float)state.WorldUnmanaged.Time.ElapsedTime;

        foreach (var (spawnerRW, transform) in SystemAPI.Query<RefRW<SpawnerData>, RefRO<LocalTransform>>())
        {
            ref var spawner = ref spawnerRW.ValueRW;

            if (!spawner.spawnOnStart || spawner.prefabEntity == Entity.Null) continue;
            if (spawner.spawnType == SpawnType.OneTime && spawner.hasSpawned) continue;

            // --- Time handling logic based on SpawnType ---
            bool shouldSpawn = false;

            switch (spawner.spawnType)
            {
                case SpawnType.OneTime:
                // Spawn immediately on first update, then mark as done
                    if (!spawner.hasSpawned)
                    {
                        shouldSpawn = true;
                        spawner.hasSpawned = true;
                   }
                break;

                case SpawnType.Endless:
                // Spawn every 'tick' seconds, starting after initial delay
                    if (elapsedTime >= spawner.nextSpawnTime)
                    {
                        shouldSpawn = true;
                        spawner.nextSpawnTime = elapsedTime + spawner.tick;
                    }
                break;

            case SpawnType.Wave:
                // Spawn a burst every 'waveTick' seconds
                    if (elapsedTime >= spawner.nextWaveTime)
                    {
                        shouldSpawn = true;
                        spawner.nextWaveTime = elapsedTime + spawner.waveTick;
                        spawner.isSpawnTickNow = true; // Flag for burst logic
                    }
                    else
                    {
                        spawner.isSpawnTickNow = false;
                    }
                break;
            }
            
            if (shouldSpawn)
            {
               uint frameSeed = (uint)(spawner.seed + (elapsedTime * 1000));
               var randomGen = new Random(math.max(1u, frameSeed));
            
               ExecuteSpawn(transform.ValueRO, ref spawner, ecb, ref randomGen);
            }
        }
    }

    private void ExecuteSpawn(LocalTransform transform, ref SpawnerData spawner, EntityCommandBuffer ecb, ref Random random)
    {
        // Determine count based on mode + wave flag
        int count = 1;
        if (spawner.spawnMode == SpawnMode.Burst || (spawner.spawnType == SpawnType.Wave && spawner.isSpawnTickNow))
           count = spawner.burstCount;
       

       for (int i = 0; i < count; i++)
       {
           Entity spawned = ecb.Instantiate(spawner.prefabEntity);
           float3 pos = GetSpawnPosition(ref transform, ref spawner, ref random, i);
           ecb.SetComponent(spawned, LocalTransform.FromPosition(pos));
       }
    }

    private float3 GetSpawnPosition(ref LocalTransform spawnerTransform, ref SpawnerData spawner, ref Random random, int index)
    {
        float3 localOffset = spawner.spawnMode switch
        {
            SpawnMode.Random => spawner.spawnArea.GetRandomPoint(ref random),
            SpawnMode.Burst => spawner.spawnArea.GetRandomPoint(ref random),
            SpawnMode.Grid => GetGridPosition(spawner.spawnArea, 1f, index),
            SpawnMode.Curve => GetCurvePosition(spawner.spawnArea, ref random),
            _ => spawner.spawnArea.GetRandomPoint(ref random)
        };

        return math.transform(spawnerTransform.ToMatrix(), localOffset);
    }

    // Grid
    private float3 GetGridPosition(SpawnArea area, float step, int index)
    {
        int xCount = math.max(1, (int)((area.xRange.y - area.xRange.x) / step));
        int yCount = math.max(1, (int)((area.yRange.y - area.yRange.x) / step));
        
        int x = index % xCount;
        int y = index / xCount % yCount;
        int z = index / (xCount * yCount);
        
        return new float3(
            math.lerp(area.xRange.x, area.xRange.y, x / (float)xCount),
            math.lerp(area.yRange.x, area.yRange.y, y / (float)yCount),
            math.lerp(area.zRange.x, area.zRange.y, math.min(1f, z / 4f))
        );
    }

    // Curve
    private float3 GetCurvePosition(SpawnArea area, ref Random random)
    {
        // Curve emulation
        float t = random.NextFloat(0f, 1f);
        float curveFactor = t * t; // Quadratic ease-in
        
        float3 center = new float3(
            math.lerp(area.xRange.x, area.xRange.y, 0.5f),
            math.lerp(area.yRange.x, area.yRange.y, 0.5f),
            math.lerp(area.zRange.x, area.zRange.y, 0.5f)
        );
        
        float3 edge = area.GetRandomPoint(ref random);
        return math.lerp(center, edge, curveFactor);
    }

    private int CalculateGridCount(SpawnArea area, float step)
    {
        int xCount = (int)((area.xRange.y - area.xRange.x) / step) + 1;
        int yCount = (int)((area.yRange.y - area.yRange.x) / step) + 1;
        int zCount = (int)((area.zRange.y - area.zRange.x) / step) + 1;
        return math.min(xCount * yCount * zCount, 100);
    }
}