namespace HCDE.Playsim.Tests;

public class VmCanvasTexture1191SliceTests
{
    [Fact]
    public void Add_ReplacesTheSameTextureAndRejectsANonCanvas()
    {
        VmCanvasTexture1191Slice.Clear();
        var level = new VmCanvasTexture1191Slice.Level();
        var actor = new VmCanvasTexture1191Slice.Actor { Id = 4, Level = level };
        var other = new VmCanvasTexture1191Slice.Actor { Id = 9, Level = level };
        level.Canvas.Add(actor, 0, 1);
        Assert.Empty(level.Canvas.List);

        var shared = new VmCanvasTexture1191Slice.CanvasTexture { Name = "SHARED", HasCanvas = true };
        VmCanvasTexture1191Slice.Textures[2] = shared;
        VmCanvasTexture1191Slice.Textures[3] = shared;
        level.Canvas.Add(actor, 2, 10);
        Assert.Single(level.Canvas.List);
        Assert.True(shared.FirstUpdate);
        Assert.Equal(2, level.Canvas.List[0].PicNum);

        shared.FirstUpdate = false;
        level.Canvas.Add(actor, 3, 10);
        Assert.False(shared.FirstUpdate);
        Assert.Single(level.Canvas.List);
        Assert.Equal(2, level.Canvas.List[0].PicNum);
        Assert.Equal(10d, level.Canvas.List[0].Fov);

        level.Canvas.Add(other, 3, 20);
        Assert.True(shared.FirstUpdate);
        Assert.Same(other, level.Canvas.List[0].Viewpoint);
        Assert.Equal(20d, level.Canvas.List[0].Fov);

        var flat = new VmCanvasTexture1191Slice.CanvasTexture { Name = "FLAT", HasCanvas = false };
        VmCanvasTexture1191Slice.Textures[4] = flat;
        level.Canvas.Add(actor, 4, 1);
        Assert.Equal("FLAT is not a valid target for a camera\n", VmCanvasTexture1191Slice.Messages[0]);
        Assert.False(flat.FirstUpdate);
        Assert.Single(level.Canvas.List);

        shared.NeedsUpdate = true;
        var quiet = new VmCanvasTexture1191Slice.CanvasTexture { Name = "QUIET", HasCanvas = true, NeedsUpdate = true };
        VmCanvasTexture1191Slice.Textures[5] = quiet;
        level.Canvas.Add(null, 5, 7);
        var seen = new List<string>();
        level.Canvas.UpdateAll((view, texture, fov) => seen.Add(view.Id + " " + texture.Name + " " + fov.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        Assert.Equal(new[] { "9 SHARED 20" }, seen);

        level.Canvas.Mark();
        Assert.Equal(2, level.Canvas.Marked.Count);
        Assert.Same(other, level.Canvas.Marked[0]);
        Assert.Null(level.Canvas.Marked[1]);
        level.Canvas.EmptyList();
        Assert.Empty(level.Canvas.List);
    }

    [Fact]
    public void SetCameraToTexture_RequiresAHardwareCanvas()
    {
        VmCanvasTexture1191Slice.Clear();
        Assert.Equal(1, VmCanvasTexture1191Slice.TextureWall);
        Assert.Equal(2, VmCanvasTexture1191Slice.TexmanOverridable);
        var home = new VmCanvasTexture1191Slice.Level();
        var away = new VmCanvasTexture1191Slice.Level();
        var actor = new VmCanvasTexture1191Slice.Actor { Id = 1, Level = home };
        var calls = new List<string>();
        VmCanvasTexture1191Slice.CheckForTexture = (name, type, flags) =>
        {
            calls.Add(name + " " + type + " " + flags);
            if (name == "MISSING")
                return 0;
            if (name == "SOFT")
                return 8;
            return 6;
        };
        var soft = new VmCanvasTexture1191Slice.CanvasTexture { Name = "SOFT", HasCanvas = true, HardwareCanvas = false };
        var hard = new VmCanvasTexture1191Slice.CanvasTexture { Name = "HARD", HasCanvas = false, HardwareCanvas = true };
        var ready = new VmCanvasTexture1191Slice.CanvasTexture { Name = "READY", HasCanvas = true, HardwareCanvas = true };
        VmCanvasTexture1191Slice.Textures[8] = soft;
        VmCanvasTexture1191Slice.Textures[6] = hard;

        VmCanvasTexture1191Slice.SetCameraToTexture(actor, "MISSING", 50);
        VmCanvasTexture1191Slice.SetCameraToTexture(actor, "SOFT", 50);
        VmCanvasTexture1191Slice.SetCameraToTexture(actor, "HARD", 50);
        Assert.Empty(home.Canvas.List);
        Assert.Empty(away.Canvas.List);
        Assert.Equal("HARD is not a valid target for a camera\n", VmCanvasTexture1191Slice.Messages[0]);

        VmCanvasTexture1191Slice.Textures[6] = ready;
        VmCanvasTexture1191Slice.SetCameraToTexture(actor, "READY", 50);
        Assert.Single(home.Canvas.List);
        Assert.Empty(away.Canvas.List);
        Assert.Equal(6, home.Canvas.List[0].PicNum);
        Assert.Equal(50d, home.Canvas.List[0].Fov);
        Assert.True(ready.FirstUpdate);
        Assert.Equal(new[] { "MISSING 1 2", "SOFT 1 2", "HARD 1 2", "READY 1 2" }, calls);
    }

    [Fact]
    public void Serialize_SkipsEmptyAndRestoresThroughAdd()
    {
        VmCanvasTexture1191Slice.Clear();
        var level = new VmCanvasTexture1191Slice.Level();
        var actor = new VmCanvasTexture1191Slice.Actor { Id = 12, Level = level };
        var empty = new VmCanvasTexture1191Slice.Archive();
        level.Canvas.Serialize(empty);
        Assert.Empty(empty.Trace);

        var canvas = new VmCanvasTexture1191Slice.CanvasTexture { Name = "CAM", HasCanvas = true };
        VmCanvasTexture1191Slice.Textures[6] = canvas;
        level.Canvas.Add(actor, 6, 90);
        level.Canvas.List.Add(new VmCanvasTexture1191Slice.Probe { Texture = canvas, Fov = 1 });
        level.Canvas.List.Add(new VmCanvasTexture1191Slice.Probe { Viewpoint = actor, Fov = 2 });

        var closed = new VmCanvasTexture1191Slice.Archive { AllowObject = false };
        level.Canvas.Serialize(closed);
        Assert.Equal(new[] { "array canvastextures", "end-array" }, closed.Trace);

        var arc = new VmCanvasTexture1191Slice.Archive();
        level.Canvas.Serialize(arc);
        Assert.Equal(new[]
        {
            "array canvastextures",
            "object",
            "viewpoint 12",
            "fov 90",
            "texture 6",
            "end-object",
            "end-array"
        }, arc.Trace);

        var reader = new VmCanvasTexture1191Slice.Level();
        var view = new VmCanvasTexture1191Slice.Actor { Id = 3, Level = reader };
        reader.Canvas.Add(view, 6, 10);
        canvas.FirstUpdate = false;
        var blocked = new VmCanvasTexture1191Slice.Archive { Writing = false, AllowArray = false };
        blocked.Incoming.Add(new VmCanvasTexture1191Slice.Saved { Viewpoint = view, Fov = 30, PicNum = 6 });
        reader.Canvas.Serialize(blocked);
        Assert.Equal(10d, reader.Canvas.List[0].Fov);
        Assert.False(canvas.FirstUpdate);
        Assert.Single(reader.Canvas.List);

        var incoming = new VmCanvasTexture1191Slice.Archive { Writing = false };
        incoming.Incoming.Add(new VmCanvasTexture1191Slice.Saved { Viewpoint = view, Fov = 30, PicNum = 6 });
        incoming.Incoming.Add(new VmCanvasTexture1191Slice.Saved { Viewpoint = view, Fov = 40, PicNum = 9 });
        var flat = new VmCanvasTexture1191Slice.CanvasTexture { Name = "NOPE", HasCanvas = false };
        VmCanvasTexture1191Slice.Textures[9] = flat;
        reader.Canvas.Serialize(incoming);
        Assert.Single(reader.Canvas.List);
        Assert.Equal(30d, reader.Canvas.List[0].Fov);
        Assert.Equal(6, reader.Canvas.List[0].PicNum);
        Assert.True(canvas.FirstUpdate);
        Assert.Equal("NOPE is not a valid target for a camera\n", VmCanvasTexture1191Slice.Messages[0]);
        Assert.False(flat.FirstUpdate);
    }
}
