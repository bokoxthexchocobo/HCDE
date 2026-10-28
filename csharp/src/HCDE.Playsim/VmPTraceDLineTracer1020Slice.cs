namespace HCDE.Playsim;

/// <summary>
/// <c>DLineTracer</c> ZScript exports, <c>Trace</c> action flag hygiene, and wall-texture callback patching from
/// <c>p_trace.cpp</c> (**1027–1132**). <c>EditTraceResult</c> (**983–1025**) stays on <see cref="VmPTraceThingPlaneEdit"/>.
/// </summary>
public static class VmPTraceDLineTracer1020Slice
{
    public const int TracePcross = 0x0002;
    public const int TraceImpact = 0x0004;
    public const int Trace3DCallback = 0x0020;

    public const int TraceHitWall = 3;
    public const int TraceStop = 0;

    public const int TierMiddle = 0;
    public const int TierUpper = 1;
    public const int TierLower = 2;
    public const int TierFfloor = 3;

    public const int FfUpperTexture = 0x20000;
    public const int FfLowerTexture = 0x40000;

    public static IReadOnlyList<string> TraceResultsVmFieldNames { get; } = new[]
    {
        "HitSector", "HitTexture", "HitPos", "HitVector", "SrcFromTarget", "SrcAngleFromTarget",
        "Distance", "Fraction", "HitActor", "HitLine", "Side", "Tier", "unlinked", "HitType",
        "ffloor", "CrossedWater", "CrossedWaterPos", "Crossed3DWater", "Crossed3DWaterPos",
    };

    public static int TraceResultsVmFieldCount => TraceResultsVmFieldNames.Count;

    public static bool TryGetTraceResultsVmFieldName(int index, out string name)
    {
        if (index < 0 || index >= TraceResultsVmFieldNames.Count)
        {
            name = "";
            return false;
        }

        name = TraceResultsVmFieldNames[index];
        return true;
    }

    public static int SanitizeZScriptTraceFlags(int traceFlags)
    {
        traceFlags &= ~(TracePcross | TraceImpact);
        return traceFlags | Trace3DCallback;
    }

    public static uint ActorMaskForIgnoreAll(bool ignoreAllActors) =>
        ignoreAllActors ? 0u : 0xFFFFFFFFu;

    public readonly record struct WallTexturePatchRequest(
        int HitType,
        int Tier,
        int SideMiddleTexture,
        int SideUpperTexture,
        int SideLowerTexture,
        int FfloorFlags,
        int FfloorMasterMiddleTexture,
        int FfloorMasterUpperTexture,
        int FfloorMasterLowerTexture);

    public static bool TryPatchWallHitTexture(WallTexturePatchRequest req, out int hitTexture)
    {
        hitTexture = 0;
        if (req.HitType != TraceHitWall)
            return false;

        hitTexture = req.Tier switch
        {
            TierMiddle => req.SideMiddleTexture,
            TierUpper => req.SideUpperTexture,
            TierLower => req.SideLowerTexture,
            TierFfloor => SelectFfloorTexturePart(req),
            _ => hitTexture,
        };
        return true;
    }

    public static int SelectFfloorTexturePart(WallTexturePatchRequest req)
    {
        if ((req.FfloorFlags & FfUpperTexture) != 0)
            return req.FfloorMasterUpperTexture;
        if ((req.FfloorFlags & FfLowerTexture) != 0)
            return req.FfloorMasterLowerTexture;
        return req.FfloorMasterMiddleTexture;
    }

    public static int ResolveTraceCallbackStatus(bool hasVirtual, Func<int>? invokeVirtual)
    {
        if (!hasVirtual)
            return TraceStop;
        return invokeVirtual?.Invoke() ?? TraceStop;
    }
}
