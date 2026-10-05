namespace HCDE.Playsim.Tests;

public class PowerupDurationTests
{
    [Theory]
    [InlineData(200, 300, false, false, 200)]
    [InlineData(200, 300, false, true, 300)]
    [InlineData(200, 100, false, true, 200)]
    [InlineData(200, 100, true, false, 300)]
    [InlineData(128, 300, false, false, 300)]
    [InlineData(128, 20, false, false, 128)]
    [InlineData(200, 0, true, true, 200)]
    public void NativeRefreshRules(int current, int incoming, bool additive, bool always, int expected)
    {
        Assert.Equal(expected, PowerupDuration.Merge(current, incoming, additive, always));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void AllPlayerGrantPathsUseAdditiveAndAlwaysPickup(int power)
    {
        var player = new PlayerPawn();
        void Grant(int tics, bool additive = false, bool always = false)
        {
            if (power == 0) player.GivePowerDamage(tics, additive, always);
            else if (power == 1) player.GivePowerProtection(tics, additive, always);
            else player.GivePowerBuddha(tics, additive, always);
        }
        int Timer() => power == 0 ? player.PowerDamageTics : power == 1 ? player.PowerProtectionTics : player.PowerBuddhaTics;
        Grant(200); Grant(50, additive: true); Assert.Equal(250, Timer());
        Grant(500); Assert.Equal(250, Timer());
        Grant(500, always: true); Assert.Equal(500, Timer());
        player.Tick(); Assert.Equal(499, Timer());
    }

    [Fact]
    public void InvalidDurationAndOverflowDoNotChangeTimer()
    {
        var player = new PlayerPawn { PowerDamageTics = int.MaxValue };
        Assert.Throws<ArgumentOutOfRangeException>(() => player.GivePowerDamage(-1));
        Assert.Throws<OverflowException>(() => player.GivePowerDamage(1, additiveTime: true));
        Assert.Equal(int.MaxValue, player.PowerDamageTics);
    }
}
