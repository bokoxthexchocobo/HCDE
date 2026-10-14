using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChainsawHitEffectsTests
{
    [Theory]
    [InlineData(ChainsawHitFlags.None, true, true)]
    [InlineData(ChainsawHitFlags.NoTurn, false, true)]
    [InlineData(ChainsawHitFlags.NoPullIn, true, false)]
    [InlineData(ChainsawHitFlags.NoTurn | ChainsawHitFlags.NoPullIn, false, false)]
    public void TurnAndPullInAreIndependent(ChainsawHitFlags flags, bool turns, bool pulls)
    {
        var (sim, player) = Room();
        ChainsawActions.ApplyHitEffects(player, BamAngle.FromDegrees(90), flags, "custom/sawhit");
        Assert.Equal(turns ? BamAngle.FromDegrees(90 - 90.0 / 21) : BamAngle.FromDegrees(0), player.Angle);
        Assert.Equal(pulls, player.JustAttacked);
        var sound = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("custom/sawhit", sound.Sound); Assert.Equal(1, sound.Channel); Assert.Equal(player.Id, sound.ActorId);
    }

    [Fact]
    public void NoPullInDoesNotClearExistingPendingPull()
    {
        var (_, player) = Room(); player.JustAttacked = true;
        ChainsawActions.ApplyHitEffects(player, BamAngle.FromDegrees(0), ChainsawHitFlags.NoPullIn);
        Assert.True(player.JustAttacked);
    }

    [Fact]
    public void UnsupportedFlagsFailBeforeAnyHitEffects()
    {
        var (sim, player) = Room();
        Assert.Throws<NotSupportedException>(() => ChainsawActions.ApplyHitEffects(player, BamAngle.FromDegrees(90), (ChainsawHitFlags)1));
        Assert.Equal(BamAngle.FromDegrees(0), player.Angle); Assert.False(player.JustAttacked);
        Assert.Empty(sim.DrainSoundRequests());
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
        });
        sim.DrainSoundRequests(); return (sim, sim.Players.Single());
    }
}
