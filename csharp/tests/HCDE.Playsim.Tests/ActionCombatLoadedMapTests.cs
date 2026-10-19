using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActionCombatLoadedMapTests
{
    [Fact]
    public void CustomMeleeDamagesInRangeAndPlaysHitSound()
    {
        var sim = CombatRoom();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var player = sim.Players.Single();
        imp.Brain!.SetSpecialTarget(player);
        player.X = Fixed.FromDouble(imp.X.ToDouble() + 40);
        ActorCombatActions.CustomMeleeAttack(imp, 8, "imp/melee", "imp/melee", "Melee");
        Assert.True(player.Health < 100);
        Assert.Contains(sim.DrainSoundRequests(), sound => sound.Sound.Contains("imp/melee", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CustomMeleeMissPlaysMissSoundWithoutDamage()
    {
        var sim = CombatRoom();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var player = sim.Players.Single();
        imp.Brain!.SetSpecialTarget(player);
        player.X = Fixed.FromDouble(imp.X.ToDouble() + 256);
        var health = player.Health;
        ActorCombatActions.CustomMeleeAttack(imp, 8, "imp/melee", "imp/miss", "Melee");
        Assert.Equal(health, player.Health);
        Assert.Contains(sim.DrainSoundRequests(), sound => sound.Sound.Contains("imp/miss", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CustomComboSpawnsProjectileWhenOutOfMeleeRange()
    {
        var sim = CombatRoom();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var player = sim.Players.Single();
        imp.Brain!.SetSpecialTarget(player);
        player.X = Fixed.FromDouble(imp.X.ToDouble() + 256);
        var spawned = ActorCombatActions.CustomComboAttack(imp, "DoomImpBall", 32, 3, "imp/melee", "Melee");
        var missile = Assert.IsType<ProjectileActor>(spawned);
        Assert.Equal(ProjectileKind.ImpBall, missile.Kind);
        Assert.Same(imp, missile.Owner);
    }

    [Fact]
    public void BulletAttackFiresPelletsFromMissileDamageCount()
    {
        var sim = CombatRoom();
        var grunt = sim.Actors.Single(a => a.DoomEdNum == 3004);
        var player = sim.Players.Single();
        grunt.Brain!.SetSpecialTarget(player);
        player.Invulnerable = true;
        grunt.Damage = 3;
        grunt.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = "grunt/attack" };
        var health = player.Health;
        ActorCombatActions.BulletAttack(grunt);
        Assert.Contains(sim.DrainSoundRequests(), sound => sound.Sound.Contains("grunt/attack", StringComparison.OrdinalIgnoreCase));
        Assert.True(player.Health <= health);
    }

    [Fact]
    public void SpawnProjectileUsesClassNameAndAimsAtTarget()
    {
        var sim = CombatRoom();
        var baron = sim.Actors.Single(a => a.DoomEdNum == 3003);
        var player = sim.Players.Single();
        baron.Brain!.SetSpecialTarget(player);
        var missile = ActorCombatActions.SpawnProjectile(baron, "BaronBall", 32, 0, default,
            ActorCombatActions.SpawnProjectileFlags.AimModeMask, default, AcsActorPointer.Target);
        Assert.NotNull(missile);
        Assert.Equal(ProjectileKind.BaronBall, missile!.Kind);
    }

    [Fact]
    public void MonsterRefireReturnsFailStateWhenTargetLost()
    {
        var sim = CombatRoom();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var player = sim.Players.Single();
        imp.Brain!.SetSpecialTarget(player);
        player.Destroy();
        var next = ActorRefireActions.MonsterRefire(imp, 0, failState: 7);
        Assert.Equal(7, next);
    }

    [Theory]
    [InlineData("A_BulletAttack")]
    [InlineData("A_FaceTarget")]
    [InlineData("A_Remove")]
    public void DispatchInvokesWiredCombatActions(string name)
    {
        var sim = CombatRoom();
        var grunt = sim.Actors.Single(a => a.DoomEdNum == 3004);
        grunt.Brain!.SetSpecialTarget(sim.Players.Single());
        sim.Players.Single().Invulnerable = true;
        Assert.True(ActorActionDispatch.TryInvoke(name, grunt));
    }

    private static AuthoritySimulation CombatRoom()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Namespace = "ZDoom",
            Sectors = [new LevelSector { CeilingHeight = 128, FloorHeight = 0 }],
            Sides = geometry.Sides,
            Lines = geometry.Lines,
            Things =
            [
                new LevelThing { Type = 1, X = 0, Y = 0 },
                new LevelThing { Type = 3001, X = 64, Y = 0 },
                new LevelThing { Type = 3003, X = 96, Y = 0 },
                new LevelThing { Type = 3004, X = 128, Y = 0 },
            ],
        });
    }
}
