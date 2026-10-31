namespace HCDE.Playsim.Tests;

public class VmPAcsModuleState1167SliceTests
{
    [Fact]
    public void Strings_MarkAndLockEveryMapVarThenEachArray()
    {
        var unlocked = new List<int>();
        VmPAcsModuleState1167Slice.UnlockLevelVarStrings(9, unlocked.Add);
        Assert.Equal(new[] { 9 }, unlocked);

        var behavior = new VmPAcsModuleState1167Slice.Behavior
        {
            NumArrays = 2,
            ModuleName = "keep",
        };
        behavior.Stores.Add(new VmPAcsModuleState1167Slice.StoredArray
        {
            Elements = new[] { 1 },
            ArraySize = 4,
        });
        behavior.Stores.Add(new VmPAcsModuleState1167Slice.StoredArray
        {
            Elements = new[] { 2, 3 },
            ArraySize = 2,
        });
        var marked = new List<int>();
        VmPAcsModuleState1167Slice.MarkMapVarStrings(behavior, (values, count) => marked.Add(count + values[0]));
        Assert.Equal(
            new[] { VmPAcsModuleState1167Slice.NumMapVars, 5, 4 },
            marked);

        var locked = new List<(int Level, int Count)>();
        VmPAcsModuleState1167Slice.LockMapVarStrings(behavior, 6, (level, values, count) => locked.Add((level, count)));
        Assert.Equal(3, locked.Count);
        Assert.All(locked, pair => Assert.Equal(6, pair.Level));
        Assert.Equal(VmPAcsModuleState1167Slice.NumMapVars, locked[0].Count);
        Assert.Equal(4, locked[1].Count);
        Assert.Equal(2, locked[2].Count);
    }

    [Fact]
    public void Modules_StopWhenTheSaveNamesOrSizesDisagree()
    {
        var live = new VmPAcsModuleState1167Slice.Behavior { ModuleName = "Lib", DataSize = 40 };
        var other = new VmPAcsModuleState1167Slice.Behavior { ModuleName = "Other", DataSize = 8 };
        var closed = new VmPAcsModuleState1167Slice.ModuleArchive { OpenArray = false };
        VmPAcsModuleState1167Slice.SerializeModuleStates(new[] { live }, closed);
        Assert.Null(closed.Error);
        Assert.Equal(0, live.SerializeVarsCalls);

        var count = new VmPAcsModuleState1167Slice.ModuleArchive { Reading = true, SavedCount = 1 };
        VmPAcsModuleState1167Slice.SerializeModuleStates(new[] { live, other }, count);
        Assert.Equal(
            "Level was saved with a different number of ACS modules. (Have 2, save has 1)",
            count.Error);
        Assert.Equal(0, live.SerializeVarsCalls);

        var order = new VmPAcsModuleState1167Slice.ModuleArchive
        {
            Reading = true,
            SavedCount = 1,
            Names = { "Other" },
            Sizes = { 40 },
            ObjectsOpen = { true },
        };
        VmPAcsModuleState1167Slice.SerializeModuleStates(new[] { live }, order);
        Assert.Equal(
            "Level was saved with a different set or order of ACS modules. (Have Lib, save has Other)",
            order.Error);
        Assert.Equal(0, live.SerializeVarsCalls);

        var grown = new VmPAcsModuleState1167Slice.ModuleArchive
        {
            Reading = true,
            SavedCount = 1,
            Names = { "LIB" },
            Sizes = { 12 },
            ObjectsOpen = { true },
        };
        VmPAcsModuleState1167Slice.SerializeModuleStates(new[] { live }, grown);
        Assert.Equal(
            "ACS module Lib has changed from what was saved. (Have 40 bytes, save has 12 bytes)",
            grown.Error);

        var writing = new VmPAcsModuleState1167Slice.ModuleArchive
        {
            ObjectsOpen = { false, true },
        };
        VmPAcsModuleState1167Slice.SerializeModuleStates(new[] { live, other }, writing);
        Assert.Null(writing.Error);
        Assert.Equal(0, live.SerializeVarsCalls);
        Assert.Equal(1, other.SerializeVarsCalls);
        Assert.Equal(("Other", 8), writing.Written[0]);
        Assert.True(writing.EndedArray);
    }

