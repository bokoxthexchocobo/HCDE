namespace HCDE.Playsim;

public enum SpawnGameMode { Single, Cooperative, Deathmatch }
public sealed record SpawnOptions(int Skill = 2, SpawnGameMode Mode = SpawnGameMode.Single, string[]? StrArgs = null, double HealthFactor = 1, double ArmorFactor = 1)
{
    public bool Includes(HCDE.MapLoader.LevelThing thing) =>
        (Actor.IsPlayerStart(thing.Type) || (thing.SkillMask & (1 << Math.Clamp(Skill, 0, 4))) != 0)
        && (Mode switch { SpawnGameMode.Cooperative => thing.Coop, SpawnGameMode.Deathmatch => thing.Deathmatch, _ => thing.Single });
}
