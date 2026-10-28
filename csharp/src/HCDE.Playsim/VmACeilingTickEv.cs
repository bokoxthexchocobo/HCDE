namespace HCDE.Playsim;

/// <summary>Ceiling sound, tick, and EV helpers from <c>a_ceiling.cpp</c> (97–117, 125–215, 506–621).</summary>
public static class VmACeilingTickEv
{
    public sealed class CeilingTickThinker
    {
        public required VmACeilingCreate.CeilingSector Sector { get; init; }
        public int Type { get; set; }
        public int Direction { get; set; }
        public int OldDirection { get; set; }
        public double Speed { get; set; }
        public double Speed1 { get; set; }
        public double Speed2 { get; set; }
        public double TopHeight { get; set; }
        public double BottomHeight { get; set; }
        public int Crush { get; set; }
        public int CrushMode { get; set; }
        public int Silent { get; set; }
        public int Tag { get; set; }
        public string Texture { get; set; } = "";
        public VmACeilingCreate.SectorSpecial NewSpecial { get; set; } = new();
        public bool Destroyed { get; set; }
    }

    public sealed class SoundContext
    {
        public Action<VmACeilingCreate.CeilingSector, int, int, int, int, bool>? StartSequenceType { get; init; }
        public Action<VmACeilingCreate.CeilingSector, int, string, int>? StartSequenceName { get; init; }
        public Action<VmACeilingCreate.CeilingSector, int, string, int>? StartSequenceSound { get; init; }
    }

    public sealed class TickContext
    {
        public SoundContext Sound { get; init; } = new();
        public Func<VmACeilingCreate.CeilingSector, double, double, int, VmADoorTickSound.MoveResult>? MoveCeilingUp { get; init; }
        public Func<VmACeilingCreate.CeilingSector, double, double, int, int, bool, VmADoorTickSound.MoveResult>? MoveCeilingDown { get; init; }
        public Func<VmACeilingCreate.CeilingSector, bool>? IsMakingLoopingSound { get; init; }
        public Action<VmACeilingCreate.CeilingSector>? StopSequence { get; init; }
        public Action<VmACeilingCreate.CeilingSector, VmACeilingCreate.SectorSpecial>? SetSpecial { get; init; }
        public Action<VmACeilingCreate.CeilingSector, string>? SetCeilingTexture { get; init; }
        public Action<CeilingTickThinker>? Destroy { get; init; }
    }

    public sealed class EvCeilingContext
    {
        public VmACeilingCreate.CreateCeilingContext Create { get; init; } = new();
        public SoundContext Sound { get; init; } = new();
        public Func<IEnumerable<CeilingTickThinker>>? CeilingThinkerIterator { get; init; }
        public Func<int, IEnumerable<int>>? SectorTagIterator { get; init; }
        public Func<int, VmACeilingCreate.CeilingSector?>? SectorByIndex { get; init; }
        public Func<VmACeilingCreate.CeilingSector, CeilingTickThinker?>? GetCeilingThinker { get; init; }
        public Action<VmACeilingCreate.CeilingSector>? StopSequence { get; init; }
        public Action<CeilingTickThinker>? DestroyThinker { get; init; }
        public Func<VmACeilingCreate.CeilingLine?, VmACeilingCreate.CeilingSector?>? ManualBackSector { get; init; }
        public Func<int, VmACeilingCreate.CeilingLine?, IEnumerable<int>>? SectorTagIteratorWithLine { get; init; }
    }

    public static void PlayCeilingSound(CeilingTickThinker ceiling, SoundContext ctx) =>
        PlayCeilingSoundSector(ceiling.Sector, ceiling.Silent, ctx);

    public static void PlayCeilingSoundSector(
        VmACeilingCreate.CeilingSector sec,
        int silent,
        SoundContext ctx)
    {
        if ((sec.Flags & VmACeilingCreate.SecfSilentmove) != 0)
            return;
        if (sec.SeqType >= 0)
        {
            ctx.StartSequenceType?.Invoke(sec, 0, sec.SeqType, 3, 0, false);
            return;
        }
        if (sec.SeqName.Length > 0)
        {
            ctx.StartSequenceName?.Invoke(sec, 0, sec.SeqName, 0);
            return;
        }
        if (silent == 2)
            ctx.StartSequenceSound?.Invoke(sec, 0, "Silence", 0);
        else if (silent == 1)
            ctx.StartSequenceSound?.Invoke(sec, 0, "CeilingSemiSilent", 0);
        else
            ctx.StartSequenceSound?.Invoke(sec, 0, "CeilingNormal", 0);
    }

