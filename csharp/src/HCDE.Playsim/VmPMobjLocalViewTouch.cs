namespace HCDE.Playsim;

/// <summary>Local view, visibility, conversation anim, and touch VM gate from <c>p_mobj.cpp</c> (1321–1542).</summary>
public static class VmPMobjLocalViewTouch
{
    public const int RfMaskrotation = 1;
    public const int RfAbsmaskangle = 1 << 1;
    public const int RfAbsmaskpitch = 1 << 2;
    public const int Mf3Ismonster = 1;

    public sealed class LocalViewQuery
    {
        public object? ConsoleCamera { get; init; }
        public object? PlayerMo { get; init; }
        public object? Camera { get; init; }
        public bool CameraHasPlayer { get; init; }
        public int CameraFlags3 { get; init; }
    }

    public static bool CheckLocalView(LocalViewQuery q, object self)
    {
        if (ReferenceEquals(q.ConsoleCamera, self))
            return true;
        if (!ReferenceEquals(q.PlayerMo, self) || q.Camera == null)
            return false;
        if (!q.CameraHasPlayer && (q.CameraFlags3 & Mf3Ismonster) == 0)
            return true;
        return false;
    }

    public static void DisableLocalRendering(int consolePlayer, int targetPlayer, bool disable, Action<bool>? setNoLocalRender)
    {
        if (targetPlayer == consolePlayer)
            setNoLocalRender?.Invoke(disable);
    }

    public static bool ShouldRenderLocally(bool noLocalRender) => !noLocalRender;

    public sealed class VisibleAnglesQuery
    {
        public int RenderFlags { get; init; }
        public double VisibleStartAngle { get; init; }
        public double VisibleEndAngle { get; init; }
        public double VisibleStartPitch { get; init; }
        public double VisibleEndPitch { get; init; }
        public object? Camera { get; init; }
        public Func<double, double, double, double, double, double, double>? DeltaToView { get; init; }
        public Func<double, double, double>? DeltaAngle { get; init; }
        public double ActorYaw { get; init; }
        public double ActorPitch { get; init; }
    }

    public static bool IsInsideVisibleAngles(VisibleAnglesQuery q)
    {
        if ((q.RenderFlags & RfMaskrotation) == 0)
            return true;
        if (q.Camera == null)
            return true;

        var angleStart = q.VisibleStartAngle;
        var angleEnd = q.VisibleEndAngle;
        if (angleStart > angleEnd)
            (angleStart, angleEnd) = (angleEnd, angleStart);

        var pitchStart = q.VisibleStartPitch;
        var pitchEnd = q.VisibleEndPitch;
        if (pitchStart > pitchEnd)
            (pitchStart, pitchEnd) = (pitchEnd, pitchStart);

        var to = q.DeltaToView?.Invoke(angleStart, angleEnd, pitchStart, pitchEnd, q.ActorYaw, q.ActorPitch) ?? 0;
        if (to < angleStart || to > angleEnd)
            return false;

        var pitch = q.DeltaAngle?.Invoke(q.ActorPitch, to) ?? to;
        return pitch >= pitchStart && pitch <= pitchEnd;
    }

    public sealed class VisibilityQuery
    {
        public bool NoLocalRender { get; init; }
        public int VisibleToTeam { get; init; }
        public bool Teamplay { get; init; }
        public int ConsoleTeam { get; init; }
        public IReadOnlyList<int>? VisibleToPlayerClassIds { get; init; }
        public Func<int, bool>? ClassIsDescendant { get; init; }
        public int CameraPlayerClassId { get; init; }
        public bool CameraHasPlayer { get; init; }
    }

    public static bool IsVisibleToPlayer(VisibilityQuery q)
    {
        if (q.NoLocalRender)
            return false;
        if (q.VisibleToTeam != 0 && q.Teamplay && q.VisibleToTeam - 1 != q.ConsoleTeam)
            return false;
        var vis = q.VisibleToPlayerClassIds;
        if (vis == null || vis.Count == 0)
            return true;
        if (!q.CameraHasPlayer)
            return true;
        foreach (var cls in vis)
        {
            if (cls != 0 && q.ClassIsDescendant?.Invoke(cls) == true)
                return true;
        }

        return false;
    }

    public sealed class ConversationContext
    {
        public Func<int, object?>? FindState { get; init; }
        public Action<object?>? SetState { get; init; }
    }

    public static void ConversationAnimation(ConversationContext ctx, int animNum)
    {
        object? state = animNum switch
        {
            0 => ctx.FindState?.Invoke(0),
            1 => ctx.FindState?.Invoke(1),
            2 => ctx.FindState?.Invoke(2),
            _ => null,
        };
        if (state != null)
            ctx.SetState?.Invoke(state);
    }

    public static void CallTouch(bool hasVirtual, Action? vmCall)
    {
        if (hasVirtual)
            vmCall?.Invoke();
    }
}
