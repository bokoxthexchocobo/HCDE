using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LagCompensatedHitscanTests
{
    [Fact]
    public void LaterPuffSurvivesHistoricalTraceWithoutBlockingCompensation()
    {
        var (sim, player, target) = Scene(); target.Y = Fixed.FromInt(200);
        var puff = sim.SpawnHitscanPuff(100, 100, 32, 77); puff.DamageSource = player;
        sim.Tick();
        var z = puff.Z; var remaining = puff.RemainingTics;
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.True(sim.Actors.Single(actor => actor.Id == target.Id).Health < 1000);
        var restored = sim.Actors.OfType<PuffActor>().Single(actor => actor.Id == puff.Id);
        Assert.True(puff.Destroyed); Assert.Equal(z, restored.Z); Assert.Equal(remaining, restored.RemainingTics);
        Assert.Equal(77, restored.ThingId); Assert.Same(player, restored.DamageSource);
        Assert.Equal(49, player.Inventory.Bullets);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OptInHitsHistoricalTargetAfterItMovesOutOfLiveAim(bool enabled)
    {
        var (sim, player, target) = Scene(); sim.Rewind.LagCompensationEnabled = enabled;
        target.Y = Fixed.FromInt(200);
        var restored = 0; sim.StateRestored += () => restored++;
        Assert.True(HitscanCombat.Fire(sim, player));
        var live = sim.Actors.Single(actor => actor.Id == target.Id);
        Assert.Equal(200, live.Y.ToDouble()); Assert.Equal(6, sim.Thinkers.Clock.Tic);
        Assert.Equal(enabled, live.Health < 1000);
        Assert.Equal(49, player.Inventory.Bullets); Assert.Equal(19, player.WeaponCooldown);
        Assert.Equal(0, restored); Assert.Equal(1, sim.Rewind.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void MissingDisabledFutureOrTooOldHistoryUsesLiveTrace(int kind)
    {
        var (sim, player, target) = Scene(); target.Y = Fixed.FromInt(200);
        if (kind == 0) sim.RewindEnabled = false;
        if (kind == 1) { sim.Rewind.Reset(); sim.Rewind.Capture(sim); }
        if (kind == 2) sim.Rewind.TryRecordClientViewTic(0, 100);
        if (kind == 3) sim.Thinkers.Clock.Restore(13);
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(1000, sim.Actors.Single(actor => actor.Id == target.Id).Health);
        Assert.Equal(49, player.Inventory.Bullets);
    }

    [Theory]
    [InlineData(12, true)]
    [InlineData(13, false)]
    public void MaximumAgeIncludesExactBoundary(int age, bool hit)
    {
        var (sim, player, target) = Scene(); target.Y = Fixed.FromInt(200);
        sim.Thinkers.Clock.Restore(age);
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(hit, sim.Actors.Single(actor => actor.Id == target.Id).Health < 1000);
    }

    [Fact]
    public void ArmorIsAppliedOnceUsingLiveArmorRatherThanHistoricalArmor()
    {
        var level = new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 200 }]
        };
        var control = AuthoritySimulation.Start(level); var sim = AuthoritySimulation.Start(level);
        var player = sim.Players.Single(player => player.PlayerNum == 0);
        var target = sim.Players.Single(player => player.PlayerNum == 1);
        sim.Rewind.Capture(sim); sim.RewindEnabled = true; sim.Rewind.LagCompensationEnabled = true;
        sim.Rewind.TryRecordClientViewTic(0, 0);
        target.Inventory.Armor = 100; target.Inventory.ArmorSavePercent = 50; target.Y = Fixed.FromInt(200);
        var controlTarget = control.Players.Single(player => player.PlayerNum == 1);
        controlTarget.Inventory.Armor = 100; controlTarget.Inventory.ArmorSavePercent = 50;
        Assert.True(HitscanCombat.Fire(control, control.Players.Single(player => player.PlayerNum == 0)));
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(controlTarget.Health, target.Health); Assert.Equal(controlTarget.Inventory.Armor, target.Inventory.Armor);
        Assert.True(target.Inventory.Armor < 100); Assert.Equal(control.CombatRandomState, sim.CombatRandomState);
    }

    [Fact]
    public void LiveInvulnerabilityStillPreventsHistoricalHitDamage()
    {
        var (sim, player, target) = Scene(); target.Y = Fixed.FromInt(200); target.Invulnerable = true;
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(1000, sim.Actors.Single(actor => actor.Id == target.Id).Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingHistoricalOrLiveTargetsDoNotDamageUnrelatedActors(bool missingLive)
    {
        var sim = Room(); var player = sim.Players.Single();
        var target = sim.AddBot(200, 0); target.Brain!.Enabled = false;
        if (!missingLive) { target.Destroy(); sim.Tick(); }
        sim.Rewind.Capture(sim); sim.RewindEnabled = true; sim.Rewind.LagCompensationEnabled = true;
        sim.Rewind.TryRecordClientViewTic(0, sim.Thinkers.Clock.Tic);
        if (missingLive) { target.Destroy(); sim.Tick(); }
        var other = sim.AddBot(400, 0); other.Brain!.Enabled = false;
        var health = other.Health;
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(health, sim.Actors.Single(actor => actor.Id == other.Id).Health);
        Assert.Equal(49, player.Inventory.Bullets);
    }

    [Fact]
    public void ShotgunPelletsPreserveLiveAmmoCooldownAndRandomProgression()
    {
        var (sim, player, target) = Scene(); var (control, controlPlayer, controlTarget) = Scene();
        control.Rewind.LagCompensationEnabled = false; target.Y = Fixed.FromInt(200);
        foreach (var shooter in new[] { player, controlPlayer })
        {
            shooter.Inventory.Weapons |= WeaponKind.Shotgun;
            shooter.Inventory.Selected = WeaponKind.Shotgun; shooter.Inventory.Shells = 10;
        }
        Assert.True(HitscanCombat.Fire(control, controlPlayer)); Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(controlTarget.Health, sim.Actors.Single(actor => actor.Id == target.Id).Health);
        Assert.Equal(9, player.Inventory.Shells); Assert.Equal(35, player.WeaponCooldown);
        Assert.Equal(control.CombatRandomState, sim.CombatRandomState);
    }

    [Fact]
    public void NewDroppedPickupSurvivesHistoricalTraceWithoutLosingAmmoOrWorldState()
    {
        var (sim, player, target) = Scene();
        Assert.True(sim.SpawnDroppedPickup(target, 2007));
        var pickup = sim.Actors.Single(actor => actor.DoomEdNum == 2007);
        target.Y = Fixed.FromInt(200);
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.True(sim.Actors.Single(actor => actor.Id == target.Id).Health < 1000);
        Assert.True(pickup.Destroyed);
        Assert.Contains(sim.Actors, actor => actor.Id == pickup.Id && actor.DoomEdNum == 2007 && !actor.Destroyed);
        Assert.Equal(49, player.Inventory.Bullets);
    }

    [Fact]
    public void MissingUnsupportedPlayerUsesLiveTrace()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 400, Y = 400 }]
        });
        var player = sim.Players.Single(player => player.PlayerNum == 0);
        var target = sim.AddBot(200, 0); target.Brain!.Enabled = false;
        sim.RewindEnabled = true; sim.Rewind.LagCompensationEnabled = true; sim.Rewind.Capture(sim);
        sim.Rewind.TryRecordClientViewTic(0, 0);
        sim.Players.Single(player => player.PlayerNum == 1).Destroy(); sim.Tick();
        var before = target.Health;
        Assert.True(HitscanCombat.Fire(sim, player)); Assert.True(target.Health < before);
        Assert.Equal(49, player.Inventory.Bullets);
    }

    [Fact]
    public void QueuedAttackUsesHistoricalTraceThroughSimulationTick()
    {
        var (sim, player, target) = Scene(); target.Y = Fixed.FromInt(200);
        sim.QueueCommand(0, new PlayerCommand { Attack = true }); sim.Tick();
        Assert.True(sim.Actors.Single(actor => actor.Id == target.Id).Health < 1000);
        Assert.Equal(49, player.Inventory.Bullets);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(100, 70)]
    public void AgeConfigurationClampsToNativeRange(int requested, int expected)
    {
        var rewind = new RewindBuffer { MaxLagCompensationAgeTics = requested };
        Assert.Equal(expected, rewind.MaxLagCompensationAgeTics);
        Assert.False(rewind.LagCompensationEnabled);
    }

    private static (AuthoritySimulation, PlayerPawn, BotPawn) Scene()
    {
        var sim = Room(); var target = sim.AddBot(200, 0); target.Brain!.Enabled = false; target.Health = 1000;
        sim.RewindEnabled = true; sim.Rewind.LagCompensationEnabled = true; sim.Rewind.Capture(sim);
        sim.Rewind.TryRecordClientViewTic(0, 0);
        for (var i = 0; i < 6; i++) sim.Tick();
        return (sim, sim.Players.Single(), target);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }]
    });
}
