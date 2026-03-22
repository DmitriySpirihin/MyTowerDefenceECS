using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "RotateConfigSO", menuName = "Configs/Entities/RotateConfig")]
public class RotateConfigSO : BaseEntityConfigSO
{
    public override string Name() => "Rotateable";
    [Header("Speed")]
    [Tooltip("Speed of the entity")]
    public float Speed = 1f;

    public override void Bake(IBaker baker, Entity entity)
    {
        base.Bake(baker, entity);

        baker.AddComponent(entity, new CRotateable {
           speed = Speed,
           randomAngle = quaternion.identity
        });
        if(NeedsInitialization)
        {
            baker.AddComponent(entity, new CRotateableRandom {
               range = Range
            });
        }
    }
}

// Initial component
public struct CRotateable : IComponentData
{
    public float speed;
    public quaternion randomAngle;
}

// Temp component to store values for randomization
public struct CRotateableRandom : IRandomizable
{
    public float2 range;

    public void Randomize(uint seed, ref CRotateable rotateable)
    {
        var random = Unity.Mathematics.Random.CreateFromIndex(seed);
        // Setting random speed
        rotateable.speed = random.NextFloat(range.x , range.y);
        // Setting random quaternion
        rotateable.randomAngle = random.NextQuaternionRotation();
    }
}