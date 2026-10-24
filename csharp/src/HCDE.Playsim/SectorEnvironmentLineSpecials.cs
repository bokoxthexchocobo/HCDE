using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native sector environment line specials from <c>p_lnspec.cpp</c>.</summary>
internal static class SectorEnvironmentLineSpecials
{
    /// <summary>Native <c>SECF_FRICTION</c>.</summary>
    internal const int FrictionFlag = 16;
    /// <summary>Native <c>SECF_KILLMONSTERS</c>.</summary>
    internal const int KillMonstersFlag = 16384;
    public static bool? Execute(AuthoritySimulation sim, int special, int tag, int a1, int a2, int a3, int a4)
    {
        return special switch
        {
            98 => SetTranslucent(sim, tag, a1, a2),
            140 => SetSound(sim, tag, a1),
            159 => SetReflection(sim, tag, a1 != 0),
            219 => SetFriction(sim, tag, a1),
            220 => SetCurrent(sim, tag, a1, a2, a3),
            227 => SetCurrent(sim, tag, a1, a2, a3),
            277 => SetGlow(sim, tag, floor: true, a1, a2, a3),
            278 => SetGlow(sim, tag, floor: false, a1, a2, a3),
            109 => ForceLightning(sim, a1),
            120 => RadiusQuake(sim, a1, a2, a3, a4),
            _ => null,
        };
    }

    private static bool SetTranslucent(AuthoritySimulation sim, int tag, int amount, int additive)
    {
        foreach (var sector in sim.Level.Sectors)
        {
            if (!sector.MatchesTag(tag)) continue;
            sector.TranslucentAlpha = Math.Clamp(amount, 0, 255);
        }
        return true;
    }

    private static bool SetSound(AuthoritySimulation sim, int tag, int sequence)
    {
        foreach (var sector in sim.Level.Sectors)
        {
            if (!sector.MatchesTag(tag)) continue;
            sector.SoundSequence = sequence;
        }
        return true;
    }

    private static bool SetReflection(AuthoritySimulation sim, int tag, bool enabled)
    {
        foreach (var sector in sim.Level.Sectors)
        {
            if (!sector.MatchesTag(tag)) continue;
            sector.PlaneReflection = enabled;
        }
        return true;
    }

    private static bool SetFriction(AuthoritySimulation sim, int tag, int amount)
    {
        var friction = ((0x1EB8 * amount) / 128.0 + 0xD001) / 65536.0;
        friction = Math.Clamp(friction, 0, 1);
        var moveFactor = ActorPhysics.FrictionToMoveFactor(friction);
        foreach (var sector in sim.Level.Sectors)
        {
            if (!sector.MatchesTag(tag)) continue;
            sector.Friction = friction;
            sector.MoveFactor = moveFactor;
            if (Math.Abs(friction - ActorPhysics.GroundFriction) < 1e-6)
                sector.RuntimeFlags &= ~FrictionFlag;
            else
                sector.RuntimeFlags |= FrictionFlag;
        }
        return true;
    }

    private static bool SetCurrent(AuthoritySimulation sim, int tag, int angleByte, int force, int flags)
    {
        if (flags != 0) return false;
        PusherActions.AdjustCurrent(sim, tag, force, angleByte);
        return true;
    }

    private static bool SetGlow(AuthoritySimulation sim, int tag, bool floor, int r, int g, int b)
    {
        var color = (uint)((r & 255) | ((g & 255) << 8) | ((b & 255) << 16));
        foreach (var sector in sim.Level.Sectors)
        {
            if (!sector.MatchesTag(tag)) continue;
            if (floor) sector.FloorGlowColor = color;
            else sector.CeilingGlowColor = color;
        }
        return true;
    }

    private static bool ForceLightning(AuthoritySimulation sim, int mode)
    {
        sim.EnvironmentLightningForced = mode != 0;
        return true;
    }

    private static bool RadiusQuake(AuthoritySimulation sim, int intensity, int duration, int radius, int tid)
    {
        sim.EnvironmentQuakes.Add((intensity, duration, radius, tid));
        return true;
    }
}
