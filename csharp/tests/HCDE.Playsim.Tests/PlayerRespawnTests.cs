using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerRespawnTests
{
    [Fact]
    public void SinglePlayerDeathRequestsReloadOnlyAfterTheWaitAndAFreshPress()
    {
        var sim = StartAt(96);
        var player = Kill(sim);
        player.X = Fixed.FromDouble(0);
        Assert.Equal(GameTicClock.TicRate, player.RespawnEarliestTic);
        Assert.False(player.RespawnArmed);
        Assert.False(sim.ReloadRequested);

        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        Assert.False(sim.ReloadRequested);
        Assert.True(player.IsDead);
        Assert.Equal(0, player.X.ToDouble());

        sim.QueueCommand(0, new PlayerCommand { Use = true });
        sim.Tick();
        Assert.True(sim.ReloadRequested);
        Assert.True(player.IsDead);
        Assert.Equal(0, player.X.ToDouble());
        Assert.False(player.RespawnArmed);
    }

    [Fact]
    public void AnEarlyPressIsRememberedUntilTheRespawnTic()
    {
        var sim = StartAt(32);
        var player = Kill(sim);
        sim.QueueCommand(0, new PlayerCommand { Use = true });
        sim.Tick();
        Assert.True(player.RespawnArmed);
        Assert.False(sim.ReloadRequested);

        for (var i = 0; i < GameTicClock.TicRate - 1; i++) sim.Tick();
        Assert.False(sim.ReloadRequested);
        sim.Tick();
        Assert.True(sim.ReloadRequested);
        Assert.True(player.IsDead);
    }

    [Fact]
    public void AttackHeldAcrossDeathDoesNotArmRespawn()
    {
        var sim = StartAt(32);
        sim.QueueCommand(0, new PlayerCommand { Attack = true });
        sim.Tick();
        var player = Kill(sim);
        Assert.Equal(1 + GameTicClock.TicRate, player.RespawnEarliestTic);

        for (var i = 0; i < 40; i++)
        {
            sim.QueueCommand(0, new PlayerCommand { Attack = true });
            sim.Tick();
        }

        Assert.False(player.RespawnArmed);
        Assert.False(sim.ReloadRequested);
        Assert.True(player.IsDead);
    }

    [Fact]
    public void DeathmatchRespawnReturnsToTheStartAndResetsInventory()
    {
        var sim = StartAt(128, SpawnGameMode.Deathmatch);
        var player = Assert.Single(sim.Players);
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = 8;
        player.Inventory.Armor = 100;
        player.Inventory.ArmorSavePercent = 33;
        player.Inventory.Bullets = 10;
        player.X = Fixed.FromDouble(4);
        sim.QueueCommand(0, new PlayerCommand { WeaponSelections = new byte[] { 3 } });
        sim.Tick();
        Assert.False(player.WeaponReady);
        ActorDamage.Apply(player, 1000);

        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        sim.QueueCommand(0, new PlayerCommand { Attack = true });
        sim.Tick();

        Assert.False(player.IsDead);
        Assert.False(sim.ReloadRequested);
        Assert.Equal(128, player.X.ToDouble());
        Assert.Equal(100, player.Health);
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.Equal(0, player.Inventory.Shells);
        Assert.Equal(0, player.Inventory.Armor);
        Assert.Equal(WeaponKind.Fist | WeaponKind.Pistol, player.Inventory.Weapons);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        Assert.Null(player.Inventory.Pending);
        Assert.True(player.WeaponReady);
        Assert.Equal(PlayerPawn.WeaponTop, player.WeaponOffsetY);
        Assert.Equal(PlayerPawn.StandingViewHeight, player.ViewHeight, 3);
        Assert.Equal(0, player.PitchDegrees, 3);
    }

    [Fact]
    public void PowerBuddhaClearsOnAPistolStartAndStaysWhenCoopKeepsInventory()
    {
        var deathmatch = StartAt(128, SpawnGameMode.Deathmatch);
        var dropped = Assert.Single(deathmatch.Players);
        dropped.GivePowerBuddha();
        ActorDamage.Apply(dropped, ActorDamage.TelefragDamage);
        PressRespawn(deathmatch);
        Assert.False(dropped.IsDead);
        Assert.Equal(0, dropped.PowerBuddhaTics);

        var coop = StartAt(64, SpawnGameMode.Cooperative);
        var kept = Assert.Single(coop.Players);
        kept.GivePowerBuddha();
        ActorDamage.Apply(kept, ActorDamage.TelefragDamage);
        PressRespawn(coop);
        Assert.False(kept.IsDead);
        Assert.Equal(PlayerPawn.PowerBuddhaDuration - GameTicClock.TicRate - 1, kept.PowerBuddhaTics);

        var strippedSim = StartAt(32, SpawnGameMode.Cooperative);
        strippedSim.CoopLoseInventory = true;
        var stripped = Assert.Single(strippedSim.Players);
        stripped.GivePowerBuddha();
        ActorDamage.Apply(stripped, ActorDamage.TelefragDamage);
        PressRespawn(strippedSim);
        Assert.False(stripped.IsDead);
        Assert.Equal(0, stripped.PowerBuddhaTics);
    }

    [Fact]
    public void CooperativeRespawnKeepsInventory()
    {
        var sim = StartAt(64, SpawnGameMode.Cooperative);
        var player = Assert.Single(sim.Players);
        player.Inventory.Shells = 8;
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.X = Fixed.FromDouble(0);
        ActorDamage.Apply(player, 1000);
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        sim.QueueCommand(0, new PlayerCommand { Use = true });
        sim.Tick();

        Assert.False(player.IsDead);
        Assert.Equal(64, player.X.ToDouble());
        Assert.Equal(8, player.Inventory.Shells);
        Assert.True(player.Inventory.Owns(WeaponKind.Shotgun));
    }

    [Fact]
    public void SinglePlayerRespawnCanBeEnabledWithoutReloading()
    {
        var sim = StartAt(80);
        sim.AllowSinglePlayerRespawn = true;
        var player = Assert.Single(sim.Players);
        player.Inventory.Shells = 4;
        player.X = Fixed.FromDouble(0);
        ActorDamage.Apply(player, 1000);
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        sim.QueueCommand(0, new PlayerCommand { Use = true });
        sim.Tick();

        Assert.False(sim.ReloadRequested);
        Assert.False(player.IsDead);
        Assert.Equal(80, player.X.ToDouble());
        Assert.Equal(4, player.Inventory.Shells);
    }

    [Fact]
    public void ForceRespawnRevivesDeathmatchWithoutAPress()
    {
        var sim = StartAt(48, SpawnGameMode.Deathmatch);
        sim.ForceRespawn = true;
        var player = Kill(sim);
        player.X = Fixed.FromDouble(0);
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        Assert.True(player.IsDead);
        sim.Tick();
        Assert.False(player.IsDead);
        Assert.Equal(48, player.X.ToDouble());
        Assert.False(sim.ReloadRequested);
    }

    [Fact]
    public void NoRespawnKeepsThePressUntilTheFlagClears()
    {
        var sim = StartAt(24);
        sim.NoRespawn = true;
        var player = Kill(sim);
        PressRespawn(sim);

        Assert.True(player.IsDead);
        Assert.False(sim.ReloadRequested);
        Assert.True(player.RespawnArmed);
        sim.NoRespawn = false;
        sim.Tick();
        Assert.True(sim.ReloadRequested);
        Assert.True(player.IsDead);
        Assert.False(player.RespawnArmed);
    }

    [Fact]
    public void NoRespawnBlocksForceRespawnUntilCleared()
    {
        var sim = StartAt(48, SpawnGameMode.Deathmatch);
        sim.ForceRespawn = true;
        sim.NoRespawn = true;
        var player = Kill(sim);
        player.X = Fixed.FromDouble(0);
        for (var i = 0; i < GameTicClock.TicRate + 1; i++) sim.Tick();
        Assert.True(player.IsDead);
        Assert.Equal(0, player.X.ToDouble());

        sim.NoRespawn = false;
        sim.Tick();
        Assert.False(player.IsDead);
        Assert.Equal(48, player.X.ToDouble());
        Assert.Equal(50, player.Inventory.Bullets);
    }

    [Fact]
    public void NoRespawnIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = StartAt(16);
        var right = StartAt(16);
        left.NoRespawn = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.NoRespawn = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(left.NoRespawn);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void CooperativeLoseInventoryReturnsToAPistolStart()
    {
        var sim = StartAt(32, SpawnGameMode.Cooperative);
        sim.CoopLoseInventory = true;
        var player = Assert.Single(sim.Players);
        player.Inventory.GiveBackpack();
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Selected = WeaponKind.Shotgun;
        player.Inventory.Shells = 8;
        player.Inventory.RedKey = true;
        player.Inventory.Armor = 100;
        player.Inventory.ArmorSavePercent = 33;
        player.X = Fixed.FromDouble(0);
        ActorDamage.Apply(player, 1000);
        PressRespawn(sim);

        Assert.False(player.IsDead);
        Assert.Equal(WeaponKind.Fist | WeaponKind.Pistol, player.Inventory.Weapons);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.Equal(0, player.Inventory.Shells);
        Assert.False(player.Inventory.RedKey);
        Assert.Equal(0, player.Inventory.Armor);
        Assert.False(player.Inventory.HasBackpack);
        Assert.Equal(200, player.Inventory.MaxBullets);
    }

    [Fact]
    public void CooperativeFilterDropsKeysWeaponsAndArmor()
    {
        var sim = StartAt(32, SpawnGameMode.Cooperative);
        sim.CoopLoseKeys = true;
        sim.CoopLoseWeapons = true;
        sim.CoopLoseArmor = true;
        var player = Assert.Single(sim.Players);
        player.Inventory.GiveBackpack();
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Selected = WeaponKind.Shotgun;
        player.Inventory.Bullets = 80;
        player.Inventory.Shells = 8;
        player.Inventory.RedKey = true;
        player.Inventory.Armor = 100;
        player.Inventory.ArmorSavePercent = 33;
        player.X = Fixed.FromDouble(0);
        ActorDamage.Apply(player, 1000);
        PressRespawn(sim);

        Assert.False(player.Inventory.RedKey);
        Assert.Equal(0, player.Inventory.Armor);
        Assert.Equal(0, player.Inventory.ArmorSavePercent);
        Assert.False(player.Inventory.Owns(WeaponKind.Shotgun));
        Assert.True(player.Inventory.Owns(WeaponKind.Pistol));
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        Assert.Equal(80, player.Inventory.Bullets);
        Assert.Equal(8, player.Inventory.Shells);
        Assert.True(player.Inventory.HasBackpack);
        Assert.Equal(400, player.Inventory.MaxBullets);
    }

    [Fact]
    public void CooperativeLoseWeaponsReadiesTheFistWhenThePistolHasNoBullet()
    {
        var sim = StartAt(32, SpawnGameMode.Cooperative);
        sim.CoopLoseWeapons = true;
        var player = Assert.Single(sim.Players);
        player.Inventory.Bullets = 0;
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Selected = WeaponKind.Shotgun;
        player.Inventory.Shells = 2;
        player.X = Fixed.FromDouble(0);
        ActorDamage.Apply(player, 1000);
        PressRespawn(sim);

        Assert.Equal(WeaponKind.Fist, player.Inventory.Selected);
        Assert.Equal(0, player.Inventory.Bullets);
        Assert.Equal(2, player.Inventory.Shells);
    }

    [Fact]
    public void CooperativeHalveAmmoFloorsBulletsAtTheStartAndLeavesASingleRound()
    {
        var high = StartAt(32, SpawnGameMode.Cooperative);
        high.CoopHalveAmmo = true;
        var player = Assert.Single(high.Players);
        player.Inventory.Bullets = 120;
        player.Inventory.Shells = 9;
        player.Inventory.Rockets = 1;
        player.X = Fixed.FromDouble(0);
        ActorDamage.Apply(player, 1000);
        PressRespawn(high);
        Assert.Equal(60, player.Inventory.Bullets);
        Assert.Equal(4, player.Inventory.Shells);
        Assert.Equal(1, player.Inventory.Rockets);

        var low = StartAt(32, SpawnGameMode.Cooperative);
        low.CoopHalveAmmo = true;
        player = Assert.Single(low.Players);
        player.Inventory.Bullets = 40;
        player.Inventory.Shells = 1;
        player.X = Fixed.FromDouble(0);
        ActorDamage.Apply(player, 1000);
        PressRespawn(low);
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.Equal(1, player.Inventory.Shells);
    }

    [Fact]
    public void CooperativeLoseAmmoZeroesExtraPoolsAndBeatsHalve()
    {
        var sim = StartAt(32, SpawnGameMode.Cooperative);
        sim.CoopLoseAmmo = true;
        sim.CoopHalveAmmo = true;
        var player = Assert.Single(sim.Players);
        player.Inventory.GiveBackpack();
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Selected = WeaponKind.Shotgun;
        player.Inventory.Bullets = 120;
        player.Inventory.Shells = 9;
        player.X = Fixed.FromDouble(0);
        ActorDamage.Apply(player, 1000);
        PressRespawn(sim);

        Assert.Equal(50, player.Inventory.Bullets);
        Assert.Equal(0, player.Inventory.Shells);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        Assert.True(player.Inventory.Owns(WeaponKind.Shotgun));
        Assert.True(player.Inventory.HasBackpack);
        Assert.Equal(400, player.Inventory.MaxBullets);
    }

    [Fact]
    public void SinglePlayerRespawnUsesTheCoopFilter()
    {
        var sim = StartAt(32);
        sim.AllowSinglePlayerRespawn = true;
        sim.CoopLoseWeapons = true;
        var player = Assert.Single(sim.Players);
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Selected = WeaponKind.Shotgun;
        player.Inventory.Shells = 4;
        player.X = Fixed.FromDouble(0);
        ActorDamage.Apply(player, 1000);
        PressRespawn(sim);

        Assert.False(sim.ReloadRequested);
        Assert.False(player.IsDead);
        Assert.False(player.Inventory.Owns(WeaponKind.Shotgun));
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        Assert.Equal(4, player.Inventory.Shells);
    }

    [Fact]
    public void DeathmatchIgnoresTheCoopFilter()
    {
        var sim = StartAt(32, SpawnGameMode.Deathmatch);
        sim.CoopHalveAmmo = true;
        var player = Assert.Single(sim.Players);
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = 8;
        player.Inventory.Bullets = 120;
        player.X = Fixed.FromDouble(0);
        ActorDamage.Apply(player, 1000);
        PressRespawn(sim);

        Assert.Equal(50, player.Inventory.Bullets);
        Assert.Equal(0, player.Inventory.Shells);
        Assert.False(player.Inventory.Owns(WeaponKind.Shotgun));
    }

    [Fact]
    public void CoopFilterFlagsAreInTheChecksumAndSurviveAPoseRestore()
    {
        var left = StartAt(16, SpawnGameMode.Cooperative);
        var right = StartAt(16, SpawnGameMode.Cooperative);
        left.CoopHalveAmmo = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.CoopHalveAmmo = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(left.CoopHalveAmmo);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void DeathmatchRespawnUsesADeathmatchStart()
    {
        var sim = StartWith(SpawnGameMode.Deathmatch, new LevelThing { Type = 1, X = 0 },
            new LevelThing { Type = DeathmatchStarts.ThingType, X = 80, Angle = 90 });
        sim.SpawnFarthest = true;
        var player = Assert.Single(sim.Players);
        player.X = Fixed.FromDouble(10);
        ActorDamage.Apply(player, 1000);
        PressRespawn(sim);

        Assert.False(player.IsDead);
        Assert.Equal(80, player.X.ToDouble());
        Assert.Equal(90, player.Angle.ToDegrees(), 3);
    }

    [Fact]
    public void CooperativeRespawnIgnoresDeathmatchStarts()
    {
        var sim = StartWith(SpawnGameMode.Cooperative, new LevelThing { Type = 1, X = 40 },
            new LevelThing { Type = DeathmatchStarts.ThingType, X = 80, Angle = 90 });
        var player = Assert.Single(sim.Players);
        player.X = Fixed.FromDouble(0);
        ActorDamage.Apply(player, 1000);
        PressRespawn(sim);

        Assert.Equal(40, player.X.ToDouble());
    }

    [Fact]
    public void SpawnFarthestPicksTheStartAwayFromTheLivingPlayer()
    {
        var sim = StartWith(SpawnGameMode.Deathmatch,
            new LevelThing { Type = 1, X = -40 },
            new LevelThing { Type = 2, X = 0 },
            new LevelThing { Type = DeathmatchStarts.ThingType, X = 0 },
            new LevelThing { Type = DeathmatchStarts.ThingType, X = 200, Angle = 180 });
        sim.SpawnFarthest = true;
        var dying = Assert.Single(sim.Players, player => player.PlayerNum == 0);
        Assert.Equal(0, Assert.Single(sim.Players, player => player.PlayerNum == 1).X.ToDouble());
        ActorDamage.Apply(dying, 1000);
        PressRespawn(sim);

        Assert.False(dying.IsDead);
        Assert.Equal(200, dying.X.ToDouble());
        Assert.Equal(180, dying.Angle.ToDegrees(), 3);
    }

    [Fact]
    public void RandomDeathmatchStartSkipsABlockedSpotThenKeepsTheLast()
    {
        var blocked = new LevelThing { Type = DeathmatchStarts.ThingType, X = 1 };
        var open = new LevelThing { Type = DeathmatchStarts.ThingType, X = 2 };
        var starts = new[] { blocked, open };
        var rolls = new Queue<int>(new[] { 0, 1 });
        var picked = DeathmatchStarts.PickRandom(starts, _ => rolls.Dequeue(), spot => spot.X != 1);
        Assert.Equal(2, picked.X);

        var alwaysBlocked = DeathmatchStarts.PickRandom(starts, _ => 0, _ => false);
        Assert.Equal(1, alwaysBlocked.X);
    }

    [Fact]
    public void FarthestDeathmatchStartKeepsTheEarlierTie()
    {
        var first = new LevelThing { Type = DeathmatchStarts.ThingType, X = -10 };
        var second = new LevelThing { Type = DeathmatchStarts.ThingType, X = 10 };
        var living = new[] { (0.0, 0.0) };
        Assert.Equal(-10, DeathmatchStarts.PickFarthest(new[] { first, second }, living)!.X);
        Assert.Null(DeathmatchStarts.PickFarthest(new[] { first }, Array.Empty<(double, double)>()));
    }

    [Fact]
    public void DeathmatchSpawnStreamIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = StartAt(16, SpawnGameMode.Deathmatch);
        var right = StartAt(16, SpawnGameMode.Deathmatch);
        left.DmSpawnRandomState = 1;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.DmSpawnRandomState = 1;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(1u, left.DmSpawnRandomState);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void RespawnTimerSurvivesAPoseRestoreOnTheSameActor()
    {
        var left = StartAt(16);
        var right = StartAt(16);
        Kill(left);
        Kill(right);
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);
        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(checksum, left.Checksum);
        Assert.Equal(right.Checksum, left.Checksum);
        Assert.Equal(GameTicClock.TicRate, Assert.Single(left.Players).RespawnEarliestTic);
    }

    [Fact]
    public void RespawnTelefragsABodyStandingOnTheSpot()
    {
        var sim = StartAt(64, SpawnGameMode.Cooperative);
        var player = sim.Players.Single();
        var onSpot = sim.AddBot(64, 0);
        var inside = sim.AddBot(64, 35);
        var spared = sim.AddBot(64, 36);
        var flagged = sim.AddBot(64, 0);
        flagged.NoTelefrag = true;
        var forced = sim.AddBot(64, 0);
        forced.NoTelefrag = true;
        forced.AlwaysTelefrag = true;
        var decoration = sim.AddBot(64, 0);
        decoration.Brain = null;
        var overhead = sim.AddBot(64, 0);
        overhead.NoGravity = true;
        overhead.Z = Fixed.FromDouble(player.Height.ToDouble() + 1);
        var armored = sim.AddBot(64, 0);
        armored.Invulnerable = true;
        foreach (var bot in new[] { onSpot, inside, spared, flagged, forced, overhead, armored })
        {
            bot.Brain!.Enabled = false;
            bot.Solid = false;
        }
        decoration.Solid = false;

        Kill(sim);
        PressRespawn(sim);

        Assert.True(onSpot.IsDead);
        Assert.True(inside.IsDead);
        Assert.False(spared.IsDead);
        Assert.False(flagged.IsDead);
        Assert.True(forced.IsDead);
        Assert.False(decoration.IsDead);
        Assert.False(overhead.IsDead);
        Assert.True(armored.IsDead);
        Assert.False(player.IsDead);

        var deathmatch = TwoPlayers(SpawnGameMode.Deathmatch);
        var revived = deathmatch.Players.Single(pawn => pawn.PlayerNum == 0);
        var other = deathmatch.Players.Single(pawn => pawn.PlayerNum == 1);
        other.GodMode = true;
        ActorDamage.Apply(revived, 1000);
        PressRespawn(deathmatch);
        Assert.False(revived.IsDead);
        Assert.True(other.IsDead);

        var coop = TwoPlayers(SpawnGameMode.Cooperative);
        var coopRevived = coop.Players.Single(pawn => pawn.PlayerNum == 0);
        var coopOther = coop.Players.Single(pawn => pawn.PlayerNum == 1);
        ActorDamage.Apply(coopRevived, 1000);
        PressRespawn(coop);
        Assert.False(coopRevived.IsDead);
        Assert.False(coopOther.IsDead);
        Assert.Equal(100, coopOther.Health);
    }

    [Fact]
    public void NoTelefragIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = StartAt(16, SpawnGameMode.Cooperative);
        var right = StartAt(16, SpawnGameMode.Cooperative);
        var leftBot = left.AddBot(16, 0);
        var rightBot = right.AddBot(16, 0);
        leftBot.Brain!.Enabled = false;
        rightBot.Brain!.Enabled = false;
        leftBot.NoTelefrag = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.NoTelefrag = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(leftBot.NoTelefrag);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void CooperativeShareKeysCopiesAKeyAndKeepsItWhenLoseKeysIsSet()
    {
        var sim = KeyRoom(SpawnGameMode.Cooperative);
        sim.CoopShareKeys = true;
        var giver = sim.Players.Single(player => player.PlayerNum == 0);
        var receiver = sim.Players.Single(player => player.PlayerNum == 1);
        ActorDamage.Apply(receiver, 1000);
        sim.Tick();

        Assert.True(giver.Inventory.RedKey);
        Assert.True(receiver.Inventory.RedKey);
        Assert.True(receiver.IsDead);
        Assert.Equal(60, giver.Inventory.Bullets);
        Assert.Equal(50, receiver.Inventory.Bullets);
        Assert.Equal(0, sim.Actors.Count(actor => actor.DoomEdNum == PickupCatalog.RedCard));

        var deathmatch = KeyRoom(SpawnGameMode.Deathmatch);
        deathmatch.CoopShareKeys = true;
        deathmatch.Tick();
        Assert.True(deathmatch.Players.Single(player => player.PlayerNum == 0).Inventory.RedKey);
        Assert.False(deathmatch.Players.Single(player => player.PlayerNum == 1).Inventory.RedKey);

        var kept = StartAt(32, SpawnGameMode.Cooperative);
        kept.CoopLoseKeys = true;
        kept.CoopShareKeys = true;
        var revived = kept.Players.Single();
        revived.Inventory.RedKey = true;
        ActorDamage.Apply(revived, 1000);
        PressRespawn(kept);
        Assert.True(revived.Inventory.RedKey);

        var wiped = StartAt(32, SpawnGameMode.Cooperative);
        wiped.CoopLoseInventory = true;
        wiped.CoopShareKeys = true;
        var stripped = wiped.Players.Single();
        stripped.Inventory.RedKey = true;
        ActorDamage.Apply(stripped, 1000);
        PressRespawn(wiped);
        Assert.False(stripped.Inventory.RedKey);
    }

    [Fact]
    public void CoopShareKeysIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = StartAt(16, SpawnGameMode.Cooperative);
        var right = StartAt(16, SpawnGameMode.Cooperative);
        left.CoopShareKeys = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.CoopShareKeys = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(left.CoopShareKeys);
        Assert.Equal(checksum, left.Checksum);
    }

    private static AuthoritySimulation KeyRoom(SpawnGameMode mode) =>
        AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = new[] { new LevelSector { CeilingHeight = 128 } },
            Things = new[]
            {
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = 2, X = 400, Y = 64 },
                new LevelThing { Type = PickupCatalog.RedCard, X = 32, Y = 64 },
                new LevelThing { Type = PickupCatalog.Clip, X = 32, Y = 64 },
            },
        }, spawnOptions: new SpawnOptions(Mode: mode));

    private static AuthoritySimulation TwoPlayers(SpawnGameMode mode) =>
        AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = new[] { new LevelSector { CeilingHeight = 128 } },
            Things = new[]
            {
                new LevelThing { Type = 1, X = 32, Single = true, Coop = true, Deathmatch = true },
                new LevelThing { Type = 2, X = 32, Single = true, Coop = true, Deathmatch = true },
            },
        }, spawnOptions: new SpawnOptions(Mode: mode));

    private static AuthoritySimulation StartWith(SpawnGameMode mode, params LevelThing[] things) =>
        AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = new[] { new LevelSector { CeilingHeight = 128 } },
            Things = things,
        }, spawnOptions: new SpawnOptions(Mode: mode));

    private static AuthoritySimulation StartAt(double x, SpawnGameMode mode = SpawnGameMode.Single) =>
        AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = new[] { new LevelSector { CeilingHeight = 128 } },
            Things = new[] { new LevelThing { Type = 1, X = x, Single = true, Coop = true, Deathmatch = true } },
        }, spawnOptions: new SpawnOptions(Mode: mode));

    private static void PressRespawn(AuthoritySimulation sim)
    {
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        sim.QueueCommand(0, new PlayerCommand { Use = true });
        sim.Tick();
    }

    private static PlayerPawn Kill(AuthoritySimulation sim)
    {
        var player = Assert.Single(sim.Players);
        ActorDamage.Apply(player, 1000);
        Assert.True(player.IsDead);
        return player;
    }
}
