using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmParticle1282SliceTests
{
    static VmParticle1282 NewParticle()
    {
        var particle = new VmParticle1282
        {
            ViewCenterX = 10,
            CenterY = 100,
            ViewWidth = 10,
            WindowRight = 100,
            Sin = 0,
            Cos = 1,
            TanCos = 1,
            TanSin = 0,
            PosX = 4,
            Size = 8,
            YaspectMul = 2,
            SkyPic = 99,
            Alpha = 1f,
            Color = 0xAB000000,
            Flags = 2,
            LightLevel = 7,
            ColormapNum = 3,
        };
        particle.SectorCeiling.Pic = 99;
        particle.SectorFloor.Pic = 99;
        return particle;
    }

    [Fact]
    public void Project_KeepsTheParticleSquareAndUsesTheUnmovedPosition()
    {
        var particle = NewParticle();
        var vis = particle.Project(0.25);
        Assert.NotNull(vis);
        Assert.Equal(8, vis!.X1);
        Assert.Equal(13, vis.X2);
        Assert.Equal(98, vis.Y1);
        Assert.Equal(103, vis.Y2);
        Assert.Equal(163840, vis.XScaleFixed);
        Assert.Equal("2.5", VmParticle1282.Num(vis.YScale));
        Assert.Equal("4", VmParticle1282.Num(vis.Depth));
        Assert.Equal("0.25", VmParticle1282.Num(vis.IDepth));
        Assert.Equal(171, vis.StartFrac);
        Assert.Equal((short)255, vis.RenderFlags);
        Assert.False(vis.FullBright);
        Assert.Equal(0f, vis.Gz);

        particle.PosZ = 1;
        var raised = particle.Project(0);
        Assert.Equal(93, raised!.Y1);
        Assert.Equal(98, raised.Y2);
        Assert.Equal(1f, raised.Gz);
        particle.PosZ = 0;

        particle.Mirror = true;
        Assert.Null(particle.Project(0));
        Assert.Equal("side", particle.Reject);
        particle.Mirror = false;

        particle.PosX = -1;
        Assert.Null(particle.Project(0));
        Assert.Equal("behind", particle.Reject);
        particle.PosX = VmParticle1282.MinZ;
        Assert.NotNull(particle.Project(0));
        particle.PosX = 4;

        particle.Paused = true;
        particle.VelX = 100;
        Assert.Equal(4f, particle.Project(1)!.Gx);
        particle.Paused = false;
        particle.Frozen = true;
        Assert.Equal(4f, particle.Project(1)!.Gx);
        particle.Frozen = false;
        particle.VelX = 2;
        Assert.Equal(5f, particle.Project(0.5)!.Gx);
        particle.VelX = 0;

        particle.HasPortal = true;
        particle.PortalSide = (x, y) => x == 4 && y == 0 ? 1 : 0;
        Assert.Null(particle.Project(0.5));
        Assert.Equal("portal", particle.Reject);
        particle.PortalSide = (x, y) => x == 4 && y == 0 ? 0 : 1;
        particle.VelX = 9;
        Assert.NotNull(particle.Project(1));
        particle.HasPortal = false;
        particle.VelX = 0;

        particle.SectorFloor.Pic = 1;
        particle.SectorFloor.Height = (x, y) => x == 4 && y == 0 ? 10 : -100;
        particle.VelX = 9;
        Assert.Null(particle.Project(1));
        Assert.Equal("floor", particle.Reject);
        particle.SectorFloor.Height = (x, y) => 0;
        Assert.NotNull(particle.Project(0));
        particle.SectorFloor.Pic = 99;
        particle.VelX = 0;

        particle.SectorCeiling.Pic = 1;
        particle.SectorCeiling.Height = (x, y) => x == 4 && y == 0 ? 0 : 100;
        Assert.Null(particle.Project(0));
        Assert.Equal("ceiling", particle.Reject);
        particle.SectorCeiling.Pic = 99;

        particle.CeilingClip[8] = 100;
        particle.CeilingClip[12] = 110;
        particle.FloorClip[8] = 100;
        particle.FloorClip[12] = 90;
        Assert.Null(particle.Project(0));
        Assert.Equal("yrange", particle.Reject);
        particle.CeilingClip[8] = int.MinValue;
        particle.CeilingClip[12] = int.MinValue;
        particle.FloorClip[8] = 110;
        particle.FloorClip[12] = 101;
        var clipped = particle.Project(0);
        Assert.Equal(98, clipped!.Y1);
        Assert.Equal(100, clipped.Y2);

        particle.WindowLeft = 20;
        particle.WindowRight = 12;
        Assert.Null(particle.Project(0));
        Assert.Equal("xrange", particle.Reject);
        particle.WindowLeft = 0;
        particle.WindowRight = 100;

        int calls = particle.ColormapCalls;
        particle.HeightSector = 4;
        particle.FakeSide = 2;
        particle.Flags3 = 0x10;
        particle.Flags = 1;
        particle.HeightCeiling.Colormap = 8;
        particle.HeightCeiling.SpriteColor = 9;
        particle.HeightCeiling.Pic = 99;
        particle.SectorCeiling.Colormap = 3;
        particle.SectorCeiling.SpriteColor = 4;
        var above = particle.Project(0);
        Assert.Equal(8, above!.MapColormap);
        Assert.Equal(9, above.MapSprite);
        Assert.True(above.NoColored);
        Assert.True(above.FullBright);
        Assert.Equal(2, above.FakeSide);
        Assert.Equal(calls + 1, particle.ColormapCalls);
        particle.FakeSide = 0;
        var center = particle.Project(0);
        Assert.Equal(3, center!.MapColormap);
        Assert.Equal(4, center.MapSprite);
        particle.FakeSide = 1;
        particle.HeightFloor.Colormap = 6;
        particle.HeightFloor.SpriteColor = 7;
        particle.HeightFloor.Pic = 99;
        particle.SectorFloor.Pic = 99;
        var below = particle.Project(0);
        Assert.Equal(6, below!.MapColormap);
        Assert.Equal(7, below.MapSprite);
    }

    [Fact]
    public void Render_StepsTheTextureAcrossABlockedColumn()
    {
        var particle = NewParticle();
        var vis = particle.Project(0)!;
        vis.Y1 = 2;
        vis.Y2 = 4;
        vis.X1 = 1;
        vis.X2 = 4;
        vis.IDepth = 1f;
        vis.RenderFlags = 128;
        vis.StartFrac = 1;
        vis.ColormapNum = 3;
        vis.PortalUniq = 7;
        particle.ColorMap[1] = 40;
        particle.BlockColumn = x => x == 2;
        particle.Segments.Add(new VmParticleSeg1282 { X1 = 0, X2 = 1, Sz1 = 1, Sz2 = 1, Sx1 = 0, Sx2 = 4, PortalUniq = 7 });
        particle.Segments.Add(new VmParticleSeg1282 { X1 = 1, X2 = 5, Sz1 = 1, Sz2 = 1, Sx1 = 0, Sx2 = 4, PortalUniq = 7 });
        particle.Segments.Add(new VmParticleSeg1282 { X1 = 1, X2 = 5, Sz1 = 1, Sz2 = 1, Sx1 = 0, Sx2 = 4, PortalUniq = 1 });
        vis.Render(particle, null, null, 5, 9, 3);
        Assert.Empty(vis.Masked);
        Assert.Equal(
            "1 2 3 196648 128 699050\n3 2 3 196648 128 3495252",
            string.Join('\n', vis.Draws));

        vis.Draws.Clear();
        vis.IDepth = 1.1f;
        vis.Render(particle, null, null, 5, 9, 3);
        Assert.Equal("1 4 3 1", Assert.Single(vis.Masked));

        vis.Y2 = 1;
        vis.Draws.Clear();
        vis.Masked.Clear();
        vis.Render(particle, null, null, 0, 0, 0);
        Assert.Equal("empty", particle.Reject);
        Assert.Empty(vis.Draws);
        Assert.Empty(vis.Masked);
    }
}
