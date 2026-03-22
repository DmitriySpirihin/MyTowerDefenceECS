using Unity.Rendering;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

[CreateAssetMenu(fileName = "NoiseConfigSO", menuName = "Configs/Entities/NoiseConfig")]
public class NoiseConfigSO : BaseEntityConfigSO
{
    public override string Name() => "Noise";
    public override void Bake(IBaker baker, Entity entity)
    {
        baker.AddComponent(entity, new CNoiseProperty{
            Value = Range.x
        });
        if(NeedsInitialization)
        {
            baker.AddComponent(entity, new CNoiseRandom {
               range = Range
            });
        }
    }
}

[MaterialProperty("_NoiseScale")]
public struct CNoiseProperty : IComponentData
{ 
    public float Value; 
}
public struct CNoiseRandom : IRandomizable
{
    public float2 range;

    public void Randomize(uint seed, ref CNoiseProperty noiseProperty)
    {
        var random = Unity.Mathematics.Random.CreateFromIndex(seed);
        // Setting random speed
        noiseProperty.Value = random.NextFloat(range.x , range.y);
    }
}