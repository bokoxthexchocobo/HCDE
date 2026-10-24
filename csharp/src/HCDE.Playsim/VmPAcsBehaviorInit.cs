namespace HCDE.Playsim;

/// <summary>Whole <c>FBehavior::Init</c> and destructor tail from <c>p_acs.cpp</c> (2281–2792).</summary>
public static class VmPAcsBehaviorInit
{
    public const int NumMapVars = 128;
    public const int LibraryIdShift = 16;

    public sealed class BehaviorModule
    {
        public object? Level { get; set; }
        public int LumpNum { get; set; }
        public AcsFormat Format { get; set; } = AcsFormat.Unknown;
        public int LibraryId { get; set; }
        public string ModuleName { get; set; } = "";
        public byte[]? Data { get; set; }
        public int DataSize { get; set; }
        public int StringTable { get; set; }
        public bool ShouldLocalize { get; set; }
        public int NumScripts { get; set; }
        public int NumFunctions { get; set; }
        public int NumArrays { get; set; }
        public int NumTotalArrays { get; set; }
        public int[] MapVarStore { get; } = new int[NumMapVars];
        public int[][]? MapVars { get; set; }
        public VmAcsBehaviorInitLoad.ScriptFunctionInFile[]? Functions { get; set; }
        public List<object> Imports { get; } = new();
        public List<int> JumpPoints { get; } = new();
        public bool HasScripts { get; set; }
        public bool HasArrays { get; set; }
        public bool HasArrayStore { get; set; }
        public bool HasFunctions { get; set; }
        public bool HasFunctionProfile { get; set; }
        public bool HasData { get; set; }
    }

    public sealed class InitContext
    {
        public required BehaviorModule Module { get; init; }
        public bool LevelNull { get; init; }
        public int LumpNum { get; init; }
        public byte[]? ReaderBuffer { get; init; }
        public int Len { get; init; }
        public int RealLumpNum { get; init; }
        public bool HexenHack { get; init; }
        public bool HexenGame { get; init; }
        public Func<int, int>? FileLength { get; init; }
        public Func<int, byte[]>? ReadLump { get; init; }
        public Func<int, string>? FileShortName { get; init; }
        public Func<int, string>? ResourceFileName { get; init; }
        public Func<object, int>? PushStaticModule { get; init; }
        public Action? LoadScriptsDirectory { get; init; }
        public Action? UnencryptStrings { get; init; }
        public Func<uint, byte[]?, byte[]?>? FindChunk { get; init; }
        public Func<byte[]?, byte[]?>? NextChunk { get; init; }
        public Action<byte[], byte[]?, bool>? UnescapeStringTable { get; init; }
        public Func<int, string?>? LookupString { get; init; }
        public Func<string, int>? AddGlobalString { get; init; }
        public Func<string, int>? CheckLibraryLump { get; init; }
        public Func<int, object?>? LoadModule { get; init; }
        public Action<string>? LogMissingLibrary { get; init; }
        public Action<string, object[]>? LogImportMismatch { get; init; }
        public Func<int, int, int, int>? ParseLocalArrayChunk { get; init; }
    }

    public static bool RejectNullLevel(bool levelNull) => levelNull;

    public static int PickLumpLength(InitContext ctx, bool readerNull)
    {
        if (readerNull && ctx.FileLength != null)
            return ctx.FileLength(ctx.LumpNum);
        return ctx.Len;
    }