    public static void Tick(CeilingTickThinker ceiling, TickContext ctx)
    {
        var sec = ceiling.Sector;
        switch (ceiling.Direction)
        {
            case 0:
                break;
            case 1:
            {
                var res = ctx.MoveCeilingUp?.Invoke(sec, ceiling.Speed, ceiling.TopHeight, ceiling.Direction)
                    ?? VmADoorTickSound.MoveResult.Ok;
                if (res == VmADoorTickSound.MoveResult.PastDest)
                {
                    switch (ceiling.Type)
                    {
                        case VmACeilingCreate.CeilCrushAndRaise:
                            ceiling.Direction = -1;
                            ceiling.Speed = ceiling.Speed1;
                            if (ctx.IsMakingLoopingSound?.Invoke(sec) != true)
                                PlayCeilingSound(ceiling, ctx.Sound);
                            break;
                        case VmACeilingCreate.GenCeilingChgT:
                        case VmACeilingCreate.GenCeilingChg0:
                            ctx.SetSpecial?.Invoke(sec, ceiling.NewSpecial);
                            goto case VmACeilingCreate.GenCeilingChg;
                        case VmACeilingCreate.GenCeilingChg:
                            ctx.SetCeilingTexture?.Invoke(sec, ceiling.Texture);
                            goto default;
                        default:
                            ctx.StopSequence?.Invoke(sec);
                            ceiling.Destroyed = true;
                            ctx.Destroy?.Invoke(ceiling);
                            break;
                    }
                }
                break;
            }
            case -1:
            {
                var hexen = ceiling.CrushMode == VmACeilingCreate.CrushHexen;
                var res = ctx.MoveCeilingDown?.Invoke(sec, ceiling.Speed, ceiling.BottomHeight, ceiling.Crush, ceiling.Direction, hexen)
                    ?? VmADoorTickSound.MoveResult.Ok;
                if (res == VmADoorTickSound.MoveResult.PastDest)
                {
                    switch (ceiling.Type)
                    {
                        case VmACeilingCreate.CeilCrushAndRaise:
                        case VmACeilingCreate.CeilCrushRaiseAndStay:
                            ceiling.Speed = ceiling.Speed2;
                            ceiling.Direction = 1;
                            if (ctx.IsMakingLoopingSound?.Invoke(sec) != true)
                                PlayCeilingSound(ceiling, ctx.Sound);
                            break;
                        case VmACeilingCreate.GenCeilingChgT:
                        case VmACeilingCreate.GenCeilingChg0:
                            ctx.SetSpecial?.Invoke(sec, ceiling.NewSpecial);
                            goto case VmACeilingCreate.GenCeilingChg;
                        case VmACeilingCreate.GenCeilingChg:
                            ctx.SetCeilingTexture?.Invoke(sec, ceiling.Texture);
                            goto default;
                        default:
                            ctx.StopSequence?.Invoke(sec);
                            ceiling.Destroyed = true;
                            ctx.Destroy?.Invoke(ceiling);
                            break;
                    }
                }
                else if (res == VmADoorTickSound.MoveResult.Crushed)
                {
                    switch (ceiling.Type)
                    {
                        case VmACeilingCreate.CeilCrushAndRaise:
                        case VmACeilingCreate.CeilLowerAndCrush:
                            if (ceiling.CrushMode == VmACeilingCreate.CrushSlowdown)
                                ceiling.Speed = 1.0 / 8;
                            break;
                    }
                }
                break;
            }
        }
    }

    public static bool EvDoCeiling(
        EvCeilingContext ctx,
        int type,
        VmACeilingCreate.CeilingLine? line,
        int tag,
        double speed,
        double speed2,
        double height,
        int crush,
        int silent,
        int change,
        int hexencrush)
    {
        var rtn = false;
        if (tag == 0)
        {
            var sec = ctx.ManualBackSector?.Invoke(line);
            if (sec == null)
                return false;
            tag ^= sec.SectorNum | 0x1000000;
            rtn |= ActivateInStasisCeiling(ctx, tag);
            return VmACeilingCreate.CreateCeiling(ctx.Create, sec, type, line, tag, speed, speed2, height, crush, silent, change, hexencrush);
        }

        if (type == VmACeilingCreate.CeilCrushAndRaise)
            rtn |= ActivateInStasisCeiling(ctx, tag);

        foreach (var secnum in ctx.SectorTagIterator?.Invoke(tag) ?? Array.Empty<int>())
        {
            var sec = ctx.SectorByIndex?.Invoke(secnum);
            if (sec == null)
                continue;
            if (VmACeilingCreate.CreateCeiling(ctx.Create, sec, type, line, tag, speed, speed2, height, crush, silent, change, hexencrush))
                rtn = true;
        }

        return rtn;
    }

    public static bool ActivateInStasisCeiling(EvCeilingContext ctx, int tag)
    {
        var rtn = false;
        foreach (var scan in ctx.CeilingThinkerIterator?.Invoke() ?? Array.Empty<CeilingTickThinker>())
        {
            if (scan.Tag == tag && scan.Direction == 0)
            {
                scan.Direction = scan.OldDirection;
                PlayCeilingSound(scan, ctx.Sound);
                rtn = true;
            }
        }
        return rtn;
    }

    public static bool EvCeilingCrushStop(EvCeilingContext ctx, int tag, bool remove)
    {
        var rtn = false;
        var list = (ctx.CeilingThinkerIterator?.Invoke() ?? Array.Empty<CeilingTickThinker>()).ToList();
        for (var i = 0; i < list.Count; i++)
        {
            var scan = list[i];
            if (scan.Tag == tag && scan.Direction != 0)
            {
                if (!remove)
                {
                    ctx.StopSequence?.Invoke(scan.Sector);
                    scan.OldDirection = scan.Direction;
                    scan.Direction = 0;
                }
                else
                {
                    ctx.DestroyThinker?.Invoke(scan);
                }
                rtn = true;
            }
        }
        return rtn;
    }

    public static bool EvStopCeiling(EvCeilingContext ctx, int tag, VmACeilingCreate.CeilingLine? line)
    {
        foreach (var secnum in ctx.SectorTagIteratorWithLine?.Invoke(tag, line) ?? Array.Empty<int>())
        {
            var sec = ctx.SectorByIndex?.Invoke(secnum);
            if (sec == null)
                continue;
            var thinker = ctx.GetCeilingThinker?.Invoke(sec);
            if (thinker != null)
            {
                ctx.StopSequence?.Invoke(sec);
                ctx.DestroyThinker?.Invoke(thinker);
            }
        }
        return true;
    }
}
