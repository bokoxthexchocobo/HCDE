using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>Scroll_Wall</c> (action special 52) and <c>SetWallScroller</c>.</summary>
internal static class WallScrollActions
{
    /// <summary>Eternity <c>Scroll_Wall</c> action number (not the Doom binary exit special).</summary>
    public const int ScrollWall = 52;
    public const int ScrollBoth = 221;

    public static bool ExecuteSpecial(AuthoritySimulation sim, LevelLine line)
        => Execute(sim, line.Special, line.Arg0, line.Arg1, line.Arg2, line.Arg3, line.Arg4) == true;

    public static bool? ExecuteAcs(AuthoritySimulation sim, int special, int id, int arg1, int arg2, int arg3, int arg4)
    {
        // Legacy synthetic maps use Doom exit 52 in ACS without a declared format.
        if (special == ScrollWall && sim.Level.Format == MapDataFormat.Unknown) return null;
        return Execute(sim, special, id, arg1, arg2, arg3, arg4);
    }

    public static bool? Execute(AuthoritySimulation sim, int special, int id, int arg1, int arg2, int arg3, int arg4)
    {
        if (special is not (ScrollWall or ScrollBoth)) return null;
        if (id == 0) return false;
        if (special == ScrollBoth)
        {
            var magnitude = Math.Abs((long)id);
            if (magnitude > int.MaxValue) return true;
            sim.SetWallTextureScroll((int)magnitude, id < 0 ? 1 : 0,
                (arg1 - (double)arg2) / 64.0, (arg4 - (double)arg3) / 64.0, WallScrollParts.All);
        }
        else if (id != -1)
            sim.SetWallTextureScroll(id, arg3 != 0 ? 1 : 0, arg1 / 65536.0, arg2 / 65536.0,
                (WallScrollParts)(arg4 & (int)WallScrollParts.All));
        return true;
    }

    internal static LevelLine? FindLineForSide(PlayLevel level, int sideIndex)
    {
        foreach (var line in level.Lines)
        {
            if (line.SideFront == sideIndex || line.SideBack == sideIndex)
                return line;
        }

        return null;
    }

    internal static bool ShouldScrollMid(LevelLine? line) =>
        line is null || line.SideBack < 0 || (line.Flags & LevelLine.MidTex3DFlag) == 0;

    internal static void ApplySideOffsets(LevelSide side, LevelLine? line, double dx, double dy, WallScrollParts parts)
    {
        if ((parts & WallScrollParts.Top) != 0)
        {
            side.TopTextureOffsetX += dx;
            side.TopTextureOffsetY += dy;
        }
        if ((parts & WallScrollParts.Mid) != 0 && ShouldScrollMid(line))
        {
            side.MidTextureOffsetX += dx;
            side.MidTextureOffsetY += dy;
        }
        if ((parts & WallScrollParts.Bottom) != 0)
        {
            side.BottomTextureOffsetX += dx;
            side.BottomTextureOffsetY += dy;
        }
    }
}
