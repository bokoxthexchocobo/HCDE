using System.Security.Cryptography;

namespace HCDE.Playsim;

/// <summary>
/// Nerve map renames, Mac Hexen extra-lump clearing, and Strife teaser voice
/// namespaces from d_main.cpp 3096-3281. DoomButtons starts at line 3283.
/// </summary>
public static class VmDMain1418Slice
{
    public const int BandStart = 3096;
    public const int BandEnd = 3281;
    public const int ButtonsStart = 3283;
    public const int Doom = 1;
    public const int Hexen = 4;
    public const int Strife = 8;
    public const int Shareware = 2;
    public const int Global = 0;
    public const int StrifeVoices = 9;
    public const int ExtraLumps = 299;
    public const int BetaTail = 12;
    public const uint Cwil = 'C' | ((uint)'W' << 8) | ((uint)'I' << 16) | ((uint)'L' << 24);
    public const uint Map0 = 'M' | ((uint)'A' << 8) | ((uint)'P' << 16) | ((uint)'0' << 24);

    public static readonly long[] NerveSizes = { 3819855, 3821966, 3821885, 4003409 };
    public static readonly byte[][] NerveDigests =
    {
        new byte[] { 0x96, 0x7d, 0x5a, 0xe2, 0x3d, 0xaf, 0x45, 0x19, 0x62, 0x12, 0xae, 0x1b, 0x60, 0x5d, 0xa3, 0xb0 },
        new byte[] { 0x42, 0x14, 0xc4, 0x76, 0x51, 0xb6, 0x3e, 0xe2, 0x25, 0x7b, 0x1c, 0x24, 0x90, 0xa5, 0x18, 0xc9 },
        new byte[] { 0x35, 0x44, 0xe1, 0x90, 0x30, 0x91, 0xc5, 0x0b, 0xa5, 0x00, 0x49, 0xdb, 0x74, 0xcd, 0x7d, 0x25 },
        new byte[] { 0x91, 0x43, 0xb3, 0x92, 0x39, 0x2a, 0x7a, 0xc8, 0x70, 0xc2, 0xca, 0x36, 0xac, 0x65, 0xaf, 0x45 },
    };
    public static readonly long DemoSize = 13596228;
    public static readonly long BetaSize = 13749984;
    public static readonly long FullSize = 21078584;
    public static readonly byte[] DemoDigest =
    {
        0x92, 0x5f, 0x9f, 0x50, 0x00, 0xe1, 0x7d, 0xc8, 0x4b, 0x0a, 0x6a, 0x3b, 0xed, 0x3a, 0x6f, 0x31,
    };
    public static readonly byte[] BetaDigest =
    {
        0x2a, 0xf1, 0xb2, 0x7c, 0xd1, 0x1f, 0xb1, 0x59, 0xe6, 0x08, 0x47, 0x2a, 0x1b, 0x53, 0xe4, 0x0e,
    };
    public static readonly byte[] FullDigest =
    {
        0xb6, 0x81, 0x40, 0xa7, 0x96, 0xf6, 0xfd, 0x7f, 0x3a, 0x5d, 0x32, 0x26, 0xa3, 0x2b, 0x93, 0xbe,
    };

    public sealed class Lump
    {
        public Lump(int space, int container, string name)
        {
            Space = space;
            Container = container;
            int n = name.Length < 8 ? name.Length : 8;
            for (int i = 0; i < n; i++)
                Name[i] = (byte)name[i];
        }

        public byte[] Name = new byte[9];
        public int Space;
        public int Container;
    }

    public sealed class Wad
    {
        public byte[]? Data;
        public int First;
        public int Last = -1;
        public int Cursor;
        public int Seeks;
        public int Hashed;
    }

    public sealed class State
    {
        public int GameType = Doom;
        public int Flags;
        public int Iwad;
        public List<Wad> Wads = new();
        public List<Lump> Lumps = new();
        public long[] NerveSizes = (long[])VmDMain1418Slice.NerveSizes.Clone();
        public byte[][] NerveDigests = CopyRows(VmDMain1418Slice.NerveDigests);
        public long DemoSize = VmDMain1418Slice.DemoSize;
        public long BetaSize = VmDMain1418Slice.BetaSize;
        public long FullSize = VmDMain1418Slice.FullSize;
        public byte[] DemoDigest = (byte[])VmDMain1418Slice.DemoDigest.Clone();
        public byte[] BetaDigest = (byte[])VmDMain1418Slice.BetaDigest.Clone();
        public byte[] FullDigest = (byte[])VmDMain1418Slice.FullDigest.Clone();
    }

