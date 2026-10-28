namespace HCDE.Playsim;

/// <summary>Faithful port of <c>P_CheckOnmobj</c> and <c>P_TestMobjZ</c> (<c>p_map.cpp</c> 2077–2247).</summary>
public static class VmPMapTestMobjZ
{
    public const int MfSolid = 1 << 0;
    public const int MfNoclip = 1 << 1;
    public const int MfSpecial = 1 << 2;
    public const int MfMissile = 1 << 3;
    public const int MfCorpse = 1 << 4;
    public const int MfIcecorpse = 1 << 5;
    public const int Mf2Thruactors = 1 << 0;
    public const int Mf2Rip = 1 << 1;
    public const int Mf2Nonshootable = 1 << 2;
    public const int Mf2Thrughost = 1 << 3;
    public const int Mf2Reflective = 1 << 4;
    public const int Mf2Boss = 1 << 5;
    public const int Mf3Ghost = 1 << 0;
    public const int Mf4Actlikebridge = 1 << 0;
    public const int Mf4Spectral = 1 << 1;
    public const int Mf5Dontrip = 1 << 0;
    public const int Mf6Touchy = 1 << 0;
    public const int Mf6Thruspecies = 1 << 1;
    public const int Mf6Mthruspecies = 1 << 2;
    public const int Mf6Nobossrip = 1 << 3;
    public const int Mf7Thureflect = 1 << 0;
    public const int Mf8Allowthrubits = 1 << 0;
    public const int Mf8Hitowner = 1 << 1;
    public const int BounceMbf = 1 << 0;

    public sealed class ThingProbe
    {
        public required string Id { get; init; }
        public int Flags { get; init; }
        public int Flags2 { get; init; }
        public int Flags3 { get; init; }
        public int Flags4 { get; init; }
        public int Flags5 { get; init; }
        public int Flags6 { get; init; }
        public int Flags7 { get; init; }
        public int Flags8 { get; init; }
        public int ThruBits { get; init; }
        public int BounceFlags { get; init; }
        public double Radius { get; init; }
        public double PosX { get; init; }
        public double PosY { get; init; }
        public double Z { get; init; }
        public double Top { get; init; }
        public double Height { get; init; }
        public double ProjectilePassHeight { get; init; }
        public int Species { get; init; }
    }

    public sealed class TestMobjZState
    {
        public required string ActorId { get; init; }
        public int ActorFlags { get; set; }
        public int ActorFlags2 { get; set; }
        public int ActorFlags4 { get; set; }
        public int ActorFlags6 { get; set; }
        public int ActorFlags8 { get; set; }
        public int ActorThruBits { get; set; }
        public int BounceFlags { get; set; }
        public double ActorRadius { get; set; }
        public double ActorPosX { get; set; }
        public double ActorPosY { get; set; }
        public double ActorZ { get; set; }
        public double ActorTop { get; set; }
        public bool HasTarget { get; set; }
        public string? TargetThingId { get; set; }
        public int ActorSpecies { get; set; }
        public int TargetSpecies { get; set; }
        public bool TargetIsPlayer { get; set; }
        public bool MissileShouldClip { get; set; }
        public IReadOnlyList<ThingProbe>? Things { get; set; }
        public Func<IEnumerable<ThingProbe>>? EnumerateThings { get; set; }
        public Func<ThingProbe, bool>? ShouldPassThroughPlayer { get; init; }
        public Func<ThingProbe, bool>? IgnoreCorpse { get; init; }
        public Func<ThingProbe, bool>? CanCollideWith { get; init; }
        public Func<ThingProbe, bool>? CheckRipLevel { get; init; }
        public Func<ThingProbe, bool>? TargetPassesThroughThing { get; init; }
    }

    public sealed class CheckOnmobjState
    {
        public double ActorZ { get; set; }
        public bool Predicting { get; set; }
        public ThingProbe? BlockingMobj { get; set; }
        public required TestMobjZState TestMobj { get; init; }
        public Action? FakeZMovement { get; init; }
        public Action<double>? SetZ { get; init; }
        public Action<ThingProbe>? CollidedWith { get; init; }
    }

    public static ThingProbe? CheckOnmobj(CheckOnmobjState s)
    {
        var oldz = s.ActorZ;
        s.FakeZMovement?.Invoke();
        var good = TestMobjZ(s.TestMobj, false, out var onmobj);
        if (!good && onmobj != null && !ReferenceEquals(onmobj, s.BlockingMobj) && !s.Predicting)
            s.CollidedWith?.Invoke(onmobj);
        s.SetZ?.Invoke(oldz);
        return good ? null : onmobj;
    }

