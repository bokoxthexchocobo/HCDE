using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TeleportStompTests
{
    [Theory]
    [InlineData(false, false, false, true)]
    [InlineData(true, false, false, false)]
    [InlineData(true, true, false, true)]
    [InlineData(false, false, true, false)]
    [InlineData(true, true, true, true)]
    public void TeleportHonorsStompAndVictimPolicies(bool noTelefrag, bool always, bool noStomp, bool succeeds)
    {
        var sim = Room(); var player = sim.Players.Single(); var victim = sim.AddBot(200, 0);
        victim.NoTelefrag = noTelefrag; victim.AlwaysTelefrag = always; victim.Invulnerable = true;
        player.NoTeleStomp = noStomp; player.VelocityX = Fixed.FromInt(3);
        Assert.Equal(succeeds, LineSpecials.Execute(sim, player, LineSpecials.Teleport, 0));
        Assert.Equal(succeeds, victim.IsDead);
        Assert.Equal(succeeds ? 200 : 0, player.X.ToDouble());
        if (!succeeds) Assert.Equal(Fixed.FromInt(3), player.VelocityX);
    }

    [Fact]
    public void OrdinaryMonsterCannotStompUntilFlagged()
    {
        var sim = Room(); var monster = sim.AddBot(-80, 0); var victim = sim.AddBot(200, 0);
        Assert.False(LineSpecials.Execute(sim, monster, LineSpecials.Teleport, 0));
        monster.TeleStomp = true;
        Assert.True(LineSpecials.Execute(sim, monster, LineSpecials.Teleport, 0)); Assert.True(victim.IsDead);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ThruActorsAndVerticalSeparationAvoidTelefrag(bool thru)
    {
        var sim = Room(); var player = sim.Players.Single(); var victim = sim.AddBot(200, 0);
        if (thru) victim.ThruActors = true; else victim.Z = Fixed.FromInt(80);
        Assert.True(LineSpecials.Execute(sim, player, LineSpecials.Teleport, 0)); Assert.False(victim.IsDead);
    }

    [Theory]
    [InlineData("actor.telestomp")]
    [InlineData("NOTELESTOMP")]
    public void StompFlagsSupportAcsAccess(string name)
    {
        var actor = Room().Players.Single();
        Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
    }

    [Fact]
    public void FreezeEnablesTeleportStomp()
    {
        var sim = Room(); var actor = sim.AddBot(-80, 0);
        Assert.False(actor.TeleStomp); ActorFreezeActions.FreezeDeath(actor); Assert.True(actor.TeleStomp);
    }

    [Fact]
    public void FlagsRoundTripAndOldSaveRestoresPlayerDefault()
    {
        var sim = Room(); var player = sim.Players.Single(); var older = SimSavegame.Write(sim, true);
        player.TeleStomp = false; player.NoTeleStomp = true;
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(309, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.False(restored.Players.Single().TeleStomp); Assert.True(restored.Players.Single().NoTeleStomp);
        Assert.Equal(saved, SimSavegame.Write(restored, true));
        SimSavegame.Apply(restored, older);
        Assert.True(restored.Players.Single().TeleStomp); Assert.False(restored.Players.Single().NoTeleStomp);
    }

    [Fact]
    public void NonSolidStomperPassesMonstersWithoutKillingThem()
    {
        var sim = Room(); var mover = sim.AddBot(-80, 0); var victim = sim.AddBot(200, 0);
        mover.Solid = false; mover.TeleStomp = true;
        Assert.True(LineSpecials.Execute(sim, mover, LineSpecials.Teleport, 0)); Assert.False(victim.IsDead);
    }

    [Fact]
    public void InvalidSavedFlagBitsAreRejected()
    {
        var sim = Room(); sim.Players.Single().NoTeleStomp = true;
        var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 4);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 200 }],
    });
}
