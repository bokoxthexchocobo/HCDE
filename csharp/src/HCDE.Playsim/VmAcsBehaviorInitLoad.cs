namespace HCDE.Playsim;

public enum AcsFormat
{
    Unknown,
    Old,
    Enhanced,
    LittleEnhanced,
}

/// <summary><c>FBehavior::Init</c> format routing and map-var bootstrap (<c>p_acs.cpp</c> 2329–2497).</summary>
public static class VmAcsBehaviorInitLoad
{
    public static AcsFormat ParseAcsFormatByte(byte fourthByte) =>
        fourthByte switch
        {
            0 => AcsFormat.Old,
            (byte)'E' => AcsFormat.Enhanced,
            (byte)'e' => AcsFormat.LittleEnhanced,
            _ => AcsFormat.Unknown,
        };

    public static string ResolveModuleName(bool fromFile, string fileShortName) =>
        fromFile ? fileShortName : "BEHAVIOR";

    public static bool ShouldUpgradeOldEnhanced(uint dirofs, uint pretag, out AcsFormat upgraded)
    {
        upgraded = AcsFormat.Old;
        if (dirofs < 6u * 4u)
            return false;
        if (pretag == AcsChunkIds.AcSe)
        {
            upgraded = AcsFormat.LittleEnhanced;
            return true;
        }

        if (pretag == AcsChunkIds.AcSE)
        {
            upgraded = AcsFormat.Enhanced;
            return true;
        }

        return false;
    }

    public static bool ShouldLocalizeHexenHack(bool hexenHack, bool hexenGame, int lumpNum, int realLumpNum, string fileName) =>
        hexenHack && hexenGame && lumpNum == -1 && realLumpNum > 0
        && (string.Equals(fileName, "HEXEN.WAD", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "HEXDD.WAD", StringComparison.OrdinalIgnoreCase));

    public static int LoadMiniChunkVars(int[] mapVarStore, int firstVar, int[] values)
    {
        for (var i = 0; i < values.Length; i++)
            mapVarStore[firstVar + i] = values[i];
        return values.Length;
    }

    public static ScriptFunctionInFile[] LoadFunctionsFromChunkNative(ReadOnlySpan<ScriptFunctionInFile> funcs)
    {
        var loaded = new ScriptFunctionInFile[funcs.Length];
        for (var i = 0; i < funcs.Length; i++)
        {
            loaded[i] = new ScriptFunctionInFile
            {
                ArgCount = funcs[i].ArgCount,
                HasReturnValue = funcs[i].HasReturnValue,
                ImportNum = funcs[i].ImportNum,
                LocalCount = funcs[i].LocalCount,
                Address = funcs[i].Address,
            };
        }

        return loaded;
    }

    public static int ApplyFunctionArrayChunkNative(
        int funcNum,
        int numFunctions,
        int argCount,
        int localCount,
        Func<int, int, int, int> parseLocalArrayChunk)
    {
        if (funcNum >= numFunctions)
            return localCount;
        return parseLocalArrayChunk(funcNum, argCount, localCount) - argCount;
    }

    public struct ScriptFunctionInFile
    {
        public int ArgCount;
        public bool HasReturnValue;
        public int ImportNum;
        public int LocalCount;
        public int Address;
    }
}

public static class AcsChunkIds
{
    public const uint AcSe = 0x65534341u; // 'ACS' little-endian 'e'
    public const uint AcSE = 0x45534341u;
}
