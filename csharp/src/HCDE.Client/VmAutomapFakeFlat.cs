namespace HCDE.Client;

/// <summary><c>AM_FakeFlat</c> from <c>am_map.cpp</c> (1947–2054).</summary>
public static class VmAutomapFakeFlat
{
    [Flags]
    public enum HeightSecFlags
    {
        None = 0,
        ClipFakePlanes = 1,
        FakeFloorOnly = 2,
        NoFakeLight = 4,
    }

    public enum FakeFlatArea
    {
        Normal = 0,
        Below = -1,
        Above = 1,
    }

    public readonly struct FakeFlatInput
    {
        public FakeFlatInput(
            bool sectorHasHeightSec,
            bool viewerSectorHasHeightSec,
            double viewZ,
            double heightSecFloorZAtView,
            double heightSecCeilingZAtView,
            HeightSecFlags heightSecFlags)
        {
            SectorHasHeightSec = sectorHasHeightSec;
            ViewerSectorHasHeightSec = viewerSectorHasHeightSec;
            ViewZ = viewZ;
            HeightSecFloorZAtView = heightSecFloorZAtView;
            HeightSecCeilingZAtView = heightSecCeilingZAtView;
            HeightSecFlags = heightSecFlags;
        }

        public bool SectorHasHeightSec { get; }
        public bool ViewerSectorHasHeightSec { get; }
        public double ViewZ { get; }
        public double HeightSecFloorZAtView { get; }
        public double HeightSecCeilingZAtView { get; }
        public HeightSecFlags HeightSecFlags { get; }
    }

    public enum FakeFlatOutcome
    {
        UseOriginalSector,
        UseDestBuffer,
        UseOriginalAfterFakeFloorOnlyBelow,
    }

    public static FakeFlatArea ResolveViewerArea(in FakeFlatInput input)
    {
        if (!input.ViewerSectorHasHeightSec)
            return FakeFlatArea.Normal;

        if (input.ViewZ <= input.HeightSecFloorZAtView)
            return FakeFlatArea.Below;

        if (input.ViewZ > input.HeightSecCeilingZAtView
            && (input.HeightSecFlags & HeightSecFlags.FakeFloorOnly) == 0)
            return FakeFlatArea.Above;

        return FakeFlatArea.Normal;
    }

    public static bool ShouldPassThrough(in FakeFlatInput input)
    {
        if (!input.SectorHasHeightSec)
            return true;
        return false;
    }

    public static FakeFlatOutcome PlanFakeFlat(in FakeFlatInput input)
    {
        if (ShouldPassThrough(input))
            return FakeFlatOutcome.UseOriginalSector;

        var area = ResolveViewerArea(input);
        var diffTex = (input.HeightSecFlags & HeightSecFlags.ClipFakePlanes) != 0;

        if (diffTex
            && (input.HeightSecFlags & HeightSecFlags.FakeFloorOnly) != 0
            && area == FakeFlatArea.Below)
        {
            return FakeFlatOutcome.UseDestBuffer;
        }

        if (diffTex
            && (input.HeightSecFlags & HeightSecFlags.FakeFloorOnly) != 0
            && area != FakeFlatArea.Below)
        {
            return FakeFlatOutcome.UseOriginalAfterFakeFloorOnlyBelow;
        }

        if (area == FakeFlatArea.Below || area == FakeFlatArea.Above)
            return FakeFlatOutcome.UseDestBuffer;

        return FakeFlatOutcome.UseDestBuffer;
    }

    public static bool ShouldCopyColormap(FakeFlatArea area) =>
        area == FakeFlatArea.Below || area == FakeFlatArea.Above;

    public static bool ShouldApplyFakeLight(HeightSecFlags flags) =>
        (flags & HeightSecFlags.NoFakeLight) == 0;

    public static int PickFloorTextureSource(
        FakeFlatArea area,
        bool diffTex,
        bool ceilingIsSkyFlat,
        int sectorFloorTex,
        int heightSecFloorTex,
        int heightSecCeilingTex)
    {
        if (area == FakeFlatArea.Below)
            return diffTex ? sectorFloorTex : heightSecFloorTex;

        if (area == FakeFlatArea.Above)
        {
            if (!ceilingIsSkyFlat)
                return heightSecFloorTex;
            return heightSecCeilingTex;
        }

        return sectorFloorTex;
    }
}
