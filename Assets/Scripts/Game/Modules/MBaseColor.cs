using Unity.Rendering;
using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseColorConfigSO", menuName = "Configs/Entities/BaseColorConfig")]
public class BaseColorConfigSO : BaseEntityConfigSO
{
    public override string Name() => "Base color";
    [Tooltip("Base color for material property")]
    public Color baseColor = new Color(1f, 1f, 1f, 1f);
    

    public override void Bake(IBaker baker, Entity entity)
    {
        baker.AddComponent(entity, new CColorProperty{
            Value = new float4(baseColor.r, baseColor.g,baseColor.b,baseColor.a)
        });
    }
}

[MaterialProperty("_BaseColor")]
public struct CColorProperty : IComponentData 
{ 
    public float4 Value; 
}
