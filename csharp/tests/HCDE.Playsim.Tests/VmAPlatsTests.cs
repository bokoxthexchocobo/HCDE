using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmAPlatsTests
{
    [Fact]
    public void Tick_Waiting_CountdownFlipsToUp()
    {
        var sec = new VmAPlats.PlatSector { FloorPlaneD = 10 };
        var plat = new VmAPlats.PlatThinker
        {
            Sector = sec,
            Status = VmAPlats.PlatWaiting,
            Count = 1,
            Low = 10,
            Type = VmAPlats.PlatPerpetualRaise,
        };
        VmAPlats.Tick(plat);
        Assert.Equal(VmAPlats.PlatUp, plat.Status);
    }

    [Fact]
    public void Reactivate_Toggle_SwapsDirection()
    {
        var plat = new VmAPlats.PlatThinker { Type = VmAPlats.PlatToggle, OldStatus = VmAPlats.PlatUp };
        VmAPlats.Reactivate(plat);
        Assert.Equal(VmAPlats.PlatDown, plat.Status);
    }

    [Fact]
    public void ActivateInStasisPlat_ReactivatesMatchingTag()
    {
        var plat = new VmAPlats.PlatThinker { Tag = 5, Status = VmAPlats.PlatInStasis, OldStatus = VmAPlats.PlatDown };
        VmAPlats.ActivateInStasisPlat(5, new[] { plat });
        Assert.Equal(VmAPlats.PlatDown, plat.Status);
    }

    [Fact]
    public void EvDoPlat_RaiseAndStay_SetsUpStatus()
    {
        var sec = new VmAPlats.PlatSector
        {
            FloorPlaneD = 0,
            PointToDist = (_, _, h) => h,
        };
        var ctx = new VmAPlats.EvDoPlatContext
        {
            Sectors = { sec },
            SectorTagIterator = (_, _) => new[] { 0 },
            FindNextHighestFloor = (_, spot) => { spot.Vertex = new object(); return 64; },
            CreatePlat = s => new VmAPlats.PlatThinker { Sector = s },
        };
        Assert.True(VmAPlats.EvDoPlat(ctx, 1, null, VmAPlats.PlatRaiseAndStay, 0, 4, 35, 0, 0));
    }

    [Fact]
    public void EvDoPlat_Toggle_ActivatesInStasis()
    {
        var inStasis = new VmAPlats.PlatThinker
        {
            Tag = 3,
            Status = VmAPlats.PlatInStasis,
            OldStatus = VmAPlats.PlatUp,
            Type = VmAPlats.PlatToggle,
            Sector = new VmAPlats.PlatSector(),
        };
        var sec = new VmAPlats.PlatSector { FloorPlaneD = 0, PointToDist = (_, _, h) => h };
        var ctx = new VmAPlats.EvDoPlatContext
        {
            Sectors = { sec },
            ExistingPlats = new[] { inStasis },
            SectorTagIterator = (_, _) => Array.Empty<int>(),
        };
        VmAPlats.EvDoPlat(ctx, 3, null, VmAPlats.PlatToggle, 0, 4, 35, 0, 0);
        Assert.Equal(VmAPlats.PlatDown, inStasis.Status);
    }

    [Fact]
    public void GetSoundByType_Stone_ReturnsFloor()
    {
        var plat = new VmAPlats.PlatThinker { Type = VmAPlats.PlatDownWaitUpStayStone };
        Assert.Equal("Floor", VmAPlats.GetSoundByType(plat));
    }

    [Fact]
    public void Tick_UpPastDest_RaiseAndStayDestroys()
    {
        var destroyed = false;
        var sec = new VmAPlats.PlatSector
        {
            MoveFloor = (_, _, _, _, _, _) => VmADoorTickSound.MoveResult.PastDest,
        };
        var plat = new VmAPlats.PlatThinker
        {
            Sector = sec,
            Status = VmAPlats.PlatUp,
            Type = VmAPlats.PlatRaiseAndStay,
            DestroySelf = () => destroyed = true,
        };
        VmAPlats.Tick(plat);
        Assert.True(destroyed);
    }
}
