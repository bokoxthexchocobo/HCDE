namespace HCDE.Playsim;

/// <summary><c>DGlow2</c> from <c>a_lights.cpp</c> (411–447).</summary>
public static class VmALightsGlow2
{
    public static int InterpolateLight(int start, int end, int tics, int maxTics) =>
        maxTics == 0 ? end : ((end - start) * tics) / maxTics + start;

    public static (bool destroy, int light, int tics, int start, int end) Tick(
        int tics,
        int maxTics,
        int start,
        int end,
        bool oneShot)
    {
        var nextTics = tics + 1;
        if (nextTics < maxTics)
            return (false, InterpolateLight(start, end, nextTics, maxTics), nextTics, start, end);

        if (oneShot)
            return (true, end, nextTics, start, end);

        var swappedStart = end;
        var swappedEnd = start;
        var wrappedTics = nextTics - maxTics;
        return (false, InterpolateLight(swappedStart, swappedEnd, wrappedTics, maxTics), wrappedTics, swappedStart, swappedEnd);
    }

    public static (int start, int end, int maxTics, int tics) Construct(
        int requestedStart,
        int requestedEnd,
        int tics) =>
        (VmALightsFireFlicker.ClampLight(requestedStart), VmALightsFireFlicker.ClampLight(requestedEnd), tics, -1);
}
