using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MasterRetaliationClassTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void MasterOrMinionClassIsProtectedIncludingOtherClassInstances(bool minion, bool sameInstance)
    {
        var (sim, hunter, source) = Room();
        if (minion)
        {
            var master = sameInstance ? hunter : sim.AddBot(600, 0, 3004);
            source.MasterId = master.Id;
        }
        else
        {
            var master = sameInstance ? source : sim.AddBot(600, 0, 3001);
            hunter.MasterId = master.Id;
        }
        ActorDamage.Apply(hunter, 1, source, DamageFlags.NoPain);
        Assert.Null(hunter.Brain!.TargetId);
        Assert.Equal(0, hunter.ReactionTime);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HateExceptionsPermitRetaliationAgainstMaster(bool differentHate)
    {
        var (_, hunter, source) = Room(); hunter.MasterId = source.Id;
        if (differentHate) source.TidToHate = 17;
        else { hunter.TidToHate = 7; source.ThingId = 7; }
        ActorDamage.Apply(hunter, 1, source, DamageFlags.NoPain);
        Assert.Equal(source.Id, hunter.Brain!.TargetId);
    }

    private static (AuthoritySimulation, Actor, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 200 }],
        });
        return (sim, sim.Actors[0], sim.Actors[1]);
    }
}
