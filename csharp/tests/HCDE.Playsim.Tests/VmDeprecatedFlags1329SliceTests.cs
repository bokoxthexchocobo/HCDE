namespace HCDE.Playsim.Tests;

public class VmDeprecatedFlags1329SliceTests
{
    [Fact]
    public void Handle_SetsTheDeprecatedPropertiesAndChecksThem()
    {
        var vm = new VmDeprecatedFlags1329();
        vm.FindTentative = (name, parent) => name + "/" + parent;
        vm.FindClass = name => name + "/Actor";
        var actor = new VmDeprecatedFlags1329.Actor
        {
            BounceFlags = VmDeprecatedFlags1329.BounceQuiet | VmDeprecatedFlags1329.BounceTypeMask,
            Flags8 = 1,
            Flags3 = 2,
        };

        Assert.Equal(0, vm.CallHandle(actor, -1, VmDeprecatedFlags1329.DepfFireDamage));
        Assert.Equal("Fire", actor.DamageType);
        Assert.Equal(1, vm.CallCheck(actor, VmDeprecatedFlags1329.DepfFireDamage));
        actor.DamageType = "FIRE";
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfFireDamage));
        vm.HandleDeprecatedFlags(actor, 0, VmDeprecatedFlags1329.DepfFireDamage);
        Assert.Equal("None", actor.DamageType);
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfFireDamage));

        vm.HandleDeprecatedFlags(actor, 2, VmDeprecatedFlags1329.DepfIceDamage);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfIceDamage));
        vm.HandleDeprecatedFlags(actor, 0, VmDeprecatedFlags1329.DepfIceDamage);
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfIceDamage));

        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfLowGravity);
        Assert.Equal(1.0 / 8.0, actor.Gravity);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfLowGravity));
        vm.HandleDeprecatedFlags(actor, 0, VmDeprecatedFlags1329.DepfLowGravity);
        Assert.Equal(1.0, actor.Gravity);
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfLowGravity));

        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfQuarterGravity);
        Assert.Equal(1.0 / 4.0, actor.Gravity);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfQuarterGravity));

        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfShortMissileRange);
        Assert.Equal(896.0, actor.MaxTargetRange);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfShortMissileRange));
        vm.HandleDeprecatedFlags(actor, 0, VmDeprecatedFlags1329.DepfShortMissileRange);
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfShortMissileRange));

        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfLongMeleeRange);
        Assert.Equal(196.0, actor.MeleeThreshold);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfLongMeleeRange));
        vm.HandleDeprecatedFlags(actor, 0, VmDeprecatedFlags1329.DepfLongMeleeRange);
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfLongMeleeRange));

        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfFireResist);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfFireResist));
        actor.DamageFactors.Insert(0, new VmDeprecatedFlags1329.DamageFactor { Name = "Ice", Value = 0.5 });
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfFireResist));
        vm.HandleDeprecatedFlags(actor, 0, VmDeprecatedFlags1329.DepfFireResist);
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfFireResist));
        actor.DamageFactors.Clear();
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfFireResist));

        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfHereticBounce);
        Assert.Equal(VmDeprecatedFlags1329.BounceQuiet, actor.BounceFlags & VmDeprecatedFlags1329.BounceQuiet);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfHereticBounce));
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfDoomBounce));
        vm.HandleDeprecatedFlags(actor, 0, VmDeprecatedFlags1329.DepfDoomBounce);
        Assert.Equal(VmDeprecatedFlags1329.BounceQuiet, actor.BounceFlags);
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfHereticBounce));
        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfHexenBounce);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfHexenBounce));
        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfDoomBounce);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfDoomBounce));

        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfPickupFlash);
        Assert.Equal("PickupFlash/Actor", actor.PickupFlash);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfPickupFlash));
        vm.FindClass = _ => "other";
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfPickupFlash));
        vm.HandleDeprecatedFlags(actor, 0, VmDeprecatedFlags1329.DepfPickupFlash);
        vm.FindClass = _ => null;
        Assert.Null(actor.PickupFlash);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfPickupFlash));

        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfInterHubStrip);
        Assert.Equal(0, actor.InterHubAmount);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfInterHubStrip));
        vm.HandleDeprecatedFlags(actor, 0, VmDeprecatedFlags1329.DepfInterHubStrip);
        Assert.Equal(1, actor.InterHubAmount);
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfInterHubStrip));

        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfHigherMProb);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfHigherMProb));
        actor.MinMissileChance = 100;
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfHigherMProb));
        actor.MinMissileChance = 161;
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfHigherMProb));
        vm.HandleDeprecatedFlags(actor, 0, VmDeprecatedFlags1329.DepfHigherMProb);
        Assert.Equal(200, actor.MinMissileChance);

        actor.MissileChanceMult = 1;
        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfMissileMore);
        Assert.Equal(0.5, actor.MissileChanceMult);
        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfMissileEvenMore);
        Assert.Equal(0.0625, actor.MissileChanceMult);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfMissileMore));
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfMissileEvenMore));
        vm.HandleDeprecatedFlags(actor, 0, VmDeprecatedFlags1329.DepfMissileMore);
        Assert.Equal(0.125, actor.MissileChanceMult);
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfMissileMore));
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfMissileEvenMore));
        actor.MissileChanceMult = 0.125;
        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfMissileMore);
        Assert.Equal(0.0625, actor.MissileChanceMult);
        vm.HandleDeprecatedFlags(actor, 0, VmDeprecatedFlags1329.DepfMissileEvenMore);
        Assert.Equal(0.5, actor.MissileChanceMult);

        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfRangeHalf);
        Assert.Equal(0.5, actor.MissileChanceMult);
        Assert.Equal(196, actor.MeleeThreshold);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfRangeHalf));
        vm.HandleDeprecatedFlags(actor, 0, VmDeprecatedFlags1329.DepfRangeHalf);
        Assert.Equal(1, actor.MissileChanceMult);
        Assert.Equal(0, actor.MeleeThreshold);
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfRangeHalf));

        vm.HandleDeprecatedFlags(actor, 1, VmDeprecatedFlags1329.DepfFullVolSounds);
        Assert.Equal(1, actor.Flags8 & 1);
        Assert.NotEqual(0, actor.Flags8 & VmDeprecatedFlags1329.Mf8FullVolSee);
        Assert.NotEqual(0, actor.Flags3 & VmDeprecatedFlags1329.Mf3FullVolDeath);
        Assert.NotEqual(0, actor.Flags3 & VmDeprecatedFlags1329.Mf3FullVolActive);
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfFullVolSounds));
        actor.Flags3 &= ~VmDeprecatedFlags1329.Mf3FullVolActive;
        Assert.Equal(1, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfFullVolSounds));
        actor.Flags3 &= ~VmDeprecatedFlags1329.Mf3FullVolDeath;
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, VmDeprecatedFlags1329.DepfFullVolSounds));
        vm.HandleDeprecatedFlags(actor, 0, VmDeprecatedFlags1329.DepfFullVolSounds);
        Assert.Equal(1, actor.Flags8);
        Assert.Equal(2, actor.Flags3);

        int gravityBits = actor.BounceFlags;
        string type = actor.DamageType;
        vm.HandleDeprecatedFlags(actor, 1, 99);
        Assert.Equal(gravityBits, actor.BounceFlags);
        Assert.Equal(type, actor.DamageType);
        Assert.Equal(0, vm.CheckDeprecatedFlags(actor, 99));
        Assert.Equal(0, vm.CallCheck(actor, 0));
    }

    [Fact]
    public void Properties_MatchStringsAndRejectBadIds()
    {
        Assert.Equal(1, VmDeprecatedFlags1329.MatchString("heretic", new string?[] { "Doom", "Heretic", null }));
        Assert.Equal(0, VmDeprecatedFlags1329.MatchString("Doom", new string?[] { "Doom", null }));
        Assert.Equal(-1, VmDeprecatedFlags1329.MatchString("none", new string?[] { "Doom", null }));
        Assert.Equal(-1, VmDeprecatedFlags1329.MatchString("Doom", new string?[] { null }));
        Assert.Equal(-1, VmDeprecatedFlags1329.MatchString("Doom", Array.Empty<string?>()));

        var parent = new VmDeprecatedFlags1329.ClassPointer { Name = "Actor" };
        var child = new VmDeprecatedFlags1329.ClassPointer { Name = "DoomImp" };
        child.Ancestors.Add("Actor");
        Assert.False(VmDeprecatedFlags1329.PointerCheck(null, parent));
        Assert.False(VmDeprecatedFlags1329.PointerCheck(child, null));
        Assert.True(VmDeprecatedFlags1329.PointerCheck(child, parent));
        Assert.True(VmDeprecatedFlags1329.PointerCheck(parent, parent));
        Assert.False(VmDeprecatedFlags1329.PointerCheck(parent, child));

        var vm = new VmDeprecatedFlags1329();
        var info = new VmDeprecatedFlags1329.ActorInfo();
        vm.ApplyGame(info, "doom");
        vm.ApplyGame(info, "Heretic");
        Assert.Equal(VmDeprecatedFlags1329.GameDoom | VmDeprecatedFlags1329.GameHeretic, info.GameFilter);
        vm.ApplyGame(info, "Raven");
        Assert.Equal(VmDeprecatedFlags1329.GameDoom | VmDeprecatedFlags1329.GameHeretic | VmDeprecatedFlags1329.GameRaven, info.GameFilter);
        vm.ApplyGame(info, "Any");
        Assert.Equal(0, info.GameFilter);
        vm.ApplyGame(info, "Strife");
        vm.ApplyGame(info, "Chex");
        vm.ApplyGame(info, "Hexen");
        Assert.Equal(VmDeprecatedFlags1329.GameStrife | VmDeprecatedFlags1329.GameChex | VmDeprecatedFlags1329.GameHexen, info.GameFilter);
        var unknown = Assert.Throws<InvalidOperationException>(() => vm.ApplyGame(info, "Quake"));
        Assert.Equal("Unknown game type Quake", unknown.Message);
        Assert.Equal(new[] { "Unknown game type Quake" }, vm.Errors);
        Assert.Equal(VmDeprecatedFlags1329.GameStrife | VmDeprecatedFlags1329.GameChex | VmDeprecatedFlags1329.GameHexen, info.GameFilter);

        vm.ApplySpawnId(info, 0);
        vm.ApplySpawnId(info, 65535);
        Assert.Equal(65535, info.SpawnId);
        var low = Assert.Throws<InvalidOperationException>(() => vm.ApplySpawnId(info, -1));
        Assert.Equal("SpawnID must be in the range [0,65535]", low.Message);
        Assert.Equal(65535, info.SpawnId);
        Assert.Throws<InvalidOperationException>(() => vm.ApplySpawnId(info, 65536));
        Assert.Equal(65535, info.SpawnId);

        var actor = new VmDeprecatedFlags1329.Actor { Info = info };
        vm.ApplyConversationId(actor, 0, 9, 8);
        Assert.Equal(-1, info.ConversationId);
        vm.ApplyConversationId(actor, 65536, 1, 2);
        vm.ApplyConversationId(actor, -4, 3, 4);
        Assert.Equal(-1, info.ConversationId);
        vm.ApplyConversationId(actor, 65535, 7, 6);
        Assert.Equal(65535, info.ConversationId);
        Assert.Equal(new[] { 9, 8, 1, 2, 3, 4, 7, 6 }, actor.ConversationExtras);

        info.TypeName = "CustomImp";
        info.Size = 4;
        info.ActorSize = 8;
        info.StateSet = true;
        actor.DefaultTail = new byte[] { 9, 8, 7 };
        actor.Tail = new byte[] { 1 };
        vm.ApplySkipSuper(actor);
        Assert.Equal(new byte[] { 1 }, actor.Tail);
        Assert.False(info.SkipSuperSet);
        Assert.Equal(0, vm.BaggageResets);
        Assert.Equal(VmDeprecatedFlags1329.MsgOptError, vm.Messages[0].Level);
        Assert.Equal("'skip_super' is only allowed in subclasses of Actor with no additional fields and will be ignored in type CustomImp.", vm.Messages[0].Text);

        info.Size = 8;
        vm.ApplySkipSuper(actor);
        Assert.Equal("'skip_super' must appear before any state definitions.", vm.Messages[1].Text);
        Assert.False(info.SkipSuperSet);
        Assert.Equal(new byte[] { 1 }, actor.Tail);

        info.StateSet = false;
        vm.ApplySkipSuper(actor);
        Assert.Equal(new byte[] { 9, 8, 7 }, actor.Tail);
        Assert.True(info.SkipSuperSet);
        Assert.Equal(1, vm.BaggageResets);

        VmDeprecatedFlags1329.ApplyDefaultStateUsage(info, 4);
        Assert.Equal(4, info.DefaultStateUsage);
        VmDeprecatedFlags1329.ApplyDefaultStateUsage(info, -1);
        Assert.Equal(-1, info.DefaultStateUsage);
    }
}
