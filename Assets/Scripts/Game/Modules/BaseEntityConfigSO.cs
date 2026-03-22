using UnityEngine;
using Unity.Entities;

public abstract class BaseEntityConfigSO : ScriptableObject
{
    public virtual string Name() => "Base config";
    [Tooltip ("Use this flag to mark the entity for InitializationSystem to randomize needed values")]
    public bool NeedsInitialization;

    [Tooltip("Min Max ranges for randomizer")]
    public Vector2 Range = Vector2.one;
    public virtual void Bake(IBaker baker, Entity entity)
    {
        if(baker == null)
        {
            Debug.Log("The Baker or Entity have not been assighned on ScaleConfigSO durind authoring process");
            return;
        }
    }
}