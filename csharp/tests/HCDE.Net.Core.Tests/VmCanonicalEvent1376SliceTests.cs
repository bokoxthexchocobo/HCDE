namespace HCDE.Net.Core.Tests;

public class VmCanonicalEvent1376SliceTests
{
    [Fact]
    public void CVarChangeAndRunArgs()
    {
        var s = VmCanonicalEvent1376Slice.DemSinfChanged;
        var xor = VmCanonicalEvent1376Slice.DemSinfChangedXor;

        var zero = RunCVar(s, new byte[] { 0x00 }, 8);
        Assert.False(zero.Ok);
        Assert.Equal(1, zero.Input);
        Assert.Empty(zero.Written);

        var missing = RunCVar(s, Array.Empty<byte>(), 8);
        Assert.False(missing.Ok);
        Assert.Equal(0, missing.Input);

        var tooLong = RunCVar(s, new byte[] { 0x05, (byte)'a', (byte)'b' }, 8);
        Assert.False(tooLong.Ok);
        Assert.Equal(1, tooLong.Input);
        Assert.Empty(tooLong.Written);

        var full = RunCVar(s, new byte[] { 0x03, (byte)'s', (byte)'v', (byte)'_', 0x01 }, 0);
        Assert.False(full.Ok);
        Assert.Equal(1, full.Input);
        Assert.Empty(full.Written);

        var partial = RunCVar(s, new byte[] { 0x04, (byte)'n', (byte)'a', (byte)'m', (byte)'e' }, 3);
        Assert.False(partial.Ok);
        Assert.Equal(1, partial.Input);
        Assert.Equal(new byte[] { VmCanonicalEvent1376Slice.CvarBool, 0x00, 0x04 }, partial.Written);

        var boolean = RunCVar(s, new byte[] { 0x03, (byte)'s', (byte)'v', (byte)'_', 0x01 }, 16);
        Assert.True(boolean.Ok);
        Assert.Equal(5, boolean.Input);
        Assert.Equal(new byte[] { 0x00, 0x00, 0x03, (byte)'s', (byte)'v', (byte)'_', 0x01 }, boolean.Written);

        var integer = RunCVar(s, new byte[] { 0x41, (byte)'a', 1, 2, 3, 4 }, 16);
        Assert.True(integer.Ok);
        Assert.Equal(new byte[] { VmCanonicalEvent1376Slice.CvarInt, 0x00, 0x01, (byte)'a', 1, 2, 3, 4 }, integer.Written);

        var floating = RunCVar(s, new byte[] { 0x81, (byte)'f', 9, 8, 7, 6 }, 16);
        Assert.True(floating.Ok);
        Assert.Equal(new byte[] { VmCanonicalEvent1376Slice.CvarFloat, 0x00, 0x01, (byte)'f', 9, 8, 7, 6 }, floating.Written);

        var text = RunCVar(s, new byte[] { 0xC1, (byte)'m', (byte)'a', (byte)'b', 0 }, 16);
        Assert.True(text.Ok);
        Assert.Equal(5, text.Input);
        Assert.Equal(new byte[] { VmCanonicalEvent1376Slice.CvarString, 0x00, 0x01, (byte)'m', 0x00, 0x02, (byte)'a', (byte)'b' }, text.Written);

        var bare = RunCVar(s, new byte[] { 0xC1, (byte)'m', (byte)'a', (byte)'b' }, 16);
        Assert.False(bare.Ok);
        Assert.Equal(2, bare.Input);
        Assert.Equal(new byte[] { VmCanonicalEvent1376Slice.CvarString, 0x00, 0x01, (byte)'m' }, bare.Written);

        var flipped = RunCVar(xor, new byte[] { 0xC1, (byte)'m', 0xAB }, 16);
        Assert.True(flipped.Ok);
        Assert.Equal(3, flipped.Input);
        Assert.Equal(new byte[] { VmCanonicalEvent1376Slice.CvarString, 0x00, 0x01, (byte)'m', 0xAB }, flipped.Written);

        var named = RunArgs(true, new byte[] { (byte)'a', (byte)'b', 0, 0x81, 1, 2, 3, 4, 9 }, 32);
        Assert.True(named.Ok);
        Assert.Equal(8, named.Input);
        Assert.Equal(new byte[] { 0x00, 0x02, (byte)'a', (byte)'b', 0x81, 1, 2, 3, 4 }, named.Written);

        var namedEnd = RunArgs(true, new byte[] { (byte)'a', 0, 0x00 }, 16);
        Assert.False(namedEnd.Ok);
        Assert.Equal(3, namedEnd.Input);
        Assert.Equal(new byte[] { 0x00, 0x01, (byte)'a', 0x00 }, namedEnd.Written);

        var namedSpare = RunArgs(true, new byte[] { (byte)'a', 0, 0x00, 0xFF }, 16);
        Assert.True(namedSpare.Ok);
        Assert.Equal(3, namedSpare.Input);
        Assert.Equal(new byte[] { 0x00, 0x01, (byte)'a', 0x00 }, namedSpare.Written);

        var tight = RunArgs(true, new byte[] { (byte)'a', 0, 0x02, 1, 2, 3, 4, 5, 6, 7, 8 }, 3);
        Assert.False(tight.Ok);
        Assert.Equal(3, tight.Input);
        Assert.Equal(new byte[] { 0x00, 0x01, (byte)'a' }, tight.Written);

        var plain = RunArgs(false, new byte[] { 0x00, 0x20, 1, 4, 5, 6, 7 }, 16);
        Assert.True(plain.Ok);
        Assert.Equal(7, plain.Input);
        Assert.Equal(new byte[] { 0x00, 0x20, 1, 4, 5, 6, 7 }, plain.Written);

        var wide = RunArgs(false, new byte[] { 0x00, 0x20, 0x81, 1, 2, 3, 4 }, 16);
        Assert.False(wide.Ok);
        Assert.Equal(3, wide.Input);
        Assert.Equal(new byte[] { 0x00, 0x20, 0x81 }, wide.Written);

        var zeroArgs = RunArgs(false, new byte[] { 0x00, 0x01, 0x00 }, 16);
        Assert.False(zeroArgs.Ok);
        Assert.Equal(3, zeroArgs.Input);
        Assert.Equal(new byte[] { 0x00, 0x01, 0x00 }, zeroArgs.Written);
    }

