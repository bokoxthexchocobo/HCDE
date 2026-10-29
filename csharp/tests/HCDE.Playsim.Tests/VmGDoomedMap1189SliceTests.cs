namespace HCDE.Playsim.Tests;

public class VmGDoomedMap1189SliceTests
{
    static string ScriptMessage(int line, string message)
    {
        return "\u001cGScript error, \"MAPINFO\"\u001cG line " + line + ":\n\u001cG" + message + "\n";
    }

    static string UnknownClass(string file, int line, string cls)
    {
        return "\u001cGScript error, \"" + file + "\" line " + line + ":\nUnknown actor class " + cls + "\n";
    }

    static string Fault(int line, string message)
    {
        return "Script error, \"MAPINFO\" line " + line + ":\n" + message + "\n";
    }

    static VmGDoomedMap1189Slice.Parser Open(string script, int format)
    {
        var parser = new VmGDoomedMap1189Slice.Parser { Format = format };
        parser.Scanner.Open("MAPINFO", script);
        return parser;
    }

    [Fact]
    public void InitActorNums_CopiesArgsAndAbortsWithoutClearingTheSource()
    {
        VmGDoomedMap1189Slice.Clear();
        VmGDoomedMap1189Slice.Actors["ZombieMan"] = "ZombieMan";
        VmGDoomedMap1189Slice.DoomEdMap[99] = new VmGDoomedMap1189Slice.DoomEdEntry { TypeName = "Old" };
        VmGDoomedMap1189Slice.DoomEdFromMapinfo[3001] = new VmGDoomedMap1189Slice.EdItem
        {
            ClassName = "ZombieMan",
            Special = 70000,
            ArgsDefined = 200,
            NoSkillFlags = true,
            Args = new[] { 9, 8, 7, 6, 5 },
            FileName = "DOOMED",
            LineNum = 12
        };
        VmGDoomedMap1189Slice.DoomEdFromMapinfo[3002] = new VmGDoomedMap1189Slice.EdItem
        {
            ClassName = "",
            Special = 1,
            Args = new[] { 4, 0, 0, 0, 0 },
            FileName = "DOOMED",
            LineNum = 13
        };
        VmGDoomedMap1189Slice.DoomEdFromMapinfo[3003] = new VmGDoomedMap1189Slice.EdItem
        {
            ClassName = "Missing",
            Args = new int[5],
            FileName = "DOOMED",
            LineNum = 40
        };

        var ex = Assert.Throws<VmGDoomedMap1189Slice.FatalError>(() => VmGDoomedMap1189Slice.InitActorNumsFromMapinfo());
        Assert.Equal("1 unknown actor classes found", ex.Detail);
        Assert.False(VmGDoomedMap1189Slice.DoomEdMap.ContainsKey(99));
        var copied = VmGDoomedMap1189Slice.DoomEdMap[3001];
        Assert.Equal("ZombieMan", copied.TypeName);
        Assert.Equal(4464, copied.Special);
        Assert.Equal(-56, copied.ArgsDefined);
        Assert.True(copied.NoSkillFlags);
        Assert.Equal(new[] { 9, 8, 7, 6, 5 }, copied.Args);
        Assert.Null(VmGDoomedMap1189Slice.DoomEdMap[3002].TypeName);
        Assert.Equal(1, VmGDoomedMap1189Slice.DoomEdMap[3002].Special);
        Assert.Null(VmGDoomedMap1189Slice.DoomEdMap[3003].TypeName);
        Assert.True(VmGDoomedMap1189Slice.DoomEdFromMapinfo.ContainsKey(3003));
        Assert.Equal(UnknownClass("DOOMED", 40, "Missing"), VmGDoomedMap1189Slice.Messages[0]);
        VmGDoomedMap1189Slice.DoomEdFromMapinfo[3001].Args[0] = 123;
        Assert.Equal(9, copied.Args[0]);

        VmGDoomedMap1189Slice.Clear();
        VmGDoomedMap1189Slice.Actors["ZombieMan"] = "ZombieMan";
        VmGDoomedMap1189Slice.DoomEdFromMapinfo[7] = new VmGDoomedMap1189Slice.EdItem
        {
            ClassName = "zombieman",
            Args = new int[5],
            FileName = "OK",
            LineNum = 1
        };
        VmGDoomedMap1189Slice.InitActorNumsFromMapinfo();
        Assert.Empty(VmGDoomedMap1189Slice.DoomEdFromMapinfo);
        Assert.Equal("ZombieMan", VmGDoomedMap1189Slice.DoomEdMap[7].TypeName);
        Assert.Empty(VmGDoomedMap1189Slice.Messages);

        VmGDoomedMap1189Slice.DoomEdFromMapinfo[1] = new VmGDoomedMap1189Slice.EdItem
        {
            ClassName = "None",
            Args = new int[5],
            FileName = "X",
            LineNum = 2
        };
        VmGDoomedMap1189Slice.InitActorNumsFromMapinfo();
        Assert.Null(VmGDoomedMap1189Slice.DoomEdMap[1].TypeName);
        Assert.False(VmGDoomedMap1189Slice.DoomEdMap.ContainsKey(7));
        Assert.Empty(VmGDoomedMap1189Slice.DoomEdFromMapinfo);
        Assert.Empty(VmGDoomedMap1189Slice.Messages);
    }

