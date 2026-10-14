using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoiseMarkingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnlinkedOrDestroyedListenersAreNotMarked(bool destroyed)
    {
        var (sim, emitter, listener) = Room(); listener.LastHeardTargetId = listener.Id;
        if (destroyed) listener.Destroy(); else listener.NoSector = true;
        SoundPropagation.Alert(sim, emitter);
        if (!destroyed) Assert.Equal(listener.Id, listener.LastHeardTargetId);
        Assert.Null(listener.TargetId);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(100, true)]
    [InlineData(99, false)]
    [InlineData(-1, false)]
    public void NoiseRangeUsesInclusiveDistanceFromEmitter(double range, bool marked)
    {
        var (sim, emitter, listener) = Room();
        SoundPropagation.Alert(sim, emitter, emitter, range);
        Assert.Equal(marked ? emitter.Id : (uint?)null, listener.LastHeardTargetId);
    }

    [Fact]
    public void SeparateEmitterAndTargetMarksBlamedTarget()
    {
        var (sim, emitter, listener) = Room(); var blamed = sim.AddBot(1000, 0, 3001);
        SoundPropagation.Alert(sim, emitter, blamed, 100);
        Assert.Equal(blamed.Id, listener.LastHeardTargetId); Assert.Equal(blamed.Id, listener.TargetId);
        Assert.Null(blamed.LastHeardTargetId);
    }

    [Fact]
    public void NullTargetClearsNoiseMemoryWithoutWaking()
    {
        var (sim, emitter, listener) = Room(); listener.LastHeardTargetId = emitter.Id;
        SoundPropagation.Alert(sim, emitter, null);
        Assert.Null(listener.LastHeardTargetId); Assert.Null(listener.TargetId);
    }

    [Fact]
    public void NoTargetPlayerDoesNotOverwriteStoredNoise()
    {
        var (sim, emitter, listener) = Room(); listener.LastHeardTargetId = listener.Id;
        ((PlayerPawn)emitter).NoTarget = true; SoundPropagation.Alert(sim, emitter);
        Assert.Equal(listener.Id, listener.LastHeardTargetId); Assert.Null(listener.TargetId);
    }

    private static (AuthoritySimulation Sim, Actor Emitter, Actor Listener) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 100 }] });
        return (sim, sim.Players.Single(), sim.Actors.Single(a => a.ClassDoomEdNum == 3004));
    }
}
