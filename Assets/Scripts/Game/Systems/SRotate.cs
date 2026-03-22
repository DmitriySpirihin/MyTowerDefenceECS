using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct SRotate : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        
        // Initializing job and pass required data
        var rotateJob = new RotateJob 
        { 
            DeltaTime = deltaTime 
        };
        
        // Scheduling the job to run parallel
        // Assigning to state.Dependency to maintain proper job ordering
        state.Dependency = rotateJob.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    public partial struct RotateJob : IJobEntity
    {
        public float DeltaTime;
        
        // Execute method runs in parallel for each entity matching the component query
        public void Execute(ref LocalTransform transform, in CRotateable rotateable)
        {
            float t = math.min(rotateable.speed * DeltaTime, 1.0f);
            quaternion frameRotation = math.slerp(quaternion.identity, rotateable.randomAngle, DeltaTime);

            transform.Rotation = math.mul(frameRotation, transform.Rotation);
        }
    }
}