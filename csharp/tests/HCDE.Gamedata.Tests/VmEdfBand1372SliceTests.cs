namespace HCDE.Gamedata.Tests;

public class VmEdfBand1372SliceTests
{
    [Fact]
    public void HeaderDehackedAndThingScan()
    {
        var slice = new VmEdfBand1372Slice();
        var thing = new VmEdfBand1372Slice.Thing();
        slice.ParseThingHeader(thing, "thingtype \"Imp Lord\" : Demon 3001, 0x2A {");
        Assert.Equal("Imp Lord", thing.Name);
        Assert.Equal("Demon", thing.BaseName);
        Assert.Equal(3001, thing.DoomedNum);
        Assert.Equal(42, thing.DehackedNum);

        var octal = new VmEdfBand1372Slice.Thing();
        slice.ParseThingHeader(octal, "thingtype Imp : Demon 010 11");
        Assert.Equal(8, octal.DoomedNum);
        Assert.Equal(11, octal.DehackedNum);

        var one = new VmEdfBand1372Slice.Thing();
        slice.ParseThingHeader(one, "thingtype Imp : Demon 3001");
        Assert.Equal(3001, one.DoomedNum);
        Assert.Equal(-1, one.DehackedNum);

        var later = new VmEdfBand1372Slice.Thing();
        slice.ParseThingHeader(later, "thingtype A : B 1 : C 2 3");
        Assert.Equal("B", later.BaseName);
        Assert.Equal(1, later.DoomedNum);
        Assert.Equal(2, later.DehackedNum);

        Assert.False(VmEdfBand1372Slice.StrictInt("08", out _));
        Assert.False(VmEdfBand1372Slice.StrictInt("10 ", out _));
        Assert.True(VmEdfBand1372Slice.StrictInt(" 10", out var spaced));
        Assert.Equal(10, spaced);
        Assert.False(VmEdfBand1372Slice.StrictInt("2147483648", out _));

        var lines = new[]
        {
            new VmEdfBand1372Slice.SourceLine { Text = "  ", Line = 1 },
            new VmEdfBand1372Slice.SourceLine { Text = "health 1 { nested", Line = 4 },
            new VmEdfBand1372Slice.SourceLine { Text = "{", Line = 5 },
        };
        var parsed = slice.ParseThingBlock("thingtype Imp : Demon 7 8", lines, -1, 9, false);
        Assert.NotNull(parsed);
        Assert.Equal(7, parsed!.DoomedNum);
        Assert.Equal(8, parsed.DehackedNum);
        Assert.Equal(1, slice.ThingTypes);
        Assert.Equal(0, slice.ThingDeltas);
        Assert.Equal(0, slice.NestedDepth);
        Assert.Equal(2, slice.Prepared.Count);
        Assert.Equal("health 1 ", slice.Prepared[0].Part);
        Assert.Equal(4, slice.Prepared[0].Line);
        Assert.Equal("", slice.Prepared[1].Part);

        var heredoc = false;
        Assert.False(slice.TryPrepare(ref heredoc, "   \t", out _));
        heredoc = true;
        Assert.False(slice.TryPrepare(ref heredoc, "still going", out _));
        Assert.True(heredoc);
        Assert.False(slice.TryPrepare(ref heredoc, "done \"@ tail", out _));
        Assert.False(heredoc);

        slice.FileName = lump => lump == 4 ? "root.edf" : null;
        var missing = slice.ParseThingBlock("thingdelta", [], 4, 12, true);
        Assert.Null(missing);
        Assert.Equal(1, slice.InvalidProperties);
        Assert.Equal(0, slice.ThingDeltas);
        Assert.Equal("thingdelta", slice.Issues[0].Context);
        Assert.Equal("missing thing name", slice.Issues[0].Detail);
        Assert.Equal("root.edf", slice.Issues[0].Source);
        Assert.Equal(12, slice.Issues[0].Line);
        var bare = new VmEdfBand1372Slice();
        var thrown = Assert.Throws<InvalidOperationException>(
            () => bare.RecordIssue("reverb", "needs file", 3, 1));
        Assert.Equal("GetFileFullName", thrown.Message);
        for (var n = 0; n < 20; n++)
        {
            slice.RecordIssue("reverb", "extra", -1, n);
        }

        Assert.Equal(20, slice.Issues.Count);
    }

