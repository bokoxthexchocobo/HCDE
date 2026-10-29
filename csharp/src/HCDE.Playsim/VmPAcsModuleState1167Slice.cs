namespace HCDE.Playsim;

/// <summary>
/// ACS module string locks and variable serialization from <c>p_acs.cpp</c>
/// (<b>2102–2280</b>). Chunk <b>1167</b>.
/// </summary>
public static class VmPAcsModuleState1167Slice
{
    public const int NumMapVars = 128;
    public const int FormatUnknown = 3;

    public sealed class StoredArray
    {
        public int[] Elements = Array.Empty<int>();
        public int ArraySize;
    }

    public sealed class Behavior
    {
        public int NumScripts;
        public int NumFunctions;
        public int NumArrays;
        public int NumTotalArrays;
        public object? Scripts;
        public object? Functions;
        public object? Arrays;
        public object? ArrayStore;
        public object? Chunks;
        public object? Data;
        public int Format;
        public int LumpNum;
        public int[] MapVarStore { get; } = new int[NumMapVars];
        public string ModuleName = "";
        public object? FunctionProfileData;
        public int DataSize;
        public List<StoredArray> Stores { get; } = new();
        public int SerializeVarsCalls;
    }

    public sealed class VarSpan
    {
        public int Count;
        public int First;
        public int[] Values = Array.Empty<int>();
    }

    public sealed class ModuleArchive
    {
        public bool Reading;
        public bool OpenArray = true;
        public int SavedCount;
        public List<bool> ObjectsOpen { get; } = new();
        public List<string> Names { get; } = new();
        public List<int> Sizes { get; } = new();
        public List<(string Name, int Size)> Written { get; } = new();
        public string? Error;
        public bool EndedArray;

        public bool BeginArray(string name)
        {
            return name == "acsmodules" && OpenArray;
        }

        public bool BeginObject(int index)
        {
            return index < ObjectsOpen.Count && ObjectsOpen[index];
        }
    }

    public sealed class LocalArrayInfo
    {
        public int Size;
        public int Offset;
    }

    public sealed class LocalArrays
    {
        public int Count;
        public LocalArrayInfo[]? Info;
    }

    public static void UnlockLevelVarStrings(int levelnum, Action<int> unlockForLevel)
    {
        unlockForLevel(levelnum);
    }

    public static void MarkMapVarStrings(Behavior behavior, Action<int[], int> markStringArray)
    {
        markStringArray(behavior.MapVarStore, NumMapVars);
        for (var i = 0; i < behavior.NumArrays; i++)
        {
            var store = behavior.Stores[i];
            markStringArray(store.Elements, store.ArraySize);
        }
    }

    public static void LockMapVarStrings(Behavior behavior, int levelnum, Action<int, int[], int> lockStringArray)
    {
        lockStringArray(levelnum, behavior.MapVarStore, NumMapVars);
        for (var i = 0; i < behavior.NumArrays; i++)
        {
            var store = behavior.Stores[i];
            lockStringArray(levelnum, store.Elements, store.ArraySize);
        }
    }

    public static void SerializeModuleStates(IReadOnlyList<Behavior> modules, ModuleArchive arc)
    {
        if (!arc.BeginArray("acsmodules"))
        {
            return;
        }

        if (arc.Reading && arc.SavedCount != modules.Count)
        {
            arc.Error = "Level was saved with a different number of ACS modules. (Have "
                + modules.Count + ", save has " + arc.SavedCount + ")";
            return;
        }

        for (var modnum = 0; modnum < modules.Count; modnum++)
        {
            var module = modules[modnum];
            if (!arc.BeginObject(modnum))
            {
                continue;
            }

            var modname = arc.Reading ? arc.Names[modnum] : module.ModuleName;
            var modSize = arc.Reading ? arc.Sizes[modnum] : module.DataSize;
            if (!arc.Reading)
            {
                arc.Written.Add((modname, modSize));
            }

            if (arc.Reading)
            {
                if (!string.Equals(modname, module.ModuleName, StringComparison.OrdinalIgnoreCase))
                {
                    arc.Error = "Level was saved with a different set or order of ACS modules. (Have "
                        + module.ModuleName + ", save has " + modname + ")";
                    return;
                }

                if (modSize != module.DataSize)
                {
                    arc.Error = "ACS module " + module.ModuleName
                        + " has changed from what was saved. (Have " + module.DataSize
                        + " bytes, save has " + modSize + " bytes)";
                    return;
                }
            }

            module.SerializeVarsCalls++;
            arc.EndedArray = modnum == modules.Count - 1;
        }

        arc.EndedArray = arc.Error == null;
    }