    [Fact]
    public void EventPayloadFamilies()
    {
        var empty = Run(VmCanonicalEvent1376Slice.DemSuicide, new byte[] { 9 }, 8);
        Assert.True(empty.Ok);
        Assert.Equal(0, empty.Input);
        Assert.Empty(empty.Written);
        Assert.True(Run(VmCanonicalEvent1376Slice.DemUseFlechette, Array.Empty<byte>(), 1).Ok);
        Assert.False(Run(VmCanonicalEvent1376Slice.DemStop, new byte[] { 1 }, 8).Ok);

        var say = Run(VmCanonicalEvent1376Slice.DemSay, new byte[] { 0x07, (byte)'h', (byte)'i', 0 }, 16);
        Assert.True(say.Ok);
        Assert.Equal(new byte[] { 0x07, 0x00, 0x02, (byte)'h', (byte)'i' }, say.Written);
        Assert.False(Run(VmCanonicalEvent1376Slice.DemTaunt, new byte[] { 0x01 }, 8).Ok);

        var give = Run(VmCanonicalEvent1376Slice.DemGiveCheat, new byte[] { (byte)'i', 0, 1, 2, 3, 4 }, 16);
        Assert.True(give.Ok);
        Assert.Equal(new byte[] { 0x00, 0x01, (byte)'i', 1, 2, 3, 4 }, give.Written);
        var setInv = Run(VmCanonicalEvent1376Slice.DemSetInv, new byte[] { (byte)'i', 0, 1, 2, 3, 4, 5 }, 16);
        Assert.True(setInv.Ok);
        Assert.Equal(new byte[] { 0x00, 0x01, (byte)'i', 1, 2, 3, 4, 5 }, setInv.Written);

        var zsc = Run(VmCanonicalEvent1376Slice.DemZscCmd, new byte[] { (byte)'c', 0, 0x00, 0x02, 8, 9 }, 16);
        Assert.True(zsc.Ok);
        Assert.Equal(new byte[] { 0x00, 0x01, (byte)'c', 0x00, 0x02, 8, 9 }, zsc.Written);
        var zscZero = Run(VmCanonicalEvent1376Slice.DemZscCmd, new byte[] { (byte)'c', 0, 0x00, 0x00 }, 16);
        Assert.False(zscZero.Ok);
        Assert.Equal(new byte[] { 0x00, 0x01, (byte)'c', 0x00, 0x00 }, zscZero.Written);
        var zscSpare = Run(VmCanonicalEvent1376Slice.DemZscCmd, new byte[] { (byte)'c', 0, 0x00, 0x00, 0xEE }, 16);
        Assert.True(zscSpare.Ok);
        Assert.Equal(4, zscSpare.Input);

        var summon = Run(VmCanonicalEvent1376Slice.DemSummon2, Cat(new byte[] { (byte)'z', 0 }, Pad(25)), 64);
        Assert.True(summon.Ok);
        Assert.Equal(3 + 25, summon.Written.Length);
        Assert.Equal(new byte[] { 0x04, 0x00, 0x03, (byte)'m', (byte)'a', (byte)'p' }, Run(VmCanonicalEvent1376Slice.DemChangeMap2, new byte[] { 0x04, (byte)'m', (byte)'a', (byte)'p', 0 }, 16).Written);
        Assert.Equal(new byte[] { 0x00, 0x03, (byte)'d', (byte)'_', (byte)'e' }, Run(VmCanonicalEvent1376Slice.DemMusicChange, new byte[] { (byte)'d', (byte)'_', (byte)'e', 0 }, 16).Written);

        Assert.Equal(6, Run(VmCanonicalEvent1376Slice.DemWarpCheat, new byte[] { 1, 2, 3, 4, 5, 6 }, 8).Written.Length);
        Assert.Equal(8, Run(VmCanonicalEvent1376Slice.DemInvDrop, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 8).Written.Length);
        Assert.Equal(4, Run(VmCanonicalEvent1376Slice.DemChangeSkill, new byte[] { 1, 2, 3, 4 }, 8).Written.Length);
        var blocked = Run(VmCanonicalEvent1376Slice.DemGenericCheat, new byte[] { 0x2A }, 0);
        Assert.False(blocked.Ok);
        Assert.Equal(0, blocked.Input);
        Assert.Equal(new byte[] { 0x2A }, Run(VmCanonicalEvent1376Slice.DemKick, new byte[] { 0x2A }, 4).Written);

        var save = Run(VmCanonicalEvent1376Slice.DemSaveGame, new byte[] { (byte)'f', 0, (byte)'d', 0 }, 16);
        Assert.True(save.Ok);
        Assert.Equal(new byte[] { 0x00, 0x01, (byte)'f', 0x00, 0x01, (byte)'d' }, save.Written);
        var oneName = Run(VmCanonicalEvent1376Slice.DemSaveGame, new byte[] { (byte)'f', 0 }, 16);
        Assert.False(oneName.Ok);
        Assert.Equal(new byte[] { 0x00, 0x01, (byte)'f' }, oneName.Written);

        var host = Run(VmCanonicalEvent1376Slice.DemSinfChanged, new byte[] { 0x03, (byte)'s', (byte)'v', (byte)'_', 1 }, 16);
        Assert.True(host.Ok);
        Assert.Equal(0x00, host.Written[0]);
        var masked = Run(VmCanonicalEvent1376Slice.DemRunNamedScript, new byte[] { (byte)'g', 0, 0x81, 9, 8, 7, 6 }, 16);
        Assert.True(masked.Ok);
        Assert.Equal(0x81, masked.Written[3]);
        Assert.Equal(3, Run(VmCanonicalEvent1376Slice.DemConvReply, new byte[] { 1, 2, 3 }, 4).Written.Length);
        Assert.Equal(2, Run(VmCanonicalEvent1376Slice.DemSetPitchLimit, new byte[] { 9, 8 }, 4).Written.Length);

        var slot = Run(VmCanonicalEvent1376Slice.DemSetSlot, new byte[] { 1, 2, 0x05, 0x81, 0x02 }, 16);
        Assert.True(slot.Ok);
        Assert.Equal(new byte[] { 1, 2, 0x00, 0x05, 0x01, 0x01 }, slot.Written);
        var missingCount = Run(VmCanonicalEvent1376Slice.DemSetSlot, new byte[] { 0x01 }, 8);
        Assert.False(missingCount.Ok);
        Assert.Equal(1, missingCount.Input);
        Assert.Empty(missingCount.Written);
        var none = Run(VmCanonicalEvent1376Slice.DemSetSlot, new byte[] { 4, 0 }, 8);
        Assert.True(none.Ok);
        Assert.Equal(new byte[] { 4, 0 }, none.Written);

        var pnum = Run(VmCanonicalEvent1376Slice.DemSetSlotPnum, new byte[] { 3, 1, 0 }, 8);
        Assert.True(pnum.Ok);
        Assert.Equal(new byte[] { 3, 1, 0 }, pnum.Written);
        var noPlayer = Run(VmCanonicalEvent1376Slice.DemSetSlotPnum, Array.Empty<byte>(), 8);
        Assert.False(noPlayer.Ok);
        Assert.Equal(0, noPlayer.Input);

        var added = Run(VmCanonicalEvent1376Slice.DemAddSlot, new byte[] { 3, 0x80, 0x01 }, 8);
        Assert.True(added.Ok);
        Assert.Equal(new byte[] { 3, 0x00, 0x80 }, added.Written);
        Assert.True(Run(VmCanonicalEvent1376Slice.DemAddSlotDefault, new byte[] { 1, 0x04 }, 8).Ok);
        var shortWeapon = Run(VmCanonicalEvent1376Slice.DemAddSlot, new byte[] { 3, 0x80 }, 8);
        Assert.False(shortWeapon.Ok);
        Assert.Equal(new byte[] { 3 }, shortWeapon.Written);
        Assert.Equal(2, shortWeapon.Input);

        var net = Run(VmCanonicalEvent1376Slice.DemNetEvent, Cat(new byte[] { (byte)'e', 0 }, Pad(14)), 32);
        Assert.True(net.Ok);
        Assert.Equal(3 + 14, net.Written.Length);
        var shortNet = Run(VmCanonicalEvent1376Slice.DemNetEvent, Cat(new byte[] { (byte)'e', 0 }, Pad(13)), 32);
        Assert.False(shortNet.Ok);
        Assert.Equal(new byte[] { 0x00, 0x01, (byte)'e' }, shortNet.Written);
    }

