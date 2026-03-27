using Unity.Entities;
using UnityEngine;

// Add to GameObject
public class PlayerAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlayerAuthoring>
    {
        
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new CPlayerTag());
            AddComponent(entity, new CGlobalPlayerState());
        }
    }
}