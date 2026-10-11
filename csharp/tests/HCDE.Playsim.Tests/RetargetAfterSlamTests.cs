using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RetargetAfterSlamTests
{
    [Theory]
    [InlineData(false, false, 4)]
    [InlineData(true, false, 0)]
    [InlineData(false, true, 3)]
    public void SavedFlagSkipsSeeButPreservesSlamPriority(bool noIdle, bool hasSlam, int expected)
    {
        var sim = Room(); var soul = Soul(sim);
        soul.States.Configure(soul, Enumerable.Range(0, 5).Select(i => new ActorFrame(-1, i)), 2);
        soul.SpawnState = 0; soul.SeeState = 1; soul.PainState = -1;
        soul.SlamState = hasSlam ? 3 : -1; soul.IdleState = noIdle ? -1 : 4;
        Assert.True(AcsActorFlags.TrySet(soul, "Actor.RETARGETAFTERSLAM", true));
        Assert.True(AcsActorFlags.TryGet(soul, "retargetafterslam", out var enabled)); Assert.True(enabled);
        soul.Brain!.StartCharge(soul, sim.Players.Single());
        var bytes = SimSavegame.Write(sim); Assert.Equal(145, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        soul.RetargetAfterSlam = false; SimSavegame.Apply(sim, bytes);
        Assert.True(soul.RetargetAfterSlam);
        for (var i = 0; i < 10 && soul.Brain.Charging; i++) soul.Tick();
        Assert.Equal(expected, soul.States.Current); Assert.False(soul.Brain.Charging);
        Assert.True(sim.Players.Single().Health < 100);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        bytes[bytes.Length - size + 8] = 2;
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-retarget-after-slam-invalid", error);
    }

    [Fact]
    public void LegacySaveClearsLaterFlagAndFlagAffectsChecksum()
    {
        var sim = Room(); var soul = Soul(sim); var bytes = SimSavegame.Write(sim);
        sim.RestoreState(sim.CaptureState());
        var original = sim.Checksum; soul.RetargetAfterSlam = true;
        sim.RestoreState(sim.CaptureState());
        Assert.NotEqual(original, sim.Checksum);
        SimSavegame.Apply(sim, bytes); Assert.False(soul.RetargetAfterSlam);
        Assert.True(AcsActorFlags.TrySet(soul, "RETARGETAFTERSLAM", false));
    }

    private static Actor Soul(AuthoritySimulation sim) => sim.Actors.Single(a => a.DoomEdNum == 3006);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1, X = 64 }, new LevelThing { Type = 3006 }] });
}
