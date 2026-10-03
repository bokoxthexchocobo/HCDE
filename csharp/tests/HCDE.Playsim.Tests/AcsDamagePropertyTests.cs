using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsDamagePropertyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(123)]
    [InlineData(int.MaxValue)]
    [InlineData(-1)]
    public void ScriptSetGetCheckUseIntegerBaseDamage(int value)
    {
        var sim = Room(); var actor = sim.Players.Single(); var expected = Math.Max(0, value);
        Run(sim, actor, [3, 0, 3, 2, 3, value, 245,
            3, 7, 3, 0, 3, 2, 246, 3, expected, 19, 5, 112, 1]);
        Assert.Equal(value, actor.Damage); Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 2, 3, expected, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 2, 3, expected ^ 1, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void ScriptDamageChangesSeededProjectileActorImpact(int damage)
    {
        var sim = Room(); var player = sim.Players.Single(); var target = sim.AddBot(128, 0);
        target.Brain = null; target.Health = 1000; target.PainChance = 0;
        var missile = sim.SpawnProjectile(player, ProjectileKind.Plasma); Set(sim, missile, damage);
        Assert.Equal(damage, missile.ImpactDamage);
        for (var i = 0; i < 10 && !missile.Destroyed; i++) sim.Tick();
        Assert.True(missile.Destroyed);
        var lost = 1000 - target.Health;
        if (damage == 0) Assert.Equal(0, lost);
        else { Assert.InRange(lost, damage, damage * 8); Assert.Equal(0, lost % damage); }
    }

    [Fact]
    public void GeometryImpactUsesConfiguredBaseAndOneDamageRoll()
    {
        var first = Room(); var second = Room();
        var lineA = new LevelLine { SideFront = 0, SideBack = -1, Health = 1000 };
        var lineB = new LevelLine { SideFront = 0, SideBack = -1, Health = 1000 };
        var missileA = first.SpawnProjectile(first.Players.Single(), ProjectileKind.Plasma);
        var missileB = second.SpawnProjectile(second.Players.Single(), ProjectileKind.Plasma);
        missileA.Damage = 7; missileB.Damage = 14;
        GeometryProjectileImpact.Apply(first, missileA, lineA); GeometryProjectileImpact.Apply(second, missileB, lineB);
        Assert.True(lineA.Health < 1000); Assert.Equal((1000 - lineA.Health) * 2, 1000 - lineB.Health);
        Assert.Equal(first.CombatRandomState, second.CombatRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedTargetsIgnoreWrites(bool destroyed)
    {
        var sim = Room(); var actor = sim.Players.Single(); if (destroyed) actor.Destroy();
        var tid = destroyed ? 0 : 999;
        Run(sim, actor, [3, tid, 3, 2, 3, 123, 245, 3, 7, 3, tid, 3, 2, 246, 5, 112, 1]);
        Assert.Equal(0, actor.Damage); Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void DamageParticipatesInRestingChecksum()
    {
        var first = Room(); var second = Room(); second.Players.Single().Damage = 123;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static void Set(AuthoritySimulation sim, Actor actor, int damage) =>
        Run(sim, actor, [3, 0, 3, 2, 3, damage, 245, 1]);
    private static void Run(AuthoritySimulation sim, Actor actor, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); sim.Acs.TryExecute(1, [], actor); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256, LightLevel = 128 }],
        Sides = [new LevelSide { Sector = 0 }], Things = [new LevelThing { Type = 1 }],
    }, rngSeed: 42);
}
