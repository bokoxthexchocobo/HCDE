namespace HCDE.Playsim;

/// <summary>
/// MD5 init, update, final, and transform from <c>md5.cpp</c> (<b>1–236</b>).
/// Chunk <b>1209</b>. This host is little endian, so <c>byteSwap</c> is compiled out.
/// </summary>
public sealed class VmMd51209Slice
{
    readonly uint[] State = new uint[4];
    readonly uint[] Bytes = new uint[2];
    readonly uint[] Block = new uint[16];

    public VmMd51209Slice()
    {
        Init();
    }

    public void Init()
    {
        State[0] = 0x67452301;
        State[1] = 0xefcdab89;
        State[2] = 0x98badcfe;
        State[3] = 0x10325476;
        Bytes[0] = 0;
        Bytes[1] = 0;
    }

    public void Update(byte[]? data)
    {
        Update(data, data?.Length ?? 0);
    }

    public void Update(byte[]? data, int len)
    {
        if (len < 0)
            throw new ArgumentOutOfRangeException(nameof(len));
        if (len == 0)
        {
            Absorb(ReadOnlySpan<byte>.Empty);
            return;
        }

        if (data == null)
            throw new NullReferenceException();
        Absorb(data.AsSpan(0, len));
    }

    public byte[] Final()
    {
        int count = (int)(Bytes[0] & 0x3f);
        SetByte(count, 0x80);
        int pad = 56 - 1 - count;
        if (pad < 0)
        {
            Zero(count + 1, pad + 8);
            Transform(State, Block);
            pad = 56;
            count = -1;
        }

        Zero(count + 1, pad);
        Block[14] = Bytes[0] << 3;
        Block[15] = (Bytes[1] << 3) | (Bytes[0] >> 29);
        Transform(State, Block);
        var digest = new byte[16];
        for (int i = 0; i < 4; i++)
        {
            digest[i * 4] = (byte)State[i];
            digest[i * 4 + 1] = (byte)(State[i] >> 8);
            digest[i * 4 + 2] = (byte)(State[i] >> 16);
            digest[i * 4 + 3] = (byte)(State[i] >> 24);
        }

        Array.Clear(State);
        Array.Clear(Bytes);
        Array.Clear(Block);
        return digest;
    }

    void Absorb(ReadOnlySpan<byte> data)
    {
        uint t = Bytes[0];
        Bytes[0] = t + (uint)data.Length;
        if (Bytes[0] < t)
            Bytes[1]++;
        int space = 64 - (int)(t & 0x3f);
        if (space > data.Length)
        {
            Copy(64 - space, data);
            return;
        }

        if (space < 64)
        {
            Copy(64 - space, data[..space]);
            Transform(State, Block);
            data = data[space..];
        }

        while (data.Length >= 64)
        {
            Copy(0, data[..64]);
            Transform(State, Block);
            data = data[64..];
        }

        Copy(0, data);
    }

    void Copy(int offset, ReadOnlySpan<byte> data)
    {
        for (int i = 0; i < data.Length; i++)
            SetByte(offset + i, data[i]);
    }

    void SetByte(int pos, byte value)
    {
        int shift = (pos & 3) * 8;
        Block[pos >> 2] = (Block[pos >> 2] & ~(0xFFu << shift)) | ((uint)value << shift);
    }

    void Zero(int pos, int count)
    {
        for (int i = 0; i < count; i++)
            SetByte(pos + i, 0);
    }

