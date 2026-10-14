using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileSkyExplodeTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void SkyAndHorizonContactsRespectExplosionFlag(int surface, bool explode)
    {
        var (sim, missile, target) = Setup(surface);
        Assert.True(AcsActorFlags.TrySet(missile, "Actor.SKYEXPLODE", explode));
        Assert.True(AcsActorFlags.TryGet(missile, "skyexplode", out var flag)); Assert.Equal(explode, flag);
        missile.Tick(); Assert.True(missile.Destroyed);
        Assert.Equal(explode, target.Health < 1000);
        var sounds = sim.DrainSoundRequests();
        if (explode) Assert.Equal("weapons/rocklx", Assert.Single(sounds).Sound);
        else Assert.Empty(sounds);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(1000, sim.Level.Sectors[0].HealthCeiling);
        if (surface == 2) Assert.Equal(1000, sim.Level.Lines[0].Health);
    }

    [Fact]
    public void ExplosionCountStillDefersSkyExplosion()
    {
        var (sim, missile, target) = Setup(0); missile.SkyExplode = missile.ExplodeCount = true;
        ActorPropertyActions.SetChaseThreshold(missile, 2, setDefault: true);
        missile.Tick(); Assert.False(missile.Destroyed); Assert.Equal(1000, target.Health); Assert.Empty(sim.DrainSoundRequests());
        missile.Tick(); Assert.True(missile.Destroyed); Assert.True(target.Health < 1000); Assert.Single(sim.DrainSoundRequests());
    }

    [Fact]
    public void ArchivePreservesFlagAndRejectsMalformedValue()
    {
        var (sim, missile, _) = Setup(0); var legacy = SimSavegame.Write(sim); missile.SkyExplode = true;
        var bytes = SimSavegame.Write(sim); Assert.Equal(198, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        missile.SkyExplode = false; SimSavegame.Apply(sim, bytes); Assert.True(missile.SkyExplode);
        Assert.Equal(bytes, SimSavegame.Write(sim)); var hash = sim.Checksum;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-sky-explode-value", error);
        SimSavegame.Apply(sim, legacy); Assert.False(missile.SkyExplode); Assert.NotEqual(hash, sim.Checksum);
        Assert.Equal(legacy, SimSavegame.Write(sim));
    }

    private static (AuthoritySimulation, ProjectileActor, Actor) Setup(int surface)
    {
        var level = new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 64, FloorPic = "F_SKY1", CeilingPic = "F_SKY1",
                HealthFloor = 1000, HealthCeiling = 1000 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = surface == 2 ? [new LevelLine { X1 = 64, Y1 = 128, X2 = 64, Y2 = -128,
                SideFront = 0, SideBack = -1, Special = 9, Health = 1000 }] : [],
        };
        var sim = AuthoritySimulation.Start(level);
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(0, 60, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket); sim.DrainSoundRequests();
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(surface == 0 ? 1 : surface == 1 ? 55 : 20);
        missile.VelocityX = Fixed.FromInt(surface == 2 ? 80 : 1); missile.VelocityY = default;
        missile.VelocityZ = Fixed.FromInt(surface == 0 ? -10 : surface == 1 ? 10 : 0);
        return (sim, missile, target);
    }
}