    [Fact]
    public void GetSpawnableType_UsesNameIndexesAndSpawnNumbers()
    {
        VmGDoomedMap1189Slice.Clear();
        VmGDoomedMap1189Slice.Names.Add("None");
        VmGDoomedMap1189Slice.Names.Add("DoomImp");
        VmGDoomedMap1189Slice.Names.Add("MissingName");
        VmGDoomedMap1189Slice.Actors["DoomImp"] = "DoomImp";
        VmGDoomedMap1189Slice.Actors["ZombieMan"] = "ZombieMan";
        Assert.Equal("DoomImp", VmGDoomedMap1189Slice.GetSpawnableType(-1));
        Assert.Null(VmGDoomedMap1189Slice.GetSpawnableType(-2));
        VmGDoomedMap1189Slice.SpawnableThings[99] = "Nope";
        VmGDoomedMap1189Slice.SpawnableThings[-99] = "Nope";
        Assert.Null(VmGDoomedMap1189Slice.GetSpawnableType(-99));
        Assert.Equal("Nope", VmGDoomedMap1189Slice.SpawnableThings[99]);

        VmGDoomedMap1189Slice.SpawnableThings.Clear();
        Assert.Null(VmGDoomedMap1189Slice.GetSpawnableType(0));
        VmGDoomedMap1189Slice.SpawnableThings[5] = "ZombieMan";
        VmGDoomedMap1189Slice.SpawnableThings[7] = null;
        Assert.Equal("ZombieMan", VmGDoomedMap1189Slice.GetSpawnableType(5));
        Assert.Null(VmGDoomedMap1189Slice.GetSpawnableType(7));
        Assert.True(VmGDoomedMap1189Slice.SpawnableThings.ContainsKey(7));

        VmGDoomedMap1189Slice.SpawnableThings.Clear();
        VmGDoomedMap1189Slice.SpawnableThings[10] = "Cyberdemon";
        VmGDoomedMap1189Slice.SpawnableThings[2] = "ZombieMan";
        Assert.Equal("2 ZombieMan\n10 Cyberdemon\n", VmGDoomedMap1189Slice.DumpSpawnables());
        VmGDoomedMap1189Slice.SpawnableThings.Clear();
        VmGDoomedMap1189Slice.Messages.Clear();
        Assert.Equal("", VmGDoomedMap1189Slice.DumpSpawnables());
        VmGDoomedMap1189Slice.StrifeTypes[3] = "Acolyte";
        VmGDoomedMap1189Slice.SpawnableThings[2] = "ZombieMan";
        Assert.Equal("3 Acolyte\n", VmGDoomedMap1189Slice.DumpConversationIds());
        VmGDoomedMap1189Slice.ClearStrifeTypes();
        Assert.Empty(VmGDoomedMap1189Slice.StrifeTypes);
        Assert.Equal("ZombieMan", VmGDoomedMap1189Slice.SpawnableThings[2]);
    }

