namespace HCDE.Client;

/// <summary>Static time helpers from <c>i_time.cpp</c> (nanoseconds / tics).</summary>
public static class VmTimeUnitConversions
{
    public static ulong MsToNs(uint ms) => (ulong)ms * 1_000_000;

    public static ulong NsToMs(ulong ns) => ns / 1_000_000;

    public static int NsToTic(ulong ns, double ticRate) =>
        (int)(ns * ticRate / 1_000_000_000);

    public static ulong TicToNs(double tic, double ticRate) =>
        (ulong)(tic * 1_000_000_000 / ticRate);
}
