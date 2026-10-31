using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmStatePointer1350SliceTests
{
    [Fact]
    public void ProtocolAndLookup()
    {
        Assert.True(VmStatePointer1350.SameName("NextState", "nExTsTaTe"));
        Assert.False(VmStatePointer1350.SameName("Frame", "Frames"));
        Assert.False(VmStatePointer1350.SameName("A", "b"));

        var queries = new StateQueries1350();
        var blank = new VmStatePointer1350("ActorState", new VmRef1350(), new object(), queries) { Id = 9 };
        var shown = new Var1350 { EvaluateName = "keep", IndexedVariables = 4, NamedVariables = 9 };
        Assert.True(blank.Serialize(shown));
        Assert.Equal("<NULL>", shown.Value);
        Assert.Equal(0, shown.VariablesReference);
        Assert.Equal(12, shown.NamedVariables);
        Assert.Equal(4, shown.IndexedVariables);
        Assert.Equal("keep", shown.EvaluateName);
        Assert.Equal("ActorState", shown.Name);
        Assert.Equal("StatePointer", shown.Type);

        var low = new State1350 { PointerLow = 0, Sprite = 3 };
        var hidden = new VmStatePointer1350("Hidden", VmRef1350.FromState(low), null, queries) { Id = 9 };
        shown = new Var1350 { IndexedVariables = 2 };
        Assert.True(hidden.Serialize(shown));
        Assert.Equal("<NULL>", shown.Value);
        Assert.Equal(0, shown.VariablesReference);
        Assert.Null(shown.EvaluateName);

        Node1350? sentinel = new Node1350 { Id = 99 };
        Assert.False(hidden.GetChildNode("sprite", ref sentinel));
        Assert.Equal(99, sentinel!.Id);
        Assert.False(blank.GetChildNode("NextState", ref sentinel));
        Assert.Equal(99, sentinel!.Id);

        var state = new State1350
        {
            Sprite = -3,
            Tics = -4,
            TicRange = 40000,
            Light = -2,
            StateFlags = 0,
            Frame = 200,
            UseFlags = 15,
            DefineFlags = 2,
            Misc1 = int.MinValue,
            Misc2 = 8,
            DehIndex = 12,
        };
        var other = new State1350 { Sprite = 1 };
        var owner = new StateOwner1350
        {
            TypeName = "DoomImp",
            Labels = new StateLabelList1350
            {
                NumLabels = 2,
                Items =
                {
                    new StateLabel1350 { Name = "Missile", State = state },
                    new StateLabel1350 { Name = "Spawn", State = state },
                    new StateLabel1350 { Name = "Ignored", State = state },
                },
            },
        };
        int nameCalls = 0;
        queries.FindOwner = s => ReferenceEquals(s, state) ? owner : null;
        queries.StateName = s =>
        {
            nameCalls++;
            return "STATENAME";
        };
        var owning = new object();
        var node = new VmStatePointer1350("S", VmRef1350.FromState(state), owning, queries) { Id = 7, EvalName = "E" };
        shown = new Var1350 { EvaluateName = "keep", IndexedVariables = 4, NamedVariables = 1 };
        Assert.True(node.Serialize(shown));
        Assert.Equal("DoomImp.Missile", shown.Value);
        Assert.Equal(7, shown.VariablesReference);
        Assert.Equal(12, shown.NamedVariables);
        Assert.Equal(4, shown.IndexedVariables);
        Assert.Equal("E", shown.EvaluateName);
        Assert.Equal("S", shown.Name);
        Assert.Equal(0, nameCalls);

        owner.Labels.NumLabels = 0;
        Assert.True(node.Serialize(shown));
        Assert.Equal("STATENAME", shown.Value);
        Assert.Equal(1, nameCalls);

        owner.Labels.NumLabels = 1;
        owner.Labels.Items[0].Name = "None";
        owner.Labels.Items[0].State = state;
        Assert.True(node.Serialize(shown));
        Assert.Equal("STATENAME", shown.Value);

        owner.Labels.Items[0].Name = "See";
        owner.Labels.Items[0].State = other;
        Assert.True(node.Serialize(shown));
        Assert.Equal("STATENAME", shown.Value);

        queries.FindOwner = _ => null;
        Assert.True(node.Serialize(shown));
        Assert.Equal("STATENAME", shown.Value);

        queries.FindOwner = null;
        var missing = Assert.Throws<InvalidOperationException>(() => node.Serialize(new Var1350()));
        Assert.Equal("StaticFindStateOwner", missing.Message);
        queries.FindOwner = _ => null;
        queries.StateName = null;
        missing = Assert.Throws<InvalidOperationException>(() => node.Serialize(new Var1350()));
        Assert.Equal("StaticGetStateName", missing.Message);

        var names = new List<string> { "kept" };
        Assert.True(node.GetChildNames(names));
        Assert.Equal(13, names.Count);
        Assert.Equal("kept", names[0]);
        Assert.Equal("DehIndex", names[12]);

        queries.FindOwner = s => ReferenceEquals(s, state) ? owner : null;
        queries.StateName = _ => "STATENAME";
        owner.Labels.NumLabels = 1;
        owner.Labels.Items[0].Name = "Spawn";
        owner.Labels.Items[0].State = state;
        sentinel = new Node1350 { Id = 99 };
        Assert.True(node.GetChildNode("Frames", ref sentinel));
        Assert.Equal(99, sentinel!.Id);
        Assert.True(node.GetChildNode("", ref sentinel));
        Assert.Equal(99, sentinel!.Id);
        Assert.True(node.GetChildNode("nope", ref sentinel));
        Assert.Equal(99, sentinel!.Id);
    }

    [Fact]
    public void ChildrenDumpAndFlags()
    {
        var next = new State1350 { Tics = 6, Sprite = 4 };
        var state = new State1350
        {
            Next = next,
            Sprite = -3,
            Tics = -4,
            TicRange = 65535,
            Light = -2,
            Frame = 255,
            UseFlags = 15,
            DefineFlags = 2,
            Misc1 = int.MinValue,
            Misc2 = 8,
            DehIndex = 12,
            StateFlags = 0,
        };
        var owning = new object();
        var queries = new StateQueries1350
        {
            FindOwner = _ => null,
            StateName = _ => "Named",
        };
        var node = new VmStatePointer1350("S", VmRef1350.FromState(state), owning, queries);

        Node1350? child = null;
        Assert.True(node.GetChildNode("SPRITE", ref child));
        var sprite = Assert.IsType<ValueNode1350>(child);
        Assert.Equal("sprite", sprite.Name);
        Assert.Equal(-3, sprite.Bits);
        Assert.Equal(VmStatePointer1350.TypeSpriteId, sprite.TypeToken);

        state.Sprite = 40;
        Node1350? again = null;
        Assert.True(node.GetChildNode("sprite", ref again));
        Assert.NotSame(sprite, again);
        Assert.Equal(40, Assert.IsType<ValueNode1350>(again).Bits);

        Assert.True(node.GetChildNode("tics", ref child));
        var tics = Assert.IsType<ValueNode1350>(child);
        Assert.Equal("Tics", tics.Name);
        Assert.Equal(-4, tics.Bits);
        Assert.Equal(VmStatePointer1350.TypeSInt16, tics.TypeToken);
        state.Tics = 3;
        Assert.True(node.GetChildNode("Tics", ref child));
        Assert.Equal(3, Assert.IsType<ValueNode1350>(child).Bits);

        Assert.True(node.GetChildNode("ticrange", ref child));
        Assert.Equal(65535, Assert.IsType<ValueNode1350>(child).Bits);
        Assert.Equal(VmStatePointer1350.TypeUInt16, Assert.IsType<ValueNode1350>(child).TypeToken);

        Assert.True(node.GetChildNode("LIGHT", ref child));
        Assert.Equal(-2, Assert.IsType<ValueNode1350>(child).Bits);

        Assert.True(node.GetChildNode("frame", ref child));
        var frame = Assert.IsType<ValueNode1350>(child);
        Assert.Equal("Frame", frame.Name);
        Assert.Equal(255, frame.Bits);
        Assert.Equal(VmStatePointer1350.TypeUInt8, frame.TypeToken);

        Assert.True(node.GetChildNode("useflags", ref child));
        var use = Assert.IsType<ValueNode1350>(child);
        Assert.Equal("UseFlags", use.Name);
        Assert.Equal(2, use.Bits);
        Assert.Equal(VmStatePointer1350.TypeUInt8, use.TypeToken);
        state.UseFlags = 1;
        Assert.True(node.GetChildNode("UseFlags", ref child));
        Assert.Equal(2, Assert.IsType<ValueNode1350>(child).Bits);

        Assert.True(node.GetChildNode("misc1", ref child));
        Assert.Equal(int.MinValue, Assert.IsType<ValueNode1350>(child).Bits);
        Assert.Equal(VmStatePointer1350.TypeSInt32, Assert.IsType<ValueNode1350>(child).TypeToken);
        Assert.True(node.GetChildNode("MISC2", ref child));
        Assert.Equal(8, Assert.IsType<ValueNode1350>(child).Bits);
        Assert.True(node.GetChildNode("dehindex", ref child));
        Assert.Equal(12, Assert.IsType<ValueNode1350>(child).Bits);

        Assert.Equal("0 ()", Flags(node, 0));
        Assert.Equal("1 (STF_SLOW)", Flags(node, VmStatePointer1350.StfSlow));
        Assert.Equal("128 (STF_CONSUMEAMMO)", Flags(node, VmStatePointer1350.StfConsumeAmmo));
        Assert.Equal(
            "7 (STF_SLOW | STF_FAST | STF_FULLBRIGHT)",
            Flags(node, (ushort)(VmStatePointer1350.StfSlow | VmStatePointer1350.StfFast | VmStatePointer1350.StfFullbright)));
        Assert.Equal(
            "48 (STF_SAMEFRAME | STF_CANRAISE)",
            Flags(node, (ushort)(VmStatePointer1350.StfSameFrame | VmStatePointer1350.StfCanRaise)));
        Assert.Equal("256 ()", Flags(node, 256));
        Assert.Equal(
            "65535 (STF_SLOW | STF_FAST | STF_FULLBRIGHT | STF_NODELAY | STF_SAMEFRAME | STF_CANRAISE | STF_DEHACKED | STF_CONSUMEAMMO)",
            Flags(node, ushort.MaxValue));

        Assert.Equal("0 SDF_NEXT", Define(node, VmStatePointer1350.SdfNext));
        Assert.Equal("1 SDF_STATE", Define(node, VmStatePointer1350.SdfState));
        Assert.Equal("2 SDF_STOP", Define(node, VmStatePointer1350.SdfStop));
        Assert.Equal("3 SDF_WAIT", Define(node, VmStatePointer1350.SdfWait));
        Assert.Equal("4 SDF_LABEL", Define(node, VmStatePointer1350.SdfLabel));
        Assert.Equal("5 SDF_INDEX", Define(node, VmStatePointer1350.SdfIndex));
        Assert.Equal("6 <INVALID>", Define(node, 6));
        Assert.Equal("7 SDF_MASK", Define(node, VmStatePointer1350.SdfMask));

        Assert.True(node.GetChildNode("nextstate", ref child));
        var link = Assert.IsType<VmStatePointer1350>(child);
        Assert.Equal("NextState", link.Name);
        Assert.Same(owning, link.OwningType);
        Assert.Same(queries, link.Queries);
        Assert.Same(next, link.Value.Address);
        Node1350? nested = null;
        Assert.True(link.GetChildNode("Tics", ref nested));
        Assert.Equal(6, Assert.IsType<ValueNode1350>(nested).Bits);
        var linkShown = new Var1350();
        link.Id = 3;
        Assert.True(link.Serialize(linkShown));
        Assert.Equal("Named", linkShown.Value);
        Assert.Equal(3, linkShown.VariablesReference);

        state.Next = null;
        Assert.True(node.GetChildNode("NextState", ref child));
        var dead = Assert.IsType<VmStatePointer1350>(child);
        Assert.Same(owning, dead.OwningType);
        var deadShown = new Var1350 { NamedVariables = 2 };
        dead.Id = 8;
        Assert.True(dead.Serialize(deadShown));
        Assert.Equal("<NULL>", deadShown.Value);
        Assert.Equal(0, deadShown.VariablesReference);
        Assert.Equal(12, deadShown.NamedVariables);

        var spawn = new State1350 { DehIndex = 4 };
        var flash = new State1350 { DehIndex = 9 };
        var bad = new State1350 { DehIndex = -1 };
        var named = new State1350 { DehIndex = -5 };
        var dumpOwner = new StateOwner1350 { TypeName = "Imp" };
        var dumpQueries = new StateQueries1350
        {
            FindOwner = s => ReferenceEquals(s, spawn) || ReferenceEquals(s, named) ? dumpOwner : null,
            StateName = s => ReferenceEquals(s, named) ? "Imp.See" : "SpawnName",
        };
        var list = new StateLabelList1350
        {
            NumLabels = 3,
            Items =
            {
                new StateLabel1350
                {
                    Name = "Spawn",
                    State = spawn,
                    Children = new StateLabelList1350
                    {
                        NumLabels = 2,
                        Items =
                        {
                            new StateLabel1350 { Name = "Flash", State = flash },
                            new StateLabel1350
                            {
                                Name = "None",
                                State = null,
                                Children = new StateLabelList1350
                                {
                                    NumLabels = 1,
                                    Items = { new StateLabel1350 { Name = "Bad", State = bad } },
                                },
                            },
                        },
                    },
                },
                new StateLabel1350 { Name = "See", State = named },
                new StateLabel1350 { Name = "Empty", State = null },
            },
        };
        VmStatePointer1350.DumpStateHelper(list, "", dumpQueries);
        Assert.Equal(new[]
        {
            "Spawn: SpawnName\n",
            ".SpawnFlash: DehExtra 9\n",
            ".Spawn.NoneBad: invalid\n",
            "See: Imp.See\n",
        }, dumpQueries.Lines);
        Assert.Throws<NullReferenceException>(() => VmStatePointer1350.DumpStateHelper(null!, "", dumpQueries));
    }

    static string Flags(VmStatePointer1350 node, ushort flags)
    {
        node.Value.Address!.StateFlags = flags;
        Node1350? child = null;
        Assert.True(node.GetChildNode("StateFlags", ref child));
        var dummy = Assert.IsType<DummyNode1350>(child);
        Assert.Equal("StateFlags", dummy.Name);
        Assert.Equal("StateFlags", dummy.TypeToken);
        return dummy.Text;
    }

    static string Define(VmStatePointer1350 node, byte flags)
    {
        node.Value.Address!.DefineFlags = flags;
        Node1350? child = null;
        Assert.True(node.GetChildNode("DefineFlags", ref child));
        var dummy = Assert.IsType<DummyNode1350>(child);
        Assert.Equal("DefineFlags", dummy.Name);
        Assert.Equal("DefineFlags", dummy.TypeToken);
        return dummy.Text;
    }
}
