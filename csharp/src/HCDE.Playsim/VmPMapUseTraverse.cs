namespace HCDE.Playsim;

/// <summary><c>P_UseTraverse</c> and <c>P_NoWayTraverse</c> from <c>p_map.cpp</c> (5829–5993).</summary>
public static class VmPMapUseTraverse
{
    public const int Mf5Usespecial = 1;
    public const int MlBlockeverything = 1;
    public const int MlBlockuse = 1 << 1;
    public const int SpacUse = 1;
    public const int SpacUseThrough = 1 << 1;
    public const int SpacUseBack = 1 << 2;
    public const int CompatfUseblocking = 1;
    public const int SecspacUse = 1;
    public const int SecspacUsewall = 1 << 1;

    public sealed class Intercept
    {
        public bool IsLine { get; init; }
        public object? Thing { get; init; }
        public LineSample? Line { get; init; }
    }

    public sealed class LineSample
    {
        public int Special { get; init; }
        public int Activation { get; init; }
        public int Flags { get; init; }
        public object? FrontSector { get; init; }
        public object? BackSector { get; init; }
        public object? SecActTarget { get; init; }
    }

    public sealed class UseTraverseState
    {
        public required object Usething { get; init; }
        public bool FoundLine { get; set; }
        public int CompatFlags { get; init; }
        public int UsePuzzleItemSpecial { get; init; }
        public Func<Intercept?>? NextIntercept { get; init; }
        public Func<Intercept, bool>? PortalRelocate { get; init; }
        public Func<LineSample, double, double, double, double>? LineOpeningRange { get; init; }
        public Func<object, object, bool>? ActivateThingSpecial { get; init; }
        public Func<object, object, int>? VirtualUsed { get; init; }
        public Func<object, object, int, bool>? TriggerSectorActions { get; init; }
        public Func<object, object, int, bool>? ActivateLine { get; init; }
        public Func<double, double, LineSample?, int>? PointOnLineSide { get; init; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public object? UsethingSector { get; init; }
        public bool HasPlayer { get; init; }
        public Action? PlayUseFail { get; init; }
    }

    public static bool UseTraverse(UseTraverseState s)
    {
        while (s.NextIntercept?.Invoke() is { } hit)
        {
            if (!hit.IsLine)
            {
                if (ReferenceEquals(hit.Thing, s.Usething))
                    continue;
                if (ThingUseSpecial(s, hit.Thing))
                    return true;
                if (s.VirtualUsed?.Invoke(hit.Thing!, s.Usething) != 0)
                    return true;
                continue;
            }

            if (s.PortalRelocate?.Invoke(hit) == true)
                continue;

            var line = GetLine(hit);
            if (line.Special == 0 || (line.Activation & (SpacUse | SpacUseThrough | SpacUseBack)) == 0)
            {
                if (BlockedUse(s, hit, line))
                    return true;
                s.FoundLine = true;
                continue;
            }

            var side = s.PointOnLineSide?.Invoke(s.PosX, s.PosY, hit.Line) ?? 0;
            if (side == 1)
            {
                if ((line.Activation & SpacUseBack) == 0)
                {
                    if (BlockedUse(s, hit, line))
                        return true;
                    continue;
                }

                s.ActivateLine?.Invoke(hit.Line!, s.Usething, SpacUseBack);
                return true;
            }

            if ((line.Activation & (SpacUse | SpacUseThrough | SpacUseBack)) == SpacUseBack)
            {
                if (BlockedUse(s, hit, line))
                    return true;
                continue;
            }

            s.ActivateLine?.Invoke(hit.Line!, s.Usething, SpacUse);

            if ((s.CompatFlags & CompatfUseblocking) != 0)
            {
                if ((line.Activation & SpacUseThrough) != 0)
                    continue;
                return true;
            }

            if ((line.Activation & SpacUse) == 0)
                continue;
            return true;
        }

        return false;
    }

    private static bool ThingUseSpecial(UseTraverseState s, object? mobj)
    {
        if (mobj == null)
            return false;
        return s.ActivateThingSpecial?.Invoke(mobj, s.Usething) == true;
    }

    private static LineSample GetLine(Intercept hit) => hit.Line ?? new LineSample();

    private static bool BlockedUse(UseTraverseState s, Intercept hit, LineSample line)
    {
        var range = s.LineOpeningRange?.Invoke(line, s.PosX, s.PosY, s.PosZ) ?? 0;
        if (range <= 0 || (line.Special != 0 && (s.CompatFlags & CompatfUseblocking) != 0))
        {
            if (s.TriggerSectorActions?.Invoke(s.UsethingSector!, s.Usething, SecspacUse) == true)
                return true;
            var sec = line.FrontSector;
            if (sec != null && s.TriggerSectorActions?.Invoke(sec, s.Usething, SecspacUsewall) == true)
                return true;
            if (s.HasPlayer)
                s.PlayUseFail?.Invoke();
            return true;
        }

        return false;
    }

    public sealed class NoWayState
    {
        public double ActorZ { get; init; }
        public double ActorTop { get; init; }
        public double MaxStepHeight { get; init; }
        public Func<Intercept?>? NextLine { get; init; }
        public Func<LineSample, double, double, double, double>? LineOpening { get; init; }
    }

    public static bool NoWayTraverse(NoWayState s)
    {
        while (s.NextLine?.Invoke() is { } hit)
        {
            var line = GetLine(hit);
            if (line.Special != 0)
                continue;
            if (line.Flags != 0 && IsPortalLine(line, null))
                return false;
            if ((line.Flags & (MlBlockeverything | MlBlockuse)) != 0)
                return true;
            var open = s.LineOpening?.Invoke(line, 0, 0, 0) ?? 0;
            if (open <= 0 || open > s.ActorZ + s.MaxStepHeight || open < s.ActorTop)
                return true;
        }

        return false;
    }

    public static bool IsPortalLine(LineSample line, Func<LineSample, bool>? detector) =>
        detector?.Invoke(line) ?? line.Flags != 0;
}
