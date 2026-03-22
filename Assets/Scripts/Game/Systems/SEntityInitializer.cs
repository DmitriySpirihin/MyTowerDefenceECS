using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;


[BurstCompile]
public partial struct SUniversalInitSystem : ISystem
{
    private ComponentLookup<CRotateableRandom> _randomLookup;
    private ComponentLookup<CRotateable> _rotateableLookup;
    private ComponentLookup<CScaleRandom> _scaleRandomLookUp;
    private ComponentLookup<Unity.Transforms.LocalTransform> _transformLookup;
    private ComponentLookup<CMoveable> _moveableLookup;
    private ComponentLookup<CMoveableRandom> _moveableRandomLookup;
    private ComponentLookup<CDisplaceProperty> _displaceLookup;
    private ComponentLookup<CDisplaceRandom> _displaceRandomLookup;
    private ComponentLookup<CNoiseProperty> _noiseLookup;
    private ComponentLookup<CNoiseRandom> _noiseRandomLookup;
    private ComponentLookup<CTintProperty> _tintLookup;
    private ComponentLookup<CTintRandom> _tintRandomLookup;

    public void OnCreate(ref SystemState state)
    {
        _randomLookup = state.GetComponentLookup<CRotateableRandom>(true);
        _rotateableLookup = state.GetComponentLookup<CRotateable>(true);
        _scaleRandomLookUp = state.GetComponentLookup<CScaleRandom>(true);
        _transformLookup = state.GetComponentLookup<Unity.Transforms.LocalTransform>(true);
        _moveableRandomLookup = state.GetComponentLookup<CMoveableRandom>(true);
        _moveableLookup = state.GetComponentLookup<CMoveable>(true);
        _displaceLookup = state.GetComponentLookup<CDisplaceProperty>(true);
        _displaceRandomLookup = state.GetComponentLookup<CDisplaceRandom>(true);
        _noiseLookup = state.GetComponentLookup<CNoiseProperty>(true);
        _noiseRandomLookup = state.GetComponentLookup<CNoiseRandom>(true);
        _tintLookup = state.GetComponentLookup<CTintProperty>(true);
        _tintRandomLookup = state.GetComponentLookup<CTintRandom>(true);
        
        state.RequireForUpdate<CInitializationTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        
        _randomLookup.Update(ref state);
        _rotateableLookup.Update(ref state);
        _scaleRandomLookUp.Update(ref state);
        _transformLookup.Update(ref state);
        _moveableRandomLookup.Update(ref state);
        _moveableLookup.Update(ref state);
        _displaceLookup.Update(ref state);
        _displaceRandomLookup.Update(ref state);
        _noiseLookup.Update(ref state);
        _noiseRandomLookup.Update(ref state);
        _tintLookup.Update(ref state);
        _tintRandomLookup.Update(ref state);

        var initJob = new InitJob 
        { 
            SeedBase = (uint)(SystemAPI.Time.ElapsedTime * 1000),
            ECB = ecb.AsParallelWriter(),
            rotatableRandomLookUp = _randomLookup,
            rotatableLookUp = _rotateableLookup,
            scaleRandomLookUp = _scaleRandomLookUp,
            transformLookUp = _transformLookup,
            moveableLookUp = _moveableLookup,
            moveableRandomLookUp = _moveableRandomLookup,

            // Pass to job
            displaceLookup = _displaceLookup,
            displaceRandomLookup = _displaceRandomLookup,
            noiseLookup = _noiseLookup,
            noiseRandomLookup = _noiseRandomLookup,
            tintLookup = _tintLookup,
            tintRandomLookup = _tintRandomLookup
        };

        state.Dependency = initJob.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct InitJob : IJobEntity
{
    public uint SeedBase;
    public EntityCommandBuffer.ParallelWriter ECB;

    [ReadOnly] public ComponentLookup<CRotateableRandom> rotatableRandomLookUp;
    [ReadOnly] public ComponentLookup<CRotateable> rotatableLookUp;
    [ReadOnly] public ComponentLookup<CScaleRandom> scaleRandomLookUp;
    [ReadOnly] public ComponentLookup<Unity.Transforms.LocalTransform> transformLookUp;
    [ReadOnly] public ComponentLookup<CMoveable> moveableLookUp;
    [ReadOnly] public ComponentLookup<CMoveableRandom> moveableRandomLookUp;
    [ReadOnly] public ComponentLookup<CDisplaceProperty> displaceLookup;
    [ReadOnly] public ComponentLookup<CDisplaceRandom> displaceRandomLookup;
    [ReadOnly] public ComponentLookup<CNoiseProperty> noiseLookup;
    [ReadOnly] public ComponentLookup<CNoiseRandom> noiseRandomLookup;
    [ReadOnly] public ComponentLookup<CTintProperty> tintLookup;
    [ReadOnly] public ComponentLookup<CTintRandom> tintRandomLookup;

    public void Execute(Entity entity, [EntityIndexInQuery] int sortKey, in CInitializationTag initTag)
    {
        uint seed = SeedBase + (uint)entity.Index;

        // Rotation
        if (rotatableRandomLookUp.HasComponent(entity))
        {
            var rotateableRandom = rotatableRandomLookUp[entity];
            var rotateable = rotatableLookUp[entity];
            
            // Perform the logic
            rotateableRandom.Randomize(seed, ref rotateable);

            // Use ECB to update the component since lookups are ReadOnly for Parallel
            ECB.SetComponent(sortKey, entity, rotateable);
            ECB.RemoveComponent<CRotateableRandom>(sortKey, entity);
        }
        // Scale
        if (scaleRandomLookUp.HasComponent(entity))
        {
            var scaleRandom = scaleRandomLookUp[entity];
            
            float newScale = scaleRandom.GetScale(seed);

            if (transformLookUp.HasComponent(entity))
            {
                var transform = transformLookUp[entity];
                transform.Scale = newScale; 
                ECB.SetComponent(sortKey, entity, transform);
            }
            ECB.RemoveComponent<CScaleRandom>(sortKey, entity);
        }
        // move
        if (moveableRandomLookUp.HasComponent(entity))
        {
            var movableRandom = moveableRandomLookUp[entity];
            var moveable = moveableLookUp[entity];
            // Perform the logic
            movableRandom.Randomize(seed, ref moveable);
            // Use ECB to update the component since lookups are ReadOnly for Parallel
            ECB.SetComponent(sortKey, entity, moveable);
            ECB.RemoveComponent<CMoveableRandom>(sortKey, entity);
        }

        // Displace
        if (displaceRandomLookup.HasComponent(entity))
        {
            var randomData = displaceRandomLookup[entity];
            var property = displaceLookup[entity];
            randomData.Randomize(seed, ref property);
            ECB.SetComponent(sortKey, entity, property);
            ECB.RemoveComponent<CDisplaceRandom>(sortKey, entity);
        }

        // Noise
        if (noiseRandomLookup.HasComponent(entity))
        {
            var randomData = noiseRandomLookup[entity];
            var property = noiseLookup[entity];
            randomData.Randomize(seed, ref property);
            ECB.SetComponent(sortKey, entity, property);
            ECB.RemoveComponent<CNoiseRandom>(sortKey, entity);
        }

        // Tint
        if (tintRandomLookup.HasComponent(entity))
        {
            var randomData = tintRandomLookup[entity];
            var property = tintLookup[entity];
            randomData.Randomize(seed, ref property);
            ECB.SetComponent(sortKey, entity, property);
            ECB.RemoveComponent<CTintRandom>(sortKey, entity);
        }

        ECB.RemoveComponent<CInitializationTag>(sortKey, entity);
    }
}





