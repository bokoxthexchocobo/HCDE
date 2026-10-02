using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native map-load plane/copy scrollers, including Doom binary translations.</summary>
internal static class ScrollCarryInitialize
{
    internal static void ApplyMapLoadScrollers(AuthoritySimulation sim)
    {
        var copies = new List<(int Tag, int Mask, int Target)>();
        foreach (var line in sim.Level.Lines)
        {
            int? selectedMask = sim.Level.Format == MapDataFormat.DoomBinary
                ? line.Special switch { 352 => 1, 353 => 2, 354 => 6, _ => null }
                : sim.Level.Format is MapDataFormat.HexenBinary or MapDataFormat.UdmfText
                    && line.Special == LineSpecials.SectorCopyScroller ? line.Arg1 : null;
            if (selectedMask is not { } mask) continue;
            var tag = sim.Level.Format == MapDataFormat.DoomBinary ? line.Tag : line.Arg0;
            if ((uint)line.SideFront >= (uint)sim.Level.Sides.Count)
                throw new InvalidOperationException("CopyScroller has no front side.");
            var target = sim.Level.Sides[line.SideFront].Sector;
            if ((uint)target >= (uint)sim.Level.Sectors.Count)
                throw new InvalidOperationException("CopyScroller front sector is invalid.");
            if (!sim.Level.Sectors[target].HasTag(tag))
                copies.Add((tag, mask, target));
            line.Special = 0;
        }
        foreach (var line in sim.Level.Lines)
        {
            if (PlaneDefinition(sim.Level.Format, line) is { } definition)
                ApplyPlaneScrollLine(sim, line, copies, definition);
            else if (sim.Level.Format == MapDataFormat.DoomBinary
                ? line.Special is 218 or 249 or 254 : line.Special == 222)
                ApplyWallModel(sim, line);
            else if (sim.Level.Format == MapDataFormat.DoomBinary
                ? line.Special is 48 or 85 or >= 422 and <= 428 : line.Special is >= 100 and <= 103 or 221)
                ApplyDirectionalWall(sim, line);
            else if (sim.Level.Format == MapDataFormat.DoomBinary
                ? line.Special is 255 or 1024 or 1025 or 1026 : line.Special == 225)
                ApplyOffsetWall(sim, line);
        }
    }

    private static WallScrollParts Parts(int mask) =>
        (WallScrollParts)(mask <= 0 || (mask & ~7) != 0 ? 7 : mask);

    private static void ApplyOffsetWall(AuthoritySimulation sim, LevelLine source)
    {
        var doom = sim.Level.Format == MapDataFormat.DoomBinary;
        var parts = doom ? WallScrollParts.All : Parts(source.Arg0);
        var id = doom ? source.Special == 255 ? 0 : source.Tag : source.Arg1;
        var flags = doom ? source.Special switch { 1025 => 1, 1026 => 2, _ => 0 } : source.Arg2;
        var divider = doom ? source.Special == 255 ? 1 : 8 : Math.Max(1, source.Arg3);
        if ((uint)source.SideFront >= (uint)sim.Level.Sides.Count)
            throw new InvalidOperationException("Offset wall scroller has no front side.");
        var side = sim.Level.Sides[source.SideFront];
        var control = (flags & 3) != 0 ? side.Sector : -1;
        if ((flags & 3) != 0 && (uint)control >= (uint)sim.Level.Sectors.Count)
            throw new InvalidOperationException("Offset wall scroller controller sector is invalid.");
        var dx = -side.MidTextureOffsetX / divider;
        var dy = side.MidTextureOffsetY / divider;
        foreach (var target in sim.Level.Lines)
        {
            if (id == 0 ? !ReferenceEquals(target, source) : !target.HasId(id)) continue;
            if ((uint)target.SideFront >= (uint)sim.Level.Sides.Count)
                throw new InvalidOperationException("Offset wall scroller target has no front side.");
            if (control >= 0) sim.AppendControlWallScroll(target.SideFront, control, dx, dy, (flags & 2) != 0, parts);
            else sim.AppendSideTextureScroll(target.SideFront, dx, dy, parts);
        }
        source.Special = 0;
    }

    private static void ApplyDirectionalWall(AuthoritySimulation sim, LevelLine line)
    {
        var doom = sim.Level.Format == MapDataFormat.DoomBinary;
        var both = doom ? line.Special is >= 425 and <= 428 : line.Special == 221;
        if (!doom && both && line.Arg0 != 0) { line.Special = 0; return; }
        if ((uint)line.SideFront >= (uint)sim.Level.Sides.Count)
            throw new InvalidOperationException("Directional wall scroller has no front side.");
        var parts = doom ? WallScrollParts.All : Parts(line.Arg1);
        // Native SCROLL_UNIT is 64; directional/Both actions divide it by 64.
        var dx = doom ? line.Special switch
        {
            48 or 425 or 426 => 1,
            85 or 422 or 427 or 428 => -1,
            _ => 0,
        } : line.Special switch
        {
            100 => line.Arg0 / 64.0,
            101 => -line.Arg0 / 64.0,
            221 => (line.Arg1 - (double)line.Arg2) / 64.0,
            _ => 0,
        };
        var dy = doom ? line.Special switch
        {
            423 or 425 or 427 => 1,
            424 or 426 or 428 => -1,
            _ => 0,
        } : line.Special switch
        {
            102 => line.Arg0 / 64.0,
            103 => -line.Arg0 / 64.0,
            221 => (line.Arg4 - (double)line.Arg3) / 64.0,
            _ => 0,
        };
        sim.AppendSideTextureScroll(line.SideFront, dx, dy, both ? WallScrollParts.All : parts);
        if (both) line.Special = 0;
    }

