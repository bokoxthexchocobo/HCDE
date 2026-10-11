using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CausePainTests
{
    [Theory]
    [InlineData(0, 256, false, false, true)]
    [InlineData(1, 256, false, false, false)]
    [InlineData(0, 0, false, false, false)]
    [InlineData(0, 256, true, false, false)]
    [InlineData(0, 256, false, true, false)]
    public void ZeroDamageWakesButUsesNormalPainThresholdChanceAndSuppressors(int threshold, int chance,
        bool noPain, bool painless, bool pain)
    {
        var sim = Room(); var owner = sim.Players.Single();
        var target = sim.AddBot(100, 0); target.PainThreshold = threshold; target.PainChance = chance;
        target.NoPain = noPain; target.ReactionTime = 12; target.Health = 100;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        sim.DrainSoundRequests();
        missile.CausePain = true; missile.Painless = painless; missile.DamageExpression = _ => 0;
        missile.DoMissileDamage(target);
        Assert.Equal(100, target.Health); Assert.Equal(0, target.ReactionTime);
        Assert.Equal(owner.Id, target.Brain!.TargetId);
        Assert.Equal(pain, target.States.Current == target.PainState);
        Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }

    [Fact]
    public void NegativeExpressionTakesPainBranchWithoutHealingAndEmitsRipperSound()
    {
        var sim = Room(); var owner = sim.Players.Single(); var target = sim.AddBot(100, 0);
        target.Health = 50; target.NoPain = true; target.ReactionTime = 12;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        sim.DrainSoundRequests();
        missile.CausePain = true; missile.DamageExpression = _ => -7;
        missile.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Rip] = "custom/rip" };
        missile.DoMissileDamage(target, true);
        Assert.Equal(50, target.Health); Assert.Equal(0, target.ReactionTime);
        Assert.Equal(owner.Id, target.Brain!.TargetId);
        Assert.Equal("custom/rip", Assert.Single(sim.DrainSoundRequests()).Sound);
    }

    [Fact]
    public void ScriptFlagAndSaveRestoreRoundTripAndRejectMalformedFlag()
    {
        var sim = Room(); var actor = sim.Players.Single();
        Assert.True(AcsActorFlags.TrySet(actor, "Actor.CAUSEPAIN", true));
        Assert.True(AcsActorFlags.TryGet(actor, "causepain", out var flag)); Assert.True(flag);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(147, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.CausePain = false; SimSavegame.Apply(sim, bytes); Assert.True(actor.CausePain);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        bytes[bytes.Length - size + 8] = 2;
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-cause-pain-invalid", error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void NonnegativeDamageCancellationStillAllowsCausePain(int cancellation)
    {
        var sim = Room(); var owner = sim.Players.Single(); var target = sim.AddBot(100, 0);
        target.Health = 100; target.PainChance = 256; target.PainThreshold = 0;
        if (cancellation == 0) target.Invulnerable = true;
        if (cancellation == 1) target.DamageFactor = Fixed.FromInt(0);
        if (cancellation == 2) { target.Armor = 20; target.ArmorSavePercent = 100; }
        var result = ActorDamage.Apply(target, 7, owner, inflictor: new Actor { CausePain = true });
        Assert.Equal(100, target.Health); Assert.Equal(target.PainState, target.States.Current);
        Assert.Equal(owner.Id, target.Brain!.TargetId);
        Assert.Equal(cancellation == 2 ? 7 : 0, result.ArmorLost);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
