using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorDamageFlagTests
{
    [Theory]
    [InlineData(false, false, 1, 95)]
    [InlineData(false, true, 1, 95)]
    [InlineData(false, false, 3001, 60)]
    [InlineData(true, false, 3001, 55)]
    [InlineData(false, true, 3001, 60)]
    [InlineData(true, true, 3001, 55)]
    public void NonPlayerDamageRequiresSectorOptIn(bool hurt, bool air, int type, int expected)
    {
        var sim = Room(hurt, air, type, 5); var actor = sim.Actors.Single(); actor.Brain = null;
        sim.Tick(); Assert.Equal(expected, actor.Health);
    }

    [Theory]
    [InlineData(false, 1, 100)]
    [InlineData(true, 1, 95)]
    [InlineData(false, 3001, 60)]
    [InlineData(true, 3001, 55)]
    public void AirborneDamageRequiresHarmInAir(bool air, int type, int expected)
    {
        var sim = Room(true, air, type, 5); var actor = sim.Actors.Single(); actor.Brain = null;
        actor.NoGravity = true; actor.Z = Fixed.FromInt(16);
        sim.Tick(); Assert.Equal(expected, actor.Health);
    }

    [Theory]
    [InlineData(55, 60)]
    [InlineData(20, 30)]
    [InlineData(80, 80)]
    [InlineData(0, 0)]
    public void MonsterHealingUsesSpawnHealth(int initial, int expected)
    {
        var sim = Room(true, false, 3001, -10); var actor = sim.Actors.Single();
        actor.Brain = null; actor.Health = initial;
        sim.Tick(); Assert.Equal(expected, actor.Health);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void DamageTransferPreservesDestinationContactFlags(bool hurt, bool air)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, HurtMonsters = hurt, HarmInAir = air },
                new LevelSector { Index = 1, CeilingHeight = 128, DamageAmount = 5, DamageInterval = 1,
                    HurtMonsters = !hurt, HarmInAir = !air }],
            Sides = [new LevelSide { Sector = 1 }],
        });
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
            { Special = 239, Arg0 = 7, Arg1 = 8, Arg2 = 4, SideFront = 0, PlayerUse = true }, true));
        Assert.Equal(5, sim.Level.Sectors[0].DamageAmount);
        Assert.Equal(hurt, sim.Level.Sectors[0].HurtMonsters);
        Assert.Equal(air, sim.Level.Sectors[0].HarmInAir);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ContactFlagsParticipateInChecksum(bool hurt, bool air)
    {
        Assert.NotEqual(Room(false, false, 1, 5).Checksum, Room(hurt, air, 1, 5).Checksum);
    }

    private static AuthoritySimulation Room(bool hurt, bool air, int type, int amount) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128, DamageAmount = amount, DamageInterval = 1,
            HurtMonsters = hurt, HarmInAir = air }],
        Things = [new LevelThing { Type = type }],
    });
}
