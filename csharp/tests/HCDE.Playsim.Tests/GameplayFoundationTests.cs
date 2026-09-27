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
        ActorDamage.Apply(player, 100); player.Health = -100;
        Assert.Equal(1, player.DeathCount);
        Assert.False(player.BlocksActors);
        Assert.False(player.CanTakeDamage);
        for (var i = 0; i < 6; i++) player.Tick();
        Assert.Equal(ActorStateMachine.Corpse, player.States.Current);
    }

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
}
