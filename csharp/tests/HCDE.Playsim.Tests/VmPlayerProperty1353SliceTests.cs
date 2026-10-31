using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmPlayerProperty1353SliceTests
{
    [Fact]
    public void CameraTailAndPowers()
    {
        var player = new Player1353 { Cheats = VmPlayerProperty1353Slice.CfRevertPlease | VmPlayerProperty1353Slice.CfGodMode };
        var activator = new Actor1353 { Player = player };
        var old = new Actor1353();
        var cleared = new List<Actor1353?>();
        Assert.True(VmPlayerProperty1353Slice.RestoreActivatorCamera(player, activator, old, cleared.Add));
        Assert.Same(activator, player.Camera);
        Assert.Equal(VmPlayerProperty1353Slice.CfGodMode, player.Cheats);
        Assert.Same(activator, Assert.Single(cleared));

        Assert.True(VmPlayerProperty1353Slice.RestoreActivatorCamera(player, activator, activator, null));
        Assert.Single(cleared);
        Assert.Equal(VmPlayerProperty1353Slice.CfGodMode, player.Cheats);

        player.Camera = old;
        player.Cheats |= VmPlayerProperty1353Slice.CfRevertPlease;
        Assert.Equal("R_ClearPastViewer", Assert.Throws<InvalidOperationException>(() =>
            VmPlayerProperty1353Slice.RestoreActivatorCamera(player, activator, old, null)).Message);
        Assert.Same(activator, player.Camera);
        Assert.Equal(VmPlayerProperty1353Slice.CfGodMode, player.Cheats);
        Assert.Equal("player", Assert.Throws<NullReferenceException>(() =>
            VmPlayerProperty1353Slice.RestoreActivatorCamera(null, activator, old, cleared.Add)).Message);

        var level = new Level1353();
        var actor = new Actor1353();
        int finds = 0;
        level.FindActor = name =>
        {
            finds++;
            return new ActorClass1353(name);
        };
        Assert.False(VmPlayerProperty1353Slice.SetPlayerProperty(null, 0, 1, VmPlayerProperty1353Slice.PropInvulnerability, level));
        Assert.False(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropInvulnerability, level));
        Assert.Equal(0, finds);

        actor.Player = player;
        Assert.False(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropUnused1, level));
        Assert.False(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropUnused2, level));
        Assert.Equal(0, finds);

        level.ConsolePlayer = player;
        level.Flags2 = 0x10;
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropAllMap, level));
        Assert.Equal(0x12, level.Flags2);
        var other = new Player1353();
        actor.Player = other;
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropAllMap, level));
        Assert.Equal(0x12, level.Flags2);
        actor.Player = player;
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 0, VmPlayerProperty1353Slice.PropAllMap, level));
        Assert.Equal(0x10, level.Flags2);

        var gives = new List<ActorClass1353?>();
        var item = new Inventory1353 { BlendColor = 7 };
        actor.Give = type =>
        {
            gives.Add(type);
            return item;
        };
        level.FindActor = name =>
        {
            finds++;
            return new ActorClass1353(name);
        };
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropInvulnerability, level));
        Assert.Equal("PowerInvulnerable", Assert.Single(gives)!.Name);
        Assert.Equal(0x00b60000u, item.BlendColor);

        item.BlendColor = 7;
        gives.Clear();
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropInfrared, level));
        Assert.Equal("PowerLightAmp", Assert.Single(gives)!.Name);
        Assert.Equal(7u, item.BlendColor);
        gives.Clear();
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 2, VmPlayerProperty1353Slice.PropInvulnerability, level));
        Assert.Equal(7u, item.BlendColor);
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropStrength, level));
        Assert.Equal(7u, item.BlendColor);
        Assert.Equal("PowerStrength", gives[^1]!.Name);
        gives.Clear();
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropFlight, level));
        Assert.Equal("PowerFlight", Assert.Single(gives)!.Name);
        Assert.Equal(0, actor.Flags2);

        actor.Give = _ => null;
        item.BlendColor = 4;
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropInvulnerability, level));
        Assert.Equal(4u, item.BlendColor);

        var taken = new Inventory1353();
        string? takenName = null;
        bool takenSubclass = false;
        actor.FindByName = (name, subclass) =>
        {
            takenName = name;
            takenSubclass = subclass;
            return taken;
        };
        int findsBeforeTake = finds;
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 0, VmPlayerProperty1353Slice.PropInfrared, level));
        Assert.Equal("PowerLightAmp", takenName);
        Assert.True(takenSubclass);
        Assert.True(taken.Destroyed);
        Assert.Equal(findsBeforeTake, finds);

        actor.FindByName = (_, _) => null;
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 0, VmPlayerProperty1353Slice.PropSpeed, level));

        var crowd = new Level1353();
        int crowdFinds = 0;
        crowd.FindActor = name =>
        {
            crowdFinds++;
            return new ActorClass1353(name);
        };
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(null, 1, 1, VmPlayerProperty1353Slice.PropInvulnerability, crowd));
        Assert.Equal(0, crowdFinds);

        var mo = new Actor1353();
        var seated = new Player1353 { Mo = mo };
        crowd.InGame[3] = true;
        crowd.Players[3] = new Player1353 { Mo = null };
        crowd.InGame[4] = true;
        crowd.Players[4] = seated;
        crowd.ConsolePlayer = seated;
        var given = new Inventory1353();
        var moGives = new List<ActorClass1353?>();
        mo.Give = type =>
        {
            moGives.Add(type);
            return given;
        };
        crowd.Flags2 = 1;
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(null, 2, 1, VmPlayerProperty1353Slice.PropInvulnerability, crowd));
        Assert.Equal("PowerInvulnerable", Assert.Single(moGives)!.Name);
        Assert.Equal(0x00b60000u, given.BlendColor);
        Assert.Equal(1, crowd.Flags2);

        moGives.Clear();
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(null, 1, 1, VmPlayerProperty1353Slice.PropAllMap, crowd));
        Assert.Empty(moGives);
        Assert.Equal(3, crowd.Flags2);

        crowd.Players[4] = new Player1353 { Mo = mo };
        crowd.InGame[0] = true;
        crowd.Players[0] = crowd.ConsolePlayer;
        crowd.ConsolePlayer!.Mo = null;
        crowd.Flags2 = 1;
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(null, 1, 1, VmPlayerProperty1353Slice.PropAllMap, crowd));
        Assert.Equal(1, crowd.Flags2);

        crowd.ConsolePlayer = seated;
        seated.Mo = mo;
        crowd.Players[0] = seated;
        var removed = new Inventory1353();
        ActorClass1353? removedClass = null;
        mo.FindByClass = type =>
        {
            removedClass = type;
            return removed;
        };
        mo.FindByName = (_, _) => throw new InvalidOperationException("name");
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(null, 1, 0, VmPlayerProperty1353Slice.PropFlight, crowd));
        Assert.Equal("PowerFlight", removedClass!.Name);
        Assert.True(removed.Destroyed);
        Assert.Equal(0, mo.Flags2);

        crowd.InGame[1] = true;
        crowd.Players[1] = null;
        Assert.Equal("player", Assert.Throws<NullReferenceException>(() =>
            VmPlayerProperty1353Slice.SetPlayerProperty(null, 1, 1, VmPlayerProperty1353Slice.PropStrength, crowd)).Message);
    }

    [Fact]
    public void FlagMasksAndFrozenLink()
    {
        var level = new Level1353();
        var player = new Player1353 { Cheats = 0x5 };
        var actor = new Actor1353
        {
            Player = player,
            Flags = 0x1,
            Flags2 = 0x2,
            Flags7 = 0x4,
        };
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropNoTarget, level));
        Assert.Equal(0x5 | VmPlayerProperty1353Slice.CfNoTarget, player.Cheats);
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 0, VmPlayerProperty1353Slice.PropNoTarget, level));
        Assert.Equal(0x5, player.Cheats);

        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropBuddha, level));
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropBuddha2, level));
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropInstantWeaponSwitch, level));
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropFrightening, level));
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropGodMode, level));
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropGodMode2, level));
        Assert.Equal(
            0x5
            | VmPlayerProperty1353Slice.CfBuddha
            | VmPlayerProperty1353Slice.CfBuddha2
            | VmPlayerProperty1353Slice.CfInstantWeapSwitch
            | VmPlayerProperty1353Slice.CfFrightening
            | VmPlayerProperty1353Slice.CfGodMode
            | VmPlayerProperty1353Slice.CfGodMode2,
            player.Cheats);

        player.Cheats = 0x5;
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropNoClip2, level));
        Assert.Equal(0x5 | VmPlayerProperty1353Slice.CfNoClip | VmPlayerProperty1353Slice.CfNoClip2, player.Cheats);

        player.Cheats = VmPlayerProperty1353Slice.CfFly | 0x5;
        actor.Flags = 0x1 | VmPlayerProperty1353Slice.MfNoGravity;
        actor.Flags2 = 0x2 | VmPlayerProperty1353Slice.Mf2Fly;
        actor.Flags7 = 0x4 | VmPlayerProperty1353Slice.Mf7FlyCheat;
        Assert.False(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropFly, level));
        Assert.Equal(VmPlayerProperty1353Slice.CfFly | 0x5, player.Cheats);
        Assert.Equal(0x1 | VmPlayerProperty1353Slice.MfNoGravity, actor.Flags);
        Assert.Equal(0x2 | VmPlayerProperty1353Slice.Mf2Fly, actor.Flags2);
        Assert.Equal(0x4 | VmPlayerProperty1353Slice.Mf7FlyCheat, actor.Flags7);
        Assert.False(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 0, VmPlayerProperty1353Slice.PropFly, level));
        Assert.Equal(VmPlayerProperty1353Slice.CfFly | 0x5, player.Cheats);
        Assert.Equal(0x1, actor.Flags);
        Assert.Equal(0x2, actor.Flags2);
        Assert.Equal(0x4, actor.Flags7);

        player.Cheats = 0x5;
        Assert.False(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, 99, level));
        Assert.Equal(0x5, player.Cheats);

        level.IbCompatFlags = VmPlayerProperty1353Slice.BCompatLinkFrozen;
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 1, VmPlayerProperty1353Slice.PropFrozen, level));
        Assert.Equal(0x5 | VmPlayerProperty1353Slice.CfFrozen, player.Cheats);
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(actor, 0, 0, VmPlayerProperty1353Slice.PropFrozen, level));
        Assert.Equal(0x5, player.Cheats);

        var a = new Actor1353 { Flags7 = 9 };
        var b = new Actor1353 { Flags7 = 9 };
        var pa = new Player1353 { Mo = a, Cheats = VmPlayerProperty1353Slice.CfGodMode };
        var pb = new Player1353 { Mo = b, Cheats = VmPlayerProperty1353Slice.CfGodMode };
        var skipped = new Player1353 { Cheats = 3, Mo = new Actor1353 { Flags7 = 9 } };
        level.InGame[0] = true;
        level.Players[0] = pa;
        level.InGame[1] = false;
        level.Players[1] = skipped;
        level.InGame[2] = true;
        level.Players[2] = pb;
        Assert.False(VmPlayerProperty1353Slice.SetPlayerProperty(null, 1, 1, VmPlayerProperty1353Slice.PropFly, level));
        Assert.Equal(9, a.Flags7);
        Assert.Equal(VmPlayerProperty1353Slice.Mf2Fly, a.Flags2);
        Assert.Equal(VmPlayerProperty1353Slice.MfNoGravity, a.Flags);
        Assert.Equal(9, b.Flags7);
        Assert.Equal(VmPlayerProperty1353Slice.Mf2Fly, b.Flags2);
        Assert.Equal(3, skipped.Cheats);
        Assert.Equal(0, skipped.Mo!.Flags2);
        Assert.False(VmPlayerProperty1353Slice.SetPlayerProperty(null, 1, 0, VmPlayerProperty1353Slice.PropFly, level));
        Assert.Equal(0, a.Flags2);
        Assert.Equal(0, a.Flags);
        Assert.Equal(9, a.Flags7);

        pa.Cheats = VmPlayerProperty1353Slice.CfGodMode;
        pb.Cheats = VmPlayerProperty1353Slice.CfGodMode | VmPlayerProperty1353Slice.CfFrozen | VmPlayerProperty1353Slice.CfTotallyFrozen;
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(null, 1, 1, VmPlayerProperty1353Slice.PropFrozen, level));
        int both = VmPlayerProperty1353Slice.CfFrozen | VmPlayerProperty1353Slice.CfTotallyFrozen;
        Assert.Equal(VmPlayerProperty1353Slice.CfGodMode | both, pa.Cheats);
        Assert.Equal(VmPlayerProperty1353Slice.CfGodMode | both, pb.Cheats);
        Assert.Equal(3, skipped.Cheats);
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(null, 1, 0, VmPlayerProperty1353Slice.PropTotallyFrozen, level));
        Assert.Equal(VmPlayerProperty1353Slice.CfGodMode, pa.Cheats);
        Assert.Equal(VmPlayerProperty1353Slice.CfGodMode, pb.Cheats);

        level.IbCompatFlags = 0;
        Assert.True(VmPlayerProperty1353Slice.SetPlayerProperty(null, 1, 1, VmPlayerProperty1353Slice.PropFrozen, level));
        Assert.Equal(VmPlayerProperty1353Slice.CfGodMode | VmPlayerProperty1353Slice.CfFrozen, pa.Cheats);
        Assert.Equal(0, pa.Cheats & VmPlayerProperty1353Slice.CfTotallyFrozen);

        Array.Clear(level.InGame);
        var later = new Player1353 { Mo = b, Cheats = 0 };
        b.Flags2 = 0;
        level.InGame[0] = true;
        level.Players[0] = new Player1353 { Mo = null, Cheats = 1 };
        level.InGame[4] = true;
        level.Players[4] = later;
        Assert.Equal("mo", Assert.Throws<NullReferenceException>(() =>
            VmPlayerProperty1353Slice.SetPlayerProperty(null, 1, 1, VmPlayerProperty1353Slice.PropFly, level)).Message);
        Assert.Equal(0, b.Flags2);
        Assert.Equal(0, later.Cheats);

        Array.Clear(level.InGame);
        level.InGame[0] = true;
        level.Players[0] = null;
        Assert.Equal("player", Assert.Throws<NullReferenceException>(() =>
            VmPlayerProperty1353Slice.SetPlayerProperty(null, 1, 1, VmPlayerProperty1353Slice.PropGodMode, level)).Message);
    }
}
