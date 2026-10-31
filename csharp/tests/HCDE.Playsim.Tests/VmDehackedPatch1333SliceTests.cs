namespace HCDE.Playsim.Tests;

public class VmDehackedPatch1333SliceTests
{
    [Fact]
    public void Frame_AppliesKeysBitsAndStops()
    {
        var w = new VmDehackedPatch1333();
        VmDehackedPatch1333.State flash = w.AddState(47, 9, 3, 2, VmDehackedPatch1333.StfFullbright);
        VmDehackedPatch1333.State next = w.AddState(48, 1, 0, 1, 0);
        w.Push(1, "Duration", "9");
        w.Push(1, "Unknown 1", "4");
        w.Push(1, "Unknown 2", "6");
        w.Push(1, "Sprite number", "8");
        w.Push(1, "Next frame", "48");
        w.Push(1, "Sprite subnumber", "32771");
        w.Push(1, "Args1", "99");
        w.Push(1, "Args9", "1");
        w.Push(1, "MBF21 Bits", "SKILL5FAST|1");
        w.Push(1, "Nope", "1");
        w.Push(2, "Pointer", "");
        w.Push(1, "Duration", "1");

        Assert.Equal(2, w.PatchFrame(47, 0));
        Assert.Equal(9, flash.Tics);
        Assert.Equal(4, flash.Misc1);
        Assert.Equal(6, flash.Misc2);
        Assert.Equal(3, flash.Frame);
        Assert.Equal(
            VmDehackedPatch1333.StfFast | VmDehackedPatch1333.StfFullbright | VmDehackedPatch1333.StfDehacked,
            flash.StateFlags);
        Assert.Equal(8, w.SpriteNumbers[47]);
        Assert.Same(next, flash.Next);
        Assert.True(w.ChangedStates);
        Assert.Equal(99, w.StateArgs[47].Args[0]);
        Assert.Equal(1, w.StateArgs[47].Used);
        Assert.Contains("Frame 47\n", w.Debugs);
        Assert.Contains("MBF21 Bits: 6 (0x00000006)\n", w.Debugs);
        Assert.Contains("Invalid frame arg 8\n", w.Prints);
        Assert.Contains("Unknown key Nope encountered in Frame 47.\n", w.Prints);
        Assert.Equal("Duration", w.Lines.Peek().Key);

        w.Lines.Clear();
        Assert.Equal(0, w.PatchFrame(48, 0));
        Assert.Equal(4, next.Tics);
        Assert.Equal(1, next.Frame);
        Assert.Equal(VmDehackedPatch1333.StfDehacked, next.StateFlags);
        Assert.Equal(0, w.StateArgs[48].Used);

        var missing = new VmDehackedPatch1333();
        missing.AddState(47, 1, 0, 0, 0);
        missing.Push(1, "Unknown 2", "5");
        missing.Push(1, "Next frame", "47");
        Assert.Equal(0, missing.PatchFrame(0, 0));
        Assert.Equal(5, missing.Dummy.Misc2);
        Assert.Equal(47, missing.Dummy.Next!.Id);
        Assert.True(missing.ChangedStates);
        Assert.Contains("Frame 0 out of range\n", missing.Prints);
        Assert.Empty(missing.StateArgs);

        var wide = new VmDehackedPatch1333();
        VmDehackedPatch1333.State wideState = wide.AddState(3, 1, 0, 0, VmDehackedPatch1333.StfFast | VmDehackedPatch1333.StfFullbright);
        wide.Push(1, "Sprite subnumber", "64");
        wide.Push(1, "Duration", "40000");
        wide.Push(1, "MBF21 Bits", "2");
        Assert.Equal(0, wide.PatchFrame(3, 0));
        Assert.Equal(0, wideState.Frame);
        Assert.Equal(VmDehackedPatch1333.ShortMax, wideState.Tics);
        Assert.Equal(VmDehackedPatch1333.StfDehacked, wideState.StateFlags);
        Assert.Contains("Frame 3: Subnumber must be in range [0,63]\n", wide.Prints);
        Assert.Contains("MBF21 Bits: 4 (0x00000004)\n", wide.Debugs);

        var slow = new VmDehackedPatch1333();
        VmDehackedPatch1333.State slowState = slow.AddState(9, 8, 0, 0, VmDehackedPatch1333.StfFast);
        slow.Push(1, "duration", "-5");
        slow.Push(1, "mbf21 bits", "nope");
        Assert.Equal(0, slow.PatchFrame(9, 0));
        Assert.Equal(-1, slowState.Tics);
        Assert.Equal(VmDehackedPatch1333.StfFast | VmDehackedPatch1333.StfDehacked, slowState.StateFlags);
        Assert.Contains("Unknown bit mnemonic nope\n", slow.Debugs);
        Assert.Contains("MBF21 Bits: 2 (0x00000002)\n", slow.Debugs);
    }

