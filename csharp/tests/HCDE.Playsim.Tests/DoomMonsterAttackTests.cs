using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoomMonsterAttackTests
{
    [Theory]
    [InlineData(3001, ProjectileKind.ImpBall, 16)]
    [InlineData(3003, ProjectileKind.BaronBall, 16)]
    [InlineData(69, ProjectileKind.BaronBall, 16)]
    [InlineData(3005, ProjectileKind.CacodemonBall, 10)]
    [InlineData(66, ProjectileKind.RevenantTracer, 10)]
    [InlineData(68, ProjectileKind.ArachnotronPlasma, 20)]
    [InlineData(16, ProjectileKind.CyberRocket, 6)]
    public void MissileActionUsesNativeDelayAndProjectile(int type, ProjectileKind kind, int delay)
    {
        var (sim, monster) = Range(type);
        EnterAttack(sim, monster);
        for (var tic = 1; tic < delay; tic++) sim.Tick();
        Assert.Empty(sim.Actors.OfType<ProjectileActor>());
        sim.Tick();
        var missile = Assert.Single(sim.Actors.OfType<ProjectileActor>());
        Assert.Equal(kind, missile.Kind);
        Assert.Same(monster, missile.Owner);
    }

    [Theory]
    [InlineData(16, 6, 30, 54)]
    [InlineData(68, 20, 29, 38)]
    public void VolleyAndRefireKeepNativeSpacing(int type, int first, int second, int third)
    {
        var (sim, monster) = Range(type);
        EnterAttack(sim, monster);
        var seen = new HashSet<uint>();
        var shots = new List<int>();
        for (var tic = 1; tic <= third; tic++)
        {
            sim.Tick();
            foreach (var missile in sim.Actors.OfType<ProjectileActor>())
                if (seen.Add(missile.Id)) shots.Add(tic);
        }
        Assert.Equal(new[] { first, second, third }, shots);
    }

    [Fact]
    public void MancubusFiresThreePairsWithDistinctSpread()
    {
        var (sim, monster) = Range(67);
        EnterAttack(sim, monster);
        var seen = new HashSet<uint>();
        var shots = new List<(int Tic, double Angle)>();
        for (var tic = 1; tic <= 60; tic++)
        {
            sim.Tick();
            foreach (var missile in sim.Actors.OfType<ProjectileActor>())
                if (seen.Add(missile.Id)) shots.Add((tic, missile.Angle.Raw * 180.0 / 0x80000000u));
        }
        Assert.Equal(new[] { 20, 20, 40, 40, 60, 60 }, shots.Select(shot => shot.Tic));
        Assert.Equal(new[] { 0, 11.25, 0, 348.75, 354.375, 5.625 }, shots.Select(shot => shot.Angle));
    }

    [Theory]
    [InlineData(ProjectileKind.ImpBall, 10, 6, 8, 3)]
    [InlineData(ProjectileKind.BaronBall, 15, 6, 16, 8)]
    [InlineData(ProjectileKind.CacodemonBall, 10, 6, 8, 5)]
    [InlineData(ProjectileKind.CyberRocket, 20, 6, 8, 20)]
    [InlineData(ProjectileKind.ArachnotronPlasma, 25, 13, 8, 5)]
    [InlineData(ProjectileKind.MancubusBall, 20, 6, 8, 8)]
    [InlineData(ProjectileKind.RevenantTracer, 10, 11, 8, 10)]
    public void ProjectileDefaultsMatchNativeDefinitions(ProjectileKind kind, int speed, int radius, int height, int damage)
    {
        var missile = new ProjectileActor(new Actor(), kind);
        Assert.Equal(speed, missile.Speed);
        Assert.Equal(radius, missile.Radius.ToDouble());
        Assert.Equal(height, missile.Height.ToDouble());
        Assert.Equal(damage, missile.ImpactDamage);
        Assert.InRange(missile.DoomEdNum, 0, ushort.MaxValue);
        Assert.Equal(Enum.GetValues<ProjectileKind>().Length,
            Enum.GetValues<ProjectileKind>().Select(value => new ProjectileActor(new Actor(), value).DoomEdNum).Distinct().Count());
    }

    [Fact]
    public void TracerTurnsOnlyOnFourTicBoundaryAndStopsTrackingDeadTarget()
    {
        var (sim, monster) = Range(66);
        monster.Brain = null;
        sim.Tick(); // Advance past tic zero before creating the tracer.
        var player = sim.Players.Single();
        var missile = sim.SpawnProjectile(monster, ProjectileKind.RevenantTracer, player);
        var initialZSpeed = missile.VelocityZ.ToDouble();
        player.Y = Fixed.FromInt(800);
        for (var tic = 0; tic < 3; tic++) sim.Tick();
        Assert.Equal(0u, missile.Angle.Raw);
        sim.Tick();
        Assert.Equal(16.875, missile.Angle.Raw * 180.0 / 0x80000000u);
        Assert.Equal(initialZSpeed + 0.125, missile.VelocityZ.ToDouble());
        player.Health = 0;
        for (var tic = 0; tic < 4; tic++) sim.Tick();
        Assert.Equal(16.875, missile.Angle.Raw * 180.0 / 0x80000000u);
    }

    [Theory]
    [InlineData(16)]
    [InlineData(7)]
    public void BossIgnoresSplashButStillTakesDirectImpact(int type)
    {
        var (sim, boss) = Range(type);
        boss.Brain = null;
        var player = sim.Players.Single();
        player.X = Fixed.FromInt(-200);
        player.Angle = default;
        var missile = sim.SpawnProjectile(player, ProjectileKind.Rocket, boss);
        var health = boss.Health;
        for (var tic = 0; tic < 20 && !missile.Destroyed; tic++) sim.Tick();
        Assert.True(missile.Destroyed);
        Assert.True(boss.NoRadiusDamage);
        Assert.InRange(health - boss.Health, 20, 160);
        Assert.Equal(0, (health - boss.Health) % 20);
    }

    [Fact]
    public void PainInterruptsPendingVolley()
    {
        var (sim, monster) = Range(16);
        EnterAttack(sim, monster);
        for (var tic = 0; tic < 6; tic++) sim.Tick();
        var first = Assert.Single(sim.Actors.OfType<ProjectileActor>()).Id;
        monster.PainChance = 256;
        ActorDamage.Apply(monster, 1);
        sim.Tick();
        Assert.Equal(MonsterMode.Pain, monster.Brain!.Mode);
        Assert.Equal(0, monster.Brain.WindupTics);
        Assert.All(sim.Actors.OfType<ProjectileActor>(), missile => Assert.Equal(first, missile.Id));
    }

    [Theory]
    [InlineData(3002)]
    [InlineData(58)]
    public void DemonBitesAfterSixteenTicsAndCannotHitFleeingTarget(int type)
    {
        foreach (var flee in new[] { false, true })
        {
            var (sim, monster) = Range(type);
            var player = sim.Players.Single();
            player.X = Fixed.FromInt(60); player.Invulnerable = false;
            EnterAttack(sim, monster);
            for (var tic = 0; tic < 15; tic++) sim.Tick();
            Assert.Equal(100, player.Health);
            if (flee) player.X = Fixed.FromInt(300);
            sim.Tick();
            if (flee) Assert.Equal(100, player.Health);
            else
            {
                Assert.InRange(100 - player.Health, 4, 40);
                Assert.Equal(0, (100 - player.Health) % 4);
            }
            Assert.Empty(sim.Actors.OfType<ProjectileActor>());
        }
    }

    [Theory]
    [InlineData(3004, 10, 1)]
    [InlineData(9, 10, 3)]
    [InlineData(65, 10, 1)]
    [InlineData(84, 20, 1)]
    [InlineData(7, 20, 3)]
    public void HitscanMonstersUseTheirBulletCountAndNativeDamage(int type, int delay, int pellets)
    {
        var (sim, monster) = Range(type);
        var player = sim.Players.Single();
        // A large target catches all three pellets without overlapping the attacker.
        player.X = Fixed.FromInt(250); player.Radius = Fixed.FromInt(100); player.Invulnerable = false;
        EnterAttack(sim, monster);
        for (var tic = 0; tic < delay - 1; tic++) sim.Tick();
        Assert.Equal(100, player.Health);
        sim.Tick();
        Assert.InRange(100 - player.Health, pellets * 3, pellets * 15);
        Assert.Equal(0, (100 - player.Health) % 3);
        Assert.Empty(sim.Actors.OfType<ProjectileActor>());
    }

    [Fact]
    public void MonsterMissileUsesNativeSpawnOffsetAndNormalizedSourceDirection()
    {
        var (sim, monster) = Range(3003);
        var player = sim.Players.Single();
        player.Z = Fixed.FromInt(400);
        var missile = sim.SpawnProjectile(monster, ProjectileKind.BaronBall, player);
        Assert.Equal(32, missile.Z.ToDouble());
        var vx = missile.VelocityX.ToDouble(); var vz = missile.VelocityZ.ToDouble();
        Assert.InRange(Math.Sqrt(vx * vx + vz * vz), 14.9999, 15.0001);
        Assert.InRange(vz / vx, 0.49999, 0.50001);
    }

    [Fact]
    public void ProfileCombatRemainsDeterministicAcrossVolleysAndPain()
    {
        var (left, leftMonster) = Range(67);
        var (right, rightMonster) = Range(67);
        for (var tic = 0; tic < 180; tic++)
        {
            if (tic == 80) { ActorDamage.Apply(leftMonster, 1); ActorDamage.Apply(rightMonster, 1); }
            left.Tick(); right.Tick();
            Assert.Equal(left.Checksum, right.Checksum);
            Assert.Equal(left.CombatRandomState, right.CombatRandomState);
        }
    }

    [Fact]
    public void BfgSprayKeepsMissileYawAfterOwnerTurns()
    {
        var states = new List<(int Health, uint Random)>();
        foreach (var turn in new[] { false, true })
        {
            var (sim, monster) = Range(3003);
            monster.Brain = null; monster.Health = 10000; monster.PainChance = 0;
            var player = sim.Players.Single();
            player.X = Fixed.FromInt(-200); player.Angle = default;
            var missile = sim.SpawnProjectile(player, ProjectileKind.Bfg);
            if (turn) player.Angle = BamAngle.FromDegrees(90);
            for (var tic = 0; tic < 20 && !missile.Destroyed; tic++) sim.Tick();
            Assert.True(missile.Destroyed);
            Assert.True(monster.Health < 9200); // Includes spray, beyond maximum direct damage.
            states.Add((monster.Health, sim.CombatRandomState));
        }
        Assert.Equal(states[0], states[1]);
    }

    private static void EnterAttack(AuthoritySimulation sim, Actor monster)
    {
        for (var tic = 0; tic < 20 && monster.Brain!.Mode != MonsterMode.Windup; tic++) sim.Tick();
        Assert.Equal(MonsterMode.Windup, monster.Brain!.Mode);
    }

    private static (AuthoritySimulation, Actor) Range(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = type }, new LevelThing { Type = 1, X = 800 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Sides = [new LevelSide { Sector = 0 }],
        }, rngSeed: 42);
        sim.Players.Single().Invulnerable = true;
        return (sim, sim.Actors.Single(actor => actor.DoomEdNum == type));
    }
}
