using HCDE.MapLoader;
using System.Buffers.Binary;

namespace HCDE.Playsim.Tests;

public class NoiseAlertSpecialTests
{
    [Fact]
    public void SeparateTargetAndEmitterUseNativeArgumentOrder()
    {
        var (sim, listener, emitter, target) = Room();
        Assert.True(NoiseAlertSpecial.Execute(sim, null, 8, 7)); Assert.Equal(target.Id, listener.LastHeardTargetId);
    }

    [Fact]
    public void ZeroTidsUseActivator()
    {
        var (sim, listener, emitter, _) = Room(); Assert.True(NoiseAlertSpecial.Execute(sim, emitter, 0, 0));
        Assert.Equal(emitter.Id, listener.LastHeardTargetId);
    }

    [Fact]
    public void MissingTargetClearsNoiseWhenEmitterExists()
    {
        var (sim, listener, emitter, target) = Room(); listener.LastHeardTargetId = target.Id;
        Assert.True(NoiseAlertSpecial.Execute(sim, null, 999, 7)); Assert.Null(listener.LastHeardTargetId);
    }

    [Fact]
    public void MissingEmitterSucceedsWithoutAlert()
    {
        var (sim, listener, _, target) = Room(); listener.LastHeardTargetId = target.Id;
        Assert.True(NoiseAlertSpecial.Execute(sim, null, 0, 999)); Assert.Equal(target.Id, listener.LastHeardTargetId);
        Assert.True(NoiseAlertSpecial.Execute(sim, null, 0, 0));
    }

    [Fact]
    public void EqualTidsUseSameActorForTargetAndEmitter()
    {
        var (sim, listener, emitter, _) = Room(); Assert.True(NoiseAlertSpecial.Execute(sim, null, 7, 7));
        Assert.Equal(emitter.Id, listener.LastHeardTargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsSupportsDirectAndStackCalls(bool stack)
    {
        var (sim, listener, _, target) = Room(); int[] words = stack ? [3, 8, 3, 7, 5, 173, 1] : [10, 173, 8, 7, 1];
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code }); Assert.True(sim.Acs.TryExecute(1, [], null));
        sim.Acs.Tick(sim); Assert.Equal(target.Id, listener.LastHeardTargetId);
    }

    [Fact]
    public void ActorActionAndMapLineDispatch()
    {
        var (sim, listener, emitter, target) = Room();
        Assert.True(ActorSpecialActions.CallSpecial(emitter, 173, 8, 7)); Assert.Equal(target.Id, listener.LastHeardTargetId);
        var line = new LevelLine { Special = 173, Arg0 = 7, Arg1 = 7, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, emitter, line, true));
        Assert.Equal(0, line.Special); Assert.Equal(emitter.Id, listener.LastHeardTargetId);
    }

    private static (AuthoritySimulation Sim, Actor Listener, Actor Emitter, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 },
                new LevelThing { Type = 1, Id = 7 }, new LevelThing { Type = 3001, X = 300, Id = 8 }] });
        return (sim, sim.Actors[0], sim.Actors[1], sim.Actors[2]);
    }
}
