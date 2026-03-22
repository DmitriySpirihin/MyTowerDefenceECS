using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[CreateAssetMenu(fileName = "DamageableConfigSO", menuName = "Configs/Entities/DamageableConfig")]
public class DamageableConfigSO : BaseEntityConfigSO
{
    public override string Name() => "Damageable";
    [Tooltip("Max health value")]
    [Range(1, 200)]public int Hp = 15;

    [Tooltip("Armor capacity , every hit reduce one armor point")]
    [Range(1, 20)]public int Armor = 1;

    [Tooltip("Damage multiplier that armor can reduce, more value equals less damage reducing")]
    [Range(0.1f, 1f)]public float ArmorDamageReduce = 0.5f;


    public override void Bake(IBaker baker, Entity entity)
    {
        base.Bake(baker, entity);
        // speed

        baker.AddComponent(entity, new CDamageable{
            maxHp = Hp,
            hp = Hp,
            armor = Armor,
            armorDamageReduce = ArmorDamageReduce
        });
        if(NeedsInitialization)
        {
            baker.AddComponent(entity, new CDamageableRandom {
               range = Range
            });
        }
    }
}


public struct CDamageable : IComponentData
{
    public int maxHp;
    public int hp;
    public int armor;
    public float armorDamageReduce;
    
    public readonly float GetNormalizedHp() => hp / maxHp;
    public readonly bool IsAlive() => hp / maxHp > 0.05f;
}

// Temp component to store values for randomization
public struct CDamageableRandom : IRandomizable
{
    public float2 range;

    public void Randomize(uint seed, ref CDamageable cDamageable)
    {
        var random = Unity.Mathematics.Random.CreateFromIndex(seed);
        // Setting random speed
        cDamageable.maxHp = (int)random.NextFloat(range.x , range.y);
        cDamageable.hp = cDamageable.maxHp;
    }
}