    static void Transform(uint[] state, uint[] input)
    {
        uint a = state[0];
        uint b = state[1];
        uint c = state[2];
        uint d = state[3];
        unchecked
        {
            a = Step(F1(b, c, d), a, input[0] + 0xd76aa478, 7, b);
            d = Step(F1(a, b, c), d, input[1] + 0xe8c7b756, 12, a);
            c = Step(F1(d, a, b), c, input[2] + 0x242070db, 17, d);
            b = Step(F1(c, d, a), b, input[3] + 0xc1bdceee, 22, c);
            a = Step(F1(b, c, d), a, input[4] + 0xf57c0faf, 7, b);
            d = Step(F1(a, b, c), d, input[5] + 0x4787c62a, 12, a);
            c = Step(F1(d, a, b), c, input[6] + 0xa8304613, 17, d);
            b = Step(F1(c, d, a), b, input[7] + 0xfd469501, 22, c);
            a = Step(F1(b, c, d), a, input[8] + 0x698098d8, 7, b);
            d = Step(F1(a, b, c), d, input[9] + 0x8b44f7af, 12, a);
            c = Step(F1(d, a, b), c, input[10] + 0xffff5bb1, 17, d);
            b = Step(F1(c, d, a), b, input[11] + 0x895cd7be, 22, c);
            a = Step(F1(b, c, d), a, input[12] + 0x6b901122, 7, b);
            d = Step(F1(a, b, c), d, input[13] + 0xfd987193, 12, a);
            c = Step(F1(d, a, b), c, input[14] + 0xa679438e, 17, d);
            b = Step(F1(c, d, a), b, input[15] + 0x49b40821, 22, c);

            a = Step(F2(b, c, d), a, input[1] + 0xf61e2562, 5, b);
            d = Step(F2(a, b, c), d, input[6] + 0xc040b340, 9, a);
            c = Step(F2(d, a, b), c, input[11] + 0x265e5a51, 14, d);
            b = Step(F2(c, d, a), b, input[0] + 0xe9b6c7aa, 20, c);
            a = Step(F2(b, c, d), a, input[5] + 0xd62f105d, 5, b);
            d = Step(F2(a, b, c), d, input[10] + 0x02441453, 9, a);
            c = Step(F2(d, a, b), c, input[15] + 0xd8a1e681, 14, d);
            b = Step(F2(c, d, a), b, input[4] + 0xe7d3fbc8, 20, c);
            a = Step(F2(b, c, d), a, input[9] + 0x21e1cde6, 5, b);
            d = Step(F2(a, b, c), d, input[14] + 0xc33707d6, 9, a);
            c = Step(F2(d, a, b), c, input[3] + 0xf4d50d87, 14, d);
            b = Step(F2(c, d, a), b, input[8] + 0x455a14ed, 20, c);
            a = Step(F2(b, c, d), a, input[13] + 0xa9e3e905, 5, b);
            d = Step(F2(a, b, c), d, input[2] + 0xfcefa3f8, 9, a);
            c = Step(F2(d, a, b), c, input[7] + 0x676f02d9, 14, d);
            b = Step(F2(c, d, a), b, input[12] + 0x8d2a4c8a, 20, c);

            a = Step(F3(b, c, d), a, input[5] + 0xfffa3942, 4, b);
            d = Step(F3(a, b, c), d, input[8] + 0x8771f681, 11, a);
            c = Step(F3(d, a, b), c, input[11] + 0x6d9d6122, 16, d);
            b = Step(F3(c, d, a), b, input[14] + 0xfde5380c, 23, c);
            a = Step(F3(b, c, d), a, input[1] + 0xa4beea44, 4, b);
            d = Step(F3(a, b, c), d, input[4] + 0x4bdecfa9, 11, a);
            c = Step(F3(d, a, b), c, input[7] + 0xf6bb4b60, 16, d);
            b = Step(F3(c, d, a), b, input[10] + 0xbebfbc70, 23, c);
            a = Step(F3(b, c, d), a, input[13] + 0x289b7ec6, 4, b);
            d = Step(F3(a, b, c), d, input[0] + 0xeaa127fa, 11, a);
            c = Step(F3(d, a, b), c, input[3] + 0xd4ef3085, 16, d);
            b = Step(F3(c, d, a), b, input[6] + 0x04881d05, 23, c);
            a = Step(F3(b, c, d), a, input[9] + 0xd9d4d039, 4, b);
            d = Step(F3(a, b, c), d, input[12] + 0xe6db99e5, 11, a);
            c = Step(F3(d, a, b), c, input[15] + 0x1fa27cf8, 16, d);
            b = Step(F3(c, d, a), b, input[2] + 0xc4ac5665, 23, c);

            a = Step(F4(b, c, d), a, input[0] + 0xf4292244, 6, b);
            d = Step(F4(a, b, c), d, input[7] + 0x432aff97, 10, a);
            c = Step(F4(d, a, b), c, input[14] + 0xab9423a7, 15, d);
            b = Step(F4(c, d, a), b, input[5] + 0xfc93a039, 21, c);
            a = Step(F4(b, c, d), a, input[12] + 0x655b59c3, 6, b);
            d = Step(F4(a, b, c), d, input[3] + 0x8f0ccc92, 10, a);
            c = Step(F4(d, a, b), c, input[10] + 0xffeff47d, 15, d);
            b = Step(F4(c, d, a), b, input[1] + 0x85845dd1, 21, c);
            a = Step(F4(b, c, d), a, input[8] + 0x6fa87e4f, 6, b);
            d = Step(F4(a, b, c), d, input[15] + 0xfe2ce6e0, 10, a);
            c = Step(F4(d, a, b), c, input[6] + 0xa3014314, 15, d);
            b = Step(F4(c, d, a), b, input[13] + 0x4e0811a1, 21, c);
            a = Step(F4(b, c, d), a, input[4] + 0xf7537e82, 6, b);
            d = Step(F4(a, b, c), d, input[11] + 0xbd3af235, 10, a);
            c = Step(F4(d, a, b), c, input[2] + 0x2ad7d2bb, 15, d);
            b = Step(F4(c, d, a), b, input[9] + 0xeb86d391, 21, c);
        }

        unchecked
        {
            state[0] += a;
            state[1] += b;
            state[2] += c;
            state[3] += d;
        }
    }

    static uint F1(uint x, uint y, uint z) => z ^ (x & (y ^ z));
    static uint F2(uint x, uint y, uint z) => F1(z, x, y);
    static uint F3(uint x, uint y, uint z) => x ^ y ^ z;
    static uint F4(uint x, uint y, uint z) => y ^ (x | ~z);

    static uint Step(uint f, uint w, uint input, int s, uint x)
    {
        unchecked
        {
            w += f + input;
            return ((w << s) | (w >> (32 - s))) + x;
        }
    }
}
