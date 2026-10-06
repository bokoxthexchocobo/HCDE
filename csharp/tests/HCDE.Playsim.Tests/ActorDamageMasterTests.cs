using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDamageMasterTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedMasterArchiveIsRejected(bool header)
    {
        var sim = Room(); sim.AddBot(0, 0).MasterId = null;
        var bytes = SimSavegame.Write(sim);
        var start = bytes.Length - System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + (header ? 0 : 8)), header ? 85 : 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    [Theory]
    [InlineData(10, 40)]
    [InlineData(-10, 60)]
    [InlineData(0, 50)]
    public void MasterActionAndReferenceRoundTrip(int amount, int expected)
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 3004); var master = sim.AddBot(100, 0, 3001);
        master.Health = 50; caller.MasterId = master.Id;
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        caller.MasterId = null; sim.RestoreState(state);
        Assert.Same(master, AcsActorPointer.Resolve(sim, caller, AcsActorPointer.Master));
        Assert.Equal(bytes, SimSavegame.Write(sim));
        ActorHealthActions.DamageMaster(caller, amount);
        Assert.Equal(expected, master.Health); Assert.Equal(20, caller.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedMasterDoesNothing(bool destroyed)
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var master = sim.AddBot(100, 0);
        if (destroyed) { caller.MasterId = master.Id; master.Destroy(); }
        var bytes = SimSavegame.Write(sim);
        ActorHealthActions.DamageMaster(caller, 100);
        Assert.Null(AcsActorPointer.Resolve(sim, caller, AcsActorPointer.Master));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void ExplicitClearIsSavedAndLegacyAbsenceClearsReference()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var master = sim.AddBot(100, 0);
        var legacy = SimSavegame.Write(sim);
        caller.MasterId = null; var cleared = SimSavegame.Write(sim);
        caller.MasterId = master.Id;
        Assert.True(SimSavegame.TryRead(legacy, out var state, out var error), error);
        sim.RestoreState(state); Assert.Null(caller.MasterId);
        Assert.True(SimSavegame.TryRead(cleared, out state, out error), error);
        sim.RestoreState(state); Assert.Null(caller.MasterId);
        Assert.Equal(cleared, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
