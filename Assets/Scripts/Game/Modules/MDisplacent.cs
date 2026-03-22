using Unity.Rendering;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "DisplaceConfigSO", menuName = "Configs/Entities/DisplaceConfig")]
public class DisplaceConfigSO : BaseEntityConfigSO
{
    public override string Name() => "Displacement";
    [Tooltip("Noise texture displace for material property")]
    [Range (0.05f, 1f)]public float displace = 0.1f;
    

    public override void Bake(IBaker baker, Entity entity)
    {
        baker.AddComponent(entity, new CDisplaceProperty{
            Value = displace
        });
        if(NeedsInitialization)
        {
            baker.AddComponent(entity, new CDisplaceRandom {
               range = Range
            });
        }
    }
}

[MaterialProperty("_DisplaceStrength")]
public struct CDisplaceProperty : IComponentData 
{ 
    public float Value; 
}
public struct CDisplaceRandom : IRandomizable
{
    public float2 range;

    public void Randomize(uint seed, ref CDisplaceProperty displaceProperty)
    {
        var random = Unity.Mathematics.Random.CreateFromIndex(seed);
        // Setting random speed
        displaceProperty.Value = random.NextFloat(range.x , range.y);
    }
}