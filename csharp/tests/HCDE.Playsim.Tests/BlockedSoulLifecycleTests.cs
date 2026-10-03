using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class BlockedSoulLifecycleTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void FailedSpawnReceivesDamageAndRestoresSavedFlags(bool noTeleport, bool parentSolid)
    {
        var sim = Room(noTeleport);
        var parent = sim.Actors.Single(a => a.DoomEdNum == 71); parent.Solid = parentSolid;
        Assert.Null(sim.SpawnLostSoul(parent, sim.Players.Single(), 0));
        var soul = sim.Actors.Single(a => a.DoomEdNum == 3006);
        Assert.True(soul.IsDead); Assert.Equal(100 - ActorDamage.TelefragDamage, soul.Health);
        Assert.Equal(1, soul.DeathCount); Assert.Equal(parent.Id, soul.LastDamageSourceId);
        Assert.False(soul.Brain!.Charging); Assert.Equal(noTeleport, soul.NoTeleport);
        Assert.Equal(parentSolid, parent.Solid);
        var deaths = soul.DeathCount; sim.Tick(); Assert.Equal(deaths, soul.DeathCount);
    }
    [Fact]
    public void FailedChildDoesNotInflateWaveMembershipOrPreventVictory()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 512 }] });
        sim.Invasion.Enabled = true; sim.Invasion.Configure(1, 0, 35); sim.Tick();
        var parent = Assert.Single(sim.Actors.OfType<BotPawn>()); parent.Brain = null;
        ((List<LevelLine>)sim.Level.Lines).Add(new LevelLine {
            X1 = parent.X.ToDouble() + 50, X2 = parent.X.ToDouble() + 50,
            Y1 = -1000, Y2 = 1000, SideBack = -1 });
        Assert.Null(sim.SpawnLostSoul(parent, null, 0));
        Assert.True(sim.Actors.Single(a => a.DoomEdNum == 3006).IsDead);
        Assert.Equal(1, sim.Invasion.Spawned);
        parent.Health = 0; sim.Tick();
        Assert.Equal(InvasionPhase.Victory, sim.Invasion.Phase);
        Assert.Equal(0, sim.Invasion.ActiveMonsters); Assert.Equal(1, sim.Invasion.Cleared);
    }
    private static AuthoritySimulation Room(bool noTeleport) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 71 }, new LevelThing { Type = 1, X = 800 }],
        Lines = [new LevelLine { X1 = 50, X2 = 50, Y1 = -200, Y2 = 200, SideBack = -1 }] },
        dehacked: noTeleport ? DehackedPatch.Apply("Thing 19\nBits = SOLID + SHOOTABLE + COUNTKILL + FLOAT + NOGRAVITY + NOTELEPORT\n") : null);
}