    public static bool Init(InitContext ctx)
    {
        var mod = ctx.Module;
        if (RejectNullLevel(ctx.LevelNull))
            return false;

        mod.Level = ctx.LevelNull ? null : mod.Level;
        mod.LumpNum = ctx.LumpNum;

        var len = PickLumpLength(ctx, ctx.ReaderBuffer == null);
        if (VmAcsBehaviorVarSerialize.InitRejectsTinyLumpNative(len))
            return false;

        var objectBytes = ctx.ReaderBuffer ?? ctx.ReadLump?.Invoke(ctx.LumpNum);
        if (objectBytes == null || objectBytes.Length < 4)
            return false;

        if (!VmAcsBehaviorVarSerialize.InitReadsAcsMagicNative(objectBytes))
            return false;

        var format = VmAcsBehaviorInitLoad.ParseAcsFormatByte(objectBytes[3]);
        if (format == AcsFormat.Unknown)
            return false;

        mod.Format = format;
        mod.LibraryId = (ctx.PushStaticModule?.Invoke(mod) ?? 0) << LibraryIdShift;
        mod.ModuleName = VmAcsBehaviorInitLoad.ResolveModuleName(ctx.ReaderBuffer == null,
            ctx.FileShortName?.Invoke(ctx.LumpNum) ?? "LUMP");
        mod.Data = objectBytes;
        mod.DataSize = len;
        mod.HasData = true;

        if (format == AcsFormat.Old)
            ApplyOldFormatHeader(mod, objectBytes, len);
        else
            mod.StringTable = 0;

        ctx.LoadScriptsDirectory?.Invoke();

        if (format == AcsFormat.Old)
            LoadOldStringTable(ctx, mod, objectBytes);
        else
            LoadEnhancedStringTable(ctx, mod);

        if (format == AcsFormat.Old)
            BindOldMapVarDefaults(mod);
        else
            LoadEnhancedChunks(ctx, mod);

        FinishInit(ctx, mod);
        return true;
    }

    static void ApplyOldFormatHeader(BehaviorModule mod, byte[] objectBytes, int len)
    {
        var dirofs = BitConverter.ToUInt32(objectBytes, 4);
        if (BitConverter.IsLittleEndian == false)
            dirofs = BinaryPrimitivesReverse(objectBytes, 4);
        var pretag = dirofs >= 6u * 4u ? ReadUInt32At(objectBytes, (int)dirofs - 4) : 0u;
        if (VmAcsBehaviorInitLoad.ShouldUpgradeOldEnhanced(dirofs, pretag, out var upgraded))
        {
            mod.Format = upgraded;
            mod.DataSize = (int)ReadUInt32At(objectBytes, 4) - 8;
        }

        mod.ShouldLocalize = false;
    }

    static void LoadOldStringTable(InitContext ctx, BehaviorModule mod, byte[] objectBytes)
    {
        var stringTable = (int)ReadUInt32At(objectBytes, 4);
        stringTable += (int)ReadUInt32At(objectBytes, stringTable) * 12 + 4;
        mod.StringTable = stringTable;
        ctx.UnescapeStringTable?.Invoke(objectBytes.AsSpan(stringTable).ToArray(), objectBytes, false);
        if (VmAcsBehaviorInitLoad.ShouldLocalizeHexenHack(ctx.HexenHack, ctx.HexenGame, ctx.LumpNum, ctx.RealLumpNum,
                ctx.ResourceFileName?.Invoke(ctx.RealLumpNum) ?? ""))
            mod.ShouldLocalize = true;
    }

    static void LoadEnhancedStringTable(InitContext ctx, BehaviorModule mod)
    {
        ctx.UnencryptStrings?.Invoke();
        var strings = ctx.FindChunk?.Invoke(AcsFourCc("S", "T", "R", "L"), mod.Data);
        if (strings != null)
        {
            mod.StringTable = strings.Length >= 8 ? BitConverter.ToInt32(strings, 0) + 8 : 0;
            ctx.UnescapeStringTable?.Invoke(strings.AsSpan(8).ToArray(), null, true);
        }
        else
            mod.StringTable = 0;
    }

    static void BindOldMapVarDefaults(BehaviorModule mod)
    {
        mod.MapVars = new int[NumMapVars][];
        VmAcsBehaviorInitArrays.BindMapVarDefaultsNative(NumMapVars, (i, storeIndex) =>
        {
            mod.MapVarStore[storeIndex] = mod.MapVarStore[i];
            mod.MapVars![i] = mod.MapVarStore;
        });
    }

    static void LoadEnhancedChunks(InitContext ctx, BehaviorModule mod)
    {
        Array.Clear(mod.MapVarStore, 0, mod.MapVarStore.Length);
        LoadMiniChunks(ctx, mod);
        VmAcsBehaviorInitArrays.BindMapVarDefaultsNative(NumMapVars, (i, storeIndex) =>
        {
            mod.MapVars ??= new int[NumMapVars][];
            mod.MapVars[i] = mod.MapVarStore;
            mod.MapVarStore[i] = mod.MapVarStore[i];
        });

        LoadFunctionChunk(ctx, mod);
        LoadFunctionArrayChunks(ctx, mod);
        LoadJumpChunk(ctx, mod);
        LoadArrayChunks(ctx, mod);
        LoadArrayInitChunks(ctx, mod);
        BuildArrayImportTable(ctx, mod);
        TagStringChunks(ctx, mod);
        LoadLibraries(ctx, mod);
    }

