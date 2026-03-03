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
        // === 1. ВРЕМЯ И ФИЗИКА ===
        // DeltaTime — критически важен. Без него на 144 ГЗ мониторе астероиды 
        // будут лететь в 2 раза быстрее, чем на 60 ГЗ. Это как бежать марафон 
        // без учета пульса: вроде стараешься, но темп сбивается.
        float deltaTime = SystemAPI.Time.DeltaTime;

        // === 2. QUERY (ВЫБОРКА ДАННЫХ) ===
        // SystemAPI.Query автоматически находит сущности со всеми указанными компонентами.
        // RefRW<T> — для записи (скорость, позиция).
        // RefRO<T> — для чтения (флаг уничтожения).
        // Это безопаснее и быстрее, чем ComponentLookup, так как данные подаются 
        // напрямую в цикл, обеспечивая когерентность памяти (cache coherence).
        foreach (var (moveable, transform, destructible) in 
                 SystemAPI.Query<RefRW<CMoveable>, RefRW<LocalTransform>, RefRO<CDestructible>>())
        {
            // === 3. ЛОГИКА УНИЧТОЖЕНИЯ ===
            // Проверяем флаг. Если уничтожен — пропускаем.
            // Академическая заметка: В ECS чаще используют тег [Disabled] вместо bool-флага.
            // Тег полностью исключает сущность из симуляции, что быстрее для CPU.
            // Но если нужна логика "мертв, но еще виден" — флаг ок.
            if (destructible.ValueRO.isDestructed) 
            {
                continue; 
            }

            // === 4. ИНТЕГРАЦИЯ ДВИЖЕНИЯ ===
            // Обновляем скорость: V_new = V_old + a * t
            // Это классическая кинематика. Ускорение накапливается со временем.
            moveable.ValueRW.speed += moveable.ValueRO.acceleration * deltaTime;

            // Обновляем позицию: P_new = P_old + V * direction * t
            // Умножаем на direction, чтобы лететь куда надо, а не просто по оси X.
            transform.ValueRW.Position += moveable.ValueRO.direction * moveable.ValueRO.speed * deltaTime;
            
            // === 5. МИКРО-ОПТИМИЗАЦИЯ (Опционально) ===
            // Если direction может изменяться, стоит нормализовать его здесь, 
            // чтобы скорость не зависела от длины вектора направления.
            // Но если направление задается один раз при спавне — можно оставить как есть.
            // float3 normalizedDir = math.normalize(moveable.ValueRO.direction);
        }
    }
}