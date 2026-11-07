using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDMain1418SliceTests
{
    [Fact]
    public void NerveAndMacHexen()
    {
        byte[] abc = VmDMain1418Slice.Digest(new VmDMain1418Slice.Wad
        {
            Data = "abc"u8.ToArray(),
            Cursor = 2,
        });
        Assert.Equal(new byte[]
        {
            0x90, 0x01, 0x50, 0x98, 0x3c, 0xd2, 0x4f, 0xb0,
            0xd6, 0x96, 0x3f, 0x7d, 0x28, 0xe1, 0x7f, 0x72,
        }, abc);
        byte[] empty = VmDMain1418Slice.Digest(new VmDMain1418Slice.Wad
        {
            Data = Array.Empty<byte>(),
            Cursor = 9,
        });
        Assert.Equal(new byte[]
        {
            0xd4, 0x1d, 0x8c, 0xd9, 0x8f, 0x00, 0xb2, 0x04,
            0xe9, 0x80, 0x09, 0x98, 0xec, 0xf8, 0x42, 0x7e,
        }, empty);
        byte[] big = new byte[10000];
        for (int i = 0; i < big.Length; i++)
            big[i] = (byte)(i * 3);
        var wide = new VmDMain1418Slice.Wad { Data = big, Cursor = 50 };
        Assert.Equal(System.Security.Cryptography.MD5.HashData(big), VmDMain1418Slice.Digest(wide));
        Assert.Equal(10000, wide.Cursor);
        Assert.Equal(1, wide.Seeks);
        Assert.Equal(new long[] { 3819855, 3821966, 3821885, 4003409 }, VmDMain1418Slice.NerveSizes);
        Assert.Equal("967d5ae23daf45196212ae1b605da3b0", Hex(VmDMain1418Slice.NerveDigests[0]));
        Assert.Equal("4214c47651b63ee2257b1c2490a518c9", Hex(VmDMain1418Slice.NerveDigests[1]));
        Assert.Equal("3544e1903091c50ba50049db74cd7d25", Hex(VmDMain1418Slice.NerveDigests[2]));
        Assert.Equal("9143b392392a7ac870c2ca36ac65af45", Hex(VmDMain1418Slice.NerveDigests[3]));
        Assert.Equal(13596228, VmDMain1418Slice.DemoSize);
        Assert.Equal(13749984, VmDMain1418Slice.BetaSize);
        Assert.Equal(21078584, VmDMain1418Slice.FullSize);
        Assert.Equal("925f9f5000e17dc84b0a6a3bed3a6f31", Hex(VmDMain1418Slice.DemoDigest));
        Assert.Equal("2af1b27cd11fb159e608472a1b53e40e", Hex(VmDMain1418Slice.BetaDigest));
        Assert.Equal("b68140a796f6fd7f3a5d3226a32b93be", Hex(VmDMain1418Slice.FullDigest));

        var other = Nerve();
        other.GameType = VmDMain1418Slice.Hexen;
        VmDMain1418Slice.RenameNerve(other);
        Assert.Equal("MAP01", VmDMain1418Slice.Text(other.Lumps[2].Name));
        Assert.Equal(0, other.Wads[1].Hashed);

        var skipped = Nerve();
        skipped.Wads[1].Data = null;
        skipped.Wads.Add(new VmDMain1418Slice.Wad { Data = new byte[] { 9, 8, 7 }, First = 5, Last = 5 });
        VmDMain1418Slice.RenameNerve(skipped);
        Assert.Equal(0, skipped.Wads[1].Hashed);
        Assert.Equal(0, skipped.Wads[2].Hashed);
        Assert.Equal("CWILV00", VmDMain1418Slice.Text(skipped.Lumps[1].Name));

        var mismatch = Nerve();
        mismatch.NerveSizes = new long[] { 4, 4, 4, 4 };
        mismatch.Wads[1].Data = new byte[] { 1, 2, 3, 4 };
        mismatch.NerveDigests[3] = new byte[16];
        VmDMain1418Slice.RenameNerve(mismatch);
        Assert.Equal(1, mismatch.Wads[1].Hashed);
        Assert.Equal(1, mismatch.Wads[1].Seeks);
        Assert.Equal(4, mismatch.Wads[1].Cursor);
        Assert.Equal("MAP01", VmDMain1418Slice.Text(mismatch.Lumps[2].Name));

        byte[] hit = { 4, 5, 6, 7 };
        var lastWins = Nerve();
        lastWins.NerveSizes = new long[] { 4, 4, 4, 4 };
        lastWins.Wads[1].Data = hit;
        lastWins.NerveDigests[0] = VmDMain1418Slice.Digest(new VmDMain1418Slice.Wad { Data = hit });
        lastWins.NerveDigests[3] = new byte[16];
        VmDMain1418Slice.RenameNerve(lastWins);
        Assert.Equal(1, lastWins.Wads[1].Hashed);
        Assert.Equal("MAP01", VmDMain1418Slice.Text(lastWins.Lumps[2].Name));

        var nerve = Nerve();
        nerve.Iwad = 1;
        nerve.NerveSizes = new[] { 9L, 4, 6, 7 };
        byte[] body = { 8, 9, 1, 2 };
        nerve.Wads[0].Data = body;
        nerve.Wads[1].Data = null;
        var matched = new VmDMain1418Slice.Wad { Data = body, First = 1, Last = 4, Cursor = 3 };
        var ignored = new VmDMain1418Slice.Wad { Data = body, First = 0, Last = 0 };
        nerve.Wads.Add(matched);
        nerve.Wads.Add(ignored);
        nerve.NerveDigests[1] = VmDMain1418Slice.Digest(new VmDMain1418Slice.Wad { Data = body });
        VmDMain1418Slice.RenameNerve(nerve);
        Assert.Equal(0, nerve.Wads[0].Hashed);
        Assert.Equal(0, nerve.Wads[1].Hashed);
        Assert.Equal(1, matched.Hashed);
        Assert.Equal(1, matched.Seeks);
        Assert.Equal(4, matched.Cursor);
        Assert.Equal(0, ignored.Hashed);
        Assert.Equal("MAP01", VmDMain1418Slice.Text(nerve.Lumps[0].Name));
        Assert.Equal("NWILV00", VmDMain1418Slice.Text(nerve.Lumps[1].Name));
        Assert.Equal("LEVEL01", VmDMain1418Slice.Text(nerve.Lumps[2].Name));
        Assert.Equal(new byte[] { (byte)'L', (byte)'E', (byte)'V', (byte)'E', (byte)'L', (byte)'0', (byte)'1', 0, 0 }, nerve.Lumps[2].Name);
        Assert.Equal("LEVEL0A", VmDMain1418Slice.Text(nerve.Lumps[3].Name));
        Assert.Equal("MAP10", VmDMain1418Slice.Text(nerve.Lumps[4].Name));
        Assert.Equal("OTHER", VmDMain1418Slice.Text(nerve.Lumps[5].Name));

        var quiet = Hexen(new byte[] { 1, 2, 3 });
        quiet.GameType = VmDMain1418Slice.Doom;
        quiet.DemoSize = 3;
        quiet.DemoDigest = VmDMain1418Slice.Digest(new VmDMain1418Slice.Wad { Data = quiet.Wads[0].Data });
        VmDMain1418Slice.FixMacHexen(quiet);
        Assert.Equal(0, quiet.Wads[0].Hashed);
        Assert.Equal((byte)'A', quiet.Lumps[200].Name[0]);

        var missing = Hexen(new byte[] { 1 });
        missing.Wads[0].Data = null;
        Assert.Throws<NullReferenceException>(() => VmDMain1418Slice.FixMacHexen(missing));

        var sized = Hexen(new byte[] { 9, 9, 9, 9 });
        VmDMain1418Slice.FixMacHexen(sized);
        Assert.Equal(0, sized.Wads[0].Hashed);
        Assert.Equal((byte)'A', sized.Lumps[399].Name[0]);

        byte[] mac = { 3, 1, 4, 1, 5 };
        byte[] macSum = VmDMain1418Slice.Digest(new VmDMain1418Slice.Wad { Data = mac });
        var rejected = Hexen(mac);
        rejected.DemoSize = mac.Length;
        VmDMain1418Slice.FixMacHexen(rejected);
        Assert.Equal(1, rejected.Wads[0].Hashed);
        Assert.Equal((byte)'A', rejected.Lumps[101].Name[0]);
        Assert.Equal((byte)'B', rejected.Lumps[101].Name[1]);

        var demo = Hexen(mac);
        demo.FullSize = mac.Length;
        demo.FullDigest = macSum;
        demo.Wads[0].Cursor = 4;
        VmDMain1418Slice.FixMacHexen(demo);
        Assert.Equal(1, demo.Wads[0].Seeks);
        Assert.Equal(0, demo.Lumps[101].Name[0]);
        Assert.Equal((byte)'B', demo.Lumps[101].Name[1]);
        Assert.Equal(0, demo.Lumps[399].Name[0]);
        Assert.Equal((byte)'A', demo.Lumps[100].Name[0]);

        var beta = Hexen(mac);
        beta.DemoSize = mac.Length;
        beta.BetaDigest = macSum;
        VmDMain1418Slice.FixMacHexen(beta);
        Assert.Equal((byte)'A', beta.Lumps[88].Name[0]);
        Assert.Equal(0, beta.Lumps[89].Name[0]);
        Assert.Equal(0, beta.Lumps[387].Name[0]);
        Assert.Equal((byte)'A', beta.Lumps[388].Name[0]);
        Assert.Equal((byte)'A', beta.Lumps[399].Name[0]);
    }

    [Fact]
    public void StrifeTeaserVoices()
    {
        var retail = Voices(VmDMain1418Slice.Strife, 0);
        VmDMain1418Slice.FindStrifeTeaserVoices(retail);
        Assert.Equal(VmDMain1418Slice.Global, retail.Lumps[0].Space);

        var otherBits = Voices(VmDMain1418Slice.Strife, 1);
        VmDMain1418Slice.FindStrifeTeaserVoices(otherBits);
        Assert.Equal(VmDMain1418Slice.Global, otherBits.Lumps[0].Space);

        var doom = Voices(VmDMain1418Slice.Doom, VmDMain1418Slice.Shareware);
        VmDMain1418Slice.FindStrifeTeaserVoices(doom);
        Assert.Equal(VmDMain1418Slice.Global, doom.Lumps[0].Space);

        var mixed = Voices(VmDMain1418Slice.Strife | VmDMain1418Slice.Doom, VmDMain1418Slice.Shareware);
        VmDMain1418Slice.FindStrifeTeaserVoices(mixed);
        Assert.Equal(VmDMain1418Slice.Global, mixed.Lumps[0].Space);

        var teaser = Voices(VmDMain1418Slice.Strife, VmDMain1418Slice.Shareware | 8);
        teaser.Lumps.Add(new VmDMain1418Slice.Lump(VmDMain1418Slice.Global, 0, "VOC"));
        teaser.Lumps.Add(new VmDMain1418Slice.Lump(VmDMain1418Slice.Global, 0, "VOC1"));
        teaser.Lumps.Add(new VmDMain1418Slice.Lump(VmDMain1418Slice.Global, 0, "VOC12345"));
        teaser.Lumps.Add(new VmDMain1418Slice.Lump(VmDMain1418Slice.Global, 0, "VOC12A00"));
        teaser.Lumps.Add(new VmDMain1418Slice.Lump(VmDMain1418Slice.Global, 0, "voc12345"));
        teaser.Lumps.Add(new VmDMain1418Slice.Lump(1, 0, "VOC12345"));
        teaser.Lumps.Add(new VmDMain1418Slice.Lump(VmDMain1418Slice.Global, 2, "VOC12345"));
        var high = new VmDMain1418Slice.Lump(VmDMain1418Slice.Global, 0, "VOC12345");
        high.Name[3] = 0xB2;
        teaser.Lumps.Add(high);
        var tail = new VmDMain1418Slice.Lump(VmDMain1418Slice.Global, 0, "VOC12345");
        tail.Name[7] = 0xFF;
        teaser.Lumps.Add(tail);
        VmDMain1418Slice.FindStrifeTeaserVoices(teaser);
        Assert.Equal(VmDMain1418Slice.StrifeVoices, teaser.Lumps[0].Space);
        Assert.Equal(VmDMain1418Slice.StrifeVoices, teaser.Lumps[1].Space);
        Assert.Equal(VmDMain1418Slice.StrifeVoices, teaser.Lumps[2].Space);
        Assert.Equal(VmDMain1418Slice.StrifeVoices, teaser.Lumps[3].Space);
        Assert.Equal(VmDMain1418Slice.Global, teaser.Lumps[4].Space);
        Assert.Equal(VmDMain1418Slice.Global, teaser.Lumps[5].Space);
        Assert.Equal(1, teaser.Lumps[6].Space);
        Assert.Equal(VmDMain1418Slice.Global, teaser.Lumps[7].Space);
        Assert.Equal(VmDMain1418Slice.Global, high.Space);
        Assert.Equal(VmDMain1418Slice.Global, tail.Space);
    }

    static VmDMain1418Slice.State Nerve()
    {
        var state = new VmDMain1418Slice.State();
        state.Wads.Add(new VmDMain1418Slice.Wad { Data = new byte[] { 1 }, First = 0, Last = 0 });
        state.Wads.Add(new VmDMain1418Slice.Wad { Data = new byte[] { 2, 2 }, First = 1, Last = 5 });
        state.Lumps.Add(new VmDMain1418Slice.Lump(0, 0, "MAP01"));
        state.Lumps.Add(new VmDMain1418Slice.Lump(0, 1, "CWILV00"));
        state.Lumps.Add(new VmDMain1418Slice.Lump(0, 1, "MAP01"));
        state.Lumps.Add(new VmDMain1418Slice.Lump(0, 1, "MAP0A"));
        state.Lumps.Add(new VmDMain1418Slice.Lump(0, 1, "MAP10"));
        state.Lumps.Add(new VmDMain1418Slice.Lump(0, 1, "OTHER"));
        return state;
    }

    static VmDMain1418Slice.State Hexen(byte[] data)
    {
        var state = new VmDMain1418Slice.State { GameType = VmDMain1418Slice.Hexen };
        state.Wads.Add(new VmDMain1418Slice.Wad { Data = data, First = 0, Last = 399 });
        for (int i = 0; i < 400; i++)
            state.Lumps.Add(new VmDMain1418Slice.Lump(0, 0, "ABCDEFGH"));
        return state;
    }

    static VmDMain1418Slice.State Voices(int game, int flags)
    {
        var state = new VmDMain1418Slice.State { GameType = game, Flags = flags };
        state.Lumps.Add(new VmDMain1418Slice.Lump(VmDMain1418Slice.Global, 0, "VOC00000"));
        return state;
    }

    static string Hex(byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();
}
