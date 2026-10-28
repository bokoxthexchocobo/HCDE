namespace HCDE.Playsim.Tests;

public class VmPMobjPortalBonesTests
{
    [Fact]
    public void SetViewRoll_SetsInterpCheat()
    {
        var actor = new VmPMobjPortalBones.OrientationActorLike { PlayerCheats = 0 };
        VmPMobjPortalBones.SetViewRoll(actor, 15, VmPMobjPortalBones.SpfInterpolate);
        Assert.Equal(15, actor.ViewRoll);
        Assert.NotEqual(0, actor.PlayerCheats & VmPMobjPortalBones.CfInterpviewangles);
    }

    [Fact]
    public void GetModelTimer_ClientSide()
    {
        Assert.Equal(42, VmPMobjPortalBones.GetModelTimer(true, 42, 1));
        Assert.Equal(99, VmPMobjPortalBones.GetModelTimer(false, 42, 99));
    }

    [Fact]
    public void GetPortalTransition_CeilingCrossing()
    {
        var sec = new VmPMobjPortalBones.PortalSector { CeilingBlocks = false, CeilingPortalZ = 10 };
        var actor = new VmPMobjPortalBones.PortalActor { Z = 12, Sector = sec, Pos = (0, 0, 12) };
        var ctx = new VmPMobjPortalBones.PortalLevelContext
        {
            PosRelativeCeiling = _ => (5, 5, 5),
            PointInSector = _ => sec
        };
        var (moved, pos, _) = VmPMobjPortalBones.GetPortalTransition(ctx, actor, 0);
        Assert.True(moved);
        Assert.Equal((5, 5, 5), pos);
    }

    [Fact]
    public void CheckPortalTransition_LinksWhenMoved()
    {
        var linked = false;
        var actor = new VmPMobjPortalBones.PortalActor
        {
            Z = 20,
            Sector = new() { CeilingBlocks = false, CeilingPortalZ = 10 }
        };
        VmPMobjPortalBones.CheckPortalTransition(
            new()
            {
                PosRelativeCeiling = p => (p.X + 1, p.Y, 5),
                PointInSector = _ => actor.Sector,
                LinkToWorld = () => linked = true
            },
            actor,
            isLinked: true);
        Assert.True(linked);
    }

    [Fact]
    public void GetBoneTrs_ReturnsStoredBone()
    {
        var data = new VmPMobjPortalBones.ModelData { Flags = VmPMobjPortalBones.ModeldataGetBoneInfo };
        data.ModelBoneInfo.Add(new());
        data.ModelBoneInfo[0].Bones.Add(new() { Tx = 1, Ty = 2, Tz = 3 });
        var trs = VmPMobjPortalBones.GetBoneTrs(data, 0, 0, false);
        Assert.Equal(1, trs.Tx);
    }

    [Fact]
    public void GetBoneMatrix_CopiesFlatMatrix()
    {
        var data = new VmPMobjPortalBones.ModelData { Flags = VmPMobjPortalBones.ModeldataGetBoneInfo };
        data.ModelBoneInfo.Add(new());
        data.ModelBoneInfo[0].Positions.Add(Enumerable.Range(0, 16).Select(i => (double)i).ToArray());
        Span<double> mat = stackalloc double[16];
        VmPMobjPortalBones.GetBoneMatrix(data, 0, 0, false, mat);
        Assert.Equal(5, mat[5]);
    }

    [Fact]
    public void GetBoneEulerAngles_ReturnsZeroWhenPicnumOverrides()
    {
        var data = new VmPMobjPortalBones.ModelData { Flags = VmPMobjPortalBones.ModeldataGetBoneInfo };
        var (yaw, pitch, roll) = VmPMobjPortalBones.GetBoneEulerAngles(
            data, picnumValid: true, 0, 0, false, _ => (0, 1, 0));
        Assert.Equal(0, yaw);
        Assert.Equal(0, pitch);
        Assert.Equal(0, roll);
    }

    [Fact]
    public void GetBoneEulerAngles_DerivesYawFromForward()
    {
        var data = new VmPMobjPortalBones.ModelData { Flags = VmPMobjPortalBones.ModeldataGetBoneInfo };
        data.ModelBoneInfo.Add(new());
        data.ModelBoneInfo[0].Positions.Add(Enumerable.Repeat(0.0, 16).ToArray());
        var (yaw, _, _) = VmPMobjPortalBones.GetBoneEulerAngles(
            data, false, 0, 0, false, _ => (1, 0, 1));
        Assert.Equal(45, yaw, 3);
    }

    [Fact]
    public void CalcBones_ProcessesWhenEligible()
    {
        var processed = false;
        var data = new VmPMobjPortalBones.ModelData
        {
            Flags = VmPMobjPortalBones.ModeldataGetBoneInfo
        };
        VmPMobjPortalBones.CalcBones(
            new()
            {
                FoundModelFrame = true,
                ModelsAmount = 1,
                TotalTime = 10,
                CalcModelOverrides = _ => true,
                ProcessModelFrame = (_, _, _) => processed = true
            },
            data,
            recalc: false);
        Assert.True(processed);
    }
}
