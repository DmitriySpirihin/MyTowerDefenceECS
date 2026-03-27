using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct SMove : ISystem
{
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        float _zMax = 100f; 
        float _zMin = -60f;
        
        new MoveJob { 
            deltaTime = SystemAPI.Time.DeltaTime ,
            ecb = ecb,
            zMax = _zMax,
            zMin = _zMin  
            }.ScheduleParallel();
    }

    [BurstCompile]
    public partial struct MoveJob : IJobEntity
    {
        public float deltaTime;
        public EntityCommandBuffer.ParallelWriter ecb;
        public float zMax;
        public float zMin;

        // Query entities with the required components in parallel
        public void Execute(Entity entity, [EntityIndexInQuery] int sortKey, ref CMoveable moveable, ref LocalTransform transform)
        {

            moveable.speed += moveable.acceleration * deltaTime;
            transform.Position += moveable.direction * moveable.speed * deltaTime;

            if (transform.Position.z > zMax || transform.Position.z < zMin)
                ecb.DestroyEntity(sortKey, entity);
        }
    }
}
