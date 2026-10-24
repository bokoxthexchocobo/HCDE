namespace HCDE.Playsim.Tests;

public class VmPMobjSerializeModelTests
{
    private sealed class TestArchiveSink : IModelArchiveFieldSink
    {
        public bool Writing { get; set; }
        public bool ForceBeginFail { get; set; }
        public Dictionary<string, int> Ints { get; } = new();
        public Dictionary<string, double> Doubles { get; } = new();
        public Dictionary<string, string> Strings { get; } = new();
        public Dictionary<string, int[]> IntArrays { get; } = new();
        public Dictionary<string, (double X, double Y, double Z)> Vectors { get; } = new();
        public int ObjectDepth { get; private set; }
        public bool LastBeginOk { get; private set; } = true;

        public bool HasKey(string key) => Ints.ContainsKey(key) || Doubles.ContainsKey(key) ||
                                          Strings.ContainsKey(key) || IntArrays.ContainsKey(key) ||
                                          Vectors.ContainsKey(key);

        public bool BeginObject(string key)
        {
            ObjectDepth++;
            LastBeginOk = !ForceBeginFail;
            return LastBeginOk;
        }

        public void EndObject() => ObjectDepth--;

        public int ReadInt(string key, int current) => Ints.TryGetValue(key, out var v) ? v : current;
        public void WriteInt(string key, int value) => Ints[key] = value;
        public double ReadDouble(string key, double current) => Doubles.TryGetValue(key, out var v) ? v : current;
        public void WriteDouble(string key, double value) => Doubles[key] = value;
        public int[] ReadIntArray(string key, int[] current) => IntArrays.TryGetValue(key, out var v) ? v : current;
        public void WriteIntArray(string key, int[] value) => IntArrays[key] = value;
        public string ReadString(string key, string current) => Strings.TryGetValue(key, out var v) ? v : current;
        public void WriteString(string key, string value) => Strings[key] = value;

        public void ArcVector3(string key, ref double x, ref double y, ref double z)
        {
            if (Writing)
                Vectors[key] = (x, y, z);
            else if (Vectors.TryGetValue(key, out var v))
            {
                x = v.X;
                y = v.Y;
                z = v.Z;
            }
        }
    }

    [Fact]
    public void SerializeModelId_ReadMissingKey_ReturnsNegativeOne()
    {
        var id = VmPMobjSerializeModel.SerializeModelId(new() { Writing = false, HasKey = false }, "model", 3);
        Assert.Equal(-1, id);
    }

    [Fact]
    public void SerializeModelId_WritePositiveId_EmitsPath()
    {
        string? key = null;
        string? written = null;
        var id = VmPMobjSerializeModel.SerializeModelId(new()
        {
            Writing = true,
            ModelPath = "models/foo.md3",
            WriteModelPath = (k, v) =>
            {
                key = k;
                written = v;
            },
        }, "model", 2);
        Assert.Equal(2, id);
        Assert.Equal("model", key);
        Assert.Equal("models/foo.md3", written);
    }

    [Fact]
    public void SerializeBoneOverrideComponent_RoundTripsFields()
    {
        var sink = new TestArchiveSink { Writing = true };
        var comp = new VmPMobjSerializeModel.BoneOverrideComponent { Mode = 2, Cur = 9 };
        VmPMobjSerializeModel.SerializeBoneOverrideComponent("bone", comp, sink);
        sink.Writing = false;
        var read = VmPMobjSerializeModel.SerializeBoneOverrideComponent("bone", new(), sink);
        Assert.Equal(2, read.Mode);
        Assert.Equal(9, read.Cur);
    }

    [Fact]
    public void SerializeTrs_WritesThreeVectors()
    {
        var sink = new TestArchiveSink { Writing = true };
        var trs = new VmPMobjSerializeModel.TrsFields { TranslationZ = 4, ScalingY = 2 };
        VmPMobjSerializeModel.SerializeTrs("trs", trs, sink);
        Assert.Equal((0, 0, 4), sink.Vectors["translation"]);
        Assert.Equal((1, 2, 1), sink.Vectors["scaling"]);
    }

