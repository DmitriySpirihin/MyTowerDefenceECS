using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[CreateAssetMenu(fileName = "MoveConfigSO", menuName = "Configs/Entities/MoveConfig")]
public class MoveConfigSO : BaseEntityConfigSO
{
    public override string Name() => "Moveable";
    
    [Header("Speed")]
    [Tooltip("Speed of the entity")]
    public float Speed = 1f;

    [Header("Acceleration")]
    [Tooltip("Acceleration , multiplying with speed , by default no acceleration")]
    public float Acceleration = 1f;

    public Vector3 Direction = Vector3.forward;


    public override void Bake(IBaker baker, Entity entity)
    {
        base.Bake(baker, entity);
        // speed
        float finalSpeed = Speed;

        baker.AddComponent(entity, new CMoveable{
            speed = finalSpeed,
            acceleration = Acceleration,
            direction = Direction
        });
        if(NeedsInitialization)
        {
            baker.AddComponent(entity, new CMoveableRandom {
               range = Range
            });
        }
    }
}


public struct CMoveable : IComponentData
{
    public float speed;
    public float acceleration;
    public float3 direction;
}

// Temp component to store values for randomization
public struct CMoveableRandom : IRandomizable
{
    public float2 range;

    public void Randomize(uint seed, ref CMoveable moovable)
    {
        var random = Unity.Mathematics.Random.CreateFromIndex(seed);
        // Setting random speed
        moovable.speed = random.NextFloat(range.x , range.y);
    }
}