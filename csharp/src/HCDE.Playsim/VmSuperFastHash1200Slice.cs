namespace HCDE.Playsim;

/// <summary>
/// Paul Hsieh SuperFastHash from <c>superfasthash.cpp</c> (<b>1–153</b>).
/// Chunk <b>1200</b>. Sixteen-bit groups are little-endian unsigned bytes.
/// A leftover byte is a signed char, then folded with C-locale tolower when insensitive.
/// </summary>
public static class VmSuperFastHash1200Slice
{
    public static uint Hash(byte[]? data, int length) => Mix(data, length, false);

    public static uint HashI(byte[]? data, int length) => Mix(data, length, true);

    static uint Mix(byte[]? data, int length, bool fold)
    {
        if (data == null || (fold ? length <= 0 : length == 0))
            return 0;

        uint len = (uint)length;
        uint rem = len & 3;
        len >>= 2;
        uint hash = 0;
        int index = 0;
        for (; len > 0; len--)
        {
            hash += Get16(data, index, fold);
            uint tmp = (Get16(data, index + 2, fold) << 11) ^ hash;
            hash = (hash << 16) ^ tmp;
            index += 4;
            hash += hash >> 11;
        }

        switch (rem)
        {
            case 3:
                hash += Get16(data, index, fold);
                hash ^= hash << 16;
                hash ^= (uint)(Signed(data[index + 2], fold) << 18);
                hash += hash >> 11;
                break;
            case 2:
                hash += Get16(data, index, fold);
                hash ^= hash << 11;
                hash += hash >> 17;
                break;
            case 1:
                hash += (uint)Signed(data[index], fold);
                hash ^= hash << 10;
                hash += hash >> 1;
                break;
        }

        hash ^= hash << 3;
        hash += hash >> 5;
        hash ^= hash << 4;
        hash += hash >> 17;
        hash ^= hash << 25;
        hash += hash >> 6;
        return hash;
    }

    static uint Get16(byte[] data, int index, bool fold)
    {
        int low = fold ? Lower(data[index]) : data[index];
        int high = fold ? Lower(data[index + 1]) : data[index + 1];
        return (uint)((high << 8) + low);
    }

    static int Lower(byte value) => value is >= (byte)'A' and <= (byte)'Z' ? value + 32 : value;

    static int Signed(byte value, bool fold)
    {
        int text = (sbyte)value;
        if (!fold || text == -1)
            return text;
        if (text >= 'A' && text <= 'Z')
            return text + 32;
        return text;
    }
}