    [Fact]
    public void Sprite_Ammo_RemapsAndScales()
    {
        Assert.Equal(129380, VmDehackedPatch1333.Toff[4]);
        Assert.Equal(-1, VmDehackedPatch1333.WeaponBits[0].Value);
        Assert.Equal(VmDehackedPatch1333.WifNoAlert, VmDehackedPatch1333.WeaponBits[1].Value);
        Assert.Equal(VmDehackedPatch1333.WifNoAutofire, VmDehackedPatch1333.WeaponBits[2].Value);
        Assert.Equal(VmDehackedPatch1333.WifMelee, VmDehackedPatch1333.WeaponBits[3].Value);
        Assert.Equal(VmDehackedPatch1333.WifWimpy, VmDehackedPatch1333.WeaponBits[4].Value);
        Assert.Equal(VmDehackedPatch1333.WifNoAutoswitch, VmDehackedPatch1333.WeaponBits[5].Value);
        Assert.Equal(VmDehackedPatch1333.StfFast, VmDehackedPatch1333.SkillBits[0].Value);

        var sound = new VmDehackedPatch1333();
        sound.Push(1, "Offset", "5");
        sound.Push(2, "Done", "");
        Assert.Equal(2, sound.PatchSound(4, 1));
        Assert.Equal(1, sound.SoundLines);
        Assert.Empty(sound.Prints);

        var sprites = new VmDehackedPatch1333();
        sprites.OrgSprOrgSize = 3;
        sprites.OrgNames.AddRange(new[] { "POSS", "SPOS", "TROO" });
        sprites.Unchanged.AddRange(new[] { "POSS", "SPOS", "TROO" });
        sprites.Sprites.AddRange(new[] { "POSS", "SPOS", "TROO" });
        sprites.Push(1, "Offset", "151096");
        Assert.Equal(0, sprites.PatchSprite(0, 0));
        Assert.Equal("SPOS", sprites.Sprites[0]);
        Assert.Contains("Sprite 0\n", sprites.Debugs);

        sprites.Lines.Clear();
        sprites.Prints.Clear();
        sprites.Push(1, "Foo", "1");
        Assert.Equal(0, sprites.PatchSprite(-3, 0));
        Assert.Contains("Sprite -3 out of range.\n", sprites.Prints);
        Assert.Contains("Unknown key Foo encountered in Sprite -1.\n", sprites.Prints);

        sprites.Lines.Clear();
        sprites.Prints.Clear();
        sprites.Dsdhacked = true;
        sprites.Sprites[0] = "POSS";
        sprites.Push(1, "offset", "1");
        Assert.Equal(0, sprites.PatchSprite(0, 0));
        Assert.Equal("POSS", sprites.Sprites[0]);
        Assert.DoesNotContain(sprites.Prints, line => line.StartsWith("Sprite name"));

        sprites.Dsdhacked = false;
        sprites.Lines.Clear();
        sprites.Prints.Clear();
        sprites.Push(1, "Offset", "1");
        Assert.Equal(0, sprites.PatchSprite(1, 0));
        Assert.Contains("Sprite name ", sprites.Prints[0]);

        var missed = new VmDehackedPatch1333();
        missed.OrgSprOrgSize = 1;
        missed.OrgNames.Add("POSS");
        missed.Sprites.Add("POSS");
        missed.Push(1, "Offset", "151088");
        Assert.Equal(0, missed.PatchSprite(0, 0));
        Assert.Equal("POSS", missed.Sprites[0]);

        var ammo = new VmDehackedPatch1333();
        var weapon = new VmDehackedPatch1333.Actor { Name = "Weapon" };
        var clip = new VmDehackedPatch1333.Actor { Name = "Clip", MaxAmount = 200, Amount = 10 };
        var box = new VmDehackedPatch1333.Actor { Name = "ClipBox", Parent = clip, MaxAmount = 50, Amount = 20 };
        var fist = new VmDehackedPatch1333.Actor { Name = "Fist", Parent = weapon, AmmoGive1 = 5 };
        var pistol = new VmDehackedPatch1333.Actor
        {
            Name = "Pistol",
            Parent = weapon,
            AmmoType1 = clip,
            AmmoGive1 = 10,
            AmmoType2 = clip,
            AmmoGive2 = 4,
        };
        var health = new VmDehackedPatch1333.Actor { Name = "Health", Amount = 5, MaxAmount = 1 };
        ammo.WeaponBase = weapon;
        ammo.AmmoNames.Add(clip);
        ammo.Actors.AddRange(new[] { clip, box, fist, pistol, health });
        ammo.Push(1, "Max ammo", "400");
        ammo.Push(1, "Per ammo", "20");
        ammo.Push(1, "Color", "1");
        Assert.Equal(0, ammo.PatchAmmo(0, 0));
        Assert.Equal(400, clip.MaxAmount);
        Assert.Equal(20, clip.Amount);
        Assert.Equal(800, clip.BackpackMax);
        Assert.Equal(20, clip.BackpackAmount);
        Assert.Equal(400, box.MaxAmount);
        Assert.Equal(40, box.Amount);
        Assert.Equal(5, fist.AmmoGive1);
        Assert.Equal(20, pistol.AmmoGive1);
        Assert.Equal(8, pistol.AmmoGive2);
        Assert.Equal(5, health.Amount);
        Assert.Contains("Ammo 0.\n", ammo.Debugs);
        Assert.Contains("Unknown key Color encountered in Ammo 0.\n", ammo.Prints);

        var maxOnly = new VmDehackedPatch1333();
        var clip2 = new VmDehackedPatch1333.Actor { Name = "Clip", MaxAmount = 200, Amount = 10 };
        var box2 = new VmDehackedPatch1333.Actor { Name = "ClipBox", Parent = clip2, MaxAmount = 50, Amount = 20 };
        maxOnly.AmmoNames.Add(clip2);
        maxOnly.Actors.Add(box2);
        maxOnly.Push(1, "Max ammo", "400");
        Assert.Equal(0, maxOnly.PatchAmmo(0, 0));
        Assert.Equal(800, clip2.BackpackMax);
        Assert.Equal(10, clip2.BackpackAmount);
        Assert.Equal(50, box2.MaxAmount);
        Assert.Equal(20, box2.Amount);

        var missingAmmo = new VmDehackedPatch1333();
        missingAmmo.AmmoNames.Add(clip2);
        Assert.Equal(0, missingAmmo.PatchAmmo(1, 0));
        Assert.Contains("Ammo 1.\n", missingAmmo.Debugs);
        Assert.Contains("Ammo 1 out of range.\n", missingAmmo.Prints);
        Assert.Equal(0, missingAmmo.PatchAmmo(5, 0));
        Assert.Contains("Ammo 5 out of range.\n", missingAmmo.Prints);
        Assert.DoesNotContain("Ammo 5.\n", missingAmmo.Debugs);
    }
}