    private static void ApplyWallModel(AuthoritySimulation sim, LevelLine source)
    {
        var doom = sim.Level.Format == MapDataFormat.DoomBinary;
        var flags = doom ? source.Special switch { 218 => 2, 249 => 1, _ => 0 } : source.Arg1;
        var id = doom ? source.Tag : source.Arg0;
        var control = -1;
        if ((flags & 3) != 0)
        {
            if ((uint)source.SideFront >= (uint)sim.Level.Sides.Count)
                throw new InvalidOperationException("Wall scroll controller has no front side.");
            control = sim.Level.Sides[source.SideFront].Sector;
            if ((uint)control >= (uint)sim.Level.Sectors.Count)
                throw new InvalidOperationException("Wall scroll controller sector is invalid.");
        }
        var dx = (source.X2 - source.X1) / 32.0;
        var dy = (source.Y2 - source.Y1) / 32.0;
        foreach (var target in sim.Level.Lines)
        {
            if (id == 0 ? !ReferenceEquals(target, source) : ReferenceEquals(target, source) || !target.HasId(id))
                continue;
            if ((uint)target.SideFront >= (uint)sim.Level.Sides.Count)
                throw new InvalidOperationException("Wall model scroller target has no front side.");
            var lx = target.X2 - target.X1;
            var ly = target.Y2 - target.Y1;
            var x = Math.Max(Math.Abs(lx), Math.Abs(ly));
            var y = Math.Min(Math.Abs(lx), Math.Abs(ly));
            if (x == 0) throw new InvalidOperationException("Wall model scroller target has zero length.");
            var length = x / Math.Sin(Math.Atan2(y, x) + Math.PI / 2);
            var tx = -(dy * ly + dx * lx) / length;
            var ty = -(dx * ly - dy * lx) / length;
            if (control >= 0)
                sim.AppendControlWallScroll(target.SideFront, control, tx, ty, (flags & 2) != 0);
            else sim.AppendSideTextureScroll(target.SideFront, tx, ty);
        }
        source.Special = 0;
    }

    // Doom retains raw special numbers; decode the plane subset of native base.txt.
    private static (bool Floor, int Tag, int Flags, int Mode)? PlaneDefinition(MapDataFormat format, LevelLine line)
    {
        if (format != MapDataFormat.DoomBinary)
            return line.Special is LineSpecials.ScrollFloor or LineSpecials.ScrollCeiling
                ? (line.Special == LineSpecials.ScrollFloor, line.Arg0, line.Arg1, line.Arg2) : null;
        var flags = line.Special switch
        {
            >= 214 and <= 217 => 6,
            >= 245 and <= 248 => 5,
            >= 250 and <= 253 => 4,
            _ => 0,
        };
        if (flags == 0) return null;
        var first = flags == 6 ? 214 : flags == 5 ? 245 : 250;
        var component = line.Special - first;
        return (component != 0, line.Tag, flags, component < 2 ? 0 : component - 1);
    }

    internal static void ApplyUdmfWallScrolls(AuthoritySimulation sim)
    {
        for (var sideIndex = 0; sideIndex < sim.Level.Sides.Count; sideIndex++)
            foreach (var (dx, dy, _) in sim.Level.Sides[sideIndex].MapLoadWallScrolls)
                // Native UDMF creation omits the computed part mask and defaults to all parts.
                sim.AppendSideTextureScroll(sideIndex, dx, dy);
    }

    private static IEnumerable<int> Targets(AuthoritySimulation sim, int tag, int mask,
        List<(int Tag, int Mask, int Target)> copies)
    {
        for (var sector = 0; sector < sim.Level.Sectors.Count; sector++)
            if (sim.Level.Sectors[sector].MatchesTag(tag)) yield return sector;
        foreach (var copy in copies)
            if (copy.Tag == tag && (copy.Mask & mask) != 0) yield return copy.Target;
    }

    private static void ApplyPlaneScrollLine(AuthoritySimulation sim, LevelLine line,
        List<(int Tag, int Mask, int Target)> copies, (bool Floor, int Tag, int Flags, int Mode) definition)
    {
        var (floor, tag, flags, mode) = definition;
        var plane = floor ? SectorTextureScrollPlane.Floor : SectorTextureScrollPlane.Ceiling;
        var controlled = (flags & 3) != 0;
        var accelerating = (flags & 2) != 0;
        var control = -1;
        if (controlled)
        {
            if ((uint)line.SideFront >= (uint)sim.Level.Sides.Count)
                throw new InvalidOperationException("Plane scroll controller has no front side.");
            control = sim.Level.Sides[line.SideFront].Sector;
            if ((uint)control >= (uint)sim.Level.Sectors.Count)
                throw new InvalidOperationException("Plane scroll controller sector is invalid.");
        }
        var dx = (flags & 4) != 0 ? (line.X2 - line.X1) / 32.0 : (line.Arg3 - 128) / 32.0;
        var dy = (flags & 4) != 0 ? (line.Y2 - line.Y1) / 32.0 : (line.Arg4 - 128) / 32.0;
        if (!floor || mode != 1)
        {
            foreach (var sector in Targets(sim, tag, floor ? 2 : 1, copies))
            {
                if (controlled) sim.AppendControlPlaneScroll(sector, control, -dx, dy, accelerating, plane);
                else sim.AppendSectorTextureScroll(sector, -dx, dy, plane);
            }
        }
        if (floor && mode > 0)
        {
            foreach (var sector in Targets(sim, tag, 4, copies))
            {
                if (controlled)
                    sim.RegisterControlSectorCarry(sector, control, dx, dy,
                        sim.Floors[control] + sim.Ceilings[control], accelerating);
                else sim.AppendSectorCarryScroll(sector, dx, dy);
            }
        }
        line.Special = 0;
    }
}
