namespace HCDE.Playsim.Tests;

public class VmPInteractionActorDieTests
{
    [Fact]
    public void Die_Missile_ExplodesEarly()
    {
        var exploded = false;
        var a = new VmPInteractionActorDie.DieActor { Flags = VmPInteractionActorDie.MfMissile };
        VmPInteractionActorDie.Die(new VmPInteractionActorDie.DieContext
        {
            Actor = a,
            ExplodeMissile = () => exploded = true,
        });
        Assert.True(exploded);
    }

    [Fact]
    public void Die_NoDieState_NonPlayer_Destroyed()
    {
        var a = new VmPInteractionActorDie.DieActor();
        VmPInteractionActorDie.Die(new VmPInteractionActorDie.DieContext
        {
            Actor = a,
            Destroy = () => a.Destroyed = true,
        });
        Assert.True(a.Destroyed);
    }

    [Fact]
    public void KillDummyMorph_MatchesNativeGate()
    {
        Assert.True(VmPInteractionActorDie.KillDummyMorph(true, true, true, false));
    }

    [Fact]
    public void RandomizeDeathTics_ClampsToOne()
    {
        Assert.Equal(3, VmPInteractionActorDie.RandomizeDeathTics(5, 2));
    }
}