    [Fact]
    public void ParseSpawnMap_WarnsAndStillInsertsUntilTheBrace()
    {
        VmGDoomedMap1189Slice.Clear();
        var oldBrace = Open("{", VmGDoomedMap1189Slice.FmtOld);
        var number = Assert.Throws<VmGDoomedMap1189Slice.ScriptFault>(() => VmGDoomedMap1189Slice.ParseSpawnNums(oldBrace));
        Assert.Equal(Fault(1, "Number expected"), number.Detail);

        VmGDoomedMap1189Slice.Clear();
        var needBrace = Open("1 Foo\n}", VmGDoomedMap1189Slice.FmtNew);
        var expected = Assert.Throws<VmGDoomedMap1189Slice.ScriptFault>(() => VmGDoomedMap1189Slice.ParseSpawnNums(needBrace));
        Assert.Equal(Fault(1, "Expected '{', got '1'."), expected.Detail);

        VmGDoomedMap1189Slice.Clear();
        var immediate = Open("}", VmGDoomedMap1189Slice.FmtUnknown);
        VmGDoomedMap1189Slice.ParseSpawnNums(immediate);
        Assert.Empty(VmGDoomedMap1189Slice.SpawnablesFromMapinfo);
        Assert.Equal(VmGDoomedMap1189Slice.FmtOld, immediate.Format);

        VmGDoomedMap1189Slice.Clear();
        var braces = Open("{}", VmGDoomedMap1189Slice.FmtUnknown);
        VmGDoomedMap1189Slice.ParseSpawnNums(braces);
        Assert.Empty(VmGDoomedMap1189Slice.SpawnablesFromMapinfo);
        Assert.Equal(VmGDoomedMap1189Slice.FmtNew, braces.Format);
        Assert.True(braces.Scanner.CMode);

        VmGDoomedMap1189Slice.Clear();
        var classic = Open("1 = Foo ; junk\n2 = Bar // zz\n}", VmGDoomedMap1189Slice.FmtOld);
        VmGDoomedMap1189Slice.ParseSpawnNums(classic);
        Assert.Equal("Foo", VmGDoomedMap1189Slice.SpawnablesFromMapinfo[1].ClassName);
        Assert.Equal(1, VmGDoomedMap1189Slice.SpawnablesFromMapinfo[1].LineNum);
        Assert.Equal("Bar", VmGDoomedMap1189Slice.SpawnablesFromMapinfo[2].ClassName);
        Assert.Equal(2, VmGDoomedMap1189Slice.SpawnablesFromMapinfo[2].LineNum);
        Assert.Equal("MAPINFO", VmGDoomedMap1189Slice.SpawnablesFromMapinfo[1].FileName);
        Assert.Empty(VmGDoomedMap1189Slice.Messages);
        Assert.False(classic.Scanner.CMode);

        VmGDoomedMap1189Slice.Clear();
        var semi = Open("{ 1 = Foo;\n}", VmGDoomedMap1189Slice.FmtUnknown);
        var semiFault = Assert.Throws<VmGDoomedMap1189Slice.ScriptFault>(() => VmGDoomedMap1189Slice.ParseSpawnNums(semi));
        Assert.Equal(Fault(1, "Number expected"), semiFault.Detail);
        Assert.Equal("Foo", VmGDoomedMap1189Slice.SpawnablesFromMapinfo[1].ClassName);
        Assert.Equal(1, VmGDoomedMap1189Slice.SpawnablesFromMapinfo[1].LineNum);
        Assert.Equal(VmGDoomedMap1189Slice.FmtNew, semi.Format);
        Assert.True(semi.Scanner.CMode);

        VmGDoomedMap1189Slice.Clear();
        var numbers = Open("0x10 = Imp\n010 = Oct\nMAXINT = Big\n}", VmGDoomedMap1189Slice.FmtUnknown);
        VmGDoomedMap1189Slice.ParseSpawnNums(numbers);
        Assert.Equal("Imp", VmGDoomedMap1189Slice.SpawnablesFromMapinfo[16].ClassName);
        Assert.Equal("Oct", VmGDoomedMap1189Slice.SpawnablesFromMapinfo[8].ClassName);
        Assert.Equal("Big", VmGDoomedMap1189Slice.SpawnablesFromMapinfo[int.MaxValue].ClassName);
        Assert.Equal(VmGDoomedMap1189Slice.FmtOld, numbers.Format);

        VmGDoomedMap1189Slice.Clear();
        var talk = Open("{\n4 = Rebel\n}", VmGDoomedMap1189Slice.FmtUnknown);
        VmGDoomedMap1189Slice.ParseConversationIDs(talk);
        Assert.Equal("Rebel", VmGDoomedMap1189Slice.ConversationIDsFromMapinfo[4].ClassName);
        Assert.Empty(VmGDoomedMap1189Slice.SpawnablesFromMapinfo);

        VmGDoomedMap1189Slice.Clear();
        VmGDoomedMap1189Slice.Actors["ZombieMan"] = "ZombieMan";
        var none = Open("{\n3 = none\n4 = ZombieMan\n}", VmGDoomedMap1189Slice.FmtUnknown);
        VmGDoomedMap1189Slice.ParseSpawnNums(none);
        Assert.Equal("none", VmGDoomedMap1189Slice.SpawnablesFromMapinfo[3].ClassName);
        VmGDoomedMap1189Slice.InitSpawnablesFromMapinfo();
        Assert.False(VmGDoomedMap1189Slice.SpawnableThings.ContainsKey(3));
        Assert.Equal("ZombieMan", VmGDoomedMap1189Slice.SpawnableThings[4]);
        Assert.Empty(VmGDoomedMap1189Slice.SpawnablesFromMapinfo);
        Assert.Empty(VmGDoomedMap1189Slice.Messages);

        VmGDoomedMap1189Slice.Clear();
        var parser = Open("{\n1 = ZombieMan\n1 = DoomImp\n-3 = Negative\n2 = Arachnotron\n}", VmGDoomedMap1189Slice.FmtUnknown);
        VmGDoomedMap1189Slice.ParseSpawnNums(parser);
        Assert.Equal(VmGDoomedMap1189Slice.FmtNew, parser.Format);
        Assert.False(parser.Scanner.ParseError);
        Assert.Equal("DoomImp", VmGDoomedMap1189Slice.SpawnablesFromMapinfo[1].ClassName);
        Assert.Equal(3, VmGDoomedMap1189Slice.SpawnablesFromMapinfo[1].LineNum);
        Assert.Equal("Negative", VmGDoomedMap1189Slice.SpawnablesFromMapinfo[-3].ClassName);
        Assert.Equal(4, VmGDoomedMap1189Slice.SpawnablesFromMapinfo[-3].LineNum);
        Assert.Equal("Arachnotron", VmGDoomedMap1189Slice.SpawnablesFromMapinfo[2].ClassName);
        Assert.Equal(5, VmGDoomedMap1189Slice.SpawnablesFromMapinfo[2].LineNum);
        Assert.Equal("MAPINFO", VmGDoomedMap1189Slice.SpawnablesFromMapinfo[2].FileName);
        Assert.False(VmGDoomedMap1189Slice.SpawnablesFromMapinfo.ContainsKey(0));
        string warnings = ScriptMessage(3, "Spawn number 1 defined more than once")
            + ScriptMessage(4, "Spawn number must be positive, got -3");
        Assert.Equal(warnings, string.Concat(VmGDoomedMap1189Slice.Messages));
        Assert.DoesNotContain("errors encountered", string.Concat(VmGDoomedMap1189Slice.Messages));

        VmGDoomedMap1189Slice.Actors["DoomImp"] = "DoomImp";
        VmGDoomedMap1189Slice.Actors["Negative"] = "Negative";
        VmGDoomedMap1189Slice.StrifeTypes[8] = "Keep";
        VmGDoomedMap1189Slice.ConversationIDsFromMapinfo[4] = new VmGDoomedMap1189Slice.SpawnItem
        {
            ClassName = "Rebel",
            FileName = "C",
            LineNum = 1
        };
        var fatal = Assert.Throws<VmGDoomedMap1189Slice.FatalError>(() => VmGDoomedMap1189Slice.InitSpawnablesFromMapinfo());
        Assert.Equal("1 unknown actor classes found", fatal.Detail);
        Assert.Equal("DoomImp", VmGDoomedMap1189Slice.SpawnableThings[1]);
        Assert.Equal("Negative", VmGDoomedMap1189Slice.SpawnableThings[-3]);
        Assert.Null(VmGDoomedMap1189Slice.SpawnableThings[2]);
        Assert.True(VmGDoomedMap1189Slice.SpawnableThings.ContainsKey(2));
        Assert.True(VmGDoomedMap1189Slice.SpawnablesFromMapinfo.ContainsKey(2));
        Assert.Equal("Keep", VmGDoomedMap1189Slice.StrifeTypes[8]);
        Assert.Equal("Rebel", VmGDoomedMap1189Slice.ConversationIDsFromMapinfo[4].ClassName);
        Assert.Equal(UnknownClass("MAPINFO", 5, "Arachnotron"), VmGDoomedMap1189Slice.Messages[2]);

        VmGDoomedMap1189Slice.Actors["Arachnotron"] = "Arachnotron";
        VmGDoomedMap1189Slice.Actors["Rebel"] = "Rebel";
        VmGDoomedMap1189Slice.InitSpawnablesFromMapinfo();
        Assert.Empty(VmGDoomedMap1189Slice.SpawnablesFromMapinfo);
        Assert.Empty(VmGDoomedMap1189Slice.ConversationIDsFromMapinfo);
        Assert.Equal("Arachnotron", VmGDoomedMap1189Slice.SpawnableThings[2]);
        Assert.Equal("Rebel", VmGDoomedMap1189Slice.StrifeTypes[4]);
        Assert.False(VmGDoomedMap1189Slice.StrifeTypes.ContainsKey(8));
    }
}
