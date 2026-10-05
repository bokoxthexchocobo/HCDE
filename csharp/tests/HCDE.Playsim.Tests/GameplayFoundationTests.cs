using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GameplayFoundationTests
{
    [Fact]
    public void MomentumCoastsAndFrictionEventuallyStopsIt()
    {
        var sim = Room();
        var player = sim.Players.Single();
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick();
        Assert.Equal(1, player.X.ToDouble());
        Assert.Equal(ActorPhysics.GroundFriction, player.VelocityX.ToDouble());
        sim.Tick();
        Assert.True(player.X.ToDouble() > 1);
        for (var i = 0; i < 100; i++) sim.Tick();
        Assert.Equal(0, player.VelocityX.Raw);
    }

    [Fact]
    public void JumpFallsBackToFloorAndCannotJumpAgainInMidair()
    {
        var sim = Room();
        var player = sim.Players.Single();
        sim.QueueCommand(0, new PlayerCommand { Jump = true });
        sim.Tick();
        Assert.Equal(7, player.Z.ToDouble());
        Assert.False(player.OnGround);
        sim.QueueCommand(0, new PlayerCommand { Jump = true });
        sim.Tick();
        Assert.Equal(6, player.VelocityZ.ToDouble());
        for (var i = 0; i < 20; i++) sim.Tick();
        Assert.True(player.OnGround);
        Assert.Equal(0, player.Z.Raw);
        Assert.Equal(0, player.VelocityZ.Raw);
    }

    [Fact]
    public void CeilingStopsUpwardVelocity()
    {
        var sim = Room(ceiling: 60);
        var player = sim.Players.Single();
        sim.QueueCommand(0, new PlayerCommand { Jump = true });
        sim.Tick();
        Assert.Equal(4, player.Z.ToDouble());
        Assert.Equal(0, player.VelocityZ.Raw);
        Assert.False(player.OnGround);
    }

    [Fact]
    public void NoGravityActorKeepsItsAltitude()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Z = Fixed.FromInt(32);
        player.NoGravity = true;
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.Equal(32, player.Z.ToDouble());
        Assert.False(player.OnGround);
    }

    [Fact]
    public void FastMotionStopsAtWallAndSlidesAlongIt()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1 } },
            Lines = new[] { new LevelLine { X1 = 24, X2 = 24, Y1 = -128, Y2 = 128, SideBack = -1 } },
        });
        var player = sim.Players.Single();
        player.VelocityX = Fixed.FromInt(1000);
        player.VelocityY = Fixed.FromInt(20);
        sim.Tick();
        Assert.InRange(player.X.ToDouble(), 0, 8);
        Assert.InRange(player.Y.ToDouble(), 19.99, 20.01);
        Assert.Equal(0, player.VelocityX.Raw);
    }

    [Fact]
    public void CornerSlideFollowsTheNearestWall()
    {
        // East wall is listed first. The center reaches the north wall sooner, so
        // FSlide::SlideMove clips Y and still advances along that wall.
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = new[] { new LevelSector { CeilingHeight = 128 } },
            Things = new[] { new LevelThing { Type = 1, X = 22.5, Y = 23.5 } },
            Lines = new[]
            {
                new LevelLine { X1 = 40, Y1 = -128, X2 = 40, Y2 = 128, SideBack = -1 },
                new LevelLine { X1 = -128, Y1 = 40, X2 = 40, Y2 = 40, SideBack = -1 },
            },
        });
        var player = sim.Players.Single();
        player.VelocityX = Fixed.FromDouble(6);
        player.VelocityY = Fixed.FromDouble(6);
        sim.Tick();
        Assert.InRange(player.X.ToDouble(), 22.75, 24);
        Assert.InRange(player.Y.ToDouble(), 23.7, 24);
        Assert.True(player.X.ToDouble() < 24);
    }

    [Theory]
    [InlineData(24, 128, true)]
    [InlineData(25, 128, false)]
    [InlineData(0, 55, false)]
    public void SectorOpeningEnforcesStepHeightAndHeadroom(short floor, short ceiling, bool pass)
    {
        var sim = TwoRooms(floor, ceiling);
        var player = sim.Players.Single();
        for (var i = 0; i < 20; i++)
        {
            sim.QueueCommand(0, new PlayerCommand { ForwardMove = 16384 });
            sim.Tick();
        }
        Assert.Equal(pass, player.X.ToDouble() > 0);
        Assert.Equal(pass ? floor : 0, player.Z.ToDouble());
        Assert.Equal(pass ? 1 : 0, player.SectorIndex);
    }

    [Theory]
    [InlineData(24, false, false, true)]
    [InlineData(25, false, false, false)]
    [InlineData(48, true, false, true)]
    [InlineData(48, false, true, true)]
    public void MonsterDropOffMatchesNativeLimit(int drop, bool allowDropOff, bool floating, bool enters)
    {
        var sim = TwoRooms((short)-drop, 128);
        var monster = sim.AddBot(-1, 80);
        monster.Brain!.Enabled = false;
        monster.AllowDropOff = allowDropOff;
        monster.Floating = floating;
        monster.VelocityX = Fixed.FromInt(4);
        sim.Tick();
        Assert.Equal(enters, monster.SectorIndex == 1);
        if (!enters)
        {
            Assert.Equal(0, monster.Z.ToDouble());
            Assert.Equal(0, monster.SectorIndex);
        }
    }

    [Fact]
    public void PlayerMayWalkOffATallLedge()
    {
        var sim = TwoRooms(-48, 128);
        var player = sim.Players.Single();
        player.X = Fixed.FromInt(-1);
        player.VelocityX = Fixed.FromInt(4);
        sim.Tick();
        Assert.Equal(1, player.SectorIndex);
        Assert.False(player.OnGround);
    }

    [Fact]
    public void DropOffFallsInsteadOfSnappingDown()
    {
        var sim = TwoRooms(-24, 128);
        var player = sim.Players.Single();
        player.X = Fixed.FromInt(-1);
        player.VelocityX = Fixed.FromInt(4);
        sim.Tick();
        Assert.Equal(1, player.SectorIndex);
        Assert.Equal(-1, player.Z.ToDouble());
        Assert.False(player.OnGround);
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.Equal(-24, player.Z.ToDouble());
    }

    [Fact]
    public void CrouchShrinksToHalfHeightAndStandsBackUp()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var standing = player.Height.ToDouble();
        for (var i = 0; i < 6; i++)
        {
            sim.QueueCommand(0, new PlayerCommand { Crouch = true });
            sim.Tick();
        }

        Assert.Equal(PlayerPawn.MinimumCrouchFactor, player.CrouchFactor, 3);
        Assert.Equal(standing / 2, player.Height.ToDouble(), 3);
        Assert.Equal(PlayerPawn.StandingViewHeight / 2, player.ViewHeight, 3);

        for (var i = 0; i < 6; i++) sim.Tick();
        Assert.Equal(1, player.CrouchFactor, 3);
        Assert.Equal(standing, player.Height.ToDouble(), 3);
        Assert.Equal(PlayerPawn.StandingViewHeight, player.ViewHeight, 3);
    }

    [Fact]
    public void JumpWhileCrouchedStandsUpWithoutLeavingTheGround()
    {
        var sim = Room();
        var player = sim.Players.Single();
        for (var i = 0; i < 6; i++)
        {
            sim.QueueCommand(0, new PlayerCommand { Crouch = true });
            sim.Tick();
        }

        sim.QueueCommand(0, new PlayerCommand { Jump = true });
        sim.Tick();
        Assert.Equal(0, player.VelocityZ.ToDouble());
        Assert.True(player.OnGround);
        Assert.True(player.CrouchFactor > PlayerPawn.MinimumCrouchFactor);
        Assert.True(player.UncrouchLocked);
    }

    [Fact]
    public void LowCeilingBlocksUncrouch()
    {
        var sim = TwoRooms(0, 30);
        var player = sim.Players.Single();
        player.X = Fixed.FromInt(-40);
        for (var i = 0; i < 6; i++)
        {
            sim.QueueCommand(0, new PlayerCommand { Crouch = true });
            sim.Tick();
        }

        var crouched = player.Height.ToDouble();
        player.X = Fixed.FromInt(1);
        sim.Tick();
        Assert.Equal(crouched, player.Height.ToDouble(), 3);
        Assert.Equal(PlayerPawn.MinimumCrouchFactor, player.CrouchFactor, 3);
    }

    [Fact]
    public void DeathRestoresStandingHeight()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var standing = player.Height.ToDouble();
        for (var i = 0; i < 6; i++)
        {
            sim.QueueCommand(0, new PlayerCommand { Crouch = true });
            sim.Tick();
        }

        ActorDamage.Apply(player, 1000);
        sim.Tick();
        Assert.Equal(standing, player.Height.ToDouble(), 3);
        Assert.Equal(1, player.CrouchFactor, 3);
    }

    [Fact]
    public void DrainReturnsHalfThePostArmorHitUpToMaxHealth()
    {
        var source = new PlayerPawn { Health = 40, DrainStrength = 0.5 };
        var monster = new Actor { Health = 80 };
        ActorDamage.Apply(monster, 80, source);
        Assert.Equal(0, monster.Health);
        Assert.Equal(80, source.Health);

        source.Health = 50;
        var armored = new Actor { Health = 100, Armor = 100, ArmorSavePercent = PlayerInventory.GreenSavePercent };
        ActorDamage.Apply(armored, 80, source);
        Assert.Equal(77, source.Health);

        var other = new PlayerPawn { Health = 50 };
        source.Health = 95;
        ActorDamage.Apply(other, 20, source);
        Assert.Equal(100, source.Health);
        Assert.Equal(30, other.Health);

        source.Health = 150;
        ActorDamage.Apply(new Actor { Health = 40 }, 20, source);
        Assert.Equal(150, source.Health);

        var blocked = new Actor { Health = 40, DontDrain = true };
        source.Health = 40;
        ActorDamage.Apply(blocked, 20, source);
        Assert.Equal(40, source.Health);
        Assert.Equal(20, blocked.Health);

        ActorDamage.Apply(source, 10, source);
        Assert.Equal(30, source.Health);
    }

    [Fact]
    public void DrainStrengthIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.Players.Single().DrainStrength = 0.5;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Players.Single().DrainStrength = 0.5;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(0.5, left.Players.Single().DrainStrength);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void WeaponDropSpawnsTheSelectedMapThingAndLeavesTheInventory()
    {
        var quiet = Room();
        var quietPlayer = quiet.Players.Single();
        quietPlayer.Inventory.Weapons |= WeaponKind.Shotgun;
        quietPlayer.Inventory.Selected = WeaponKind.Shotgun;
        var before = quiet.Actors.Count;
        ActorDamage.Apply(quietPlayer, 1000);
        Assert.Equal(before, quiet.Actors.Count);

        var sim = Room();
        sim.WeaponDrop = true;
        var player = sim.Players.Single();
        player.X = Fixed.FromInt(24);
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Selected = WeaponKind.Shotgun;
        ActorDamage.Apply(player, 1000);
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Shotgun);
        Assert.Equal(24, drop.X.ToDouble());
        Assert.True(player.Inventory.Owns(WeaponKind.Shotgun));
        Assert.False(drop.Solid);

        var pistol = Room();
        pistol.WeaponDrop = true;
        var count = pistol.Actors.Count;
        ActorDamage.Apply(pistol.Players.Single(), 1000);
        Assert.Equal(count + 1, pistol.Actors.Count);
        var pistolDrop = Assert.Single(pistol.Actors, actor => actor.DoomEdNum == PickupCatalog.Pistol);
        Assert.True(pistolDrop.IgnoreAmmoSkill);
        Assert.False(pistolDrop.SuppressWeaponPickupAmmo);
        Assert.True(pistol.Players.Single().Inventory.Owns(WeaponKind.Pistol));
    }

    [Fact]
    public void WeaponDropFlagIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.WeaponDrop = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.WeaponDrop = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(left.WeaponDrop);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void DeadPlayerTurnsTowardTheKillerAndLowersTheView()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var killer = sim.AddBot(0, 64);
        killer.Brain = null;
        player.PitchDegrees = 10;
        ActorDamage.Apply(player, 1000, killer);
        sim.Tick();
        Assert.Equal(PlayerPawn.DeathTurnStep, player.Angle.ToDegrees(), 3);
        Assert.Equal(PlayerPawn.StandingViewHeight - 1, player.ViewHeight, 3);
        Assert.Equal(10 - PlayerPawn.DeathPitchStep, player.PitchDegrees, 3);

        for (var i = 0; i < 17; i++) sim.Tick();
        Assert.Equal(90, player.Angle.ToDegrees(), 3);
        Assert.Equal(0, player.PitchDegrees, 3);
        for (var i = 0; i < 20; i++) sim.Tick();
        Assert.Equal(PlayerPawn.DeathViewHeight, player.ViewHeight, 3);
        Assert.Equal(90, player.Angle.ToDegrees(), 3);
    }

    [Fact]
    public void DeathWithoutAKillerDoesNotTurnAndIceSkipsTheViewDrop()
    {
        var plain = Room();
        var player = plain.Players.Single();
        player.Angle = BamAngle.FromDegrees(40);
        ActorDamage.Apply(player, 1000, player);
        plain.Tick();
        Assert.Equal(40, player.Angle.ToDegrees(), 3);
        Assert.Equal(PlayerPawn.StandingViewHeight - 1, player.ViewHeight, 3);

        var frozen = Room();
        var ice = frozen.Players.Single();
        var killer = frozen.AddBot(0, 64);
        killer.Brain = null;
        ice.IceCorpse = true;
        ice.PitchDegrees = 10;
        ActorDamage.Apply(ice, 1000, killer);
        frozen.Tick();
        Assert.Equal(PlayerPawn.DeathTurnStep, ice.Angle.ToDegrees(), 3);
        Assert.Equal(PlayerPawn.StandingViewHeight, ice.ViewHeight, 3);
        Assert.Equal(10, ice.PitchDegrees, 3);

        var checksum = frozen.Checksum;
        frozen.RestoreState(frozen.CaptureState());
        Assert.Equal(PlayerPawn.StandingViewHeight, ice.ViewHeight, 3);
        Assert.Equal(checksum, frozen.Checksum);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(64, true)]
    public void SweptActorCollisionRespectsVerticalSeparation(int targetZ, bool passes)
    {
        var sim = Room();
        var player = sim.Players.Single();
        var target = sim.AddBot(10, 0);
        target.Radius = player.Radius = Fixed.FromInt(1);
        target.Z = Fixed.FromInt(targetZ);
        target.NoGravity = true;
        player.VelocityX = Fixed.FromInt(30);
        sim.Tick();
        Assert.Equal(passes, player.X.ToDouble() > target.X.ToDouble());
    }

    [Theory]
    [InlineData(24, true)]
    [InlineData(25, false)]
    public void PlayerStepsOntoASolidActorWithinMaxStepHeight(int height, bool stepsUp)
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Radius = Fixed.FromInt(16);
        var platform = sim.AddBot(36, 0);
        platform.Brain = null;
        platform.Radius = Fixed.FromInt(16);
        platform.Height = Fixed.FromInt(height);
        player.VelocityX = Fixed.FromInt(8);
        sim.Tick();
        Assert.Equal(stepsUp, player.Z.ToDouble() == height);
        Assert.Equal(stepsUp, player.X.ToDouble() > 6);
        if (!stepsUp)
        {
            Assert.Equal(0, player.Z.ToDouble());
            Assert.True(player.X.ToDouble() < 5);
        }
    }

    [Fact]
    public void PlayerLandsOnAnActorInsteadOfTheFloor()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var platform = sim.AddBot(0, 0);
        platform.Brain = null;
        platform.Height = Fixed.FromInt(16);
        player.Z = Fixed.FromInt(64);
        for (var i = 0; i < 20; i++) sim.Tick();
        Assert.Equal(16, player.Z.ToDouble());
        Assert.Equal(0, player.VelocityZ.ToDouble());
        Assert.True(player.OnGround);
    }

    [Fact]
    public void SectorPinchCrushesEveryFourTics()
    {
        var sim = Room(ceiling: 48);
        var player = sim.Players.Single();
        Assert.Equal(100, player.Health);
        for (var tic = 0; tic < 3; tic++)
        {
            sim.Tick();
            Assert.Equal(100, player.Health);
        }
        sim.Tick();
        Assert.Equal(90, player.Health);
        sim.Tick();
        Assert.Equal(90, player.Health);
    }

    [Fact]
    public void MoverCrushDamagesActorsStandingOnTheCarrier()
    {
        var sim = Room();
        var platform = sim.AddBot(0, 0);
        platform.Brain = null;
        platform.Height = Fixed.FromInt(16);
        var player = sim.Players.Single();
        player.Z = Fixed.FromInt(16);
        player.VelocityZ = default;
        var tic = sim.Thinkers.Clock.Tic;
        ActorPhysics.CrushStandingRiders(sim, platform, 10, tic);
        Assert.Equal(90, player.Health);
        Assert.Equal(20, platform.Health);
    }

    [Fact]
    public void HorizontalCarryMovesRidersWithTheCarrier()
    {
        var sim = Room();
        var platform = sim.AddBot(0, 64);
        platform.Brain = null;
        platform.Radius = Fixed.FromInt(16);
        platform.Height = Fixed.FromInt(16);
        var player = sim.Players.Single();
        player.Radius = Fixed.FromInt(16);
        player.X = platform.X;
        player.Y = platform.Y;
        player.Z = Fixed.FromInt(16);
        player.VelocityZ = default;
        player.VelocityX = default;
        platform.VelocityX = Fixed.FromInt(8);
        sim.Tick();
        Assert.True(platform.X.ToDouble() > 0);
        Assert.Equal(platform.X.Raw, player.X.Raw);
        Assert.True(player.OnMobj);
    }

    [Fact]
    public void OnMobjIsInTheChecksum()
    {
        var left = Room();
        var right = Room();
        var platform = left.AddBot(0, 64);
        platform.Brain = null;
        platform.Height = Fixed.FromInt(16);
        var rider = left.Players.Single();
        rider.Z = Fixed.FromInt(16);
        rider.X = platform.X;
        rider.Y = platform.Y;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        var rightPlatform = right.AddBot(0, 64);
        rightPlatform.Brain = null;
        rightPlatform.Height = Fixed.FromInt(16);
        var rightRider = right.Players.Single();
        rightRider.Z = Fixed.FromInt(16);
        rightRider.X = rightPlatform.X;
        rightRider.Y = rightPlatform.Y;
        right.Tick();
        left.Tick();
        Assert.Equal(left.Checksum, right.Checksum);
    }

    [Fact]
    public void SectorPinchCrushDamagesActorsStandingOnThePinchedActor()
    {
        var sim = Room(ceiling: 32);
        var platform = sim.AddBot(32, 0);
        platform.Brain = null;
        platform.Health = 100;
        platform.Height = Fixed.FromInt(40);
        var player = sim.Players.Single();
        player.Health = 100;
        player.X = platform.X;
        player.Z = Fixed.FromInt(40);
        player.VelocityZ = default;
        for (var tic = 0; tic < 4; tic++)
            sim.Tick();
        Assert.Equal(90, platform.Health);
        Assert.Equal(90, player.Health);
    }

    [Fact]
    public void SectorPinchCrushSkipsDeadAndNonShootableActors()
    {
        var sim = Room(ceiling: 48);
        var dead = sim.AddBot(32, 0);
        dead.Health = 0;
        dead.Brain = null;
        var decoration = sim.AddBot(64, 0);
        decoration.Brain = null;
        decoration.Shootable = false;
        for (var tic = 0; tic < 4; tic++)
            sim.Tick();
        Assert.Equal(0, dead.Health);
        Assert.Equal(20, decoration.Health);
    }

    [Fact]
    public void LowCeilingRejectsAStepOntoAnActor()
    {
        var sim = Room(70);
        var player = sim.Players.Single();
        player.Radius = Fixed.FromInt(16);
        var platform = sim.AddBot(36, 0);
        platform.Brain = null;
        platform.Radius = Fixed.FromInt(16);
        platform.Height = Fixed.FromInt(16);
        player.VelocityX = Fixed.FromInt(8);
        sim.Tick();
        Assert.Equal(0, player.Z.ToDouble());
        Assert.True(player.X.ToDouble() < 5);
    }

    [Fact]
    public void MonsterDoesNotStepOntoAnotherActor()
    {
        var sim = Room();
        var walker = sim.AddBot(0, 80);
        var platform = sim.AddBot(20, 80);
        walker.Brain = platform.Brain = null;
        walker.Radius = platform.Radius = Fixed.FromInt(16);
        platform.Height = Fixed.FromInt(16);
        walker.VelocityX = Fixed.FromInt(8);
        sim.Tick();
        Assert.Equal(0, walker.Z.ToDouble());
        Assert.Equal(0, walker.X.ToDouble());
    }

    [Theory]
    [InlineData(16, true)]
    [InlineData(24, true)]
    [InlineData(25, false)]
    public void MonsterStepsOntoABridgeWithinMaxStepHeight(int height, bool steps)
    {
        var sim = Room();
        var walker = sim.AddBot(0, 80);
        var bridge = sim.AddBot(20, 80);
        walker.Brain = bridge.Brain = null;
        walker.Radius = bridge.Radius = Fixed.FromInt(16);
        bridge.Height = Fixed.FromInt(height);
        bridge.ActsLikeBridge = true;
        walker.VelocityX = Fixed.FromInt(8);
        sim.Tick();
        if (steps)
        {
            Assert.Equal(height, walker.Z.ToDouble());
            Assert.True(walker.X.ToDouble() > 0);
            Assert.True(walker.OnGround);
        }
        else
        {
            Assert.Equal(0, walker.Z.ToDouble());
            Assert.Equal(0, walker.X.ToDouble());
        }
    }

    [Fact]
    public void DeadBridgeIsNotAPlatform()
    {
        var sim = Room();
        var walker = sim.AddBot(0, 80);
        var bridge = sim.AddBot(20, 80);
        walker.Brain = bridge.Brain = null;
        walker.Radius = bridge.Radius = Fixed.FromInt(16);
        bridge.Height = Fixed.FromInt(16);
        bridge.ActsLikeBridge = true;
        bridge.Health = 0;
        walker.VelocityX = Fixed.FromInt(8);
        sim.Tick();
        Assert.Equal(0, walker.Z.ToDouble());
        Assert.True(walker.X.ToDouble() > 0);
    }

    [Theory]
    [InlineData(16, true)]
    [InlineData(24, true)]
    [InlineData(25, false)]
    [InlineData(56, false)]
    public void IceCorpseStepsOntoACorpseWithinMaxStepHeight(int height, bool steps)
    {
        var sim = Room();
        var walker = sim.AddBot(0, 80);
        var corpse = sim.AddBot(20, 80);
        walker.Brain = corpse.Brain = null;
        walker.Radius = corpse.Radius = Fixed.FromInt(16);
        walker.IceCorpse = true;
        corpse.Height = Fixed.FromInt(height);
        corpse.Health = 0;
        walker.VelocityX = Fixed.FromInt(8);
        sim.Tick();
        if (steps)
        {
            Assert.Equal(height, walker.Z.ToDouble());
            Assert.True(walker.X.ToDouble() > 0);
            Assert.True(walker.OnGround);
        }
        else
        {
            Assert.Equal(0, walker.Z.ToDouble());
            Assert.Equal(0, walker.X.ToDouble());
        }
    }

    [Fact]
    public void LivingActorWalksThroughACorpse()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var corpse = sim.AddBot(20, 0);
        corpse.Brain = null;
        corpse.Radius = Fixed.FromInt(16);
        corpse.Height = Fixed.FromInt(16);
        corpse.Health = 0;
        player.Radius = Fixed.FromInt(16);
        player.VelocityX = Fixed.FromInt(8);
        sim.Tick();
        Assert.Equal(0, player.Z.ToDouble());
        Assert.True(player.X.ToDouble() > 0);
    }

    [Fact]
    public void IceCorpseFlagIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var first = left.AddBot(80, 0);
        var second = right.AddBot(80, 0);
        first.Brain = second.Brain = null;
        first.IceCorpse = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        second.IceCorpse = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(first.IceCorpse);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void BridgeFlagIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var first = left.AddBot(80, 0);
        var second = right.AddBot(80, 0);
        first.Brain = second.Brain = null;
        first.ActsLikeBridge = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        second.ActsLikeBridge = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(first.ActsLikeBridge);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void ClosingDoorReopensBeforeClippingALivingActor()
    {
        var sim = TwoRooms(0, 56);
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.DoorRaise, 0, new LevelLine { SideFront = 1 }));
        for (var i = 0; i < 9; i++) sim.Tick();
        Assert.Equal(124, sim.CeilingOf(1));
        var player = sim.Players.Single();
        player.X = Fixed.FromInt(40);
        for (var i = 0; i < 35; i++)
        {
            sim.Tick();
            Assert.True(sim.CeilingOf(1) >= player.Z.ToDouble() + player.Height.ToDouble());
            Assert.Equal(0, sim.FloorOf(1));
        }
    }

    [Fact]
    public void MovingFloorCarriesGroundedPlayer()
    {
        var sim = Room();
        Assert.True(LineSpecials.Execute(sim, sim.Players.Single(), LineSpecials.FloorRaise, 1));
        sim.Tick();
        Assert.Equal(8, sim.Players.Single().Z.ToDouble());
        Assert.True(sim.Players.Single().OnGround);
    }

    [Fact]
    public void TimedStateActionsRunOnceAndZeroTicFramesChainImmediately()
    {
        var actor = new Actor();
        var actions = new List<int>();
        actor.States.Configure(actor, new ActorFrame[]
        {
            new(2, 1, _ => actions.Add(0)), new(0, 2, _ => actions.Add(1)), new(-1, 2, _ => actions.Add(2)),
        }, 0);
        actor.Tick();
        Assert.Equal(new[] { 0 }, actions);
        actor.Tick(); actor.Tick();
        Assert.Equal(new[] { 0, 1, 2 }, actions);
        Assert.Equal(2, actor.States.Current);
    }

    [Fact]
    public void ActionCanRedirectStateAndImmediateCyclesAreBounded()
    {
        var actor = new Actor();
        actor.States.Configure(actor, new ActorFrame[]
        {
            new(-1, 0, a => a.States.Enter(a, 1)), new(-1, 1),
        }, 0);
        Assert.Equal(1, actor.States.Current);
        Assert.Throws<InvalidOperationException>(() => actor.States.Configure(actor, new[] { new ActorFrame(0, 0) }, 0));
    }

    [Fact]
    public void StateRemovalUnlinksThinkerAndNeverReusesActorId()
    {
        var sim = Room();
        var bot = sim.AddBot(100, 0);
        bot.States.Configure(bot, new[] { new ActorFrame(1, -1) }, 0);
        sim.Tick();
        Assert.True(bot.Destroyed);
        Assert.DoesNotContain(bot, sim.Actors);
        Assert.DoesNotContain(bot, sim.Thinkers.ThinkersIn(ThinkerStat.Default));
        Assert.True(sim.AddBot(100, 0).Id > bot.Id);
    }

    [Fact]
    public void DestroyedFreshThinkerDoesNotReceivePostBeginPlay()
    {
        var thinkers = new ThinkerCollection();
        var actor = new Actor();
        thinkers.Add(actor); actor.Destroy(); thinkers.Run();
        Assert.Equal(0, actor.PostBeginCount);
        Assert.Empty(thinkers.ThinkersIn(ThinkerStat.Default));
    }

    [Fact]
    public void ChangingThinkerStatDuringTickDoesNotSkipNeighborsOrTickTwice()
    {
        var thinkers = new ThinkerCollection();
        var moving = new MovingThinker(thinkers);
        var neighbor = new Thinker();
        thinkers.Add(moving, ThinkerStat.FirstThinking);
        thinkers.Add(neighbor, ThinkerStat.FirstThinking);
        thinkers.Run(); thinkers.Run();
        Assert.Equal(2, moving.TickCount);
        Assert.Equal(2, neighbor.TickCount);
        Assert.Equal(1, moving.PostBeginCount);
    }

    private sealed class MovingThinker(ThinkerCollection thinkers) : Thinker
    {
        public override void Tick()
        {
            base.Tick();
            thinkers.ChangeStatNum(this, ThinkerStat.Default);
        }
    }

    [Fact]
    public void DamageUsesArmorPainInvulnerabilityAndSingleDeathTransition()
    {
        var player = new PlayerPawn();
        player.Inventory.Armor = 5;
        player.Inventory.ArmorSavePercent = 50;
        Assert.Equal(new DamageResult(15, 5, false), ActorDamage.Apply(player, 20));
        Assert.Equal(ActorStateMachine.Pain, player.States.Current);
        player.Invulnerable = true;
        Assert.Equal(default, ActorDamage.Apply(player, 100));
        Assert.Equal(85, player.Health);
        Assert.True(ActorDamage.Apply(player, 100, flags: DamageFlags.BypassInvulnerability).Killed);
        Assert.Equal(-15, player.Health);
        ActorDamage.Apply(player, 100); player.Health = -100;
        Assert.Equal(-100, player.Health);
        Assert.Equal(1, player.DeathCount);
        Assert.False(player.BlocksActors);
        Assert.False(player.CanTakeDamage);
        for (var i = 0; i < 6; i++) player.Tick();
        Assert.Equal(ActorStateMachine.Corpse, player.States.Current);
    }

    [Fact]
    public void OverkillRemainsBelowZeroAndGibsOnlyPastSpawnHealth()
    {
        var player = new PlayerPawn();
        Assert.Equal(new DamageResult(200, 0, true), ActorDamage.Apply(player, 200));
        Assert.Equal(-100, player.Health);
        Assert.Equal(ActorStateMachine.Death, player.States.Current);

        var armored = new PlayerPawn();
        armored.Inventory.Armor = 300;
        armored.Inventory.ArmorSavePercent = PlayerInventory.GreenSavePercent;
        var saved = ActorDamage.Apply(armored, 450);
        Assert.Equal(150, saved.ArmorLost);
        Assert.Equal(300, saved.HealthLost);
        Assert.Equal(-200, armored.Health);

        var gibbed = new PlayerPawn { ExtremeDeathState = 4 };
        gibbed.States.Configure(gibbed, DeathFrames(), 0);
        ActorDamage.Apply(gibbed, 201);
        Assert.Equal(-101, gibbed.Health);
        Assert.Equal(4, gibbed.States.Current);

        var boundary = new PlayerPawn { ExtremeDeathState = 4 };
        boundary.States.Configure(boundary, DeathFrames(), 0);
        ActorDamage.Apply(boundary, 200);
        Assert.Equal(-100, boundary.Health);
        Assert.Equal(ActorStateMachine.Death, boundary.States.Current);

        var monster = new Actor { Health = 30, GibHealth = -30 };
        Assert.Equal(new DamageResult(80, 0, true), ActorDamage.Apply(monster, 80));
        Assert.Equal(-50, monster.Health);
        Assert.Equal(ActorStateMachine.Death, monster.States.Current);
    }

    [Fact]
    public void ATypedDeathBeatsTheGibStateUnlessTheTypeIsMissing()
    {
        var burned = TypedVictim();
        burned.SetTypedDeath("Fire", 6, extreme: true);
        ActorDamage.Apply(burned, 201, damageType: "Fire");
        Assert.Equal(-101, burned.Health);
        Assert.Equal(6, burned.States.Current);

        var plain = TypedVictim();
        ActorDamage.Apply(plain, 201, damageType: "Fire");
        Assert.Equal(5, plain.States.Current);

        var wrongCase = TypedVictim();
        ActorDamage.Apply(wrongCase, 201, damageType: "fire");
        Assert.Equal(5, wrongCase.States.Current);

        var forced = TypedVictim();
        ActorDamage.Apply(forced, 100, damageType: "Extreme");
        Assert.Equal(0, forced.Health);
        Assert.Equal(4, forced.States.Current);

        var iced = TypedVictim();
        ActorDamage.Apply(iced, 201, damageType: "Ice");
        Assert.Equal(4, iced.States.Current);

        var scratch = TypedVictim();
        ActorDamage.Apply(scratch, 10, damageType: "Fire");
        Assert.Equal(90, scratch.Health);
        Assert.False(scratch.IsDead);
        Assert.Equal(1, scratch.States.Current);
    }

    [Fact]
    public void InflictorExtremeDeathFlagsChooseTheGibFrame()
    {
        var quiet = TypedVictim();
        ActorDamage.Apply(quiet, 150);
        Assert.Equal(-50, quiet.Health);
        Assert.Equal(2, quiet.States.Current);

        var forced = TypedVictim();
        ActorDamage.Apply(forced, 150, inflictor: new Actor { ExtremeDeath = true });
        Assert.Equal(-50, forced.Health);
        Assert.Equal(4, forced.States.Current);

        var blocked = TypedVictim();
        ActorDamage.Apply(blocked, 201, inflictor: new Actor { NoExtremeDeath = true });
        Assert.Equal(-101, blocked.Health);
        Assert.Equal(2, blocked.States.Current);

        var typed = TypedVictim();
        typed.SetTypedDeath("Fire", 6, extreme: true);
        ActorDamage.Apply(typed, 201, damageType: "Fire", inflictor: new Actor { NoExtremeDeath = true });
        Assert.Equal(5, typed.States.Current);

        var sourceOnly = TypedVictim();
        ActorDamage.Apply(sourceOnly, 150, source: new Actor { ExtremeDeath = true });
        Assert.Equal(2, sourceOnly.States.Current);
    }

    [Fact]
    public void ChaseThresholdBlocksTargetSwitchUntilItCountsDown()
    {
        var sim = Room();
        var bot = sim.AddBot(64, 64);
        bot.Health = 100;
        bot.Brain!.Enabled = false;
        bot.Brain.DefThreshold = 100;
        bot.PainChance = 256;
        var player = sim.Players.Single();
        var other = RivalBot(sim, 128, 64);
        other.Health = 100;
        other.Brain!.Enabled = false;
        ActorDamage.Apply(bot, 10, source: other, inflictor: other);
        Assert.Equal(other.Id, bot.Brain.TargetId);
        Assert.Equal(100, bot.Brain.Threshold);
        ActorDamage.Apply(bot, 10, source: player, inflictor: player);
        Assert.Equal(other.Id, bot.Brain.TargetId);

        bot.Brain.Enabled = true;
        for (var tic = 0; tic < 100; tic++)
            sim.Tick();
        Assert.Equal(0, bot.Brain.Threshold);
        bot.Brain.Enabled = false;
        ActorDamage.Apply(bot, 10, source: player, inflictor: player);
        Assert.Equal(player.Id, bot.Brain.TargetId);
        Assert.Equal(100, bot.Brain.Threshold);

        var picky = sim.AddBot(160, 64);
        picky.Health = 100;
        picky.Brain!.Enabled = false;
        picky.Brain.DefThreshold = 0;
        ActorDamage.Apply(picky, 10, source: other, inflictor: other);
        ActorDamage.Apply(picky, 10, source: player, inflictor: player);
        Assert.Equal(player.Id, picky.Brain!.TargetId);

        var ignored = sim.AddBot(192, 64);
        ignored.Health = 100;
        ignored.Brain!.Enabled = false;
        ignored.Brain.DefThreshold = 0;
        other.NeverTarget = true;
        ActorDamage.Apply(ignored, 10, source: other, inflictor: other);
        Assert.Null(ignored.Brain!.TargetId);
    }

    [Fact]
    public void NoTargetSwitchKeepsTheChaseTargetUntilItIsGone()
    {
        var sim = Room();
        var bot = sim.AddBot(64, 64);
        bot.Health = 100;
        bot.Brain!.Enabled = false;
        bot.Brain.DefThreshold = 0;
        bot.PainChance = 0;
        bot.NoTargetSwitch = true;
        var player = sim.Players.Single();
        var other = RivalBot(sim, 128, 64);
        other.Health = 100;
        other.Brain!.Enabled = false;
        ActorDamage.Apply(bot, 10, source: other, inflictor: other);
        ActorDamage.Apply(bot, 10, source: player, inflictor: player);
        Assert.Equal(other.Id, bot.Brain.TargetId);
    }

    [Fact]
    public void NoHatePlayersIgnoresPlayerWakeButStillRetaliatesAgainstMonsters()
    {
        var sim = Room();
        var bot = sim.AddBot(64, 64);
        bot.Health = 100;
        bot.Brain!.Enabled = false;
        bot.Brain.DefThreshold = 0;
        bot.PainChance = 0;
        bot.NoHatePlayers = true;
        var player = sim.Players.Single();
        var other = RivalBot(sim, 128, 64);
        other.Health = 100;
        other.Brain!.Enabled = false;
        ActorDamage.Apply(bot, 10, source: player, inflictor: player);
        Assert.Null(bot.Brain.TargetId);
        Assert.Equal(90, bot.Health);
        ActorDamage.Apply(bot, 10, source: other, inflictor: other);
        Assert.Equal(other.Id, bot.Brain.TargetId);
        ActorDamage.Apply(bot, 10, source: player, inflictor: player);
        Assert.Equal(other.Id, bot.Brain.TargetId);
    }

    [Fact]
    public void NoInfightingBlocksMonsterWakeAndTargetSwitch()
    {
        var sim = Room();
        var victim = sim.AddBot(64, 64);
        victim.Health = 100;
        victim.Brain!.Enabled = false;
        victim.Brain.DefThreshold = 0;
        victim.PainChance = 0;
        victim.NoInfighting = true;
        var rival = sim.AddBot(96, 64);
        rival.Health = 100;
        rival.Brain!.Enabled = false;
        var player = sim.Players.Single();
        ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Null(victim.Brain!.TargetId);
        ActorDamage.Apply(victim, 10, source: player, inflictor: player);
        Assert.Equal(player.Id, victim.Brain.TargetId);
        ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Equal(player.Id, victim.Brain.TargetId);
    }

    [Fact]
    public void LevelInfightingOffBlocksMonsterMonstersButNotThePlayer()
    {
        var sim = Room();
        sim.Infighting = -1;
        var victim = sim.AddBot(64, 64);
        victim.Health = 100;
        victim.Brain!.Enabled = false;
        victim.Brain.DefThreshold = 0;
        victim.PainChance = 0;
        var rival = sim.AddBot(96, 64);
        rival.Health = 100;
        rival.Brain!.Enabled = false;
        var player = sim.Players.Single();
        ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Null(victim.Brain!.TargetId);
        ActorDamage.Apply(victim, 10, source: player, inflictor: player);
        Assert.Equal(player.Id, victim.Brain.TargetId);
    }

    [Fact]
    public void StandardInfightingBlocksSameSpeciesMonsterDamage()
    {
        var sim = Room();
        sim.Infighting = 0;
        var victim = sim.AddBot(64, 64);
        victim.Health = 100;
        var rival = sim.AddBot(96, 64);
        rival.Health = 100;
        var result = ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Equal(0, result.HealthLost);
        Assert.Equal(100, victim.Health);

        var sergeant = sim.AddBot(128, 64, doomEdNum: 3001);
        sergeant.Health = 100;
        result = ActorDamage.Apply(victim, 10, source: sergeant, inflictor: sergeant);
        Assert.Equal(10, result.HealthLost);
        Assert.Equal(90, victim.Health);
    }

    [Fact]
    public void AlwaysInfightingAllowsSameSpeciesMonsterDamage()
    {
        var sim = Room();
        sim.Infighting = 1;
        var victim = sim.AddBot(64, 64);
        victim.Health = 100;
        var rival = sim.AddBot(96, 64);
        rival.Health = 100;
        var result = ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Equal(10, result.HealthLost);
        Assert.Equal(90, victim.Health);
    }

    [Fact]
    public void LevelInfightingOffBlocksMonsterDamageUnlessHostile()
    {
        var sim = Room();
        sim.Infighting = -1;
        var victim = sim.AddBot(64, 64);
        victim.Health = 100;
        var rival = sim.AddBot(96, 64);
        rival.Health = 100;
        var result = ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Equal(0, result.HealthLost);
        Assert.Equal(100, victim.Health);

        victim.Friendly = rival.Friendly = true;
        victim.FriendPlayer = 1;
        rival.FriendPlayer = 2;
        result = ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Equal(10, result.HealthLost);
    }

    [Fact]
    public void InfightingOffAllowsMonsterDamageToNonMonstersSuchAsBarrels()
    {
        var sim = Room();
        sim.Infighting = -1;
        var barrel = sim.AddBot(96, 64);
        barrel.Brain = null;
        barrel.IsMonster = false;
        barrel.Health = 20;
        var rival = RivalBot(sim, 64, 64);
        rival.Health = 100;
        var result = ActorDamage.Apply(barrel, 10, source: rival, inflictor: rival);
        Assert.Equal(10, result.HealthLost);
        Assert.Equal(10, barrel.Health);
    }

    [Fact]
    public void HarmFriendsAllowsFriendlyDamageAtStandardInfighting()
    {
        var sim = Room();
        sim.Infighting = 0;
        var victim = sim.AddBot(64, 64);
        victim.Health = 100;
        victim.Friendly = true;
        victim.FriendPlayer = 1;
        var ally = RivalBot(sim, 96, 64);
        ally.Health = 100;
        ally.Friendly = true;
        ally.FriendPlayer = 1;
        var blocked = ActorDamage.Apply(victim, 10, source: ally, inflictor: ally);
        Assert.Equal(0, blocked.HealthLost);
        ally.HarmFriends = true;
        var allowed = ActorDamage.Apply(victim, 10, source: ally, inflictor: ally);
        Assert.Equal(10, allowed.HealthLost);
        Assert.Equal(90, victim.Health);
    }

    [Fact]
    public void StandardInfightingBlocksSameSpeciesMonsterWake()
    {
        var sim = Room();
        sim.Infighting = 0;
        var victim = sim.AddBot(64, 64);
        victim.Health = 100;
        victim.Brain!.Enabled = false;
        victim.Brain.DefThreshold = 0;
        victim.PainChance = 0;
        var rival = sim.AddBot(96, 64);
        rival.Health = 100;
        rival.Brain!.Enabled = false;
        ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Null(victim.Brain!.TargetId);

        var sergeant = sim.AddBot(128, 64, doomEdNum: 3001);
        sergeant.Health = 100;
        sergeant.Brain!.Enabled = false;
        ActorDamage.Apply(victim, 10, source: sergeant, inflictor: sergeant);
        Assert.Equal(sergeant.Id, victim.Brain!.TargetId);
    }

    [Fact]
    public void AlwaysInfightingAllowsSameSpeciesWake()
    {
        var sim = Room();
        sim.Infighting = 1;
        var victim = sim.AddBot(64, 64);
        victim.Health = 100;
        victim.Brain!.Enabled = false;
        victim.Brain.DefThreshold = 0;
        victim.PainChance = 0;
        var rival = sim.AddBot(96, 64);
        rival.Health = 100;
        rival.Brain!.Enabled = false;
        ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Equal(rival.Id, victim.Brain!.TargetId);
    }

    [Fact]
    public void DoHarmSpeciesAllowsSameSpeciesWakeAtStandardInfighting()
    {
        var sim = Room();
        sim.Infighting = 0;
        var victim = sim.AddBot(64, 64);
        victim.Health = 100;
        victim.Brain!.Enabled = false;
        victim.Brain.DefThreshold = 0;
        victim.PainChance = 0;
        victim.DoHarmSpecies = true;
        var rival = sim.AddBot(96, 64);
        rival.Health = 100;
        rival.Brain!.Enabled = false;
        ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Equal(rival.Id, victim.Brain!.TargetId);
    }

    [Fact]
    public void ForceInfightingUsesStandardRulesWhenLevelInfightingIsOff()
    {
        var sim = Room();
        sim.Infighting = -1;
        var victim = sim.AddBot(64, 64);
        victim.Health = 100;
        victim.Brain!.Enabled = false;
        victim.Brain.DefThreshold = 0;
        victim.PainChance = 0;
        victim.ForceInfighting = true;
        var rival = sim.AddBot(96, 64);
        rival.Health = 100;
        rival.Brain!.Enabled = false;
        ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Null(victim.Brain!.TargetId);
        var sergeant = sim.AddBot(128, 64, doomEdNum: 3001);
        sergeant.Health = 100;
        sergeant.Brain!.Enabled = false;
        ActorDamage.Apply(victim, 10, source: sergeant, inflictor: sergeant);
        Assert.Equal(sergeant.Id, victim.Brain!.TargetId);
    }

    [Fact]
    public void OpposingFriendliesRetaliateWhenInfightingIsOff()
    {
        var sim = Room();
        sim.Infighting = -1;
        var left = sim.AddBot(64, 64);
        left.Health = 100;
        left.Brain!.Enabled = false;
        left.Brain.DefThreshold = 0;
        left.PainChance = 0;
        left.Friendly = true;
        left.FriendPlayer = 1;
        var right = sim.AddBot(96, 64);
        right.Health = 100;
        right.Brain!.Enabled = false;
        right.Friendly = true;
        right.FriendPlayer = 2;
        ActorDamage.Apply(left, 10, source: right, inflictor: right);
        Assert.Equal(right.Id, left.Brain!.TargetId);
    }

    [Fact]
    public void TidToHateAllowsMonsterDamageWhenInfightingIsOff()
    {
        var sim = Room();
        sim.Infighting = -1;
        var victim = sim.AddBot(64, 64, thingId: 42);
        victim.Health = 100;
        victim.Brain!.Enabled = false;
        victim.Brain.DefThreshold = 0;
        victim.PainChance = 0;
        var rival = RivalBot(sim, 96, 64);
        rival.Health = 100;
        rival.Brain!.Enabled = false;
        rival.TidToHate = 42;
        var result = ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Equal(10, result.HealthLost);
        Assert.Null(victim.Brain!.TargetId);
    }

    [Fact]
    public void TidToHateAllowsSameSpeciesWakeAtStandardInfighting()
    {
        var sim = Room();
        sim.Infighting = 0;
        var victim = sim.AddBot(64, 64, thingId: 9);
        victim.Health = 100;
        victim.Brain!.Enabled = false;
        victim.Brain.DefThreshold = 0;
        victim.PainChance = 0;
        var rival = sim.AddBot(96, 64);
        rival.Health = 100;
        rival.Brain!.Enabled = false;
        rival.TidToHate = 9;
        ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Equal(rival.Id, victim.Brain!.TargetId);
    }

    [Fact]
    public void SharedTidToHateBlocksMonsterDamageAtStandardInfighting()
    {
        var sim = Room();
        sim.Infighting = 0;
        var victim = sim.AddBot(64, 64);
        victim.Health = 100;
        var rival = RivalBot(sim, 96, 64);
        rival.Health = 100;
        victim.TidToHate = rival.TidToHate = 7;
        var result = ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Equal(0, result.HealthLost);
        Assert.Equal(100, victim.Health);
    }

    [Fact]
    public void TidToHateAllowsSameSpeciesDamageAtStandardInfighting()
    {
        var sim = Room();
        sim.Infighting = 0;
        var victim = sim.AddBot(64, 64, thingId: 9);
        victim.Health = 100;
        var rival = sim.AddBot(96, 64);
        rival.Health = 100;
        rival.TidToHate = 9;
        var result = ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Equal(10, result.HealthLost);
    }

    [Fact]
    public void HatedTargetSwitcherStickinessSometimesBlocksRetarget()
    {
        var blocked = false;
        var allowed = false;
        for (var seed = 0; seed < 256; seed++)
        {
            var sim = AuthoritySimulation.Start(new PlayLevel
            {
                Sectors = new[] { new LevelSector { CeilingHeight = 128 } },
                Things = new[] { new LevelThing { Type = 1 } },
            }, rngSeed: seed);
            sim.Infighting = 1;
            var victim = sim.AddBot(64, 64);
            victim.Health = 100;
            victim.Brain!.Enabled = false;
            victim.Brain.DefThreshold = 0;
            victim.PainChance = 0;
            victim.TidToHate = 7;
            var hated = sim.AddBot(96, 64, doomEdNum: 3001, thingId: 7);
            hated.Health = 100;
            hated.Brain!.Enabled = false;
            var rival = RivalBot(sim, 128, 64);
            rival.Health = 100;
            rival.Brain!.Enabled = false;
            ActorDamage.Apply(victim, 10, source: hated, inflictor: hated);
            ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
            if (victim.Brain!.TargetId == hated.Id) blocked = true;
            if (victim.Brain.TargetId == rival.Id) allowed = true;
        }
        Assert.True(blocked);
        Assert.True(allowed);
    }

    [Fact]
    public void SharedTidToHateBlocksMonsterWake()
    {
        var sim = Room();
        sim.Infighting = 1;
        var victim = sim.AddBot(64, 64);
        victim.Health = 100;
        victim.Brain!.Enabled = false;
        victim.Brain.DefThreshold = 0;
        victim.PainChance = 0;
        victim.TidToHate = 3;
        var rival = RivalBot(sim, 96, 64);
        rival.Health = 100;
        rival.Brain!.Enabled = false;
        rival.TidToHate = 3;
        ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Null(victim.Brain!.TargetId);
    }

    [Fact]
    public void NoTargetBlocksWakeUnlessTheTidIsHated()
    {
        var sim = Room();
        var victim = sim.AddBot(64, 64);
        victim.Health = 100;
        victim.Brain!.Enabled = false;
        victim.Brain.DefThreshold = 0;
        victim.PainChance = 0;
        var rival = RivalBot(sim, 96, 64);
        rival.Health = 100;
        rival.Brain!.Enabled = false;
        rival.NoTarget = true;
        ActorDamage.Apply(victim, 10, source: rival, inflictor: rival);
        Assert.Null(victim.Brain!.TargetId);

        var hated = sim.AddBot(112, 64, doomEdNum: 3001, thingId: 55);
        hated.Health = 100;
        hated.Brain!.Enabled = false;
        hated.NoTarget = true;
        victim.TidToHate = 55;
        ActorDamage.Apply(victim, 10, source: hated, inflictor: hated);
        Assert.Equal(hated.Id, victim.Brain!.TargetId);
    }

    [Fact]
    public void QuickToRetaliateSwitchesTargetsDespitePositiveThreshold()
    {
        var sim = Room();
        var bot = sim.AddBot(64, 64);
        bot.Health = 100;
        bot.Brain!.Enabled = false;
        bot.Brain.DefThreshold = 100;
        bot.PainChance = 0;
        bot.QuickToRetaliate = true;
        var player = sim.Players.Single();
        var other = RivalBot(sim, 128, 64);
        other.Health = 100;
        other.Brain!.Enabled = false;
        ActorDamage.Apply(bot, 10, source: other, inflictor: other);
        Assert.Equal(100, bot.Brain!.Threshold);
        ActorDamage.Apply(bot, 10, source: player, inflictor: player);
        Assert.Equal(player.Id, bot.Brain.TargetId);
        Assert.Equal(100, bot.Brain.Threshold);
    }

    [Fact]
    public void LastEnemyResumesChaseWhenTheCurrentTargetIsGone()
    {
        var sim = Room();
        var bot = sim.AddBot(64, 64);
        bot.Health = 100;
        bot.PainChance = 0;
        bot.Brain!.DefThreshold = 0;
        var other = RivalBot(sim, 96, 64);
        other.Health = 100;
        other.Brain!.Enabled = false;
        var player = sim.Players.Single();
        bot.Brain.Enabled = false;
        ActorDamage.Apply(bot, 10, source: other, inflictor: other);
        ActorDamage.Apply(bot, 10, source: player, inflictor: player);
        Assert.Equal(other.Id, bot.Brain.LastEnemyId);
        ActorDamage.Apply(player, 200, source: bot, inflictor: bot);
        Assert.False(player.CanTakeDamage);
        bot.Brain.Enabled = true;
        sim.Tick();
        Assert.Equal(other.Id, bot.Brain.TargetId);
        Assert.Null(bot.Brain.LastEnemyId);
        Assert.NotEqual(MonsterMode.Idle, bot.Brain.Mode);
    }

    [Fact]
    public void DeadLastEnemyIsClearedWithoutResumingChase()
    {
        var sim = Room();
        var bot = sim.AddBot(64, 64);
        bot.Health = 100;
        bot.PainChance = 0;
        bot.Brain!.DefThreshold = 0;
        var other = RivalBot(sim, 96, 64);
        other.Health = 100;
        other.Brain!.Enabled = false;
        var player = sim.Players.Single();
        bot.Brain.Enabled = false;
        ActorDamage.Apply(bot, 10, source: other, inflictor: other);
        ActorDamage.Apply(bot, 10, source: player, inflictor: player);
        ActorDamage.Apply(other, 200, source: player, inflictor: player);
        ActorDamage.Apply(player, 200, source: bot, inflictor: bot);
        bot.Brain.Enabled = true;
        sim.Tick();
        Assert.Null(bot.Brain!.TargetId);
        Assert.Null(bot.Brain.LastEnemyId);
        Assert.Equal(MonsterMode.Idle, bot.Brain.Mode);
    }

    [Fact]
    public void LastEnemyRecordsThePriorTargetOnWakeSwitch()
    {
        var sim = Room();
        var bot = sim.AddBot(64, 64);
        bot.Health = 100;
        bot.Brain!.Enabled = false;
        bot.Brain.DefThreshold = 0;
        bot.PainChance = 0;
        var other = RivalBot(sim, 96, 64);
        other.Health = 100;
        other.Brain!.Enabled = false;
        var player = sim.Players.Single();
        ActorDamage.Apply(bot, 10, source: other, inflictor: other);
        ActorDamage.Apply(bot, 10, source: player, inflictor: player);
        Assert.Equal(player.Id, bot.Brain!.TargetId);
        Assert.Equal(other.Id, bot.Brain.LastEnemyId);
    }

    [Fact]
    public void LastEnemyKeepsAPlayerWhenLaterSwitchesStayAmongMonsters()
    {
        var sim = Room();
        var bot = sim.AddBot(64, 64);
        bot.Health = 100;
        bot.Brain!.Enabled = false;
        bot.Brain.DefThreshold = 0;
        bot.PainChance = 0;
        var player = sim.Players.Single();
        var other = RivalBot(sim, 96, 64);
        other.Health = 100;
        other.Brain!.Enabled = false;
        var third = RivalBot(sim, 128, 64);
        third.Health = 100;
        third.Brain!.Enabled = false;
        ActorDamage.Apply(bot, 10, source: player, inflictor: player);
        ActorDamage.Apply(bot, 10, source: other, inflictor: other);
        Assert.Equal(player.Id, bot.Brain!.LastEnemyId);
        ActorDamage.Apply(bot, 10, source: third, inflictor: third);
        Assert.Equal(third.Id, bot.Brain.TargetId);
        Assert.Equal(player.Id, bot.Brain.LastEnemyId);
    }

    [Fact]
    public void LastEnemyIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftBot = left.AddBot(8, 8);
        var rightBot = right.AddBot(8, 8);
        leftBot.Brain!.Enabled = false;
        rightBot.Brain!.Enabled = false;
        leftBot.Brain.DefThreshold = 0;
        leftBot.PainChance = 0;
        rightBot.Brain.DefThreshold = 0;
        rightBot.PainChance = 0;
        var leftOther = RivalBot(left, 32, 8);
        var rightOther = RivalBot(right, 32, 8);
        ActorDamage.Apply(leftBot, 5, source: leftOther, inflictor: leftOther);
        ActorDamage.Apply(leftBot, 5, source: left.Players.Single(), inflictor: left.Players.Single());
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        ActorDamage.Apply(rightBot, 5, source: rightOther, inflictor: rightOther);
        ActorDamage.Apply(rightBot, 5, source: right.Players.Single(), inflictor: right.Players.Single());
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(leftOther.Id, leftBot.Brain!.LastEnemyId);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void TidToHateIsInTheChecksum()
    {
        var left = Room();
        var right = Room();
        left.AddBot(8, 8).TidToHate = 11;
        var rightBot = right.AddBot(8, 8);
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.TidToHate = 11;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);
    }

    [Fact]
    public void InfightingFlagsAreInTheChecksumAndSurviveAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftBot = left.AddBot(8, 8);
        var rightBot = right.AddBot(8, 8);
        leftBot.NoInfighting = true;
        rightBot.NoInfighting = true;
        left.Infighting = -1;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Infighting = -1;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(leftBot.NoInfighting);
        Assert.Equal(-1, left.Infighting);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void TargetSwitchFlagsAreInTheChecksumAndSurviveAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftBot = left.AddBot(8, 8);
        var rightBot = right.AddBot(8, 8);
        leftBot.NoTargetSwitch = true;
        leftBot.QuickToRetaliate = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.NoTargetSwitch = true;
        rightBot.QuickToRetaliate = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(leftBot.NoTargetSwitch);
        Assert.True(leftBot.QuickToRetaliate);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void ChaseThresholdIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftBot = left.AddBot(8, 8);
        var rightBot = right.AddBot(8, 8);
        leftBot.Brain!.Enabled = false;
        rightBot.Brain!.Enabled = false;
        leftBot.PainChance = 0;
        rightBot.PainChance = 0;
        leftBot.Brain.DefThreshold = 60;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.Brain.DefThreshold = 60;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        leftBot.Health = 100;
        rightBot.Health = 100;
        var leftPlayer = left.Players.Single();
        var rightPlayer = right.Players.Single();
        ActorDamage.Apply(leftBot, 10, source: leftPlayer, inflictor: leftPlayer);
        ActorDamage.Apply(rightBot, 10, source: rightPlayer, inflictor: rightPlayer);
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(60, leftBot.Brain!.DefThreshold);
        Assert.Equal(60, leftBot.Brain.Threshold);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void PlayerExtremelyDeadMarksAnExtremeDeathFrame()
    {
        var gibbed = TypedVictim();
        ActorDamage.Apply(gibbed, 201);
        Assert.True(gibbed.IsDead);
        Assert.True(gibbed.ExtremelyDead);
        Assert.Equal(4, gibbed.States.Current);

        var burned = TypedVictim();
        ActorDamage.Apply(burned, 201, damageType: "Fire");
        Assert.True(burned.IsDead);
        Assert.False(burned.ExtremelyDead);
        Assert.Equal(5, burned.States.Current);

        var forced = TypedVictim();
        ActorDamage.Apply(forced, 150, inflictor: new Actor { ExtremeDeath = true });
        Assert.True(forced.ExtremelyDead);
        Assert.Equal(-50, forced.Health);

        gibbed.Health = 100;
        Assert.False(gibbed.ExtremelyDead);
    }

    [Fact]
    public void ExtremelyDeadIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.Players.Single().ExtremelyDead = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Players.Single().ExtremelyDead = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(left.Players.Single().ExtremelyDead);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void IceCorpseShatterZeroesVelocityAndSnapsTheCorpseTics()
    {
        var corpse = new Actor { Health = 0, Shootable = true, IceCorpse = true, VelocityX = Fixed.FromInt(8) };
        corpse.States.Configure(corpse, new ActorFrame[] { new(10, 0), new(-1, 0) }, 0);
        ActorDamage.Apply(corpse, 5, inflictor: new Actor(), damageType: "Fire");
        Assert.True(corpse.Shattering);
        Assert.Equal(0, corpse.VelocityX.Raw);
        Assert.Equal(1, corpse.States.RemainingTics);

        var quiet = new Actor { Health = 0, Shootable = true, IceCorpse = true };
        quiet.States.Configure(quiet, new ActorFrame[] { new(10, 0), new(-1, 0) }, 0);
        ActorDamage.Apply(quiet, 5, inflictor: new Actor(), damageType: "Ice");
        Assert.False(quiet.Shattering);
        Assert.Equal(10, quiet.States.RemainingTics);

        var allowed = new Actor { Health = 0, Shootable = true, IceCorpse = true };
        allowed.States.Configure(allowed, new ActorFrame[] { new(10, 0), new(-1, 0) }, 0);
        ActorDamage.Apply(allowed, 5, inflictor: new Actor { IceShatter = true }, damageType: "Ice");
        Assert.True(allowed.Shattering);
        Assert.Equal(1, allowed.States.RemainingTics);

        var warm = new Actor { Health = 0, Shootable = true };
        warm.States.Configure(warm, new ActorFrame[] { new(10, 0), new(-1, 0) }, 0);
        ActorDamage.Apply(warm, 5, inflictor: new Actor(), damageType: "Fire");
        Assert.False(warm.Shattering);
        Assert.Equal(10, warm.States.RemainingTics);
    }

    [Fact]
    public void IceCorpseShatterSpawnsDeterministicChunksOnTheSimulation()
    {
        var left = Room();
        var right = Room();
        ShatterCorpse(left);
        ShatterCorpse(right);
        var count = left.Actors.OfType<IceChunkActor>().Count();
        Assert.InRange(count, 25, 64);
        Assert.Equal(count, right.Actors.OfType<IceChunkActor>().Count());
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);
    }

    [Fact]
    public void IceChunksWithoutASimulationStayAbsent()
    {
        var corpse = new Actor { Health = 0, Shootable = true, IceCorpse = true };
        corpse.States.Configure(corpse, new ActorFrame[] { new(10, 0), new(-1, 0) }, 0);
        ActorDamage.Apply(corpse, 5, inflictor: new Actor(), damageType: "Fire");
        Assert.True(corpse.Shattering);
    }

    [Fact]
    public void IceCorpseShatterFlagsAreInTheChecksumAndSurviveAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftBot = left.AddBot(8, 8);
        var rightBot = right.AddBot(8, 8);
        leftBot.Brain!.Enabled = false;
        rightBot.Brain!.Enabled = false;
        leftBot.Shattering = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.Shattering = true;
        leftBot.IceShatter = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.IceShatter = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(leftBot.Shattering);
        Assert.True(leftBot.IceShatter);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void ExtremeDeathClampsAMonsterToGibHealthMinusOne()
    {
        var monster = FreezeBody();
        monster.Brain = new MonsterBrain(MonsterAttack.Melee);
        ActorDamage.Apply(monster, 150, inflictor: new Actor { ExtremeDeath = true });
        Assert.Equal(-101, monster.Health);
        Assert.Equal(4, monster.States.Current);

        var player = TypedVictim();
        ActorDamage.Apply(player, 150, inflictor: new Actor { ExtremeDeath = true });
        Assert.Equal(-50, player.Health);
        Assert.Equal(4, player.States.Current);

        var pastGib = FreezeBody();
        pastGib.Brain = new MonsterBrain(MonsterAttack.Melee);
        ActorDamage.Apply(pastGib, 201);
        Assert.Equal(-101, pastGib.Health);
        Assert.Equal(4, pastGib.States.Current);
    }

    [Fact]
    public void JustHitMarksAPainFlinchAndClearsOnTheNextBrainTick()
    {
        var sim = Room();
        var bot = sim.AddBot(64, 64);
        bot.Health = 100;
        bot.Brain!.Enabled = false;
        bot.PainChance = 256;
        var player = sim.Players.Single();
        ActorDamage.Apply(bot, 10, source: player, inflictor: player);
        Assert.True(bot.JustHit);
        bot.Brain.Enabled = true;
        sim.Tick();
        Assert.False(bot.JustHit);

        var distracted = sim.AddBot(96, 64);
        distracted.Health = 100;
        distracted.Brain!.Enabled = false;
        distracted.PainChance = 256;
        var other = RivalBot(sim, 128, 64);
        other.Health = 100;
        other.Brain!.Enabled = false;
        ActorDamage.Apply(distracted, 10, source: other, inflictor: other);
        distracted.JustHit = false;
        ActorDamage.Apply(distracted, 10, source: player, inflictor: player);
        Assert.True(distracted.JustHit);

        var wounded = sim.AddBot(160, 64);
        wounded.Health = 55;
        wounded.Brain!.Enabled = false;
        wounded.WoundHealth = 50;
        wounded.States.Configure(wounded, new ActorFrame[]
        {
            new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(5, 0),
        }, 0);
        wounded.SetTypedWound("Fire", 4);
        ActorDamage.Apply(wounded, 10, source: player, inflictor: player, damageType: "Fire");
        Assert.False(wounded.JustHit);
    }

    [Fact]
    public void FriendlyWakeUpIgnoresAnotherFriendlySource()
    {
        var sim = Room();
        var victim = sim.AddBot(64, 64);
        victim.Health = 100;
        victim.Brain!.Enabled = false;
        victim.Friendly = true;
        victim.FriendPlayer = 1;
        victim.PainChance = 0;
        var ally = sim.AddBot(96, 64);
        ally.Health = 100;
        ally.Brain!.Enabled = false;
        ally.Friendly = true;
        ally.FriendPlayer = 1;
        var player = sim.Players.Single();
        ActorDamage.Apply(victim, 10, source: ally, inflictor: ally);
        Assert.Null(victim.Brain.TargetId);
        ActorDamage.Apply(victim, 10, source: player, inflictor: player);
        Assert.Equal(player.Id, victim.Brain!.TargetId);
    }

    [Fact]
    public void JustHitHonorsFriendlyChaseTargets()
    {
        var sim = Room();
        var bot = sim.AddBot(64, 64);
        bot.Health = 100;
        bot.Brain!.Enabled = false;
        bot.Brain.DefThreshold = 100;
        bot.PainChance = 256;
        var ally = RivalBot(sim, 96, 64);
        ally.Health = 100;
        ally.Brain!.Enabled = false;
        var player = sim.Players.Single();
        ActorDamage.Apply(bot, 10, source: ally, inflictor: ally);
        Assert.Equal(ally.Id, bot.Brain!.TargetId);
        bot.Friendly = ally.Friendly = true;
        bot.FriendPlayer = ally.FriendPlayer = 1;
        bot.JustHit = false;
        ActorDamage.Apply(bot, 10, source: player, inflictor: player);
        Assert.False(bot.JustHit);

        var rival = sim.AddBot(128, 64, doomEdNum: 3002);
        rival.Health = 100;
        rival.Brain!.Enabled = false;
        bot.Brain.DefThreshold = 0;
        bot.Brain.Enabled = true;
        for (var tic = 0; tic < 100; tic++)
            sim.Tick();
        bot.Brain.Enabled = false;
        ActorDamage.Apply(bot, 10, source: rival, inflictor: rival);
        Assert.Equal(rival.Id, bot.Brain.TargetId);
        bot.JustHit = false;
        ActorDamage.Apply(bot, 10, source: player, inflictor: player);
        Assert.True(bot.JustHit);
    }

    [Fact]
    public void FriendlyFlagsAreInTheChecksumAndSurviveAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftBot = left.AddBot(8, 8);
        var rightBot = right.AddBot(8, 8);
        leftBot.Friendly = true;
        leftBot.FriendPlayer = 1;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.Friendly = true;
        rightBot.FriendPlayer = 1;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(leftBot.Friendly);
        Assert.Equal(1, leftBot.FriendPlayer);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void JustHitIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftBot = left.AddBot(8, 8);
        var rightBot = right.AddBot(8, 8);
        leftBot.Brain!.Enabled = false;
        rightBot.Brain!.Enabled = false;
        leftBot.JustHit = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.JustHit = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(leftBot.JustHit);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void TypedWoundReplacesPainAndSkipsWakeWhenHealthIsLowEnough()
    {
        var sim = Room();
        var bot = sim.AddBot(64, 64);
        bot.Health = 100;
        bot.Brain!.Enabled = false;
        bot.WoundHealth = 50;
        bot.PainChance = 256;
        bot.States.Configure(bot, new ActorFrame[]
        {
            new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(5, 0),
        }, 0);
        bot.SetTypedWound("Fire", 4);
        var player = sim.Players.Single();
        ActorDamage.Apply(bot, 10, source: player, inflictor: player, damageType: "Fire");
        Assert.Equal(90, bot.Health);
        Assert.Equal(1, bot.States.Current);
        Assert.Equal(0, bot.Brain.ReactionTics);

        var wounded = sim.AddBot(80, 64);
        wounded.Health = 55;
        wounded.Brain!.Enabled = false;
        wounded.WoundHealth = 50;
        wounded.States.Configure(wounded, new ActorFrame[]
        {
            new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(5, 0),
        }, 0);
        wounded.SetTypedWound("Fire", 4);
        Assert.Equal(8, wounded.Brain!.ReactionTics);
        ActorDamage.Apply(wounded, 10, source: player, inflictor: player, damageType: "Fire");
        Assert.Equal(45, wounded.Health);
        Assert.Equal(4, wounded.States.Current);
        Assert.Equal(8, wounded.Brain.ReactionTics);

        var other = sim.AddBot(96, 64);
        other.Health = 100;
        other.Brain!.Enabled = false;
        other.WoundHealth = 50;
        other.PainChance = 256;
        other.States.Configure(other, new ActorFrame[]
        {
            new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(5, 0),
        }, 0);
        other.SetTypedWound("Fire", 4);
        ActorDamage.Apply(other, 50, source: player, inflictor: player, damageType: "Slime");
        Assert.Equal(1, other.States.Current);

        var edge = sim.AddBot(128, 64);
        edge.Health = 100;
        edge.Brain!.Enabled = false;
        edge.WoundHealth = 50;
        edge.States.Configure(edge, new ActorFrame[]
        {
            new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(5, 0),
        }, 0);
        edge.SetTypedWound("Fire", 4);
        ActorDamage.Apply(edge, 50, source: player, inflictor: player, damageType: "Fire");
        Assert.Equal(50, edge.Health);
        Assert.Equal(4, edge.States.Current);
    }

    [Fact]
    public void TypedWoundIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftBot = left.AddBot(8, 8);
        var rightBot = right.AddBot(8, 8);
        leftBot.Brain!.Enabled = false;
        rightBot.Brain!.Enabled = false;
        leftBot.WoundHealth = 25;
        leftBot.SetTypedWound("Fire", 3);
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.WoundHealth = 25;
        rightBot.SetTypedWound("Fire", 3);
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(25, leftBot.WoundHealth);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void ElectricPainEitherFlinchesOrSetsFullBright()
    {
        var painSeed = -1;
        var brightSeed = -1;
        for (var seed = 0; seed < 10_000 && (painSeed < 0 || brightSeed < 0); seed++)
        {
            var sim = SeededRoom(seed);
            var shocked = ShockedBot(sim);
            ActorDamage.Apply(shocked, 10, damageType: "Electric");
            if (shocked.States.Current == 4) painSeed = seed;
            if (shocked.FullBright) brightSeed = seed;
        }
        Assert.True(painSeed >= 0);
        Assert.True(brightSeed >= 0);

        var painVictim = ShockedBot(SeededRoom(painSeed));
        ActorDamage.Apply(painVictim, 10, damageType: "Electric");
        Assert.Equal(4, painVictim.States.Current);
        Assert.False(painVictim.FullBright);

        var brightVictim = ShockedBot(SeededRoom(brightSeed));
        ActorDamage.Apply(brightVictim, 10, damageType: "Electric");
        Assert.Equal(0, brightVictim.States.Current);
        Assert.True(brightVictim.FullBright);

        var forced = ShockedBot(SeededRoom(brightSeed));
        ActorDamage.Apply(forced, 10, damageType: "Electric", inflictor: new Actor { ForcePain = true });
        Assert.Equal(4, forced.States.Current);
        Assert.False(forced.FullBright);
    }

    [Fact]
    public void FullBrightIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftBot = left.AddBot(8, 8);
        var rightBot = right.AddBot(8, 8);
        leftBot.Brain!.Enabled = false;
        rightBot.Brain!.Enabled = false;
        leftBot.FullBright = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.FullBright = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(leftBot.FullBright);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void DamageWakesAMonsterAndEntersSeeFromSpawn()
    {
        var sim = Room();
        var bot = sim.AddBot(64, 64);
        bot.Brain!.Enabled = false;
        bot.PainChance = 0;
        bot.SeeState = 3;
        bot.States.Configure(bot, new ActorFrame[]
        {
            new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(7, 3),
        }, 0);
        Assert.Equal(8, bot.Brain.ReactionTics);
        var player = sim.Players.Single();
        ActorDamage.Apply(bot, 10, source: player, inflictor: player);
        Assert.Equal(0, bot.Brain.ReactionTics);
        Assert.Equal(player.Id, bot.Brain.TargetId);
        Assert.Equal(3, bot.States.Current);

        var quiet = sim.AddBot(96, 64);
        quiet.Brain!.Enabled = false;
        quiet.NoPain = true;
        quiet.SeeState = 3;
        quiet.States.Configure(quiet, new ActorFrame[]
        {
            new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(7, 3),
        }, 0);
        ActorDamage.Apply(quiet, 10, source: player, inflictor: player);
        Assert.Equal(0, quiet.Brain!.ReactionTics);
        Assert.Equal(3, quiet.States.Current);

        var pained = sim.AddBot(128, 64);
        pained.Brain!.Enabled = false;
        pained.SeeState = 3;
        pained.States.Configure(pained, new ActorFrame[]
        {
            new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(7, 3),
        }, 0);
        ActorDamage.Apply(pained, 10, source: player, inflictor: player);
        Assert.Equal(1, pained.States.Current);
    }

    [Fact]
    public void SeeStateIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftBot = left.AddBot(8, 8);
        var rightBot = right.AddBot(8, 8);
        leftBot.Brain!.Enabled = false;
        rightBot.Brain!.Enabled = false;
        leftBot.SeeState = 3;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.SeeState = 3;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(3, leftBot.SeeState);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void InflictorExtremeDeathFlagsAreInTheChecksumAndSurviveAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftBot = left.AddBot(8, 8);
        var rightBot = right.AddBot(8, 8);
        leftBot.Brain!.Enabled = false;
        rightBot.Brain!.Enabled = false;
        leftBot.ExtremeDeath = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.ExtremeDeath = true;
        leftBot.NoExtremeDeath = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.NoExtremeDeath = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(leftBot.ExtremeDeath);
        Assert.True(leftBot.NoExtremeDeath);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void GenericFreezeDeathReplacesAMissingIceDeath()
    {
        var frozen = TypedVictim();
        frozen.GenericFreezeDeath = 6;
        ActorDamage.Apply(frozen, 201, damageType: "Ice");
        Assert.Equal(-101, frozen.Health);
        Assert.Equal(6, frozen.States.Current);

        var typed = TypedVictim();
        typed.GenericFreezeDeath = 6;
        typed.SetTypedDeath("Ice", 5);
        ActorDamage.Apply(typed, 201, damageType: "Ice");
        Assert.Equal(5, typed.States.Current);

        var extremeIce = TypedVictim();
        extremeIce.GenericFreezeDeath = 6;
        extremeIce.SetTypedDeath("Ice", 2, extreme: true);
        ActorDamage.Apply(extremeIce, 201, damageType: "Ice");
        Assert.Equal(2, extremeIce.States.Current);

        var blocked = TypedVictim();
        blocked.GenericFreezeDeath = 6;
        blocked.NoIceDeath = true;
        ActorDamage.Apply(blocked, 201, damageType: "Ice");
        Assert.Equal(4, blocked.States.Current);

        var decoration = FreezeBody();
        ActorDamage.Apply(decoration, 201, damageType: "Ice");
        Assert.Equal(4, decoration.States.Current);

        var monster = FreezeBody();
        monster.IsMonster = true;
        monster.Brain = new MonsterBrain(MonsterAttack.Melee);
        ActorDamage.Apply(monster, 201, damageType: "Ice");
        Assert.Equal(6, monster.States.Current);

        var wrongCase = TypedVictim();
        wrongCase.GenericFreezeDeath = 6;
        ActorDamage.Apply(wrongCase, 201, damageType: "ice");
        Assert.Equal(6, wrongCase.States.Current);

        var fire = TypedVictim();
        fire.GenericFreezeDeath = 6;
        ActorDamage.Apply(fire, 201, damageType: "Fire");
        Assert.Equal(5, fire.States.Current);

        var scratch = TypedVictim();
        scratch.GenericFreezeDeath = 6;
        ActorDamage.Apply(scratch, 10, damageType: "Ice");
        Assert.Equal(90, scratch.Health);
        Assert.False(scratch.IsDead);
        Assert.Equal(1, scratch.States.Current);
    }

    [Fact]
    public void ATypedPainUsesThatFrameAndItsOwnChance()
    {
        var burned = PainVictim();
        burned.SetTypedPain("Fire", 4);
        ActorDamage.Apply(burned, 10, damageType: "Fire");
        Assert.Equal(90, burned.Health);
        Assert.Equal(4, burned.States.Current);

        var other = PainVictim();
        other.SetTypedPain("Fire", 4);
        ActorDamage.Apply(other, 10, damageType: "Slime");
        Assert.Equal(1, other.States.Current);

        var quiet = PainVictim();
        quiet.SetTypedPain("Fire", 4, chance: 0);
        ActorDamage.Apply(quiet, 10, damageType: "Fire");
        Assert.Equal(90, quiet.Health);
        Assert.Equal(0, quiet.States.Current);
        ActorDamage.Apply(quiet, 10, damageType: "Slime");
        Assert.Equal(1, quiet.States.Current);

        var wrongCase = PainVictim();
        wrongCase.SetTypedPain("Fire", 4);
        ActorDamage.Apply(wrongCase, 10, damageType: "fire");
        Assert.Equal(4, wrongCase.States.Current);
    }

    [Fact]
    public void TypedPainIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.Players.Single().SetTypedPain("Fire", 1, chance: 0);
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Players.Single().SetTypedPain("Fire", 1, chance: 0);
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(checksum, left.Checksum);
    }

    private static Actor PainVictim()
    {
        var actor = new Actor { Health = 100 };
        actor.States.Configure(actor, new ActorFrame[]
        {
            new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(5, 0),
        }, 0);
        return actor;
    }

    private static AuthoritySimulation SeededRoom(int seed) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = new[] { new LevelSector { CeilingHeight = 128 } },
        Things = new[] { new LevelThing { Type = 1 } },
    }, rngSeed: seed);

    private static Actor ShockedBot(AuthoritySimulation sim)
    {
        var bot = sim.AddBot(64, 64);
        bot.Brain!.Enabled = false;
        bot.PainChance = 256;
        bot.States.Configure(bot, new ActorFrame[]
        {
            new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(5, 0),
        }, 0);
        bot.SetTypedPain("Electric", 4);
        return bot;
    }

    [Fact]
    public void PainThresholdSkipsAHitBelowThePostArmorAmount()
    {
        var actor = new Actor { Health = 100, PainThreshold = 10 };
        ActorDamage.Apply(actor, 9);
        Assert.Equal(91, actor.Health);
        Assert.Equal(0, actor.States.Current);

        ActorDamage.Apply(actor, 10);
        Assert.Equal(81, actor.Health);
        Assert.Equal(1, actor.States.Current);

        var armored = new Actor
        {
            Health = 100,
            PainThreshold = 21,
            Armor = 100,
            ArmorSavePercent = PlayerInventory.GreenSavePercent,
        };
        ActorDamage.Apply(armored, 30);
        Assert.Equal(80, armored.Health);
        Assert.Equal(0, armored.States.Current);
    }

    [Fact]
    public void PainThresholdIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.Players.Single().PainThreshold = 10;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Players.Single().PainThreshold = 10;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(10, left.Players.Single().PainThreshold);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void ForcePainFlinchesThroughTheThresholdAndAFailedRoll()
    {
        var forcer = new Actor { ForcePain = true };
        var below = new Actor { Health = 100, PainThreshold = 10 };
        ActorDamage.Apply(below, 9, inflictor: forcer);
        Assert.Equal(91, below.Health);
        Assert.Equal(1, below.States.Current);

        var quiet = new Actor { Health = 100, PainChance = 0, PainThreshold = 10 };
        ActorDamage.Apply(quiet, 9, source: forcer);
        Assert.Equal(91, quiet.Health);
        Assert.Equal(0, quiet.States.Current);

        var absorbed = new Actor { Health = 100, Armor = 100, ArmorSavePercent = 100, PainChance = 0 };
        ActorDamage.Apply(absorbed, 30, inflictor: forcer);
        Assert.Equal(100, absorbed.Health);
        Assert.Equal(0, absorbed.States.Current);

        var blocked = new Actor { Health = 100, PainThreshold = 10 };
        ActorDamage.Apply(blocked, 9, inflictor: forcer, flags: DamageFlags.NoPain);
        Assert.Equal(91, blocked.Health);
        Assert.Equal(0, blocked.States.Current);
    }

    [Fact]
    public void ForcePainIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftBot = left.AddBot(8, 8);
        var rightBot = right.AddBot(8, 8);
        leftBot.Brain!.Enabled = false;
        rightBot.Brain!.Enabled = false;
        leftBot.ForcePain = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.ForcePain = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(leftBot.ForcePain);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void NoPainAndPainlessBlockTheFlinchEvenWhenForced()
    {
        var forcer = new Actor { ForcePain = true, Painless = true };
        var quiet = new Actor { Health = 100, NoPain = true };
        ActorDamage.Apply(quiet, 50, inflictor: forcer);
        Assert.Equal(50, quiet.Health);
        Assert.Equal(0, quiet.States.Current);

        var player = new PlayerPawn { Health = 100, NoPain = true };
        ActorDamage.Apply(player, 50, inflictor: new Actor { ForcePain = true });
        Assert.Equal(50, player.Health);
        Assert.Equal(0, player.States.Current);

        var victim = new Actor { Health = 100 };
        ActorDamage.Apply(victim, 50, inflictor: forcer);
        Assert.Equal(50, victim.Health);
        Assert.Equal(0, victim.States.Current);

        var stillHurts = new Actor { Health = 100 };
        ActorDamage.Apply(stillHurts, 50, source: new Actor { Painless = true });
        Assert.Equal(50, stillHurts.Health);
        Assert.Equal(1, stillHurts.States.Current);
    }

    [Fact]
    public void NoPainAndPainlessAreInTheChecksumAndSurviveAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftBot = left.AddBot(8, 8);
        var rightBot = right.AddBot(8, 8);
        leftBot.Brain!.Enabled = false;
        rightBot.Brain!.Enabled = false;
        leftBot.NoPain = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.NoPain = true;
        leftBot.Painless = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.Painless = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(leftBot.NoPain);
        Assert.True(leftBot.Painless);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void TypedDeathIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.Players.Single().SetTypedDeath("Fire", 2);
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Players.Single().SetTypedDeath("Fire", 2);
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void GenericFreezeDeathIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftPlayer = left.Players.Single();
        var rightPlayer = right.Players.Single();
        leftPlayer.GenericFreezeDeath = 6;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightPlayer.GenericFreezeDeath = 6;
        leftPlayer.NoIceDeath = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightPlayer.NoIceDeath = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(6, leftPlayer.GenericFreezeDeath);
        Assert.True(leftPlayer.NoIceDeath);
        Assert.Equal(checksum, left.Checksum);
    }

    private static Actor FreezeBody()
    {
        var actor = new Actor { Health = 100, ExtremeDeathState = 4, GibHealth = -100, GenericFreezeDeath = 6 };
        actor.States.Configure(actor, new ActorFrame[]
        {
            new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(7, 3), new(8, 3), new(9, 3),
        }, 0);
        return actor;
    }

    private static PlayerPawn TypedVictim()
    {
        var actor = new PlayerPawn { ExtremeDeathState = 4, GibHealth = -100 };
        actor.States.Configure(actor, new ActorFrame[]
        {
            new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(7, 3), new(8, 3), new(9, 3),
        }, 0);
        actor.SetTypedDeath("Fire", 5);
        return actor;
    }

    [Fact]
    public void MonsterArmorAbsorbsWithThePlayerSaveFormula()
    {
        var green = new Actor { Health = 30, GibHealth = -30, Armor = 100, ArmorSavePercent = PlayerInventory.GreenSavePercent };
        var saved = ActorDamage.Apply(green, 80);
        Assert.Equal(26, saved.ArmorLost);
        Assert.Equal(54, saved.HealthLost);
        Assert.Equal(-24, green.Health);
        Assert.Equal(74, green.Armor);
        Assert.Equal(PlayerInventory.GreenSavePercent, green.ArmorSavePercent);

        var depleted = new Actor { Health = 100, Armor = 10, ArmorSavePercent = 50 };
        var hit = ActorDamage.Apply(depleted, 100);
        Assert.Equal(10, hit.ArmorLost);
        Assert.Equal(90, hit.HealthLost);
        Assert.Equal(10, depleted.Health);
        Assert.Equal(0, depleted.Armor);
        Assert.Equal(0, depleted.ArmorSavePercent);

        var forced = new Actor { Health = 50, Armor = 100, ArmorSavePercent = 100 };
        Assert.Equal(0, ActorDamage.Apply(forced, 20, flags: DamageFlags.BypassArmor).ArmorLost);
        Assert.Equal(30, forced.Health);
        Assert.Equal(100, forced.Armor);
    }

    [Fact]
    public void MonsterArmorIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var first = left.AddBot(80, 0);
        var second = right.AddBot(80, 0);
        first.Brain = second.Brain = null;
        first.Armor = 50;
        first.ArmorSavePercent = 33;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        second.Armor = 50;
        second.ArmorSavePercent = 33;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(50, first.Armor);
        Assert.Equal(33, first.ArmorSavePercent);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void BuddhaStopsAKillingBlowAtOneHealth()
    {
        var monster = new Actor { Health = 30, GibHealth = -30, Buddha = true };
        var saved = ActorDamage.Apply(monster, 80);
        Assert.Equal(new DamageResult(29, 0, false), saved);
        Assert.Equal(1, monster.Health);
        Assert.Equal(0, monster.DeathCount);
        Assert.NotEqual(ActorStateMachine.Death, monster.States.Current);

        var wounded = new Actor { Health = 30, Buddha = true };
        Assert.Equal(10, ActorDamage.Apply(wounded, 10).HealthLost);
        Assert.Equal(20, wounded.Health);

        var telefrag = new Actor { Health = 30, Buddha = true };
        ActorDamage.Apply(telefrag, ActorDamage.TelefragDamage);
        Assert.Equal(30 - ActorDamage.TelefragDamage, telefrag.Health);
        Assert.Equal(ActorStateMachine.Death, telefrag.States.Current);

        var forced = new Actor { Health = 30, Buddha = true, Armor = 100, ArmorSavePercent = 100 };
        var forcedHit = ActorDamage.Apply(forced, 80, flags: DamageFlags.Forced);
        Assert.Equal(0, forcedHit.ArmorLost);
        Assert.Equal(-50, forced.Health);
        Assert.Equal(100, forced.Armor);

        var foiled = new Actor { Health = 30, Buddha = true };
        ActorDamage.Apply(foiled, 80, flags: DamageFlags.FoilBuddha);
        Assert.Equal(-50, foiled.Health);

        var player = new PlayerPawn { Health = 40, Buddha = true };
        ActorDamage.Apply(player, 80, flags: DamageFlags.FoilBuddha);
        Assert.Equal(1, player.Health);
        Assert.False(player.IsDead);

        var armored = new Actor { Health = 30, Buddha = true, Armor = 100, ArmorSavePercent = PlayerInventory.GreenSavePercent };
        var absorbed = ActorDamage.Apply(armored, 80);
        Assert.Equal(26, absorbed.ArmorLost);
        Assert.Equal(1, armored.Health);
        Assert.Equal(74, armored.Armor);
        Assert.False(armored.IsDead);
    }

    [Fact]
    public void Buddha2SurvivesTelefragAndForcedDamage()
    {
        var player = new PlayerPawn { Health = 30, Buddha2 = true };
        ActorDamage.Apply(player, ActorDamage.TelefragDamage);
        Assert.Equal(1, player.Health);
        Assert.False(player.IsDead);
        Assert.Equal(0, player.DeathCount);

        player.Health = 30;
        var forced = ActorDamage.Apply(player, 80, flags: DamageFlags.Forced);
        Assert.Equal(1, player.Health);
        Assert.Equal(29, forced.HealthLost);
        Assert.False(player.IsDead);

        player.Health = 30;
        player.Inventory.Armor = 100;
        player.Inventory.ArmorSavePercent = 100;
        var armored = ActorDamage.Apply(player, 80, flags: DamageFlags.Forced);
        Assert.Equal(80, armored.ArmorLost);
        Assert.Equal(30, player.Health);
        Assert.Equal(20, player.Inventory.Armor);

        player.GodMode = true;
        player.Health = 40;
        var blocked = ActorDamage.Apply(player, 80, flags: DamageFlags.Forced);
        Assert.Equal(0, blocked.HealthLost);
        Assert.Equal(40, player.Health);

        var ordinary = new PlayerPawn { Health = 30, Buddha = true };
        ActorDamage.Apply(ordinary, ActorDamage.TelefragDamage);
        Assert.Equal(30 - ActorDamage.TelefragDamage, ordinary.Health);
        Assert.True(ordinary.IsDead);

        var scratch = new PlayerPawn { Health = 30, Buddha2 = true };
        Assert.Equal(10, ActorDamage.Apply(scratch, 10).HealthLost);
        Assert.Equal(20, scratch.Health);
    }

    [Fact]
    public void Buddha2IsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.Players.Single().Buddha2 = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Players.Single().Buddha2 = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(left.Players.Single().Buddha2);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void PowerBuddhaLastsSixtySecondsAndStopsAKillingBlow()
    {
        var player = new PlayerPawn { Health = 30 };
        player.GivePowerBuddha();
        Assert.Equal(PlayerPawn.PowerBuddhaDuration, player.PowerBuddhaTics);
        Assert.Equal(60 * GameTicClock.TicRate, player.PowerBuddhaTics);

        ActorDamage.Apply(player, 80);
        Assert.Equal(1, player.Health);
        Assert.False(player.IsDead);

        player.Health = 30;
        ActorDamage.Apply(player, 80, flags: DamageFlags.FoilBuddha);
        Assert.Equal(1, player.Health);
        Assert.False(player.IsDead);

        player.Health = 30;
        player.Inventory.Armor = 100;
        player.Inventory.ArmorSavePercent = 100;
        var forced = ActorDamage.Apply(player, 80, flags: DamageFlags.Forced);
        Assert.Equal(0, forced.ArmorLost);
        Assert.Equal(100, player.Inventory.Armor);
        Assert.True(player.IsDead);

        var telefrag = new PlayerPawn { Health = 30 };
        telefrag.GivePowerBuddha();
        ActorDamage.Apply(telefrag, ActorDamage.TelefragDamage);
        Assert.True(telefrag.IsDead);

        var scratch = new PlayerPawn { Health = 30 };
        scratch.GivePowerBuddha();
        Assert.Equal(10, ActorDamage.Apply(scratch, 10).HealthLost);
        Assert.Equal(20, scratch.Health);

        var held = new PlayerPawn { PowerBuddhaTics = PlayerPawn.PowerBuddhaBlinkThreshold + 1 };
        held.GivePowerBuddha();
        Assert.Equal(PlayerPawn.PowerBuddhaBlinkThreshold + 1, held.PowerBuddhaTics);

        var low = new PlayerPawn { PowerBuddhaTics = PlayerPawn.PowerBuddhaBlinkThreshold };
        low.GivePowerBuddha();
        Assert.Equal(PlayerPawn.PowerBuddhaDuration, low.PowerBuddhaTics);

        var fading = new PlayerPawn { Health = 30, PowerBuddhaTics = 1 };
        fading.Tick();
        Assert.Equal(0, fading.PowerBuddhaTics);
        ActorDamage.Apply(fading, 80);
        Assert.True(fading.IsDead);
    }

    [Fact]
    public void PowerBuddhaIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.Players.Single().PowerBuddhaTics = 5;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Players.Single().PowerBuddhaTics = left.Players.Single().PowerBuddhaTics;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(3, left.Players.Single().PowerBuddhaTics);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void InflictorFoilBuddhaKillsAMonsterAndNotAPlayer()
    {
        var missile = new Actor { FoilBuddha = true };
        var monster = new Actor { Health = 30, Buddha = true };
        ActorDamage.Apply(monster, 80, inflictor: missile);
        Assert.Equal(-50, monster.Health);
        Assert.True(monster.IsDead);

        var plain = new Actor { Health = 30, Buddha = true };
        ActorDamage.Apply(plain, 80, source: new Actor { FoilBuddha = true });
        Assert.Equal(1, plain.Health);
        Assert.False(plain.IsDead);

        var player = new PlayerPawn { Health = 30, Buddha = true };
        ActorDamage.Apply(player, 80, inflictor: missile);
        Assert.Equal(1, player.Health);
        Assert.False(player.IsDead);

        var powered = new PlayerPawn { Health = 30 };
        powered.GivePowerBuddha();
        ActorDamage.Apply(powered, 80, inflictor: missile);
        Assert.Equal(1, powered.Health);
        Assert.False(powered.IsDead);

        var melee = new Actor { Health = 30, Buddha = true };
        var attacker = new Actor { FoilBuddha = true };
        ActorDamage.Apply(melee, 80, attacker, inflictor: attacker);
        Assert.True(melee.IsDead);
    }

    [Fact]
    public void FoilBuddhaIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var first = left.AddBot(80, 0);
        var second = right.AddBot(80, 0);
        first.Brain = second.Brain = null;
        first.FoilBuddha = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        second.FoilBuddha = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(first.FoilBuddha);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void BuddhaIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var first = left.AddBot(80, 0);
        var second = right.AddBot(80, 0);
        first.Brain = second.Brain = null;
        first.Buddha = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        second.Buddha = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(first.Buddha);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void NegativeHealthRoundTripsThroughThePose()
    {
        var sim = Room();
        var player = sim.Players.Single();
        ActorDamage.Apply(player, 250);
        Assert.Equal(-150, player.Health);
        Assert.Equal(ActorStateMachine.Death, player.States.Current);
        var saved = SimSavegame.Write(sim);
        sim.Tick();
        SimSavegame.Apply(sim, saved);
        Assert.Equal(-150, player.Health);
        Assert.Equal(ActorStateMachine.Death, player.States.Current);
        var checksum = sim.Checksum;
        var view = player.ViewHeight;
        sim.Tick();
        SimSavegame.Apply(sim, saved);
        Assert.Equal(-150, player.Health);
        Assert.Equal(ActorStateMachine.Death, player.States.Current);
        // The pose does not store the death view, so another dead tic keeps the lower value.
        Assert.Equal(view - 1, player.ViewHeight, 3);
        Assert.NotEqual(checksum, sim.Checksum);
    }

    private static ActorFrame[] DeathFrames() =>
    [
        new(-1, 0), new(4, 0), new(6, 3), new(-1, 3), new(6, 3),
    ];

    [Fact]
    public void ShootingUsesShootableFlagAndVerticalOpening()
    {
        var sim = Room();
        var target = sim.AddBot(64, 0);
        target.Solid = false;
        Assert.Same(target, CombatTrace.FindTarget(sim, sim.Players.Single(), 100));
        target.Z = Fixed.FromInt(64);
        Assert.Null(CombatTrace.FindTarget(sim, sim.Players.Single(), 100));
        target.Z = default; target.Shootable = false;
        Assert.Null(CombatTrace.FindTarget(sim, sim.Players.Single(), 100));
    }

    [Fact]
    public void SaveRestoresAirborneMomentumAndStateWithoutRepeatingActions()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var actions = 0;
        player.States.Configure(player, new[] { new ActorFrame(30, 0, _ => actions++) }, 0);
        sim.QueueCommand(0, new PlayerCommand { Jump = true, ForwardMove = 8192 });
        sim.Tick();
        var checksum = sim.Checksum;
        var saved = SimSavegame.Write(sim);
        sim.Tick();
        var future = sim.Checksum;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(checksum, sim.Checksum);
        Assert.False(player.OnGround);
        Assert.Equal(1, actions);
        sim.Tick();
        Assert.Equal(future, sim.Checksum);
    }

    [Fact]
    public void SaveRejectsImpossibleCountBeforeAllocatingAndAcceptsVersionOne()
    {
        var bytes = new byte[20];
        SimSavegame.Magic.CopyTo(bytes);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 1);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(12), int.MaxValue);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(12), 0);
        Assert.True(SimSavegame.TryRead(bytes, out _, out _));
    }

    [Fact]
    public void IdenticalCommandStreamsProduceIdenticalFoundationChecksums()
    {
        var left = Room(); var right = Room();
        for (var i = 0; i < 120; i++)
        {
            var command = new PlayerCommand { ForwardMove = (short)(i % 3 * 4096), YawDelta = 60, Jump = i % 35 == 0 };
            left.QueueCommand(0, command); right.QueueCommand(0, command);
            left.Tick(); right.Tick();
            Assert.Equal(left.Checksum, right.Checksum);
        }
    }

    [Fact]
    public void WeaponSwitchLowersAndRaisesBeforeItCanFire()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = 5;
        Assert.Equal(PlayerPawn.WeaponTop, player.WeaponOffsetY);

        sim.QueueCommand(0, new PlayerCommand { Attack = true, WeaponSelections = new byte[] { 3 } });
        sim.Tick();
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
        Assert.Equal(PlayerPawn.WeaponTop + PlayerPawn.WeaponMoveSpeed, player.WeaponOffsetY);
        Assert.False(player.WeaponReady);
        Assert.Equal(5, player.Inventory.Shells);
        Assert.Equal(0, player.WeaponCooldown);

        for (var i = 0; i < 15; i++) sim.Tick();
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Selected);
        Assert.Null(player.Inventory.Pending);
        Assert.Equal(PlayerPawn.WeaponBottom, player.WeaponOffsetY);
        Assert.False(player.WeaponLowering);
        Assert.False(player.WeaponReady);
        Assert.Equal(5, player.Inventory.Shells);

        for (var i = 0; i < 16; i++) sim.Tick();
        Assert.True(player.WeaponReady);
        Assert.Equal(PlayerPawn.WeaponTop, player.WeaponOffsetY);

        sim.QueueCommand(0, new PlayerCommand { Attack = true });
        sim.Tick();
        Assert.Equal(4, player.Inventory.Shells);
        Assert.Equal(35, player.WeaponCooldown);
        sim.QueueCommand(0, new PlayerCommand { Attack = true });
        sim.Tick();
        Assert.Equal(4, player.Inventory.Shells);
        Assert.Equal(34, player.WeaponCooldown);
    }

    [Fact]
    public void ALaterSlotReplacesPendingAndTheReadySlotLeavesIt()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Inventory.Weapons |= WeaponKind.Shotgun | WeaponKind.SuperShotgun;
        player.Inventory.Shells = 8;
        sim.QueueCommand(0, new PlayerCommand { WeaponSelections = new byte[] { 3 } });
        sim.Tick();
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        Assert.Equal(WeaponKind.SuperShotgun, player.Inventory.Pending);

        sim.QueueCommand(0, new PlayerCommand { WeaponSelections = new byte[] { 3 } });
        sim.Tick();
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);

        sim.QueueCommand(0, new PlayerCommand { WeaponSelections = new byte[] { 2 } });
        sim.Tick();
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }

    [Fact]
    public void PendingWeaponIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.Players.Single().Inventory.Pending = WeaponKind.Chaingun;
        right.Players.Single().Inventory.Pending = WeaponKind.Shotgun;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Players.Single().Inventory.Pending = WeaponKind.Chaingun;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(WeaponKind.Chaingun, left.Players.Single().Inventory.Pending);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void ViewBobFollowsHorizontalSpeedAndLeavesEyeHeightAlone()
    {
        var player = new PlayerPawn { BobTimer = 5 };
        player.VelocityX = Fixed.FromDouble(8);
        player.UpdateViewBob();
        Assert.Equal(8, player.ViewBobOffset, 3);
        Assert.Equal(PlayerPawn.StandingViewHeight, player.ViewHeight);

        player.BobTimer = 10;
        player.UpdateViewBob();
        Assert.Equal(0, player.ViewBobOffset, 3);

        player.VelocityX = Fixed.FromDouble(100);
        player.BobTimer = 5;
        player.UpdateViewBob();
        Assert.Equal(8, player.ViewBobOffset, 3);

        player.VelocityX = default;
        player.UpdateViewBob();
        Assert.Equal(0, player.ViewBobOffset, 3);

        var sim = Room();
        var pawn = sim.Players.Single();
        var eye = pawn.ViewHeight;
        sim.Tick();
        Assert.Equal(1, pawn.BobTimer);
        Assert.Equal(0, pawn.ViewBobOffset, 3);
        Assert.Equal(eye, pawn.ViewHeight);
    }

    [Fact]
    public void ViewBobTimerFollowsTheClockThroughAPoseRestore()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.VelocityX = Fixed.FromDouble(8);
        sim.Tick();
        Assert.Equal(sim.Thinkers.Clock.Tic, player.BobTimer);
        var checksum = sim.Checksum;
        var saved = SimSavegame.Write(sim);
        sim.Tick();
        Assert.NotEqual(checksum, sim.Checksum);
        SimSavegame.Apply(sim, saved);
        Assert.Equal(sim.Thinkers.Clock.Tic, player.BobTimer);
        Assert.Equal(checksum, sim.Checksum);
    }

    [Fact]
    public void WeaponBobUsesTheNormalStyleAndStopsWhileFiringOrLowering()
    {
        var player = new PlayerPawn { BobTimer = 16 };
        player.VelocityX = Fixed.FromDouble(8);
        player.VelocityZ = Fixed.FromDouble(40);
        player.UpdateViewBob();
        Assert.Equal(PlayerPawn.MaxBob, player.MovementBob, 3);
        Assert.Equal(0, player.WeaponBobX, 3);
        Assert.Equal(PlayerPawn.MaxBob, player.WeaponBobY, 3);
        Assert.Equal(PlayerPawn.StandingViewHeight, player.ViewHeight);

        player.BobTimer = 0;
        player.UpdateViewBob();
        Assert.Equal(PlayerPawn.MaxBob, player.WeaponBobX, 3);
        Assert.Equal(0, player.WeaponBobY, 3);

        player.BobTimer = 16;
        player.AttackPressed = true;
        player.UpdateViewBob();
        Assert.Equal(0, player.WeaponBobX, 3);
        Assert.Equal(0, player.WeaponBobY, 3);

        var sim = Room();
        var pawn = sim.Players.Single();
        sim.QueueCommand(0, new PlayerCommand { WeaponSelections = new byte[] { 1 } });
        sim.Tick();
        pawn.VelocityX = Fixed.FromDouble(8);
        pawn.BobTimer = 16;
        pawn.UpdateViewBob();
        Assert.Equal(0, pawn.WeaponBobX, 3);
        Assert.Equal(0, pawn.WeaponBobY, 3);

        var firing = Room();
        var shooter = firing.Players.Single();
        firing.QueueCommand(0, new PlayerCommand { Attack = true });
        firing.Tick();
        shooter.VelocityX = Fixed.FromDouble(8);
        shooter.BobTimer = 16;
        shooter.UpdateViewBob();
        Assert.Equal(0, shooter.WeaponBobX, 3);
        Assert.Equal(0, shooter.WeaponBobY, 3);
    }

    [Fact]
    public void Turn180TakesNineTicsAndIgnoresYawUntilItFinishes()
    {
        var sim = Room();
        var player = sim.Players.Single();
        sim.QueueCommand(0, new PlayerCommand { Turn180 = true, YawDelta = 16384 });
        sim.Tick();
        Assert.Equal(20, player.Angle.ToDegrees(), 3);
        Assert.Equal(PlayerPawn.Turn180Ticks - 1, player.TurnTicks);

        for (var i = 0; i < PlayerPawn.Turn180Ticks - 1; i++)
        {
            sim.QueueCommand(0, new PlayerCommand { Turn180 = true, YawDelta = 16384 });
            sim.Tick();
        }
        Assert.Equal(180, player.Angle.ToDegrees(), 3);
        Assert.Equal(0, player.TurnTicks);

        sim.QueueCommand(0, new PlayerCommand { Turn180 = true });
        sim.Tick();
        Assert.Equal(180, player.Angle.ToDegrees(), 3);
        Assert.Equal(0, player.TurnTicks);

        sim.Tick();
        sim.QueueCommand(0, new PlayerCommand { Turn180 = true });
        sim.Tick();
        Assert.Equal(200, player.Angle.ToDegrees(), 3);
        Assert.Equal(PlayerPawn.Turn180Ticks - 1, player.TurnTicks);

        var checksum = sim.Checksum;
        var ticks = player.TurnTicks;
        sim.RestoreState(sim.CaptureState());
        Assert.Equal(ticks, player.TurnTicks);
        Assert.Equal(checksum, sim.Checksum);
        sim.QueueCommand(0, new PlayerCommand { Turn180 = true });
        sim.Tick();
        Assert.Equal(ticks - 1, player.TurnTicks);
    }

    [Fact]
    public void Turn180IsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.Players.Single().TurnTicks = 4;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Players.Single().TurnTicks = left.Players.Single().TurnTicks;
        right.Players.Single().Angle = left.Players.Single().Angle;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(2, left.Players.Single().TurnTicks);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void InstantWeaponSwitchIsReadyOnTheSameTic()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = 5;
        player.InstantWeaponSwitch = true;
        sim.QueueCommand(0, new PlayerCommand { Attack = true, WeaponSelections = new byte[] { 3 } });
        sim.Tick();
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Selected);
        Assert.Null(player.Inventory.Pending);
        Assert.Equal(PlayerPawn.WeaponTop, player.WeaponOffsetY);
        Assert.True(player.WeaponReady);
        Assert.Equal(4, player.Inventory.Shells);
        Assert.Equal(35, player.WeaponCooldown);
    }

    [Fact]
    public void InstantWeaponSwitchIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.Players.Single().InstantWeaponSwitch = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Players.Single().InstantWeaponSwitch = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(left.Players.Single().InstantWeaponSwitch);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void DirectWeaponAssignmentStaysReady()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = 2;
        player.Inventory.Selected = WeaponKind.Shotgun;
        sim.QueueCommand(0, new PlayerCommand { Attack = true });
        sim.Tick();
        Assert.Equal(1, player.Inventory.Shells);
        Assert.True(player.WeaponReady);
    }

    [Fact]
    public void WeaponOffsetIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var command = new PlayerCommand { WeaponSelections = new byte[] { 1 } };
        left.QueueCommand(0, command);
        right.QueueCommand(0, command);
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);
        left.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        var player = left.Players.Single();
        left.RestoreState(left.CaptureState());
        Assert.Equal(PlayerPawn.WeaponTop + PlayerPawn.WeaponMoveSpeed * 2, player.WeaponOffsetY);
        Assert.True(player.WeaponLowering);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void BackpackLiftsTheCapsThenGivesOnePack()
    {
        var player = Room().Players.Single();
        player.Inventory.Bullets = 200;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Backpack));
        Assert.True(player.Inventory.HasBackpack);
        Assert.Equal(400, player.Inventory.MaxBullets);
        Assert.Equal(100, player.Inventory.MaxShells);
        Assert.Equal(100, player.Inventory.MaxRockets);
        Assert.Equal(600, player.Inventory.MaxCells);
        Assert.Equal(210, player.Inventory.Bullets);
        Assert.Equal(4, player.Inventory.Shells);
        Assert.Equal(1, player.Inventory.Rockets);
        Assert.Equal(20, player.Inventory.Cells);
    }

    [Fact]
    public void ASecondBackpackAddsAmmoAndAFullPackIsStillTaken()
    {
        var player = Room().Players.Single();
        player.Inventory.Bullets = 50;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Backpack));
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Backpack));
        Assert.Equal(70, player.Inventory.Bullets);
        Assert.Equal(8, player.Inventory.Shells);
        Assert.Equal(2, player.Inventory.Rockets);
        Assert.Equal(40, player.Inventory.Cells);
        Assert.Equal(400, player.Inventory.MaxBullets);
        player.Inventory.Bullets = 400;
        player.Inventory.Shells = 100;
        player.Inventory.Rockets = 100;
        player.Inventory.Cells = 600;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Backpack));
        Assert.Equal(400, player.Inventory.Bullets);
        Assert.Equal(100, player.Inventory.Shells);
        Assert.Equal(100, player.Inventory.Rockets);
        Assert.Equal(600, player.Inventory.Cells);
        Assert.Equal(400, player.Inventory.MaxBullets);
    }

    [Fact]
    public void PistolStartClearsTheBackpack()
    {
        var player = Room().Players.Single();
        PickupCatalog.TryGive(player, PickupCatalog.Backpack);
        player.Inventory.ResetToPistolStart();
        Assert.False(player.Inventory.HasBackpack);
        Assert.Equal(200, player.Inventory.MaxBullets);
        Assert.Equal(50, player.Inventory.MaxShells);
        Assert.Equal(50, player.Inventory.MaxRockets);
        Assert.Equal(300, player.Inventory.MaxCells);
        Assert.Equal(50, player.Inventory.Bullets);
    }

    [Fact]
    public void ADroppedBackpackRestoresTheCapsAndGivesNoAmmo()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Backpack));
        player.Inventory.Bullets = 250;
        Assert.Null(sim.DropBackpack(new PlayerPawn()));
        var drop = sim.DropBackpack(player);
        Assert.NotNull(drop);
        Assert.True(drop.Depleted);
        Assert.Equal(PickupCatalog.Backpack, drop.DoomEdNum);
        Assert.False(player.Inventory.HasBackpack);
        Assert.Equal(200, player.Inventory.MaxBullets);
        Assert.Equal(50, player.Inventory.MaxShells);
        Assert.Equal(200, player.Inventory.Bullets);
        Assert.Equal(4, player.Inventory.Shells);
        Assert.Equal(1, player.Inventory.Rockets);
        Assert.Equal(20, player.Inventory.Cells);
        Assert.Null(sim.DropBackpack(player));

        drop.VelocityX = default; drop.VelocityY = default; drop.VelocityZ = default;
        drop.NoGravity = true;
        for (var i = 0; i < 29; i++) sim.Tick();
        Assert.False(player.Inventory.HasBackpack);
        sim.Tick();
        Assert.True(player.Inventory.HasBackpack);
        Assert.Equal(400, player.Inventory.MaxBullets);
        Assert.Equal(200, player.Inventory.Bullets);
        Assert.Equal(4, player.Inventory.Shells);
        Assert.Equal(1, player.Inventory.Rockets);
        Assert.Equal(20, player.Inventory.Cells);
        Assert.DoesNotContain(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Backpack);

        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Backpack, depleted: true));
        Assert.Equal(210, player.Inventory.Bullets);
        Assert.Equal(8, player.Inventory.Shells);
    }

    [Fact]
    public void DepletedIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        var leftBot = left.AddBot(8, 8);
        var rightBot = right.AddBot(8, 8);
        leftBot.Brain!.Enabled = false;
        rightBot.Brain!.Enabled = false;
        leftBot.Depleted = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        rightBot.Depleted = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(leftBot.Depleted);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void BackpackIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        Assert.True(PickupCatalog.TryGive(left.Players.Single(), PickupCatalog.Backpack));
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        Assert.True(PickupCatalog.TryGive(right.Players.Single(), PickupCatalog.Backpack));
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(left.Players.Single().Inventory.HasBackpack);
        Assert.Equal(400, left.Players.Single().Inventory.MaxBullets);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void BabyAndNightmareDoubleAmmoAndTheOtherSkillsDoNot()
    {
        var baby = Skilled(0);
        Assert.True(PickupCatalog.TryGive(baby.Players.Single(), PickupCatalog.Clip));
        Assert.Equal(70, baby.Players.Single().Inventory.Bullets);
        Assert.True(PickupCatalog.TryGive(baby.Players.Single(), PickupCatalog.Backpack));
        Assert.Equal(90, baby.Players.Single().Inventory.Bullets);
        Assert.Equal(8, baby.Players.Single().Inventory.Shells);
        Assert.Equal(2, baby.Players.Single().Inventory.Rockets);
        Assert.Equal(40, baby.Players.Single().Inventory.Cells);
        Assert.Equal(400, baby.Players.Single().Inventory.MaxBullets);

        var nightmare = Skilled(4);
        Assert.True(PickupCatalog.TryGive(nightmare.Players.Single(), PickupCatalog.Clip));
        Assert.Equal(70, nightmare.Players.Single().Inventory.Bullets);

        var easy = Skilled(1);
        Assert.True(PickupCatalog.TryGive(easy.Players.Single(), PickupCatalog.Clip));
        Assert.Equal(60, easy.Players.Single().Inventory.Bullets);
    }

    [Fact]
    public void AmmoFactorScalesTheSkillValueAndTruncatesTowardZero()
    {
        var sim = Room();
        sim.AmmoFactor = 1.5;
        var player = sim.Players.Single();
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Clip));
        Assert.Equal(65, player.Inventory.Bullets);
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Rocket));
        Assert.Equal(1, player.Inventory.Rockets);

        sim.AmmoFactor = 0;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Clip));
        Assert.Equal(65, player.Inventory.Bullets);
    }

    [Fact]
    public void ADroppedWeaponIgnoresTheSkillAmmoFactor()
    {
        var doubled = Skilled(0, new LevelThing { Type = PickupCatalog.Shotgun });
        doubled.Tick();
        Assert.Equal(16, doubled.Players.Single().Inventory.Shells);

        var ignored = Skilled(0, new LevelThing { Type = PickupCatalog.Shotgun });
        Assert.Single(ignored.Actors, actor => actor.DoomEdNum == PickupCatalog.Shotgun).IgnoreAmmoSkill = true;
        ignored.Tick();
        Assert.Equal(8, ignored.Players.Single().Inventory.Shells);
    }

    [Fact]
    public void DoubleAmmoReplacesTheSkillFactorWithTwo()
    {
        var normal = Room();
        normal.DoubleAmmo = true;
        Assert.True(PickupCatalog.TryGive(normal.Players.Single(), PickupCatalog.Clip));
        Assert.Equal(70, normal.Players.Single().Inventory.Bullets);

        var easy = Skilled(1);
        easy.DoubleAmmo = true;
        Assert.True(PickupCatalog.TryGive(easy.Players.Single(), PickupCatalog.Clip));
        Assert.Equal(70, easy.Players.Single().Inventory.Bullets);

        var baby = Skilled(0);
        baby.DoubleAmmo = true;
        baby.AmmoFactor = 1.5;
        Assert.True(PickupCatalog.TryGive(baby.Players.Single(), PickupCatalog.Clip));
        Assert.Equal(80, baby.Players.Single().Inventory.Bullets);

        var dropped = Skilled(2, new LevelThing { Type = PickupCatalog.Shotgun });
        dropped.DoubleAmmo = true;
        Assert.Single(dropped.Actors, actor => actor.DoomEdNum == PickupCatalog.Shotgun).IgnoreAmmoSkill = true;
        dropped.Tick();
        Assert.Equal(8, dropped.Players.Single().Inventory.Shells);
    }

    [Fact]
    public void DoubleAmmoIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.DoubleAmmo = true;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.DoubleAmmo = true;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.True(left.DoubleAmmo);
        Assert.Equal(checksum, left.Checksum);
    }

    [Fact]
    public void AmmoFactorIsInTheChecksumAndSurvivesAPoseRestore()
    {
        var left = Room();
        var right = Room();
        left.AmmoFactor = 1.5;
        left.Tick();
        right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        right.AmmoFactor = 1.5;
        left.Tick();
        right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);

        var checksum = left.Checksum;
        left.RestoreState(left.CaptureState());
        Assert.Equal(1.5, left.AmmoFactor);
        Assert.Equal(checksum, left.Checksum);
    }

    private static AuthoritySimulation Skilled(int skill, params LevelThing[] extras) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = new[] { new LevelSector { CeilingHeight = 128 } },
        Things = new[] { new LevelThing { Type = 1 } }.Concat(extras).ToArray(),
    }, spawnOptions: new SpawnOptions(Skill: skill));

    private static AuthoritySimulation Room(short ceiling = 128) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = new[] { new LevelSector { CeilingHeight = ceiling, Tag = 1 } },
        Things = new[] { new LevelThing { Type = 1 } },
    });

    internal static AuthoritySimulation TwoRooms(short floor, short ceiling) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = new[] { new LevelSector { CeilingHeight = 128 }, new LevelSector { FloorHeight = floor, CeilingHeight = ceiling } },
        Sides = new[] { new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 } },
        Things = new[] { new LevelThing { Type = 1, X = -40 } },
        Lines = new[]
        {
            Edge(-128, -128, 0, -128, 0), Edge(0, -128, 128, -128, 1),
            Edge(128, -128, 128, 128, 1), Edge(128, 128, 0, 128, 1),
            Edge(0, 128, -128, 128, 0), Edge(-128, 128, -128, -128, 0),
            new LevelLine { X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 1 },
        },
    });

    private static LevelLine Edge(double x1, double y1, double x2, double y2, int side) =>
        new() { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, SideFront = side, SideBack = -1 };

    private static BotPawn RivalBot(AuthoritySimulation sim, double x, double y) =>
        sim.AddBot(x, y, doomEdNum: 3001);

    private static void ShatterCorpse(AuthoritySimulation sim)
    {
        var bot = sim.AddBot(64, 64);
        bot.Health = 0;
        bot.IceCorpse = true;
        bot.Shootable = true;
        bot.States.Configure(bot, new ActorFrame[] { new(10, 0), new(-1, 0) }, 0);
        ActorDamage.Apply(bot, 5, inflictor: new Actor(), damageType: "Fire");
    }
}
