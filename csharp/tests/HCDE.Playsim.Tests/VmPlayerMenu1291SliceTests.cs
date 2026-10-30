namespace HCDE.Playsim.Tests;

public class VmPlayerMenu1291SliceTests
{
    [Fact]
    public void Menu_EscapesTheNameAndStoresTheTeam()
    {
        var menu = new VmPlayerMenu1291();
        Assert.Equal(0, menu.ColorChanged(255, 16, 0));
        Assert.Equal(0x00FF1000u, menu.ColorBits);
        Assert.Equal("color \"ff 10 00\"", menu.Commands[0]);
        menu.ColorChanged(0x7FFFFFFF, 0x7FFFFFFF, 0x7FFFFFFF);
        Assert.Equal("color \"7fffffff 7ffffff", menu.Commands[1]);

        menu.NameChanged("a\"b\\c");
        Assert.Equal("name \"a\\\"b\\\\c\"", menu.Commands[2]);
        menu.ColorSetChanged(7);
        Assert.Equal(7, menu.ColorSet);
        Assert.Equal("colorset 7", menu.Commands[3]);
        menu.ClassChanged(-1, "Fighter");
        Assert.Equal("Random", menu.PlayerClass);
        menu.ClassChanged(2, "Fighter");
        Assert.Equal("cvar playerclass Fighter", menu.Commands[5]);
        menu.SkinChanged(4, "Marine");
        Assert.Equal(4, menu.SkinNum);
        Assert.Equal("cvar skin Marine", menu.Commands[6]);
        menu.AutoaimChanged(0.5f);
        Assert.Equal(0.5f, menu.Autoaim);
        menu.TeamChanged(0);
        Assert.Equal(255, menu.Team);
        menu.TeamChanged(2);
        Assert.Equal(1, menu.Team);
        menu.GenderChanged(1);
        menu.GenderChanged(4);
        Assert.Equal("cvar gender female", menu.Commands[7]);
        Assert.Equal(8, menu.Commands.Count);
        menu.SwitchOnPickupChanged(2);
        menu.AlwaysRunChanged(0);
        Assert.True(menu.NeverSwitch);
        Assert.False(menu.AlwaysRun);
        menu.AlwaysRunChanged(-1);
        Assert.True(menu.AlwaysRun);
    }

    [Fact]
    public void Closed_LeavesTheCommandsUntouched()
    {
        var menu = new VmPlayerMenu1291 { InMenu = false, Team = 3, Autoaim = 1f };
        Assert.Equal(0, menu.ColorChanged(1, 2, 3));
        Assert.Equal(0, menu.NameChanged("nope"));
        Assert.Equal(0, menu.ColorSetChanged(9));
        Assert.Equal(0, menu.ClassChanged(-1, "Fighter"));
        Assert.Equal(0, menu.SkinChanged(1, "Marine"));
        Assert.Equal(0, menu.AutoaimChanged(0f));
        Assert.Equal(0, menu.TeamChanged(0));
        Assert.Equal(0, menu.GenderChanged(0));
        Assert.Equal(0, menu.SwitchOnPickupChanged(1));
        Assert.Equal(0, menu.AlwaysRunChanged(1));
        Assert.Empty(menu.Commands);
        Assert.False(menu.WroteColor);
        Assert.False(menu.WroteTeam);
        Assert.Equal(3, menu.Team);
        Assert.Equal(1f, menu.Autoaim);
        Assert.Equal("", menu.PlayerClass);
    }
}
