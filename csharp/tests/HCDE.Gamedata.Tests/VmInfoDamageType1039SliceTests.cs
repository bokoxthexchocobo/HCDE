namespace HCDE.Gamedata.Tests;

public class VmInfoDamageType1039SliceTests
{
    [Fact]
    public void ResolveMobjDamageFactor_UsesTypedActorFactor()
    {
        var factors = new List<(string, double)> { ("Fire", 0.5) };
        var global = new Dictionary<string, VmInfoDamageType1039Slice.GlobalDamageTypeDraft>();
        Assert.Equal(0.5, VmInfoDamageType1039Slice.ResolveMobjDamageFactor("Fire", factors, global));
    }

    [Fact]
    public void ResolveMobjDamageFactor_ReplaceFactorOverridesUntyped()
    {
        var factors = new List<(string, double)> { ("None", 2.0) };
        var global = new Dictionary<string, VmInfoDamageType1039Slice.GlobalDamageTypeDraft>
        {
            ["Fire"] = new(0.25, ReplaceFactor: true, false, "")
        };
        Assert.Equal(0.25, VmInfoDamageType1039Slice.ResolveMobjDamageFactor("Fire", factors, global));
    }

    [Fact]
    public void ApplyMobjDamageFactor_ScalesDamage()
    {
        var global = new Dictionary<string, VmInfoDamageType1039Slice.GlobalDamageTypeDraft>();
        var factors = new List<(string, double)> { ("Cut", 0.5) };
        Assert.Equal(10, VmInfoDamageType1039Slice.ApplyMobjDamageFactor(20, "Cut", factors, global));
    }

    [Fact]
    public void GlobalIgnoresArmor_ReadsFlag()
    {
        var global = new Dictionary<string, VmInfoDamageType1039Slice.GlobalDamageTypeDraft>
        {
            ["Disintegrate"] = new(1, false, NoArmor: true, "")
        };
        Assert.True(VmInfoDamageType1039Slice.GlobalIgnoresArmor(global, "Disintegrate"));
    }

    [Fact]
    public void ApplyFactorProperty_ZeroSetsReplace()
    {
        var draft = VmInfoDamageType1039Slice.ApplyFactorProperty(default, 0);
        Assert.True(draft.ReplaceFactor);
    }

    [Fact]
    public void ClassifyDamageDefinitionProperty_RecognizesNoArmor()
    {
        Assert.Equal(
            VmInfoDamageType1039Slice.ParseDamageProperty.NoArmor,
            VmInfoDamageType1039Slice.ClassifyDamageDefinitionProperty("NOARMOR"));
    }

    [Fact]
    public void RegisterGlobalDamageType_StoresDraft()
    {
        var table = new Dictionary<string, VmInfoDamageType1039Slice.GlobalDamageTypeDraft>();
        VmInfoDamageType1039Slice.RegisterGlobalDamageType(table, "Poison", new(2, false, false, "died"));
        Assert.Equal(2, table["Poison"].DefaultFactor);
    }

    [Fact]
    public void ResolveGlobalObituary_ReturnsEmptyWhenMissing()
    {
        Assert.Equal("", VmInfoDamageType1039Slice.ResolveGlobalObituary(new Dictionary<string, VmInfoDamageType1039Slice.GlobalDamageTypeDraft>(), "X"));
    }

    [Fact]
    public void ResolveMobjDamageFactor_NoneWithoutFactors_ReturnsOne()
    {
        Assert.Equal(1.0, VmInfoDamageType1039Slice.ResolveMobjDamageFactor("None", null, new Dictionary<string, VmInfoDamageType1039Slice.GlobalDamageTypeDraft>()));
    }
}
