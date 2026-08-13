using HCDE.Net.Transport;

namespace HCDE.Net.Core;

public interface IWorldDeltaApplySink
{
    bool ApplyPose(int recipientClientSlot, PlayerPoseWorldDelta pose, int sequenceAck);

    bool ApplySector(SectorWorldDelta sector);
}

public readonly struct WorldDeltaApplyResult
{
    public WorldDeltaApplyResult(int posesReceived, int posesApplied, int sectorsApplied)
    {
        PosesReceived = posesReceived;
        PosesApplied = posesApplied;
        SectorsApplied = sectorsApplied;
    }

    public int PosesReceived { get; }
    public int PosesApplied { get; }
    public int SectorsApplied { get; }
}

public static class WorldDeltaApplySession
{
    private const byte ValidPoseFlags =
        LiveConstants.ServerWorldDeltaPoseHasActor
        | LiveConstants.ServerWorldDeltaPoseLive
        | LiveConstants.ServerWorldDeltaPoseOnGround;

    public static bool TryApply(
        ServerWorldDeltaHeader header,
        IReadOnlyList<PlayerPoseWorldDelta> poses,
        IReadOnlyList<SectorWorldDelta> sectors,
        ulong snapshotPlayersMask,
        int recipientClientSlot,
        int sequenceAck,
        IWorldDeltaApplySink? sink,
        out WorldDeltaApplyResult result,
        out string? rejectReason)
    {
        result = default;
        rejectReason = null;

        if (header.RecordCount != poses.Count)
        {
            rejectReason = "world-delta-count-mismatch";
            return false;
        }

        if (header.RecordCount > NetConstants.MaxPlayers)
        {
            rejectReason = "world-delta-player-count-overflow";
            return false;
        }

        var playersSeen = 0UL;
        var posesApplied = 0;
        foreach (var pose in poses)
        {
            if (pose.PlayerNum >= NetConstants.MaxPlayers)
            {
                rejectReason = "world-delta-invalid-player";
                return false;
            }

            if ((pose.Flags & ~ValidPoseFlags) != 0)
            {
                rejectReason = "world-delta-invalid-pose-flags";
                return false;
            }

            var playerMask = 1UL << pose.PlayerNum;
            if ((playersSeen & playerMask) != 0)
            {
                rejectReason = "world-delta-duplicate-player";
                return false;
            }

            playersSeen |= playerMask;

            if ((pose.Flags & LiveConstants.ServerWorldDeltaPoseHasActor) == 0)
                continue;

            if (sink != null && sink.ApplyPose(recipientClientSlot, pose, sequenceAck))
                posesApplied++;
        }

        var sectorsApplied = 0;
        if (sink != null)
        {
            foreach (var sector in sectors)
            {
                if (sink.ApplySector(sector))
                    sectorsApplied++;
            }
        }

        _ = snapshotPlayersMask;
        result = new WorldDeltaApplyResult(poses.Count, posesApplied, sectorsApplied);
        return true;
    }
}
