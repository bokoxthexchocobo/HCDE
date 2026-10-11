using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileDontThrustTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SavedFlagSuppressesLaunchWithoutSuppressingDamageOrSound(bool dontThrust, bool fire)
    {
        var (sim, attacker, target) = Room();
        Assert.True(AcsActorFlags.TrySet(target, "dontthrust", dontThrust));
        Assert.True(AcsActorFlags.TryGet(target, "DONTTHRUST", out var flag));
        Assert.Equal(dontThrust, flag);
        var bytes = SimSavegame.Write(sim);
        target.DontThrust = !dontThrust;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(dontThrust, target.DontThrust);
        for (var pass = 0; pass < 2; pass++)
        {
            ArchvileActions.Attack(sim, attacker, target, fire, thrust: 2);
            Assert.Equal(dontThrust ? -3 : 20, target.VelocityZ.ToDouble());
            Assert.Equal(fire ? 918 : 980, target.Health);
            Assert.Equal("vile/stop", Assert.Single(sim.DrainSoundRequests()).Sound);
            SimSavegame.Apply(sim, bytes);
        }
    }

    [Fact]
    public void LegacySaveResetsFlagAndChecksumTracksIt()
    {
        var (sim, _, target) = Room(); var bytes = SimSavegame.Write(sim);
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        target.DontThrust = true; var state = sim.CaptureState();
        target.DontThrust = false; sim.RestoreState(state);
        Assert.True(target.DontThrust);
        Assert.NotEqual(checksum, sim.Checksum);
        SimSavegame.Apply(sim, bytes);
        Assert.False(target.DontThrust); Assert.Equal(checksum, sim.Checksum);
    }

    [Fact]
    public void CorruptFlagIsRejectedWithoutApplyingState()
    {
        var (sim, _, target) = Room(); target.DontThrust = true;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(186, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-dontthrust-value", error);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.True(target.DontThrust);
    }

    private static (AuthoritySimulation, Actor, PlayerPawn) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1, X = 300 }],
        });
        var attacker = sim.AddBot(0, 0, 64); attacker.Brain = null;
        var target = sim.Players.Single(); target.Health = 1000; target.NoPain = true;
        target.VelocityZ = Fixed.FromInt(-3);
        return (sim, attacker, target);
    }
}
