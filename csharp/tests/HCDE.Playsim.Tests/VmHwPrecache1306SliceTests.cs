namespace HCDE.Playsim.Tests;

public class VmHwPrecache1306SliceTests
{
    [Fact]
    public void Collect_ExpandsSkyFacesAndPrecachesTranslatedSprites()
    {
        var host = new VmHwPrecache1306.Host
        {
            UpscaleMask = VmHwPrecache1306.UfTexture | VmHwPrecache1306.UfSprite,
            GlPrecache = true,
            TrueColor = true,
            ElapsedMs = 1.25,
            Hits = new byte[6],
            Textures = new VmHwPrecache1306.Tex?[]
            {
                new() { Layers = new[] { 0 }, SkyFaces = Face(1) },
                new() { UseType = VmHwPrecache1306.UseFontChar, SkyFaces = Face(2, 3) },
                new() { Layers = new[] { 2, 4 }, Hardware = new object() },
                new() { UseType = VmHwPrecache1306.UseNull, UpscaleFlag = 0 },
                new() { HasImage = false, UpscaleFlag = 0 },
                new() { UseType = VmHwPrecache1306.UseSpecial },
            },
            Sprites = new[]
            {
                new VmHwPrecache1306.SpriteDef { Frames = new[] { Frame(5, 0) } },
                new VmHwPrecache1306.SpriteDef { Frames = new[] { Frame(5, 2) } },
            },
            Models = new[] { new VmHwPrecache1306.Model(), new VmHwPrecache1306.Model(), new VmHwPrecache1306.Model() },
        };
        host.Hits[0] = (byte)(VmHwPrecache1306.HitWall | VmHwPrecache1306.HitSky);
        host.Hits[1] = VmHwPrecache1306.HitSky;
        host.Materials[0] = Layer(0, 8);
        host.Materials[2] = Layer(2, 4);
        host.Materials[3] = Layer(3, 1);
        host.Materials[4] = null;
        host.Materials[5] = Layer(5, 9);
        var skins = new int[33];
        for (int i = 0; i < skins.Length; i++)
            skins[i] = i;
        var first = new VmHwPrecache1306.Actor
        {
            Remap = 5,
            States = new[] { (1, 0), (0, 0) },
        };
        first.Models[(1, 0)] = new VmHwPrecache1306.ModelFrame
        {
            Skins = new[] { 0, 4 },
            ModelIds = new[] { 0, 1 },
            SurfaceSkins = skins,
        };
        var second = new VmHwPrecache1306.Actor { States = new[] { (0, 0) } };
        host.Actors.Add(first);
        host.Actors.Add(second);

        new VmHwPrecache1306(host).Precache();

        Assert.Equal(VmHwPrecache1306.HitWall | VmHwPrecache1306.HitSky, host.Hits[0]);
        Assert.Equal(VmHwPrecache1306.HitSky, host.Hits[1]);
        Assert.Equal(VmHwPrecache1306.HitFlat, host.Hits[2]);
        Assert.Equal(VmHwPrecache1306.HitFlat, host.Hits[3]);
        Assert.Equal(VmHwPrecache1306.HitFlat, host.Hits[4]);
        Assert.Equal(0, host.Textures[0]!.CleanMarkers);
        Assert.Equal(1, host.Textures[2]!.CleanMarkers);
        Assert.Equal(1, host.Textures[4]!.CleanMarkers);
        Assert.Equal(new[] { 5, 0 }, host.SpriteKeys[0]);
        Assert.Equal(new[] { 5 }, host.SpriteKeys[1]);
        Assert.Equal(33, Assert.Single(host.Models[0].SkinCalls)!.Length);
        Assert.Equal(32, host.Models[0].SkinCalls[0]![32]);
        Assert.Empty(host.Models[1].SkinCalls);
        Assert.Equal(1, host.Models[2].Destroyed);
        Assert.Equal(0, host.Models[0].Destroyed);
        Assert.Equal(1, host.Models[0].Built);
        Assert.Equal(1, host.Models[1].Built);
        Assert.Equal(0, host.Models[2].Built);
        Assert.Equal("Textures precached in 1.250 ms\n", Assert.Single(host.Prints));

        int markSprite = host.Steps.IndexOf("validate:2:1:1");
        int cacheSprite = host.Steps.IndexOf("validate:2:3:1");
        Assert.True(markSprite >= 0 && cacheSprite > markSprite);
        Assert.Contains("validate:5:1:1", host.Steps);
        Assert.Contains("validate:5:3:1", host.Steps);
        Assert.Contains("validate:0:2:1", host.Steps);
        Assert.Contains("add-skins:0:33", host.Steps);
        Assert.DoesNotContain("add-skins:1:", host.Steps);
        Assert.Contains("find:1:0:0", host.Steps);
        Assert.Equal(new[] { "destroy:2", "start" }, host.Steps.Where(step => step is "destroy:2" or "start").ToArray());
        Assert.True(host.Steps.IndexOf("start") < host.Steps.IndexOf("begin"));
        Assert.Contains("mark:5:5:9", host.Steps);
        Assert.Contains("mark:5:0:9", host.Steps);
        Assert.Contains("mark:2:0:4", host.Steps);
        Assert.Contains("mark:2:5:4", host.Steps);
        Assert.DoesNotContain("mark:4:", host.Steps);
        Assert.Contains("clean-unused:2", host.Steps);
        Assert.Contains("clean-unused:4", host.Steps);
        Assert.DoesNotContain("clean-unused:0", host.Steps);
        Assert.DoesNotContain("clean-unused:1", host.Steps);
        Assert.DoesNotContain("clean-unused:5", host.Steps);
        Assert.Contains("hw:0:1", host.Steps);
        Assert.Contains("hw:2:1", host.Steps);
        Assert.Contains("hw:3:0", host.Steps);
        Assert.DoesNotContain("hw:4:", host.Steps);
        Assert.Contains("register:0:1", host.Steps);
        Assert.Contains("register:5:1", host.Steps);
        Assert.DoesNotContain("register:2:", host.Steps);
        Assert.Contains("material:5:5", host.Steps);
        Assert.Contains("material:5:0", host.Steps);
        Assert.Contains("material:0:0", host.Steps);
        Assert.DoesNotContain("material:4:", host.Steps);
        Assert.Contains("renderer:-1", host.Steps);
        Assert.Contains("build:0", host.Steps);
        Assert.Contains("build:1", host.Steps);
        Assert.DoesNotContain("build:2", host.Steps);
        Assert.True(host.Steps.IndexOf("end") < host.Steps.IndexOf("renderer:-1"));
        Assert.True(host.Steps.IndexOf("renderer:-1") < host.Steps.IndexOf("delete-renderer"));
        Assert.Equal(VmHwPrecache1306.CtfExpand | VmHwPrecache1306.CtfUpscale, 3);
    }

