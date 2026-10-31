using S = HCDE.Playsim.VmNetDoCommand1358Slice;

namespace HCDE.Playsim.Tests;

public class VmNetDoCommand1358SliceTests
{
    [Fact]
    public void CrouchThroughCheats()
    {
        Assert.Equal(0, S.GsLevel);
        Assert.Equal(0, S.PstLive);
        Assert.Equal(4, S.BtJump);
        Assert.Equal(10, S.NumWeaponSlots);
        Assert.Equal(2, S.AcsAlways);
        Assert.Equal(8, S.AcsNet);
        Assert.Equal(39, S.DemCrouch);
        Assert.Equal(43, S.DemMorphex);
        Assert.Equal(48, S.DemAddController);
        Assert.Equal(49, S.DemDelController);
        Assert.Equal(50, S.DemKillClassCheat);
        Assert.Equal(68, S.DemRemove);
        Assert.Equal(59, S.DemConvReply);
        Assert.Equal(60, S.DemConvClose);
        Assert.Equal(61, S.DemConvNull);

        var blocked = Host();
        blocked.GameState = 1;
        blocked.TotallyFrozen = _ => throw new InvalidOperationException("frozen");
        S.DoCommand(S.DemCrouch, Raw(), 50, blocked);
        Assert.Empty(blocked.Trace);
        Assert.Equal(0, (int)blocked.Players[0].Crouching);

        var missing = Host();
        missing.Players[0].Mo = null;
        missing.TotallyFrozen = _ => throw new InvalidOperationException("frozen");
        S.DoCommand(S.DemCrouch, Raw(), 0, missing);
        Assert.Empty(missing.Trace);

        var dead = Host();
        dead.Players[0].State = 1;
        dead.TotallyFrozen = _ => throw new InvalidOperationException("frozen");
        S.DoCommand(S.DemCrouch, Raw(), 0, dead);
        Assert.Empty(dead.Trace);

        var jumping = Host();
        jumping.Players[0].OldButtons = 2;
        jumping.Players[0].CrouchDir = -1;
        int frozenCalls = 0;
        jumping.TotallyFrozen = _ =>
        {
            frozenCalls++;
            return false;
        };
        S.DoCommand(S.DemCrouch, Raw(), 0, jumping);
        Assert.Equal(1, frozenCalls);
        Assert.Equal(1, (int)jumping.Players[0].Crouching);

        var held = Host();
        held.Players[0].OldButtons = 5;
        held.TotallyFrozen = _ => throw new InvalidOperationException("frozen");
        S.DoCommand(S.DemCrouch, Raw(), 0, held);
        Assert.Equal(0, (int)held.Players[0].Crouching);

        var down = Host();
        down.TotallyFrozen = who =>
        {
            who.CrouchDir = 4;
            return true;
        };
        down.Players[0].CrouchDir = -1;
        down.Players[0].Crouching = 7;
        S.DoCommand(S.DemCrouch, Raw(), 0, down);
        Assert.Equal(new[] { "frozen" }, down.Trace);
        Assert.Equal(4, (int)down.Players[0].CrouchDir);
        Assert.Equal(7, (int)down.Players[0].Crouching);

        var up = Host();
        up.TotallyFrozen = who =>
        {
            who.CrouchDir = -2;
            return false;
        };
        S.DoCommand(S.DemCrouch, Raw(), 0, up);
        Assert.Equal(1, (int)up.Players[0].Crouching);
        var flat = Host();
        flat.TotallyFrozen = _ => false;
        flat.Players[0].CrouchDir = 0;
        S.DoCommand(S.DemCrouch, Raw(), 0, flat);
        Assert.Equal(-1, (int)flat.Players[0].Crouching);

        var oob = Host(1);
        oob.TotallyFrozen = _ => throw new InvalidOperationException("frozen");
        var oobEx = Assert.Throws<IndexOutOfRangeException>(() => S.DoCommand(S.DemCrouch, Raw(9), 3, oob));
        Assert.Equal("players", oobEx.Message);
        Assert.Equal(1, Raw(9).Size);

        var bare = Raw(0xC0, 0);
        var morphMiss = Host();
        var miss = Assert.Throws<InvalidOperationException>(() => S.DoCommand(S.DemMorphex, bare, 0, morphMiss));
        Assert.Equal("FindActor", miss.Message);
        Assert.Equal(2, bare.Pos);
        Assert.Equal(0, bare.Size);

        var cut = Raw(0x41, 0x42);
        var cutEx = Assert.Throws<InvalidOperationException>(() => S.DoCommand(S.DemMorphex, cut, 0, Host()));
        Assert.Equal(S.ReadPastEnd, cutEx.Message);

        S.ActorClass? seenClass = null;
        S.Player? seenPlayer = null;
        bool quick = true;
        var console = Host();
        console.ConsolePlayer = 0;
        console.FindActor = name =>
        {
            Assert.Equal("\u00C0", name);
            return null;
        };
        console.Morph = (who, cls, undo) =>
        {
            seenPlayer = who;
            seenClass = cls;
            quick = undo;
            return "\0hidden";
        };
        var named = Raw(0xC0, 0, 0x7F);
        S.DoCommand(S.DemMorphex, named, 0, console);
        Assert.Same(console.Players[0], seenPlayer);
        Assert.Null(seenClass);
        Assert.False(quick);
        Assert.Equal("Morph failed.\n", Assert.Single(console.Prints));
        Assert.Equal(new[] { "find", "morph", "print" }, console.Trace);
        Assert.Equal(1, named.Size);

        var other = Host();
        other.ConsolePlayer = 1;
        other.FindActor = name => new S.ActorClass { TypeName = name };
        string returned = "";
        other.Morph = (who, cls, undo) =>
        {
            Assert.Same(other.Players[0], who);
            Assert.Equal("DoomImp", cls!.TypeName);
            Assert.False(undo);
            returned = "Became imp";
            return returned;
        };
        S.DoCommand(S.DemMorphex, Text("DoomImp"), 0, other);
        Assert.Empty(other.Prints);
        Assert.Equal(new[] { "find", "morph" }, other.Trace);

        var emptyMsg = Host();
        emptyMsg.FindActor = _ => new S.ActorClass();
        emptyMsg.Morph = (_, _, _) => "";
        S.DoCommand(S.DemMorphex, Text("X"), 0, emptyMsg);
        Assert.Equal("Morph failed.\n", Assert.Single(emptyMsg.Prints));

        var shown = Host();
        shown.FindActor = _ => null;
        shown.Morph = (_, _, _) => "ok";
        S.DoCommand(S.DemMorphex, Text("X"), 0, shown);
        Assert.Equal("ok\n", Assert.Single(shown.Prints));

        var badMorph = Host(1);
        badMorph.FindActor = name =>
        {
            Assert.Equal("Z", name);
            return new S.ActorClass();
        };
        var badStream = Text("Z");
        var badEx = Assert.Throws<IndexOutOfRangeException>(() => S.DoCommand(S.DemMorphex, badStream, 4, badMorph));
        Assert.Equal("players", badEx.Message);
        Assert.Equal(0, badStream.Size);
        Assert.Equal(new[] { "find" }, badMorph.Trace);

        var self = Host();
        self.ConsolePlayer = 1;
        var selfStream = Raw(1, 9);
        S.DoCommand(S.DemAddController, selfStream, 0, self);
        Assert.True(self.Players[1].SettingsController);
        Assert.False(self.Players[0].SettingsController);
        Assert.Equal("You can now control game settings\n", Assert.Single(self.Prints));
        Assert.Equal(new[] { "print" }, self.Trace);
        Assert.Equal(1, selfStream.Size);

        var granted = Host();
        granted.IsAuthority = () =>
        {
            granted.Players[1].Name = "Ada";
            return true;
        };
        S.DoCommand(S.DemAddController, Raw(1), 0, granted);
        Assert.True(granted.Players[1].SettingsController);
        Assert.Equal("Ada [1] is now a settings controller\n", Assert.Single(granted.Prints));
        Assert.Equal(new[] { "authority", "print" }, granted.Trace);

        var quiet = Host();
        quiet.IsAuthority = () => false;
        S.DoCommand(S.DemAddController, Raw(1), 0, quiet);
        Assert.True(quiet.Players[1].SettingsController);
        Assert.Empty(quiet.Prints);
        Assert.Equal(new[] { "authority" }, quiet.Trace);

        var dropped = Host();
        dropped.ConsolePlayer = 0;
        dropped.Players[0].SettingsController = true;
        S.DoCommand(S.DemDelController, Raw(0), 3, dropped);
        Assert.False(dropped.Players[0].SettingsController);
        Assert.Equal("You can no longer control game settings\n", Assert.Single(dropped.Prints));

        var remote = Host();
        remote.IsAuthority = () => true;
        remote.Players[2].Name = "Bea";
        S.DoCommand(S.DemDelController, Raw(2), 0, remote);
        Assert.False(remote.Players[2].SettingsController);
        Assert.Equal("Bea [2] is no longer a settings controller\n", Assert.Single(remote.Prints));

        var missingPlayer = Host(2);
        var missingStream = Raw(9);
        var missingEx = Assert.Throws<IndexOutOfRangeException>(() =>
            S.DoCommand(S.DemAddController, missingStream, 0, missingPlayer));
        Assert.Equal("players", missingEx.Message);
        Assert.Equal(0, missingStream.Size);
        Assert.False(missingPlayer.Players[0].SettingsController);

        var unknown = Host();
        unknown.Primary = null;
        unknown.FindActor = _ => null;
        S.DoCommand(S.DemKillClassCheat, Text(""), 9, unknown);
        Assert.Equal(" is not an actor class.\n", Assert.Single(unknown.Prints));
        Assert.Equal(new[] { "find", "print" }, unknown.Trace);

        var imp = new S.ActorClass { TypeName = "Demon" };
        var once = Host();
        int massacres = 0;
        once.FindActor = name =>
        {
            Assert.Equal("Imp", name);
            return imp;
        };
        once.Primary!.Massacre = (credit, type) =>
        {
            Assert.False(credit);
            Assert.Equal("Demon", type);
            massacres++;
            return 4;
        };
        imp.Replace = level =>
        {
            Assert.Same(once.Primary, level);
            return imp;
        };
        S.DoCommand(S.DemKillClassCheat, Text("Imp"), 3, once);
        Assert.Equal(1, massacres);
        Assert.Equal("Killed 4 monsters of type Imp.\n", Assert.Single(once.Prints));
        Assert.Equal(new[] { "find", "massacre", "replace", "print" }, once.Trace);

        var rep = new S.ActorClass { TypeName = "Demon2" };
        var twice = Host();
        var types = new List<string>();
        twice.FindActor = _ => imp;
        imp.Replace = level =>
        {
            Assert.Same(twice.Primary, level);
            return rep;
        };
        twice.Primary!.Massacre = (credit, type) =>
        {
            Assert.False(credit);
            types.Add(type);
            return type == "Demon" ? 4 : 1;
        };
        S.DoCommand(S.DemKillClassCheat, Text("Imp"), 0, twice);
        Assert.Equal(new[] { "Demon", "Demon2" }, types);
        Assert.Equal("Killed 5 monsters of type Imp.\n", Assert.Single(twice.Prints));

        var broken = Host();
        broken.FindActor = _ => imp;
        imp.Replace = _ => null;
        broken.Primary!.Massacre = (_, _) => 2;
        var repEx = Assert.Throws<NullReferenceException>(() =>
            S.DoCommand(S.DemKillClassCheat, Text("Imp"), 0, broken));
        Assert.Equal("cls_rep", repEx.Message);
        Assert.Empty(broken.Prints);
        Assert.Equal(new[] { "find", "massacre", "replace" }, broken.Trace);

        var noLevel = Host();
        noLevel.Primary = null;
        noLevel.FindActor = _ => imp;
        var noLevelEx = Assert.Throws<NullReferenceException>(() =>
            S.DoCommand(S.DemKillClassCheat, Text("Imp"), 0, noLevel));
        Assert.Equal("primaryLevel", noLevelEx.Message);
        Assert.Equal(new[] { "find" }, noLevel.Trace);

        var decoration = new S.ActorClass { IsActor = false, TypeName = "Deco" };
        var notActor = Host();
        notActor.Primary = null;
        notActor.FindActor = _ => decoration;
        S.DoCommand(S.DemRemove, Text("Column"), 0, notActor);
        Assert.Equal("Column is not an actor class.\n", Assert.Single(notActor.Prints));

        var prop = new S.ActorClass { TypeName = "Prop", IsActor = true };
        var removed = Host();
        var removedTypes = new List<S.ActorClass>();
        removed.FindActor = _ => prop;
        prop.Replace = _ => prop;
        removed.Primary!.RemoveClass = cls =>
        {
            removedTypes.Add(cls);
            return 3;
        };
        S.DoCommand(S.DemRemove, Text("Prop"), 1, removed);
        Assert.Same(prop, Assert.Single(removedTypes));
        Assert.Equal("Removed 3 actors of type Prop.\n", Assert.Single(removed.Prints));
        Assert.Equal(new[] { "find", "remove", "replace", "print" }, removed.Trace);

        var propRep = new S.ActorClass { TypeName = "PropRep" };
        var both = Host();
        var bothTypes = new List<S.ActorClass>();
        both.FindActor = _ => prop;
        prop.Replace = _ => propRep;
        both.Primary!.RemoveClass = cls =>
        {
            bothTypes.Add(cls);
            return cls == prop ? 2 : 6;
        };
        S.DoCommand(S.DemRemove, Text("Prop"), 0, both);
        Assert.Equal(2, bothTypes.Count);
        Assert.Same(prop, bothTypes[0]);
        Assert.Same(propRep, bothTypes[1]);
        Assert.Equal("Removed 8 actors of type Prop.\n", Assert.Single(both.Prints));
    }

