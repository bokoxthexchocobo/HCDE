namespace HCDE.Playsim;

/// <summary><c>P_LineAttack</c> class overload from <c>p_map.cpp</c> (4699–5050).</summary>
public static class VmPMapLineAttack
{
    public const int LafNointeract = 1;
    public const int LafIsmeleeattack = 1 << 1;
    public const int LafNorandompuffz = 1 << 2;
    public const int LafOverridez = 1 << 3;
    public const int LafAbsposition = 1 << 4;
    public const int LafAbsoffset = 1 << 5;
    public const int LafNoimpactdecal = 1 << 6;
    public const int LafTargetissource = 1 << 7;
    public const int TraceNosky = 1;
    public const int TraceImpact = 1 << 1;
    public const int TraceHitsky = 1 << 2;
    public const int PfMeleerange = 1;
    public const int PfNorandomz = 1 << 1;
    public const int PfHitsky = 1 << 2;
    public const int PfHitthing = 1 << 3;
    public const int PfHitthingbleed = 1 << 4;
    public const int PfTemporary = 1 << 5;
    public const int Mf3Alwayspuff = 1;
    public const int Mf3Puffonactors = 1 << 1;
    public const int Mf3Bloodlessimpact = 1 << 2;
    public const int Mf3Skyexplode = 1 << 3;
    public const int Mf6Notrigger = 1;
    public const int Mf6Forcepain = 1 << 1;
    public const int Mf7Allowthruflags = 1;
    public const int Mf7Forcedecal = 1 << 1;
    public const int Mf7Causedpain = 1 << 2;
    public const int Mf7Nodecal = 1 << 3;
    public const int MfNoblood = 1;
    public const int Mf2Invulnerable = 1;
    public const int Mf2Dormant = 1 << 1;
    public const int Mf5Piercearmor = 1;
    public const int DmgPlayerattack = 1;
    public const int DmgInflictorIsPuff = 1 << 1;
    public const int DmgNoArmor = 1 << 2;
    public const int DmgUseangle = 1 << 3;
    public const int CompatfHitscan = 1;
    public const int TraceHitActor = 2;
    public const int TraceHitWall = 1;
    public const int TraceHitFloor = 3;
    public const int TraceHasHitsky = 4;

    public sealed class TraceResult
    {
        public int HitType { get; init; }
        public double HitX { get; init; }
        public double HitY { get; init; }
        public double HitZ { get; init; }
        public double HitVecX { get; init; }
        public double HitVecY { get; init; }
        public double HitVecZ { get; init; }
        public double SrcAngleDeg { get; init; }
        public object? Actor { get; init; }
        public object? Line { get; init; }
        public int LineSpecial { get; init; }
        public bool Unlinked { get; init; }
        public object? CrossedWater { get; init; }
        public object? Crossed3dWater { get; init; }
        public object? Sector { get; init; }
        public bool HeightSecNull { get; init; }
    }

    public sealed class PuffDefaults
    {
        public int Flags2 { get; init; }
        public int Flags3 { get; init; }
        public int Flags4 { get; init; }
        public int Flags6 { get; init; }
        public int Flags7 { get; init; }
        public int ThruBits { get; init; }
        public string? DamageType { get; init; }
        public bool ActiveSoundValid { get; init; }
        public int PoisonDamage { get; init; }
        public int PoisonDuration { get; init; }
        public object? DecalGenerator { get; init; }
    }

