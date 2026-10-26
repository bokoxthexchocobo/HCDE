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
    public void Die_ClearsShootableFloatSkullfly_AndMarksKilled()
    {
        var a = new VmPInteractionActorDie.DieActor
        {
            Flags = VmPInteractionActorDie.MfShootable | VmPInteractionActorDie.MfFloat | VmPInteractionActorDie.MfSkullfly,
        };
        VmPInteractionActorDie.Die(new VmPInteractionActorDie.DieContext
        {
            Actor = a,
            StateProbe = new VmPInteractionActorDie.DieStateProbe { GenericDeathFound = true },
        });
        Assert.Equal(0, a.Flags & (VmPInteractionActorDie.MfShootable | VmPInteractionActorDie.MfFloat | VmPInteractionActorDie.MfSkullfly));
        Assert.NotEqual(0, a.Flags6 & VmPInteractionActorDie.Mf6Killed);
    }

    [Fact]
    public void Die_Unmorphed_DestroysBeforeDieState()
    {
        var a = new VmPInteractionActorDie.DieActor
        {
            Flags = VmPInteractionActorDie.MfUnmorphed,
            HasPlayer = true,
        };
        VmPInteractionActorDie.Die(new VmPInteractionActorDie.DieContext
        {
            Actor = a,
            Destroy = () => a.Destroyed = true,
        });
        Assert.True(a.Destroyed);
        Assert.Null(a.DieState);
    }

    [Fact]
    public void Die_Fraglimit_ExitCallback()
    {
        var exited = false;
        var a = new VmPInteractionActorDie.DieActor { HasPlayer = true };
        VmPInteractionActorDie.Die(new VmPInteractionActorDie.DieContext
        {
            Actor = a,
            Deathmatch = true,
            Maptime = 1,
            SourceIsPlayer = true,
            Fraglimit = 10,
            SourceFragTotal = 10,
            SourcePlayer = new VmPInteractionActorDie.SourcePlayerCredit(),
            VictimPlayer = new VmPInteractionActorDie.VictimPlayerCredit(),
            StateProbe = new VmPInteractionActorDie.DieStateProbe { GenericDeathFound = true },
            ExitLevelFraglimit = () => exited = true,
        });
        Assert.True(exited);
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

    [Fact]
    public void ClearShootableFloatSkullfly_PreservesOtherBits()
    {
        const int other = 1 << 20;
        var cleared = VmPInteractionActorDie.ClearShootableFloatSkullfly(
            VmPInteractionActorDie.MfShootable | other);
        Assert.Equal(other, cleared);
    }
}
