using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AllowPainTests
{
    [Theory]
    [InlineData(7, 0, false, true)]
    [InlineData(7, 1, false, true)]
    [InlineData(7, 2, false, true)]
    [InlineData(0, 0, false, false)]
    [InlineData(-7, 0, false, false)]
    [InlineData(7, 0, true, false)]
    public void OriginalPositiveHitControlsFakePain(int amount, int cancellation, bool noPain, bool pain)
    {
        var sim = Room(); var owner = sim.Players.Single(); var target = sim.AddBot(100, 0);
        target.Health = 100; target.AllowPain = true; target.PainChance = 256;
        target.PainThreshold = 0; target.NoPain = noPain; target.ReactionTime = 12;
        if (cancellation == 0) target.DamageFactor = default;
        if (cancellation == 1) { target.Armor = 20; target.ArmorSavePercent = 100; }
        if (cancellation == 2) target.Invulnerable = true;
        var result = ActorDamage.Apply(target, amount, owner);
        Assert.Equal(100, target.Health); Assert.Equal(0, result.HealthLost);
        Assert.Equal(cancellation == 1 ? 7 : 0, result.ArmorLost);
        Assert.Equal(pain, target.States.Current == target.PainState);
        Assert.Equal(amount > 0 ? 0 : 12, target.ReactionTime);
        Assert.Equal(amount > 0 ? owner.Id : (uint?)null, target.Brain!.TargetId);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 256)]
    public void AllowPainDoesNotBypassChanceOrThreshold(int threshold, int chance)
    {
        var target = new Actor { Health = 100, AllowPain = true, DamageFactor = default,
            PainThreshold = threshold, PainChance = chance, ReactionTime = 12 };
        ActorDamage.Apply(target, 7);
        Assert.Equal(0, target.States.Current); Assert.Equal(0, target.ReactionTime);
    }

    [Fact]
    public void NegativeSpecialDamageCancellationStopsAllReactions()
    {
        var target = new CancellingActor { Health = 100, AllowPain = true, ReactionTime = 12 };
        ActorDamage.Apply(target, 7, new Actor());
        Assert.Equal(100, target.Health); Assert.Equal(0, target.States.Current);
        Assert.Equal(12, target.ReactionTime);
    }

    [Fact]
    public void FlagRoundTripsAndMalformedBooleanIsRejected()
    {
        var sim = Room(); var actor = sim.Players.Single();
        Assert.True(AcsActorFlags.TrySet(actor, "Actor.ALLOWPAIN", true));
        Assert.True(AcsActorFlags.TryGet(actor, "allowpain", out var flag)); Assert.True(flag);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(149, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.AllowPain = false; SimSavegame.Apply(sim, bytes);
        Assert.True(actor.AllowPain); Assert.Equal(bytes, SimSavegame.Write(sim));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        bytes[bytes.Length - size + 8] = 2;
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-allow-pain-invalid", error);
    }

    private sealed class CancellingActor : Actor
    {
        public override int TakeSpecialDamage(Actor? inflictor, Actor? source, int damage, string? damageType, DamageFlags flags) => -1;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
