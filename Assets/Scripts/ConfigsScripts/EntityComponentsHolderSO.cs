using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using GameEnums;

[CreateAssetMenu(fileName = "SpawnRangeConfigSO", menuName = "Configs/Entities/EntityComponentsHolder")]
public class EntityComponentsHolderSO : ScriptableObject
{
    [HideInInspector]public List<BaseEntityConfigSO> Modules = new();

    [Header ("Use this flag if need to randomize one or more component's values")]
    public bool NeedsInitialization;

    [Header ("Choose the type of entity")]
    public EntityType Type;

    public virtual void Bake(IBaker baker, Entity entity)
    {
        if(NeedsInitialization) 
            baker.AddComponent(entity, new CInitializationTag{});
        switch (Type)
        {
            case EntityType.Player:
                baker.AddComponent(entity, new CPlayerTag{});
            break;

            case EntityType.Enemy:
                baker.AddComponent(entity, new CEnemyTag{});
            break;

            case EntityType.Friendly:
                baker.AddComponent(entity, new CFriendly{});
            break;

            case EntityType.Projectile:
                baker.AddComponent(entity, new CProjectileTag{});
            break;

            case EntityType.Collectible:
                baker.AddComponent(entity, new CCollectibleTag{});
            break;
        }
    }
}