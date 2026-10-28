using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmPSectorNextSpecialTests
{
    static (VmPSectorNextSpecial.SpecialSector center, VmPSectorNextSpecial.SpecialSector neighbor) LinkedPair(int centerSpecial, int neighborSpecial, int neighborValid = 0)
    {
        var center = new VmPSectorNextSpecial.SpecialSector { Special = centerSpecial };
        var neighbor = new VmPSectorNextSpecial.SpecialSector { Special = neighborSpecial, ValidCount = neighborValid };
        var line = new VmPSectorNextSpecial.SpecialLine { FrontSector = center, BackSector = neighbor };
        center.Lines.Add(line);
        neighbor.Lines.Add(line);
        return (center, neighbor);
    }

    [Fact]
    public void NextSpecialSectorVc_SkipsMatchingValidCount()
    {
        var (center, _) = LinkedPair(1, 7, neighborValid: 3);
        Assert.Null(VmPSectorNextSpecial.NextSpecialSectorVc(center, 7, validCount: 3));
        Assert.NotNull(VmPSectorNextSpecial.NextSpecialSectorVc(center, 7, validCount: 4));
    }

    [Fact]
    public void NextSpecialSector_RespectsNogood()
    {
        var (center, neighbor) = LinkedPair(1, 9);
        Assert.Equal(neighbor, VmPSectorNextSpecial.NextSpecialSector(center, 9, nogood: null));
        Assert.Null(VmPSectorNextSpecial.NextSpecialSector(center, 9, nogood: neighbor));
    }

    [Fact]
    public void NextSpecialSectorAction_MatchesNextSpecialSector()
    {
        var (center, neighbor) = LinkedPair(1, 11);
        Assert.Equal(neighbor, VmPSectorNextSpecial.NextSpecialSectorAction(center, 11, nogood: null));
    }

    [Fact]
    public void IsDamaging_DetectsDamageAmountAndRecentManualDamage()
    {
        var sec = new VmPSectorNextSpecial.DangerSectorModel { DamageAmount = 5 };
        Assert.True(VmPSectorNextSpecial.IsDamaging(sec, moTid: 0));

        sec.DamageAmount = 0;
        sec.LastDamage = 100;
        sec.MapTime = 120;
        Assert.True(VmPSectorNextSpecial.IsDamaging(sec, moTid: 0, gameTicRate: 35));
        sec.MapTime = 276;
        Assert.False(VmPSectorNextSpecial.IsDamaging(sec, moTid: 0, gameTicRate: 35));
    }

    [Fact]
    public void IsDamaging_SectorAction214RequiresTag()
    {
        var sec = new VmPSectorNextSpecial.DangerSectorModel
        {
            SecActTarget = new VmPSectorNextSpecial.SecActTarget { Special = 214 },
        };
        sec.SecActTarget.Args[1] = 10;
        sec.SecActTarget.Args[0] = 5;
        sec.SectorHasTag = tag => tag == 5;
        Assert.True(VmPSectorNextSpecial.IsDamaging(sec, moTid: 0));
        sec.SectorHasTag = _ => false;
        Assert.False(VmPSectorNextSpecial.IsDamaging(sec, moTid: 0));
        sec.SecActTarget.Args[0] = 0;
        Assert.True(VmPSectorNextSpecial.IsDamaging(sec, moTid: 0));
    }

    [Fact]
    public void IsCrushing_UsesCrushDelegates()
    {
        var sec = new VmPSectorNextSpecial.DangerSectorModel
        {
            CeilingData = "c",
            GetCeilingCrush = o => o as string == "c" ? 8 : 0,
            FloorData = "f",
            GetFloorCrush = o => o as string == "f" ? 0 : 0,
        };
        Assert.True(VmPSectorNextSpecial.IsCrushing(sec));
        sec.GetCeilingCrush = _ => 0;
        sec.GetFloorCrush = _ => 4;
        Assert.True(VmPSectorNextSpecial.IsCrushing(sec));
    }

    [Fact]
    public void IsDangerous_Checks3dFloors()
    {
        var sec = new VmPSectorNextSpecial.DangerSectorModel();
        var rover = new VmPSectorNextSpecial.FFloorRover
        {
            Flags = VmPSectorNextSpecial.FfExists,
        };
        rover.Model.DamageAmount = 3;
        rover.Top.ZAtPoint = (_, _) => 64;
        rover.Bottom.ZAtPoint = (_, _) => 32;
        var pos = new VmPSectorNextSpecial.SectorVec3(0, 0, 40);
        Assert.True(VmPSectorNextSpecial.IsDangerous(sec, pos, height: 16, moTid: 0, ffloors: [rover]));
    }
}
