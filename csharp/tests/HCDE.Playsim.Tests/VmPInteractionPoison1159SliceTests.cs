namespace HCDE.Playsim.Tests;

public class VmPInteractionPoison1159SliceTests
{
    [Fact]
    public void Poison_ScalesTeamDamageAndCapsAtOneHundred()
    {
        var god = VmPInteractionPoison1159Slice.Poison(
            VmPInteractionPoison1159Slice.God, 0, 0, true, false, false, 1, 10, 4, 2, true, "Poison", "Burn");
        Assert.False(god.Applied);
        Assert.Equal(4, god.Count);

        var invul = VmPInteractionPoison1159Slice.Poison(
            0, VmPInteractionPoison1159Slice.Invulnerable, 0, true, false, false, 1, 10, 4, 2, true, "Poison", "Burn");
        Assert.False(invul.Applied);

        var self = VmPInteractionPoison1159Slice.Poison(
            0, 0, 0, true, true, true, 0.5, 10, 4, 2, true, "Poison", "Burn");
        Assert.True(self.Applied);
        Assert.Equal(14, self.Count);
        Assert.Equal(2, self.PoisonerId);
        Assert.Equal("Burn", self.PainType);

        var team = VmPInteractionPoison1159Slice.Poison(
            0, 0, 0, true, false, true, 0.5, 7, 90, 8, false, "Poison", "Burn");
        Assert.Equal(93, team.Count);
        Assert.Equal(VmPInteractionPoison1159Slice.None, team.Type);
        Assert.Equal(8, team.PoisonerId);

        var capped = VmPInteractionPoison1159Slice.Poison(
            0, 0, 0, false, false, false, 1, 30, 80, 1, true, "Slime", VmPInteractionPoison1159Slice.None);
        Assert.Equal(100, capped.Count);
        Assert.Equal("Slime", capped.PainType);
        Assert.Equal(-1, capped.PoisonerId);

        var faded = VmPInteractionPoison1159Slice.Poison(
            0, 0, 0, true, false, true, 0, 12, 5, 1, true, "Poison", "Burn");
        Assert.True(faded.Applied);
        Assert.Equal(5, faded.Count);
        Assert.Equal(-1, faded.PoisonerId);
    }

    [Fact]
    public void Damage_BypassesGodForATelefragAndSavesABuddha()
    {
        var dead = VmPInteractionPoison1159Slice.Damage(
            true, 0, 40, 10, 0, 0, 0, 0, 1, 1, false, 0, false, false, false, 0, 0, 0,
            "Poison", "", 3, 0, true, true);
        Assert.True(dead.Ignored);
        Assert.Equal(40, dead.PlayerHealth);

        var god = VmPInteractionPoison1159Slice.Damage(
            true, 80, 80, 10, VmPInteractionPoison1159Slice.God, 0, 0, 0, 1, 1, false, 0,
            false, false, false, 0, 0, 0, "Poison", "", 3, 0, true, true);
        Assert.True(god.Ignored);

        var absolute = VmPInteractionPoison1159Slice.Damage(
            true, 80, 80, VmPInteractionPoison1159Slice.TelefragDamage, VmPInteractionPoison1159Slice.God2, 0, 0, 0,
            1, 1, false, 0, false, false, false, 0, 0, 1, "Poison", "", 3, 0, true, true);
        Assert.True(absolute.Ignored);

        var frag = VmPInteractionPoison1159Slice.Damage(
            true, 100, 100, VmPInteractionPoison1159Slice.TelefragDamage, VmPInteractionPoison1159Slice.God, 0, 0, 0,
            1, 1, false, 0, false, false, false, 0, 0, 1, "Poison", "", 3, 0, true, true);
        Assert.False(frag.Ignored);
        Assert.True(frag.Died);
        Assert.False(frag.Saved);
        Assert.Equal(0, frag.PlayerHealth);
        Assert.Equal(VmPInteractionPoison1159Slice.TelefragDamage, frag.Special);

        var lax = VmPInteractionPoison1159Slice.Damage(
            true, 100, 100, VmPInteractionPoison1159Slice.TelefragDamage, VmPInteractionPoison1159Slice.God, 0, 0,
            VmPInteractionPoison1159Slice.LaxTelefrag, 1, 1, false, 0, false, false, false, 0, 0, 1,
            "Poison", "", 3, 0, true, true);
        Assert.True(lax.Ignored);

        var saved = VmPInteractionPoison1159Slice.Damage(
            true, 20, 20, 30, 0, 0, 0, 0, 1, 1, false, 0, false, false, false, 0, 5, 2,
            VmPInteractionPoison1159Slice.Fire, "", 9, 0, true, true);
        Assert.True(saved.Saved);
        Assert.Equal(1, saved.PlayerHealth);
        Assert.Equal(1, saved.ActorHealth);
        Assert.False(saved.Died);
        Assert.False(saved.PlayedPain);

        var burned = VmPInteractionPoison1159Slice.Damage(
            true, 10, 40, 30, 0, 0, 0, 0, 1, 1, false, 0, false, false, false, 0, 5, 0,
            VmPInteractionPoison1159Slice.Fire, "Ice", 9, 0, true, true);
        Assert.True(burned.Died);
        Assert.Equal(VmPInteractionPoison1159Slice.Fire, burned.DamageType);
        Assert.Equal(-20, burned.ActorHealth);
        Assert.Equal(5, burned.PlayerHealth);
        Assert.True(burned.UsedStrife);
        Assert.Equal(30, burned.Special);
        Assert.False(burned.PlayedPain);

        var halved = VmPInteractionPoison1159Slice.Damage(
            true, 80, 80, 5, 0, 0, 0, 0, 0.5, 1, false, 0, false, false, false, 0, 0, 0,
            "Poison", "", 4, 64, true, true);
        Assert.Equal(78, halved.PlayerHealth);
        Assert.Equal(78, halved.ActorHealth);
        Assert.True(halved.PlayedPain);
        Assert.Equal(4, halved.AttackerId);

        var quiet = VmPInteractionPoison1159Slice.Damage(
            true, 80, 80, 5, 0, 0, 0, 0, 0.5, 1, true, 0, false, false, false, 0, 0, 0,
            "Poison", "", 4, 1, true, true);
        Assert.True(quiet.Ignored);

        var healed = VmPInteractionPoison1159Slice.Damage(
            true, 90, 40, 40, 0, 0, 0, 0, 1, 1, false, 0, true, false, false, 50, 10, 0,
            "Poison", "", 4, 1, false, true);
        Assert.Equal(1, healed.AutoRequest);
        Assert.True(healed.UsedHealth);
        Assert.True(healed.UsedStrife);
        Assert.Equal(10, healed.PlayerHealth);
        Assert.Equal(50, healed.ActorHealth);
        Assert.False(healed.PlayedPain);

        var matched = VmPInteractionPoison1159Slice.Damage(
            true, 90, 10, 10, 0, 0, 0, 0, 1, 1, false, 0, false, true, false, 30, 99, 0,
            "Poison", "", 4, 0, true, false);
        Assert.True(matched.UsedHealth);
        Assert.False(matched.UsedStrife);
        Assert.Equal(20, matched.PlayerHealth);
        Assert.False(matched.PlayedPain);
    }

