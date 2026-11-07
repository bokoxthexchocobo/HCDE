using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmAdvSound1429SliceTests
{
    static VmAdvSound1429Slice.Sfx Sound(string name, int link = VmAdvSound1429Slice.NoLink) => new() { Name = name, Link = link };

    static VmAdvSound1429Slice.State With(params VmAdvSound1429Slice.Sfx[] sounds)
    {
        var state = new VmAdvSound1429Slice.State();
        foreach (var sound in sounds)
        {
            state.Ids[sound.Name] = state.Sounds.Count;
            state.Sounds.Add(sound);
        }
        return state;
    }

    [Fact]
    public void CyclesAndLengths()
    {
        Assert.Equal(394, VmAdvSound1429Slice.BandEnd - VmAdvSound1429Slice.BandStart + 1);
        Assert.Equal(13, VmAdvSound1429Slice.Sounds);
        var reserved = Sound("pl");
        reserved.User = VmAdvSound1429Slice.Reserve;
        reserved.Link = 0;
        var chain = new List<VmAdvSound1429Slice.Sfx>();
        Assert.True(VmAdvSound1429Slice.Check(With(reserved), reserved, reserved, chain));
        Assert.Empty(chain);
        var self = Sound("loop", 0);
        var seen = new List<VmAdvSound1429Slice.Sfx>();
        Assert.False(VmAdvSound1429Slice.Check(With(self), self, self, seen));
        Assert.Empty(seen);
        var alias = With(self);
        Assert.False(VmAdvSound1429Slice.Check(alias, self, self, new List<VmAdvSound1429Slice.Sfx>()));
        Assert.Equal(VmAdvSound1429Slice.Red + "recursive sound $alias found for loop:\n", Assert.Single(alias.Lines));
        var a = Sound("a", 1);
        var b = Sound("b", 0);
        var pair = With(a, b);
        VmAdvSound1429Slice.Integrity(pair);
        Assert.Contains("recursive sound $alias found for a", pair.Lines[0]);
        Assert.Contains("    -> b", pair.Lines[1]);
        Assert.Contains("recursive sound $alias found for b", pair.Lines[2]);
        Assert.Contains("    -> a", pair.Lines[3]);
        Assert.Contains("Sound a has been disabled", pair.Lines[4]);
        Assert.Contains("Sound b has been disabled", pair.Lines[5]);
        Assert.False(a.Random);
        Assert.Equal(VmAdvSound1429Slice.NoSound, a.Link);
        Assert.Equal(VmAdvSound1429Slice.NoSound, b.Link);
        Assert.Equal(1, pair.RumbleReady);
        var rand = Sound("roll");
        rand.Random = true;
        rand.Choices = new[] { 0 };
        var rolled = With(rand);
        VmAdvSound1429Slice.Integrity(rolled);
        Assert.Contains("$random found for roll", Assert.Single(rolled.Lines, line => line.Contains("recursive")));
        Assert.DoesNotContain(rolled.Lines, line => line.Contains("->"));
        Assert.False(rand.Random);
        Assert.Equal(0, rand.Link);
        var parent = Sound("parent");
        parent.Random = true;
        parent.Choices = new[] { 1 };
        var child = Sound("child");
        child.Random = true;
        child.Choices = new[] { 1 };
        var nested = With(parent, child);
        VmAdvSound1429Slice.Integrity(nested);
        Assert.True(parent.Random);
        Assert.False(child.Random);
        Assert.Equal(0, child.Link);
        Assert.Contains("$random found for child", nested.Lines[0]);
        Assert.Contains("Sound child has been disabled", nested.Lines[1]);
        Assert.Equal(1, nested.RumbleReady);
        var shortOne = Sound("short");
        shortOne.Ms = 10;
        var longOne = Sound("long");
        longOne.Ms = 40;
        var header = Sound("mix");
        header.Random = true;
        header.Link = 1;
        header.Choices = new[] { 1, 2 };
        var timed = With(header, shortOne, longOne);
        Assert.Equal(40, VmAdvSound1429Slice.Milliseconds(timed, 0));
        Assert.Equal(2, timed.Loads);
        Assert.Equal(0.04d, VmAdvSound1429Slice.LengthSeconds(timed, 2));
        Assert.Equal(3, timed.Loads);
        var skin = Sound("skinme");
        skin.User = VmAdvSound1429Slice.Reserve;
        skin.Link = 4;
        var body = Sound("body");
        body.Ms = 2500;
        var skinned = With(skin, body);
        skinned.SkinReturns.Add(1);
        Assert.Equal(2.5d, VmAdvSound1429Slice.LengthSeconds(skinned, 0));
        Assert.Equal(1, skinned.Loads);
        body.LoadNull = true;
        Assert.Equal(0, VmAdvSound1429Slice.Milliseconds(skinned, 1));
        body.LoadNull = false;
        body.DataNull = true;
        Assert.Throws<NullReferenceException>(() => VmAdvSound1429Slice.Milliseconds(skinned, 1));
        Assert.Equal(0, VmAdvSound1429Slice.Milliseconds(new VmAdvSound1429Slice.State(), 3));
        Assert.Throws<NullReferenceException>(() => VmAdvSound1429Slice.Integrity(new VmAdvSound1429Slice.State { EngineNull = true }));
    }

    [Fact]
    public void AddsClearsAndParse()
    {
        var fresh = new VmAdvSound1429Slice.State { PitchMask = 7, FullNames = { ["dspistol"] = 4 } };
        Assert.Equal(0, VmAdvSound1429Slice.AddName(fresh, "pistol", "dspistol"));
        Assert.Equal(new[] { "dspistol" }, fresh.Looks);
        Assert.Equal(4, fresh.Sounds[0]!.Lump);
        Assert.Equal(new[] { 7 }, fresh.Masks);
        var again = With(Sound("pistol"));
        again.Sounds[0]!.Random = true;
        again.Sounds[0]!.Choices = new[] { 1, 2 };
        again.Sounds[0]!.Tentative = true;
        again.Sounds[0]!.Link = 3;
        again.Sounds[0]!.Near = -1;
        VmAdvSound1429Slice.AddLump(again, "pistol", 9, true);
        Assert.Equal(9, again.Sounds[0]!.Lump);
        Assert.Empty(again.Sounds[0]!.Choices);
        Assert.Equal(VmAdvSound1429Slice.NoSound, again.Sounds[0]!.Owner);
        Assert.False(again.Sounds[0]!.Random);
        Assert.Equal(VmAdvSound1429Slice.NoLink, again.Sounds[0]!.Link);
        Assert.False(again.Sounds[0]!.Tentative);
        Assert.Equal(2, again.Sounds[0]!.Near);
        Assert.Equal(65536, again.Sounds[0]!.Range);
        var kept = With(Sound("keep"));
        kept.Sounds[0]!.Near = 4;
        kept.Sounds[0]!.Range = 1;
        VmAdvSound1429Slice.AddLump(kept, "keep", 2, false);
        Assert.Equal(4, kept.Sounds[0]!.Near);
        Assert.Equal(1, kept.Sounds[0]!.Range);
        var reserved = With(Sound("pl"));
        reserved.Sounds[0]!.User = VmAdvSound1429Slice.Reserve;
        reserved.Sounds[0]!.Lump = 8;
        var error = Assert.Throws<InvalidOperationException>(() => VmAdvSound1429Slice.AddName(reserved, "pl", "dspl"));
        Assert.Equal(VmAdvSound1429Slice.Reserved, error.Message);
        Assert.False(reserved.Script);
        Assert.Equal(8, reserved.Sounds[0]!.Lump);
        var scripted = With(Sound("pl"));
        scripted.Sounds[0]!.User = VmAdvSound1429Slice.Reserve;
        Assert.Throws<InvalidOperationException>(() => VmAdvSound1429Slice.AddLump(scripted, "pl", 1, true));
        Assert.True(scripted.Script);
        var compat = With(Sound("compat", 1), Sound("target"));
        compat.Sounds[0]!.User = VmAdvSound1429Slice.Compat;
        compat.Sounds[1]!.Near = -1;
        VmAdvSound1429Slice.AddLump(compat, "compat", 6, false);
        Assert.Equal(6, compat.Sounds[1]!.Lump);
        Assert.Equal(2, compat.Sounds[1]!.Near);
        Assert.Equal(1, compat.Sounds[0]!.Link);
        Assert.Equal(VmAdvSound1429Slice.NoLink, compat.Sounds[1]!.Link);
        var player = With(Sound("empty"), Sound("*death", 5));
        player.ClassReturns.Add(2);
        player.GenderReturns.Add(0);
        player.PlayerSounds.Add(new List<(int, int)>());
        player.FullNames["dsfdth"] = 11;
        int added = VmAdvSound1429Slice.AddPlayerName(player, "Fighter", 1, 1, "dsfdth");
        Assert.Equal(2, added);
        Assert.Equal("Fighter\"1\"*death", player.Sounds[2]!.Name);
        Assert.Equal(11, player.Sounds[2]!.Lump);
        Assert.Equal((5, 2), player.PlayerSounds[0][0]);
        Assert.Equal(new[] { "Fighter" }, player.ClassNames);
        Assert.Equal((2, 1), player.Genders[0]);
        Assert.Equal(0, VmAdvSound1429Slice.AddPlayer(player, "Fighter", 0, VmAdvSound1429Slice.NoSound, 3, true));
        Assert.Empty(player.Saves);
        var existing = With(Sound("empty"), Sound("*grunt", 8));
        existing.ClassReturns.Add(1);
        existing.GenderReturns.Add(0);
        existing.PlayerSounds.Add(new List<(int, int)>());
        Assert.Equal(4, VmAdvSound1429Slice.AddExisting(existing, "Mage", 0, 1, 4, true));
        Assert.Equal((8, 4), existing.PlayerSounds[0][0]);
        var saved = Assert.Single(existing.Saves);
        Assert.True(saved.Alias);
        Assert.Equal(4, saved.Lump);
        Assert.Equal("Mage", saved.ClassName);
        existing.ClassReturns.Add(1);
        existing.GenderReturns.Add(0);
        Assert.Equal(VmAdvSound1429Slice.NoSound, VmAdvSound1429Slice.AddExisting(existing, "Mage", 0, VmAdvSound1429Slice.NoSound, 4, false));
        Assert.Equal(2, existing.ClassNames.Count);
        var dup = With(Sound("empty"), Sound("*pain", 3));
        dup.LookupReturns.Add(6);
        dup.ClassReturns.Add(1);
        dup.GenderReturns.Add(0);
        dup.PlayerSounds.Add(new List<(int, int)>());
        Assert.Equal(6, VmAdvSound1429Slice.Dup(dup, "Mage", 1, 1, 9));
        Assert.Equal(("Mage", 1, 9), dup.Lookups[0]);
        Assert.Equal((3, 6), dup.PlayerSounds[0][0]);
        Assert.Empty(dup.Saves);
        var wiped = new VmAdvSound1429Slice.State();
        wiped.Sounds.Add(Sound("old"));
        wiped.Ids["old"] = 0;
        VmAdvSound1429Slice.Clear(wiped);
        Assert.Equal(1, wiped.EngineClears);
        Assert.Empty(wiped.Sounds);
        Assert.Empty(wiped.Classes);
        Assert.Equal(0, wiped.Reserves);
        Assert.False(wiped.Sorted);
        Assert.Equal("", wiped.DefName);
        Assert.Equal(0, wiped.DefClass);
        Assert.Empty(wiped.Aliases);
        Assert.Equal(1, wiped.RumbleResets);
        var dead = new VmAdvSound1429Slice.State { EngineNull = true };
        dead.Sounds.Add(Sound("stay"));
        VmAdvSound1429Slice.Clear(dead);
        Assert.Single(dead.Sounds);
        Assert.Equal(0, dead.EngineClears);
        Assert.Equal(1, dead.RumbleResets);
        var parsed = new VmAdvSound1429Slice.State
        {
            PitchMask = 9,
            FullNames = { ["DSEMPTY"] = 2 },
            Entries =
            {
                new VmAdvSound1429Slice.Entry { Space = VmAdvSound1429Slice.Global, Name = "SNDINFO" },
                new VmAdvSound1429Slice.Entry { Space = VmAdvSound1429Slice.Sounds, Name = "SNDINFO" },
                new VmAdvSound1429Slice.Entry { Space = VmAdvSound1429Slice.Voices, Name = "voice" },
            },
        };
        parsed.Sounds.Add(Sound("old"));
        parsed.Ids["old"] = 0;
        VmAdvSound1429Slice.Parse(parsed, false);
        Assert.Equal("{ no sound }", parsed.Sounds[0]!.Name);
        Assert.Equal(2, parsed.Sounds[0]!.Lump);
        Assert.Equal(new[] { 0 }, parsed.Masks);
        Assert.Equal(new[] { 0 }, parsed.InfoLumps);
        Assert.Equal(new[] { 2 }, parsed.VoiceLumps);
        Assert.Equal(1, parsed.Restores);
        Assert.Equal(1, parsed.Hashes);
        Assert.Equal(1, parsed.Shrinks);
        Assert.Equal(1, parsed.RumbleReady);
        Assert.Empty(parsed.Saved);
        Assert.Equal(0, parsed.PitchMask);
        parsed.Locked = true;
        parsed.Saved.Add(new VmAdvSound1429Slice.Save { ClassName = "keep" });
        VmAdvSound1429Slice.Parse(parsed, true);
        Assert.Equal("keep", parsed.Saved[0].ClassName);
        Assert.Equal(1, parsed.Hashes);
        VmAdvSound1429Slice.Parse(parsed, false);
        Assert.Empty(parsed.Saved);
        VmAdvSound1429Slice.Lock(parsed);
        Assert.True(parsed.Locked);
        VmAdvSound1429Slice.AddLocal(parsed, 5);
        Assert.Equal(VmAdvSound1429Slice.LocalLocked, Assert.Single(parsed.Lines));
        parsed.Locked = false;
        parsed.Lines.Clear();
        int hashes = parsed.Hashes;
        VmAdvSound1429Slice.AddLocal(parsed, 5);
        Assert.Equal(5, parsed.InfoLumps[^1]);
        Assert.Equal(hashes + 1, parsed.Hashes);
        Assert.Throws<NullReferenceException>(() => VmAdvSound1429Slice.AddName(new VmAdvSound1429Slice.State(), "x", null));
        Assert.Throws<NullReferenceException>(() => VmAdvSound1429Slice.AddPlayer(new VmAdvSound1429Slice.State(), null, 0, 0, 1, false));
    }
}
