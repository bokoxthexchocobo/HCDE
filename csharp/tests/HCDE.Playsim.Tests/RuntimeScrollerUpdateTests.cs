using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RuntimeScrollerUpdateTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 },
            new LevelSector { Index = 1, Tag = 7, CeilingHeight = 128 },
            new LevelSector { Index = 2, CeilingHeight = 128 }],
    });

    private static void Update(AuthoritySimulation sim, int special, int x, int mode = 2) =>
        Assert.True(LineSpecials.ExecuteFloorSpecial(sim, special, 7, x, 0, mode, 0));

    [Theory]
    [InlineData(223, false)]
    [InlineData(223, true)]
    [InlineData(224, false)]
    [InlineData(224, true)]
    public void RateUpdatesPreserveControllerHistoryAndVelocity(int special, bool accelerating)
    {
        var sim = Room();
        var plane = special == 223 ? SectorTextureScrollPlane.Floor : SectorTextureScrollPlane.Ceiling;
        sim.AppendControlPlaneScroll(0, 2, -1, 0, accelerating, plane);
        sim.Floors[2] = 2; sim.Tick();
        Update(sim, special, 64, mode: 0);
        var record = sim.CaptureState().TextureScrolls!.Single();
        Assert.Equal(-2, record.Dx);
        Assert.Equal(130, record.LastHeight);
        Assert.Equal(accelerating ? -2 : 0, record.Vdx);
        sim.Floors[2] = 3; sim.Tick();
        var sector = sim.Level.Sectors[0];
        Assert.Equal(accelerating ? -6 : -4,
            plane == SectorTextureScrollPlane.Floor ? sector.FloorTextureOffsetX : sector.CeilingTextureOffsetX);
        Assert.Equal(0, sim.Level.Sectors[1].FloorTextureOffsetX);
        Assert.Equal(0, sim.Level.Sectors[1].CeilingTextureOffsetX);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ZeroRateKeepsControlledCarryAndItsAcceleration(bool accelerating)
    {
        var sim = Room();
        sim.RegisterControlSectorCarry(0, 2, 1, 0, 128, accelerating);
        sim.Floors[2] = 2; sim.Tick();
        Update(sim, 223, 0, mode: 1);
        var record = sim.CaptureState().TextureScrolls!.Single();
        Assert.Equal(0, record.Dx);
        Assert.Equal(accelerating ? 2 : 0, record.Vdx);
        sim.Floors[2] = 3; sim.Tick();
        Assert.Equal(accelerating ? 2 : 0, sim.SectorScrollX[0]);
        Update(sim, 223, 64, mode: 1);
        sim.Floors[2] = 4; sim.Tick();
        Assert.Equal(accelerating ? 4 : 2, sim.SectorScrollX[0]);
        Assert.Equal(0, sim.SectorScrollX[1]);
    }

    [Theory]
    [InlineData(223)]
    [InlineData(224)]
    public void DuplicatePlaneScrollersAreUpdatedWithoutCollapsing(int special)
    {
        var sim = Room();
        var plane = special == 223 ? SectorTextureScrollPlane.Floor : SectorTextureScrollPlane.Ceiling;
        sim.AppendSectorTextureScroll(0, -1, 0, plane);
        sim.AppendSectorTextureScroll(0, -3, 0, plane);
        Update(sim, special, 64, mode: 0); sim.Tick();
        Assert.Equal(2, sim.CaptureState().TextureScrolls!.Count);
        Assert.Equal(-4, plane == SectorTextureScrollPlane.Floor
            ? sim.Level.Sectors[0].FloorTextureOffsetX : sim.Level.Sectors[0].CeilingTextureOffsetX);
    }

    [Fact]
    public void CarryUpdatesPreserveDuplicatesAffectMasksAndAcceleratedVelocity()
    {
        var sim = Room();
        sim.AppendSectorCarryScroll(0, 1, 0, ScrollCarryAffect.Players);
        sim.AppendSectorCarryScroll(0, 3, 0, ScrollCarryAffect.Monsters);
        sim.AppendAcceleratingSectorCarryScroll(0, 1, 0, ScrollCarryAffect.StaticObjects);
        sim.Tick(); Update(sim, 223, 64, mode: 1); sim.Tick();
        Assert.Equal(7, sim.SectorScrollX[0]);
        var records = sim.CaptureState().TextureScrolls!;
        Assert.Equal(3, records.Count);
        Assert.Equal(ScrollCarryAffect.Players, records[0].Affect);
        Assert.Equal(ScrollCarryAffect.Monsters, records[1].Affect);
        Assert.Equal(ScrollCarryAffect.StaticObjects, records[2].Affect);
        Update(sim, 223, 0, mode: 1); sim.Tick();
        Assert.Equal(3, sim.SectorScrollX[0]);
    }

    [Fact]
    public void ZeroRateWithoutThinkersDoesNotCreateThem()
    {
        var sim = Room(); Update(sim, 223, 0); Update(sim, 224, 0);
        Assert.Empty(sim.CaptureState().TextureScrolls!);
    }

    [Fact]
    public void NewNonzeroRateCreatesOnEveryTaggedSectorOnlyWhenNoneExist()
    {
        var sim = Room(); Update(sim, 223, 32); sim.Tick();
        Assert.Equal(4, sim.CaptureState().TextureScrolls!.Count);
        Assert.Equal(-1, sim.Level.Sectors[0].FloorTextureOffsetX);
        Assert.Equal(-1, sim.Level.Sectors[1].FloorTextureOffsetX);
        Assert.Equal(1, sim.SectorScrollX[0]); Assert.Equal(1, sim.SectorScrollX[1]);
    }

    [Fact]
    public void UpdatedControllerStateSurvivesArchiveAndChecksum()
    {
        var original = Room();
        original.AppendControlPlaneScroll(0, 2, -1, 0, true, SectorTextureScrollPlane.Floor);
        original.RegisterControlSectorCarry(0, 2, 1, 0, 128, true);
        original.Floors[2] = 2; original.Tick(); Update(original, 223, 64);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(original), out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        for (var i = 0; i < 4; i++)
        {
            original.Floors[2] += i % 2; restored.Floors[2] = original.Floors[2];
            original.Tick(); restored.Tick();
            Assert.Equal(original.Checksum, restored.Checksum);
            Assert.Equal(original.CaptureState().TextureScrolls, restored.CaptureState().TextureScrolls);
        }
    }

    [Theory]
    [InlineData(223)]
    [InlineData(224)]
    public void ZeroPlaneRateRetainsAcceleratedMotionAndCanResume(int special)
    {
        var sim = Room();
        var plane = special == 223 ? SectorTextureScrollPlane.Floor : SectorTextureScrollPlane.Ceiling;
        sim.AppendControlPlaneScroll(0, 2, -1, 0, true, plane);
        sim.Floors[2] = 2; sim.Tick();
        Update(sim, special, 0, mode: 0); sim.Tick();
        Assert.Equal(-4, plane == SectorTextureScrollPlane.Floor
            ? sim.Level.Sectors[0].FloorTextureOffsetX : sim.Level.Sectors[0].CeilingTextureOffsetX);
        Assert.Single(sim.CaptureState().TextureScrolls!);
        Update(sim, special, 64, mode: 0);
        sim.Floors[2] = 3; sim.Tick();
        Assert.Equal(-8, plane == SectorTextureScrollPlane.Floor
            ? sim.Level.Sectors[0].FloorTextureOffsetX : sim.Level.Sectors[0].CeilingTextureOffsetX);
    }

    [Fact]
    public void InitiallyZeroCarryStillSuppressesCreationOnOtherTaggedSectors()
    {
        var sim = Room(); sim.AppendSectorCarryScroll(0, 0, 0);
        Update(sim, 223, 32, mode: 1); sim.Tick();
        Assert.Single(sim.CaptureState().TextureScrolls!);
        Assert.Equal(1, sim.SectorScrollX[0]); Assert.Equal(0, sim.SectorScrollX[1]);
    }
}
