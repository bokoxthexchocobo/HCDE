namespace HCDE.Playsim.Tests;

public class VmTFuncActorNames1181SliceTests
{
    [Fact]
    public void ActorNames_KeepTheOriginalDoomOrder()
    {
        var names = VmTFuncActorNames1181Slice.ActorNamesInit;
        Assert.Equal(VmFScriptInitFunctions.ActorNamesInit, names);
        Assert.Equal("DoomPlayer", names[0]);
        Assert.Equal("ZombieMan", names[1]);
        Assert.Equal("Cyberdemon", names[21]);
        Assert.Equal("WolfensteinSS", names[23]);
        Assert.Equal("SuperShotgun", names[78]);
        Assert.Equal("Slalagtite", names[97]);
        Assert.Equal("PointPusher", names[137]);
        Assert.Equal("PointPuller", names[138]);
    }

    [Fact]
    public void ActorTypes_StartNullWithOneSlotPerName()
    {
        var names = VmTFuncActorNames1181Slice.ActorNamesInit;
        var slots = VmTFuncActorNames1181Slice.ActorTypes;
        Assert.Equal(names.Length, slots.Length);
        Assert.All(slots, Assert.Null);
        slots[21] = new VmTFuncActorNames1181Slice.ActorClassSlot { ClassName = "Cyberdemon" };
        Assert.Equal("Cyberdemon", slots[21]!.ClassName);
        slots[21] = null;
        Assert.Null(slots[21]);
    }

    [Fact]
    public void ScriptRandom_UsesTheFScriptStream()
    {
        Assert.Equal("FScript", VmTFuncActorNames1181Slice.ScriptRandomName);
        Assert.Equal("PointPuller", VmTFuncActorNames1181Slice.ActorNamesInit[^1]);
        Assert.Equal("DoomPlayer", VmTFuncActorNames1181Slice.ActorNamesInit[0]);
    }
}
