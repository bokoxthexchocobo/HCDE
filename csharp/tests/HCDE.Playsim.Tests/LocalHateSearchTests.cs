using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LocalHateSearchTests
{
    [Fact]
    public void LocalHateePrecedesEarlierGlobalTidMatch()
    {
        var sim = Room(); var hunter = sim.AddBot(40, 40); hunter.TidToHate = 7;
        var far = sim.AddBot(300, 40); far.ThingId = 7;
        var local = sim.AddBot(80, 40); local.ThingId = 7;
        hunter.Brain!.Tick(sim, hunter); Assert.Equal(local.Id, hunter.TargetId);
    }

    [Fact]
    public void LocalActorsCanRevealTheirHatedTargetBehindHunter()
    {
        var sim = Room(); var hunter = sim.AddBot(40, 40); hunter.TidToHate = 7;
        var hatee = sim.AddBot(-200, 40); hatee.ThingId = 7;
        var link = sim.AddBot(80, 40); link.SetTargetPointer(hatee);
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.DontLookAllAround);
        Assert.Equal(hatee.Id, hunter.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HiddenOrDormantLocalLinkCannotRevealHatee(bool dormant)
    {
        var sim = Room(); var hunter = sim.AddBot(40, 40); hunter.TidToHate = 7;
        var hatee = sim.AddBot(-200, 40); hatee.ThingId = 7;
        var link = sim.AddBot(80, 40); link.SetTargetPointer(hatee);
        if (dormant) link.Dormant = true; else link.NoBlockmap = true;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.DontLookAllAround); Assert.Null(hunter.TargetId);
    }

    [Fact]
    public void ActorRadiusLinksAcrossCellBoundary()
    {
        var sim = Room(); var hunter = sim.AddBot(110, 40); hunter.TidToHate = 7;
        var far = sim.AddBot(300, 40); far.ThingId = 7;
        var overlapping = sim.AddBot(130, 40); overlapping.ThingId = 7;
        hunter.Brain!.Tick(sim, hunter); Assert.Equal(overlapping.Id, hunter.TargetId);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 8, 8), []),
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 800 }]
    });
}
