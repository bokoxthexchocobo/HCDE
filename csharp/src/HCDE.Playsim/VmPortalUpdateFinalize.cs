namespace HCDE.Playsim;

/// <summary>Portal update/finalize from <c>portal.cpp</c> (240–322).</summary>
public static class VmPortalUpdateFinalize
{
    public const int PorttVisual = 0;
    public const int PorttTeleport = 1;
    public const int PorttLinked = 2;
    public const int PorttInteractive = 3;
    public const int PortfPassable = 1;
    public const int PortfSoundTraverse = 2;
    public const int PortfInteractive = 4;

    public sealed class LinePortalState
    {
        public int Type { get; set; }
        public int Flags { get; set; }
        public int DefFlags { get; set; }
        public bool OriginHasBacksector { get; set; }
        public bool OriginPolyobj { get; set; }
        public int? DestinationLineIndex { get; set; }
        public int? DestinationPortalIndex { get; set; }
        public int DestinationReversePortalIndex { get; set; } = -1;
        public double DisplacementX { get; set; }
        public double DisplacementY { get; set; }
        public double DestV2X { get; set; }
        public double DestV2Y { get; set; }
        public double OriginV1X { get; set; }
        public double OriginV1Y { get; set; }
        public VmPortalBuildBlockmap.PortalRotationState Rotation { get; } = new();
    }

    public static void UpdatePortal(LinePortalState port, LinePortalState? destPortal, Action<string>? warn = null)
    {
        if (port.Type != PorttVisual && !port.OriginHasBacksector && !port.OriginPolyobj)
        {
            warn?.Invoke("visual-fallback");
            port.Type = PorttVisual;
            port.DefFlags &= ~(PortfPassable | PortfSoundTraverse | PortfInteractive);
        }

        if (port.DestinationLineIndex is null)
        {
            port.Flags = 0;
        }
        else if (port.DestinationReversePortalIndex < 0 ||
                 port.DestinationReversePortalIndex != port.DestinationLineIndex)
        {
            port.Flags = port.DefFlags & ~PortfInteractive;
            if (port.Type == PorttLinked)
            {
                port.Type = PorttTeleport;
                port.DefFlags &= ~PortfInteractive;
                warn?.Invoke("linked-demote");
            }
        }
        else
        {
            port.Flags = port.DefFlags;
            if (port.Type == PorttLinked && destPortal is not null)
            {
                if (destPortal.Type != PorttLinked)
                    port.Type = PorttInteractive;
                else
                {
                    port.DisplacementX = port.DestV2X - port.OriginV1X;
                    port.DisplacementY = port.DestV2Y - port.OriginV1Y;
                }
            }
        }

        VmPortalBuildBlockmap.SetPortalRotation(
            port.Type,
            port.DestinationLineIndex is not null,
            0,
            0,
            port.OriginPolyobj,
            false,
            port.Rotation);
    }

    public static List<int> CollectLinkedPortalIndices(IReadOnlyList<LinePortalState> linePortals)
    {
        var linked = new List<int>();
        for (var i = 0; i < linePortals.Count; i++)
        {
            if (linePortals[i].Type == PorttLinked)
                linked.Add(i);
        }
        return linked;
    }

    public sealed class FinalizePortalsContext
    {
        public IList<LinePortalState> LinePortals { get; set; } = new List<LinePortalState>();
        public Action? BuildPortalBlockmap { get; init; }
        public Action? CreateLinkedPortals { get; init; }
        public Func<int, LinePortalState?>? GetDestinationPortal { get; init; }
        public Func<int, int>? GetReverseOriginIndex { get; init; }
    }

    public static void FinalizePortals(FinalizePortalsContext ctx)
    {
        for (var i = 0; i < ctx.LinePortals.Count; i++)
        {
            var port = ctx.LinePortals[i];
            port.DestinationReversePortalIndex = port.DestinationLineIndex is int d
                ? ctx.GetReverseOriginIndex?.Invoke(d) ?? -1
                : -1;
            var dest = port.DestinationPortalIndex is int pi ? ctx.GetDestinationPortal?.Invoke(pi) : null;
            UpdatePortal(port, dest);
        }
        _ = CollectLinkedPortalIndices((IReadOnlyList<LinePortalState>)ctx.LinePortals);
        ctx.BuildPortalBlockmap?.Invoke();
        ctx.CreateLinkedPortals?.Invoke();
    }
}