    [Fact]
    public void SlotsEventsAndRunScript()
    {
        Assert.Equal(55, S.DemAddSlotDefault);
        Assert.Equal(56, S.DemAddSlot);
        Assert.Equal(57, S.DemSetSlot);
        Assert.Equal(63, S.DemSetPitchLimit);
        Assert.Equal(66, S.DemRevertCamera);
        Assert.Equal(67, S.DemSetSlotPnum);
        Assert.Equal(69, S.DemFinishGame);
        Assert.Equal(70, S.DemNetEvent);
        Assert.Equal(73, S.DemEndScreenJob);
        Assert.Equal(74, S.DemZscCmd);
        Assert.Equal(75, S.DemChangeSkill);
        Assert.Equal(76, S.DemKick);
        Assert.Equal(77, S.DemReadied);
        Assert.Equal(78, S.DemWeapSelect);
        Assert.Equal(79, S.DemUseFlechette);

        var talked = new List<(int Cmd, int Player, byte Next)>();
        var talk = Host();
        talk.Conversation = (cmd, player, stream) =>
        {
            talked.Add((cmd, player, S.ReadInt8(stream)));
        };
        S.DoCommand(S.DemConvReply, Raw(4, 1), 2, talk);
        S.DoCommand(S.DemConvClose, Raw(5), 2, talk);
        S.DoCommand(S.DemConvNull, Raw(6), 7, talk);
        Assert.Equal((S.DemConvReply, 2, (byte)4), talked[0]);
        Assert.Equal((S.DemConvClose, 2, (byte)5), talked[1]);
        Assert.Equal((S.DemConvNull, 7, (byte)6), talked[2]);
        Assert.Equal(new[] { "talk", "talk", "talk" }, talk.Trace);
        Assert.Empty(talk.Prints);

        var silent = Raw(4);
        var noTalk = Host();
        var noTalkEx = Assert.Throws<InvalidOperationException>(() =>
            S.DoCommand(S.DemConvReply, silent, 0, noTalk));
        Assert.Equal("P_ConversationCommand", noTalkEx.Message);
        Assert.Equal(0, silent.Pos);

        var bow = new S.ActorClass { TypeName = "Bow", IsWeapon = true };
        var gun = new S.ActorClass { TypeName = "Gun", IsWeapon = true };
        var old = new S.ActorClass { TypeName = "Old" };
        var slots = Host();
        slots.ConsolePlayer = 1;
        slots.Players[1].Weapons.Slots[1].Add(old);
        int reads = 0;
        slots.ReadWeapon = stream =>
        {
            reads++;
            int index = S.ReadInt8(stream);
            if ((index & 0x80) != 0)
                index = (index & 0x7F) | (S.ReadInt8(stream) << 7);
            return index == 1 ? bow : gun;
        };
        S.DoCommand(S.DemSetSlotPnum, Raw(1, 1, 2, 0x81, 0x00, 2), 0, slots);
        Assert.Equal(2, reads);
        Assert.Equal(2, slots.Players[1].Weapons.Slots[1].Count);
        Assert.Same(bow, slots.Players[1].Weapons.Slots[1][0]);
        Assert.Same(gun, slots.Players[1].Weapons.Slots[1][1]);
        Assert.DoesNotContain(old, slots.Players[1].Weapons.Slots[1]);
        Assert.Equal("clear", slots.Players[1].Weapons.Calls[0].Kind);
        Assert.Equal(1, slots.Players[1].Weapons.Calls[0].Slot);
        Assert.True(slots.Players[1].Weapons.Calls[1].Feedback);
        Assert.True(slots.Players[1].Weapons.Calls[2].Feedback);
        Assert.Same(bow, slots.Players[1].Weapons.Calls[1].Type);
        Assert.Empty(slots.Prints);

        var sender = Host();
        sender.ConsolePlayer = 0;
        sender.ReadWeapon = stream =>
        {
            Assert.Equal(3, S.ReadInt8(stream));
            return gun;
        };
        S.DoCommand(S.DemSetSlotPnum, Raw(1, 4, 1, 3), 0, sender);
        Assert.False(sender.Players[1].Weapons.Calls[1].Feedback);
        Assert.Same(gun, Assert.Single(sender.Players[1].Weapons.Slots[4]));

        var cleared = Host();
        cleared.ReadWeapon = _ => throw new InvalidOperationException("Net_ReadWeapon");
        S.DoCommand(S.DemSetSlot, Raw(2, 0), 0, cleared);
        Assert.Equal("clear", Assert.Single(cleared.Players[0].Weapons.Calls).Kind);
        Assert.Equal(new[] { "clear" }, cleared.Trace);

        var late = Host(1);
        late.ReadWeapon = _ => throw new InvalidOperationException("Net_ReadWeapon");
        var lateStream = Raw(4, 2, 9, 7);
        var lateEx = Assert.Throws<IndexOutOfRangeException>(() =>
            S.DoCommand(S.DemSetSlotPnum, lateStream, 0, late));
        Assert.Equal("players", lateEx.Message);
        Assert.Equal(1, lateStream.Size);
        Assert.Empty(late.Players[0].Weapons.Calls);

        var badSlot = Host();
        badSlot.ConsolePlayer = 0;
        badSlot.ReadWeapon = stream =>
        {
            Assert.Equal(1, S.ReadInt8(stream));
            return bow;
        };
        var badSlotStream = Raw(10, 2, 1, 2);
        var slotEx = Assert.Throws<IndexOutOfRangeException>(() =>
            S.DoCommand(S.DemSetSlot, badSlotStream, 0, badSlot));
        Assert.Equal("Slots", slotEx.Message);
        Assert.Equal(1, badSlotStream.Size);
        Assert.DoesNotContain(badSlot.Players[0].Weapons.Calls, c => c.Kind == "clear");
        var added = Assert.Single(badSlot.Players[0].Weapons.Calls);
        Assert.Equal("add", added.Kind);
        Assert.Equal(10, added.Slot);
        Assert.True(added.Feedback);
        Assert.Same(bow, added.Type);

        var nullWeapon = Host();
        nullWeapon.ReadWeapon = stream =>
        {
            S.ReadInt8(stream);
            return null;
        };
        S.DoCommand(S.DemSetSlot, Raw(15, 2, 0, 0), 0, nullWeapon);
        Assert.Equal(2, nullWeapon.Players[0].Weapons.Calls.Count);
        Assert.All(nullWeapon.Players[0].Weapons.Calls, call => Assert.Equal("add", call.Kind));
        Assert.Equal(0, nullWeapon.Players[0].Weapons.Slots.Sum(s => s.Count));

        var localAdd = Host();
        localAdd.ConsolePlayer = 0;
        localAdd.ReadWeapon = stream =>
        {
            S.ReadInt8(stream);
            return bow;
        };
        var addStream = Raw(3, 1, 9);
        S.DoCommand(S.DemAddSlot, addStream, 0, localAdd);
        var addCall = Assert.Single(localAdd.Players[0].Weapons.Calls);
        Assert.Equal("add", addCall.Kind);
        Assert.Equal(3, addCall.Slot);
        Assert.True(addCall.Feedback);
        Assert.Same(bow, Assert.Single(localAdd.Players[0].Weapons.Slots[3]));
        Assert.Equal(1, addStream.Size);

        var remoteAdd = Host();
        remoteAdd.ConsolePlayer = 1;
        remoteAdd.ReadWeapon = stream =>
        {
            S.ReadInt8(stream);
            return null;
        };
        var remoteStream = Raw(8, 0);
        var remoteEx = Assert.Throws<IndexOutOfRangeException>(() =>
            S.DoCommand(S.DemAddSlot, remoteStream, 9, remoteAdd));
        Assert.Equal("players", remoteEx.Message);
        Assert.Equal(0, remoteStream.Size);
        Assert.Equal(new[] { "weapon-read" }, remoteAdd.Trace);

        var notWeapon = new S.ActorClass { IsWeapon = false };
        var skipped = Host();
        skipped.ReadWeapon = _ => notWeapon;
        S.DoCommand(S.DemAddSlotDefault, Raw(99), 0, skipped);
        var skipCall = Assert.Single(skipped.Players[0].Weapons.Calls);
        Assert.Equal("default", skipCall.Kind);
        Assert.Equal(99, skipCall.Slot);
        Assert.True(skipCall.Feedback);
        Assert.Equal(0, skipped.Players[0].Weapons.Slots.Sum(s => s.Count));

        var placed = Host();
        placed.ReadWeapon = _ => bow;
        S.DoCommand(S.DemAddSlotDefault, Raw(2), 0, placed);
        Assert.Same(bow, Assert.Single(placed.Players[0].Weapons.Slots[2]));
        Assert.Equal("default", Assert.Single(placed.Players[0].Weapons.Calls).Kind);

        var def = Host();
        def.ConsolePlayer = 0;
        def.ReadWeapon = _ => bow;
        var defEx = Assert.Throws<IndexOutOfRangeException>(() =>
            S.DoCommand(S.DemAddSlotDefault, Raw(12), 1, def));
        Assert.Equal("Slots", defEx.Message);
        var defCall = Assert.Single(def.Players[1].Weapons.Calls);
        Assert.Equal("default", defCall.Kind);
        Assert.False(defCall.Feedback);
        Assert.Same(bow, defCall.Type);

        var pitched = Host();
        pitched.Players[0].MinPitch = 44;
        pitched.Players[0].MaxPitch = 55;
        var oneByte = Raw(30);
        var pitchEx = Assert.Throws<InvalidOperationException>(() =>
            S.DoCommand(S.DemSetPitchLimit, oneByte, 0, pitched));
        Assert.Equal(S.ReadPastEnd, pitchEx.Message);
        Assert.Equal(-30d, pitched.Players[0].MinPitch);
        Assert.Equal(55d, pitched.Players[0].MaxPitch);

        var angles = Host();
        S.DoCommand(S.DemSetPitchLimit, Raw(255, 128), 1, angles);
        Assert.Equal(-255d, angles.Players[1].MinPitch);
        Assert.Equal(128d, angles.Players[1].MaxPitch);
        Assert.Equal(0d, angles.Players[0].MinPitch);

        var pitchOob = Host(1);
        var pitchStream = Raw(10, 20);
        var pitchOobEx = Assert.Throws<IndexOutOfRangeException>(() =>
            S.DoCommand(S.DemSetPitchLimit, pitchStream, 4, pitchOob));
        Assert.Equal("players", pitchOobEx.Message);
        Assert.Equal(1, pitchStream.Size);
        Assert.Equal(0d, pitchOob.Players[0].MinPitch);

        var cam = Host();
        cam.Players[0].Camera = new S.Actor();
        S.DoCommand(S.DemRevertCamera, Raw(1), 0, cam);
        Assert.Same(cam.Players[0].Mo, cam.Players[0].Camera);
        cam.Players[0].Mo = null;
        S.DoCommand(S.DemRevertCamera, Raw(), 0, cam);
        Assert.Null(cam.Players[0].Camera);

        var finish = Host();
        var finishStream = Raw(1, 2, 3);
        S.DoCommand(S.DemFinishGame, finishStream, 99, finish);
        var change = Assert.Single(finish.Primary!.Changes);
        Assert.Null(change.Name);
        Assert.Equal(0, change.Position);
        Assert.Equal(0, change.Flags);
        Assert.Equal(0, finishStream.Pos);
        Assert.Equal(new[] { "change-level" }, finish.Trace);

        finish.Primary = null;
        var finishEx = Assert.Throws<NullReferenceException>(() =>
            S.DoCommand(S.DemFinishGame, Raw(), 0, finish));
        Assert.Equal("primaryLevel", finishEx.Message);

        var buy = new List<(int Player, string Name, int A, int B, int C)>();
        var ev = Host();
        ev.Primary = null;
        ev.Predator = (player, name, a, b, c) =>
        {
            buy.Add((player, name, a, b, c));
            return true;
        };
        var evStream = Packet("buy", 10, 1, 2, -4, 2, 0xEE);
        S.DoCommand(S.DemNetEvent, evStream, 3, ev);
        Assert.Equal((3, "buy", 1, 2, -4), Assert.Single(buy));
        Assert.Equal(0xEE, evStream.Buf[evStream.Pos]);
        Assert.Equal(1, evStream.Size);
        Assert.Equal(new[] { "predator" }, ev.Trace);

        var observed = Host();
        observed.Predator = (_, _, _, _, _) => false;
        var obsStream = Packet("ping", 0, 8, 9, 10, 0, 0x11);
        S.DoCommand(S.DemNetEvent, obsStream, 1, observed);
        var console = Assert.Single(observed.Primary!.Consoles);
        Assert.Equal(1, console.Player);
        Assert.Equal("ping", console.Name);
        Assert.Equal(8, console.A);
        Assert.Equal(9, console.B);
        Assert.Equal(10, console.C);
        Assert.False(console.Manual);
        Assert.False(console.FromScript);
        Assert.Equal(0x11, obsStream.Buf[obsStream.Pos]);
        Assert.Equal(new[] { "predator", "console" }, observed.Trace);

        var droppedEvent = Host();
        droppedEvent.Primary = null;
        droppedEvent.Predator = (_, _, _, _, _) => false;
        var dropEx = Assert.Throws<NullReferenceException>(() =>
            S.DoCommand(S.DemNetEvent, Packet("ping", 1, 1, 1, 1, 1), 0, droppedEvent));
        Assert.Equal("primaryLevel", dropEx.Message);
        Assert.Equal(new[] { "predator" }, droppedEvent.Trace);

        var shortEvent = Host();
        shortEvent.Predator = (_, _, _, _, _) => throw new InvalidOperationException("early");
        var shortBytes = new List<byte>();
        AddStr(shortBytes, "ping");
        shortBytes.Add(1);
        AddI32(shortBytes, 1);
        AddI32(shortBytes, 1);
        AddI32(shortBytes, 1);
        var shortEx = Assert.Throws<InvalidOperationException>(() =>
            S.DoCommand(S.DemNetEvent, Raw(shortBytes.ToArray()), 0, shortEvent));
        Assert.Equal(S.ReadPastEnd, shortEx.Message);
        Assert.Empty(shortEvent.Trace);

        var screen = Host();
        S.DoCommand(S.DemEndScreenJob, Raw(), 0, screen);
        S.DoCommand(S.DemEndScreenJob, Raw(), 0, screen);
        S.DoCommand(S.DemReadied, Raw(), 4, screen);
        S.DoCommand(S.DemUseFlechette, Raw(9), 4, screen);
        Assert.Equal(2, screen.EndScreens);
        Assert.Equal(4, Assert.Single(screen.Readied));
        Assert.Equal(4, Assert.Single(screen.Flechettes));
        var pickStream = Raw(6);
        S.DoCommand(S.DemWeapSelect, pickStream, 2, screen);
        Assert.Equal((2, 6), Assert.Single(screen.WeaponPicks));
        Assert.Equal(0, pickStream.Size);
        var pickEx = Assert.Throws<InvalidOperationException>(() =>
            S.DoCommand(S.DemWeapSelect, Raw(), 2, screen));
        Assert.Equal(S.ReadPastEnd, pickEx.Message);
        Assert.Single(screen.WeaponPicks);

        var skill = Host();
        skill.NextSkill = -999;
        var skillEx = Assert.Throws<InvalidOperationException>(() =>
            S.DoCommand(S.DemChangeSkill, Raw(0, 0, 1), 0, skill));
        Assert.Equal(S.ReadPastEnd, skillEx.Message);
        Assert.Equal(-999, skill.NextSkill);
        S.DoCommand(S.DemChangeSkill, Raw(0xFF, 0xFF, 0xFF, 0xFB), 8, skill);
        Assert.Equal(-5, skill.NextSkill);

        var emptyCmd = Host();
        var emptyStream = Raw(0, 0, 0, 0xAB);
        S.DoCommand(S.DemZscCmd, emptyStream, 6, emptyCmd);
        var emptyNet = Assert.Single(emptyCmd.Primary!.NetCommands);
        Assert.Equal(6, emptyNet.Player);
        Assert.Equal("", emptyNet.Name);
        Assert.Empty(emptyNet.Bytes);
        Assert.Equal(0xAB, emptyStream.Buf[emptyStream.Pos]);

        var zsc = Host();
        var zscBytes = new List<byte>();
        AddStr(zscBytes, "Give");
        zscBytes.Add(0);
        zscBytes.Add(2);
        zscBytes.Add(0x10);
        zscBytes.Add(0x20);
        zscBytes.Add(0x30);
        var zscStream = Raw(zscBytes.ToArray());
        S.DoCommand(S.DemZscCmd, zscStream, 1, zsc);
        var net = Assert.Single(zsc.Primary!.NetCommands);
        Assert.Equal("Give", net.Name);
        Assert.Equal(new byte[] { 0x10, 0x20 }, net.Bytes);
        Assert.Equal(0x30, zscStream.Buf[zscStream.Pos]);

        var huge = Host();
        huge.Primary = null;
        var hugeBytes = new List<byte>();
        AddStr(hugeBytes, "X");
        hugeBytes.Add(0xFF);
        hugeBytes.Add(0xFF);
        var hugeEx = Assert.Throws<InvalidOperationException>(() =>
            S.DoCommand(S.DemZscCmd, Raw(hugeBytes.ToArray()), 0, huge));
        Assert.Equal(S.ReadPastEnd, hugeEx.Message);
        Assert.Empty(huge.Trace);

        var partial = Host();
        partial.Primary = null;
        var partialBytes = new List<byte>();
        AddStr(partialBytes, "X");
        partialBytes.Add(0);
        partialBytes.Add(2);
        partialBytes.Add(0x01);
        var partialEx = Assert.Throws<InvalidOperationException>(() =>
            S.DoCommand(S.DemZscCmd, Raw(partialBytes.ToArray()), 0, partial));
        Assert.Equal(S.ReadPastEnd, partialEx.Message);

        var noMap = Host();
        noMap.Primary = null;
        var noMapEx = Assert.Throws<NullReferenceException>(() =>
            S.DoCommand(S.DemZscCmd, Raw(0, 0, 0), 0, noMap));
        Assert.Equal("primaryLevel", noMapEx.Message);

        var kickSelf = Host();
        kickSelf.ConsolePlayer = 1;
        kickSelf.GameTic = 10;
        var kickStream = Raw(1, 2);
        var kickEx = Assert.Throws<InvalidOperationException>(() =>
            S.DoCommand(S.DemKick, kickStream, 0, kickSelf));
        Assert.Equal("You have been kicked from the game", kickEx.Message);
        Assert.Equal(1, kickStream.Size);
        Assert.Empty(kickSelf.Prints);
        Assert.Empty(kickSelf.Disconnects);
        Assert.Empty(kickSelf.Trace);

        var kickMissing = Host();
        kickMissing.InGame = n => n == 9;
        var kickMissingEx = Assert.Throws<IndexOutOfRangeException>(() =>
            S.DoCommand(S.DemKick, Raw(9), 2, kickMissing));
        Assert.Equal("players", kickMissingEx.Message);
        Assert.Equal(new[] { "in-game" }, kickMissing.Trace);
        Assert.Empty(kickMissing.Warnings);
        Assert.Empty(kickMissing.Disconnects);

        var kickOut = Host();
        kickOut.InGame = _ => false;
        var kickOutStream = Raw(1, 0x55);
        S.DoCommand(S.DemKick, kickOutStream, 0, kickOut);
        Assert.Equal(new[] { "in-game" }, kickOut.Trace);
        Assert.Empty(kickOut.Prints);
        Assert.Equal(0x55, kickOutStream.Buf[kickOutStream.Pos]);

        var kick = Host();
        kick.ConsolePlayer = 0;
        kick.GameTic = 40;
        kick.ClientTic = 41;
        kick.Room = uint.MaxValue;
        kick.Players[1].Name = "Cara";
        kick.InGame = n => n == 1;
        S.DoCommand(S.DemKick, Raw(1), 2, kick);
        Assert.Equal("Cara [1] has been kicked from the game\n", Assert.Single(kick.Prints));
        var warning = Assert.Single(kick.Warnings);
        Assert.Equal("net", warning.Channel);
        Assert.Equal("kick command disconnecting player=1 by=2 gametic=40 clienttic=41 room=4294967295", warning.Text);
        Assert.Equal(1, Assert.Single(kick.Disconnects));
        Assert.Equal(new[] { "in-game", "print", "warn", "disconnect" }, kick.Trace);

        var noHook = Host();
        var noHookStream = Raw(1);
        var noHookEx = Assert.Throws<InvalidOperationException>(() =>
            S.DoCommand(S.DemKick, noHookStream, 0, noHook));
        Assert.Equal("NetworkClients.InGame", noHookEx.Message);
        Assert.Equal(0, noHookStream.Size);

        var unknown = Raw(1, 2, 3);
        var unknownEx = Assert.Throws<InvalidOperationException>(() =>
            S.DoCommand(32, unknown, 0, Host()));
        Assert.Equal("Unknown net command: 32", unknownEx.Message);
        Assert.Equal(0, unknown.Pos);
        var neg = Assert.Throws<InvalidOperationException>(() => S.DoCommand(-1, Raw(9), 0, Host()));
        Assert.Equal("Unknown net command: -1", neg.Message);

        var world = Host();
        var pawn = world.Players[0].Mo!;
        var untouched = Raw(0, 0, 0, 4);
        S.RunScript(untouched, null, 7, 5, S.AcsAlways, world);
        Assert.Equal(0, untouched.Pos);
        Assert.Empty(world.Primary!.Scripts);
        world.Primary = null;
        S.RunScript(untouched, null, 7, 5, 0, world);
        Assert.Equal(0, untouched.Pos);

        world.Primary = new S.Level { MapName = "E1M1" };
        pawn.Level = null;
        var none = Raw(1);
        S.RunScript(none, pawn, -4, 0, 0, world);
        var zero = Assert.Single(world.Primary.Scripts);
        Assert.Null(zero.PawnLevel);
        Assert.Same(pawn, zero.Pawn);
        Assert.Null(zero.Line);
        Assert.Equal(-4, zero.Number);
        Assert.Equal("E1M1", zero.Map);
        Assert.Equal(new[] { 0, 0, 0, 0 }, zero.Args);
        Assert.Equal(0, zero.Count);
        Assert.Equal(S.AcsNet, zero.Flags);
        Assert.Equal(1, none.Size);

        world.Primary.Scripts.Clear();
        world.Trace.Clear();
        pawn.Level = "pawn-level";
        var args = new List<byte>();
        AddI32(args, 11);
        AddI32(args, 22);
        AddI32(args, 33);
        AddI32(args, 44);
        AddI32(args, 55);
        AddI32(args, 66);
        var scriptStream = Raw(args.ToArray());
        S.RunScript(scriptStream, pawn, 9, 6, S.AcsAlways, world);
        var started = Assert.Single(world.Primary.Scripts);
        Assert.Equal("pawn-level", started.PawnLevel);
        Assert.Equal(9, started.Number);
        Assert.Equal(new[] { 11, 22, 33, 44 }, started.Args);
        Assert.Equal(4, started.Count);
        Assert.Equal(S.AcsNet | S.AcsAlways, started.Flags);
        Assert.Equal(0, scriptStream.Size);
        Assert.Equal(new[] { "script" }, world.Trace);

        world.Primary.Scripts.Clear();
        var shortScript = new List<byte>();
        AddI32(shortScript, 1);
        AddI32(shortScript, 2);
        AddI32(shortScript, 3);
        AddI32(shortScript, 4);
        var shortStream = Raw(shortScript.ToArray());
        var scriptEx = Assert.Throws<InvalidOperationException>(() =>
            S.RunScript(shortStream, pawn, 1, 5, 0, world));
        Assert.Equal(S.ReadPastEnd, scriptEx.Message);
        Assert.Empty(world.Primary.Scripts);
        Assert.Equal(0, shortStream.Size);

        var negative = Raw(9);
        S.RunScript(negative, pawn, 3, -1, 0x11, world);
        var negCall = Assert.Single(world.Primary.Scripts);
        Assert.Equal(-1, negCall.Count);
        Assert.Equal(S.AcsNet | 0x11, negCall.Flags);
        Assert.Equal(new[] { 0, 0, 0, 0 }, negCall.Args);
        Assert.Equal(1, negative.Size);

        world.Primary = null;
        var afterRead = Raw(0, 0, 0, 0);
        var mapEx = Assert.Throws<NullReferenceException>(() =>
            S.RunScript(afterRead, pawn, 1, 0, 0, world));
        Assert.Equal("primaryLevel", mapEx.Message);
        Assert.Equal(4, afterRead.Size);
    }

