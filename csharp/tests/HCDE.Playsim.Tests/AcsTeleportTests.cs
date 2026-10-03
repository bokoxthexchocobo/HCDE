using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsTeleportTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void DirectAndStackCallsPreserveTidTagAndSideAndContinue(bool stack, bool back)
    {
        var sim = Room();
        var player = Assert.Single(sim.Players);
        var destinations = sim.Actors.Where(a => a.DoomEdNum == LineSpecials.TeleportDestType).ToArray();
        destinations[0].ThingId = 8; destinations[0].SectorIndex = 0;
        destinations[1].ThingId = 7; destinations[1].SectorIndex = 0;
        destinations[2].ThingId = 7; destinations[2].SectorIndex = 1;
        Add(sim, stack ? [3, 7, 3, 20, 5, 70] : [10, 70, 7, 20]);
        Assert.True(sim.Acs.TryExecute(1, [], player, null, back));
        sim.Acs.Tick(sim);
        Assert.Equal(back ? 0 : 300, player.X.ToDouble());
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureAngle);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NullOrDestroyedActivatorFailsButScriptContinues(bool destroyed)
    {
        var sim = Room(); var player = Assert.Single(sim.Players);
        Add(sim, [10, 70, 7, 20]);
        Assert.True(sim.Acs.TryExecute(1, [], destroyed ? player : null));
        if (destroyed) player.Destroy();
        sim.Acs.Tick(sim);
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureAngle);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ZeroTidAndTagRejectWithoutStoppingScript(bool stack)
    {
        var sim = Room(); var player = Assert.Single(sim.Players);
        player.ReactionTime = 7; player.VelocityX = Fixed.FromInt(3);
        Add(sim, stack ? [3, 0, 3, 0, 5, 70] : [10, 70, 0, 0]);
        Assert.True(sim.Acs.TryExecute(1, [], player)); sim.Acs.Tick(sim);
        Assert.Equal(0, player.X.ToDouble()); Assert.Equal(7, player.ReactionTime);
        Assert.Equal(3, player.VelocityX.ToDouble());
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureAngle);
    }

    private static void Add(AuthoritySimulation sim, int[] teleport)
    {
        int[] words = [.. teleport, 11, 185, 10, 90, 0, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 10, CeilingHeight = 128 }, new LevelSector { Tag = 20, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 100 },
            new LevelThing { Type = LineSpecials.TeleportDestType, X = 200 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 300 }],
    });
}