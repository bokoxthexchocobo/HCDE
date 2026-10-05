using System.Buffers.Binary;
using HCDE.MapLoader;
using HCDE.MapLoader.Tests;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class StrifeMapBootTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void ServerOptionReachesSpawnedActor(bool strife, bool expected)
    {
        using var host = new DedicatedServerHost(new DedicatedServerOptions
        {
            Port = 0, IwadBytes = AllyWad(),
            ThingFlagFormat = strife ? BinaryThingFlagFormat.Strife : BinaryThingFlagFormat.Doom,
        });
        var sim = Assert.IsType<AuthoritySimulation>(host.Simulation);
        Assert.Equal(expected, sim.Actors.Any(actor => actor.DoomEdNum == 3004 && actor.Friendly));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HeadlessBootPropagatesFlagSelection(bool strife)
    {
        Assert.True(HeadlessMapBoot.TryBoot(AllyWad(), "MAP01", out var sim, out var error,
            thingFlagFormat: strife ? BinaryThingFlagFormat.Strife : BinaryThingFlagFormat.Doom), error);
        Assert.Equal(strife, Assert.Single(sim!.Actors).Friendly);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void CommandLineFlagReachesServerStartup(int count)
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, AllyWad());
            var args = new List<string> { "--iwad", path };
            for (var i = 0; i < count; i++) args.Add("--strife-thing-flags");
            Assert.True(DedicatedServerCommandLine.TryParse(args.ToArray(), out var options, out var error), error);
            Assert.Equal(count == 0 ? BinaryThingFlagFormat.Doom : BinaryThingFlagFormat.Strife, options.ThingFlagFormat);
            options.Port = 0;
            using var host = new DedicatedServerHost(options);
            Assert.Equal(count != 0, host.Simulation!.Actors.Any(actor => actor.Friendly));
        }
        finally { File.Delete(path); }
    }

    private static byte[] AllyWad()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(WadArchiveReader.TryReadDirectory(wad, out var entries, out _));
        var thing = entries.Single(entry => entry.Name == "THINGS");
        BinaryPrimitives.WriteInt16LittleEndian(wad.AsSpan((int)thing.FilePosition + 6), 3004);
        BinaryPrimitives.WriteUInt16LittleEndian(wad.AsSpan((int)thing.FilePosition + 8), 0x47);
        return wad;
    }
}