    public static void RenameNerve(State state)
    {
        if (state.GameType != Doom)
            return;
        int w = state.Iwad;
        int matched = -1;
        while (++w < state.Wads.Count)
        {
            Wad wad = state.Wads[w];
            if (wad.Data == null)
                continue;
            int sizeIndex = -1;
            long length = wad.Data.LongLength;
            for (int i = 0; i < state.NerveSizes.Length; i++)
            {
                if (length == state.NerveSizes[i])
                    sizeIndex = i;
            }
            if (sizeIndex < 0)
                continue;
            byte[] sum = Digest(wad);
            wad.Hashed++;
            if (Same(state.NerveDigests[sizeIndex], sum))
            {
                matched = w;
                break;
            }
        }
        if (matched < 0)
            return;
        Wad hit = state.Wads[matched];
        for (int i = hit.First; i <= hit.Last; i++)
        {
            Lump lump = state.Lumps[i];
            uint id = Dword(lump.Name);
            if (id == Cwil)
                lump.Name[0] = (byte)'N';
            else if (id == Map0)
            {
                lump.Name[6] = lump.Name[4];
                lump.Name[5] = (byte)'0';
                lump.Name[4] = (byte)'L';
                lump.Name[0] = (byte)'L';
                lump.Name[1] = (byte)'E';
                lump.Name[2] = (byte)'V';
                lump.Name[3] = (byte)'E';
            }
        }
    }

    public static void FixMacHexen(State state)
    {
        if (state.GameType != Hexen)
            return;
        Wad wad = state.Wads[state.Iwad];
        if (wad.Data == null)
            throw new NullReferenceException();
        long size = wad.Data.LongLength;
        if (size != state.DemoSize && size != state.BetaSize && size != state.FullSize)
            return;
        byte[] sum = Digest(wad);
        wad.Hashed++;
        bool beta = Same(state.BetaDigest, sum);
        if (!beta && !Same(state.DemoDigest, sum) && !Same(state.FullDigest, sum))
            return;
        int last = wad.Last - (beta ? BetaTail : 0);
        for (int i = last - ExtraLumps + 1; i <= last; i++)
            state.Lumps[i].Name[0] = 0;
    }

    public static void FindStrifeTeaserVoices(State state)
    {
        if (state.GameType == Strife && (state.Flags & Shareware) != 0)
        {
            for (int i = 0; i < state.Lumps.Count; i++)
            {
                Lump lump = state.Lumps[i];
                if (lump.Space == Global)
                {
                    if (lump.Container == state.Iwad)
                    {
                        byte[] name = lump.Name;
                        if (name[0] == (byte)'V' && name[1] == (byte)'O' && name[2] == (byte)'C')
                        {
                            int j;
                            for (j = 3; j < 8; j++)
                            {
                                if (name[j] != 0 && (name[j] < (byte)'0' || name[j] > (byte)'9'))
                                    break;
                            }
                            if (j == 8)
                                lump.Space = StrifeVoices;
                        }
                    }
                }
            }
        }
    }

    public static byte[] Digest(Wad wad)
    {
        if (wad.Data == null)
            throw new NullReferenceException();
        byte[] data = wad.Data;
        wad.Seeks++;
        wad.Cursor = 0;
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
        int len = data.Length;
        while (len > 0)
        {
            int ask = len < 8192 ? len : 8192;
            len -= ask;
            int n = 0;
            if (wad.Cursor < data.Length)
            {
                int have = data.Length - wad.Cursor;
                n = have < ask ? have : ask;
            }
            if (n > 0)
            {
                hash.AppendData(data.AsSpan(wad.Cursor, n));
                wad.Cursor += n;
            }
        }
        return hash.GetHashAndReset();
    }

    public static string Text(byte[] name)
    {
        int n = 0;
        while (n < name.Length && name[n] != 0)
            n++;
        return System.Text.Encoding.ASCII.GetString(name, 0, n);
    }

    static uint Dword(byte[] name) =>
        name[0] | ((uint)name[1] << 8) | ((uint)name[2] << 16) | ((uint)name[3] << 24);

    static bool Same(byte[] left, byte[] right)
    {
        if (left.Length != 16 || right.Length != 16)
            return false;
        for (int i = 0; i < 16; i++)
        {
            if (left[i] != right[i])
                return false;
        }
        return true;
    }

    static byte[][] CopyRows(byte[][] rows)
    {
        var copy = new byte[rows.Length][];
        for (int i = 0; i < rows.Length; i++)
            copy[i] = (byte[])rows[i].Clone();
        return copy;
    }
}