    [Fact]
    public void Precache_StopsBeforeImagesWhenTheCvarIsOff()
    {
        var host = new VmHwPrecache1306.Host
        {
            UpscaleMask = 0,
            GlPrecache = false,
            TrueColor = false,
            Hits = new byte[2],
            Textures = new VmHwPrecache1306.Tex?[]
            {
                new() { Layers = new[] { 0 }, UpscaleFlag = 1 },
                new() { Layers = new[] { 1 }, UpscaleFlag = 1 },
            },
            Sprites = new[] { new VmHwPrecache1306.SpriteDef { Frames = new[] { Frame() } } },
            Models = new[] { new VmHwPrecache1306.Model(), new VmHwPrecache1306.Model() },
        };
        host.Hits[1] = VmHwPrecache1306.HitWall;
        var actor = new VmHwPrecache1306.Actor { States = new[] { (0, 3) } };
        actor.Models[(0, 3)] = new VmHwPrecache1306.ModelFrame
        {
            Skins = new[] { 0, 1 },
            ModelIds = new[] { 0, -1 },
            SurfaceSkins = Array.Empty<int>(),
        };
        host.Actors.Add(actor);

        new VmHwPrecache1306(host).Precache();

        Assert.Equal(VmHwPrecache1306.HitWall | VmHwPrecache1306.HitFlat, host.Hits[1]);
        Assert.Equal(new[] { 0 }, Assert.Single(host.SpriteKeys));
        Assert.Null(Assert.Single(host.Models[0].SkinCalls));
        Assert.Contains("add-skins:0:null", host.Steps);
        Assert.Contains("find:0:3:0", host.Steps);
        Assert.Equal(0, host.Models[0].Destroyed);
        Assert.Equal(1, host.Models[1].Destroyed);
        Assert.Equal(0, host.Models[0].Built);
        Assert.Equal(0, host.Textures[0]!.CleanMarkers);
        Assert.Equal(1, host.Textures[1]!.CleanMarkers);
        Assert.Contains("start", host.Steps);
        Assert.Contains("validate:1:0:1", host.Steps);
        Assert.DoesNotContain(host.Steps, step => step.StartsWith("validate:0:", StringComparison.Ordinal));
        Assert.DoesNotContain("begin", host.Steps);
        Assert.DoesNotContain(host.Steps, step => step.StartsWith("register:", StringComparison.Ordinal));
        Assert.DoesNotContain(host.Steps, step => step.StartsWith("material:", StringComparison.Ordinal));
        Assert.DoesNotContain(host.Steps, step => step.StartsWith("build:", StringComparison.Ordinal));
        Assert.Empty(host.Prints);
        Assert.Contains("clean-unused:1", host.Steps);
        Assert.DoesNotContain("clean-unused:0", host.Steps);
        Assert.Equal("Textures precached in 0.000 ms\n", VmHwPrecache1306.FormatPrecache(0));
    }

    static int[] Face(int first, int last = -1)
    {
        var faces = new[] { -1, -1, -1, -1, -1, -1 };
        faces[0] = first;
        if (last >= 0)
            faces[5] = last;
        return faces;
    }

    static int[] Frame(int first = -1, int second = -1)
    {
        var frame = new int[16];
        for (int i = 0; i < frame.Length; i++)
            frame[i] = -1;
        frame[0] = first;
        frame[1] = second;
        return frame;
    }

    static VmHwPrecache1306.Material Layer(int texture, int scale)
    {
        return new VmHwPrecache1306.Material
        {
            Layers = new[] { new VmHwPrecache1306.Layer { TextureId = texture, ScaleFlags = scale } },
        };
    }
}