    public sealed class LineAttackState
    {
        public int Flags { get; init; }
        public int Damage { get; init; }
        public string DamageType { get; set; } = "None";
        public double Distance { get; init; }
        public double PitchSin { get; init; }
        public double PitchCos { get; init; }
        public double AngleCos { get; init; }
        public double AngleSin { get; init; }
        public double ShootZ { get; set; }
        public double OffsetForward { get; init; }
        public double OffsetSide { get; init; }
        public double Sz { get; init; }
        public bool HasPlayer { get; init; }
        public bool WeaponThruGhost { get; init; }
        public PuffDefaults? PuffDefaults { get; init; }
        public bool SuppressLocalHitscan { get; init; }
        public bool PreFiredBlocked { get; init; }
        public int LevelCompatFlags { get; init; }
        public Func<LineAttackState, (double x, double y, double z)>? BuildStartPos { get; init; }
        public Func<LineAttackState, TraceResult?>? Trace { get; init; }
        public Action? PlayMissSound { get; init; }
        public Func<LineAttackState, TraceResult, int, object?>? SpawnPuff { get; init; }
        public Action<TraceResult>? GeometryLineAttack { get; init; }
        public Func<LineAttackState, TraceResult, object?, int, int, int>? DamageMobj { get; init; }
        public Action<object?, object?, TraceResult>? SpawnDecal { get; init; }
        public Action<object?, TraceResult>? HitWater { get; init; }
        public Action<object?, TraceResult, object?>? DeepSplash { get; init; }
        public Action<object?, object?, int, int, int>? PoisonMobj { get; init; }
        public object? VictimOut { get; set; }
        public int ActualDamage { get; set; }
    }

    public static bool ShouldSuppressHitscan(bool suppress) => suppress;

    public static bool PreFireBlocked(bool blocked) => blocked;

    public static bool NoInteract(int flags) => (flags & LafNointeract) != 0;

    public static bool OverrideZ(int flags) => (flags & LafOverridez) != 0;

    public static bool AbsPosition(int flags) => (flags & LafAbsposition) != 0;

    public static bool AbsOffset(int flags) => (flags & LafAbsoffset) != 0;

    public static int MeleePuffFlags(int flags) => (flags & LafIsmeleeattack) != 0 ? PfMeleerange : 0;

    public static bool NorandomPuffZ(int flags) => (flags & LafNorandompuffz) != 0;

    public static bool ThruGhost(bool player, bool weapon, bool puff) =>
        player && weapon || puff;

    public static bool SpawnSkyPuff(PuffDefaults? d) => d != null && (d.Flags3 & Mf3Skyexplode) != 0;

    public static bool ClearImpactTrace(bool nointeract, PuffDefaults? d) =>
        nointeract || (d != null && (d.Flags6 & Mf6Notrigger) != 0);

    public static bool UsePuffDamageType(string requested, PuffDefaults? d) =>
        (requested is "None" or "Melee" or "Hitscan") && d?.DamageType is { Length: > 0 };

    public static bool TraceMissed(TraceResult? trace) => trace == null;

    public static bool HitActor(TraceResult trace) => trace.HitType == TraceHitActor;

    public static bool HitWall(TraceResult trace) => trace.HitType == TraceHitWall;

    public static bool HitSky(TraceResult trace) =>
        trace.HitType == TraceHasHitsky || (trace.HitType == TraceHitWall && trace.LineSpecial == 999);

    public static bool AlwaysPuff(PuffDefaults? d) => d != null && (d.Flags3 & Mf3Alwayspuff) != 0;

    public static bool PuffOnActors(PuffDefaults? d) => d != null && (d.Flags3 & Mf3Puffonactors) != 0;

    public static bool ActorNoBlood(object? actorFlags) => actorFlags is int f && (f & MfNoblood) != 0;

    public static bool ActorInvulnDormant(int flags2) => (flags2 & (Mf2Invulnerable | Mf2Dormant)) != 0;

    public static bool ShouldDamage(int damage, PuffDefaults? d) =>
        damage != 0 || (d != null && ((d.Flags6 & Mf6Forcepain) != 0 || (d.Flags7 & Mf7Causedpain) != 0));

    public static int BuildDamageFlags(bool playerAttack, bool pierceArmor) =>
        (playerAttack ? DmgPlayerattack : 0) | DmgInflictorIsPuff | (pierceArmor ? DmgNoArmor : 0) | DmgUseangle;

