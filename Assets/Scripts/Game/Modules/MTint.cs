using Unity.Rendering;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "TintConfigSO", menuName = "Configs/Entities/TintConfig")]
public class TintConfigSO : BaseEntityConfigSO
{
    public override string Name() => "Tint";
    [Header("Set tint range beetwen 0.1-0.3")]
    public float tint = 1f;
    

    public override void Bake(IBaker baker, Entity entity)
    {
        baker.AddComponent(entity, new CTintProperty{
            Value = tint
        });
        if(NeedsInitialization)
        {
            baker.AddComponent(entity, new CTintRandom {
               range = Range
            });
        }
    }
}

[MaterialProperty("_TintStrength")]
public struct CTintProperty : IComponentData 
{ 
    public float Value; 
}
public struct CTintRandom : IRandomizable
{
    public float2 range;

    public void Randomize(uint seed, ref CTintProperty tintProperty)
    {
        var random = Unity.Mathematics.Random.CreateFromIndex(seed);
        // Setting random speed
        tintProperty.Value = random.NextFloat(range.x , range.y);
    }
}