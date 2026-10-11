using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HighJumpPowerTests
{
    [Theory]
    [InlineData(8, 2, 18)]
    [InlineData(0, 4, 4)]
    [InlineData(-2, 8, 4)]
    public void GroundJumpMultipliesImpulseRatherThanExistingVelocity(int jump, int velocity, int expected)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.JumpZ = Fixed.FromInt(jump); player.VelocityZ = Fixed.FromInt(velocity);
        player.GivePowerHighJump(10); Step(sim);
        Assert.Equal(expected, player.Z.ToDouble()); Assert.Equal(expected - 1, player.VelocityZ.ToDouble());
        Assert.Equal(9, player.PowerHighJumpTics);
    }

    [Fact]
    public void LastActiveTickUsesPowerThenExpires()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerHighJump(1);
        Step(sim); Assert.Equal(16, player.Z.ToDouble()); Assert.Equal(0, player.PowerHighJumpTics);
    }

    [Fact]
    public void ExpiredPowerUsesNormalJump()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerHighJump(1);
        sim.Tick(); Step(sim); Assert.Equal(8, player.Z.ToDouble());
    }

    [Fact]
    public void FlyingJumpKeepsNativeThreeUnitVelocity()
    {
        var sim = Room(); var player = sim.Players.Single(); player.NoGravity = true;
        player.GivePowerHighJump(10); Step(sim); Assert.Equal(3, player.Z.ToDouble());
    }

    [Fact]
    public void TimedPowerSupportsNativeRefreshAndDefinitionUnits()
    {
        var player = Room().Players.Single(); player.GivePowerHighJump(200);
        player.GivePowerHighJump(300); Assert.Equal(200, player.PowerHighJumpTics);
        player.GivePowerHighJump(300, alwaysPickup: true); Assert.Equal(300, player.PowerHighJumpTics);
        player.GivePowerup(ManagedPowerupKind.HighJump, -2, additiveTime: true);
        Assert.Equal(370, player.PowerHighJumpTics);
        Assert.Throws<ArgumentOutOfRangeException>(() => player.GivePowerHighJump(-1));
    }

    [Fact]
    public void QueriesTakeAndClearSupportExplicitlyTimedPower()
    {
        var player = Room().Players.Single(); player.GivePowerHighJump(99);
        Assert.Equal(99, AcsActorPowerups.RemainingTics(player, "PowerHighJump"));
        Assert.Equal(1, AcsPlayerInventory.Count(player, "PowerHighJump", false));
        Assert.False(AcsPlayerInventory.Drop(player, "PowerHighJump"));
        AcsPlayerInventory.Take(player, "PowerHighJump", 1); Assert.Equal(0, player.PowerHighJumpTics);
        player.GivePowerHighJump(99); ActorInventoryActions.ClearInventory(player);
        Assert.Equal(0, player.PowerHighJumpTics);
    }

    [Fact]
    public void SaveAndChecksumPreservePowerAndLegacyRestoreClearsIt()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerDoubleFiringSpeed(100);
        var legacy = SimSavegame.Write(sim); var before = sim.Checksum;
        player.GivePowerHighJump(99); sim.RestoreState(sim.CaptureState()); Assert.NotEqual(before, sim.Checksum);
        var bytes = SimSavegame.Write(sim); Assert.Equal(175, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(99, player.PowerHighJumpTics); Assert.Equal(100, player.PowerDoubleFiringSpeedTics);
        Step(sim); Assert.Equal(16, player.Z.ToDouble());
        SimSavegame.Apply(sim, legacy); Assert.Equal(0, player.PowerHighJumpTics);
    }

    [Fact]
    public void InvalidTimerFailsBeforeRestoreMutation()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerHighJump(99);
        var bytes = SimSavegame.Write(sim); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), -1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(99, player.PowerHighJumpTics);
        var state = sim.CaptureState(); state.Actors.Single().PowerHighJumpTics = -1;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(99, player.PowerHighJumpTics);
    }

    private static void Step(AuthoritySimulation sim)
    { sim.QueueCommand(0, new PlayerCommand { Jump = true }); sim.Tick(); }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
