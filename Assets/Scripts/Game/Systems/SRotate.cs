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
        // Schedule the job to run across all CPU cores
        new RotateJob { DeltaTime = SystemAPI.Time.DeltaTime }.Run();
    }

    [BurstCompile]
    public partial struct RotateJob : IJobEntity
    {
        public float DeltaTime;

        // Execute runs in parallel for every entity matching these components
        public void Execute(ref LocalTransform transform, in CRotateable rotateable, in CDestructible destructible)
        {
            // Skip logic for destroyed asteroids
            if (destructible.isDestructed) return;

            // Calculate rotation increment: Axis * Speed * Time
            quaternion deltaRotation = quaternion.Euler(rotateable.axis * rotateable.speed * DeltaTime);
            
            // Multiply current rotation by delta (order matters: New * Old)
            transform.Rotation = math.mul(transform.Rotation, deltaRotation);
        }
    }
}