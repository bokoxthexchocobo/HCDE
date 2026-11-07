using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDoomSound1427SliceTests
{
    static VmDoomSound1427Slice.State Origin() => new()
    {
        ListenX = 10,
        ListenY = 20,
        ListenZ = 30,
        FloorZ = 10,
        CeilingZ = 40,
        Channel = VmDoomSound1427Slice.Interior,
    };

    [Fact]
    public void SectorAndPolyOrigins()
    {
        Assert.Equal(240, VmDoomSound1427Slice.BandEnd - VmDoomSound1427Slice.BandStart + 1);
        Assert.Equal(1014, VmDoomSound1427Slice.SaveEnd);
        Assert.Equal(1255, VmDoomSound1427Slice.ReadSound);
        var inside = Origin();
        inside.Channel = VmDoomSound1427Slice.Floor;
        inside.FloorZ = 40;
        VmDoomSound1427Slice.SectorOrigin(inside);
        Assert.Equal(10f, inside.Pos.X);
        Assert.Equal(20f, inside.Pos.Z);
        Assert.Equal(30f, inside.Pos.Y);
        var wall = Origin();
        wall.Inside = false;
        wall.ClosestX = 4;
        wall.ClosestY = 6;
        wall.Channel = VmDoomSound1427Slice.Ceiling;
        wall.CeilingZ = 50;
        VmDoomSound1427Slice.SectorOrigin(wall);
        Assert.Equal(4f, wall.Pos.X);
        Assert.Equal(6f, wall.Pos.Z);
        Assert.Equal(50f, wall.Pos.Y);
        var low = Origin();
        low.Channel = VmDoomSound1427Slice.Ceiling;
        low.CeilingZ = 25;
        VmDoomSound1427Slice.SectorOrigin(low);
        Assert.Equal(30f, low.Pos.Y);
        var clamped = Origin();
        clamped.ListenZ = 5;
        clamped.FloorZ = 10;
        clamped.CeilingZ = 20;
        VmDoomSound1427Slice.SectorOrigin(clamped);
        Assert.Equal(10f, clamped.Pos.Y);
        var high = Origin();
        high.ListenZ = 25;
        high.FloorZ = 10;
        high.CeilingZ = 20;
        VmDoomSound1427Slice.SectorOrigin(high);
        Assert.Equal(20f, high.Pos.Y);
        var other = Origin();
        other.Channel = 0;
        other.Pos.Y = 7;
        VmDoomSound1427Slice.SectorOrigin(other);
        Assert.Equal(7f, other.Pos.Y);
        Assert.Equal(10f, other.Pos.X);
        var center = Origin();
        center.Compat = VmDoomSound1427Slice.SectorSounds;
        center.CenterX = 8;
        center.CenterY = 9;
        center.Inside = false;
        center.PrimaryNull = true;
        VmDoomSound1427Slice.SectorOrigin(center);
        Assert.Equal(8f, center.Pos.X);
        Assert.Equal(9f, center.Pos.Z);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1427Slice.SectorOrigin(new VmDoomSound1427Slice.State { Sector = null }));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1427Slice.SectorOrigin(new VmDoomSound1427Slice.State { LevelNull = true }));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1427Slice.SectorOrigin(new VmDoomSound1427Slice.State { PrimaryNull = true }));
        var poly = Origin();
        poly.PolyX = 3;
        poly.PolyY = 4;
        poly.ListenZ = 5;
        poly.FloorZ = 1;
        poly.CeilingZ = 9;
        VmDoomSound1427Slice.PolyOrigin(poly);
        Assert.Equal(3f, poly.Pos.X);
        Assert.Equal(4f, poly.Pos.Z);
        Assert.Equal(5f, poly.Pos.Y);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1427Slice.PolyOrigin(new VmDoomSound1427Slice.State { PolyNull = true }));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1427Slice.PolyOrigin(new VmDoomSound1427Slice.State { SideNull = true }));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1427Slice.PolyOrigin(new VmDoomSound1427Slice.State { Sector = null }));
    }

    [Fact]
    public void PositionVelocityAndLimits()
    {
        var skipped = new VmDoomSound1427Slice.State { HasPos = false, Vel = new VmDoomSound1427Slice.Vec { X = 5 } };
        VmDoomSound1427Slice.Calc(skipped);
        Assert.Equal(5f, skipped.Vel!.X);
        var loose = new VmDoomSound1427Slice.State
        {
            Type = VmDoomSound1427Slice.Unattached,
            Point = new[] { 10f, 11f, 12f },
            DispX = 1,
            DispY = 2,
            Flags = 0,
        };
        VmDoomSound1427Slice.Calc(loose);
        Assert.Equal(9f, loose.Pos.X);
        Assert.Equal(11f, loose.Pos.Y);
        Assert.Equal(10f, loose.Pos.Z);
        var stuck = new VmDoomSound1427Slice.State
        {
            Type = VmDoomSound1427Slice.Unattached,
            Point = new[] { 10f, 11f, 12f },
            Flags = VmDoomSound1427Slice.ListenerZ,
            CamZ = 40,
        };
        VmDoomSound1427Slice.Calc(stuck);
        Assert.Equal(40f, stuck.Pos.Y);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1427Slice.Calc(new VmDoomSound1427Slice.State { Type = VmDoomSound1427Slice.Unattached }));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1427Slice.Calc(new VmDoomSound1427Slice.State { Type = VmDoomSound1427Slice.Unattached, Point = new[] { 1f, 2f, 3f }, HitNull = true }));
        var actor = new VmDoomSound1427Slice.State
        {
            Type = VmDoomSound1427Slice.Actor,
            SourceX = 10,
            SourceY = 20,
            SourceZ = 30,
            DispX = 1,
            DispY = 2,
            MoveX = 1,
            MoveY = 2,
            MoveZ = 3,
            SoundX = 1,
            SoundY = 2,
            SoundZ = 3,
        };
        VmDoomSound1427Slice.Calc(actor);
        Assert.Equal(9f, actor.Pos.X);
        Assert.Equal(30f, actor.Pos.Y);
        Assert.Equal(18f, actor.Pos.Z);
        Assert.Equal(35f, actor.Vel!.X);
        Assert.Equal(105f, actor.Vel.Y);
        Assert.Equal(70f, actor.Vel.Z);
        var heard = new VmDoomSound1427Slice.State
        {
            Type = VmDoomSound1427Slice.Actor,
            SourceX = 10,
            SourceY = 20,
            SourceZ = 30,
            Flags = VmDoomSound1427Slice.ListenerZ,
            CamZ = 4,
        };
        VmDoomSound1427Slice.Calc(heard);
        Assert.Equal(4f, heard.Pos.Y);
        Assert.Equal(0f, heard.Vel!.X);
        var missing = new VmDoomSound1427Slice.State { Type = VmDoomSound1427Slice.Actor, SourceNull = true, SoundX = 1, SoundY = 2, SoundZ = 3, Vel = new VmDoomSound1427Slice.Vec { X = 8 } };
        VmDoomSound1427Slice.Calc(missing);
        Assert.Equal(1f, missing.Pos.X);
        Assert.Equal(2f, missing.Pos.Y);
        Assert.Equal(3f, missing.Pos.Z);
        Assert.Equal(0f, missing.Vel!.X);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1427Slice.Calc(new VmDoomSound1427Slice.State { Type = VmDoomSound1427Slice.Actor, SourceSectorNull = true }));
        var area = new VmDoomSound1427Slice.State
        {
            Type = VmDoomSound1427Slice.Sector,
            Flags = VmDoomSound1427Slice.Area,
            CamX = 10,
            CamY = 20,
            CamZ = 30,
            DispX = 1,
            DispY = 2,
            Inside = true,
            FloorZ = 0,
            CeilingZ = 100,
            Channel = VmDoomSound1427Slice.Interior,
            ListenX = 99,
        };
        VmDoomSound1427Slice.Calc(area);
        Assert.Equal(10f, area.Pos.X);
        Assert.Equal(20f, area.Pos.Z);
        Assert.Equal(30f, area.Pos.Y);
        Assert.Equal(99d, area.ListenX);
        Assert.Equal(VmDoomSound1427Slice.Area, area.Flags);
        var middle = new VmDoomSound1427Slice.State
        {
            Type = VmDoomSound1427Slice.Sector,
            CenterX = 50,
            CenterY = 60,
            DispX = 5,
            DispY = 6,
            CamZ = 8,
            SoundY = 1,
        };
        VmDoomSound1427Slice.Calc(middle);
        Assert.Equal(45f, middle.Pos.X);
        Assert.Equal(54f, middle.Pos.Z);
        Assert.Equal(8f, middle.Pos.Y);
        Assert.Equal(0, middle.Flags);
        var blind = new VmDoomSound1427Slice.State { Type = VmDoomSound1427Slice.Sector, Camera = null, CenterX = 5, CenterY = 6 };
        VmDoomSound1427Slice.Calc(blind);
        Assert.Equal(5f, blind.Pos.X);
        Assert.Equal(6f, blind.Pos.Z);
        Assert.Equal(0f, blind.Pos.Y);
        var poly = new VmDoomSound1427Slice.State
        {
            Type = VmDoomSound1427Slice.Poly,
            PolyX = 7,
            PolyY = 8,
            CamX = 1,
            CamY = 2,
            CamZ = 3,
            DispX = 1,
            FloorZ = 0,
            CeilingZ = 10,
            ListenZ = 3,
        };
        VmDoomSound1427Slice.Calc(poly);
        Assert.Equal(6f, poly.Pos.X);
        Assert.Equal(8f, poly.Pos.Z);
        Assert.Equal(3f, poly.Pos.Y);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1427Slice.Calc(new VmDoomSound1427Slice.State { Type = VmDoomSound1427Slice.Poly, CenterNull = true }));
        var quiet = new VmDoomSound1427Slice.State { Type = VmDoomSound1427Slice.None, SoundX = 1, SoundY = 2, SoundZ = 3, Flags = VmDoomSound1427Slice.ListenerZ, CamZ = 9 };
        VmDoomSound1427Slice.Calc(quiet);
        Assert.Equal(1f, quiet.Pos.X);
        Assert.Equal(9f, quiet.Pos.Y);
        Assert.Equal(3f, quiet.Pos.Z);
        var other = new VmDoomSound1427Slice.State { Type = VmDoomSound1427Slice.Sector, Pos = new VmDoomSound1427Slice.Vec { X = 9000000 } };
        Assert.True(VmDoomSound1427Slice.CheckPair(other));
        Assert.Empty(other.Lines);
        var edge = new VmDoomSound1427Slice.State
        {
            Type = VmDoomSound1427Slice.Actor,
            Pos = new VmDoomSound1427Slice.Vec { X = VmDoomSound1427Slice.PositionLimit, Y = -VmDoomSound1427Slice.PositionLimit, Z = 0 },
            Vel = new VmDoomSound1427Slice.Vec { X = VmDoomSound1427Slice.VelocityLimit },
        };
        Assert.True(VmDoomSound1427Slice.CheckPair(edge));
        Assert.Empty(edge.Lines);
        var far = new VmDoomSound1427Slice.State
        {
            Type = VmDoomSound1427Slice.Actor,
            SourceNull = true,
            Pos = new VmDoomSound1427Slice.Vec { X = VmDoomSound1427Slice.PositionLimit + 1 },
            Vel = new VmDoomSound1427Slice.Vec(),
        };
        Assert.False(VmDoomSound1427Slice.CheckPair(far));
        Assert.Contains("1048577.000000, 0.000000, 0.000000", far.Lines[0]);
        Assert.Contains("position", far.Lines[0]);
        Assert.Equal("\n", far.Lines[1]);
        var named = new VmDoomSound1427Slice.State
        {
            Type = VmDoomSound1427Slice.Actor,
            ClassName = "Zombie",
            Pos = new VmDoomSound1427Slice.Vec(),
            Vel = new VmDoomSound1427Slice.Vec { Y = VmDoomSound1427Slice.VelocityLimit + 8f },
        };
        Assert.False(VmDoomSound1427Slice.CheckPair(named));
        Assert.Contains("velocity", named.Lines[0]);
        Assert.Contains("0.000000, 0.000000, 73400328.000000", named.Lines[0]);
        Assert.Contains("Zombie", named.Lines[1]);
        var both = new VmDoomSound1427Slice.State
        {
            Type = VmDoomSound1427Slice.Actor,
            SourceNull = true,
            Pos = new VmDoomSound1427Slice.Vec { Z = VmDoomSound1427Slice.PositionLimit + 1 },
            Vel = new VmDoomSound1427Slice.Vec { X = VmDoomSound1427Slice.VelocityLimit + 8f },
        };
        Assert.False(VmDoomSound1427Slice.CheckPair(both));
        Assert.Contains("position", both.Lines[0]);
        Assert.Contains("velocity", both.Lines[2]);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1427Slice.CheckPair(new VmDoomSound1427Slice.State
        {
            Type = VmDoomSound1427Slice.Actor,
            ClassNull = true,
            Pos = new VmDoomSound1427Slice.Vec { X = VmDoomSound1427Slice.PositionLimit + 1 },
            Vel = new VmDoomSound1427Slice.Vec(),
        }));
    }
}
