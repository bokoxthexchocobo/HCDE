using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HearingDamageEligibilityTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LivingUnshootableListenerCanHear(bool remembered)
    {
        var (sim, actor, source) = Room(3001); actor.Shootable = false; actor.Vulnerable = false;
        Assert.False(actor.CanTakeDamage); Wake(sim, actor, source, remembered);
        Assert.Equal(source.Id, actor.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ShootableCatalogMarkerIsEligibleDespiteDamageFilter(bool remembered)
    {
        var (sim, actor, source) = Room(LineSpecials.TeleportDestType); source.Shootable = true; source.Health = 100;
        Assert.False(source.CanTakeDamage); Wake(sim, actor, source, remembered);
        Assert.Equal(source.Id, actor.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeadSourceIsRejected(bool remembered)
    {
        var (sim, actor, source) = Room(3001); source.Health = 0;
        Wake(sim, actor, source, remembered); Assert.Null(actor.TargetId);
    }

    [Fact]
    public void DeadListenerDoesNotWakeFromDirectHearing()
    {
        var (sim, actor, source) = Room(3001); actor.Health = 0;
        actor.Brain!.Hear(sim, actor, source); Assert.Null(actor.TargetId);
    }

    private static void Wake(AuthoritySimulation sim, Actor actor, Actor source, bool remembered)
    {
        if (remembered)
        {
            actor.LastHeardTargetId = source.Id;
            actor.Brain!.Tick(sim, actor, useMelee: false, useMissile: false);
        }
        else actor.Brain!.Hear(sim, actor, source);
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Source) Room(int sourceType)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = sourceType, X = 300 }] });
        return (sim, sim.Actors[0], sim.Actors[1]);
    }
}