    [Fact]
    public void KillAndRemove_WriteTheCheatBytesAndRefuseSuicide()
    {
        var blocked = VmPInteractionPoison1159Slice.Console("kill", new[] { "monsters" }, true, false);
        Assert.False(blocked.Hidden);
        Assert.Empty(blocked.Net);

        var monsters = VmPInteractionPoison1159Slice.Console("KILL", new[] { "Monsters" }, false, false);
        Assert.True(monsters.Hidden);
        Assert.Equal(VmPInteractionPoison1159Slice.GenericCheat, monsters.Net[0].Command);
        Assert.Equal(VmPInteractionPoison1159Slice.Massacre, monsters.Net[0].Extra);
        Assert.True(monsters.Net[0].HasExtra);

        var baddies = VmPInteractionPoison1159Slice.Console("kill", new[] { "BADDIES" }, false, false);
        Assert.Equal(VmPInteractionPoison1159Slice.Massacre2, baddies.Net[0].Extra);

        var one = VmPInteractionPoison1159Slice.Console("kill", new[] { "DoomImp" }, false, false);
        Assert.Equal(VmPInteractionPoison1159Slice.KillClass, one.Net[0].Command);
        Assert.Equal("DoomImp", one.Net[0].Text);
        Assert.False(one.Net[0].HasExtra);

        var refused = VmPInteractionPoison1159Slice.Console("kill", Array.Empty<string>(), false, true);
        Assert.False(refused.Hidden);
        Assert.Empty(refused.Net);

        var suicide = VmPInteractionPoison1159Slice.Console("kill", Array.Empty<string>(), false, false);
        Assert.Equal(VmPInteractionPoison1159Slice.Suicide, suicide.Net[0].Command);
        Assert.True(suicide.Hidden);

        var removed = VmPInteractionPoison1159Slice.Console("remove", new[] { "ZombieMan" }, false, false);
        Assert.Equal(VmPInteractionPoison1159Slice.Remove, removed.Net[0].Command);
        Assert.Equal("ZombieMan", removed.Net[0].Text);
        Assert.True(removed.Hidden);

        var usage = VmPInteractionPoison1159Slice.Console("remove", Array.Empty<string>(), false, false);
        Assert.True(usage.Usage);
        Assert.False(usage.Hidden);
        Assert.Empty(usage.Net);
    }
}
