using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDraw2d1276SliceTests
{
    [Fact]
    public void Empty_LeavesTheFirstPassAndSkipsTheSwap()
    {
        var d = new VmDraw2d1276
        {
            ProfileActive = true,
            SmoothLines = true,
            DrawerWidth = 640,
            DrawerHeight = 400,
            ViewportLeft = 10,
            ViewportTop = 5,
            ViewportWidth = 100,
            ViewportHeight = 50,
        };
        d.Vertices.Add(new VmDraw2dVert1276 { R = 0x22, G = 0x33, B = 0x44, A = 0x11 });
        d.DrawViewport();
        Assert.Equal(
"""
clock
viewport 10 5 100 50
set2d 640 400
stencil off
stencil 0 0 0
clear 2
depth off
msaa off
smooth 1
mip 1
unclock
""",
            string.Join('\n', d.Log));
        Assert.True(d.FirstPass);
        Assert.Equal(1, d.Cycle);
        Assert.Equal(2, d.SampleSeq);
        Assert.Equal(0, d.AnisoReads);
        Assert.Equal(0x22, d.Vertices[0].R);
        Assert.Equal(0x44, d.Vertices[0].B);

        d.Log.Clear();
        d.HasAnisoCallback = true;
        d.AnisoDisables = true;
        d.DrawViewport();
        Assert.Equal(
"""
clock
viewport 10 5 100 50
set2d 640 400
stencil off
stencil 0 0 0
clear 2
depth off
msaa off
smooth 1
mip 0
unclock
""",
            string.Join('\n', d.Log));
        Assert.True(d.FirstPass);
        Assert.Equal(2, d.Cycle);
        Assert.Equal(4, d.SampleSeq);
        Assert.Equal(1, d.AnisoReads);
        Assert.Equal(0x22, d.Vertices[0].R);

        d.Log.Clear();
        d.Hw2dMip = false;
        d.AnisoDisables = false;
        d.DrawViewport();
        Assert.Equal(
"""
clock
viewport 10 5 100 50
set2d 640 400
stencil off
stencil 0 0 0
clear 2
depth off
msaa off
smooth 1
mip 0
unclock
""",
            string.Join('\n', d.Log));
        Assert.True(d.FirstPass);
        Assert.Equal(3, d.Cycle);
        Assert.Equal(6, d.SampleSeq);
        Assert.Equal(1, d.AnisoReads);

        d.Log.Clear();
        d.ProfileActive = false;
        d.Hw2dMip = true;
        d.HasAnisoCallback = false;
        d.SmoothLines = false;
        d.DrawViewport();
        Assert.Equal(
"""
clock
viewport 10 5 100 50
set2d 640 400
stencil off
stencil 0 0 0
clear 2
depth off
msaa off
smooth 0
mip 1
unclock
""",
            string.Join('\n', d.Log));
        Assert.True(d.FirstPass);
        Assert.Equal(3, d.Cycle);
        Assert.Equal(6, d.SampleSeq);
        Assert.Equal(1, d.AnisoReads);

        d.Log.Clear();
        d.ProfileActive = true;
        d.SmoothLines = true;
        d.Indices.AddRange([0, 1, 2]);
        d.Commands.Add(new VmDraw2dCmd1276 { Special = 1, StencilOn = true });
        d.Commands.Add(new VmDraw2dCmd1276 { Special = 2, StencilOffs = 4, StencilOp = 1, StencilFlags = 2 });
        d.Commands.Add(new VmDraw2dCmd1276 { Special = 3 });
        d.Commands.Add(new VmDraw2dCmd1276 { Special = 4 });
        d.Commands.Add(new VmDraw2dCmd1276
        {
            Type = 0,
            Style = 1,
            StyleFlags = 0,
            DrawMode = 3,
            Desaturate = 0,
            ScreenFade = 1f,
            IndexIndex = 1,
            IndexCount = 3,
        });
        d.Draw(1, 2, 3, 4);
        Assert.Equal(
"""
clock
viewport 1 2 3 4
set2d 640 400
stencil off
stencil 0 0 0
clear 2
depth off
msaa off
smooth 1
mip 1
swap 44332211
upload 1 3
vb upload
fog 0
special stencil 1
special set 4 1 2
special clear 2
special other
style 1 0
bright 1
fog 2
fade 1
mode 3
scissor -1 -1 -1 -1
fogc 00000000 -0
color 1 1 1 1 0
light 0 0
alpha 1 0
tex off
drawidx 2 1 3
obj ffffffff
obj2 00000000
add 00000000
texmat off
model off
effect -1
scissor -1 -1 -1 -1
style 6
vb screen
stencil off
stencil 0 0 0
tex on
bright on
mode 0
fog 0
fade 1
soft 255 0
reset
unclock
""",
            string.Join('\n', d.Log));
        Assert.False(d.FirstPass);
        Assert.Equal(4, d.Cycle);
        Assert.Equal(8, d.SampleSeq);
        Assert.Equal(0x44, d.Vertices[0].R);
        Assert.Equal(0x33, d.Vertices[0].G);
        Assert.Equal(0x22, d.Vertices[0].B);
        Assert.Equal(0x11, d.Vertices[0].A);

        d.Log.Clear();
        d.Commands.Clear();
        d.Commands.Add(new VmDraw2dCmd1276
        {
            Type = 1,
            VertIndex = 2,
            VertCount = 5,
            Style = 6,
            StyleFlags = 8,
            DrawMode = 1,
            Desaturate = 255,
            ScreenFade = 0.5f,
            Color1 = 0xabc,
        });
        d.Draw(1, 2, 3, 4);
        Assert.Equal(
"""
clock
viewport 1 2 3 4
set2d 640 400
stencil off
stencil 0 0 0
clear 2
depth off
msaa off
smooth 1
mip 1
upload 1 3
vb upload
fog 0
style 6 8
bright 0
fog 2
fade 0.5
mode 1
scissor -1 -1 -1 -1
fogc 00000abc -0
color 1 1 1 1 1
light 0 0
alpha 1 0
tex off
draw 1 2 5
obj ffffffff
obj2 00000000
add 00000000
texmat off
model off
effect -1
scissor -1 -1 -1 -1
style 6
vb screen
stencil off
stencil 0 0 0
tex on
bright on
mode 0
fog 0
fade 1
soft 255 0
reset
unclock
""",
            string.Join('\n', d.Log));
        Assert.False(d.FirstPass);
        Assert.Equal(0x44, d.Vertices[0].R);
        Assert.Equal(0x22, d.Vertices[0].B);
        Assert.DoesNotContain("swap ", string.Join('\n', d.Log));
    }

    [Fact]
    public void Commands_ClipTheScissorAndResetTheShapeBuffer()
    {
        var b = new VmDraw2d1276
        {
            DrawerWidth = 640,
            DrawerHeight = 480,
            ViewportLeft = 10,
            ViewportTop = 5,
            ViewportWidth = 100,
            ViewportHeight = 50,
            GameWidth = 320,
            GameHeight = 200,
        };
        b.Vertices.Add(new VmDraw2dVert1276 { R = 1, G = 2, B = 3, A = 4 });
        b.Vertices.Add(new VmDraw2dVert1276 { R = 5, G = 6, B = 7, A = 8 });
        b.Indices.AddRange([4, 5]);
        b.Commands.Add(new VmDraw2dCmd1276
        {
            Style = 8,
            StyleFlags = 8,
            DrawMode = 4,
            Desaturate = 4,
            ScreenFade = 0.25f,
            Color1 = 0x01020304,
            Flags = 2 | 4 | 8,
            LightLevel = 200,
            Translation = 12,
            Sc0 = -40,
            Sc1 = 8,
            Sc2 = 16,
            Sc3 = 240,
            Map0 = 0x11AABBCC,
            Map1 = 0x22010203,
            UseTransform = true,
            C00 = 1.5,
            C01 = -2,
            C02 = 0.25,
            C10 = 4,
            C11 = 0.5,
            C12 = 8,
            Texture = new VmDraw2dTex1276 { Id = 3, UseType = 9, HardwareCanvas = true },
            IndexIndex = 0,
            IndexCount = 2,
        });
        b.Commands.Add(new VmDraw2dCmd1276
        {
            Type = 1,
            VertIndex = 1,
            VertCount = 4,
            Style = 2,
            StyleFlags = 0,
            DrawMode = 0,
            Flags = 4,
            Texture = new VmDraw2dTex1276 { Id = 1, UseType = 13, HardwareCanvas = true },
        });
        b.Commands.Add(new VmDraw2dCmd1276
        {
            Type = 2,
            VertIndex = 3,
            VertCount = 1,
            Flags = 4,
        });
        b.Commands.Add(new VmDraw2dCmd1276
        {
            Type = 1,
            VertIndex = 6,
            VertCount = 2,
            Flags = 1,
            Translation = 7,
            Texture = new VmDraw2dTex1276 { Id = 5, UseType = 14 },
        });
        b.Commands.Add(new VmDraw2dCmd1276
        {
            Type = 3,
            IndexIndex = 8,
            IndexCount = 9,
            Translation = 1,
            Texture = new VmDraw2dTex1276 { Id = 6, UseType = 15 },
        });
        b.Commands.Add(new VmDraw2dCmd1276
        {
            Type = 99,
            IndexIndex = 2,
            IndexCount = 2,
            Flags = 1,
            Texture = new VmDraw2dTex1276 { Id = 7, UseType = 16 },
        });
        b.Commands.Add(new VmDraw2dCmd1276
        {
            IndexIndex = 4,
            IndexCount = 1,
            Texture = new VmDraw2dTex1276 { Id = 8, UseType = 17 },
            Map0 = 0x00FFFFFF,
            Map1 = 0x12345678,
        });
        var keep = new VmDraw2dShape1276 { BufIndex = 2, LastCommand = 5, UploadedOnce = true };
        keep.Buffers.AddRange([7, 8, 9]);
        b.Commands.Add(new VmDraw2dCmd1276 { Shape = keep, ShapeBufIndex = 1, ShapeIndexCount = 12, ShapeCounter = 1 });
        var zero = new VmDraw2dShape1276 { BufIndex = 0, LastCommand = 3, UploadedOnce = true };
        zero.Buffers.Add(11);
        b.Commands.Add(new VmDraw2dCmd1276 { Shape = zero, ShapeBufIndex = 0, ShapeIndexCount = 6, ShapeCounter = 3 });
        var drop = new VmDraw2dShape1276 { BufIndex = 3, LastCommand = 4, UploadedOnce = true };
        drop.Buffers.AddRange([1, 2, 3]);
        b.Commands.Add(new VmDraw2dCmd1276 { Shape = drop, ShapeBufIndex = 2, ShapeIndexCount = 5, ShapeCounter = 4 });
        b.Draw(1, 2, 3, 4);
        Assert.Equal(
"""
clock
viewport 1 2 3 4
set2d 640 480
stencil off
stencil 0 0 0
clear 2
depth off
msaa off
smooth 0
mip 1
swap 03020104 07060508
upload 2 2
vb upload
fog 0
style 8 8
bright 0
fog 2
fade 0.25
mode 4
scissor 0 0 15 53
mode 7
obj 11aabbcc
add 22010203
fogc 01020304 -0
color 1 1 1 1 0.0156862754
soft 200 0
light 0 0
alpha 1 0
matrix 1.5 4 0 0 -2 0.5 0 0 0 0 1 0 0.25 8 0 1
model on
mat 3 4 4 3 12 -1
tex on
texmat 1 0 0 0 -0 -1 -0 -0 0 0 1 0 0 -1 0 1
texmat on
effect 2
drawidx 2 0 2
obj ffffffff
obj2 00000000
add 00000000
texmat off
model off
effect -1
style 2 0
bright 1
fog 2
fade 1
mode 0
scissor -1 -1 -1 -1
fogc 00000000 -0
color 1 1 1 1 0
light 0 0
alpha 1 0
tex off
draw 1 1 4
obj ffffffff
obj2 00000000
add 00000000
texmat off
model off
effect -1
style 0 0
bright 1
fog 2
fade 1
mode 0
scissor -1 -1 -1 -1
fogc 00000000 -0
color 1 1 1 1 0
light 0 0
alpha 1 0
tex off
draw 0 3 1
obj ffffffff
obj2 00000000
add 00000000
texmat off
model off
effect -1
style 0 0
bright 1
fog 2
fade 1
mode 0
scissor -1 -1 -1 -1
fogc 00000000 -0
color 1 1 1 1 0
light 0 0
alpha 1 0
mat 5 1 0 0 7 -1
tex on
draw 1 6 2
obj ffffffff
obj2 00000000
add 00000000
texmat off
model off
effect -1
style 0 0
bright 1
fog 2
fade 1
mode 0
scissor -1 -1 -1 -1
fogc 00000000 -0
color 1 1 1 1 0
light 0 0
alpha 1 0
mat 6 0 0 3 1 -1
tex on
drawidx 2 8 9
obj ffffffff
obj2 00000000
add 00000000
texmat off
model off
effect -1
style 0 0
bright 1
fog 2
fade 1
mode 0
scissor -1 -1 -1 -1
fogc 00000000 -0
color 1 1 1 1 0
light 0 0
alpha 1 0
mat 7 0 0 0 0 -1
tex on
drawidx 2 2 2
obj ffffffff
obj2 00000000
add 00000000
texmat off
model off
effect -1
style 0 0
bright 1
fog 2
fade 1
mode 0
scissor -1 -1 -1 -1
fogc 00000000 -0
color 1 1 1 1 0
light 0 0
alpha 1 0
mat 8 0 0 3 0 -1
tex on
drawidx 2 4 1
obj ffffffff
obj2 00000000
add 00000000
texmat off
model off
effect -1
style 0 0
bright 1
fog 2
fade 1
mode 0
scissor -1 -1 -1 -1
fogc 00000000 -0
color 1 1 1 1 0
light 0 0
alpha 1 0
tex off
vb shape 8
drawidx 2 0 12
vb upload
shape 5 2 0 0 3
obj ffffffff
obj2 00000000
add 00000000
texmat off
model off
effect -1
style 0 0
bright 1
fog 2
fade 1
mode 0
scissor -1 -1 -1 -1
fogc 00000000 -0
color 1 1 1 1 0
light 0 0
alpha 1 0
tex off
vb shape 11
drawidx 2 0 6
vb upload
shape -1 0 0 0 1
obj ffffffff
obj2 00000000
add 00000000
texmat off
model off
effect -1
style 0 0
bright 1
fog 2
fade 1
mode 0
scissor -1 -1 -1 -1
fogc 00000000 -0
color 1 1 1 1 0
light 0 0
alpha 1 0
tex off
vb shape 3
drawidx 2 0 5
vb upload
shape -1 -1 1 0 0
obj ffffffff
obj2 00000000
add 00000000
texmat off
model off
effect -1
scissor -1 -1 -1 -1
style 6
vb screen
stencil off
stencil 0 0 0
tex on
bright on
mode 0
fog 0
fade 1
soft 255 0
reset
unclock
""",
            string.Join('\n', b.Log));
        Assert.False(b.FirstPass);
        Assert.Equal(0, b.Cycle);
        Assert.Equal(0, b.SampleSeq);
        Assert.Equal(0, b.AnisoReads);
        Assert.Equal(3, b.Vertices[0].R);
        Assert.Equal(1, b.Vertices[0].B);
        Assert.Equal(7, b.Vertices[1].R);
        Assert.Equal(5, b.Vertices[1].B);
        Assert.Equal(5, keep.LastCommand);
        Assert.Equal(2, keep.BufIndex);
        Assert.False(keep.NeedsVertexUpload);
        Assert.False(keep.UploadedOnce);
        Assert.Equal(3, keep.Buffers.Count);
        Assert.Equal(-1, zero.LastCommand);
        Assert.Equal(0, zero.BufIndex);
        Assert.False(zero.NeedsVertexUpload);
        Assert.False(zero.UploadedOnce);
        Assert.Equal(new[] { 11 }, zero.Buffers);
        Assert.Equal(-1, drop.LastCommand);
        Assert.Equal(-1, drop.BufIndex);
        Assert.True(drop.NeedsVertexUpload);
        Assert.False(drop.UploadedOnce);
        Assert.Empty(drop.Buffers);

        b.Log.Clear();
        b.Commands.Clear();
        b.HasAnisoCallback = true;
        b.AnisoDisables = true;
        b.Commands.Add(new VmDraw2dCmd1276
        {
            Flags = 1,
            Translation = 4,
            Texture = new VmDraw2dTex1276 { Id = 9, UseType = 1 },
            IndexIndex = 1,
            IndexCount = 1,
        });
        b.Commands.Add(new VmDraw2dCmd1276
        {
            Flags = 8,
            LightLevel = 9,
            Translation = 2,
            Texture = new VmDraw2dTex1276 { Id = 10, UseType = 2 },
            IndexIndex = 0,
            IndexCount = 3,
        });
        b.Draw(1, 2, 3, 4);
        Assert.Equal(
"""
clock
viewport 1 2 3 4
set2d 640 480
stencil off
stencil 0 0 0
clear 2
depth off
msaa off
smooth 0
mip 0
upload 2 2
vb upload
fog 0
style 0 0
bright 1
fog 2
fade 1
mode 0
scissor -1 -1 -1 -1
fogc 00000000 -0
color 1 1 1 1 0
light 0 0
alpha 1 0
mat 9 1 0 0 4 -1
tex on
drawidx 2 1 1
obj ffffffff
obj2 00000000
add 00000000
texmat off
model off
effect -1
style 0 0
bright 1
fog 2
fade 1
mode 0
scissor -1 -1 -1 -1
fogc 00000000 -0
color 1 1 1 1 0
soft 9 0
light 0 0
alpha 1 0
mat 10 1 4 4 2 -1
tex on
drawidx 2 0 3
obj ffffffff
obj2 00000000
add 00000000
texmat off
model off
effect -1
scissor -1 -1 -1 -1
style 6
vb screen
stencil off
stencil 0 0 0
tex on
bright on
mode 0
fog 0
fade 1
soft 255 0
reset
unclock
""",
            string.Join('\n', b.Log));
        Assert.Equal(1, b.AnisoReads);
        Assert.False(b.FirstPass);
        Assert.Equal(3, b.Vertices[0].R);
        Assert.Equal(1, b.Vertices[0].B);
        Assert.DoesNotContain("swap ", string.Join('\n', b.Log));

        var c = new VmDraw2d1276
        {
            ViewportLeft = 10,
            ViewportTop = 5,
            ViewportWidth = 100,
            ViewportHeight = 50,
            GameWidth = 320,
            GameHeight = 200,
            DrawerWidth = 8,
            DrawerHeight = 9,
        };
        c.Vertices.Add(new VmDraw2dVert1276 { R = 9, G = 8, B = 7, A = 6 });
        c.Commands.Add(new VmDraw2dCmd1276 { Flags = 2, Sc0 = 8, Sc1 = 10, Sc2 = 40, Sc3 = 20, IndexIndex = 1, IndexCount = 1 });
        c.Commands.Add(new VmDraw2dCmd1276 { Flags = 2, Sc0 = -40, Sc1 = 10, Sc2 = 16, Sc3 = 20, Type = 2, VertIndex = 0, VertCount = 1 });
        c.Draw(9, 8, 7, 6);
        var shown = c.Log.Where(line => line.StartsWith("scissor ") || line.StartsWith("swap ") || line.StartsWith("draw")).ToList();
        Assert.Equal(
"""
swap 07080906
scissor 13 50 10 2
drawidx 2 1 1
scissor 0 50 15 2
draw 0 0 1
scissor -1 -1 -1 -1
""",
            string.Join('\n', shown));
        Assert.False(c.FirstPass);
        Assert.Equal(7, c.Vertices[0].R);
        Assert.Equal(9, c.Vertices[0].B);
    }
}
