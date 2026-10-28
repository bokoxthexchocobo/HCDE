namespace HCDE.Playsim;

/// <summary><c>DPlaneWatcher::Construct</c> / <c>Tick</c> from <c>p_acs.cpp</c> (1899–1971).</summary>
public sealed class VmAcsPlaneWatcher
{
    public int Special { get; private set; }
    public bool Ceiling { get; private set; }
    public double LastD { get; private set; }
    public double WatchD { get; private set; }
    public int[] Args { get; } = new int[5];
    public bool Destroyed { get; private set; }

    public static VmAcsPlaneWatcher Construct(
        bool ceiling,
        double planeD,
        int heightDelta,
        int special,
        int arg0,
        int arg1,
        int arg2,
        int arg3,
        int arg4)
    {
        var watcher = new VmAcsPlaneWatcher
        {
            Ceiling = ceiling,
            Special = special,
            LastD = planeD,
            WatchD = planeD + heightDelta,
        };
        watcher.Args[0] = arg0;
        watcher.Args[1] = arg1;
        watcher.Args[2] = arg2;
        watcher.Args[3] = arg3;
        watcher.Args[4] = arg4;
        return watcher;
    }

    public static bool PlaneCrossedWatch(double lastD, double watchD, double newD) =>
        (lastD < watchD && newD >= watchD) || (lastD > watchD && newD <= watchD);

    public bool Tick(double? currentPlaneD, Func<int, int, int, int, int, int, bool>? executeSpecial)
    {
        if (currentPlaneD is null)
        {
            Destroyed = true;
            return false;
        }

        var newD = currentPlaneD.Value;
        if (PlaneCrossedWatch(LastD, WatchD, newD) && executeSpecial is not null)
        {
            executeSpecial(Special, Args[0], Args[1], Args[2], Args[3], Args[4]);
            Destroyed = true;
            return true;
        }

        return false;
    }
}
