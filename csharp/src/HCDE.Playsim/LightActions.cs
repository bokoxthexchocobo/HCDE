namespace HCDE.Playsim;

internal enum LightAction { Change, Raise, Lower, Minimum, Maximum }

internal static class LightActions
{
    internal static bool? ExecuteSpecial(AuthoritySimulation sim, int special, int tag, int a = 0, int b = 0, int c = 0, int d = 0) => special switch
    {
        110 => Execute(sim, tag, LightAction.Raise, a),
        111 => Execute(sim, tag, LightAction.Lower, a),
        112 => Execute(sim, tag, LightAction.Change, a),
        113 => Animate(sim, tag, LightEffectKind.Fade, a, 0, b),
        114 => Animate(sim, tag, LightEffectKind.Glow, a, b, c),
        115 => Flicker(sim, tag, a, b),
        116 => Animate(sim, tag, LightEffectKind.Strobe, a, b, c, d),
        117 => Stop(sim, tag),
        232 => DoomStrobe(sim, tag, a, b),
        233 => Execute(sim, tag, LightAction.Minimum),
        234 => Execute(sim, tag, LightAction.Maximum),
        _ => null,
    };

    internal static void Initialize(AuthoritySimulation sim)
    {
        var specials = sim.Level.Sectors.Select(sector =>
            sim.Level.Format == HCDE.MapLoader.MapDataFormat.UdmfText
                && sim.Level.Namespace.Equals("Doom", StringComparison.OrdinalIgnoreCase)
                && (sector.Special & 31) is 17 or 21 or 22 or 23 or 24 ? 0 :
            sim.Level.UsesDoomSectorNumbers
                ? (sector.Special & 31) switch { 1 => 65, 2 => 66, 3 => 67, 4 => 68, 8 => 72, 12 => 76, 13 => 77,
                    17 => 81, 21 => 1, 22 => 2, 23 => 3, 24 => 4, _ => 0 }
                : (int)sector.Special).ToArray();
        for (var sector = 0; sector < sim.Level.Sectors.Count; sector++)
        {
            // Native strips flags as each sector is initialized, not for the whole map up front.
            var special = specials[sector] &= 255;
            if (special == 72)
                sim.LightEffects.Add(new LightEffect { Sector = sector, Kind = LightEffectKind.SectorGlow,
                    Start = sim.LightOf(sector), End = NeighborMinimum(sim, sector), Tics = -1 });
            else if (special is 66 or 67 or 68 or 76 or 77 or 104)
                AddDoomStrobe(sim, sector, 5, special is 67 or 76 ? 35 : 15, special is 76 or 77);
            else if (special == 65)
                sim.LightEffects.Add(new LightEffect { Sector = sector, Kind = LightEffectKind.LightFlash,
                    Start = sim.LightOf(sector), End = NeighborMinimum(sim, sector),
                    Tics = (sim.NextLightFlashRandom() & 64) + 1 });
            else if (special == 81)
                sim.LightEffects.Add(new LightEffect { Sector = sector, Kind = LightEffectKind.FireFlicker,
                    Start = sim.LightOf(sector), End = Math.Clamp(NeighborMinimum(sim, sector) + 16, short.MinValue, short.MaxValue),
                    Tics = 4 });
            else if (special == 1)
                sim.LightEffects.Add(new LightEffect { Sector = sector, Kind = LightEffectKind.Phased,
                    Start = 48, Tics = 63 - (sim.LightOf(sector) & 63) });
            else if (special == 2)
                InitializeSequence(sim, sector, specials);
        }
    }

    private static void InitializeSequence(AuthoritySimulation sim, int sector, int[] specials)
    {
        var chain = new List<(int Sector, int Base)>();
        var visited = new HashSet<int>();
        var previous = -1; var brightness = 0;
        while (sector >= 0 && visited.Add(sector))
        {
            if (sim.LightOf(sector) != 0) brightness = sim.LightOf(sector);
            chain.Add((sector, brightness));
            var target = specials[sector] == 3 ? 4 : 3;
            var next = -1;
            foreach (var line in sim.Level.Lines)
            {
                if ((uint)line.SideFront >= (uint)sim.Level.Sides.Count || (uint)line.SideBack >= (uint)sim.Level.Sides.Count) continue;
                var front = sim.Level.Sides[line.SideFront].Sector; var back = sim.Level.Sides[line.SideBack].Sector;
                var other = front == sector ? back : back == sector ? front : -1;
                if ((uint)other >= (uint)specials.Length || other == sector || other == previous || specials[other] != target) continue;
                next = other; break; // Native chooses the first matching neighbor, even if already visited.
            }
            previous = sector; sector = next;
        }
        for (var i = 0; i < chain.Count; i++)
        {
            var entry = chain[i];
            sim.LightEffects.Add(new LightEffect { Sector = entry.Sector, Kind = LightEffectKind.Phased,
                Start = entry.Base, Tics = ((long)(chain.Count - i - 1) * 64) / chain.Count });
            specials[entry.Sector] = 0;
        }
    }

    private static short NeighborMinimum(AuthoritySimulation sim, int sector)
    {
        var lower = sim.LightOf(sector);
        foreach (var line in sim.Level.Lines)
        {
            if ((uint)line.SideFront >= (uint)sim.Level.Sides.Count || (uint)line.SideBack >= (uint)sim.Level.Sides.Count) continue;
            var front = sim.Level.Sides[line.SideFront].Sector; var back = sim.Level.Sides[line.SideBack].Sector;
            var other = front == sector ? back : back == sector ? front : -1;
            if (other == sector || (uint)other >= (uint)sim.Lights.Length) continue;
            lower = Math.Min(lower, sim.LightOf(other));
        }
        return lower;
    }

