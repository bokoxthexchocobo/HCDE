using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TerrainFrictionTests
{
    [Theory]
    [InlineData(false, 1, 1)]
    [InlineData(false, 0.5, 32.0 / 2048)]
    [InlineData(true, 0.5, 32.0 / 2048)]
    [InlineData(true, 2, 602.3529411764706 / 2048)]
    public void NoGravityAndAirborneMovementUseActorFactorWithoutFloorTerrain(bool airborne, double multiplier, double scale)
    {
        var sim = ParsedRoom(); var player = sim.Players.Single(); player.NoGravity = true;
        player.Friction = Fixed.FromDouble(multiplier);
        if (airborne) { player.Z = Fixed.FromInt(64); player.OnGround = false; }
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
        Assert.Equal(Fixed.FromDouble(scale), player.X);
        Assert.Equal(Fixed.FromDouble(scale * (airborne ? 1 : Math.Clamp(ActorPhysics.GroundFriction * multiplier, 0, 1))), player.VelocityX);
    }

    [Fact]
    public void NoGravityTerrainEligibilitySurvivesSaveAndRestoresOnGravityToggle()
    {
        var sim = ParsedRoom(); var player = sim.Players.Single(); player.NoGravity = true;
        var restored = ParsedRoom(); SimSavegame.Apply(restored, SimSavegame.Write(sim));
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        restored.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick(); restored.Tick(); Assert.Equal(Fixed.FromInt(1), player.X); Assert.Equal(sim.Checksum, restored.Checksum);
        player.NoGravity = false; player.VelocityX = default;
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
        Assert.Equal(Fixed.FromDouble(1 + 1.0 / 64), player.X);
    }

    [Fact]
    public void OlderSaveResetsActorFrictionAndMalformedPresenceFails()
    {
        var sim = ParsedRoom(); var legacy = SimSavegame.Write(sim); var player = sim.Players.Single();
        player.Friction = Fixed.FromInt(2); SimSavegame.Apply(sim, legacy);
        Assert.Equal(Fixed.FromInt(1), player.Friction);
        player.Friction = Fixed.FromInt(2); var saved = SimSavegame.Write(sim);
        Assert.Equal(119, System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var size = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-friction-value", error);
    }

    [Theory]
    [InlineData(0, 32.0 / 2048)]
    [InlineData(0.5, 32.0 / 2048)]
    [InlineData(2, 602.3529411764706 / 2048)]
    public void ActorFrictionRecalculatesGroundAcceleration(double multiplier, double scale)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }],
        });
        var player = sim.Players.Single(); player.Friction = Fixed.FromDouble(multiplier);
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
        Assert.Equal(Fixed.FromDouble(scale), player.X);
    }

    [Fact]
    public void ActorMultiplierReplacesTerrainFactorAndSurvivesSave()
    {
        var sim = ParsedRoom(); var player = sim.Players.Single(); player.Friction = Fixed.FromInt(2);
        var restored = ParsedRoom(); SimSavegame.Apply(restored, SimSavegame.Write(sim));
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        restored.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        sim.Tick(); restored.Tick();
        // Doubled mud friction clamps to one, selecting the ice formula without a mud boost.
        Assert.Equal(Fixed.FromDouble(602.3529411764706 / 2048), player.X);
        Assert.Equal(sim.Checksum, restored.Checksum);
        Assert.Equal(SimSavegame.Write(sim), SimSavegame.Write(restored));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(15000, 1)]
    [InlineData(15001, 2)]
    [InlineData(30000, 2)]
    [InlineData(30001, 4)]
    [InlineData(60000, 4)]
    [InlineData(60001, 8)]
    public void MudAccelerationUsesStrictNativeSpeedThresholds(int speedRaw, int multiplier)
    {
        var sim = ParsedRoom(); var player = sim.Players.Single(); player.VelocityX = new Fixed(speedRaw);
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
        var expected = Fixed.FromDouble(speedRaw / 65536.0 + multiplier / 64.0);
        Assert.Equal(expected, player.X);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AirborneAndFlyingInputIgnoreTerrainMovementFactor(bool airborne, bool flying)
    {
        var sim = ParsedRoom(); var player = sim.Players.Single(); player.NoGravity = true; player.Fly = flying;
        if (airborne) { player.Z = Fixed.FromInt(64); player.OnGround = false; }
        sim.QueueCommand(0, new PlayerCommand { SideMove = 8192 }); sim.Tick();
        Assert.Equal(Fixed.FromInt(-1), player.Y);
    }

    [Fact]
    public void SavedMudMotionContinuesWithSameInputAcceleration()
    {
        var sim = ParsedRoom(); sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
        var restored = ParsedRoom(); SimSavegame.Apply(restored, SimSavegame.Write(sim));
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192, SideMove = 4096 });
        restored.QueueCommand(0, new PlayerCommand { ForwardMove = 8192, SideMove = 4096 });
        sim.Tick(); restored.Tick(); Assert.Equal(sim.Checksum, restored.Checksum);
        Assert.Equal(SimSavegame.Write(sim), SimSavegame.Write(restored));
    }

    private static AuthoritySimulation ParsedRoom()
    {
        Assert.True(TerrainDefinitionParser.TryParseData("terrain Mud { friction 0 } defaultterrain Mud", out var data, out var error), error);
        return AuthoritySimulation.Start(new PlayLevel
        {
            TerrainDefinitions = data.Definitions, DefaultTerrain = data.DefaultTerrain,
            Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }],
        }, rngSeed: 42);
    }

    [Theory]
    [InlineData(false, false, 0.5)]
    [InlineData(true, false, 1)]
    [InlineData(false, true, ActorPhysics.FlyingFriction)]
    public void FloorTerrainDampsGroundedActorsOnly(bool airborne, bool flying, double expected)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.NoGravity = airborne || flying; player.Fly = flying;
        if (airborne) { player.Z = Fixed.FromInt(64); player.OnGround = false; }
        player.VelocityX = Fixed.FromInt(4); sim.Tick();
        Assert.Equal(Fixed.FromDouble(4 * expected), player.VelocityX);
    }

    [Fact]
    public void FrictionConfigurationAndMotionResumeAfterSave()
    {
        var sim = Room(); sim.Players.Single().VelocityX = Fixed.FromInt(4); sim.Tick();
        var restored = Room(); SimSavegame.Apply(restored, SimSavegame.Write(sim));
        sim.Tick(); restored.Tick(); Assert.Equal(sim.Checksum, restored.Checksum);
        Assert.Equal(SimSavegame.Write(sim), SimSavegame.Write(restored));
        var changed = Room(0.75); Assert.NotEqual(Room().Checksum, changed.Checksum);
    }

    private static AuthoritySimulation Room(double friction = 0.5) => AuthoritySimulation.Start(new PlayLevel
    {
        TerrainDefinitions = [new LevelTerrainDefinition("Mud", Friction: friction)], DefaultTerrain = "Mud",
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }],
    }, rngSeed: 42);
}
