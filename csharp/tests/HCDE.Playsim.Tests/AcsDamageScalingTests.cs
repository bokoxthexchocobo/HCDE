using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsDamageScalingTests
{
    [Theory]
    [InlineData(24, 0)]
    [InlineData(24, 81920)]
    [InlineData(24, -81920)]
    [InlineData(24, int.MaxValue)]
    [InlineData(43, 0)]
    [InlineData(43, 81920)]
    [InlineData(43, -81920)]
    [InlineData(43, int.MinValue)]
    public void SetGetCheckPreserveSignedFixedValue(int property, int value)
    {
        var sim = Room(); var actor = sim.Players.Single();
        Run(sim, actor, [3, 0, 3, property, 3, value, 245,
            3, 7, 3, 0, 3, property, 246, 3, value, 19, 5, 112, 1]);
        Assert.Equal(value, property == 24 ? actor.DamageFactor.Raw : actor.DamageMultiplier.Raw);
        Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, property, 3, value, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, property, 3, value ^ 1, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(2, 0.5, 7)]
    [InlineData(0.5, 2, 6)]
    [InlineData(1.25, 1.25, 10)]
    [InlineData(0, 2, 0)]
    [InlineData(2, 0, 0)]
    [InlineData(-1, -1, 0)]
    [InlineData(1, -1, 0)]
    public void ScriptFactorsTruncateSeparatelyAndCancelNonPositiveDamage(double outgoing, double incoming, int lost)
    {
        var sim = Room(); var source = sim.Players.Single(); var target = sim.AddBot(128, 0);
        Set(sim, source, 43, outgoing); Set(sim, target, 24, incoming);
        var before = target.Health; var random = sim.CombatRandomState;
        Assert.Equal(lost, ActorDamage.Apply(target, 7, source).HealthLost);
        Assert.Equal(before - lost, target.Health);
        if (lost == 0) { Assert.Equal(random, sim.CombatRandomState); Assert.Null(target.LastDamageSourceId); }
    }

    [Fact]
    public void ScalingPrecedesArmorAbsorption()
    {
        var sim = Room(); var target = sim.Players.Single(); var source = sim.AddBot(128, 0);
        Set(sim, source, 43, 2); Set(sim, target, 24, 0.5);
        target.Inventory.Armor = 100; target.Inventory.ArmorSavePercent = 50;
        var result = ActorDamage.Apply(target, 10, source);
        Assert.Equal(5, result.HealthLost); Assert.Equal(5, result.ArmorLost);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForcedAndTelefragDamageBypassFactors(bool telefrag)
    {
        var sim = Room(); var target = sim.Players.Single(); var source = sim.AddBot(128, 0);
        Set(sim, target, 24, 0); Set(sim, source, 43, 0);
        var damage = telefrag ? ActorDamage.TelefragDamage : 10;
        var result = ActorDamage.Apply(target, damage, source, telefrag ? DamageFlags.None : DamageFlags.Forced);
        Assert.Equal(damage, result.HealthLost);
    }

    [Fact]
    public void InflictorDoesNotSubstituteForSourceMultiplier()
    {
        var sim = Room(); var target = sim.Players.Single(); var inflictor = sim.AddBot(128, 0);
        Set(sim, inflictor, 43, 0); Set(sim, target, 24, 0.5);
        Assert.Equal(5, ActorDamage.Apply(target, 10, inflictor: inflictor).HealthLost);
    }

    [Theory]
    [InlineData(24)]
    [InlineData(43)]
    public void FactorsParticipateInRestingChecksum(int property)
    {
        var first = Room(); var second = Room(); var actor = second.Players.Single();
        if (property == 24) actor.DamageFactor = Fixed.FromInt(2);
        else actor.DamageMultiplier = Fixed.FromInt(2);
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Theory]
    [InlineData(24, false)]
    [InlineData(24, true)]
    [InlineData(43, false)]
    [InlineData(43, true)]
    public void MissingOrDestroyedTargetsReturnZeroAndIgnoreWrites(int property, bool destroyed)
    {
        var sim = Room(); var player = sim.Players.Single(); if (destroyed) player.Destroy();
        var tid = destroyed ? 0 : 999;
        Run(sim, player, [3, tid, 3, property, 3, 0, 245,
            3, 7, 3, tid, 3, property, 246, 5, 112, 1]);
        Assert.Equal(65536, player.DamageFactor.Raw); Assert.Equal(65536, player.DamageMultiplier.Raw);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(24)]
    [InlineData(43)]
    public void TidWritesAllMatchesAndQueriesNewest(int property)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 3001, Id = 42 }, new LevelThing { Type = 3001, Id = 42, X = 128 }],
        });
        var actor = sim.Actors.First();
        Run(sim, actor, [3, 42, 3, property, 3, 32768, 245, 1]);
        Assert.All(sim.Actors, candidate => Assert.Equal(32768,
            property == 24 ? candidate.DamageFactor.Raw : candidate.DamageMultiplier.Raw));
        if (property == 24) sim.Actors.Last().DamageFactor = new Fixed(12345);
        else sim.Actors.Last().DamageMultiplier = new Fixed(12345);
        Run(sim, actor, [3, 7, 3, 42, 3, property, 246, 3, 12345, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
    }

    private static void Set(AuthoritySimulation sim, Actor actor, int property, double value) =>
        Run(sim, actor, [3, 0, 3, property, 3, Fixed.FromDouble(value).Raw, 245, 1]);
    private static void Run(AuthoritySimulation sim, Actor actor, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); sim.Acs.TryExecute(1, [], actor); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256, LightLevel = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
