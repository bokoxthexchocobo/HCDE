namespace HCDE.Playsim;

/// <summary>
/// Texture checksum lists from <c>md5check.cpp</c> (<b>1–260</b>).
/// Chunk <b>1214</b>. Digests are lowercase hex from the little-endian MD5 in chunk <b>1209</b>.
/// A patch longer than 10164 bytes is skipped before any digest. IMGZ reads the file from the start.
/// </summary>
public sealed class VmImgzReader1214
{
    public byte[]? Bytes;
    public int Position;
    public int Seeks;
    public int ReadCalls;

    public uint Length()
    {
        return Bytes == null ? 0u : (uint)Bytes.Length;
    }

    public void SeekStart()
    {
        Seeks++;
        Position = 0;
    }

    public int Read(byte[] dest, int count)
    {
        ReadCalls++;
        if (Bytes == null)
            throw new NullReferenceException();
        int n = 0;
        while (n < count && Position < Bytes.Length)
        {
            dest[n] = Bytes[Position];
            n++;
            Position++;
        }
        return n;
    }
}

public static class VmMd5Check1214Slice
{
    public const uint PatchLengthLimit = 10164;

    internal static readonly (int Length, string Digest)[] AlphaPatches =
    {
        (18, "fb1472adb34ce1de7a40156d484dd3fc"),
        (59, "7149115092f5e8f01f84dacc120aae94"),
        (62, "7a8d11c24b51e7daff2ab0ffbe3641d9"),
        (75, "53ca06706de386ec440a501156dd4bc6"),
        (78, "299bd69e46b4ba3632e8bf5a96f1b0cb"),
        (86, "643535b5db4361db4177978152294a01"),
        (89, "773bb976dd343266bfe2fc11c1d2a2aa"),
        (100, "493dd248c2cb0d45635ef9cc995db952"),
        (105, "257a140b3533713fb59d9be493ba6803"),
        (105, "7e4b578c7217b7f0c7120d62177dd5bc"),
        (111, "fd9fec58b188284ece38e43c823abd73"),
        (119, "d6fd267eb249ce01d06fe3e2361eab64"),
        (122, "4daba1da79b0a009b46e2949a2f8c6f8"),
        (128, "ed83d41bcf4a91968ffc3ca5cad6a383"),
        (133, "ca55c89aac30bc8f7e5722520389209e"),
        (134, "2b00983c62e69cad182a5204df163701"),
        (137, "4fb1b26ac278de9ef382e8268cc7e7bb"),
        (138, "b3b69ce27c4b57912232bf718768390a"),
        (147, "ae5625169660aaf44ddffe5553be3e28"),
        (149, "e594798cfad078a553cd21ea6544abac"),
        (152, "bd30eccd4e6afe596af8823d4f8ccf40"),
        (153, "5f2111f1b44cc8cd153e6c08eec3f68c"),
        (160, "27682ca38c949072848c186f499bf027"),
        (168, "7f3d0cf78df24473ffa98ff41d8c93ce"),
        (169, "a0df55d2aee6f72c9164127f8fc9f0dd"),
        (171, "08c3d1b4bcb5643ae133938778e74c6f"),
        (171, "603355244602e830a2fa9949711a7e83"),
        (177, "6dae9b9bd34805cb066309679f9aaf81"),
        (177, "a3312ae7011faf08e7903c753e7108b0"),
        (183, "142acb37f179aff6e91b430415189c5f"),
        (185, "ce1428acd24ced861aa628b4fcefc761"),
        (194, "ab742500964a2298076a4e3ca5f8d8dc"),
        (196, "36e6dd0c675ceb94304954fb1d321f87"),
        (196, "48ef0cdb8c25e6993e38ddedb0b1a69d"),
        (239, "72384cc34d4345de824e9b775e185155"),
        (251, "f3ba8dc69906efee68e27bc37ad8b164"),
        (251, "f9138bed705dd05b7ac925751f2173c8"),
        (261, "c6325f4f79f3e36d00eced5d4a160e50"),
        (268, "319f297738ecd5b8e9c32d635c5dc781"),
        (268, "b6e71e5e2a5df85f5a1496ff322cda76"),
        (270, "5fe0a8e06182c155fb0508aae1e2c661"),
        (288, "ceb3a910c338fe4f7cdc6c3b49bcb986"),
        (290, "5c8810d3be4b27fdbb7660af04ef91ce"),
        (307, "53564f95e3bcd9f84f463cdb06d4c959"),
        (329, "943cda6f9fbc13bef59c02d38481c1b9"),
        (330, "397071348b313cbf4631e16e292a2816"),
        (334, "a7efbab516ae51271d7dece02ee7a3da"),
        (341, "ae1f343cbea6ab7892bcc25bd544b149"),
        (359, "3828fb4462f3c559d6a1d5659d736d38"),
        (397, "3a3a887506a77d812f07ddb46e13d828"),
        (399, "80a0b9b54618a94fdbd9ff907eb69f2c"),
        (412, "78cef7b3872c4a47bc47155e3fdc2f45"),
        (438, "2c08ca266a7df57d609d85b774fb589f"),
        (480, "1f84994c04f4a4941ffc183bda3d9ce3"),
        (521, "833d10c6a1b0ed1cf76ce0012628415d"),
        (521, "8658b919b52228d740c057c50038eaac"),
        (521, "c9650cf2f9564b149fbc8c1bbd13c157"),
        (542, "ddc08f004ee63eeec2fbb474393ec5ed"),
        (569, "3baa1f2b6c05ceb60e5a7ed5f8990263"),
        (569, "a1951138cfa47d4ee1033ae088f7ed04"),
        (642, "0f4320a0c53f645d2c3551eeea5ba898"),
        (642, "968c486e2f2880a3a8d5c7df87a3d8ad"),
        (642, "c41089a518b28be1db182a9d1e7fa7de"),
        (653, "7bd11e17dd6672680f5325a4f5496c98"),
        (653, "b57e64f5f98b1fea132a5d6572f24724"),
        (653, "e53b9967ef7f93f2ea069b30d6b42444"),
        (780, "0f95bd26c68d0f3180527aa8dc539d28"),
        (780, "3e0075c2e707083e83c0b897a20d6089"),
        (801, "3dd3a4bb095deffd70489b9bb6636c96"),
        (801, "44703476a6702522047fe0ff0831cc31"),
        (801, "c4a348a488d948dcc70faa6a396b6625"),
        (801, "f22f176a5e0e2d608c33e2572b28a841"),
        (855, "5385652621b2df49fedd6d970b1d582f"),
        (855, "559817e4658047519abbc58a14b2c97c"),
        (855, "6dee11141dbedff291b9393677276b54"),
        (864, "0300a526b884a27d68d22291b926c1d1"),
        (900, "0fca1f72e22661c39ba0f4e4e9a39c87"),
        (900, "700c0e4cbf0087392e026ac8ab01283d"),
        (900, "f414ff86af1611bb90043ca859eb2eda"),
        (970, "b9ebf0a5895fd2fcadb3304a8f7351b1"),
        (1020, "08d4691ac72069c5a0897650e91a34b8"),
        (1020, "3559fb14d0a30f5fae9705f67c9e2b83"),
        (1020, "afe88bbb055060e39e6e757aebd29a96"),
        (1034, "8671f2be0d2be65340b7a3b819d57933"),
        (1124, "4ec6d839eb2c10a33dfa0191b2200ede"),
        (1149, "5a0040d28c3f84ef895806a3e80d49a6"),
        (1149, "783e85a5195ba91667491002a80e03a3"),
        (1189, "295e144b6318abe47b14d1e6bcc7a7fa"),
        (1189, "78606aefd9807b51760b4e6dd9203afa"),
        (1189, "892d3a9fc35e14fd97f526c8b6ea9a3f"),
        (1218, "3bafcd6672b95d1e4fc92367ed10732d"),
        (1218, "916964ed55798ebfd288a65ffb09bb02"),
        (1218, "a3f4fca1e8050d7e5d8e64f48bc16515"),
        (1235, "7927d6dd72ff5dd3b4b0f538fe78d7b9"),
        (1235, "d62f324008e74bfea74efd160b668872"),
        (1235, "f6a0df424b064ed6f6744d5891936ab2"),
        (1717, "df6d834ad357b48feaa9e9d2b0a1f449"),
        (1856, "9a9a2cc871a0dec902c8c91e631bc9c8"),
        (1856, "c1e539d6a93a952a5752018d65d577f4"),
        (1856, "e60306eefd66bceef1c802732b9163e3"),
        (1942, "84974f010afdabd9351163175b19bb49"),
        (1942, "a4caf2b9ade6467ea0018bd78321ff50"),
        (1952, "3ee4ddc539059b533c0e31afbf07bb7f"),
        (1952, "e868f1c2bab9752a6cd819b7a7ce6487"),
        (2035, "36b4e593a7a60d100e4ec081a7d2f809"),
        (2080, "ebb5c3017a9beb76258f7d374e99a339"),
        (2092, "5461be1e0b02d00ff2204ee457fc5d7c"),
        (2247, "35f936c271a190e3f67aaea1f02dee57"),
        (2247, "3e237bb101d4e3236a5052a1bc5c80d2"),
        (2247, "58b4e240b1c5d41e6150df6e320c1349"),
        (2280, "4e1369d2d683d757445a3d78ace82e98"),
        (2302, "351c1d1cd32637a1d0b4e0168165b266"),
        (2821, "8c822e94db02cc9d6c851a189c77123e"),
        (2834, "d0099d0fc5aebcdd3994a45ff2560dbc"),
        (2865, "5c0350860fcb89ed28041d6ab0d023a8"),
        (4370, "bda605422fb9144f9671c03405e7c73b"),
        (4370, "f3862fb4f336506542e7e9a91bc88261"),
        (4483, "d05e14fbce5864b2df4b92599c48a4c5"),
        (4506, "230ee2c1a8c5baeafbea7248659da3f4"),
        (4855, "d27a510aa0285f1c74cb12c7f8ac6d25"),
        (6232, "9e7f0b2c6ca50aa8335929c959ba8c51"),
        (6858, "876c130f90700587b1a71d1aab095746"),
        (6858, "c72d6f4c3a0ff3a2d2db1082aea1c884"),
        (7444, "81e4a147ce264394fad80cfd04965d5d"),
        (7452, "aa3f72a47fe2b7c3e353ea62c75d6b42"),
        (7576, "ac2b9c1b53e91bbb0cb260665189291b"),
        (10164, "3c8908619cd5b357de701a0c6b04f408"),
    };

