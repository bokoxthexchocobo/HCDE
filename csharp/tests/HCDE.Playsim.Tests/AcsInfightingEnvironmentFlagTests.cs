namespace HCDE.Playsim.Tests;

public class AcsInfightingEnvironmentFlagTests
{
    [Theory]
    [InlineData("harmfriends")]
    [InlineData("notrigger")]
    [InlineData("nosectordamage")]
    [InlineData("forcesectordamage")]
    [InlineData("doharmspecies")]
    [InlineData("noinfighting")]
    [InlineData("forceinfighting")]
    public void FlagsSupportNativeCaseInsensitiveNames(string name)
    {
        var actor = new Actor();
        Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name.ToUpperInvariant(), out var enabled));
        Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, name.ToUpperInvariant(), false));
        Assert.True(AcsActorFlags.TryGet(actor, name, out enabled));
        Assert.False(enabled);
    }

    [Fact]
    public void ScriptDoHarmSpeciesControlsProjectileImmunity()
    {
        var target = new Actor { DoomEdNum = 3001 };
        var source = new Actor { DoomEdNum = 3001 };
        Assert.True(target.ProjectileImmune(source));
        Assert.True(AcsActorFlags.TrySet(target, "DOHARMSPECIES", true));
        Assert.False(target.ProjectileImmune(source));
        Assert.True(AcsActorFlags.TrySet(target, "DOHARMSPECIES", false));
        Assert.True(target.ProjectileImmune(source));
    }
}
