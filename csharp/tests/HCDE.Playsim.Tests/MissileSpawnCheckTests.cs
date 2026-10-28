using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MissileSpawnCheckTests
{
    [Fact]
    public void CheckAdvancesSpawnPositionInOpenRoom()
    {
        var sim = Room();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var startX = imp.X.ToDouble();
        var missile = sim.SpawnProjectile(imp, ProjectileKind.ImpBall, sim.Players.Single());
        Assert.True(MissileSpawnCheck.Check(sim, missile, imp, imp.Radius.ToDouble()));
        Assert.True(missile.X.ToDouble() != startX || missile.Y.ToDouble() != imp.Y.ToDouble());
    }

    [Fact]
    public void ClearLastHeardDispatchClearsField()
    {
        var sim = Room();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        imp.LastHeardTargetId = sim.Players.Single().Id;
        Assert.True(ActorActionDispatch.TryInvoke("A_ClearLastHeard", imp));
        Assert.Null(imp.LastHeardTargetId);
    }

    private static AuthoritySimulation Room()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Namespace = "ZDoom",
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Sides = geometry.Sides,
            Lines = geometry.Lines,
            Things = [new LevelThing { Type = 1, X = 0, Y = 0 }, new LevelThing { Type = 3001, X = 64, Y = 0 }],
        });
    }

}
