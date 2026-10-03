using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class DynamicExtendedFlagsTests
{
    [Theory]
    [InlineData("DORMANT")]
    [InlineData("INVULNERABLE")]
    [InlineData("NOTELEPORT")]
    [InlineData("CANSLIDE")]
    [InlineData("LOGRAV")]
    public void DynamicSpawnUsesPatchedExtendedClassDefaults(string flag)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {flag}\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel(), dehacked: patch);
        var bot = sim.AddBot(0, 0);
        Assert.Equal(flag == "DORMANT", bot.Dormant);
        Assert.Equal(flag == "INVULNERABLE", bot.Invulnerable);
        Assert.Equal(flag == "NOTELEPORT", bot.NoTeleport);
        Assert.Equal(flag == "CANSLIDE", bot.CanSlide);
        Assert.Equal(flag == "LOGRAV" ? 0.25 : 1, bot.Gravity.ToDouble());
        if (bot.Dormant)
        {
            Assert.Equal(-1, bot.States.RemainingTics);
            Assert.True(ThingActivation.Execute(sim, bot, 0, true)); Assert.False(bot.Dormant);
        }
        if (bot.Invulnerable)
        {
            var health = bot.Health; ActorDamage.Apply(bot, 5); Assert.Equal(health, bot.Health);
        }
    }
    [Fact]
    public void UnpatchedOtherClassKeepsItsDefaults()
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = DORMANT+INVULNERABLE+LOGRAV\n");
        var sim = AuthoritySimulation.Start(new PlayLevel(), dehacked: patch);
        var bot = sim.AddBot(0, 0, 3001);
        Assert.False(bot.Dormant); Assert.False(bot.Invulnerable); Assert.Equal(1, bot.Gravity.ToDouble());
    }
}