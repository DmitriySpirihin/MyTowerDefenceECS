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
        // Schedule the job to run on all available CPU cores
        new MoveJob { deltaTime = SystemAPI.Time.DeltaTime }.Run();
    }

    [BurstCompile]
    public partial struct MoveJob : IJobEntity
    {
        public float deltaTime;

        // Query entities with the required components in parallel
        public void Execute(ref CMoveable moveable, ref LocalTransform transform)
        {

            moveable.speed += moveable.acceleration * deltaTime;
            transform.Position += moveable.direction * moveable.speed * deltaTime;
        }
    }
}
