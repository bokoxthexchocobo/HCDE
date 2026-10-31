using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmSpriteLight1278SliceTests
{
    [Fact]
    public void Grid_FloorsNegativeCellsAndLeavesTheCompiledScan()
    {
        Assert.Equal("0", VmSpriteLight1278.Num((float)VmSpriteLight1278.SmoothStep(0, 1, -1)));
        Assert.Equal("0", VmSpriteLight1278.Num((float)VmSpriteLight1278.SmoothStep(0, 1, 0)));
        Assert.Equal("0.15625", VmSpriteLight1278.Num((float)VmSpriteLight1278.SmoothStep(0, 1, 0.25)));
        Assert.Equal("0.5", VmSpriteLight1278.Num((float)VmSpriteLight1278.SmoothStep(0, 1, 0.5)));
        Assert.Equal("1", VmSpriteLight1278.Num((float)VmSpriteLight1278.SmoothStep(0, 1, 1)));
        Assert.Equal("1", VmSpriteLight1278.Num((float)VmSpriteLight1278.SmoothStep(0, 1, 2)));
        Assert.Equal("0.84375", VmSpriteLight1278.Num((float)VmSpriteLight1278.SmoothStep(1, 0, 0.25)));
        Assert.Equal("0.5", VmSpriteLight1278.Num((float)VmSpriteLight1278.SmoothStep(2, 4, 3)));

        var gate = new VmSpriteLight1278
        {
            Width = 2,
            Height = 2,
            Cells = Cells(
                Cell(new VmSpriteProbe1278 { Id = 9, X = 0, Y = 0, Z = 0, Red = 1 }),
                new VmSpriteProbeCell1278(),
                new VmSpriteProbeCell1278(),
                new VmSpriteProbeCell1278()),
        };
        Assert.Null(gate.FindLightProbe(0, 0, 0));
        Assert.Equal(0, gate.CellReads);

        var grid = new VmSpriteLight1278
        {
            MinX = -1,
            MinY = 1,
            Width = 3,
            Height = 3,
            Probes = { new VmSpriteProbe1278 { Id = 99, X = -40, Y = 10, Z = 0, Red = 9 } },
        };
        grid.Cells = new VmSpriteProbeCell1278[9];
        for (int i = 0; i < 9; i++)
            grid.Cells[i] = new VmSpriteProbeCell1278();
        grid.Cells[0].Probes = new[]
        {
            new VmSpriteProbe1278 { Id = 1, X = -40, Y = 42, Z = 0, Red = 0.25f },
        };
        grid.Cells[1].Probes = new[]
        {
            new VmSpriteProbe1278 { Id = 2, X = -40, Y = 10, Z = 0, Red = 8 },
        };
        VmSpriteProbe1278 neg = grid.FindLightProbe(-40, 10, 0)!;
        Assert.Equal(1, neg.Id);
        Assert.Equal(1, grid.CellReads);
        Assert.Equal("0.25", VmSpriteLight1278.Num(neg.Red));

        var tie = new VmSpriteLight1278
        {
            Width = 2,
            Height = 2,
            Probes = { new VmSpriteProbe1278 { Id = 1 } },
            Cells = Cells(
                Cell(
                    new VmSpriteProbe1278 { Id = 4, X = 6, Y = 8, Z = 0 },
                    new VmSpriteProbe1278 { Id = 5, X = 0, Y = 10, Z = 0 }),
                Cell(new VmSpriteProbe1278 { Id = 6, X = 10, Y = 0, Z = 0 }),
                new VmSpriteProbeCell1278(),
                Cell(new VmSpriteProbe1278 { Id = 8, X = 0, Y = 0, Z = 10 })),
        };
        Assert.Equal(4, tie.FindLightProbe(0, 0, 0)!.Id);

        var closer = new VmSpriteLight1278
        {
            Width = 2,
            Height = 1,
            Probes = { new VmSpriteProbe1278 { Id = 1 } },
            Cells = Cells(
                Cell(new VmSpriteProbe1278 { Id = 4, X = 6, Y = 8, Z = 0 }),
                Cell(new VmSpriteProbe1278 { Id = 7, X = 0, Y = 0, Z = 1 })),
        };
        Assert.Equal(7, closer.FindLightProbe(0, 0, 0)!.Id);

        var sprite = World();
        var section = Sec(0);
        float[] color = { 4, 5, 6 };
        var flagged = new VmSpriteActor1278
        {
            RenderFlags2 = VmSpriteLight1278.NoDynamicLighting,
            Section = section,
        };
        sprite.GetDynSpriteLight(flagged, null, color);
        Assert.Equal("4 5 6", Fmt(color));
        Assert.Equal(0, sprite.CellReads);

        color[0] = 7;
        color[1] = 8;
        color[2] = 9;
        sprite.GetDynSpriteLight(null, null, color);
        Assert.Equal("7 8 9", Fmt(color));

        var particle = new VmSpriteParticle1278
        {
            X = 4,
            Y = 5,
            Z = 6,
            Flags = VmSpriteLight1278.ParticleNoDynamicLighting,
            Subsector = new VmSpriteSubsector1278 { Section = section, PortalGroup = 3 },
        };
        sprite.GetDynSpriteLight(null, particle, color);
        Assert.Equal("7 8 9", Fmt(color));
    }

    [Fact]
    public void Lights_FallOffAcrossTheSpotAndSkipARepeatedModelLight()
    {
        float[] color = new float[3];
        var direct = World();
        var lamp = Lamp(1, 255, 0, 0);
        direct.FlatLists[0].Add(new VmSpriteLightNode1278(lamp));
        direct.GetDynSpriteLight(null, 0, 0, 0, Sec(0), 4, color);
        Assert.Equal("1 0 0", Fmt(color));
        Assert.Equal(3, lamp.IntensityReads);
        Assert.Equal(1, lamp.RadiusReads);
        Assert.Null(lamp.LastActor);
        Assert.Equal(0, direct.ShadowTests);

        var bound = World();
        var edge = Lamp(2, 255, 0, 0);
        edge.Radius = 16;
        bound.FlatLists[0].Add(new VmSpriteLightNode1278(edge));
        bound.GetDynSpriteLight(null, 16, 0, 0, Sec(0), 0, color);
        Assert.Equal("0 0 0", Fmt(color));
        Assert.Equal(1, edge.RadiusReads);

        var probeAdd = World();
        probeAdd.Probes.Add(new VmSpriteProbe1278 { Id = 1 });
        probeAdd.Width = 1;
        probeAdd.Height = 1;
        probeAdd.Cells = Cells(Cell(new VmSpriteProbe1278 { Id = 3, Red = 0.25f, Green = 0.5f, Blue = -1f }));
        probeAdd.FlatLists[0].Add(new VmSpriteLightNode1278(Lamp(3, 255, 0, 0)));
        probeAdd.GetDynSpriteLight(null, 0, 0, 0, Sec(0), 1, color);
        Assert.Equal("1.25 0.5 -1", Fmt(color));

        var shortList = World();
        shortList.Probes.Add(new VmSpriteProbe1278 { Id = 1 });
        shortList.Width = 1;
        shortList.Height = 1;
        shortList.Cells = Cells(Cell(new VmSpriteProbe1278 { Id = 8, Red = 0.125f }));
        shortList.GetDynSpriteLight(null, 0, 0, 0, Sec(1), 1, color);
        Assert.Equal("0.125 0 0", Fmt(color));

        var off = World();
        off.DisplacementCount = 2;
        off.Offsets[(2, 5)] = (8, -4);
        var moved = Lamp(4, 255, 0, 0);
        moved.SectorGroup = 2;
        off.FlatLists[0].Add(new VmSpriteLightNode1278(moved));
        off.GetDynSpriteLight(null, 0, 0, 0, Sec(0), 5, color);
        Assert.Equal("0.440982997 0 0", Fmt(color));

        var same = World();
        same.DisplacementCount = 2;
        same.Offsets[(2, 2)] = (8, -4);
        var stayed = Lamp(5, 255, 0, 0);
        stayed.SectorGroup = 2;
        same.FlatLists[0].Add(new VmSpriteLightNode1278(stayed));
        same.GetDynSpriteLight(null, 0, 0, 0, Sec(0), 2, color);
        Assert.Equal("1 0 0", Fmt(color));

        var zeroFrom = World();
        zeroFrom.DisplacementCount = 2;
        zeroFrom.Offsets[(2, 5)] = (8, -4);
        var zeroed = Lamp(6, 255, 0, 0);
        zeroed.SectorGroup = 0;
        zeroFrom.FlatLists[0].Add(new VmSpriteLightNode1278(zeroed));
        zeroFrom.GetDynSpriteLight(null, 0, 0, 0, Sec(0), 5, color);
        Assert.Equal("1 0 0", Fmt(color));

        var toZero = World();
        toZero.DisplacementCount = 2;
        toZero.Offsets[(2, 5)] = (8, -4);
        var toward = Lamp(17, 255, 0, 0);
        toward.SectorGroup = 2;
        toZero.FlatLists[0].Add(new VmSpriteLightNode1278(toward));
        toZero.GetDynSpriteLight(null, 0, 0, 0, Sec(0), 0, color);
        Assert.Equal("1 0 0", Fmt(color));

        var noTable = World();
        noTable.Offsets[(2, 5)] = (8, -4);
        var plain = Lamp(7, 255, 0, 0);
        plain.SectorGroup = 2;
        noTable.FlatLists[0].Add(new VmSpriteLightNode1278(plain));
        noTable.GetDynSpriteLight(null, 0, 0, 0, Sec(0), 5, color);
        Assert.Equal("1 0 0", Fmt(color));

        var spot = World();
        var cone = Lamp(8, 255, 128, 64);
        cone.Spot = true;
        cone.Radius = 32;
        cone.OuterDegrees = 90;
        cone.Intensity = 2;
        spot.FlatLists[0].Add(new VmSpriteLightNode1278(cone));
        spot.GetDynSpriteLight(null, 16, 0, 0, Sec(0), 1, color);
        Assert.Equal("1 0.501960814 0.250980407", Fmt(color));
        Assert.Equal(3, cone.IntensityReads);
        Assert.Equal(0, spot.ShadowTests);

        var away = World();
        var back = Lamp(9, 255, 0, 0);
        back.Spot = true;
        back.Radius = 32;
        back.Yaw = 180;
        back.OuterDegrees = 90;
        back.ShadowMapped = true;
        away.FlatLists[0].Add(new VmSpriteLightNode1278(back));
        away.GetDynSpriteLight(null, 16, 0, 0, Sec(0), 1, color);
        Assert.Equal("0 0 0", Fmt(color));
        Assert.Equal(1, back.RadiusReads);
        Assert.Equal(0, away.ShadowTests);

        var origin = World();
        var boom = Lamp(10, 255, 0, 0);
        boom.Spot = true;
        boom.ShadowMapped = true;
        boom.OuterDegrees = 90;
        origin.FlatLists[0].Add(new VmSpriteLightNode1278(boom));
        origin.GetDynSpriteLight(null, 0, 0, 0, Sec(0), 1, color);
        Assert.Equal("0 0 0", Fmt(color));
        Assert.Equal(1, boom.RadiusReads);
        Assert.Equal(0, origin.ShadowTests);

        var shade = World();
        var dim = Lamp(11, 255, 0, 0);
        dim.ShadowMapped = true;
        dim.ShadowPass = false;
        shade.FlatLists[0].Add(new VmSpriteLightNode1278(dim));
        shade.GetDynSpriteLight(null, 4, 5, 6, Sec(0), 1, color);
        Assert.Equal("0 0 0", Fmt(color));
        Assert.Equal(1, shade.ShadowTests);
        Assert.Equal(1, dim.ShadowCalls);
        Assert.Equal("4", VmSpriteLight1278.Num(shade.LastShadowX));
        Assert.Equal("5", VmSpriteLight1278.Num(shade.LastShadowY));
        Assert.Equal("6", VmSpriteLight1278.Num(shade.LastShadowZ));
        Assert.Equal(2, dim.RadiusReads);

        var pass = World();
        var lit = Lamp(18, 255, 0, 0);
        lit.ShadowMapped = true;
        pass.FlatLists[0].Add(new VmSpriteLightNode1278(lit));
        pass.GetDynSpriteLight(null, 0, 0, 0, Sec(0), 1, color);
        Assert.Equal("1 0 0", Fmt(color));
        Assert.Equal(1, pass.ShadowTests);

        var drop = World();
        var second = Lamp(12, 255, 0, 0);
        second.ShadowMapped = true;
        second.UseSecondRadius = true;
        second.SecondRadius = 0;
        drop.FlatLists[0].Add(new VmSpriteLightNode1278(second));
        drop.GetDynSpriteLight(null, 0, 0, 0, Sec(0), 1, color);
        Assert.Equal("0 0 0", Fmt(color));
        Assert.Equal(0, drop.ShadowTests);
        Assert.Equal(2, second.RadiusReads);

        var sub = World();
        var negc = Lamp(13, 255, 128, 64);
        negc.Subtractive = true;
        negc.Intensity = 2;
        sub.FlatLists[0].Add(new VmSpriteLightNode1278(negc));
        sub.GetDynSpriteLight(null, 0, 0, 0, Sec(0), 1, color);
        Assert.Equal("-0.293430567 -1.28950894 -1.79146981", Fmt(color));
        Assert.Equal(3, negc.IntensityReads);

        var faded = World();
        var tinted = Lamp(14, 255, 0, 0);
        tinted.Target = new VmSpriteActor1278
        {
            RenderFlags2 = VmSpriteLight1278.LightMultAlpha,
            Alpha = 0.5,
        };
        faded.FlatLists[0].Add(new VmSpriteLightNode1278(tinted));
        faded.GetDynSpriteLight(null, 0, 0, 0, Sec(0), 1, color);
        Assert.Equal("0.5 0 0", Fmt(color));

        var skipActor = World();
        var blocked = Lamp(15, 255, 0, 0);
        blocked.LightActor = false;
        skipActor.FlatLists[0].Add(null);
        skipActor.FlatLists[0].Add(new VmSpriteLightNode1278(blocked));
        skipActor.GetDynSpriteLight(null, 0, 0, 0, Sec(0), 1, color);
        Assert.Equal("0 0 0", Fmt(color));
        Assert.Equal(1, blocked.ActorChecks);
        Assert.Equal(0, blocked.RadiusReads);

        var route = World();
        route.DisplacementCount = 1;
        route.Offsets[(2, 4)] = (0, 0);
        route.Offsets[(2, 9)] = (48, 0);
        var routeLight = Lamp(16, 255, 0, 0);
        routeLight.Radius = 32;
        routeLight.SectorGroup = 2;
        route.FlatLists[0].Add(new VmSpriteLightNode1278(routeLight));
        var thing = new VmSpriteActor1278
        {
            Id = 70,
            X = 16,
            Y = 0,
            Center = 0,
            Section = Sec(0),
            PortalGroup = 4,
        };
        var part = new VmSpriteParticle1278
        {
            X = 0,
            Y = 0,
            Z = 0,
            Subsector = new VmSpriteSubsector1278 { Section = Sec(0), PortalGroup = 9 },
        };
        route.GetDynSpriteLight(thing, part, color);
        Assert.Equal("0.5 0 0", Fmt(color));
        Assert.Equal(70, routeLight.LastActor!.Id);

        routeLight.ActorChecks = 0;
        routeLight.LastActor = null;
        thing.RenderFlags2 = VmSpriteLight1278.NoDynamicLighting;
        color[0] = 3;
        color[1] = 3;
        color[2] = 3;
        route.GetDynSpriteLight(thing, part, color);
        Assert.Equal("0 0 0", Fmt(color));
        Assert.Null(routeLight.LastActor);
        Assert.Equal(1, routeLight.ActorChecks);

        var model = new VmSpriteLight1278();
        model.FlatLists.Add(new List<VmSpriteLightNode1278?>());
        model.FlatLists.Add(new List<VmSpriteLightNode1278?>());
        model.FlatLists.Add(new List<VmSpriteLightNode1278?>());
        var skipSec = new VmSpriteSection1278 { Index = 0, ValidCount = 2 };
        var midSec = new VmSpriteSection1278 { Index = 1, ValidCount = 0 };
        var lastSec = new VmSpriteSection1278 { Index = 2, ValidCount = 5 };
        var skipLight = RelLamp(21, 0, 0, 0, 10);
        var near = RelLamp(22, 0, 0, 0, 1);
        var edgeLight = RelLamp(23, 4, 0, 0, 1);
        var extra = RelLamp(24, 0, 3, 0, 2);
        model.FlatLists[0].Add(new VmSpriteLightNode1278(skipLight));
        model.FlatLists[1].Add(null);
        model.FlatLists[1].Add(new VmSpriteLightNode1278(near));
        model.FlatLists[1].Add(new VmSpriteLightNode1278(edgeLight));
        model.FlatLists[2].Add(new VmSpriteLightNode1278(near));
        model.FlatLists[2].Add(new VmSpriteLightNode1278(extra));
        model.Walk.Add(new VmSpriteSubsector1278 { Section = skipSec, PortalGroup = 4 });
        model.Walk.Add(new VmSpriteSubsector1278 { Section = midSec, PortalGroup = 7 });
        model.Walk.Add(new VmSpriteSubsector1278 { Section = lastSec, PortalGroup = 8 });
        model.Added.Add(new VmSpriteAdded1278 { Id = 1, Group = 2, Force = false });
        model.GetDynModelLight(new VmSpriteActor1278 { Id = 3, RenderRadius = 3 });
        Assert.Equal("9", VmSpriteLight1278.Num(model.LastRadiusSquared));
        Assert.Equal(2, model.ValidCount);
        Assert.Equal(2, model.ListChecks);
        Assert.Equal(2, model.Added.Count);
        Assert.Equal(22, model.Added[0].Id);
        Assert.Equal(7, model.Added[0].Group);
        Assert.True(model.Added[0].Force);
        Assert.Equal(24, model.Added[1].Id);
        Assert.Equal(8, model.Added[1].Group);
        Assert.True(model.Added[1].Force);
        Assert.Equal(2, skipSec.ValidCount);
        Assert.Equal(0, midSec.ValidCount);
        Assert.Equal(5, lastSec.ValidCount);
        Assert.Equal(2, near.ActorChecks);
        Assert.Equal(1, edgeLight.RadiusReads);
        Assert.Equal(0, skipLight.ActorChecks);
        model.GetDynModelLight(null);
        Assert.Empty(model.Added);
        Assert.Equal(2, model.ValidCount);
        Assert.Equal("9", VmSpriteLight1278.Num(model.LastRadiusSquared));

        var cast = new VmSpriteLight1278();
        cast.FlatLists.Add(new List<VmSpriteLightNode1278?>());
        cast.FlatLists[0].Add(new VmSpriteLightNode1278(RelLamp(31, 16777216, 0, 0, 0.5f)));
        cast.FlatLists[0].Add(new VmSpriteLightNode1278(RelLamp(32, 16777217, 0, 0, 0.5f)));
        cast.Walk.Add(new VmSpriteSubsector1278
        {
            Section = new VmSpriteSection1278 { Index = 0 },
            PortalGroup = 6,
        });
        cast.GetDynModelLight(new VmSpriteActor1278 { X = 16777217 });
        Assert.Single(cast.Added);
        Assert.Equal(31, cast.Added[0].Id);
        Assert.Equal(6, cast.Added[0].Group);
        Assert.True(cast.Added[0].Force);
        Assert.Equal("0", VmSpriteLight1278.Num(cast.LastRadiusSquared));
    }

    static VmSpriteLight1278 World()
    {
        var level = new VmSpriteLight1278();
        level.FlatLists.Add(new List<VmSpriteLightNode1278?>());
        return level;
    }

    static VmSpriteSection1278 Sec(int index)
    {
        return new VmSpriteSection1278 { Index = index };
    }

    static VmSpriteDynLight1278 Lamp(int id, int red, int green, int blue)
    {
        return new VmSpriteDynLight1278
        {
            Id = id,
            Radius = 16,
            Red = red,
            Green = green,
            Blue = blue,
        };
    }

    static VmSpriteDynLight1278 RelLamp(int id, double x, double y, double z, float radius)
    {
        return new VmSpriteDynLight1278
        {
            Id = id,
            Radius = radius,
            RelX = x,
            RelY = y,
            RelZ = z,
        };
    }

    static VmSpriteProbeCell1278 Cell(params VmSpriteProbe1278[] probes)
    {
        return new VmSpriteProbeCell1278 { Probes = probes };
    }

    static VmSpriteProbeCell1278[] Cells(params VmSpriteProbeCell1278[] cells)
    {
        return cells;
    }

    static string Fmt(float[] color)
    {
        return VmSpriteLight1278.Num(color[0]) + " " + VmSpriteLight1278.Num(color[1]) + " " + VmSpriteLight1278.Num(color[2]);
    }
}
