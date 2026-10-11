namespace HCDE.Playsim;

/// <summary>Attack action timings from the Doom actor state tables; times are relative to entering Missile.</summary>
internal sealed record DoomMonsterAttack(MonsterAttack Kind, int[] Shots, int EndTic,
    ProjectileKind? Projectile = null, int Pellets = 1, int MeleeDice = 0, int MeleeMultiplier = 0,
    int RepeatFrom = -1, int MeleeShotTic = -1, int MeleeEndTic = -1,
    int[]? FaceTics = null, int[]? MeleeFaceTics = null, int RefireTic = -1);

internal static class DoomMonsterAttacks
{
    private static readonly Dictionary<int, DoomMonsterAttack> Profiles = new()
    {
        [3004] = new(MonsterAttack.Hitscan, [10], 26, FaceTics: [0]),
        [3006] = new(MonsterAttack.Fireball, [10], int.MaxValue, FaceTics: [0]),
        [71] = new(MonsterAttack.Fireball, [15], 16, FaceTics: [0, 5, 10]),
        [64] = new(MonsterAttack.Fireball, [10, 66], 94, FaceTics: [0, 18, 26, 34, 42, 50, 58]),
        [9] = new(MonsterAttack.Hitscan, [10], 30, Pellets: 3, FaceTics: [0]),
        [65] = new(MonsterAttack.Hitscan, [10, 14], 19, RepeatFrom: 10, FaceTics: [0], RefireTic: 18),
        [84] = new(MonsterAttack.Hitscan, [20, 30], 35, RepeatFrom: 10, FaceTics: [0, 10, 24], RefireTic: 34),
        [3002] = new(MonsterAttack.Melee, [16], 24, MeleeDice: 10, MeleeMultiplier: 4, FaceTics: [0, 8]),
        [58] = new(MonsterAttack.Melee, [16], 24, MeleeDice: 10, MeleeMultiplier: 4, FaceTics: [0, 8]),
        [3001] = new(MonsterAttack.Fireball, [16], 22, ProjectileKind.ImpBall, MeleeDice: 8, MeleeMultiplier: 3, FaceTics: [0, 8]),
        [3003] = new(MonsterAttack.Fireball, [16], 24, ProjectileKind.BaronBall, MeleeDice: 8, MeleeMultiplier: 10, FaceTics: [0, 8]),
        [69] = new(MonsterAttack.Fireball, [16], 24, ProjectileKind.BaronBall, MeleeDice: 8, MeleeMultiplier: 10, FaceTics: [0, 8]),
        [3005] = new(MonsterAttack.Fireball, [10], 15, ProjectileKind.CacodemonBall, MeleeDice: 6, MeleeMultiplier: 10, FaceTics: [0, 5]),
        [16] = new(MonsterAttack.Fireball, [6, 30, 54], 66, ProjectileKind.CyberRocket, FaceTics: [0, 18, 42]),
        [68] = new(MonsterAttack.Fireball, [20], 29, ProjectileKind.ArachnotronPlasma, RepeatFrom: 20, FaceTics: [0], RefireTic: 28),
        [7] = new(MonsterAttack.Hitscan, [20, 24], 29, Pellets: 3, RepeatFrom: 20, FaceTics: [0], RefireTic: 28),
        [67] = new(MonsterAttack.Fireball, [20, 40, 60], 80, ProjectileKind.MancubusBall, FaceTics: [0, 30, 35, 50, 55, 70, 75]),
        [66] = new(MonsterAttack.Fireball, [10], 30, ProjectileKind.RevenantTracer,
            MeleeDice: 10, MeleeMultiplier: 6, MeleeShotTic: 12, MeleeEndTic: 18,
            FaceTics: [0, 20], MeleeFaceTics: [0, 6]),
    };

    internal static DoomMonsterAttack? Find(int type) => Profiles.GetValueOrDefault(type);
}