    static S.World Host(int players = 4)
    {
        var world = new S.World { Primary = new S.Level { MapName = "MAP01" } };
        for (int i = 0; i < players; i++)
        {
            world.Players.Add(new S.Player
            {
                Name = "p" + i,
                Mo = new S.Actor { Level = "L" + i },
            });
        }

        return world;
    }

    static S.NetStream Raw(params byte[] data) => new() { Buf = data, End = data.Length };

    static S.NetStream Text(string text)
    {
        var data = new List<byte>();
        AddStr(data, text);
        return Raw(data.ToArray());
    }

    static S.NetStream Packet(string name, byte argn, int a, int b, int c, byte manual, byte trail = 0)
    {
        var data = new List<byte>();
        AddStr(data, name);
        data.Add(argn);
        AddI32(data, a);
        AddI32(data, b);
        AddI32(data, c);
        data.Add(manual);
        if (trail != 0)
            data.Add(trail);
        return Raw(data.ToArray());
    }

    static void AddStr(List<byte> data, string text)
    {
        for (int i = 0; i < text.Length; i++)
            data.Add((byte)text[i]);
        data.Add(0);
    }

    static void AddI32(List<byte> data, int value)
    {
        data.Add((byte)(value >> 24));
        data.Add((byte)(value >> 16));
        data.Add((byte)(value >> 8));
        data.Add((byte)value);
    }
}
