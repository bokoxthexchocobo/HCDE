namespace HCDE.Client.Tests;

public class VmCvarArchiveTests
{
    [Fact]
    public void ArchiveCvars_FiltersByMask()
    {
        var reg = new VmCvarRegistry();
        reg.Register(new VmCvarEntry("crosshair", VmCvarFlags.Archive | VmCvarFlags.UserInfo, "1"));
        reg.Register(new VmCvarEntry("sv_cheats", VmCvarFlags.ServerInfo, "0"));
        var config = new VmConfigFile();
        config.SetSection("Doom.Player", allowCreate: true);
        VmCvarArchive.ArchiveCvars(config, reg, VmCvarFlags.Archive | VmCvarFlags.UserInfo);
        Assert.Equal("1", config.GetValueForKey("crosshair"));
        Assert.Null(config.GetValueForKey("sv_cheats"));
    }

    [Fact]
    public void Registry_FindAndGetCVar()
    {
        var reg = new VmCvarRegistry();
        reg.Register(new VmCvarEntry("foo", VmCvarFlags.Archive, "bar"));
        Assert.NotNull(reg.Find("foo"));
        Assert.NotNull(reg.FindSub("foobar", 3));
        Assert.Null(reg.GetCVar(0, "missing"));
    }
}
