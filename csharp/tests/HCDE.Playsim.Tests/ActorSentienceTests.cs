namespace HCDE.Playsim.Tests;

public class ActorSentienceTests
{
    [Theory]
    [InlineData(100, -1, false)]
    [InlineData(100, 0, true)]
    [InlineData(1, 1, true)]
    [InlineData(0, 0, false)]
    [InlineData(-1, 0, false)]
    [InlineData(100, int.MaxValue, false)]
    public void SentienceRequiresPositiveHealthAndRepresentedSeeState(int health, int seeState, bool expected)
    {
        var actor = new Actor { SeeState = seeState, Health = health };
        Assert.Equal(expected, actor.IsSentient());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BrainMonsterAndDormantFlagsDoNotReplaceSeeState(bool monster)
    {
        var actor = new Actor { Health = 100, IsMonster = monster, Dormant = true,
            Brain = new MonsterBrain(MonsterAttack.Melee) { Enabled = false }, SeeState = -1 };
        Assert.False(actor.IsSentient());
        actor.SeeState = 0; Assert.True(actor.IsSentient());
        actor.Brain = null; Assert.True(actor.IsSentient());
    }

    [Fact]
    public void QueryTracksDeathAndHealthRestorationWithoutChangingActor()
    {
        var actor = new Actor { SeeState = 0, Health = 100 };
        var state = actor.States.Current;
        Assert.True(actor.IsSentient()); Assert.Equal(state, actor.States.Current);
        actor.Health = 0; Assert.False(actor.IsSentient());
        actor.RestoreHealth(100); Assert.True(actor.IsSentient());
        actor.SeeState = -1; Assert.False(actor.IsSentient());
    }
}
