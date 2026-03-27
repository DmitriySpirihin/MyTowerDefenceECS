using Unity.Entities;
using UnityEngine;

// Add to GameObject
public class GameStatsAuthoring : MonoBehaviour
{
    public class Baker : Baker<GameStatsAuthoring>
    {
        
        public override void Bake(GameStatsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new CGlobalStatistics());
        }
    }
}