    static void LoadMiniChunks(InitContext ctx, BehaviorModule mod)
    {
        var chunk = ctx.FindChunk?.Invoke(AcsFourCc("M", "I", "N", "I"), mod.Data);
        while (chunk != null)
        {
            if (chunk.Length >= 8)
            {
                var numvars = (BitConverter.ToInt32(chunk, 0) / 4) - 1;
                var firstvar = BitConverter.ToInt32(chunk, 4);
                var values = new int[Math.Max(0, numvars)];
                for (var i = 0; i < values.Length && 8 + i * 4 < chunk.Length; i++)
                    values[i] = BitConverter.ToInt32(chunk, 8 + i * 4);
                VmAcsBehaviorInitLoad.LoadMiniChunkVars(mod.MapVarStore, firstvar, values);
            }

            chunk = ctx.NextChunk?.Invoke(chunk);
        }
    }

    static void LoadFunctionChunk(InitContext ctx, BehaviorModule mod)
    {
        var funcs = ctx.FindChunk?.Invoke(AcsFourCc("F", "U", "N", "C"), mod.Data);
        if (funcs == null || funcs.Length < 8)
            return;

        var numFunctions = BitConverter.ToInt32(funcs, 0) / 8;
        mod.NumFunctions = numFunctions;
        mod.HasFunctionProfile = numFunctions > 0;
        mod.Functions = new VmAcsBehaviorInitLoad.ScriptFunctionInFile[numFunctions];
        mod.HasFunctions = true;
        for (var i = 0; i < numFunctions; i++)
        {
            var off = 8 + i * 8;
            if (off + 8 > funcs.Length)
                break;
            mod.Functions[i] = new VmAcsBehaviorInitLoad.ScriptFunctionInFile
            {
                ArgCount = funcs[off],
                HasReturnValue = funcs[off + 1] != 0,
                ImportNum = funcs[off + 2],
                LocalCount = funcs[off + 3],
                Address = BitConverter.ToInt32(funcs, off + 4),
            };
        }
    }

    static void LoadFunctionArrayChunks(InitContext ctx, BehaviorModule mod)
    {
        if (mod.Functions == null || mod.NumFunctions <= 0 || ctx.ParseLocalArrayChunk == null)
            return;

        var chunk = ctx.FindChunk?.Invoke(AcsFourCc("F", "A", "R", "Y"), mod.Data);
        while (chunk != null)
        {
            if (chunk.Length >= 6)
            {
                var funcNum = BitConverter.ToUInt16(chunk, 8);
                if (funcNum < mod.NumFunctions)
                {
                    var func = mod.Functions[funcNum];
                    func.LocalCount = VmAcsBehaviorInitLoad.ApplyFunctionArrayChunkNative(
                        funcNum, mod.NumFunctions, func.ArgCount, func.LocalCount, ctx.ParseLocalArrayChunk);
                }
            }

            chunk = ctx.NextChunk?.Invoke(chunk);
        }
    }

    static void LoadJumpChunk(InitContext ctx, BehaviorModule mod)
    {
        var chunk = ctx.FindChunk?.Invoke(AcsFourCc("J", "U", "M", "P"), mod.Data);
        if (chunk == null)
            return;
        var count = BitConverter.ToInt32(chunk, 0);
        for (var i = 0; i < count; i += 4)
        {
            if (8 + i + 4 <= chunk.Length)
                mod.JumpPoints.Add(BitConverter.ToInt32(chunk, 8 + i));
        }
    }

    static void LoadArrayChunks(InitContext ctx, BehaviorModule mod)
    {
        var chunk = ctx.FindChunk?.Invoke(AcsFourCc("A", "R", "R", "Y"), mod.Data);
        if (chunk == null)
            return;
        mod.NumArrays = BitConverter.ToInt32(chunk, 0) / 8;
        mod.HasArrayStore = mod.NumArrays > 0;
        for (var i = 0; i < mod.NumArrays; i++)
        {
            var mapIndex = BitConverter.ToInt32(chunk, 8 + i * 8);
            var size = BitConverter.ToInt32(chunk, 12 + i * 8);
            mod.MapVarStore[mapIndex] = i;
            _ = size;
        }
    }

