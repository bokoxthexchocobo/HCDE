using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CombatAiTests
{
    [Fact]
    public void HeldAttackUsesCadenceAndSwitchingWeaponsCannotBypassIt()
    {
        var sim = Range(); var player = sim.Players.Single();
        Fire(sim);
        Assert.Equal(49, player.Inventory.Bullets);
        for (var i = 0; i < 18; i++) Fire(sim);
        Assert.Equal(49, player.Inventory.Bullets);
        Fire(sim);
        Assert.Equal(48, player.Inventory.Bullets);
        player.Inventory.Selected = WeaponKind.Fist;
        Assert.False(HitscanCombat.Fire(sim, player));
    }

    [Theory]
    [InlineData(WeaponKind.Shotgun, AmmoKind.Shells, 1)]
    [InlineData(WeaponKind.SuperShotgun, AmmoKind.Shells, 2)]
    [InlineData(WeaponKind.RocketLauncher, AmmoKind.Rockets, 1)]
    [InlineData(WeaponKind.Plasma, AmmoKind.Cells, 1)]
    [InlineData(WeaponKind.Bfg, AmmoKind.Cells, 40)]
    public void WeaponsRequireTheirFullAmmoCost(WeaponKind weapon, AmmoKind ammo, int cost)
    {
        var sim = Range(); var player = sim.Players.Single();
        player.Inventory.Weapons |= weapon; player.Inventory.Selected = weapon;
        player.Inventory.TryAddAmmo(ammo, cost - 1);
        Assert.False(HitscanCombat.Fire(sim, player));
        Assert.Equal(0, player.WeaponCooldown);
        player.Inventory.TryAddAmmo(ammo, 1);
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(0, player.Inventory.Ammo(ammo));
    }

    [Fact]
    public void CombinedWeaponFlagsCannotFire()
    {
        var sim = Range(); var player = sim.Players.Single();
        player.Inventory.Selected = WeaponKind.Fist | WeaponKind.Pistol;
        Assert.False(HitscanCombat.Fire(sim, player));
        Assert.Equal(50, player.Inventory.Bullets);
    }

    [Theory]
    [InlineData(WeaponKind.Shotgun, 7)]
    [InlineData(WeaponKind.SuperShotgun, 20)]
    public void ShotgunsProduceSeededMultiPelletDamage(WeaponKind weapon, int pellets)
    {
        var left = Range(targetX: 48); var right = Range(targetX: 48);
        foreach (var sim in new[] { left, right })
        {
            var player = sim.Players.Single();
            player.Inventory.Weapons |= weapon; player.Inventory.Selected = weapon; player.Inventory.Shells = 10;
            Fire(sim);
        }
        var lost = 1000 - Target(left).Health;
        Assert.InRange(lost, pellets * 5, pellets * 15);
        Assert.Equal(Target(left).Health, Target(right).Health);
        Assert.Equal(left.CombatRandomState, right.CombatRandomState);
        Assert.Equal(left.Checksum, right.Checksum);
    }

    [Theory]
    [InlineData(ProjectileKind.Plasma)]
    [InlineData(ProjectileKind.Rocket)]
    [InlineData(ProjectileKind.ImpBall)]
    public void ProjectilesTravelBeforeImpactAndDamageOnlyOnce(ProjectileKind kind)
    {
        var sim = Range(); var victim = Target(sim);
        var projectile = sim.SpawnProjectile(sim.Players.Single(), kind);
        Assert.Equal(1000, victim.Health);
        sim.Tick();
        Assert.False(projectile.Destroyed);
        Assert.Equal(1000, victim.Health);
        for (var i = 0; i < 30; i++) sim.Tick();
        Assert.True(projectile.Destroyed);
        Assert.True(victim.Health < 1000);
        var health = victim.Health;
        for (var i = 0; i < 20; i++) sim.Tick();
        Assert.Equal(health, victim.Health);
        Assert.Equal(sim.Players.Single().Id, victim.LastDamageSourceId);
    }

    [Fact]
    public void RocketSplashCannotDamageThroughWallButCanHurtOwner()
    {
        var sim = Range(targetX: 100, wall: Wall(50));
        var projectile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.True(projectile.Destroyed);
        Assert.Equal(1000, Target(sim).Health);
        Assert.True(sim.Players.Single().Health < 100);
    }

    [Fact]
    public void ProjectileHitsFirstActorAndNeverItsOwnerDirectly()
    {
        var sim = Range(targetX: 100);
        var first = sim.AddBot(60, 0); first.Brain = null; first.Health = 1000;
        var projectile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.True(projectile.Destroyed);
        Assert.Equal(980, first.Health);
        Assert.Equal(1000, Target(sim).Health);
        Assert.Equal(100, sim.Players.Single().Health);
    }

    [Fact]
    public void ProjectileCanPassBelowElevatedActorAndExpires()
    {
        var sim = Range(targetX: 60); Target(sim).Z = Fixed.FromInt(100); Target(sim).NoGravity = true;
        var projectile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        for (var i = 0; i < 176; i++) sim.Tick();
        Assert.Equal(1000, Target(sim).Health);
        Assert.True(projectile.Destroyed);
        Assert.DoesNotContain(projectile, sim.Actors);
    }

    [Fact]
    public void ProjectileCrossingDoesNotActivateExit()
    {
        var sim = Range(wall: new LevelLine { X1 = 50, X2 = 50, Y1 = -128, Y2 = 128,
            SideFront = 0, SideBack = 0, Special = LineSpecials.ExitNormal });
        sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.False(sim.Exited);
    }

    [Theory]
    [InlineData(LevelLine.BlockSightFlag, false, false)]
    [InlineData(LevelLine.BlockHitscanFlag, true, false)]
    [InlineData(LevelLine.BlockProjectileFlag, true, true)]
    public void SightHitscanAndProjectileFlagsAreIndependent(int flags, bool visible, bool blocksProjectile)
    {
        var sim = Range(wall: new LevelLine { X1 = 50, X2 = 50, Y1 = -128, Y2 = 128, SideFront = 0, SideBack = 0, Flags = flags });
        Assert.Equal(visible, CombatTrace.HasLineOfSight(sim, sim.Players.Single(), Target(sim)));
        var projectile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(blocksProjectile, projectile.Destroyed);
    }

    [Fact]
    public void MapMonsterAcquiresPlayerAndFiresAfterWindup()
    {
        var sim = Range(targetX: 160); var monster = Target(sim);
        monster.Brain = new MonsterBrain(MonsterAttack.Fireball);
        for (var i = 0; i < 19; i++) sim.Tick();
        Assert.Equal(sim.Players.Single().Id, monster.Brain.TargetId);
        Assert.Contains(sim.Actors, actor => actor is ProjectileActor);
        for (var i = 0; i < 20; i++) sim.Tick();
        Assert.True(sim.Players.Single().Health < 100);
    }

    [Fact]
    public void MonsterCannotAcquireThroughWall()
    {
        var sim = Range(wall: Wall(100)); var monster = Target(sim);
        monster.Brain = new MonsterBrain(MonsterAttack.Hitscan);
        for (var i = 0; i < 70; i++) sim.Tick();
        Assert.Null(monster.Brain.TargetId);
        Assert.Equal(100, sim.Players.Single().Health);
        Assert.Equal(200, monster.X.ToDouble());
    }

    [Fact]
    public void MeleeMonsterChasesThenAttacksAndRetargetsAfterPlayerDeath()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = new[]
        {
            new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = -100 }, new LevelThing { Type = 3002, X = 160 },
        }});
        var monster = sim.Actors.Single(a => a.DoomEdNum == 3002);
        for (var i = 0; i < 145; i++) sim.Tick();
        var first = sim.Players.First();
        Assert.True(monster.X.ToDouble() < 160);
        Assert.True(first.Health < 100);
        first.Health = 0; sim.Tick();
        Assert.Equal(sim.Players.Last().Id, monster.Brain!.TargetId);
    }

    [Fact]
    public void PainAndDeathCancelMonsterWindup()
    {
        var sim = Range(); var monster = Target(sim);
        monster.PainChance = 256; // This test exercises pain cancellation, not the random pain gate.
        monster.Brain = new MonsterBrain(MonsterAttack.Hitscan);
        for (var i = 0; i < 11; i++) sim.Tick();
        Assert.True(monster.Brain.WindupTics > 0);
        ActorDamage.Apply(monster, 1, sim.Players.Single()); sim.Tick();
        Assert.Equal(MonsterMode.Pain, monster.Brain.Mode);
        Assert.Equal(0, monster.Brain.WindupTics);
        monster.Health = 0;
        for (var i = 0; i < 50; i++) sim.Tick();
        Assert.Equal(MonsterMode.Dead, monster.Brain.Mode);
        Assert.Equal(100, sim.Players.Single().Health);
    }

    [Fact]
    public void DamageSourceCanTriggerInfighting()
    {
        var sim = Range(); var monster = Target(sim);
        monster.Brain = new MonsterBrain(MonsterAttack.Hitscan);
        var aggressor = sim.AddBot(100, 50); aggressor.Brain = null;
        ActorDamage.Apply(monster, 1, aggressor);
        for (var i = 0; i < 11; i++) sim.Tick();
        Assert.Equal(aggressor.Id, monster.Brain.TargetId);
    }

    [Fact]
    public void PoseArchiveKeepsWeaponCooldownAndRandomState()
    {
        var sim = Range(); var player = sim.Players.Single();
        Fire(sim); sim.NextCombatRandom();
        var random = sim.CombatRandomState;
        var cooldown = player.WeaponCooldown;
        var bytes = SimSavegame.Write(sim);
        sim.NextCombatRandom(); sim.Tick();
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(random, sim.CombatRandomState);
        Assert.Equal(cooldown, player.WeaponCooldown);
        Assert.False(HitscanCombat.Fire(sim, player));
    }

    [Fact]
    public void UdmfCombatFlagsReachRuntimeLines()
    {
        var text = "linedef { blockeverything=true; blocksight=true; blockhitscan=true; blockprojectiles=true; }"u8;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var line = Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Lines);
        Assert.Equal(LevelLine.BlockEverythingFlag | LevelLine.BlockSightFlag | LevelLine.BlockHitscanFlag | LevelLine.BlockProjectileFlag, line.Flags);
    }

    [Theory]
    [InlineData(-4)]
    [InlineData(int.MaxValue)]
    public void AuditInvalidAcsJumpStopsWithoutCrashing(int destination)
    {
        var sim = Range();
        var bytes = new byte[8];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes, (int)AcsPcode.Goto);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), destination);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        sim.Acs.Enqueue(1); sim.Tick();
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void AuditTruncatedAcsSpecialCannotExitMap()
    {
        var sim = Range();
        var bytes = new byte[8];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes, (int)AcsPcode.Lspec1Direct);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), LineSpecials.ExitNormal);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        sim.Acs.Enqueue(1); sim.Tick();
        Assert.False(sim.Exited);
    }

    [Fact]
    public void SameSeedAndCommandsKeepCombatAndAiDeterministic()
    {
        var left = Range(); var right = Range();
        foreach (var sim in new[] { left, right }) Target(sim).Brain = new MonsterBrain(MonsterAttack.Fireball);
        for (var i = 0; i < 90; i++)
        {
            left.QueueCommand(0, new PlayerCommand { Attack = i % 19 == 0 });
            right.QueueCommand(0, new PlayerCommand { Attack = i % 19 == 0 });
            left.Tick(); right.Tick();
            Assert.Equal(left.Checksum, right.Checksum);
        }
    }

    private static void Fire(AuthoritySimulation sim)
    {
        sim.QueueCommand(0, new PlayerCommand { Attack = true }); sim.Tick();
    }
    private static Actor Target(AuthoritySimulation sim) => sim.Actors.Single(a => a.DoomEdNum == 3001);
    private static LevelLine Wall(int x) => new() { X1 = x, X2 = x, Y1 = -128, Y2 = 128, SideBack = -1 };
    private static AuthoritySimulation Range(short targetX = 200, LevelLine? wall = null)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = targetX } },
            Sectors = new[] { new LevelSector { CeilingHeight = 256 } },
            Sides = new[] { new LevelSide { Sector = 0 } },
            Lines = wall == null ? Array.Empty<LevelLine>() : new[] { wall },
        }, rngSeed: 42);
        Target(sim).Brain = null; Target(sim).Health = 1000;
        return sim;
    }
}
