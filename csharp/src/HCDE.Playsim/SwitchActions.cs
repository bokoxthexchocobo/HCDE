using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>p_switch.cpp</c> switch textures, buttons, and sounds.</summary>
internal static class SwitchActions
{
    internal static void Initialize(AuthoritySimulation sim) => SwitchCatalog.EnsureInitialized();

    internal static void Tick(AuthoritySimulation sim)
    {
        for (var i = sim.ActiveButtons.Count - 1; i >= 0; i--)
        {
            var button = sim.ActiveButtons[i];
            var def = button.Returning ? button.Definition.Pair : button.Definition;
            if (def is null)
            {
                sim.ActiveButtons.RemoveAt(i);
                continue;
            }
            if (--button.Timer != 0) continue;
            if (button.Frame == def.Frames.Length - 1)
            {
                button.Returning = true;
                var pair = button.Definition.Pair;
                if (pair is null)
                {
                    sim.ActiveButtons.RemoveAt(i);
                    continue;
                }
                button.Frame = -1;
                PlaySwitchSound(sim, button, pair.Sound ?? "switches/normbutn");
                button.Flippable = false;
            }
            var kill = button.AdvanceFrame(sim);
            def = button.Returning ? button.Definition.Pair! : button.Definition;
            ApplyFrame(sim, button.SideIndex, button.Part, def.Frames[button.Frame].Texture);
            if (kill) sim.ActiveButtons.RemoveAt(i);
        }
    }

    internal static bool TryChangeLineFront(AuthoritySimulation sim, LevelLine line, bool useAgain, int special)
        => TryChangeSide(sim, line.SideFront, line, useAgain, special);

    internal static bool TryChangeSide(AuthoritySimulation sim, int sideIndex, LevelLine? line, bool useAgain, int special)
    {
        if ((uint)sideIndex >= (uint)sim.Level.Sides.Count) return false;
        return TryChangeSide(sim, sim.Level.Sides[sideIndex], line, useAgain, special);
    }

    internal static bool TryChangeSide(AuthoritySimulation sim, LevelSide side, LevelLine? line, bool useAgain, int special)
    {
        SwitchDefinition? def = null;
        SwitchWallPart part = SwitchWallPart.Top;
        if ((def = SwitchCatalog.Find(side.TopTexture)) is not null) part = SwitchWallPart.Top;
        else if ((def = SwitchCatalog.Find(side.MidTexture)) is not null) part = SwitchWallPart.Mid;
        else if ((def = SwitchCatalog.Find(side.BottomTexture)) is not null) part = SwitchWallPart.Bottom;
        else return false;

        ApplyFrame(sim, side.Index, part, def.Frames[0].Texture);
        var (x, y) = line is null ? (0.0, 0.0) : ((line.X1 + line.X2) * 0.5, (line.Y1 + line.Y2) * 0.5);
        var playSound = useAgain || def.Frames.Length > 1
            ? StartButton(sim, side.Index, part, def, x, y, useAgain)
            : true;
        if (playSound) PlaySwitchSound(sim, side.Index, x, y, ResolveSound(def, special));
        return true;
    }

    private static string ResolveSound(SwitchDefinition def, int special)
    {
        if (!string.IsNullOrEmpty(def.Sound)) return def.Sound!;
        return special is LineSpecials.ExitNormal or 52 or 124 or 39 or 97
            ? "switches/exitbutn" : "switches/normbutn";
    }

    private static bool StartButton(AuthoritySimulation sim, int sideIndex, SwitchWallPart part, SwitchDefinition def,
        double x, double y, bool useAgain)
    {
        foreach (var existing in sim.ActiveButtons)
        {
            if (existing.SideIndex == sideIndex)
            {
                existing.Timer = 1;
                return false;
            }
        }
        var button = new ActiveButton
        {
            SideIndex = sideIndex,
            Part = part,
            Definition = def,
            PosX = x,
            PosY = y,
            Flippable = useAgain,
        };
        button.AdvanceFrame(sim);
        ApplyFrame(sim, sideIndex, part, def.Frames[button.Frame].Texture);
        sim.ActiveButtons.Add(button);
        return true;
    }

    private static void ApplyFrame(AuthoritySimulation sim, int sideIndex, SwitchWallPart part, string texture)
    {
        var side = sim.Level.Sides[sideIndex];
        switch (part)
        {
            case SwitchWallPart.Top: side.TopTexture = texture; break;
            case SwitchWallPart.Mid: side.MidTexture = texture; break;
            default: side.BottomTexture = texture; break;
        }
    }

    private static void PlaySwitchSound(AuthoritySimulation sim, ActiveButton button, string sound)
        => PlaySwitchSound(sim, button.SideIndex, button.PosX, button.PosY, sound);

    private static void PlaySwitchSound(AuthoritySimulation sim, int sideIndex, double x, double y, string sound)
    {
        var sector = (uint)sideIndex < (uint)sim.Level.Sides.Count ? sim.Level.Sides[sideIndex].Sector : 0;
        sim.QueueSoundRequest(new SimulationSoundRequest(0, sound, 0, (int)SimulationSoundFlags.ListenerZ,
            1, false, 1, false, DetachedOrigin: new SimulationSoundOrigin((float)x, (float)y, 0, sector)));
    }

    internal static bool CheckUseRange(AuthoritySimulation sim, PlayerPawn user, LevelLine line, bool backSide)
    {
        var sideIndex = backSide ? line.SideBack : line.SideFront;
        if (sideIndex < 0) return true;
        var flags = line.Flags;
        if ((flags & LevelLine.MidTex3DFlag) == 0 && (flags & LevelLine.CheckSwitchRangeFlag) == 0) return true;
        var side = sim.Level.Sides[sideIndex];
        var topZ = user.Z.ToDouble() + user.Height.ToDouble();
        var floorZ = user.Z.ToDouble();
        if (line.OneSided || line.SideBack < 0)
            return topZ >= sim.Floors[side.Sector] && floorZ <= sim.Ceilings[side.Sector];
        var front = sim.Level.Sides[line.SideFront].Sector;
        var back = sim.Level.Sides[line.SideBack].Sector;
        var lowFloor = Math.Max(sim.Floors[front], sim.Floors[back]);
        var highCeil = Math.Min(sim.Ceilings[front], sim.Ceilings[back]);
        if (highCeil <= lowFloor) return topZ >= lowFloor && floorZ <= highCeil;
        if (SwitchCatalog.Find(side.TopTexture) is not null)
            return topZ >= highCeil - 8;
        if (SwitchCatalog.Find(side.BottomTexture) is not null)
            return floorZ <= lowFloor + 8;
        if ((flags & LevelLine.MidTex3DFlag) != 0 || SwitchCatalog.Find(side.MidTexture) is not null)
            return floorZ < highCeil - 8 && topZ > lowFloor + 8;
        return topZ >= highCeil - 8 || floorZ <= lowFloor + 8;
    }
}