    static void LoadArrayInitChunks(InitContext ctx, BehaviorModule mod)
    {
        var chunk = ctx.FindChunk?.Invoke(AcsFourCc("A", "I", "N", "I"), mod.Data);
        while (chunk != null)
        {
            if (chunk.Length >= 8)
            {
                var arraynum = mod.MapVarStore[BitConverter.ToInt32(chunk, 4)];
                var elems = new int[mod.NumArrays];
                var values = new int[(chunk.Length - 8) / 4];
                for (var j = 0; j < values.Length; j++)
                    values[j] = BitConverter.ToInt32(chunk, 8 + j * 4);
                VmAcsBehaviorInitArrays.InitArrayFromChunkNative(arraynum, values.Length, elems, values);
            }

            chunk = ctx.NextChunk?.Invoke(chunk);
        }
    }

    static void BuildArrayImportTable(InitContext ctx, BehaviorModule mod)
    {
        mod.NumTotalArrays = mod.NumArrays;
        var imp = ctx.FindChunk?.Invoke(AcsFourCc("A", "I", "M", "P"), mod.Data);
        if (imp != null && imp.Length >= 8)
            mod.NumTotalArrays += BitConverter.ToInt32(imp, 4);
        if (mod.NumTotalArrays > 0)
            mod.HasArrays = true;
    }

    static void TagStringChunks(InitContext ctx, BehaviorModule mod)
    {
        if (mod.LibraryId == 0 || ctx.LookupString == null || ctx.AddGlobalString == null)
            return;

        TagMapStringChunk(ctx, mod);
        TagArrayStringChunk(ctx, mod);
        TagAtagChunks(ctx, mod);
    }

    static void TagMapStringChunk(InitContext ctx, BehaviorModule mod)
    {
        var chunk = ctx.FindChunk?.Invoke(AcsFourCc("M", "S", "T", "R"), mod.Data);
        if (chunk == null)
            return;
        var count = BitConverter.ToInt32(chunk, 0) / 4;
        for (var i = 0; i < count; i++)
        {
            var idx = BitConverter.ToInt32(chunk, 8 + i * 4);
            var str = ctx.LookupString!(mod.MapVarStore[idx]);
            if (str != null)
                mod.MapVarStore[idx] = ctx.AddGlobalString!(str);
        }
    }

    static void TagArrayStringChunk(InitContext ctx, BehaviorModule mod)
    {
        var chunk = ctx.FindChunk?.Invoke(AcsFourCc("A", "S", "T", "R"), mod.Data);
        if (chunk == null)
            return;
        var count = BitConverter.ToInt32(chunk, 0) / 4;
        for (var i = 0; i < count; i++)
        {
            var arraynum = mod.MapVarStore[BitConverter.ToInt32(chunk, 8 + i * 4)];
            if ((uint)arraynum >= (uint)mod.NumArrays)
                continue;
            var elems = new[] { arraynum };
            VmAcsBehaviorInitArrays.TagStringArrayElementsNative(elems.Length, elems, ctx.LookupString!, ctx.AddGlobalString!);
        }
    }

    static void TagAtagChunks(InitContext ctx, BehaviorModule mod)
    {
        var chunk = ctx.FindChunk?.Invoke(AcsFourCc("A", "T", "A", "G"), mod.Data);
        while (chunk != null)
        {
            if (chunk.Length >= 5 && chunk[8] == 0)
            {
                var arraynum = mod.MapVarStore[BitConverter.ToInt32(chunk, 9)];
                if ((uint)arraynum < (uint)mod.NumArrays)
                {
                    var elem = arraynum;
                    for (var j = 9; j < chunk.Length; j++)
                    {
                        VmAcsBehaviorInitArrays.ApplyAtagElementNative(ref elem, chunk[j], mod.LibraryId,
                            ctx.LookupString!, ctx.AddGlobalString!);
                    }
                }
            }

            chunk = ctx.NextChunk?.Invoke(chunk);
        }
    }

