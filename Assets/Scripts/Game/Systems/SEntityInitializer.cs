using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct SUniversalInitSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // Schedule the job
        var initJob = new InitJob
        {
            SeedBase = (uint)(SystemAPI.Time.ElapsedTime * 1000) + (uint)state.GlobalSystemVersion,
            ECB = ecb.AsParallelWriter()
        };

        state.Dependency = initJob.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct InitJob : IJobEntity
{
    public uint SeedBase;
    public EntityCommandBuffer.ParallelWriter ECB;

    [BurstCompile]
    public void Execute(
        Entity entity, 
        [EntityIndexInQuery] int sortKey, 
        EnabledRefRW<CInitializationTag> initTag,
        RefRW<CMoveable> moveable, 
        RefRO<CMoveableRandom> moveableRandom,
        RefRW<CRotateable> rotateable, 
        RefRO<CRotateableRandom> rotateableRandom,
        RefRW<LocalTransform> transform, 
        RefRO<CScaleRandom> scaleRandom,
        RefRW<CDisplaceProperty> displace, 
        RefRO<CDisplaceRandom> displaceRandom,
        RefRW<CNoiseProperty> noise, 
        RefRO<CNoiseRandom> noiseRandom,
        RefRW<CTintProperty> tint, 
        RefRO<CTintRandom> tintRandom
    )
    {
        uint seed = math.hash(new uint2(SeedBase, (uint)entity.Index));

        //Move Randomization
        if (moveableRandom.IsValid)
        {
            moveableRandom.ValueRO.Randomize(seed, ref moveable.ValueRW);
            ECB.RemoveComponent<CMoveableRandom>(sortKey, entity);
        }

        //Rotation Randomization
        if (rotateable.IsValid && rotateableRandom.IsValid)
        {
            rotateableRandom.ValueRO.Randomize(seed, ref rotateable.ValueRW);
            ECB.RemoveComponent<CRotateableRandom>(sortKey, entity);
        }

        //Scale Randomization
        if (transform.IsValid && scaleRandom.IsValid)
        {
            transform.ValueRW.Scale = scaleRandom.ValueRO.GetScale(seed);
            ECB.RemoveComponent<CScaleRandom>(sortKey, entity);
        }

        // Displace Randomization
        if (displace.IsValid && displaceRandom.IsValid)
        {
            displaceRandom.ValueRO.Randomize(seed, ref displace.ValueRW);
            ECB.RemoveComponent<CDisplaceRandom>(sortKey, entity);
        }

        // Noise Randomization
        if (noise.IsValid && noiseRandom.IsValid)
        {
            noiseRandom.ValueRO.Randomize(seed, ref noise.ValueRW);
            ECB.RemoveComponent<CNoiseRandom>(sortKey, entity);
        }

        //Tint Randomization
        if (tint.IsValid && tintRandom.IsValid)
        {
            tintRandom.ValueRO.Randomize(seed, ref tint.ValueRW);
            ECB.RemoveComponent<CTintRandom>(sortKey, entity);
        }

        // Turn off the initialization tag
        initTag.ValueRW = false;
        
        //Cleanup of the tag to keep chunks clean
        ECB.RemoveComponent<CInitializationTag>(sortKey, entity);
    }
}







