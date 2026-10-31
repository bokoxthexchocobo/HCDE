using HCDE.Gamedata;

namespace HCDE.Gamedata.Tests;

public class VmKeySections1360SliceTests
{
    [Fact]
    public void CommandsAndPlayerClasses()
    {
        var slice = new VmKeySections1360Slice();
        Assert.Equal(-1, slice.CurrentKeySection);
        Assert.Equal(1, VmKeySections1360Slice.PcfNoMenu);

        slice.AddKeySection("addkeysection", "Menu", "ini");
        slice.AddMenuKey("addmenukey", "desc", "cmd");
        slice.AddPlayerClass("addplayerclass", "DoomPlayer", "nomenu");
        Assert.Empty(slice.KeySections);
        Assert.Empty(slice.Messages);
        Assert.Empty(slice.PlayerClasses);

        slice.ParsingKeyConf = true;
        slice.AddKeySection("addkeysection");
        slice.AddKeySection("addkeysection", "only");
        slice.AddKeySection("addkeysection", "a", "b", "c");
        Assert.Equal(3, slice.Messages.Count);
        Assert.All(slice.Messages, line =>
            Assert.Equal("Usage: addkeysection <menu section name> <ini name>\n", line));

        var loads = new List<(string Name, bool Double)>();
        slice.LoadKeys = (name, dbl) => loads.Add((name, dbl));
        slice.Messages.Clear();
        slice.AddKeySection("addkeysection", "keys", "weapons");
        Assert.Equal(0, slice.CurrentKeySection);
        Assert.Equal("keys", slice.KeySections[0].Title);
        Assert.Equal("weapons", slice.KeySections[0].Ini);
        Assert.Equal(new[] { ("weapons", false), ("weapons", true) }, loads);

        loads.Clear();
        slice.AddKeySection("addkeysection", "Other", "KEYS");
        Assert.Equal(0, slice.CurrentKeySection);
        Assert.Single(slice.KeySections);
        Assert.Empty(loads);

        loads.Clear();
        slice.AddKeySection("addkeysection", "keys", "other");
        Assert.Equal(1, slice.CurrentKeySection);
        Assert.Equal("keys", slice.KeySections[1].Title);
        Assert.Equal("other", slice.KeySections[1].Ini);
        Assert.Equal(new[] { ("other", false), ("other", true) }, loads);

        var exact = new string('e', 32);
        slice.Messages.Clear();
        loads.Clear();
        slice.AddKeySection("addkeysection", "Exact", exact);
        Assert.Empty(slice.Messages);
        Assert.Equal(exact, slice.KeySections[2].Ini);
        Assert.Equal(new[] { (exact, false), (exact, true) }, loads);

        var longName = new string('n', 33);
        slice.Messages.Clear();
        loads.Clear();
        slice.AddKeySection("addkeysection", "LongMenu", longName);
        Assert.Equal(
            $"WARNING: {longName} is too long as an ini name! The ini name should be 32 bytes or less.\n",
            Assert.Single(slice.Messages));
        Assert.Equal("LongMenu", slice.KeySections[3].Title);
        Assert.Equal(longName, slice.KeySections[3].Ini);
        Assert.Equal(new[] { (longName, false), (longName, true) }, loads);

        slice.KeySections[0].Title = new string('n', 32);
        var longer = new string('n', 40);
        slice.Messages.Clear();
        loads.Clear();
        var before = slice.KeySections.Count;
        slice.AddKeySection("addkeysection", "Nope", longer);
        Assert.Equal(
            $"WARNING: {longer} is too long as an ini name! The ini name should be 32 bytes or less.\n",
            Assert.Single(slice.Messages));
        Assert.Equal(0, slice.CurrentKeySection);
        Assert.Equal(before, slice.KeySections.Count);
        Assert.Empty(loads);

        slice.LoadKeys = null;
        var added = Assert.Throws<InvalidOperationException>(() =>
            slice.AddKeySection("addkeysection", "T", "fresh"));
        Assert.Equal("LoadKeys", added.Message);
        Assert.Equal("T", slice.KeySections[^1].Title);
        Assert.Equal("fresh", slice.KeySections[^1].Ini);
        Assert.Equal(slice.KeySections.Count - 1, slice.CurrentKeySection);

        var calls = 0;
        slice.LoadKeys = (_, dbl) =>
        {
            calls++;
            if (!dbl)
                throw new InvalidOperationException("dbl0");
        };
        var second = Assert.Throws<InvalidOperationException>(() =>
            slice.AddKeySection("addkeysection", "M", "sec"));
        Assert.Equal("dbl0", second.Message);
        Assert.Equal(1, calls);
        Assert.Equal("sec", slice.KeySections[^1].Ini);

        slice.ParsingKeyConf = false;
        slice.CurrentKeySection = -2;
        slice.Messages.Clear();
        slice.AddMenuKey("addmenukey", "d", "c");
        Assert.Empty(slice.Messages);

        slice.ParsingKeyConf = true;
        slice.AddMenuKey("addmenukey");
        Assert.Equal("Usage: addmenukey <description> <command>\n", Assert.Single(slice.Messages));

        slice.Messages.Clear();
        slice.CurrentKeySection = -1;
        slice.AddMenuKey("addmenukey", "d", "c");
        slice.CurrentKeySection = slice.KeySections.Count;
        slice.AddMenuKey("addmenukey", "d", "c");
        Assert.Equal(2, slice.Messages.Count);
        Assert.All(slice.Messages, line => Assert.Equal("You must use addkeysection first.\n", line));

        slice.CurrentKeySection = -2;
        var missing = Assert.Throws<IndexOutOfRangeException>(() =>
            slice.AddMenuKey("addmenukey", "d", "c"));
        Assert.Equal("KeySections", missing.Message);
        Assert.Empty(slice.KeySections[0].Actions);

        slice.CurrentKeySection = 1;
        slice.AddMenuKey("addmenukey", "  desc  ", "cmd");
        slice.AddMenuKey("addmenukey", "next", "use");
        Assert.Equal("  desc  ", slice.KeySections[1].Actions[0].Title);
        Assert.Equal("cmd", slice.KeySections[1].Actions[0].Command);
        Assert.Equal("use", slice.KeySections[1].Actions[1].Command);

        var a = new object();
        var b = new object();
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = a, Flags = 1 });
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = a, Flags = 2 });
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = b, Flags = 3 });
        slice.ClearIwadPlayerClasses(a);
        Assert.Equal(2, slice.PlayerClasses.Count);
        Assert.Same(a, slice.PlayerClasses[0].Type);
        Assert.Equal(2, slice.PlayerClasses[0].Flags);
        Assert.Same(b, slice.PlayerClasses[1].Type);
        Assert.Equal(3, slice.PlayerClasses[1].Flags);

        slice.PlayerClasses.Clear();
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = a, Flags = 1 });
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = a, Flags = 2 });
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = a, Flags = 3 });
        slice.ClearIwadPlayerClasses(a);
        var left = Assert.Single(slice.PlayerClasses);
        Assert.Same(a, left.Type);
        Assert.Equal(2, left.Flags);

        slice.PlayerClasses.Clear();
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = a, Flags = 1 });
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = b, Flags = 2 });
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = a, Flags = 3 });
        slice.ClearIwadPlayerClasses(a);
        Assert.Same(b, Assert.Single(slice.PlayerClasses).Type);

        slice.PlayerClasses.Clear();
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = null, Flags = 4 });
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = b, Flags = 5 });
        slice.ClearIwadPlayerClasses(null);
        Assert.Same(b, Assert.Single(slice.PlayerClasses).Type);
        Assert.Equal(5, slice.PlayerClasses[0].Flags);

        slice.PlayerClasses.Clear();
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = a, Flags = 9 });
        slice.ClearIwadPlayerClasses(a);
        Assert.Empty(slice.PlayerClasses);

        var finds = new List<string>();
        slice.ParsingKeyConf = false;
        slice.SetSlotStrict = true;
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = b, Flags = 1 });
        slice.FindActor = name =>
        {
            finds.Add(name);
            return null;
        };
        slice.ClearPlayerClasses();
        Assert.Empty(finds);
        Assert.Single(slice.PlayerClasses);

        slice.ParsingKeyConf = true;
        slice.ClearPlayerClasses();
        Assert.Empty(slice.PlayerClasses);
        Assert.Empty(finds);

        var actors = new Dictionary<string, object?>
        {
            ["DoomPlayer"] = new object(),
            ["HereticPlayer"] = new object(),
            ["StrifePlayer"] = new object(),
            ["FighterPlayer"] = new object(),
            ["ClericPlayer"] = new object(),
            ["MagePlayer"] = new object(),
            ["ChexPlayer"] = new object(),
        };
        var custom = new object();
        slice.SetSlotStrict = false;
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = actors["DoomPlayer"], Flags = 1 });
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = actors["HereticPlayer"], Flags = 1 });
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = custom, Flags = 7 });
        slice.FindActor = name =>
        {
            finds.Add(name);
            return actors[name];
        };
        slice.ClearPlayerClasses();
        Assert.Equal(
            new[]
            {
                "DoomPlayer", "HereticPlayer", "StrifePlayer", "FighterPlayer",
                "ClericPlayer", "MagePlayer", "ChexPlayer",
            },
            finds);
        Assert.Same(custom, Assert.Single(slice.PlayerClasses).Type);
        Assert.Equal(7, slice.PlayerClasses[0].Flags);

        slice.PlayerClasses.Clear();
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = null, Flags = 1 });
        slice.PlayerClasses.Add(new VmKeySections1360Slice.PlayerClass { Type = custom, Flags = 2 });
        finds.Clear();
        slice.FindActor = name =>
        {
            finds.Add(name);
            return null;
        };
        slice.ClearPlayerClasses();
        Assert.Equal(7, finds.Count);
        Assert.Same(custom, Assert.Single(slice.PlayerClasses).Type);

        slice.FindActor = null;
        var noFind = Assert.Throws<InvalidOperationException>(() => slice.ClearPlayerClasses());
        Assert.Equal("FindActor", noFind.Message);

        slice.Messages.Clear();
        slice.PlayerClasses.Clear();
        slice.ParsingKeyConf = false;
        slice.FindActor = _ => throw new InvalidOperationException("FindActor");
        slice.AddPlayerClass("addplayerclass", "DoomPlayer", "nomenu");
        slice.ParsingKeyConf = true;
        slice.AddPlayerClass("addplayerclass");
        Assert.Empty(slice.Messages);

        var zombie = new object();
        var seen = new List<(object? Type, string Name)>();
        slice.FindActor = name =>
        {
            finds.Add(name);
            return name == "Zombie" ? zombie : null;
        };
        slice.ValidatePlayerClass = (ti, name) =>
        {
            seen.Add((ti, name));
            return false;
        };
        finds.Clear();
        slice.AddPlayerClass("addplayerclass", "Zombie", "nomenu");
        Assert.Equal("Zombie", Assert.Single(finds));
        Assert.Same(zombie, Assert.Single(seen).Type);
        Assert.Empty(slice.PlayerClasses);
        Assert.Empty(slice.Messages);

        slice.ValidatePlayerClass = null;
        var noValidate = Assert.Throws<InvalidOperationException>(() =>
            slice.AddPlayerClass("addplayerclass", "Zombie"));
        Assert.Equal("ValidatePlayerClass", noValidate.Message);
        Assert.Empty(slice.PlayerClasses);

        slice.ValidatePlayerClass = (_, _) => true;
        slice.Messages.Clear();
        slice.AddPlayerClass("addplayerclass", "Zombie", "NoMenu");
        var first = Assert.Single(slice.PlayerClasses);
        Assert.Same(zombie, first.Type);
        Assert.Equal(VmKeySections1360Slice.PcfNoMenu, first.Flags);
        Assert.Empty(slice.Messages);

        slice.AddPlayerClass("addplayerclass", "Zombie", "nope", "nomenu");
        Assert.Empty(slice.Messages);
        Assert.Single(slice.PlayerClasses);
        Assert.Equal(VmKeySections1360Slice.PcfNoMenu, slice.PlayerClasses[0].Flags);

        slice.AddPlayerClass("addplayerclass", "Other", "weird", "NOMENU", "");
        Assert.Equal(
            new[]
            {
                "Unknown flag 'weird' for player class 'Other'\n",
                "Unknown flag '' for player class 'Other'\n",
            },
            slice.Messages);
        Assert.Equal(2, slice.PlayerClasses.Count);
        Assert.Null(slice.PlayerClasses[1].Type);
        Assert.Equal(VmKeySections1360Slice.PcfNoMenu, slice.PlayerClasses[1].Flags);

        slice.Messages.Clear();
        var held = slice.PlayerClasses.Count;
        slice.AddPlayerClass("addplayerclass", "AlsoMissing", "nomenu2");
        Assert.Empty(slice.Messages);
        Assert.Equal(held, slice.PlayerClasses.Count);

        var plain = new object();
        slice.FindActor = name => name == "Plain" ? plain : null;
        slice.AddPlayerClass("addplayerclass", "Plain", "nomenu2");
        Assert.Equal("Unknown flag 'nomenu2' for player class 'Plain'\n", Assert.Single(slice.Messages));
        Assert.Same(plain, slice.PlayerClasses[2].Type);
        Assert.Equal(0, slice.PlayerClasses[2].Flags);
    }

    [Fact]
    public void KeyConfLines()
    {
        var slice = new VmKeySections1360Slice();
        slice.CurrentKeySection = 4;
        slice.ParsingKeyConf = false;
        slice.KeySections.Add(new VmKeySections1360Slice.KeySection { Title = "gone", Ini = "gone" });
        slice.KeyConfWeapons.Add("fist");
        slice.LoadWadSettings(
        [
            "a\n\nb",
            "\n\nb",
            "a\n",
            "a",
            "",
            "\n\n",
            "a\r\nb",
            "a\n\n\nb",
            "a\n\n",
            "  a  ",
            "http://x",
            "https://x",
            "a // c",
            "// only",
            "a/*b*/c",
            "say \" // x\"",
            "/*\n*/\nb",
            "start /*\nhide\n*/end",
            "/*",
            "*/no",
            "\"\n//x",
            "a/\n/b",
            "*//",
            "a/*/b\nok",
            "*/z",
            "/* visible */ yes",
            "say \"/*\" tail",
        ]);

        Assert.False(slice.ParsingKeyConf);
        Assert.Equal(4, slice.CurrentKeySection);
        Assert.Empty(slice.KeySections);
        Assert.Empty(slice.KeyConfWeapons);
        Assert.Equal(
            new[]
            {
                "a", "b",
                "", "b",
                "a",
                "a",
                "",
                "a", "b",
                "a", "b",
                "a",
                "  a  ",
                "http:",
                "https:",
                "a ",
                "",
                "ac",
                "say \" // x\"",
                "", "", "b",
                "start ", "", "end",
                "",
                "*/no",
                "\"", "",
                "a/", "/b",
                "*",
                "a", "",
                "*/z",
                " yes",
                "say \"/*\" tail",
            },
            slice.Submitted);
    }
}
