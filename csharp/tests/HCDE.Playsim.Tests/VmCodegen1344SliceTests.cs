using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmCodegen1344SliceTests
{
    [Fact]
    public void DispatchAndScope()
    {
        var names = new NameTable1344();
        Assert.Equal(0, names.Intern(null));
        var empty = names.Intern("");
        Assert.NotEqual(0, empty);
        Assert.Equal("", names.Chars(empty));
        Assert.Equal(names.Intern("GetClass"), names.Intern("GETCLASS"));
        Assert.Equal("GetClass", names.Chars(names.Intern("getclass")));
        Assert.Equal("Call", names.Chars(names.Intern("call")));

        var actor = ClassOf("Actor", new ClassDesc1344(), VmScopeBarrier1207Slice.ScopePlay);
        var receiver = new Expr1344 { ValueType = TypeNode1344.PointerTo(actor) };
        var member = CallOn(receiver, "getclass");
        var created = member.Continue(OpenCtx());
        var getClass = Assert.IsType<GetClassExpr1344>(created);
        Assert.True(getClass.Resolved);
        Assert.Equal(1, getClass.ResolveCount);
        Assert.Same(receiver, getClass.Self);
        Assert.Same(receiver, member.Self);
        Assert.False(member.Deleted);
        Assert.False(receiver.Deleted);
        Assert.Empty(member.Log.Notes);
        Assert.Equal(0, member.FindCalls);

        var extra = new Expr1344();
        var tooMany = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(actor) }, "GetClass");
        tooMany.ArgList.Add(extra);
        Assert.Null(tooMany.Continue(OpenCtx()));
        AssertOnly(tooMany.Log, "error", "Too many parameters in call to GetClass");
        Assert.True(tooMany.Deleted);
        Assert.True(tooMany.Self == null);
        Assert.True(extra.Deleted);

        var pointed = new Expr1344 { ValueType = TypeNode1344.PointerTo(actor) };
        var other = CallOn(pointed, "Tick");
        var tick = MethodFn("A_Tick", VmCodegen1344Slice.VarfMethod);
        var ctx = OpenCtx();
        ctx.FromDecorate = true;
        ctx.Class = actor;
        ctx.Version = ScriptVersion1344.Make(4, 10, 2);
        Wire(other, tick);
        var produced = Assert.IsType<VmFunctionCall1344>(other.Continue(ctx));
        Assert.Equal(1, other.FindCalls);
        Assert.Same(actor, other.LastFind!.Cls);
        Assert.Same(actor, other.LastFind.ContextClass);
        Assert.False(other.LastFind.NotFromDecorate);
        Assert.Equal(ctx.Version.Major, other.LastFind.Version.Major);
        Assert.Equal(ctx.Version.Minor, other.LastFind.Version.Minor);
        Assert.Equal(ctx.Version.Revision, other.LastFind.Version.Revision);
        Assert.Same(tick, produced.Function);
        Assert.Same(pointed, produced.Self);
        Assert.False(pointed.Deleted);
        Assert.Null(other.Self);
        Assert.True(other.Deleted);
        Assert.False(produced.NoVirtual);
        Assert.False(produced.FnPtrCall);
        Assert.True(produced.Continued);
        Assert.Empty(other.Log.Notes);

        var blob = new TypeNode1344();
        var alien = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(blob) }, "Tick");
        Assert.Null(alien.Continue(OpenCtx()));
        AssertOnly(alien.Log, "error", "Left hand side of Tick must point to a class object");
        Assert.Equal(0, alien.FindCalls);

        var record = ClassOf("Vec", null, 0);
        record.IsStruct = true;
        record.IsClass = false;
        var recordRecv = new Expr1344 { ValueType = TypeNode1344.PointerTo(record) };
        var recordPtr = CallOn(recordRecv, "GetClass");
        var recordFn = MethodFn("Get", VmCodegen1344Slice.VarfMethod);
        Wire(recordPtr, recordFn);
        Assert.IsType<VmFunctionCall1344>(recordPtr.Continue(OpenCtx()));
        Assert.Same(record, recordPtr.LastFind!.Cls);
        Assert.Equal(1, recordPtr.FindCalls);
        Assert.Equal(0, recordRecv.AddressCount);

        var dangling = new TypeNode1344 { IsRealPointer = true };
        dangling.Pointer = dangling;
        var invalidPointer = CallOn(new Expr1344 { ValueType = dangling }, "Tick");
        Assert.Null(invalidPointer.Continue(OpenCtx()));
        AssertOnly(invalidPointer.Log, "error", "Invalid expression on left hand side of Tick");

        var broken = new TypeNode1344 { IsRealPointer = true };
        var brokenCall = CallOn(new Expr1344 { ValueType = broken }, "Tick");
        var pointerEx = Assert.Throws<NullReferenceException>(() => brokenCall.Continue(OpenCtx()));
        Assert.Equal("pointer", pointerEx.Message);
        Assert.False(brokenCall.Deleted);

        var writable = ClassOf("Vec", null, 0);
        writable.IsStruct = true;
        var writableRecv = new Expr1344 { ValueType = writable };
        writableRecv.AddressRequest = _ => (true, true);
        var writableCall = CallOn(writableRecv, "Tick");
        var writableFn = MethodFn("Tick", VmCodegen1344Slice.VarfMethod);
        Wire(writableCall, writableFn);
        Assert.IsType<VmFunctionCall1344>(writableCall.Continue(OpenCtx()));
        Assert.False(writableCall.IsReadonly);
        Assert.True(writableRecv.ValueType!.IsRealPointer);
        Assert.Same(writable, writableRecv.ValueType.Pointed);
        Assert.Same(writable, writableCall.LastFind!.Cls);
        Assert.Equal(1, writableRecv.AddressCount);

        var locked = ClassOf("Vec", null, 0);
        locked.IsStruct = true;
        var lockedRecv = new Expr1344 { ValueType = locked };
        lockedRecv.AddressRequest = _ => (true, false);
        var lockedCall = CallOn(lockedRecv, "Tick");
        Wire(lockedCall, MethodFn("Tick", VmScopeBarrier1207Slice.VarfUi | VmCodegen1344Slice.VarfMethod));
        Assert.Null(lockedCall.Continue(OpenCtx()));
        Assert.True(lockedCall.IsReadonly);
        Assert.False(lockedCall.BarrierBuilt);
        AssertOnly(lockedCall.Log, "error", "Readonly struct on left hand side of Tick not allowed");
        Assert.Same(locked, lockedRecv.ValueType!.Pointed);

        var unread = ClassOf("Vec", null, 0);
        unread.IsStruct = true;
        var unreadRecv = new Expr1344 { ValueType = unread };
        unreadRecv.AddressRequest = _ => (false, true);
        var unreadCall = CallOn(unreadRecv, "Tick");
        Wire(unreadCall, MethodFn("Tick", VmCodegen1344Slice.VarfMethod));
        Assert.Null(unreadCall.Continue(OpenCtx()));
        Assert.True(unreadCall.IsReadonly);
        AssertOnly(unreadCall.Log, "error", "Readonly struct on left hand side of Tick not allowed");

        var frozen = ClassOf("Vec", null, 0);
        frozen.IsStruct = true;
        var frozenRecv = new Expr1344 { ValueType = frozen };
        var frozenCall = CallOn(frozenRecv, "Tick");
        frozenCall.IsReadonly = true;
        frozenRecv.AddressRequest = _ => (true, true);
        Assert.Null(frozenCall.Continue(OpenCtx()));
        Assert.Equal(0, frozenRecv.AddressCount);
        Assert.Same(frozen, frozenRecv.ValueType);
        AssertOnly(frozenCall.Log, "error", "Invalid expression on left hand side of Tick");

        var allowed = ClassOf("Vec", null, 0);
        allowed.IsStruct = true;
        var allowedRecv = new Expr1344 { ValueType = allowed };
        allowedRecv.AddressRequest = _ => (false, false);
        var allowedCall = CallOn(allowedRecv, "Tick");
        Wire(allowedCall, MethodFn("Tick", VmScopeBarrier1207Slice.VarfReadOnly | VmCodegen1344Slice.VarfMethod));
        Assert.IsType<VmFunctionCall1344>(allowedCall.Continue(OpenCtx()));
        Assert.True(allowedCall.IsReadonly);
        Assert.True(allowedCall.BarrierBuilt);
        Assert.Empty(allowedCall.Log.Notes);

        var fake = MethodFn("Invoke", 0, 0);
        var fnType = new TypeNode1344
        {
            IsFunctionPointer = true,
            Pointed = new TypeNode1344(),
            FakeFunction = fake,
        };
        var fnRecv = new Expr1344 { ValueType = fnType };
        var fnCall = CallOn(fnRecv, "Call");
        fnCall.StaticOnly = true;
        fnCall.NoVirtual = true;
        fnCall.Find = _ => throw new InvalidOperationException("find");
        var fnCtx = OpenCtx();
        var fnProduced = Assert.IsType<VmFunctionCall1344>(fnCall.Continue(fnCtx));
        Assert.True(fnCall.FnPtr);
        Assert.Same(fake, fnCall.AfdOverride);
        Assert.Equal(0, fnCall.FindCalls);
        Assert.Same(fake, fnProduced.Function);
        Assert.True(fnProduced.FnPtrCall);
        Assert.False(fnProduced.NoVirtual);
        Assert.Same(fnRecv, fnProduced.Self);
        Assert.False(fnRecv.Deleted);
        Assert.Null(fnProduced.Defaults);
        Assert.True(fnCtx.Function!.Variant.Implementation!.BlockJit);

        var voidType = new TypeNode1344 { IsFunctionPointer = true, Pointed = TypeNode1344.Void };
        var voidCall = CallOn(new Expr1344 { ValueType = voidType }, "Call");
        Assert.Null(voidCall.Continue(OpenCtx()));
        AssertOnly(voidCall.Log, "error", "Unknown function Call");
        Assert.Equal(0, voidCall.FindCalls);

        var ping = CallOn(new Expr1344 { ValueType = fnType }, "Ping");
        Assert.Null(ping.Continue(OpenCtx()));
        AssertOnly(ping.Log, "error", "Unknown function Ping");

        var plain = CallOn(new Expr1344 { ValueType = new TypeNode1344() }, "Foo");
        Assert.Null(plain.Continue(OpenCtx()));
        AssertOnly(plain.Log, "error", "Invalid expression on left hand side of Foo");

        var preset = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(actor) }, "Tick");
        preset.Cls = actor;
        preset.AfdOverride = MethodFn("Tick", VmCodegen1344Slice.VarfMethod);
        preset.Find = _ => throw new InvalidOperationException("find");
        Assert.IsType<VmFunctionCall1344>(preset.ResolveResolved(OpenCtx()));
        Assert.Equal(0, preset.FindCalls);

        var badFind = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(actor) }, "Tick");
        badFind.Find = _ => new FindResult1344 { Function = MethodFn("Tick", VmCodegen1344Slice.VarfMethod), Error = true };
        Assert.Null(badFind.Continue(OpenCtx()));
        Assert.Empty(badFind.Log.Notes);
        Assert.False(badFind.BarrierBuilt);
        Assert.True(badFind.Deleted);

        var missing = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(actor) }, "Tick");
        missing.Find = _ => new FindResult1344();
        Assert.Null(missing.Continue(OpenCtx()));
        AssertOnly(missing.Log, "error", "Unknown function Tick");

        var playClass = ClassOf("Imp", new ClassDesc1344(), VmScopeBarrier1207Slice.ScopePlay);
        var fromPlay = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(playClass) }, "Tick");
        var uiFn = MethodFn("A_Chase", VmScopeBarrier1207Slice.VarfUi | VmCodegen1344Slice.VarfMethod);
        Wire(fromPlay, uiFn);
        var playCtx = OpenCtx();
        playCtx.Class = playClass;
        playCtx.Function!.Variant.Flags = VmScopeBarrier1207Slice.VarfVirtualScope | VmCodegen1344Slice.VarfVirtual;
        Assert.Null(fromPlay.Continue(playCtx));
        AssertOnly(fromPlay.Log, "error", "Can't call ui function Tick from play context (not readable)");

        var fromData = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(playClass) }, "Tick");
        Wire(fromData, uiFn);
        var dataCtx = OpenCtx();
        dataCtx.Class = null;
        dataCtx.Function!.Variant.Flags = VmScopeBarrier1207Slice.VarfVirtualScope | VmCodegen1344Slice.VarfVirtual;
        Assert.Null(fromData.Continue(dataCtx));
        AssertOnly(fromData.Log, "error", "Can't call ui function Tick from data context (not readable)");

        var uiClass = ClassOf("Menu", new ClassDesc1344(), VmScopeBarrier1207Slice.ScopeUi);
        var virtualOnly = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(uiClass) }, "Tick");
        Wire(virtualOnly, MethodFn("Tick", VmScopeBarrier1207Slice.VarfVirtualScope | VmCodegen1344Slice.VarfMethod));
        Assert.Null(virtualOnly.Continue(OpenCtx()));
        AssertOnly(virtualOnly.Log, "error", "Can't call ui function Tick from data context (not readable)");

        var sided = ClassOf("Vec", null, 0);
        sided.IsStruct = true;
        var sidedRecv = new Expr1344
        {
            ValueType = sided,
            ExprType = VmCodegen1344Slice.EfxStructMember,
            BarrierSide = VmScopeBarrier1207Slice.SidePlay,
        };
        sidedRecv.AddressRequest = _ => (true, true);
        var sidedCall = CallOn(sidedRecv, "Tick");
        Wire(sidedCall, MethodFn("A_Chase", VmCodegen1344Slice.VarfMethod));
        Assert.Null(sidedCall.Continue(OpenCtx()));
        AssertOnly(sidedCall.Log, "error", "Can't call play function Tick from data context");

        var uiSided = ClassOf("Vec", null, 0);
        uiSided.IsStruct = true;
        var uiRecv = new Expr1344
        {
            ValueType = uiSided,
            ExprType = VmCodegen1344Slice.EfxStructMember,
            BarrierSide = VmScopeBarrier1207Slice.SidePlay,
        };
        uiRecv.AddressRequest = _ => (true, true);
        var uiCall = CallOn(uiRecv, "Tick");
        Wire(uiCall, MethodFn("A_Chase", VmScopeBarrier1207Slice.VarfUi | VmCodegen1344Slice.VarfMethod));
        Assert.Null(uiCall.Continue(OpenCtx()));
        AssertOnly(uiCall.Log, "error", "Can't call ui function Tick from data context (not readable)");

        var clearRecv = new Expr1344 { ValueType = TypeNode1344.PointerTo(actor) };
        var clearCall = CallOn(clearRecv, "Tick");
        Wire(clearCall, MethodFn("Tick", VmScopeBarrier1207Slice.VarfClearScope | VmCodegen1344Slice.VarfMethod));
        Assert.IsType<VmFunctionCall1344>(clearCall.Continue(OpenCtx()));
        Assert.Empty(clearCall.Log.Notes);

        var playRead = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(actor) }, "Tick");
        Wire(playRead, MethodFn("Tick", VmScopeBarrier1207Slice.VarfPlay | VmScopeBarrier1207Slice.VarfReadOnly | VmCodegen1344Slice.VarfMethod));
        Assert.IsType<VmFunctionCall1344>(playRead.Continue(OpenCtx()));
        Assert.Empty(playRead.Log.Notes);

        var matched = ClassOf("Imp", new ClassDesc1344(), VmScopeBarrier1207Slice.ScopePlay);
        var matchedRecv = new Expr1344 { ValueType = TypeNode1344.PointerTo(matched) };
        var matchedCall = CallOn(matchedRecv, "Tick");
        var matchedFn = MethodFn("Tick", VmScopeBarrier1207Slice.VarfVirtualScope | VmCodegen1344Slice.VarfMethod);
        Wire(matchedCall, matchedFn);
        var matchedCtx = OpenCtx();
        matchedCtx.Class = matched;
        matchedCtx.Function!.Variant.Flags = VmScopeBarrier1207Slice.VarfVirtualScope | VmCodegen1344Slice.VarfVirtual;
        var matchedProduced = Assert.IsType<VmFunctionCall1344>(matchedCall.Continue(matchedCtx));
        Assert.Same(matchedRecv, matchedProduced.Self);
        Assert.False(matchedRecv.Deleted);
        Assert.NotEqual(0, matchedFn.Variant.Flags & VmCodegen1344Slice.VarfMethod);

        var parentDesc = new ClassDesc1344();
        var childDesc = new ClassDesc1344 { Parent = parentDesc };
        var parentType = ClassOf("Actor", parentDesc, 0);
        var childType = ClassOf("Imp", childDesc, 0);
        var qualified = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(parentType) }, "Tick");
        qualified.StaticOnly = true;
        Wire(qualified, MethodFn("Tick", VmCodegen1344Slice.VarfMethod));
        var qualifiedCtx = OpenCtx();
        qualifiedCtx.Class = childType;
        Assert.Null(qualified.Continue(qualifiedCtx));
        AssertOnly(qualified.Log, "error", "Qualified member call to parent class Actor::Tick is not yet implemented");
        Assert.True(qualified.Deleted);

        var same = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(parentType) }, "Tick");
        same.StaticOnly = true;
        Wire(same, MethodFn("Tick", VmCodegen1344Slice.VarfMethod));
        var sameCtx = OpenCtx();
        sameCtx.Class = parentType;
        Assert.Null(same.Continue(sameCtx));
        AssertOnly(same.Log, "error", "Qualified member call to parent class Actor::Tick is not yet implemented");

        var outside = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(childType) }, "Tick");
        outside.StaticOnly = true;
        Wire(outside, MethodFn("Tick", VmCodegen1344Slice.VarfMethod));
        var outsideCtx = OpenCtx();
        outsideCtx.Class = parentType;
        Assert.Null(outside.Continue(outsideCtx));
        AssertOnly(outside.Log, "error", "Cannot call non-static function Imp::Tick from here");

        var vec = ClassOf("Vec", null, 0);
        vec.IsStruct = true;
        var vecRecv = new Expr1344 { ValueType = vec };
        vecRecv.AddressRequest = _ => (true, true);
        var vecCall = CallOn(vecRecv, "Tick");
        vecCall.StaticOnly = true;
        Wire(vecCall, MethodFn("Tick", VmCodegen1344Slice.VarfMethod));
        Assert.Null(vecCall.Continue(OpenCtx()));
        AssertOnly(vecCall.Log, "error", "Cannot call non-static function Vec::Tick from here");

        var nullCls = CallOn(new Expr1344 { ValueType = fnType }, "Call");
        nullCls.StaticOnly = true;
        nullCls.AfdOverride = MethodFn("Invoke", VmCodegen1344Slice.VarfMethod);
        fnType.FakeFunction = nullCls.AfdOverride;
        var nullClsEx = Assert.Throws<NullReferenceException>(() => nullCls.Continue(OpenCtx()));
        Assert.Equal("class", nullClsEx.Message);
        Assert.False(nullCls.Deleted);

        var skipped = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(childType) }, "Tick");
        skipped.StaticOnly = true;
        skipped.NoVirtual = true;
        Wire(skipped, MethodFn("Tick", VmCodegen1344Slice.VarfMethod | VmCodegen1344Slice.VarfVirtual));
        var skippedCtx = OpenCtx();
        skippedCtx.Class = parentType;
        var skippedProduced = Assert.IsType<VmFunctionCall1344>(skipped.Continue(skippedCtx));
        Assert.True(skippedProduced.NoVirtual);
        Assert.Empty(skipped.Log.Notes);

        var stillStatic = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(childType) }, "Tick");
        stillStatic.StaticOnly = true;
        stillStatic.NoVirtual = true;
        Wire(stillStatic, MethodFn("Tick", VmCodegen1344Slice.VarfMethod));
        var stillCtx = OpenCtx();
        stillCtx.Class = parentType;
        Assert.Null(stillStatic.Continue(stillCtx));
        AssertOnly(stillStatic.Log, "error", "Cannot call non-static function Imp::Tick from here");

        var constant = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(actor) }, "Tick");
        Wire(constant, MethodFn("Tick", VmCodegen1344Slice.VarfMethod));
        var constantCtx = OpenCtx();
        constantCtx.Function = null;
        Assert.Null(constant.Continue(constantCtx));
        AssertOnly(constant.Log, "error", "Unable to call Tick from constant declaration");

        var selfRecv = new Expr1344
        {
            ValueType = TypeNode1344.PointerTo(actor),
            ExprType = VmCodegen1344Slice.EfxSelf,
        };
        var selfCall = CallOn(selfRecv, "Tick");
        Wire(selfCall, MethodFn("Tick", VmCodegen1344Slice.VarfMethod, 0));
        selfCall.Compatible = (_, _) => false;
        Assert.Null(selfCall.Continue(OpenCtx()));
        Assert.Empty(selfCall.Log.Notes);
        Assert.Equal(1, selfCall.CompatibleCalls);
        Assert.True(selfRecv.Deleted);

        var selfOk = new Expr1344
        {
            ValueType = TypeNode1344.PointerTo(actor),
            ExprType = VmCodegen1344Slice.EfxSelf,
        };
        var selfOkCall = CallOn(selfOk, "Tick");
        Wire(selfOkCall, MethodFn("Tick", VmCodegen1344Slice.VarfMethod, 0));
        selfOkCall.Compatible = (_, _) => true;
        var selfProduced = Assert.IsType<VmFunctionCall1344>(selfOkCall.Continue(OpenCtx()));
        Assert.Equal(1, selfOkCall.CompatibleCalls);
        Assert.Same(selfOk, selfProduced.Self);
        Assert.Empty(selfOkCall.Log.Notes);

        var foreign = CallOn(new Expr1344 { ValueType = TypeNode1344.PointerTo(actor) }, "Tick");
        Wire(foreign, MethodFn("A_Tick", VmCodegen1344Slice.VarfMethod, 0));
        Assert.Null(foreign.Continue(OpenCtx()));
        AssertOnly(foreign.Log, "error", "Function A_Tick cannot be used with a non-self object");

        var arg = new Expr1344();
        var args = new List<Expr1344> { arg };
        var movedRecv = new Expr1344 { ValueType = TypeNode1344.PointerTo(actor) };
        var moved = CallOn(movedRecv, "Tick");
        moved.ArgList = args;
        Wire(moved, MethodFn("Tick", VmCodegen1344Slice.VarfMethod));
        var movedProduced = Assert.IsType<VmFunctionCall1344>(moved.Continue(OpenCtx()));
        Assert.Same(args, movedProduced.ArgList);
        Assert.Empty(moved.ArgList);
        Assert.False(arg.Deleted);
        Assert.Same(movedRecv, movedProduced.Self);
        Assert.False(movedProduced.NoVirtual);

        var staticRecv = new Expr1344 { ValueType = TypeNode1344.PointerTo(actor) };
        var staticCall = CallOn(staticRecv, "Tick");
        staticCall.StaticOnly = true;
        Wire(staticCall, MethodFn("Tick", 0, 0));
        var staticProduced = Assert.IsType<VmFunctionCall1344>(staticCall.Continue(OpenCtx()));
        Assert.Null(staticProduced.Self);
        Assert.True(staticRecv.Deleted);
        Assert.True(staticProduced.NoVirtual);
        Assert.False(staticProduced.FnPtrCall);
    }

    [Fact]
    public void DirectCallsAndAccessibility()
    {
        var left = ScriptVersion1344.Make(1, 2, 3);
        var same = ScriptVersion1344.Make(1, 2, 3);
        Assert.True(left.LessOrEqual(same));
        Assert.True(left.GreaterOrEqual(same));
        Assert.False(left.Greater(same));
        Assert.False(left.Less(same));
        var older = ScriptVersion1344.Make(1, 2, 2);
        Assert.True(left.Greater(older));
        Assert.False(older.Greater(left));
        Assert.True(older.Less(left));
        Assert.True(left.GreaterOrEqual(older));
        Assert.False(older.GreaterOrEqual(left));
        Assert.True(older.LessOrEqual(left));
        Assert.False(left.LessOrEqual(older));
        Assert.True(ScriptVersion1344.Make(2, 0, 0).Greater(ScriptVersion1344.Make(1, 9, 9)));
        Assert.True(ScriptVersion1344.Make(1, 3, 0).Greater(ScriptVersion1344.Make(1, 2, 9)));
        Assert.True(ScriptVersion1344.Make(1, 2, 0).Less(ScriptVersion1344.Make(1, 2, 1)));
        Assert.False(ScriptVersion1344.Make(1, 2, 0).Greater(ScriptVersion1344.Make(1, 2, 0)));
        Assert.True(ScriptVersion1344.Make(2, 3, 0).GreaterOrEqual(ScriptVersion1344.Make(2, 3)));
        Assert.False(ScriptVersion1344.Make(2, 2, 9).GreaterOrEqual(ScriptVersion1344.Make(2, 3)));

        var blocked = Routine("Foo", 0);
        blocked.Version = ScriptVersion1344.Make(2, 4, 0);
        var blockedCall = new VmFunctionCall1344(null, blocked, new List<Expr1344>(), false);
        Assert.False(blockedCall.CheckAccessibility(ScriptVersion1344.Make(2, 3, 0)));
        AssertOnly(blockedCall.Log, "error", "Foo not accessible to ZScript version 2.3.0");

        var decorate = Routine("Foo", 0);
        decorate.Version = ScriptVersion1344.Make(2, 4, 0);
        var decorateCall = new VmFunctionCall1344(null, decorate, new List<Expr1344>(), false);
        Assert.False(decorateCall.CheckAccessibility(ScriptVersion1344.Make(2, 2, 9)));
        AssertOnly(decorateCall.Log, "error", "Foo not accessible to DECORATE");

        var silent = Routine("Foo", VmCodegen1344Slice.VarfDeprecated);
        silent.Version = ScriptVersion1344.Make(1, 2, 4);
        var silentCall = new VmFunctionCall1344(null, silent, new List<Expr1344>(), false);
        Assert.True(silentCall.CheckAccessibility(ScriptVersion1344.Make(1, 2, 3)));
        Assert.Empty(silentCall.Log.Notes);

        var current = Routine("Foo", 0);
        current.Version = ScriptVersion1344.Make(1, 2, 3);
        var currentCall = new VmFunctionCall1344(null, current, new List<Expr1344>(), false);
        Assert.True(currentCall.CheckAccessibility(ScriptVersion1344.Make(1, 2, 3)));
        Assert.Empty(currentCall.Log.Notes);

        var owner = new ContainerType1344 { DefFileNo = 7 };
        var internalFn = Routine("Foo", VmCodegen1344Slice.VarfDeprecated);
        internalFn.Version = ScriptVersion1344.Make(1, 2, 3);
        internalFn.OwningClass = owner;
        var internalCall = new VmFunctionCall1344(null, internalFn, new List<Expr1344>(), false);
        internalCall.FileContainer = file => file == 7 ? 0 : 1;
        Assert.True(internalCall.CheckAccessibility(ScriptVersion1344.Make(1, 2, 3)));
        AssertOnly(internalCall.Log, "debug", "Accessing deprecated function Foo - deprecated since 1.2.3");

        var externalFn = Routine("Foo", VmCodegen1344Slice.VarfDeprecated);
        externalFn.Version = ScriptVersion1344.Make(1, 2, 3);
        externalFn.OwningClass = owner;
        externalFn.Variant.DeprecationMessage = "use Bar";
        var externalCall = new VmFunctionCall1344(null, externalFn, new List<Expr1344>(), false);
        externalCall.FileContainer = _ => 4;
        Assert.True(externalCall.CheckAccessibility(ScriptVersion1344.Make(1, 2, 5)));
        AssertOnly(externalCall.Log, "warning", "Accessing deprecated function Foo - deprecated since 1.2.3, use Bar");

        var orphan = Routine("Foo", VmCodegen1344Slice.VarfDeprecated);
        orphan.Version = ScriptVersion1344.Make(1, 0, 0);
        var orphanCall = new VmFunctionCall1344(null, orphan, new List<Expr1344>(), false);
        var orphanEx = Assert.Throws<NullReferenceException>(() => orphanCall.CheckAccessibility(ScriptVersion1344.Make(1, 0, 0)));
        Assert.Equal("owning class", orphanEx.Message);

        var withArgs = Routine("Foo", 0);
        withArgs.Version = ScriptVersion1344.Make(9, 0, 0);
        var argCall = new VmFunctionCall1344(null, withArgs, new List<Expr1344> { new Expr1344() }, false);
        Assert.Null(argCall.GetDirectFunction(null, ScriptVersion1344.Make(1, 0, 0)));
        Assert.Equal(0, argCall.AccessibilityCalls);
        Assert.Equal(0, argCall.DirectCompatibleCalls);

        var virtualFn = Routine("Foo", VmCodegen1344Slice.VarfVirtual);
        virtualFn.Version = ScriptVersion1344.Make(9, 0, 0);
        var virtualCall = new VmFunctionCall1344(null, virtualFn, new List<Expr1344>(), false);
        Assert.Null(virtualCall.GetDirectFunction(null, ScriptVersion1344.Make(1, 0, 0)));
        Assert.Equal(0, virtualCall.AccessibilityCalls);

        var pointerSelf = new Expr1344 { ValueType = new TypeNode1344 { IsFunctionPointer = true } };
        var pointerFn = Routine("Foo", 0);
        pointerFn.Version = ScriptVersion1344.Make(9, 0, 0);
        var pointerCall = new VmFunctionCall1344(pointerSelf, pointerFn, new List<Expr1344>(), false);
        Assert.True(pointerCall.FnPtrCall);
        Assert.Null(pointerCall.GetDirectFunction(null, ScriptVersion1344.Make(1, 0, 0)));
        Assert.Equal(0, pointerCall.AccessibilityCalls);

        var hidden = Routine("Foo", 0);
        hidden.Version = ScriptVersion1344.Make(9, 0, 0);
        var hiddenCall = new VmFunctionCall1344(null, hidden, new List<Expr1344>(), false);
        hiddenCall.DirectCompatible = (_, _) => true;
        Assert.Null(hiddenCall.GetDirectFunction(null, ScriptVersion1344.Make(2, 3, 0)));
        Assert.Equal(1, hiddenCall.AccessibilityCalls);
        Assert.Equal(0, hiddenCall.DirectCompatibleCalls);
        AssertOnly(hiddenCall.Log, "error", "Foo not accessible to ZScript version 2.3.0");

        var rejected = Routine("Foo", 0);
        var rejectedCall = new VmFunctionCall1344(null, rejected, new List<Expr1344>(), false);
        rejectedCall.DirectCompatible = (_, _) => false;
        Assert.Null(rejectedCall.GetDirectFunction(rejected, ScriptVersion1344.Make(0, 0, 0)));
        Assert.Equal(1, rejectedCall.DirectCompatibleCalls);

        var optional = Routine("Foo", VmCodegen1344Slice.VarfMethod);
        optional.Variant.ArgFlags.Add(0);
        optional.Variant.ArgFlags.Add(VmCodegen1344Slice.VarfOptional);
        optional.Variant.ArgFlags.Add(0);
        var optionalCall = Direct(optional);
        Assert.Same(optional.Variant.Implementation, optionalCall.GetDirectFunction(optional, ScriptVersion1344.Make(0, 0)));

        var required = Routine("Foo", VmCodegen1344Slice.VarfMethod);
        required.Variant.ArgFlags.Add(0);
        required.Variant.ArgFlags.Add(0);
        var requiredCall = Direct(required);
        Assert.Null(requiredCall.GetDirectFunction(required, ScriptVersion1344.Make(0, 0)));

        var implicitOnly = Routine("Foo", VmCodegen1344Slice.VarfMethod);
        implicitOnly.Variant.ArgFlags.Add(0);
        var implicitCall = Direct(implicitOnly);
        Assert.Same(implicitOnly.Variant.Implementation, implicitCall.GetDirectFunction(implicitOnly, ScriptVersion1344.Make(0, 0)));

        var action = Routine("Foo", VmCodegen1344Slice.VarfAction | VmCodegen1344Slice.VarfMethod);
        Assert.Equal(3, action.GetImplicitArgs());
        action.Variant.ArgFlags.AddRange(new[] { 0, 0, 0 });
        var actionCall = Direct(action);
        Assert.Same(action.Variant.Implementation, actionCall.GetDirectFunction(action, ScriptVersion1344.Make(0, 0)));
        action.Variant.ArgFlags.Add(0);
        action.Variant.ArgFlags[1] = VmCodegen1344Slice.VarfOptional;
        var actionBlocked = Direct(action);
        Assert.Null(actionBlocked.GetDirectFunction(action, ScriptVersion1344.Make(0, 0)));
        action.Variant.ArgFlags[3] = VmCodegen1344Slice.VarfOptional;
        var actionOpen = Direct(action);
        Assert.Same(action.Variant.Implementation, actionOpen.GetDirectFunction(action, ScriptVersion1344.Make(0, 0)));

        var plainFn = Routine("Foo", 0);
        Assert.Equal(0, plainFn.GetImplicitArgs());
        plainFn.Variant.ArgFlags.Add(0);
        Assert.Null(Direct(plainFn).GetDirectFunction(plainFn, ScriptVersion1344.Make(0, 0)));
        plainFn.Variant.ArgFlags[0] = VmCodegen1344Slice.VarfOptional;
        Assert.Same(plainFn.Variant.Implementation, Direct(plainFn).GetDirectFunction(plainFn, ScriptVersion1344.Make(0, 0)));

        var warned = Routine("Foo", VmCodegen1344Slice.VarfDeprecated);
        warned.Version = ScriptVersion1344.Make(1, 2, 3);
        warned.OwningClass = owner;
        var warnedCall = Direct(warned);
        warnedCall.FileContainer = _ => 0;
        Assert.Same(warned.Variant.Implementation, warnedCall.GetDirectFunction(warned, ScriptVersion1344.Make(1, 2, 3)));
        AssertOnly(warnedCall.Log, "debug", "Accessing deprecated function Foo - deprecated since 1.2.3");

        var marker = new TypeNode1344();
        var stringCall = new VmFunctionCall1344(null, Routine("Foo", 0), new List<Expr1344>(), false);
        stringCall.HasStringArgs = true;
        stringCall.ValueType = marker;
        var stringProto = stringCall.ReturnProto();
        Assert.NotSame(stringCall.Function.Variant.Proto, stringProto);
        Assert.Same(marker, Assert.Single(stringProto.ReturnTypes));
        Assert.Empty(stringProto.ArgumentTypes);

        var voidCall = new VmFunctionCall1344(null, Routine("Foo", 0), new List<Expr1344>(), false);
        voidCall.HasStringArgs = true;
        voidCall.ValueType = TypeNode1344.Void;
        Assert.Empty(voidCall.ReturnProto().ReturnTypes);

        var nullType = new VmFunctionCall1344(null, Routine("Foo", 0), new List<Expr1344>(), false);
        nullType.HasStringArgs = true;
        var typeEx = Assert.Throws<NullReferenceException>(() => nullType.ReturnProto());
        Assert.Equal("value type", typeEx.Message);

        var directProto = Routine("Foo", 0);
        directProto.Variant.Proto!.ArgumentTypes.Add(marker);
        var directCall = new VmFunctionCall1344(null, directProto, new List<Expr1344>(), false);
        Assert.Same(directProto.Variant.Proto, directCall.ReturnProto());

        var bare = new Expr1344();
        var objectCall = new VmFunctionCall1344(bare, Routine("Foo", 0), new List<Expr1344>(), true);
        Assert.False(objectCall.FnPtrCall);
        Assert.True(objectCall.NoVirtual);
        Assert.Null(objectCall.CallingFunction);
        var nullSelf = new VmFunctionCall1344(null, Routine("Foo", 0), new List<Expr1344>(), false);
        Assert.False(nullSelf.FnPtrCall);
        var untyped = new Expr1344();
        Assert.False(new VmFunctionCall1344(untyped, Routine("Foo", 0), new List<Expr1344>(), false).FnPtrCall);

        var resolvedSelf = new Expr1344 { ValueType = new TypeNode1344() };
        var resolvedFn = Routine("Foo", VmCodegen1344Slice.VarfMethod);
        var resolved = new VmFunctionCall1344(resolvedSelf, resolvedFn, new List<Expr1344>(), false);
        var resolvedCtx = OpenCtx();
        resolvedCtx.Version = ScriptVersion1344.Make(4, 13, 0);
        Assert.Same(resolved, resolved.Resolve(resolvedCtx));
        Assert.True(resolved.Continued);
        Assert.True(resolved.RelaxedNamedArguments);
        Assert.Equal(1, resolved.Implicit);
        Assert.False(resolvedCtx.Function!.Variant.Implementation!.BlockJit);
        Assert.Same(resolvedFn.Variant.Implementation!.DefaultArgs, resolved.Defaults);
        Assert.Same(resolvedFn.Variant.Proto!.ArgumentTypes, resolved.CapturedArgTypes);
        Assert.Same(resolvedFn.Variant.ArgNames, resolved.CapturedArgNames);
        Assert.Same(resolvedFn.Variant.ArgFlags, resolved.CapturedArgFlags);
        Assert.Equal(1, resolvedSelf.ResolveCount);
        Assert.Same(resolved, resolved.Resolve(resolvedCtx));
        Assert.Equal(1, resolvedSelf.ResolveCount);
        Assert.Equal(1, resolved.AccessibilityCalls);

        var loose = new Expr1344 { ValueType = new TypeNode1344() };
        var looseCall = new VmFunctionCall1344(loose, Routine("Foo", 0), new List<Expr1344>(), false);
        var looseCtx = OpenCtx();
        looseCtx.Version = ScriptVersion1344.Make(4, 12, 0);
        Assert.Same(looseCall, looseCall.Resolve(looseCtx));
        Assert.False(looseCall.RelaxedNamedArguments);
        var newer = new VmFunctionCall1344(new Expr1344 { ValueType = new TypeNode1344() }, Routine("Foo", 0), new List<Expr1344>(), false);
        var newerCtx = OpenCtx();
        newerCtx.Version = ScriptVersion1344.Make(4, 13, 1);
        Assert.Same(newer, newer.Resolve(newerCtx));
        Assert.True(newer.RelaxedNamedArguments);

        var failedSelf = new Expr1344 { ValueType = new TypeNode1344() };
        failedSelf.Resolver = _ => null;
        var failed = new VmFunctionCall1344(failedSelf, Routine("Foo", 0), new List<Expr1344>(), false);
        var failedCtx = OpenCtx();
        Assert.Null(failed.Resolve(failedCtx));
        Assert.True(failed.Resolved);
        Assert.True(failed.Deleted);
        Assert.Null(failed.Self);
        Assert.False(failed.Continued);
        Assert.False(failed.RelaxedNamedArguments);
        Assert.Equal(0, failed.Implicit);
        Assert.False(failedCtx.Function!.Variant.Implementation!.BlockJit);
        Assert.Equal(1, failedSelf.ResolveCount);
        Assert.Equal(0, failed.AccessibilityCalls);
        Assert.Same(failed, failed.Resolve(failedCtx));
        Assert.Equal(1, failedSelf.ResolveCount);

        var jitSelf = new Expr1344 { ValueType = new TypeNode1344 { IsFunctionPointer = true } };
        var jitArg = new Expr1344();
        var jitFn = Routine("Foo", 0);
        jitFn.Version = ScriptVersion1344.Make(5, 0, 0);
        var jitCall = new VmFunctionCall1344(jitSelf, jitFn, new List<Expr1344> { jitArg }, false);
        var jitCtx = OpenCtx();
        jitCtx.Version = ScriptVersion1344.Make(4, 13, 0);
        var jitCaller = jitCtx.Function!.Variant.Implementation!;
        Assert.Null(jitCall.Resolve(jitCtx));
        Assert.True(jitCaller.BlockJit);
        Assert.True(jitCall.RelaxedNamedArguments);
        Assert.Equal(0, jitCall.Implicit);
        Assert.False(jitCall.Continued);
        Assert.True(jitCall.Deleted);
        Assert.True(jitArg.Deleted);
        Assert.False(jitSelf.Deleted);
        Assert.Null(jitCall.Defaults);
        Assert.Same(jitFn.Variant.ArgNames, jitCall.CapturedArgNames);
        Assert.Equal(1, jitCall.AccessibilityCalls);
        Assert.Same(jitCall, jitCall.Resolve(jitCtx));
        Assert.Equal(1, jitCall.AccessibilityCalls);
        Assert.Equal(1, jitSelf.ResolveCount);
        AssertOnly(jitCall.Log, "error", "Foo not accessible to ZScript version 4.13.0");

        var early = new Expr1344 { ValueType = new TypeNode1344 { IsFunctionPointer = true } };
        var earlyFn = Routine("Foo", 0);
        earlyFn.Version = ScriptVersion1344.Make(5, 0, 0);
        var earlyCall = new VmFunctionCall1344(early, earlyFn, new List<Expr1344>(), false);
        var earlyCtx = OpenCtx();
        earlyCtx.Version = ScriptVersion1344.Make(4, 12, 9);
        Assert.Null(earlyCall.Resolve(earlyCtx));
        Assert.False(earlyCall.RelaxedNamedArguments);
        Assert.True(earlyCtx.Function!.Variant.Implementation!.BlockJit);

        var missingImpl = Routine("Foo", 0);
        missingImpl.Variant.Implementation = null;
        var missingSelf = new Expr1344 { ValueType = new TypeNode1344() };
        var missingCall = new VmFunctionCall1344(missingSelf, missingImpl, new List<Expr1344>(), false);
        var missingEx = Assert.Throws<NullReferenceException>(() => missingCall.Resolve(OpenCtx()));
        Assert.Equal("implementation", missingEx.Message);
        Assert.Equal(1, missingSelf.ResolveCount);
        Assert.False(missingCall.Continued);
        Assert.Null(missingCall.Defaults);
        Assert.Equal(0, missingCall.Implicit);
        Assert.True(missingCall.Resolved);
        Assert.NotNull(missingCall.CapturedArgTypes);

        var noCaller = new VmFunctionCall1344(new Expr1344 { ValueType = new TypeNode1344 { IsFunctionPointer = true } }, Routine("Foo", 0), new List<Expr1344>(), false);
        var noCallerCtx = OpenCtx();
        noCallerCtx.Function = null;
        var noCallerEx = Assert.Throws<NullReferenceException>(() => noCaller.Resolve(noCallerCtx));
        Assert.Equal("caller", noCallerEx.Message);
        Assert.Null(noCaller.Defaults);
        Assert.False(noCaller.Continued);

        var noBody = Routine("Foo", 0);
        var noBodyCall = new VmFunctionCall1344(new Expr1344 { ValueType = new TypeNode1344 { IsFunctionPointer = true } }, noBody, new List<Expr1344>(), false);
        var noBodyCtx = OpenCtx();
        noBodyCtx.Function!.Variant.Implementation = null;
        var noBodyEx = Assert.Throws<NullReferenceException>(() => noBodyCall.Resolve(noBodyCtx));
        Assert.Equal("caller", noBodyEx.Message);

        var noProto = Routine("Foo", 0);
        noProto.Variant.Proto = null;
        var noProtoCall = new VmFunctionCall1344(new Expr1344 { ValueType = new TypeNode1344() }, noProto, new List<Expr1344>(), false);
        var noProtoEx = Assert.Throws<NullReferenceException>(() => noProtoCall.Resolve(OpenCtx()));
        Assert.Equal("proto", noProtoEx.Message);
        Assert.Null(noProtoCall.CapturedArgTypes);

        var ahead = MethodFn("Tick", VmCodegen1344Slice.VarfMethod);
        ahead.Version = ScriptVersion1344.Make(5, 0, 0);
        ahead.SymbolName = "Tick";
        var aheadRecv = new Expr1344 { ValueType = TypeNode1344.PointerTo(ClassOf("Actor", new ClassDesc1344(), 0)) };
        var aheadArg = new Expr1344();
        var aheadCall = CallOn(aheadRecv, "Tick");
        aheadCall.ArgList.Add(aheadArg);
        Wire(aheadCall, ahead);
        var aheadCtx = OpenCtx();
        aheadCtx.Version = ScriptVersion1344.Make(4, 13, 0);
        Assert.Null(aheadCall.Continue(aheadCtx));
        Assert.NotNull(aheadCall.Produced);
        Assert.True(aheadCall.Produced!.Deleted);
        Assert.True(aheadCall.Produced.RelaxedNamedArguments);
        Assert.False(aheadCall.Produced.Continued);
        Assert.False(aheadCtx.Function!.Variant.Implementation!.BlockJit);
        Assert.True(aheadArg.Deleted);
        Assert.False(aheadRecv.Deleted);
        Assert.Null(aheadCall.Self);
        AssertOnly(aheadCall.Log, "error", "Tick not accessible to ZScript version 4.13.0");

        var openMethod = new VmFunctionCall1344(null, MethodFn("Tick", VmCodegen1344Slice.VarfMethod), new List<Expr1344>(), false);
        var openCtx = OpenCtx();
        Assert.Same(openMethod, openMethod.Resolve(openCtx));
        Assert.True(openMethod.Continued);
        Assert.Empty(openMethod.Log.Notes);
    }

    static ContainerType1344 ClassOf(string name, ClassDesc1344? descriptor, int scope)
    {
        return new ContainerType1344
        {
            IsClass = descriptor != null,
            TypeName = name,
            Descriptor = descriptor,
            ScopeFlags = scope,
        };
    }

    static VmCodegen1344Slice CallOn(Expr1344 self, string method)
    {
        var call = new VmCodegen1344Slice { Self = self };
        call.Method = call.Names.Intern(method);
        return call;
    }

    static FunctionSymbol1344 MethodFn(string symbol, int flags, int use = VmCodegen1344Slice.SufActor)
    {
        var fn = Routine(symbol, flags);
        fn.Variant.UseFlags = use;
        return fn;
    }

    static FunctionSymbol1344 Routine(string symbol, int flags)
    {
        var fn = new FunctionSymbol1344
        {
            SymbolName = symbol,
            Version = ScriptVersion1344.Make(0, 0),
        };
        fn.Variant.Flags = flags;
        fn.Variant.Proto = new Proto1344();
        fn.Variant.Implementation = new VmRoutine1344();
        fn.Variant.ArgNames.Add(symbol);
        return fn;
    }

    static void Wire(VmCodegen1344Slice call, FunctionSymbol1344 fn)
    {
        call.Find = _ => new FindResult1344 { Function = fn };
    }

    static CompileCtx1344 OpenCtx()
    {
        var ctx = new CompileCtx1344
        {
            Function = new FunctionSymbol1344(),
            Version = ScriptVersion1344.Make(4, 14, 0),
        };
        ctx.Function.Variant.Implementation = new VmRoutine1344();
        return ctx;
    }

    static VmFunctionCall1344 Direct(FunctionSymbol1344 fn)
    {
        var call = new VmFunctionCall1344(null, fn, new List<Expr1344>(), false);
        call.DirectCompatible = (_, _) => true;
        return call;
    }

    static void AssertOnly(MessageLog1344 log, string level, string text)
    {
        var note = Assert.Single(log.Notes);
        Assert.Equal(level, note.Level);
        Assert.Equal(text, note.Text);
    }
}
