namespace HCDE.Playsim.Tests;

public class VmTCmdEmulate1182SliceTests
{
    [Fact]
    public void Gimme_RewritesLegacyPrefixesInOrder()
    {
        var commands = new List<string>();
        void Give(string what) => VmTCmdEmulate1182Slice.Gimme(what, commands.Add);

        Give("HEALTH100");
        Give("AMMOBOX");
        Give("armor");
        Give("keys");
        Give("weapons");
        Give("chainsaw");
        Give("shotgun");
        Give("supershotgun");
        Give("rocketlauncher");
        Give("plasma");
        Give("bfg");
        Give("chaingun");
        Give("BERSERKPACK");
        Give("mapxyz");
        Give("fullmap");
        Give("nope");

        Assert.Equal(
            new[]
            {
                "give health",
                "give ammo",
                "give greenarmor",
                "give keys",
                "give weapons",
                "give chainsaw",
                "give shotgun",
                "give supershotgun",
                "give rocketlauncher",
                "give plasmarifle",
                "give BFG9000",
                "give chaingun",
                "give Berserk",
                "give Allmap",
                "give Allmap",
            },
            commands);
    }

    [Fact]
    public void MapCmd_AppliesSkillMonstersAndTheRealNoMonstersBit()
    {
        var level = new VmTCmdEmulate1182Slice.Level { Dmflags = 4, SkillCount = 5 };
        var plain = new VmTCmdEmulate1182Slice.Scanner(level);
        plain.OpenMem("RUNCMD", "MAP01");
        VmTCmdEmulate1182Slice.MapCmd(level, plain);
        Assert.Equal("MAP01", level.NextMap);
        Assert.Equal(0, level.NextPosition);
        Assert.Equal(-1, level.NextSkill);
        Assert.Equal(
            VmTCmdEmulate1182Slice.ChangeLevelResetInventory | VmTCmdEmulate1182Slice.ChangeLevelResetHealth,
            level.NextFlags);

        level = new VmTCmdEmulate1182Slice.Level
        {
            Dmflags = VmTCmdEmulate1182Slice.DfNoMonsters,
            SkillCount = 5,
        };
        var sc = new VmTCmdEmulate1182Slice.Scanner(level);
        sc.OpenMem("RUNCMD", "123456789 -skill 0x10 -monsters 1 -noresetplayers");
        VmTCmdEmulate1182Slice.MapCmd(level, sc);
        Assert.Equal("12345678", level.NextMap);
        Assert.Equal(0, level.NextPosition);
        Assert.Equal(4, level.NextSkill);
        Assert.Equal(0, level.NextFlags);

        level = new VmTCmdEmulate1182Slice.Level { SkillCount = 5 };
        sc = new VmTCmdEmulate1182Slice.Scanner(level);
        sc.OpenMem("RUNCMD", "E1M1 -skill 1 -monsters 0");
        VmTCmdEmulate1182Slice.MapCmd(level, sc);
        Assert.Equal(0, level.NextSkill);
        Assert.Equal(
            VmTCmdEmulate1182Slice.ChangeLevelResetInventory
                | VmTCmdEmulate1182Slice.ChangeLevelResetHealth
                | VmTCmdEmulate1182Slice.ChangeLevelNoMonsters,
            level.NextFlags);

        level = new VmTCmdEmulate1182Slice.Level();
        sc = new VmTCmdEmulate1182Slice.Scanner(level);
        sc.OpenMem("RUNCMD", "");
        VmTCmdEmulate1182Slice.MapCmd(level, sc);
        Assert.False(level.LevelChanged);
        Assert.Contains("Missing string (unexpected end of file).", level.Errors[0]);
    }