    internal static readonly (int Length, string Digest)[] PalImgzs =
    {
        (1728, "9ffdd749b414a1d34c74650828ad6504"),
        (64024, "bee2a3ddec08bf975353b3dc139bb956"),
    };

    public static string MakeMd5(byte[]? buffer, int length)
    {
        var md5 = new VmMd51209Slice();
        md5.Update(buffer, length);
        byte[] sum = md5.Final();
        char[] hex = new char[32];
        for (int i = 0; i < 16; i++)
        {
            hex[i * 2] = Hex(sum[i] >> 4);
            hex[i * 2 + 1] = Hex(sum[i] & 15);
        }
        return new string(hex);
    }

    public static bool CheckPatchForAlpha(byte[]? buffer, uint length)
    {
        if (length > PatchLengthLimit)
            return false;
        return MatchListed(buffer, length, AlphaPatches);
    }

    public static bool CheckImgzPalette(VmImgzReader1214 file)
    {
        return CheckImgzPalette(file, PalImgzs);
    }

    internal static bool CheckImgzPalette(VmImgzReader1214 file, (int Length, string Digest)[] table)
    {
        uint length = file.Length();
        string? digest = null;
        for (int i = 0; i < table.Length; i++)
        {
            if (table[i].Length != (int)length)
                continue;
            if (digest == null)
            {
                byte[] buffer = new byte[length];
                file.SeekStart();
                file.Read(buffer, (int)length);
                digest = MakeMd5(buffer, (int)length);
            }
            if (SameDigest(digest, table[i].Digest))
                return true;
        }
        return false;
    }

    internal static bool MatchListed(byte[]? buffer, uint length, (int Length, string Digest)[] table)
    {
        string? digest = null;
        for (int i = 0; i < table.Length; i++)
        {
            if (table[i].Length != (int)length)
                continue;
            if (digest == null)
                digest = MakeMd5(buffer, (int)length);
            if (SameDigest(digest, table[i].Digest))
                return true;
        }
        return false;
    }

    static bool SameDigest(string hex, string listed)
    {
        for (int i = 0; i < 32; i++)
        {
            if (hex[i] != listed[i])
                return false;
        }
        return true;
    }

    static char Hex(int nibble)
    {
        if (nibble < 10)
            return (char)('0' + nibble);
        return (char)('a' + (nibble - 10));
    }
}
