using UnityEngine;
using GameEnums;

[CreateAssetMenu(fileName = "SpawnConfigSO", menuName = "Configs/Game/SpawnConfig")]
public class SpawnConfigSO : ScriptableObject
{
    public SpawnArea spawnArea;
    public SpawnMode mode = SpawnMode.Random;
    public SpawnType type = SpawnType.Endless;
    [Range (0.01f, 2f)]public float tick = 1f;
    [Range (1f, 60f)]public float waveTick = 5f;
    public bool isSpawnTickNow = true;
    [Range (1, 50)]public int burstCount = 10;
    [Range (1f, 10f)]public float gridStep = 1f;
    public AnimationCurve distributionCurve = AnimationCurve.Linear(0,0,1,1);
}
