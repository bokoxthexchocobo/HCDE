using S = HCDE.Playsim.VmDoomMenu1404Slice;

namespace HCDE.Playsim.Tests;

public class VmDoomMenu1404SliceTests
{
    [Fact]
    public void DefaultColorsStopBeforeTheDelegateWhenTheHelpClassFails()
    {
        var info = new S.GameInfo
        {
            Title = "title",
            Font = "font",
            Value = "value",
            More = "more",
            Header = "header",
            Highlight = "highlight",
            Selection = "selection",
            HelpMenuClass = "Help",
            MenuDelegateClass = "Delegate",
        };
        var colors = new S.Colors();
        var seen = new List<string>();
        int finds = 0;
        Assert.Throws<InvalidOperationException>(() => S.SetDefaultMenuColors(info, colors, name =>
        {
            seen.Add(name);
            return seen.Count;
        }, name =>
        {
            finds++;
            Assert.Equal("Help", name);
            return null;
        }));
        Assert.Equal(new[] { "title", "font", "value", "more", "header", "highlight", "selection" }, seen);
        Assert.Equal(7, colors.Selection);
        Assert.Equal(1, finds);

        finds = 0;
        var help = new S.MenuClass { IsMenu = false };
        var wrong = Assert.Throws<InvalidOperationException>(() => S.SetDefaultMenuColors(info, colors, _ => 1, name =>
        {
            finds++;
            return name == "Help" ? help : throw new InvalidOperationException("delegate");
        }));
        Assert.Equal("'Help' does not inherit from Menu", wrong.Message);
        Assert.Equal(1, finds);

        var created = new object();
        var deleg = new S.MenuClass { IsDelegate = true, Created = created };
        object? result = S.SetDefaultMenuColors(info, colors, _ => 4, name => name == "Help"
            ? new S.MenuClass { IsMenu = true }
            : deleg);
        Assert.Same(created, result);
        info.MenuDelegateClass = "Other";
        var missing = Assert.Throws<InvalidOperationException>(() => S.SetDefaultMenuColors(info, colors, _ => 4, name =>
            name == "Help" ? new S.MenuClass { IsMenu = true } : null));
        Assert.Equal("Other: Undefined menu delegate class", missing.Message);
        deleg.IsDelegate = false;
        var baseError = Assert.Throws<InvalidOperationException>(() => S.SetDefaultMenuColors(info, colors, _ => 4, name =>
            name == "Help" ? new S.MenuClass { IsMenu = true } : deleg));
        Assert.Equal("'Other' does not inherit from MenuDelegateBase", baseError.Message);
    }

    [Fact]
    public void MenuCommandsOpenNamedMenusAndMainLeavesTheFullConsole()
    {
        Assert.Equal(21, S.Commands.Length);
        int state = S.FullConsole;
        foreach (S.Command command in S.Commands)
        {
            bool panel = false;
            string? menu = null;
            int param = 0;
            int before = command.LeaveConsole ? S.FullConsole : S.Level;
            state = before;
            S.Run(command.Name, ref state, sound => panel = sound, (name, index) =>
            {
                menu = name;
                param = index;
            });
            Assert.True(panel);
            Assert.Equal(command.Menu, menu);
            Assert.Equal(-1, param);
            Assert.Equal(command.LeaveConsole ? S.MenuScreen : before, state);
        }
        state = S.HideConsole;
        S.Run("menu_main", ref state, _ => { }, (_, _) => { });
        Assert.Equal(S.HideConsole, state);
        Assert.Throws<ArgumentException>(() =>
        {
            int unused = S.Level;
            S.Run("menu_missing", ref unused, _ => { }, (_, _) => { });
        });
    }
}
