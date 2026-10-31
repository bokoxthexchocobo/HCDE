namespace HCDE.Playsim;

/// <summary>
/// Local rendering disable, visible angle/pitch masks, and player visibility from
/// <c>p_mobj.cpp</c> (**1351–1500**). Chunk **1108**.
/// </summary>
public static class VmPMobjVisibility1108Slice
{
    public const int RfMaskRotation = 0x2000000;
    public const int RfAbsMaskAngle = 0x4000000;
    public const int RfAbsMaskPitch = 0x8000000;

    public static void RunDisableLocalRendering(
        int consolePlayer,
        int targetPlayer,
        bool disable,
        Action<bool>? assignNoLocalRender)
    {
        if (targetPlayer == consolePlayer)
            assignNoLocalRender?.Invoke(disable);
    }

    public static bool ShouldRenderLocally(bool noLocalRender) => !noLocalRender;

    public sealed class VisibleAnglesContext
    {
        public int RenderFlags { get; init; }
        public bool HasConsoleCamera { get; init; }
        public float VisibleStartAngle { get; init; }
        public float VisibleEndAngle { get; init; }
        public float VisibleStartPitch { get; init; }
        public float VisibleEndPitch { get; init; }
        public float ActorYaw { get; init; }
        public float ActorPitch { get; init; }
        public float DeltaAngle { get; init; }
        public float DeltaPitch { get; init; }
        public Func<float, float, float>? NormalizeDeltaAngle { get; init; }
    }

    public static bool IsInsideVisibleAngles(VisibleAnglesContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if ((ctx.RenderFlags & RfMaskRotation) == 0)
            return true;
        if (!ctx.HasConsoleCamera)
            return true;

        var angleStart = ctx.VisibleStartAngle;
        var angleEnd = ctx.VisibleEndAngle;
        if (angleStart > angleEnd)
            (angleStart, angleEnd) = (angleEnd, angleStart);

        var pitchStart = ctx.VisibleStartPitch;
        var pitchEnd = ctx.VisibleEndPitch;
        if (pitchStart > pitchEnd)
            (pitchStart, pitchEnd) = (pitchEnd, pitchStart);

        var toAngle = ctx.DeltaAngle;
        if ((ctx.RenderFlags & RfAbsMaskAngle) == 0)
            toAngle = ctx.NormalizeDeltaAngle?.Invoke(ctx.ActorYaw, toAngle) ?? toAngle;

        if (toAngle < angleStart || toAngle > angleEnd)
            return false;

        var toPitch = ctx.DeltaPitch;
        if ((ctx.RenderFlags & RfAbsMaskPitch) == 0)
            toPitch = ctx.NormalizeDeltaAngle?.Invoke(ctx.ActorPitch, toPitch) ?? toPitch;

        return toPitch >= pitchStart && toPitch <= pitchEnd;
    }

    public sealed class VisibleToPlayerContext
    {
        public bool HasConsoleCamera { get; init; }
        public bool RendersLocally { get; init; }
        public int VisibleToTeam { get; init; }
        public bool TeamPlay { get; init; }
        public int PlayerTeamIndex { get; init; }
        public IReadOnlyList<Type?> VisibleToClasses { get; init; } = Array.Empty<Type?>();
        public Type? CameraPawnClass { get; init; }
        public Func<Type?, Type?, bool>? IsDescendant { get; init; }
        public bool CameraHasPlayer { get; init; }
    }

    public static bool IsVisibleToPlayer(VisibleToPlayerContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (!ctx.HasConsoleCamera)
            return true;
        if (!ctx.RendersLocally)
            return false;

        if (ctx.VisibleToTeam != 0 && ctx.TeamPlay
            && ctx.VisibleToTeam - 1 != ctx.PlayerTeamIndex)
        {
            return false;
        }

        if (ctx.VisibleToClasses.Count == 0)
            return true;

        if (!ctx.CameraHasPlayer || ctx.CameraPawnClass is null)
            return true;

        for (var i = 0; i < ctx.VisibleToClasses.Count; i++)
        {
            var cls = ctx.VisibleToClasses[i];
            if (cls is null)
                continue;
            if (ctx.IsDescendant?.Invoke(ctx.CameraPawnClass, cls) == true)
                return true;
        }

        return false;
    }
}