    static Result RunCVar(byte eventType, byte[] data, int capacity)
    {
        var output = new byte[capacity];
        var input = 0;
        var cursor = 0;
        var ok = VmCanonicalEvent1376Slice.AppendCVarChange(eventType, data, ref input, output, ref cursor);
        return new Result(ok, input, output[..cursor]);
    }

    static Result RunArgs(bool named, byte[] data, int capacity)
    {
        var output = new byte[capacity];
        var input = 0;
        var cursor = 0;
        var ok = VmCanonicalEvent1376Slice.AppendRunArgs(data, ref input, output, ref cursor, named);
        return new Result(ok, input, output[..cursor]);
    }

    static Result Run(byte eventType, byte[] data, int capacity)
    {
        var output = new byte[capacity];
        var input = 0;
        var cursor = 0;
        var ok = VmCanonicalEvent1376Slice.AppendEventPayload(eventType, data, ref input, output, ref cursor);
        return new Result(ok, input, output[..cursor]);
    }

    static byte[] Pad(int count)
    {
        var bytes = new byte[count];
        for (var i = 0; i < count; i++)
            bytes[i] = (byte)(i + 1);
        return bytes;
    }

    static byte[] Cat(byte[] head, byte[] tail)
    {
        var all = new byte[head.Length + tail.Length];
        head.CopyTo(all, 0);
        tail.CopyTo(all, head.Length);
        return all;
    }

    readonly record struct Result(bool Ok, int Input, byte[] Written);
}
