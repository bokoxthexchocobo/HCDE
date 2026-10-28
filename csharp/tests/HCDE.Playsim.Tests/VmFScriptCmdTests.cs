using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptCmdTests
{
    [Fact]
    public void Gimme_Health_NormalizesAndGives()
    {
        var cmd = "";
        VmFScriptCmd.Gimme("HEALTH100", s => cmd = s);
        Assert.Equal("give health", cmd);
    }

    [Fact]
    public void MapCmd_ChangeLevelWithFlags()
    {
        string? map = null;
        var flags = -1;
        var level = new VmFScriptCmd.LevelCmdHost
        {
            Dmflags = VmFScriptCmd.DfNoMonsters,
            ChangeLevel = (m, _, f, _) =>
            {
                map = m;
                flags = f;
            },
        };
        var sc = new VmFScriptCmd.CmdScanner();
        sc.OpenMem("x", "MAP01 -monsters 1");
        VmFScriptCmd.MapCmd(level, sc);
        Assert.Equal("MAP01", map);
        Assert.Equal(0, flags & VmFScriptCmd.ChangelevelNoMonsters);
    }

    [Fact]
    public void EmulateCmd_GimmeAndGravity()
    {
        var cmds = new List<string>();
        var level = new VmFScriptCmd.LevelCmdHost { AddCommandString = cmds.Add };
        VmFScriptCmd.EmulateCmd(level, "GIMME health ; gravity 1.5 ;");
        Assert.Contains("give health", cmds);
        Assert.Equal(1200, level.Gravity);
    }

    [Fact]
    public void EmulateCmd_AllowJumpTogglesDmflags()
    {
        var level = new VmFScriptCmd.LevelCmdHost { Dmflags = VmFScriptCmd.DfNoJump };
        VmFScriptCmd.EmulateCmd(level, "ALLOWJUMP 1 ;");
        Assert.Equal(0, level.Dmflags & VmFScriptCmd.DfNoJump);
    }
}