    private static void AddDoomStrobe(AuthoritySimulation sim, int sector, int brightTime, int darkTime, bool sync)
    {
        var upper = sim.LightOf(sector); var lower = NeighborMinimum(sim, sector);
        sim.LightEffects.Add(new LightEffect
        {
            Sector = sector, Kind = LightEffectKind.Strobe, Start = upper,
            End = lower == upper ? 0 : lower, Duration = brightTime, DarkTime = darkTime,
            Tics = sync ? 1 : sim.NextStrobeDelay(),
        });
    }

    // These native thinkers do not acquire sector.lightingdata. Multiple effects
    // can coexist; the separate FraggleScript light lock is not implemented yet.
    internal static bool Flicker(AuthoritySimulation sim, int tag, int upper, int lower)
    {
        for (var sector = 0; sector < sim.Level.Sectors.Count; sector++)
        {
            if (!sim.Level.Sectors[sector].MatchesTag(tag)) continue;
            var start = Math.Clamp(upper, short.MinValue, short.MaxValue);
            sim.Lights[sector] = (short)start;
            sim.LightEffects.Add(new LightEffect
            {
                Sector = sector, Kind = LightEffectKind.Flicker, Start = start,
                End = Math.Clamp(lower, short.MinValue, short.MaxValue),
                Tics = (sim.NextFlickerRandom() & 64) + 1,
            });
        }
        return true;
    }

    internal static bool DoomStrobe(AuthoritySimulation sim, int tag, int brightTime, int darkTime)
    {
        for (var sector = 0; sector < sim.Level.Sectors.Count; sector++)
        {
            if (!sim.Level.Sectors[sector].MatchesTag(tag)) continue;
            AddDoomStrobe(sim, sector, brightTime, darkTime, false);
        }
        return true;
    }

    internal static bool Animate(AuthoritySimulation sim, int tag, LightEffectKind kind, int upper, int lower, int tics, int darkTime = 0)
    {
        if (kind == LightEffectKind.Glow && tics <= 0) return true;
        if (kind == LightEffectKind.Glow && upper < lower) (upper, lower) = (lower, upper);
        for (var sector = 0; sector < sim.Level.Sectors.Count; sector++)
        {
            if (!sim.Level.Sectors[sector].MatchesTag(tag)) continue;
            if (kind == LightEffectKind.Fade && tics <= 0)
            {
                sim.Lights[sector] = (short)Math.Clamp(upper, short.MinValue, short.MaxValue);
                continue;
            }
            if (kind == LightEffectKind.Fade && sim.LightOf(sector) == upper) continue;
            sim.LightEffects.Add(new LightEffect
            {
                Sector = sector, Kind = kind,
                Start = kind == LightEffectKind.Fade ? sim.LightOf(sector) : Math.Clamp(upper, short.MinValue, short.MaxValue),
                End = Math.Clamp(kind == LightEffectKind.Fade ? upper : lower, short.MinValue, short.MaxValue),
                Duration = tics, DarkTime = darkTime, Tics = kind == LightEffectKind.Strobe ? 1 : -1,
            });
        }
        return true;
    }

    internal static bool Stop(AuthoritySimulation sim, int tag)
    {
        sim.LightEffects.RemoveAll(effect => sim.Level.Sectors[effect.Sector].MatchesTag(tag));
        return true;
    }

    internal static bool Execute(AuthoritySimulation sim, int tag, LightAction action, int value = 0)
    {
        var turnOn = action is LightAction.Change or LightAction.Maximum;
        long brightness = action == LightAction.Maximum ? -1 : value;
        // Native light actions use the tag iterator even for tag zero, not the line's back sector.
        for (var sector = 0; sector < sim.Level.Sectors.Count; sector++)
        {
            if (!sim.Level.Sectors[sector].MatchesTag(tag)) continue;
            long light = action switch
            {
                LightAction.Raise => (long)sim.LightOf(sector) + value,
                LightAction.Lower => (long)sim.LightOf(sector) - value,
                LightAction.Minimum => sim.LightOf(sector),
                _ => brightness,
            };
            if (action == LightAction.Minimum || turnOn && brightness < 0)
            {
                foreach (var line in sim.Level.Lines)
                {
                    if ((uint)line.SideFront >= (uint)sim.Level.Sides.Count || (uint)line.SideBack >= (uint)sim.Level.Sides.Count) continue;
                    var front = sim.Level.Sides[line.SideFront].Sector; var back = sim.Level.Sides[line.SideBack].Sector;
                    var other = front == sector ? back : back == sector ? front : -1;
                    if (other == sector || (uint)other >= (uint)sim.Lights.Length) continue;
                    light = action == LightAction.Minimum ? Math.Min(light, sim.LightOf(other)) : Math.Max(light, sim.LightOf(other));
                }
            }
            sim.Lights[sector] = (short)Math.Clamp(light, short.MinValue, short.MaxValue);
            if (turnOn && sim.Compat.HasFlag(CompatSurface.SharedLightMaximum)) brightness = light;
        }
        return true; // Native immediate light actions succeed even if no sector matches.
    }
}