    [Fact]
    public void SerializeModelOverride_EmitsModelIdAndSkins()
    {
        var sink = new TestArchiveSink { Writing = true };
        var mo = VmPMobjSerializeModel.SerializeModelOverride(
            "mo",
            new() { ModelId = 1, SurfaceSkinIds = [3, 4] },
            new() { Writing = true, ModelPath = "x" },
            sink);
        Assert.Equal(1, mo.ModelId);
        Assert.Equal([3, 4], sink.IntArrays["surfaceSkinIDs"]);
    }

    [Fact]
    public void SerializeAnimModelOverride_ReadGuard()
    {
        var sink = new TestArchiveSink { Writing = false, ForceBeginFail = true };
        var amo = VmPMobjSerializeModel.SerializeAnimModelOverride("amo", new(), new(), sink);
        Assert.Equal(-1, amo.Id);
    }

    [Fact]
    public void SerializeModelAnim_ReadsAllFields()
    {
        var sink = new TestArchiveSink
        {
            Writing = false,
            Ints =
            {
                ["firstFrame"] = 3,
                ["lastFrame"] = 10,
                ["loopFrame"] = 1,
                ["startFrame"] = 2,
                ["flags"] = 5,
                ["startTic"] = 7,
                ["switchOffset"] = 8,
            },
            Doubles = { ["framerate"] = 35 },
        };
        var anim = VmPMobjSerializeModel.SerializeModelAnim("anim", new(), sink);
        Assert.Equal(3, anim.FirstFrame);
        Assert.Equal(35, anim.Framerate);
        Assert.Equal(8, anim.SwitchOffset);
    }

    [Fact]
    public void ReadModelAnimFrameType_PrecalcMapsToNull()
    {
        Assert.Equal(VmPMobjSerializeModel.ModelAnimFrameKind.Null,
            VmPMobjSerializeModel.ReadModelAnimFrameType("precalcIQM"));
    }

    [Fact]
    public void SerializeModelAnimFrame_LegacyFirstFrameClearsInterp()
    {
        var sink = new TestArchiveSink { Writing = false };
        var state = VmPMobjSerializeModel.SerializeModelAnimFrame(
            "frame",
            new() { Kind = VmPMobjSerializeModel.ModelAnimFrameKind.Interp, Interp = new() { Frame1 = 1 } },
            sink,
            hasFirstFrameKeyOnRead: true);
        Assert.Equal(VmPMobjSerializeModel.ModelAnimFrameKind.Null, state.Kind);
        Assert.Null(state.Interp);
    }

    [Fact]
    public void SerializeModelAnimFrame_InterpRoundTrip()
    {
        var sink = new TestArchiveSink { Writing = true };
        var state = new VmPMobjSerializeModel.ModelAnimFrameState
        {
            Kind = VmPMobjSerializeModel.ModelAnimFrameKind.Interp,
            Interp = new() { Inter = 0.5, Frame1 = 2, Frame2 = 3 },
        };
        VmPMobjSerializeModel.SerializeModelAnimFrame("frame", state, sink, false);
        sink.Writing = false;
        sink.Strings["type"] = "interp";
        sink.Doubles["inter"] = 0.5;
        sink.Ints["frame1"] = 2;
        sink.Ints["frame2"] = 3;
        var read = VmPMobjSerializeModel.SerializeModelAnimFrame("frame", new(), sink, false);
        Assert.Equal(VmPMobjSerializeModel.ModelAnimFrameKind.Interp, read.Kind);
        Assert.Equal(2, read.Interp!.Frame1);
    }

    [Fact]
    public void WriteModelAnimFrameTypeToken_Nullptr()
    {
        Assert.Equal("nullptr",
            VmPMobjSerializeModel.WriteModelAnimFrameTypeToken(VmPMobjSerializeModel.ModelAnimFrameKind.Null));
    }
}