    public static bool TestMobjZ(TestMobjZState s, bool quick, out ThingProbe? onmobj)
    {
        onmobj = null;
        if ((s.ActorFlags & MfNoclip) != 0 || (s.ActorFlags2 & Mf2Thruactors) != 0)
            return true;

        var things = s.Things?.ToList()
                     ?? s.EnumerateThings?.Invoke().ToList()
                     ?? new List<ThingProbe>();
        var actorTop = s.ActorTop > s.ActorZ ? s.ActorTop : s.ActorZ + 1;

        foreach (var thing in things)
        {
            if (thing.Id == s.ActorId)
                continue;
            if (!quick && onmobj != null && thing.Top < onmobj.Top)
                continue;
            if ((thing.Flags2 & Mf2Thruactors) != 0)
                continue;
            if ((s.ActorThruBits & thing.ThruBits) != 0 &&
                ((s.ActorFlags8 | thing.Flags8) & Mf8Allowthrubits) != 0)
                continue;
            if (s.IgnoreCorpse?.Invoke(thing) == true)
                continue;
            if ((thing.Flags & MfSolid) == 0)
                continue;
            if ((thing.Flags & MfSpecial) != 0 || (thing.Flags & MfNoclip) != 0 || (thing.Flags6 & Mf6Touchy) != 0)
                continue;

            var blockdist = thing.Radius + s.ActorRadius;
            if (Math.Abs(thing.PosX - s.ActorPosX) >= blockdist || Math.Abs(thing.PosY - s.ActorPosY) >= blockdist)
                continue;
            if ((s.ActorFlags6 & Mf6Thruspecies) != 0 && thing.Species == s.ActorSpecies)
                continue;
            if ((thing.Flags & MfCorpse) != 0 && (s.ActorFlags & MfIcecorpse) == 0)
                continue;
            if ((thing.Flags4 & Mf4Actlikebridge) == 0 && (s.ActorFlags & MfSpecial) != 0)
                continue;
            if (s.ShouldPassThroughPlayer?.Invoke(thing) == true)
                continue;
            if (s.ActorZ > thing.Top)
                continue;
            if (actorTop <= thing.Z)
                continue;
            if (s.CanCollideWith?.Invoke(thing) == false)
                continue;

            if ((s.ActorFlags & MfMissile) != 0 ||
                ((s.BounceFlags & BounceMbf) != 0 && (s.ActorFlags & MfSolid) == 0))
            {
                if ((thing.Flags2 & Mf2Nonshootable) != 0)
                    continue;
                if ((thing.Flags3 & Mf3Ghost) != 0 && (s.ActorFlags2 & Mf2Thrughost) != 0)
                    continue;
                if ((thing.Flags4 & Mf4Spectral) != 0 && (s.ActorFlags4 & Mf4Spectral) == 0)
                    continue;
                if (s.HasTarget)
                {
                    if ((s.ActorFlags6 & Mf6Mthruspecies) != 0 && s.TargetSpecies == thing.Species)
                        continue;
                    if (s.TargetThingId == thing.Id && (s.ActorFlags8 & Mf8Hitowner) == 0)
                        continue;
                    if (s.TargetIsPlayer && s.TargetPassesThroughThing?.Invoke(thing) == true)
                        continue;
                }

                if ((thing.Flags7 & Mf7Thureflect) != 0 && (thing.Flags2 & Mf2Reflective) != 0 &&
                    (s.ActorFlags & MfMissile) != 0)
                    continue;

                var clipheight = thing.Height;
                if (thing.ProjectilePassHeight > 0)
                    clipheight = thing.ProjectilePassHeight;
                else if (thing.ProjectilePassHeight < 0 && s.MissileShouldClip)
                    clipheight = -thing.ProjectilePassHeight;

                if (s.ActorZ > thing.Z + clipheight)
                    continue;

                if ((s.ActorFlags2 & Mf2Rip) != 0 && (thing.Flags5 & Mf5Dontrip) == 0 &&
                    ((s.ActorFlags6 & Mf6Nobossrip) == 0 || (thing.Flags2 & Mf2Boss) == 0) &&
                    s.CheckRipLevel?.Invoke(thing) == true)
                    continue;
            }

            onmobj = thing;
            if (quick)
                break;
        }

        return onmobj == null;
    }
}
