namespace HCDE.Playsim;

/// <summary><c>P_UseLines</c>, <c>P_UsePuzzleItem</c> from <c>p_map.cpp</c> (6003–6100).</summary>
public static class VmPMapUseLinesPuzzle
{
    public const int BtUse = 1 << 0;
    public const int SecspacUse = 1;
    public const int SecspacUsewall = 1 << 1;

    public sealed class UseLinesPlayer
    {
        public required object Mo { get; init; }
        public object? Sector { get; init; }
        public object? SecActTarget { get; init; }
        public double UseRange { get; init; }
        public double Height { get; init; }
        public double YawDegrees { get; init; }
        public int CmdButtons { get; init; }
    }

    public sealed class UseLinesState
    {
        public required UseLinesPlayer Player { get; init; }
        public Func<double, (double X, double Y)>? PortalTransition { get; init; }
        public Func<UseLinesPlayer, bool, bool>? UseTraverse { get; init; }
        public Func<object, object, int, bool>? TriggerSectorActions { get; init; }
        public Func<object, (double X, double Y), (double X, double Y), bool>? NoWayTraverse { get; init; }
        public Action? PlayUseFail { get; init; }
        public Action<int, bool>? DiagTrace { get; init; }
    }

    public static void UseLines(UseLinesState s)
    {
        var foundline = false;
        var start = s.PortalTransition?.Invoke(s.Player.Height / 2) ?? (0d, 0d);
        var end = (
            start.X + Math.Cos(s.Player.YawDegrees * Math.PI / 180) * s.Player.UseRange,
            start.Y + Math.Sin(s.Player.YawDegrees * Math.PI / 180) * s.Player.UseRange);

        if (s.UseTraverse?.Invoke(s.Player, foundline) == true)
            return;

        var spac = SecspacUse;
        if (foundline)
            spac |= SecspacUsewall;

        if (s.Player.SecActTarget == null || s.TriggerSectorActions?.Invoke(s.Player.Sector!, s.Player.Mo, spac) != true)
        {
            if (s.NoWayTraverse?.Invoke(s.Player.Mo, start, end) == true)
            {
                s.PlayUseFail?.Invoke();
                s.DiagTrace?.Invoke(-1, false);
            }
        }
    }

    public static bool UseButtonPressed(int buttons, int btUse) => (buttons & btUse) != 0;

    public sealed class PuzzleItemState
    {
        public required object User { get; init; }
        public int PuzzleItemType { get; init; }
        public bool IsPlayer { get; init; }
        public double UseRange { get; init; }
        public int UsePuzzleItemLineSpecial { get; init; }
        public Func<Intercept?, Intercept?>? Next { get; init; }
        public Func<LineSample, double>? LineOpeningRange { get; init; }
        public Func<object, (double X, double Y), object, int>? PointOnLineSide { get; init; }
        public Action<object, object, int, int>? StartScript { get; init; }
    }

    public sealed class Intercept
    {
        public bool IsLine { get; init; }
        public object? Thing { get; init; }
        public LineSample? Line { get; init; }
    }

    public sealed class LineSample
    {
        public int Special { get; init; }
        public int[] Args { get; init; } = Array.Empty<int>();
    }

    public sealed class ThingSample
    {
        public int Special { get; init; }
        public int[] Args { get; init; } = Array.Empty<int>();
    }

    public static bool UsePuzzleItem(PuzzleItemState s)
    {
        while (s.Next?.Invoke(null) is { } hit)
        {
            if (hit.IsLine)
            {
                var line = hit.Line!;
                if (line.Special != s.UsePuzzleItemLineSpecial)
                {
                    if ((s.LineOpeningRange?.Invoke(line) ?? 0) <= 0)
                        return false;
                    continue;
                }

                if (s.PointOnLineSide?.Invoke(s.User, (0, 0), hit.Line!) == 1)
                    return false;
                if (s.PuzzleItemType != line.Args.ElementAtOrDefault(0))
                    return false;
                s.StartScript?.Invoke(s.User, hit.Line!, line.Args.ElementAtOrDefault(1), 3);
                return true;
            }

            if (hit.Thing is ThingSample thing)
            {
                if (thing.Special != s.UsePuzzleItemLineSpecial)
                    continue;
                if (s.PuzzleItemType != thing.Args.ElementAtOrDefault(0))
                    continue;
                s.StartScript?.Invoke(s.User, null!, thing.Args.ElementAtOrDefault(1), 3);
                return true;
            }
        }

        return false;
    }
}
