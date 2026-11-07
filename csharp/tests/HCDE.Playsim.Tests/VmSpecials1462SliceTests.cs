using Xunit;

namespace HCDE.Playsim.Tests;

public class VmSpecials1462SliceTests
{
    static VmSpecials1462Slice.Sector Sec(int special = 0, int tag = 0) =>
        new() { Special = special, Tag = tag };

    static VmSpecials1462Slice.Line Ln(VmSpecials1462Slice.Sector front, int special, params int[] args)
    {
        var line = new VmSpecials1462Slice.Line { Front = front, Special = special, Args = new int[5] };
        for (int i = 0; i < args.Length && i < 5; i++)
            line.Args[i] = args[i];
        return line;
    }

    [Fact]
    public void SetupOrderSkipsPlainSectorsAndOpensScripts()
    {
        var plain = Sec();
        var marked = Sec(68);
        var world = new VmSpecials1462Slice.World
        {
            Sectors = { plain, marked },
            Actors =
            {
                new VmSpecials1462Slice.Actor { Name = "Player" },
                new VmSpecials1462Slice.Actor { Name = "SkyCamCompat" },
            },
        };
        VmSpecials1462Slice.Spawn(world);
        Assert.Equal(new[] { 1 }, world.Inited);
        Assert.Equal(68, marked.Special);
        Assert.Equal(
            new[]
            {
                "SetupPortals", "InitSectorSpecial", "ProcessEDSectors", "SpawnScrollers",
                "SpawnFriction", "SpawnPushers", "SpawnSkybox:1", "StartTypedScripts",
            },
            world.Calls);
        Assert.Equal(1, world.OpenScripts);
        Assert.False(world.OpenFromActor);
        Assert.False(world.OpenAlways);
    }

    [Fact]
    public void HeightsPortalsLiftsAndStaticInit()
    {
        var control = Sec();
        var water = Sec(tag: 4);
        var dry = Sec(tag: 9);
        var lift = Sec();
        var hurt = Sec(tag: 3);
        var sky = Sec(tag: 8);
        var world = new VmSpecials1462Slice.World
        {
            ForceWater = true,
            Sectors = { control, water, dry, lift, hurt, sky },
        };
        var heights = Ln(control, VmSpecials1462Slice.TransferHeights, 4, 8 | 32);
        heights.Index = 2;
        var forced = Ln(control, VmSpecials1462Slice.TransferHeights, 9, 0);
        var generic = Ln(control, VmSpecials1462Slice.GenericLift, 0, 0, 0, 0);
        generic.Flags = VmSpecials1462Slice.RepeatSpecial;
        generic.Back = 3;
        var stay = Ln(control, VmSpecials1462Slice.PlatDownWaitUpStay, 0);
        stay.Flags = VmSpecials1462Slice.RepeatSpecial;
        stay.Back = 3;
        var damage = Ln(control, VmSpecials1462Slice.StaticInit, 3, VmSpecials1462Slice.InitDamage);
        damage.Delta = 50;
        var transfer = Ln(control, VmSpecials1462Slice.StaticInit, 8, VmSpecials1462Slice.InitTransferSky);
        transfer.Index = 11;
        var plane = Ln(control, VmSpecials1462Slice.SectorSetPortal, 1, 3, 1, 0, 40);
        var anchor = Ln(control, VmSpecials1462Slice.SectorSetPortal, 2, 6, 0, 1, 0);
        var link = Ln(control, VmSpecials1462Slice.SectorSetLink, 7, 1, 2, 3);
        var quick = Ln(control, VmSpecials1462Slice.LineQuickPortal);
        quick.Index = 15;
        world.Lines.AddRange(new[] { heights, forced, generic, stay, damage, transfer, plane, anchor, link, quick });

        VmSpecials1462Slice.Spawn(world);

        Assert.Equal(
            VmSpecials1462Slice.Underwater | VmSpecials1462Slice.NoFakeLight | VmSpecials1462Slice.ForcedUnderwater,
            control.MoreFlags);
        Assert.True(world.HasHeightSecs);
        Assert.Same(control, water.HeightSec);
        Assert.True(water.FloorClipAdjusted);
        Assert.Contains(water, control.FakeFloors);
        Assert.Equal(0, water.MoreFlags);
        Assert.Same(control, dry.HeightSec);
        Assert.Equal(VmSpecials1462Slice.ForcedUnderwater, control.MoreFlags & VmSpecials1462Slice.ForcedUnderwater);
        Assert.Equal(VmSpecials1462Slice.Lift, lift.MoreFlags);
        Assert.Equal(50, hurt.DamageAmount);
        Assert.Equal(256, hurt.LeakyDamage);
        Assert.Equal(1, hurt.DamageInterval);
        Assert.Equal("None", hurt.DamageType);
        Assert.Equal((12) | VmSpecials1462Slice.SkyFlat, sky.SkyTransfer);
        var copy = Assert.Single(world.Spawns, s => s.Kind == "CopyPortal");
        Assert.Equal(1, copy.A);
        Assert.Equal(1, copy.B);
        Assert.Equal(0, copy.C);
        Assert.Equal(VmSpecials1462Slice.PortsPlane, copy.D);
        Assert.True(copy.Flag);
        Assert.DoesNotContain(world.Spawns, s => s.Kind == "SectorPortal");
        Assert.DoesNotContain(world.Spawns, s => s.Kind == "SectorLinks");
        Assert.Equal("LinePortal", world.Spawns[^1].Kind);
        Assert.Equal(301, world.Spawns[^1].B);
    }
}
