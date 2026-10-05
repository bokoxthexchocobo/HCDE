using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoDeathSpecialTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void NoDeathSpecialPreservesSpecialWithoutRunningIt(bool suppressed, bool hexenHack)
    {
        var sim = Room(hexenHack); var player = sim.Players.Single(); var victim = sim.Actors[1];
        victim.ActivationType = suppressed ? 64 : 0;
        player.VelocityX = Fixed.FromInt(8);
        ActorDamage.Apply(victim, 1000, player);
        Assert.Equal(suppressed ? 8 : 0, player.VelocityX.ToDouble());
        Assert.Equal(suppressed || hexenHack ? 19 : 0, victim.Special);
        Assert.True(victim.IsDead);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(64)]
    public void ActivationTypeSurvivesArchive(int activation)
    {
        var sim = Room(false); var actor = sim.Actors[1]; actor.ActivationType = activation;
        var bytes = SimSavegame.Write(sim.CaptureState());
        Assert.Equal(activation == 0 ? 42 : 43, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.ActivationType = activation == 0 ? 64 : 0;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(activation, actor.ActivationType);
    }

    [Fact]
    public void ActivationTypeAffectsChecksum()
    {
        var first = Room(false); var second = Room(false);
        second.Actors[1].ActivationType = 64;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(bool hexenHack) => AuthoritySimulation.Start(new PlayLevel
    {
        HexenHack = hexenHack, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20, Special = 19 }],
    });
}
