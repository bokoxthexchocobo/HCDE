namespace HCDE.Client.Tests;

public class VmStartupGameFiltersTests
{
    [Theory]
    [InlineData(VmStartupGameType.Doom, "game-DoomStrifeChex,game-DoomChex,game-Doom")]
    [InlineData(VmStartupGameType.Heretic, "game-Raven,game-Heretic")]
    [InlineData(VmStartupGameType.Hexen, "game-Raven,game-Hexen")]
    [InlineData(VmStartupGameType.Strife, "game-DoomStrifeChex,game-Strife")]
    [InlineData(VmStartupGameType.Chex, "game-DoomStrifeChex,game-DoomChex,game-Chex")]
    public void NativeGameFamiliesPrecedeGameAndIwad(VmStartupGameType game, string families)
    {
        Assert.Equal(families.Split(',').Append("selected.iwad"), VmStartupGameFilters.Build(game, "selected.iwad"));
        Assert.Equal("", VmStartupGameFilters.Build(game)[^1]);
    }

    [Theory]
    [InlineData("doom.id.doom2", "doom.doom2")]
    [InlineData("doom.id.doom.shareware", "doom.doom.shareware")]
    [InlineData("xdoom.id.doom2", "doom.doomm2")]
    public void LegacyDoomAliasesUseNativeFixedOffset(string filter, string alias)
    {
        Assert.Equal(new[] { filter, alias }, VmStartupGameFilters.Build(VmStartupGameType.Doom, filter).TakeLast(2));
    }

    [Fact]
    public void CaseSensitiveAliasAndInvalidGameValidation()
    {
        Assert.Equal(4, VmStartupGameFilters.Build(VmStartupGameType.Doom, "DOOM.ID.DOOM2").Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => VmStartupGameFilters.Build((VmStartupGameType)3));
        Assert.Throws<ArgumentNullException>(() => VmStartupGameFilters.Build(VmStartupGameType.Doom, null!));
        Assert.False(LauncherPlan.TryParse(["--iwad", "game.wad", "--game-type", "3"], out _, out var error));
        Assert.Equal("game-type", error);
        Assert.False(LauncherPlan.TryParse(["--iwad", "game.wad", "--iwad-filter", "doom.id.doom2"], out _, out error));
        Assert.Equal("iwad-filter", error);
        Assert.False(LauncherPlan.TryParse(["--iwad", "game.wad", "--game-type"], out _, out error));
        Assert.Equal("game-type", error);
    }
}
