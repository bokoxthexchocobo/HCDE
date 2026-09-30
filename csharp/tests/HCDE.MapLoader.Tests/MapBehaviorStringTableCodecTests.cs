using System.Buffers.Binary;
using System.Text;

namespace HCDE.MapLoader.Tests;

public class MapBehaviorStringTableCodecTests
{
    [Fact]
    public void OldFormat_ReadsEscapedClassNames()
    {
        var lump = BuildOldLump(["DoomImp", "Zombieman"]);
        Assert.True(MapBehaviorStringTableCodec.TryRead(lump, MapBehaviorFormat.AcsOld, 24, out var strings, out var error));
        Assert.Null(error);
        Assert.Equal(["DoomImp", "Zombieman"], strings);
    }

    [Fact]
    public void OldFormat_DecodesStrBinEscapes()
    {
        var lump = BuildOldLump(["Doom\\nImp"]);
        Assert.True(MapBehaviorStringTableCodec.TryRead(lump, MapBehaviorFormat.AcsOld, 24, out var strings, out _));
        Assert.Equal(["Doom\nImp"], strings);
    }

    [Fact]
    public void EnhancedStrlChunk_ReadsStrings()
    {
        var lump = BuildEnhancedStrlLump(["DoomImp"]);
        Assert.True(MapBehaviorStringTableCodec.TryRead(lump, MapBehaviorFormat.AcsEnhanced, 24, out var strings, out var error));
        Assert.Null(error);
        Assert.Equal(["DoomImp"], strings);
    }

    [Fact]
    public void EnhancedStreChunk_DecryptsLikeNativeUnencryptStrings()
    {
        var lump = BuildEnhancedStreLump(["DoomImp", "Zombieman"]);
        Assert.True(MapBehaviorStringTableCodec.TryRead(lump, MapBehaviorFormat.AcsEnhanced, 24, out var strings, out var error));
        Assert.Null(error);
        Assert.Equal(["DoomImp", "Zombieman"], strings);
    }

    private static byte[] BuildOldLump(string[] names)
    {
        const int directory = 24;
        const int scriptCount = 1;
        var stringTableStart = directory + 4 + scriptCount * 12;
        var code = new byte[] { 1, 0, 0, 0 };
        var blob = new List<byte>();
        foreach (var name in names)
            blob.AddRange(Encoding.ASCII.GetBytes(name + "\0"));

        var stringDataStart = stringTableStart + 4 + names.Length * 4;
        var codeStart = stringDataStart + blob.Count;
        var lump = new byte[codeStart + code.Length];
        lump[0] = (byte)'A';
        lump[1] = (byte)'C';
        lump[2] = (byte)'S';
        BinaryPrimitives.WriteUInt32LittleEndian(lump.AsSpan(4), directory);
        BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(directory), scriptCount);
        BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(directory + 4), 1001);
        BinaryPrimitives.WriteUInt32LittleEndian(lump.AsSpan(directory + 8), (uint)codeStart);
        BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(directory + 12), 0);
        BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(stringTableStart), names.Length);
        var cursor = stringTableStart + 4;
        var offset = stringDataStart;
        for (var i = 0; i < names.Length; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(cursor), offset);
            cursor += 4;
            offset += Encoding.ASCII.GetByteCount(names[i]) + 1;
        }

        blob.CopyTo(lump.AsSpan(stringDataStart));
        code.CopyTo(lump.AsSpan(codeStart));
        return lump;
    }

    private static byte[] BuildEnhancedStreLump(string[] names)
    {
        var payload = BuildEnhancedStrlPayload(names);
        ScrambleStrePayload(payload);
        var lump = new byte[32 + payload.Length];
        lump[0] = (byte)'A';
        lump[1] = (byte)'C';
        lump[2] = (byte)'S';
        lump[3] = (byte)'E';
        BinaryPrimitives.WriteUInt32LittleEndian(lump.AsSpan(4), 24);
        BinaryPrimitives.WriteUInt32LittleEndian(lump.AsSpan(24), 0x45545253);
        BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(28), payload.Length);
        payload.CopyTo(lump.AsSpan(32));
        return lump;
    }

    /// <summary>Native <c>FBehavior::UnencryptStrings</c> forward scramble for fixtures.</summary>
    private static void ScrambleStrePayload(Span<byte> payload)
    {
        var count = BinaryPrimitives.ReadInt32LittleEndian(payload[4..]);
        for (var strnum = 0; strnum < count; strnum++)
        {
            var ofs = BinaryPrimitives.ReadInt32LittleEndian(payload[(12 + strnum * 4)..]);
            var p = (byte)(ofs * 157135);
            var i = 0;
            while (ofs + i < payload.Length)
            {
                var plain = payload[ofs + i];
                payload[ofs + i] = (byte)(plain ^ (byte)(p + (i >> 1)));
                i++;
                if (plain == 0)
                    break;
            }
        }
    }

    private static byte[] BuildEnhancedStrlLump(string[] names)
    {
        var payload = BuildEnhancedStrlPayload(names);
        var lump = new byte[32 + payload.Length];
        lump[0] = (byte)'A';
        lump[1] = (byte)'C';
        lump[2] = (byte)'S';
        lump[3] = (byte)'E';
        BinaryPrimitives.WriteUInt32LittleEndian(lump.AsSpan(4), 24);
        BinaryPrimitives.WriteUInt32LittleEndian(lump.AsSpan(24), 0x4C525453);
        BinaryPrimitives.WriteInt32LittleEndian(lump.AsSpan(28), payload.Length);
        payload.CopyTo(lump.AsSpan(32));
        return lump;
    }

    private static byte[] BuildEnhancedStrlPayload(string[] names)
    {
        var blob = new List<byte>();
        var stringDataStart = 12 + names.Length * 4;
        var cursor = stringDataStart;
        var offsets = new int[names.Length];
        for (var i = 0; i < names.Length; i++)
        {
            offsets[i] = cursor;
            blob.AddRange(Encoding.ASCII.GetBytes(names[i] + "\0"));
            cursor += Encoding.ASCII.GetByteCount(names[i]) + 1;
        }

        var payload = new byte[stringDataStart + blob.Count];
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(4), names.Length);
        for (var i = 0; i < names.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(12 + i * 4), offsets[i]);
        blob.CopyTo(payload.AsSpan(stringDataStart));
        return payload;
    }
}
