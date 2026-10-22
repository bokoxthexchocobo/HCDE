namespace HCDE.Playsim.Tests;

public class VmPEnemySmartAssistTests
{
    [Fact]
    public void IsOnLift_DetectsPlatThinker()
    {
        var actor = new VmPEnemySmartAssist.MoveActor { SectorFloorData = new object() };
        Assert.True(VmPEnemySmartAssist.IsOnLift(actor, _ => true));
    }

    [Fact]
    public void IsOnLift_DetectsSectorFlag()
    {
        var actor = new VmPEnemySmartAssist.MoveActor { SectorIsLift = true };
        Assert.True(VmPEnemySmartAssist.IsOnLift(actor, _ => false));
    }

    [Fact]
    public void IsUnderDamage_PlayerHarmReturnsSerious()
    {
        var actor = new VmPEnemySmartAssist.MoveActor
        {
            HasPlayer = true,
            AtSectorFloor = true,
            TouchingSectors = new[]
            {
                new VmPEnemySmartAssist.TouchingSector { DamageAmount = 5 },
            },
        };
        Assert.Equal(-1, VmPEnemySmartAssist.IsUnderDamage(actor));
    }

    [Fact]
    public void CheckTags_MatchesPrimaryTag()
    {
        var a = new VmPEnemySmartAssist.TagSector
        {
            HasTags = () => true,
            FirstTag = () => 7,
        };
        var b = new VmPEnemySmartAssist.TagSector
        {
            HasTags = () => true,
            FirstTag = () => 7,
        };
        Assert.True(VmPEnemySmartAssist.CheckTags(a, b));
    }

    [Fact]
    public void FaceTarget_UsesInjectedFace()
    {
        object? faced = null;
        VmPEnemySmartAssist.FaceTarget("self", "target", (_, t) => faced = t);
        Assert.Equal("target", faced);
    }

    [Fact]
    public void DropItem_UsesVmHook()
    {
        var got = VmPEnemySmartAssist.DropItem("src", "type", 1, 256, (_, type, amt, chance) => ValueTuple.Create(type, amt, chance));
        Assert.IsType<ValueTuple<object?, int, int>>(got);
        var tuple = (ValueTuple<object?, int, int>)got!;
        Assert.Equal("type", tuple.Item1);
        Assert.Equal(1, tuple.Item2);
        Assert.Equal(256, tuple.Item3);
    }

    [Fact]
    public void Massacre_CountsMonsterKills()
    {
        var actors = new[]
        {
            new VmPEnemySmartAssist.MassacreActor
            {
                Flags3 = VmPEnemySmartAssist.Mf3Ismonster,
                Massacre = () => 2,
            },
            new VmPEnemySmartAssist.MassacreActor
            {
                Flags2 = VmPEnemySmartAssist.Mf2Dormant,
                Flags3 = VmPEnemySmartAssist.Mf3Ismonster,
                Massacre = () => 1,
            },
            new VmPEnemySmartAssist.MassacreActor
            {
                Flags3 = VmPEnemySmartAssist.Mf3Ismonster,
                Flags = VmPEnemySmartAssist.MfFriendly,
                Massacre = () => 1,
            },
        };
        Assert.Equal(2, VmPEnemySmartAssist.Massacre(actors, true, _ => true));
    }
}
