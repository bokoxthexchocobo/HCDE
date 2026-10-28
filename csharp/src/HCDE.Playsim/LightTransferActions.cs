using HCDE.MapLoader;

namespace HCDE.Playsim;

internal sealed class LightTransfer
{
    internal int SourceSector { get; init; }
    internal int TargetTag { get; init; }
    internal bool CopyFloor { get; init; }
    internal short LastLight { get; set; }
}

/// <summary>Native <c>DLightTransfer</c> from <c>a_lighttransfer.cpp</c>.</summary>
internal static class LightTransferActions
{
    internal static void Add(AuthoritySimulation sim, int sourceSector, int targetTag, bool copyFloor)
    {
        var transfer = new LightTransfer
        {
            SourceSector = sourceSector,
            TargetTag = targetTag,
            CopyFloor = copyFloor,
            LastLight = sim.LightOf(sourceSector),
        };
        Apply(sim, transfer, transfer.LastLight);
        sim.LightTransfers.Add(transfer);
    }

    internal static void Tick(AuthoritySimulation sim)
    {
        foreach (var transfer in sim.LightTransfers)
        {
            var light = sim.LightOf(transfer.SourceSector);
            if (light == transfer.LastLight) continue;
            transfer.LastLight = light;
            Apply(sim, transfer, light);
        }
    }

    private static void Apply(AuthoritySimulation sim, LightTransfer transfer, short light)
    {
        foreach (var sector in sim.Level.Sectors)
        {
            if (!sector.MatchesTag(transfer.TargetTag)) continue;
            sim.HasLightOverride = true;
            sim.Lights[sector.Index] = light;
        }
    }
}
