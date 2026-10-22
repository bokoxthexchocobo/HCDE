namespace HCDE.Playsim.Tests;

public class VmADecalSpreadTests
{
    [Fact]
    public void Length_MatchesHypot()
    {
        Assert.Equal(5, VmADecalSpread.Length(3, 4));
    }

    [Fact]
    public void GetWallStuff_FlipsDeltaWhenNotSide0()
    {
        VmADecalSpread.GetWallStuff(false, 0, 0, 1, 0, 10, 0, out var ldx, out _);
        Assert.Equal(-10, ldx);
    }

    [Fact]
    public void Spread_InvokesSpreadLeftAndRight()
    {
        var source = new VmADecalBase.DecalState { PicNum = 1, ScaleX = 1 };
        var wall = new object();
        var touched = 0;
        var wallOps = new VmADecalSpread.SpreadWallOps
        {
            WallIsSide0 = _ => true,
            GetV1X = _ => 0,
            GetV1Y = _ => 0,
            GetDeltaX = _ => 10,
            GetDeltaY = _ => 0,
            GetLeftSide = _ => VmADecalSpread.NoSide,
            GetRightSide = _ => VmADecalSpread.NoSide,
        };
        var baseOps = new VmADecalBase.CloneSelfOps
        {
            CreateThinker = _ => { touched++; return new VmADecalBase.DecalState(); },
            StickToWallValid = (_, _, _, _, _) => false,
            DestroyThinker = _ => { },
        };
        VmADecalSpread.Spread(source, null, wall, 5, 0, 0, null, false, 1, _ => 32, _ => 0, _ => true, wallOps, baseOps, new VmADecalImpact.ImpactCloneSelfOps());
        Assert.Equal(0, touched);
    }
}
