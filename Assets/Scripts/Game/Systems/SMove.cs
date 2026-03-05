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
        float deltaTime = SystemAPI.Time.DeltaTime;

        // находим сущности со всеми указанными компонентами.
        foreach (var (moveable, transform, destructible) in SystemAPI.Query<RefRW<CMoveable>, RefRW<LocalTransform>, RefRO<CDestructible>>())
        {
            if (destructible.ValueRO.isDestructed) continue; 
            //накапливаем ускорение
            moveable.ValueRW.speed += moveable.ValueRO.acceleration * deltaTime;
            transform.ValueRW.Position += moveable.ValueRO.direction * moveable.ValueRO.speed * deltaTime;
        }
    }
}