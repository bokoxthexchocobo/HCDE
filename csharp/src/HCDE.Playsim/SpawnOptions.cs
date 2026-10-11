namespace HCDE.Playsim;

public enum SpawnGameMode { Single, Cooperative, Deathmatch }
public sealed record SpawnOptions(int Skill = 2, SpawnGameMode Mode = SpawnGameMode.Single, string[]? StrArgs = null, double HealthFactor = 1, double ArmorFactor = 1,
    bool? FastMonsters = null, bool SlowMonsters = false, int? DefaultDropStyle = null, bool NoMonsterPain = false,
    double PlayerDamageFactor = 1, double MonsterDamageFactor = 1, double FriendlyDamageFactor = 1,
    double SkillDamageFactor = 1, bool AutoUseHealth = false, bool DisableAutoHealth = false, bool StrifeHealthPickups = false,
    bool UnlimitedHealthPickups = false, int DefaultInventoryMaximum = 25, bool RavenHealthPickups = false,
    bool InfiniteHealthItemUse = false, bool InfiniteStoredInventoryUse = false, bool InfiniteAmmo = false)
{
    public bool Includes(HCDE.MapLoader.LevelThing thing) =>
        (Actor.IsPlayerStart(thing.Type) || (thing.SkillMask & (1 << Math.Clamp(Skill, 0, 4))) != 0)
        && (Mode switch { SpawnGameMode.Cooperative => thing.Coop, SpawnGameMode.Deathmatch => thing.Deathmatch, _ => thing.Single });
}
