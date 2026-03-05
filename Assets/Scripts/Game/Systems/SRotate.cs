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

        // находим сущности со всеми указанными компонентами.
        foreach (var (rotateable, transform, destructible) in SystemAPI.Query<RefRW<CRotateable>, RefRW<LocalTransform>, RefRO<CDestructible>>())
        {
            if (destructible.ValueRO.isDestructed) continue; 
            
            // простое вращение для визуала
            transform.ValueRW.Rotation = math.mul(transform.ValueRO.Rotation, quaternion.Euler( rotateable.ValueRO.speed * rotateable.ValueRO.axis * deltaTime));
        }
    }
}