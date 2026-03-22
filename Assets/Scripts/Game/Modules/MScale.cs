using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[CreateAssetMenu(fileName = "ScaleConfigSO", menuName = "Configs/Entities/ScaleConfig")]
public class ScaleConfigSO : BaseEntityConfigSO
{
    public override string Name() => "Scale";
    [Header("Scale")]
    [Tooltip("Scale of the entity")]
    public float Scale = 1f;
    

    public override void Bake(IBaker baker, Entity entity)
    {
        if(NeedsInitialization)
        {
            base.Bake(baker, entity);
            baker.AddComponent(entity, new CScaleRandom {
               range = Range
            });
        }
    }
}

// Temp component to store values for randomization
public struct CScaleRandom : IComponentData
{
    public float2 range;
    
    public float GetScale(uint seed)
    {
        var random = Unity.Mathematics.Random.CreateFromIndex(seed);
        return  random.NextFloat(range.x, range.y);
    }
}