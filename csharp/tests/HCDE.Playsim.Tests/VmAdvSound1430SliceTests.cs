using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmAdvSound1430SliceTests
{
    [Fact]
    public void PlayerListsSortAndFallbacks()
    {
        Assert.Equal(1454, VmAdvSound1430Slice.BandStart);
        Assert.Equal(1759, VmAdvSound1430Slice.BandEnd);
        var grown = new VmAdvSound1430Slice.State();
        grown.Classes.Add(new VmAdvSound1430Slice.Class { Name = "Fighter" });
        Assert.Equal(0, VmAdvSound1430Slice.AddGender(grown, 0, 0));
        Assert.Equal(0, VmAdvSound1430Slice.AddGender(grown, 0, 0));
        Assert.Equal(1, VmAdvSound1430Slice.AddGender(grown, 0, 1));
        Assert.Equal(2, grown.Tables.Count);

        var empty = new VmAdvSound1430Slice.State { DefName = "player" };
        VmAdvSound1430Slice.Shrink(empty);
        Assert.True(empty.Sorted);
        Assert.Equal(-1, empty.Def);

        var s = new VmAdvSound1430Slice.State { DefName = "mage" };
        s.Classes.Add(new VmAdvSound1430Slice.Class { Name = "sigma" });
        s.Classes.Add(new VmAdvSound1430Slice.Class { Name = "Kappa" });
        s.Classes.Add(new VmAdvSound1430Slice.Class { Name = "mAge" });
        s.Classes.Add(new VmAdvSound1430Slice.Class { Name = "alpha" });
        s.Classes.Add(new VmAdvSound1430Slice.Class { Name = "beta" });
        Assert.Equal(2, VmAdvSound1430Slice.Find(s, "mage"));
        VmAdvSound1430Slice.Shrink(s);
        Assert.Equal(new[] { "alpha", "beta", "Kappa", "mAge", "sigma" }, s.Classes.ConvertAll(c => c.Name));
        Assert.True(s.Sorted);
        Assert.Equal(3, s.Def);
        Assert.Equal(1, VmAdvSound1430Slice.Find(s, "BETA"));
        Assert.Equal(4, VmAdvSound1430Slice.Find(s, "SIGMA"));
        Assert.Equal(-1, VmAdvSound1430Slice.Find(s, "nope"));

        var hear = Hear();
        Assert.Equal(6, VmAdvSound1430Slice.Lookup(hear, "nope", 0, 6));
        Assert.Equal(6, VmAdvSound1430Slice.Lookup(hear, "player", 1, 1));
        hear.Sorted = false;
        Assert.Equal(0, VmAdvSound1430Slice.Lookup(hear, "player", 1, 1));
        hear.Sorted = true;
        hear.Tables[1][4] = 2;
        Assert.Equal(2, VmAdvSound1430Slice.Lookup(hear, "player", 1, 1));
        hear.Tables[1][4] = 3;
        Assert.Equal(3, VmAdvSound1430Slice.Lookup(hear, "player", 1, 1));
        hear.Tables[1].Remove(4);
        Assert.Equal(6, VmAdvSound1430Slice.Lookup(hear, "mage", 0, 1));
        hear.Def = 0;
        Assert.Equal(0, VmAdvSound1430Slice.Lookup(hear, "mage", 0, 1));
        hear.Def = 1;
        Assert.Equal(6, VmAdvSound1430Slice.LookupIndex(hear, -1, 0, 1));
        Assert.Equal(6, VmAdvSound1430Slice.Lookup(hear, "zzz", 0, 1));
        Assert.Throws<NullReferenceException>(() => VmAdvSound1430Slice.Lookup(hear, null, 0, 1));
        VmAdvSound1430Slice.Save(hear, "player", 0, 1, 12, false);
        VmAdvSound1430Slice.Save(hear, "player", 1, 1, 6, true);
        VmAdvSound1430Slice.Restore(hear);
        Assert.Equal(2, hear.Calls.Count);
        Assert.False(hear.Calls[0].Alias);
        Assert.Equal(12, hear.Calls[0].Lump);
        Assert.Equal(0, hear.Calls[0].Gender);
        Assert.True(hear.Calls[1].Alias);
        Assert.Equal(6, hear.Calls[1].Lump);
        Assert.Equal(1, hear.Calls[1].Gender);
        Assert.Equal("player", hear.Calls[1].ClassName);
    }

    [Fact]
    public void SkinsEquivalenceAndMarks()
    {
        var s = Skin();
        Assert.Equal("player", VmAdvSound1430Slice.SoundClass(s, new VmAdvSound1430Slice.Actor()));
        Assert.Equal("zombie", VmAdvSound1430Slice.SoundClass(s, new VmAdvSound1430Slice.Actor { SoundClass = "zombie" }));
        var who = new VmAdvSound1430Slice.Player { Gender = 1 };
        var actor = new VmAdvSound1430Slice.Actor { Who = who, SoundClass = "player" };
        Assert.Equal(10, VmAdvSound1430Slice.Skinned(s, actor, 1));
        who.Gender = 0;
        Assert.Equal(6, VmAdvSound1430Slice.Skinned(s, actor, 1));
        Assert.Equal(6, VmAdvSound1430Slice.Skinned(s, null, 1));
        s.Game = VmAdvSound1430Slice.Hexen;
        Assert.Equal(11, VmAdvSound1430Slice.Skinned(s, null, 1));
        s.Game = 0;
        who.Skin = 2;
        actor.SoundClass = "ignored";
        Assert.Equal("ranger", VmAdvSound1430Slice.SoundClass(s, actor));
        who.Mo = new VmAdvSound1430Slice.Actor { Flags4 = VmAdvSound1430Slice.Noskin };
        Assert.Equal("ignored", VmAdvSound1430Slice.SoundClass(s, actor));
        who.Mo = null;
        who.SoundClass = "Mage";
        Assert.Equal("Mage", VmAdvSound1430Slice.SoundClass(s, actor));
        who.SoundClass = "";
        who.Skin = -1;
        actor.SoundClass = null;
        Assert.Equal("player", VmAdvSound1430Slice.SoundClass(s, actor));
        who.Skin = 1;
        Assert.Equal("marine", VmAdvSound1430Slice.SoundClass(s, actor));
        who.Skin = 3;
        Assert.Equal("player", VmAdvSound1430Slice.SoundClass(s, actor));
        s.Names["pain-ouch"] = 6;
        s.Names["pain"] = 8;
        Assert.Equal(6, VmAdvSound1430Slice.SkinnedEx(s, null, "Pain", "OUCH"));
        s.Names.Remove("pain-ouch");
        Assert.Equal(8, VmAdvSound1430Slice.SkinnedEx(s, null, "pain", "ouch"));
        s.Names.Clear();
        Assert.Equal(0, VmAdvSound1430Slice.SkinnedEx(s, null, "pain", "ouch"));
        Assert.Throws<NullReferenceException>(() => VmAdvSound1430Slice.SkinnedEx(s, null, null, "ouch"));
        Assert.True(VmAdvSound1430Slice.Equivalent(s, null, 6, 6));
        Assert.True(VmAdvSound1430Slice.Equivalent(s, null, 0, 0));
        Assert.True(VmAdvSound1430Slice.Equivalent(s, null, -1, -1));
        Assert.False(VmAdvSound1430Slice.Equivalent(s, null, 0, 6));
        Assert.True(VmAdvSound1430Slice.Equivalent(s, null, 8, 6));
        Assert.True(VmAdvSound1430Slice.Equivalent(s, null, 9, 6));
        Assert.False(VmAdvSound1430Slice.Equivalent(s, null, 3, 6));
        Assert.True(VmAdvSound1430Slice.Equivalent(s, null, 1, 6));
        int broken = s.Sounds.Count;
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "broken", Link = 40 });
        Assert.Throws<NullReferenceException>(() => VmAdvSound1430Slice.Equivalent(s, null, broken, 6));
        VmAdvSound1430Slice.Mark(s, new VmAdvSound1430Slice.Actor { SoundClass = "player" });
        Assert.Equal(4, s.MarkPasses);
        Assert.Equal(new[] { 6, 6, 6, 6 }, s.Marks);
        s.Marks.Clear();
        s.MarkPasses = 0;
        VmAdvSound1430Slice.Mark(s, new VmAdvSound1430Slice.Actor { SoundClass = "mage" });
        Assert.Equal(0, s.MarkPasses);
        Assert.Empty(s.Marks);
        VmAdvSound1430Slice.Mark(s, new VmAdvSound1430Slice.Actor { SoundClass = "ranger" });
        Assert.Equal(4, s.MarkPasses);
        Assert.Equal(new[] { 6, 6, 6, 6 }, s.Marks);
    }

    static VmAdvSound1430Slice.State Hear()
    {
        var mage = new VmAdvSound1430Slice.Class { Name = "mage" };
        var player = new VmAdvSound1430Slice.Class { Name = "player" };
        player.Lists[0] = 0;
        player.Lists[1] = 1;
        var s = new VmAdvSound1430Slice.State { Sorted = true, Def = 1, DefName = "player" };
        s.Classes.Add(mage);
        s.Classes.Add(player);
        s.Tables.Add(new Dictionary<int, int> { [4] = 6 });
        s.Tables.Add(new Dictionary<int, int>());
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "{ no sound }", Lump = -1, Link = -1 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "*pain", User = VmAdvSound1430Slice.Reserve, Link = 4 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "silent", User = VmAdvSound1430Slice.Silent, Lump = -1, Link = -1 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "linked", Lump = -1, Link = 6 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "pad", Lump = 1, Link = -1 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "pad", Lump = 1, Link = -1 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "real", Lump = 9, Link = -1 });
        return s;
    }

    static VmAdvSound1430Slice.State Skin()
    {
        var fighter = new VmAdvSound1430Slice.Class { Name = "fighter" };
        fighter.Lists[0] = 2;
        var mage = new VmAdvSound1430Slice.Class { Name = "mage" };
        mage.Lists[1] = 0;
        var player = new VmAdvSound1430Slice.Class { Name = "player" };
        player.Lists[0] = 0;
        player.Lists[1] = 1;
        var s = new VmAdvSound1430Slice.State
        {
            Sorted = true,
            Def = 2,
            DefName = "player",
            PlayerClassCount = 1,
        };
        s.Skins.Add("doomguy");
        s.Skins.Add("marine");
        s.Skins.Add("ranger");
        s.Classes.Add(fighter);
        s.Classes.Add(mage);
        s.Classes.Add(player);
        s.Tables.Add(new Dictionary<int, int> { [4] = 6 });
        s.Tables.Add(new Dictionary<int, int> { [4] = 10 });
        s.Tables.Add(new Dictionary<int, int> { [4] = 11 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "{ no sound }", Lump = -1, Link = -1 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "*pain", User = VmAdvSound1430Slice.Reserve, Link = 4 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "pad", Lump = 1, Link = -1 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "random", Lump = 3, Link = 6, Random = true });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "pad", Lump = 1, Link = -1 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "pad", Lump = 1, Link = -1 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "real", Lump = 9, Link = -1 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "pad", Lump = 1, Link = -1 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "alias", Lump = 2, Link = 6 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "alias2", Lump = 2, Link = 8 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "female", Lump = 8, Link = -1 });
        s.Sounds.Add(new VmAdvSound1430Slice.Sfx { Name = "fighter", Lump = 7, Link = -1 });
        return s;
    }
}
