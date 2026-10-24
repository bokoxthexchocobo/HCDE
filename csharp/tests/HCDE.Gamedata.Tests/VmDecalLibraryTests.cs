namespace HCDE.Gamedata.Tests;

public class VmDecalLibraryTests
{
    private static VmDecalDefinition Def(string name, ushort id, double scaleX = 1)
    {
        var d = new VmDecalDefinition { Name = name, Id = id };
        d.Template.ScaleX = scaleX;
        d.Template.PicName = name;
        return d;
    }

    [Fact]
    public void AddDecal_InsertsAndFindsByNameAndNum()
    {
        var lib = new VmDecalLibrary();
        lib.AddDecal(Def("Blood", 10));
        lib.AddDecal(Def("Scorch", 20));
        Assert.Equal(10, lib.GetDecalByName("blood")!.Id);
        Assert.Equal("Scorch", lib.GetDecalByNum(20)!.Name);
        Assert.Null(lib.GetDecalByNum(0));
    }

    [Fact]
    public void AddDecal_ReplacesSameNameAndClearsDuplicateId()
    {
        var lib = new VmDecalLibrary();
        lib.AddDecal(Def("A", 5, 1));
        lib.AddDecal(Def("B", 5, 2));
        lib.AddDecal(Def("A", 7, 3));
        Assert.Equal(3, lib.GetDecalByName("A")!.Template.ScaleX);
        Assert.Equal("B", lib.GetDecalByNum(5)!.Name);
        Assert.Equal(7, lib.GetDecalByNum(7)!.Id);
    }

    [Fact]
    public void Clear_RemovesAllDecals()
    {
        var lib = new VmDecalLibrary();
        lib.AddDecal(Def("X", 1));
        lib.Clear();
        Assert.Null(lib.GetDecalByName("X"));
    }
}