    static void LoadLibraries(InitContext ctx, BehaviorModule mod)
    {
        var chunk = ctx.FindChunk?.Invoke(AcsFourCc("L", "O", "A", "D"), mod.Data);
        if (chunk == null || mod.Functions == null)
            return;

        var parse = System.Text.Encoding.ASCII.GetString(chunk.AsSpan(8));
        for (var i = 0; i < parse.Length;)
        {
            if (parse[i] != '\0')
            {
                var tokenStart = VmAcsBehaviorInitArrays.LoadLibraryTokenNative(parse, i, out i);
                if (tokenStart >= 0)
                {
                    var name = parse.Substring(tokenStart, Math.Max(0, parse.IndexOf('\0', tokenStart) - tokenStart));
                    var lump = ctx.CheckLibraryLump?.Invoke(name) ?? -1;
                    if (lump < 0)
                        ctx.LogMissingLibrary?.Invoke(name);
                    else
                    {
                        var module = ctx.LoadModule?.Invoke(lump);
                        VmAcsBehaviorInitImports.RegisterLoadedImportNative(module, mod.Imports.Add);
                    }
                }

                while (i < parse.Length && parse[i] != '\0')
                    i++;
            }

            i++;
        }

        ResolveImports(ctx, mod, chunk);
    }

    static void ResolveImports(InitContext ctx, BehaviorModule mod, byte[] loadChunk)
    {
        if (mod.Functions == null)
            return;

        for (var i = 0; i < mod.Imports.Count; i++)
        {
            var lib = mod.Imports[i];
            if (VmAcsBehaviorInitImports.ShouldSkipNullImport(lib))
                continue;
            ResolveImportFunctions(ctx, mod, i);
            ResolveImportMapVars(ctx, mod, lib);
            if (VmAcsBehaviorInitImports.HasImportArrays(mod.NumTotalArrays, mod.NumArrays))
                ResolveImportArrays(ctx, mod);
        }
    }

    static void ResolveImportFunctions(InitContext ctx, BehaviorModule mod, int libIndex)
    {
        if (mod.Functions == null)
            return;
        for (var j = 0; j < mod.NumFunctions; j++)
        {
            var func = mod.Functions[j];
            if (!VmAcsBehaviorInitImports.ShouldResolveImportFunction(func.Address, func.ImportNum))
                continue;
            func.ImportNum = libIndex + 1;
            func.Address = j;
        }
    }

    static void ResolveImportMapVars(InitContext ctx, BehaviorModule mod, object? lib)
    {
        _ = ctx;
        _ = lib;
        mod.MapVars ??= new int[NumMapVars][];
    }

    static void ResolveImportArrays(InitContext ctx, BehaviorModule mod)
    {
        _ = ctx;
        _ = mod;
    }

    static void FinishInit(InitContext ctx, BehaviorModule mod)
    {
        _ = ctx;
        mod.NumScripts = VmAcsBehaviorInitArrays.CountLoadedScriptsNative(mod.NumScripts, mod.NumFunctions);
        _ = VmAcsBehaviorInitImports.InitSucceededNative(mod.NumScripts, mod.NumFunctions);
    }

    public static void ReleaseBehavior(BehaviorModule mod, Action? releaseScripts, Action? releaseArrays,
        Action<int>? releaseArrayElements, Action? releaseFunctions, Action? releaseProfile, Action? releaseData)
    {
        VmAcsBehaviorInitImports.ReleaseBehaviorNative(
            mod.HasScripts,
            mod.HasArrays,
            mod.HasArrayStore,
            () =>
            {
                mod.HasScripts = false;
                releaseScripts?.Invoke();
            },
            () =>
            {
                mod.HasArrays = false;
                releaseArrays?.Invoke();
            },
            () =>
            {
                if (releaseArrayElements != null)
                    VmAcsBehaviorInitImports.ReleaseArrayStoreNative(mod.NumArrays, releaseArrayElements);
                mod.HasArrayStore = false;
            });
        if (mod.HasFunctions)
        {
            mod.HasFunctions = false;
            releaseFunctions?.Invoke();
        }

        if (mod.HasFunctionProfile)
        {
            mod.HasFunctionProfile = false;
            releaseProfile?.Invoke();
        }

        if (mod.HasData)
        {
            mod.HasData = false;
            mod.Data = null;
            releaseData?.Invoke();
        }
    }

    static uint ReadUInt32At(byte[] data, int offset) =>
        offset + 4 <= data.Length ? BitConverter.ToUInt32(data, offset) : 0;

    static uint BinaryPrimitivesReverse(byte[] data, int offset) => ReadUInt32At(data, offset);

    static uint AcsFourCc(string a, string b, string c, string d) =>
        (uint)(a[0] | (b[0] << 8) | (c[0] << 16) | (d[0] << 24));
}