    public static bool BloodlessImpact(PuffDefaults? d) => d != null && (d.Flags3 & Mf3Bloodlessimpact) != 0;

    public static bool SkipDecal(int flags, PuffDefaults? d) =>
        (flags & LafNoimpactdecal) != 0 || (d?.Flags7 & Mf7Nodecal) != 0;

    public static object? LineAttack(LineAttackState st)
    {
        if (st.PreFiredBlocked)
            return null;
        if (ShouldSuppressHitscan(st.SuppressLocalHitscan))
        {
            st.ActualDamage = 0;
            return null;
        }

        var nointeract = NoInteract(st.Flags);
        var puffFlags = MeleePuffFlags(st.Flags);
        if (NorandomPuffZ(st.Flags))
            puffFlags |= PfNorandomz;

        st.ShootZ += st.Sz;

        if (UsePuffDamageType(st.DamageType, st.PuffDefaults))
            st.DamageType = st.PuffDefaults!.DamageType!;

        var start = st.BuildStartPos?.Invoke(st) ?? (0, 0, st.ShootZ);
        var trace = st.Trace?.Invoke(st);
        if (TraceMissed(trace))
        {
            if (!nointeract && st.PuffDefaults?.ActiveSoundValid == true)
                st.PlayMissSound?.Invoke();
            if (nointeract || AlwaysPuff(st.PuffDefaults))
            {
                var puff = st.SpawnPuff?.Invoke(st, new TraceResult { HitX = start.x, HitY = start.y, HitZ = start.z, SrcAngleDeg = 0 }, 2);
                return nointeract ? puff : null;
            }

            return null;
        }

        var hit = trace!;
        if (!HitActor(hit))
        {
            if (HitSky(hit))
                puffFlags |= PfHitsky;
            st.GeometryLineAttack?.Invoke(hit);
            if (nointeract || !HitWall(hit) || HitSky(hit))
            {
                var puff = st.SpawnPuff?.Invoke(st, hit, 0);
                if (nointeract)
                    return puff;
            }

            if (HitWall(hit) && hit.LineSpecial != 999 && !SkipDecal(st.Flags, st.PuffDefaults))
                st.SpawnDecal?.Invoke(null, null, hit);
            else if (hit.CrossedWater == null && hit.HeightSecNull && hit.HitType == TraceHitFloor)
                st.HitWater?.Invoke(null, hit);
        }
        else
        {
            var bleed = hit;
            if ((st.LevelCompatFlags & CompatfHitscan) != 0)
            {
                bleed = new TraceResult
                {
                    HitX = hit.HitX - 10 * hit.HitVecX,
                    HitY = hit.HitY - 10 * hit.HitVecY,
                    HitZ = hit.HitZ - 10 * hit.HitVecZ,
                    HitType = hit.HitType,
                    SrcAngleDeg = hit.SrcAngleDeg,
                    Actor = hit.Actor,
                };
            }

            object? puff = null;
            if (nointeract || PuffOnActors(st.PuffDefaults)
                || ActorNoBlood(hit.Actor)
                || ActorInvulnDormant(0))
            {
                puff = st.SpawnPuff?.Invoke(st, bleed, 2);
            }

            if (nointeract)
                return puff;

            if (ShouldDamage(st.Damage, st.PuffDefaults))
            {
                var dmgFlags = BuildDamageFlags(st.HasPlayer, false);
                st.ActualDamage = st.DamageMobj?.Invoke(st, hit, puff, st.Damage, dmgFlags) ?? 0;
            }

            if (!BloodlessImpact(st.PuffDefaults) && st.Damage != 0)
            {
                // blood + trace bleed injected at seam
            }
        }

        if (hit.Crossed3dWater != null || hit.CrossedWater != null)
            st.DeepSplash?.Invoke(null, hit, null);

        return null;
    }
}
