using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsAttackZOffsetTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(81920)]
    [InlineData(-81920)]
    [InlineData(1048576)]
    public void ScriptSetGetCheckPreservesSignedFixedOffset(int value)
    {
        var sim = Room(); var player = sim.Players.Single();
        Run(sim, player, [3, 0, 3, 40, 3, value, 245,
            3, 7, 3, 0, 3, 40, 246, 3, value, 19, 5, 112, 1]);
        Assert.Equal(value, player.AttackZOffset.Raw); Assert.Equal(1, sim.LightOf(0));
        Run(sim, player, [3, 7, 3, 0, 3, 40, 3, value, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
        Run(sim, player, [3, 7, 3, 0, 3, 40, 3, value ^ 1, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(8, false, 36, 32)]
    [InlineData(16, false, 44, 40)]
    [InlineData(-8, false, 20, 16)]
    [InlineData(8, true, 18, 16)]
    [InlineData(16, true, 22, 20)]
    public void AttacksUseConfiguredAndCrouchScaledOrigins(int offset, bool crouched, double hitscanZ, double missileZ)
    {
        var sim = Room(); var player = sim.Players.Single();
        Run(sim, player, [3, 0, 3, 40, 3, offset * 65536, 245, 1]);
        if (crouched)
            for (var i = 0; i < 6; i++) { sim.QueueCommand(0, new PlayerCommand { Crouch = true }); sim.Tick(); }
        var hit = CombatTrace.TraceLineAttack(sim, player, player.Angle, new BamAngle(0), 64);
        Assert.False(hit.Hit); Assert.Equal(hitscanZ, hit.Z, 10);
        var missile = sim.SpawnProjectile(player, ProjectileKind.Rocket);
        Assert.Equal(missileZ, missile.Z.ToDouble(), 4);
    }

    [Theory]
    [InlineData(0, -64, 0)]
    [InlineData(100, -64, 60)]
    public void PlayerMissileClampsToSectorFloorRatherThanOwnerHeight(int z, int offset, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Z = Fixed.FromInt(z);
        player.AttackZOffset = Fixed.FromInt(offset);
        Assert.Equal(expected, sim.SpawnProjectile(player, ProjectileKind.Rocket).Z.ToDouble());
    }

    [Fact]
    public void MonsterQueryRemainsZeroButNativeAttackOffsetIsEight()
    {
        var sim = Room(); var monster = sim.AddBot(128, 0);
        Run(sim, monster, [3, 0, 3, 40, 3, 12345, 245, 3, 7, 3, 0, 3, 40, 246, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
        monster.Angle = BamAngle.FromDegrees(90);
        var hit = CombatTrace.TraceLineAttack(sim, monster, monster.Angle, new BamAngle(0), 64);
        Assert.Equal(monster.Z.ToDouble() + monster.Height.ToDouble() / 2 + 8, hit.Z);
    }

    [Fact]
    public void ScriptOffsetChangesWhichTargetHeightCanBeHit()
    {
        var sim = Room(); var player = sim.Players.Single(); var target = sim.AddBot(128, 0);
        target.Height = Fixed.FromInt(4); target.Z = Fixed.FromInt(35);
        Assert.Same(target, CombatTrace.TraceLineAttack(sim, player, player.Angle, new BamAngle(0), 256).Victim);
        Run(sim, player, [3, 0, 3, 40, 3, 0, 245, 1]);
        Assert.Null(CombatTrace.TraceLineAttack(sim, player, player.Angle, new BamAngle(0), 256).Victim);
    }

    [Fact]
    public void OffsetParticipatesInRestingChecksum()
    {
        var first = Room(); var second = Room(); second.Players.Single().AttackZOffset = Fixed.FromInt(16);
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedTargetsIgnoreSetterAndReturnZero(bool destroyed)
    {
        var sim = Room(); var player = sim.Players.Single(); if (destroyed) player.Destroy();
        var tid = destroyed ? 0 : 999;
        Run(sim, player, [3, tid, 3, 40, 3, 12345, 245,
            3, 7, 3, tid, 3, 40, 246, 5, 112, 1]);
        Assert.Equal(8 * 65536, player.AttackZOffset.Raw); Assert.Equal(0, sim.LightOf(0));
    }

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
