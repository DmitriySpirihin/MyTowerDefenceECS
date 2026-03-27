namespace GameEnums
{
    public enum EntityType {None, Enemy, Friendly, Projectile, Collectible}

    public enum SpawnMode {Random, Grid, Burst, Curve }
    public enum SpawnType {Endless, Wave, OneTime, PlayerFire}

    public enum GameState {Menu, Pause, Run, Defeated, Win}
}
