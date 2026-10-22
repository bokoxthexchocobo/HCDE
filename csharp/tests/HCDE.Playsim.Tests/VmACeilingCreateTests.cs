namespace HCDE.Playsim.Tests;

public class VmACeilingCreateTests
{
    [Fact]
    public void CreateCeiling_RaiseByValue_SetsUpDirection()
    {
        var sec = new VmACeilingCreate.CeilingSector
        {
            CeilingPlaneD = 128,
            CeilingZAtPoint = (_, _) => 64,
            PointToDist = (_, _, z) => z * 2,
        };
        sec.Lines.Add(new VmACeilingCreate.CeilingLine());
        VmACeilingCreate.CeilingThinker? made = null;
        var ok = VmACeilingCreate.CreateCeiling(new VmACeilingCreate.CreateCeilingContext
        {
            CreateThinker = (s, sp1, sp2, sil) =>
            {
                made = new VmACeilingCreate.CeilingThinker { Sector = s, Speed = sp1, Speed1 = sp1, Speed2 = sp2, Silent = sil };
                return made;
            },
            PlayCeilingSound = _ => { },
        }, sec, VmACeilingCreate.CeilRaiseByValue, null, 1, 4, 8, 16, 0, 0, 0, VmACeilingCreate.CrushDoom);
        Assert.True(ok);
        Assert.NotNull(made);
        Assert.Equal(1, made!.Direction);
        Assert.Equal(160, made.TopHeight);
    }

    [Fact]
    public void CreateCeiling_LowerByValue_SetsDownDirection()
    {
        var sec = new VmACeilingCreate.CeilingSector
        {
            CeilingZAtPoint = (_, _) => 100,
            PointToDist = (_, _, z) => z,
        };
        sec.Lines.Add(new VmACeilingCreate.CeilingLine());
        VmACeilingCreate.CeilingThinker? made = null;
        VmACeilingCreate.CreateCeiling(new VmACeilingCreate.CreateCeilingContext
        {
            CreateThinker = (s, sp1, sp2, sil) =>
            {
                made = new VmACeilingCreate.CeilingThinker { Sector = s, Speed = sp1, Speed1 = sp1, Speed2 = sp2 };
                return made;
            },
            PlayCeilingSound = _ => { },
        }, sec, VmACeilingCreate.CeilLowerByValue, null, 0, 4, 0, 32, 0, 0, 0, 0);
        Assert.NotNull(made);
        Assert.Equal(-1, made!.Direction);
        Assert.Equal(68, made.BottomHeight);
    }

    [Fact]
    public void CreateCeiling_SkipsWhenPlaneMoving()
    {
        var sec = new VmACeilingCreate.CeilingSector();
        var ok = VmACeilingCreate.CreateCeiling(new VmACeilingCreate.CreateCeilingContext
        {
            PlaneMovingCeiling = _ => true,
        }, sec, VmACeilingCreate.CeilLowerByValue, null, 0, 4, 0, 8, 0, 0, 0, 0);
        Assert.False(ok);
    }
}
