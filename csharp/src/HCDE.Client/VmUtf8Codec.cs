namespace HCDE.Client;

/// <summary>Native <c>utf8.cpp</c> <c>utf8_encode</c> / <c>utf8_decode</c>.</summary>
public static class VmUtf8Codec
{
    public static bool TryEncode(int codepoint, Span<byte> buffer, out int size)
    {
        size = 0;
        if (codepoint < 0 || codepoint > 0x10FFFF) return false;
        if (codepoint < 0x80)
        {
            buffer[0] = (byte)codepoint;
            size = 1;
        }
        else if (codepoint < 0x800)
        {
            buffer[0] = (byte)(0xC0 + ((codepoint & 0x7C0) >> 6));
            buffer[1] = (byte)(0x80 + (codepoint & 0x03F));
            size = 2;
        }
        else if (codepoint < 0x10000)
        {
            buffer[0] = (byte)(0xE0 + ((codepoint & 0xF000) >> 12));
            buffer[1] = (byte)(0x80 + ((codepoint & 0x0FC0) >> 6));
            buffer[2] = (byte)(0x80 + (codepoint & 0x003F));
            size = 3;
        }
        else
        {
            buffer[0] = (byte)(0xF0 + ((codepoint & 0x1C0000) >> 18));
            buffer[1] = (byte)(0x80 + ((codepoint & 0x03F000) >> 12));
            buffer[2] = (byte)(0x80 + ((codepoint & 0x000FC0) >> 6));
            buffer[3] = (byte)(0x80 + (codepoint & 0x00003F));
            size = 4;
        }

        return true;
    }

    public static bool TryDecode(ReadOnlySpan<byte> src, out int codepoint, out int size)
    {
        codepoint = 0;
        size = 0;
        if (src.Length == 0) return false;
        var c = src[0];
        size = 1;
        if ((c & 0x80) == 0)
        {
            codepoint = c;
            return true;
        }

        if (src.Length < 2 || src[1] < 0x80 || src[1] >= 0xC0) return false;
        var c1 = src[1] & 0x3F;
        if ((c & 0xE0) == 0xC0)
        {
            var r = ((c & 0x1F) << 6) | c1;
            if (r < 128) return false;
            codepoint = r;
            size = 2;
            return true;
        }

        if (src.Length < 3 || src[2] < 0x80 || src[2] >= 0xC0) return false;
        var c2 = src[2] & 0x3F;
        if ((c & 0xF0) == 0xE0)
        {
            var r = ((c & 0x0F) << 12) | (c1 << 6) | c2;
            if (r < 2048 || (r >= 0xD800 && r <= 0xDFFF)) return false;
            codepoint = r;
            size = 3;
            return true;
        }

        if (src.Length < 4 || src[3] < 0x80 || src[3] >= 0xC0) return false;
        var c3 = src[3] & 0x3F;
        if ((c & 0xF8) != 0xF0) return false;
        var r4 = ((c & 0x07) << 18) | (c1 << 12) | (c2 << 6) | c3;
        if (r4 < 0x10000 || r4 > 0x10FFFF) return false;
        codepoint = r4;
        size = 4;
        return true;
    }
}