    [Fact]
    public void EmulateCmd_CommentsBareSemicolonsAndStillHonorsAQuotedOne()
    {
        var commented = new VmTCmdEmulate1182Slice.Level();
        VmTCmdEmulate1182Slice.EmulateCmd(commented, "GIMME health ; ammo");
        Assert.Equal(new[] { "give health" }, commented.Commands);
        Assert.Equal(0f, commented.Gravity);

        var continued = new VmTCmdEmulate1182Slice.Level();
        VmTCmdEmulate1182Slice.EmulateCmd(continued, "GIMME health\nammo");
        Assert.Equal(new[] { "give health", "give ammo" }, continued.Commands);

        var quoted = new VmTCmdEmulate1182Slice.Level { Dmflags = 3 };
        VmTCmdEmulate1182Slice.EmulateCmd(quoted, "GIMME health \";\" gravity 1.5 \";\" ALLOWJUMP 0");
        Assert.Equal(new[] { "give health" }, quoted.Commands);
        Assert.Equal(1200f, quoted.Gravity);
        Assert.Equal(3 | VmTCmdEmulate1182Slice.DfNoJump, quoted.Dmflags);

        var swallowed = new VmTCmdEmulate1182Slice.Level { Dmflags = 3 };
        VmTCmdEmulate1182Slice.EmulateCmd(swallowed, "gravity 2\nALLOWJUMP 0");
        Assert.Equal(1600f, swallowed.Gravity);
        Assert.Equal(3, swallowed.Dmflags);

        var withActor = new VmTCmdEmulate1182Slice.Player { HasActor = true };
        var withoutActor = new VmTCmdEmulate1182Slice.Player();
        var view = new VmTCmdEmulate1182Slice.Level();
        view.Players.Add(withActor);
        view.Players.Add(withoutActor);
        VmTCmdEmulate1182Slice.EmulateCmd(view, "/* hide */ viewheight 41 // tail");
        Assert.Equal(41d, withActor.ViewHeight);
        Assert.Equal(41d, withActor.ActorViewHeight);
        Assert.Equal(1, withActor.UncrouchCount);
        Assert.Equal(41d, withoutActor.ViewHeight);
        Assert.Equal(0d, withoutActor.ActorViewHeight);
        Assert.Equal(1, withoutActor.UncrouchCount);

        var fog = new VmTCmdEmulate1182Slice.Level();
        VmTCmdEmulate1182Slice.EmulateCmd(fog, "gr_fogdensity 399\ngravity 1.5\ngr_fogcolor 12zz");
        Assert.Equal(69, fog.FogDensity);
        Assert.Equal(0, fog.OutsideFogDensity);
        Assert.Equal(0, fog.SkyFog);
        Assert.Equal(0, fog.OutsideFog);
        Assert.Equal(1200f, fog.Gravity);
        Assert.Equal(0u, fog.FadeTo);

        var color = new VmTCmdEmulate1182Slice.Level();
        VmTCmdEmulate1182Slice.EmulateCmd(color, "gr_fogcolor FF");
        Assert.Equal(255u, color.FadeTo);
        Assert.Equal(16u, VmTCmdEmulate1182Slice.ParseHexUnsigned("0x10"));
        Assert.Equal(0u, VmTCmdEmulate1182Slice.ParseHexUnsigned("zz"));

        var map = new VmTCmdEmulate1182Slice.Level();
        VmTCmdEmulate1182Slice.EmulateCmd(map, "map E1M1\ngravity 9");
        Assert.True(map.LevelChanged);
        Assert.Equal("E1M1", map.NextMap);
        Assert.Equal(0, map.NextPosition);
        Assert.Equal(-1, map.NextSkill);
        Assert.Equal(0f, map.Gravity);

        var bad = new VmTCmdEmulate1182Slice.Level();
        VmTCmdEmulate1182Slice.EmulateCmd(bad, "gravity nope\ngr_fogdensity 400");
        Assert.Equal(0f, bad.Gravity);
        Assert.Equal(0, bad.FogDensity);
        Assert.Contains("SC_GetFloat: Bad numeric constant \"nope\".", bad.Errors[0]);
        Assert.Contains("RUNCMD", bad.Errors[0]);
    }
}
