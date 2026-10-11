using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class OnlySlamSolidTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    public void ContactGateSurvivesSaveAndPreservesChargeOnlyForNonsolidUnshootable(bool flag, bool solid, bool shootable)
    {
        var sim = Room(); var soul = sim.Actors.Single(a => a.DoomEdNum == 3006);
        var target = sim.Players.Single(); target.Solid = solid; target.Shootable = shootable;
        target.SpecialPickup = true; target.NoPain = true;
        Assert.True(AcsActorFlags.TrySet(soul, "Actor.ONLYSLAMSOLID", flag));
        Assert.True(AcsActorFlags.TryGet(soul, "onlyslamsolid", out var value)); Assert.Equal(flag, value);
        soul.Brain!.StartCharge(soul, target); var bytes = SimSavegame.Write(sim);
        if (flag) Assert.Equal(146, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        soul.OnlySlamSolid = !flag; SimSavegame.Apply(sim, bytes); Assert.Equal(flag, soul.OnlySlamSolid);
        for (var i = 0; i < 4 && soul.Brain.Charging; i++) soul.Tick();
        var pass = flag && !solid && !shootable;
        Assert.Equal(pass, soul.Brain.Charging);
        Assert.Equal(!pass, sim.CaptureState().MissileDamageRandomState.HasValue);
        if (pass) Assert.True(soul.X.ToDouble() > target.X.ToDouble());
        Assert.Equal(shootable, target.Health < 100);
        if (flag)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
            bytes[bytes.Length - size + 8] = 2;
            Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
            Assert.Equal("save-only-slam-solid-invalid", error);
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1, X = 64 }, new LevelThing { Type = 3006 }] });
}