    [Fact]
    public void ReverbHeaderAndProperties()
    {
        var slice = new VmEdfBand1372Slice();
        var reverb = new VmEdfBand1372Slice.Reverb();
        Assert.Equal(0.5f, reverb.Room);
        Assert.Equal(1f, reverb.Damping);
        Assert.Equal(1f, reverb.DecayHFLimit);
        Assert.Equal(5000f, reverb.HFReference);
        Assert.Equal(250f, reverb.LFReference);
        Assert.Equal(0.33333334f, reverb.wetscale);
        Assert.Equal(0f, reverb.dryscale);
        Assert.Equal(1f, reverb.width);
        Assert.Equal(0, reverb.predelay);
        Assert.Equal(1f, reverb.eq_lowgain);
        slice.ParseReverbHeader(reverb, "reverb Hall : 1, 2 {");
        Assert.Equal("Hall", reverb.Name);
        Assert.Equal(1, reverb.Id1);
        Assert.Equal(2, reverb.Id2);

        var partial = new VmEdfBand1372Slice.Reverb();
        slice.ParseReverbHeader(partial, "reverb Hall : 7");
        Assert.Equal(7, partial.Id1);
        Assert.Equal(-1, partial.Id2);
        var nameless = new VmEdfBand1372Slice.Reverb();
        slice.ParseReverbHeader(nameless, "reverb");
        Assert.Equal("", nameless.Name);

        reverb.SourceLump = -1;
        reverb.Line = 30;
        slice.ParseReverbProperty(reverb, "RoomSize 1.5junk");
        slice.ParseReverbProperty(reverb, "damping 0.25");
        slice.ParseReverbProperty(reverb, "wetscale 0.5");
        slice.ParseReverbProperty(reverb, "dryscale 0.1");
        slice.ParseReverbProperty(reverb, "width 2");
        slice.ParseReverbProperty(reverb, "predelay = 12");
        slice.ParseReverbProperty(reverb, "-equalized");
        slice.ParseReverbProperty(reverb, "eq.lowfreq 100");
        slice.ParseReverbProperty(reverb, "eq.highfreq 8000");
        slice.ParseReverbProperty(reverb, "eq.lowgain 0.2");
        slice.ParseReverbProperty(reverb, "eq.midgain 0.3");
        slice.ParseReverbProperty(reverb, "eq.highgain 0.4");
        slice.ParseReverbProperty(reverb, "roomsize");
        slice.ParseReverbProperty(reverb, "");
        slice.ParseReverbProperty(reverb, "mystery 1");
        Assert.Equal(1.5f, reverb.Room);
        Assert.Equal(0.25f, reverb.Damping);
        Assert.Equal(0.5f, reverb.wetscale);
        Assert.Equal(0.1f, reverb.dryscale);
        Assert.Equal(2f, reverb.width);
        Assert.Equal(12, reverb.predelay);
        Assert.Equal(0f, reverb.DecayHFLimit);
        Assert.Equal(100f, reverb.LFReference);
        Assert.Equal(8000f, reverb.HFReference);
        Assert.Equal(0.2f, reverb.eq_lowgain);
        Assert.Equal(0.3f, reverb.eq_midgain);
        Assert.Equal(0.4f, reverb.eq_highgain);
        Assert.Equal(14, slice.Properties);
        Assert.Equal(1, slice.UnsupportedProperties);
        Assert.Equal("Hall property 'mystery' is not supported", slice.Issues[0].Detail);
        Assert.Equal("<unknown>", slice.Issues[0].Source);
        slice.ParseReverbProperty(reverb, "+equalized");
        Assert.Equal(1f, reverb.DecayHFLimit);
        var bad = Assert.Throws<FormatException>(() => slice.ParseReverbProperty(reverb, "width nope"));
        Assert.Equal("stof", bad.Message);
    }
}
