using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoDamageTests
{
    [Theory]
    [InlineData(false, 17, false, 100, true)]
    [InlineData(true, 17, false, 100, true)]
    [InlineData(false, 17, true, 83, true)]
    [InlineData(true, 17, true, 100, true)]
    [InlineData(false, ActorDamage.TelefragDamage, false, 100, true)]
    [InlineData(true, ActorDamage.TelefragDamage, false, 100, true)]
    [InlineData(false, 0, false, 100, false)]
    [InlineData(true, 0, false, 100, false)]
    public void ProtectionPainAndForcedDamageFollowTargetKind(bool player, int damage, bool forced, int health, bool pain)
    {
        Actor target = player ? new PlayerPawn() : new Actor();
        target.Health = 100; target.NoDamage = true; target.PainChance = 256; target.PainThreshold = 0;
        target.Armor = 50; target.ArmorSavePercent = 50;
        if (target is PlayerPawn pawn) { pawn.Inventory.Armor = 50; pawn.Inventory.ArmorSavePercent = 50; }
        var result = ActorDamage.Apply(target, damage, flags: forced ? DamageFlags.Forced : DamageFlags.None);
        Assert.Equal(health, target.Health); Assert.Equal(100 - health, result.HealthLost);
        Assert.Equal(0, result.ArmorLost); Assert.Equal(pain ? target.PainState : target.SpawnState, target.States.Current);
        Assert.Equal(50, target is PlayerPawn saved ? saved.Inventory.Armor : target.Armor);
    }

    [Fact]
    public void PainSuppressionStillAllowsWakeupWithoutHealthLoss()
    {
        var sim = Room(); var owner = sim.Players.Single(); var target = sim.AddBot(100, 0);
        target.Health = 100; target.NoDamage = target.NoPain = true; target.ReactionTime = 12;
        ActorDamage.Apply(target, 7, owner);
        Assert.Equal(100, target.Health); Assert.Equal(0, target.ReactionTime);
        Assert.Equal(owner.Id, target.Brain!.TargetId); Assert.NotEqual(target.PainState, target.States.Current);
    }

    [Fact]
    public void NegativeSpecialDamageCancelsPainEvenWithNoDamage()
    {
        var actor = new CancellingActor { Health = 100, NoDamage = true, ReactionTime = 12 };
        ActorDamage.Apply(actor, 7);
        Assert.Equal(100, actor.Health); Assert.Equal(actor.SpawnState, actor.States.Current);
        Assert.Equal(12, actor.ReactionTime);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void InvulnerablePlayerCanReactButGodModeSuppressesPain(bool godMode, bool pain)
    {
        var target = new PlayerPawn { Health = 100, NoDamage = true, Invulnerable = true,
            GodMode = godMode, PainChance = 256, PainThreshold = 0 };
        ActorDamage.Apply(target, 7);
        Assert.Equal(100, target.Health);
        Assert.Equal(pain ? target.PainState : target.SpawnState, target.States.Current);
    }

    [Fact]
    public void ScriptFlagAndArchiveRoundTripWithMalformedFlagRejection()
    {
        var sim = Room(); var actor = sim.Players.Single();
        Assert.True(AcsActorFlags.TrySet(actor, "Actor.NODAMAGE", true));
        Assert.True(AcsActorFlags.TryGet(actor, "nodamage", out var flag)); Assert.True(flag);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(150, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.NoDamage = false; SimSavegame.Apply(sim, bytes);
        Assert.True(actor.NoDamage); Assert.Equal(bytes, SimSavegame.Write(sim));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        bytes[bytes.Length - size + 8] = 2;
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-no-damage-invalid", error);
    }

    private sealed class CancellingActor : Actor
    {
        public override int TakeSpecialDamage(Actor? inflictor, Actor? source, int damage, string? damageType, DamageFlags flags) => -1;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
