namespace HCDE.Net.Core.Tests;

public class SectorWorldDeltaMetadataTests
{
    [Fact]
    public void WriteAndRead_RoundTripsLightAndSpecial()
    {
        var sector = new SectorWorldDelta(
            sectorIndex: 4,
            flags: LiveConstants.ServerWorldDeltaSectorHasLight | LiveConstants.ServerWorldDeltaSectorHasSpecial,
            floor: 8,
            ceiling: 192,
            lightLevel: 160,
            special: 3);

        Span<byte> buffer = stackalloc byte[32];
        var cursor = 0;
        Assert.Equal(15, WorldDeltaPoseCodec.WriteSector(buffer, ref cursor, sector));

        cursor = 0;
        Assert.True(WorldDeltaPoseCodec.TryReadSector(buffer, ref cursor, out var parsed));
        Assert.Equal(sector.SectorIndex, parsed.SectorIndex);
        Assert.Equal(sector.Flags, parsed.Flags);
        Assert.Equal(sector.Floor, parsed.Floor);
        Assert.Equal(sector.Ceiling, parsed.Ceiling);
        Assert.Equal(160, parsed.LightLevel);
        Assert.Equal(3, parsed.Special);
    }

    [Fact]
    public void ApplySector_UpdatesLightAndSpecialWhenFlagged()
    {
        var store = new GuestWorldStateStore();
        var sector = new SectorWorldDelta(
            sectorIndex: 1,
            flags: LiveConstants.ServerWorldDeltaSectorHasLight | LiveConstants.ServerWorldDeltaSectorHasSpecial,
            floor: 0,
            ceiling: 128,
            lightLevel: 144,
            special: 7);

        Assert.True(store.ApplySector(sector));
        Assert.True(store.Sectors.TryGetValue(1, out var state));
        Assert.Equal(144, state.LightLevel);
        Assert.Equal(7, state.Special);
    }
}
