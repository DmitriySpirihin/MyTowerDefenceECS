using Unity.Entities;
using UnityEngine;

// Add to GameObject
public class UniversalEntityAuthoring : MonoBehaviour
{
    [SerializeField] private EntityComponentsHolderSO entityConfig;

    public class Baker : Baker<UniversalEntityAuthoring>
    {
        
        public override void Bake(UniversalEntityAuthoring authoring)
        {
            if (authoring.entityConfig == null) return;

            DependsOn(authoring.entityConfig);

            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            authoring.entityConfig.Bake(this, entity);

            if (authoring.entityConfig != null)
            {
                foreach (var module in authoring.entityConfig.Modules)
                {
                    module.Bake(this, entity);
                }
            }
        }
    }
}