    [Fact]
    public void VarSets_DropTheZeroEdgesAndClampALongRead()
    {
        var vars = new[] { 0, 0, 5, 0, 7, 0 };
        var span = VmPAcsModuleState1167Slice.WriteVarSet(vars, vars.Length);
        Assert.Equal(3, span.Count);
        Assert.Equal(2, span.First);
        Assert.Equal(new[] { 5, 0, 7 }, span.Values);

        var zeros = VmPAcsModuleState1167Slice.WriteVarSet(new int[4], 4);
        Assert.Equal(0, zeros.Count);
        Assert.Empty(zeros.Values);

        var destination = new[] { 9, 9, 9, 9 };
        VmPAcsModuleState1167Slice.ReadVarSet(destination, destination.Length, new VmPAcsModuleState1167Slice.VarSpan
        {
            Count = 3,
            First = 2,
            Values = new[] { 4, 6, 8 },
        });
        Assert.Equal(new[] { 0, 0, 4, 6 }, destination);

        var behavior = new VmPAcsModuleState1167Slice.Behavior { NumArrays = 1 };
        behavior.MapVarStore[1] = 11;
        behavior.Stores.Add(new VmPAcsModuleState1167Slice.StoredArray
        {
            Elements = new[] { 0, 4 },
            ArraySize = 2,
        });
        var written = new List<VmPAcsModuleState1167Slice.VarSpan>();
        VmPAcsModuleState1167Slice.SerializeVars(behavior, false, false, written);
        Assert.Empty(written);
        VmPAcsModuleState1167Slice.SerializeVars(behavior, true, false, written);
        Assert.Equal(2, written.Count);
        Assert.Equal(1, written[0].First);
        Assert.Equal(new[] { 11 }, written[0].Values);
        Assert.Equal(new[] { 4 }, written[1].Values);

        behavior.MapVarStore[1] = 99;
        behavior.Stores[0].Elements[1] = 99;
        VmPAcsModuleState1167Slice.SerializeVars(behavior, true, true, written);
        Assert.Equal(11, behavior.MapVarStore[1]);
        Assert.Equal(0, behavior.MapVarStore[0]);
        Assert.Equal(4, behavior.Stores[0].Elements[1]);
    }

    [Fact]
    public void ChunkAndConstruct_ReadLittleSizesAndResetTheModule()
    {
        var chunk = new byte[24];
        BitConverter.GetBytes(10u).CopyTo(chunk, 4);
        BitConverter.GetBytes(4).CopyTo(chunk, 10);
        BitConverter.GetBytes(6).CopyTo(chunk, 14);
        var arrays = new VmPAcsModuleState1167Slice.LocalArrays();
        var next = VmPAcsModuleState1167Slice.ParseLocalArrayChunk(chunk, arrays, 20);
        Assert.Equal(2, arrays.Count);
        Assert.Equal(4, arrays.Info![0].Size);
        Assert.Equal(20, arrays.Info[0].Offset);
        Assert.Equal(6, arrays.Info[1].Size);
        Assert.Equal(24, arrays.Info[1].Offset);
        Assert.Equal(30, next);

        var empty = new byte[8];
        BitConverter.GetBytes(2u).CopyTo(empty, 4);
        var none = new VmPAcsModuleState1167Slice.LocalArrays { Info = Array.Empty<VmPAcsModuleState1167Slice.LocalArrayInfo>() };
        Assert.Equal(7, VmPAcsModuleState1167Slice.ParseLocalArrayChunk(empty, none, 7));
        Assert.Equal(0, none.Count);
        Assert.Empty(none.Info!);

        var wide = new byte[16];
        BitConverter.GetBytes(65542u).CopyTo(wide, 4);
        BitConverter.GetBytes(3).CopyTo(wide, 10);
        var one = new VmPAcsModuleState1167Slice.LocalArrays();
        Assert.Equal(11, VmPAcsModuleState1167Slice.ParseLocalArrayChunk(wide, one, 8));
        Assert.Equal(1, one.Count);
        Assert.Equal(3, one.Info![0].Size);
        Assert.Equal(8, one.Info[0].Offset);

        var behavior = new VmPAcsModuleState1167Slice.Behavior
        {
            NumScripts = 4,
            NumFunctions = 5,
            NumArrays = 6,
            NumTotalArrays = 7,
            Scripts = "s",
            Functions = "f",
            Arrays = "a",
            ArrayStore = "store",
            Chunks = "c",
            Data = "d",
            Format = 1,
            LumpNum = 12,
            ModuleName = "LIB",
            FunctionProfileData = "p",
        };
        behavior.MapVarStore[3] = 50;
        VmPAcsModuleState1167Slice.Construct(behavior);
        Assert.Equal(0, behavior.NumScripts);
        Assert.Equal(0, behavior.NumFunctions);
        Assert.Equal(0, behavior.NumArrays);
        Assert.Equal(0, behavior.NumTotalArrays);
        Assert.Null(behavior.Scripts);
        Assert.Null(behavior.Functions);
        Assert.Null(behavior.Arrays);
        Assert.Null(behavior.ArrayStore);
        Assert.Null(behavior.Chunks);
        Assert.Null(behavior.Data);
        Assert.Equal(VmPAcsModuleState1167Slice.FormatUnknown, behavior.Format);
        Assert.Equal(-1, behavior.LumpNum);
        Assert.Equal(0, behavior.MapVarStore[3]);
        Assert.Equal("", behavior.ModuleName);
        Assert.Null(behavior.FunctionProfileData);
    }
}