    public static void SerializeVars(
        Behavior behavior,
        bool beginArray,
        bool reading,
        List<VarSpan> spans)
    {
        if (!beginArray)
        {
            return;
        }

        if (!reading)
        {
            spans.Add(WriteVarSet(behavior.MapVarStore, NumMapVars));
            for (var i = 0; i < behavior.NumArrays; i++)
            {
                var store = behavior.Stores[i];
                spans.Add(WriteVarSet(store.Elements, store.ArraySize));
            }

            return;
        }

        if (spans.Count == 0)
        {
            return;
        }

        ReadVarSet(behavior.MapVarStore, NumMapVars, spans[0]);
        for (var i = 0; i < behavior.NumArrays; i++)
        {
            var store = behavior.Stores[i];
            ReadVarSet(store.Elements, store.ArraySize, spans[i + 1]);
        }
    }

    public static VarSpan WriteVarSet(int[] vars, int max)
    {
        var first = 0;
        while (first < max)
        {
            if (vars[first] != 0)
            {
                break;
            }

            first++;
        }

        var last = max - 1;
        while (last >= first)
        {
            if (vars[last] != 0)
            {
                break;
            }

            last--;
        }

        var span = new VarSpan();
        if (last < first)
        {
            span.Count = 0;
            return span;
        }

        span.Count = last - first + 1;
        span.First = first;
        span.Values = new int[span.Count];
        Array.Copy(vars, first, span.Values, 0, span.Count);
        return span;
    }

    public static void ReadVarSet(int[] vars, int max, VarSpan span)
    {
        Array.Clear(vars, 0, max);
        var count = span.Count;
        if (count == 0)
        {
            return;
        }

        var first = span.First;
        if (first + count > max)
        {
            count = max - first;
        }

        if (count > 0)
        {
            Array.Copy(span.Values, 0, vars, first, count);
        }
    }

    public static int ParseLocalArrayChunk(byte[] chunk, LocalArrays arrays, int offset)
    {
        var word = BitConverter.ToUInt32(chunk, 4);
        var count = (int)((ushort)(word - 2) / 4);
        arrays.Count = count;
        if (count > 0)
        {
            var info = new LocalArrayInfo[count];
            arrays.Info = info;
            for (var i = 0; i < count; i++)
            {
                var size = BitConverter.ToInt32(chunk, 10 + (i * 4));
                info[i] = new LocalArrayInfo { Size = size, Offset = offset };
                offset += size;
            }
        }

        return offset;
    }

    public static void Construct(Behavior behavior)
    {
        behavior.NumScripts = 0;
        behavior.NumFunctions = 0;
        behavior.NumArrays = 0;
        behavior.NumTotalArrays = 0;
        behavior.Scripts = null;
        behavior.Functions = null;
        behavior.Arrays = null;
        behavior.ArrayStore = null;
        behavior.Chunks = null;
        behavior.Data = null;
        behavior.Format = FormatUnknown;
        behavior.LumpNum = -1;
        Array.Clear(behavior.MapVarStore, 0, behavior.MapVarStore.Length);
        behavior.ModuleName = "";
        behavior.FunctionProfileData = null;
    }
}
