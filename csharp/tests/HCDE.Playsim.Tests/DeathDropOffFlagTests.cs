using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DeathDropOffFlagTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DamageDeathEnablesDropOffEvenWithoutCorpseFlag(bool dontCorpse)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004);
        actor.DontCorpse = dontCorpse;
        Assert.False(actor.AllowDropOff);
        ActorDamage.Apply(actor, 1000);
        Assert.True(actor.AllowDropOff);
        Assert.Equal(!dontCorpse, actor.Corpse);
    }

    [Fact]
    public void DeathSavePreservesDropOffAndRaiseRestoresClassDefault()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004);
        ActorDamage.Apply(actor, 1000);
        actor.States.Enter(actor, ActorStateMachine.Corpse);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(100, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.AllowDropOff = false;
        SimSavegame.Apply(sim, bytes);
        Assert.True(actor.AllowDropOff);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.True(ActorRaiseActions.RaiseSelf(actor));
        Assert.False(actor.AllowDropOff);
    }

    [Fact]
    public void LivingSaveWithoutOverrideRestoresDropOffDefault()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004);
        var bytes = SimSavegame.Write(sim);
        ActorDamage.Apply(actor, 1000);
        SimSavegame.Apply(sim, bytes);
        Assert.False(actor.AllowDropOff);
        Assert.False(actor.IsDead);
    }

    [Fact]
    public void PlayerFalseOverrideSurvivesSave()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.AllowDropOff = false;
        var bytes = SimSavegame.Write(sim);
        player.AllowDropOff = true;
        SimSavegame.Apply(sim, bytes);
        Assert.False(player.AllowDropOff);
    }

    [Fact]
    public void DeadMonsterCanCrossLedgeRefusedWhileAlive()
    {
        var geometry = GameplayFoundationTests.TwoRooms(-100, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides, Lines = geometry.Lines,
            Things = [new LevelThing { Type = 3004, X = -40 }],
        });
        var actor = Assert.Single(sim.Actors);
        Assert.False(ActorPhysics.TryMove(sim, actor, 1, 0, out _));
        ActorDamage.Apply(actor, 1000);
        Assert.True(ActorPhysics.TryMove(sim, actor, 1, 0, out _));
    }

    [Fact]
    public void InvalidDropOffFlagIsRejected()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004);
        actor.AllowDropOff = true;
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-allow-dropoff-value", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }] });
}
