using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDynLightData1343SliceTests
{
    [Fact]
    public void AliasSerializeApplyAndAttenuation()
    {
        var vm = new VmDynLightData1343();
        var actor = vm.AddActor("Zombie", false);
        var same = vm.GetRealType(actor);
        Assert.Same(actor, same);
        Assert.True(actor.SawReplacement);
        Assert.Null(actor.SeenLevel);
        Assert.False(actor.SeenLook);
        Assert.Equal(0, actor.DescendantChecks);

        actor.ReplacementReturnsNull = true;
        Assert.Same(actor, vm.GetRealType(actor));
        Assert.Equal(0, actor.DescendantChecks);

        actor.ReplacementReturnsNull = false;
        var other = vm.AddActor("Other", false);
        actor.Replacement = other;
        Assert.Same(actor, vm.GetRealType(actor));
        Assert.Equal(1, other.DescendantChecks);

        var deh = vm.AddActor("ZombieDeh", true);
        actor.Replacement = deh;
        Assert.Same(deh, vm.GetRealType(actor));
        Assert.False(deh.IsDescendantOf(vm.Intern("Zombie")));
        Assert.True(deh.IsDescendantOf(vm.DehackedPickup));

        var closed = new Arc { OpenObject = false };
        var sample = Sample(vm);
        vm.SerializeDefaults(closed, "defs", sample);
        Assert.Equal(new[] { "O:defs" }, closed.Ops);

        var writing = new Arc();
        vm.SerializeDefaults(writing, "defs", sample);
        Assert.Equal(new[]
        {
            "O:defs", "name", "args", "param", "pos", "type", "attenuate", "flags",
            "swapped", "spot", "explicitpitch", "spotinner", "spotouter", "pitch",
            "lightdefintensity", "o",
        }, writing.Ops);
        Assert.Equal((sbyte)1, writing.Values[11]);
        Assert.Equal(11.0, writing.Values[16]);
        Assert.Equal(22.0, writing.Values[17]);

        var copy = new LightDefaults(VmDynLightData1343.NameNone);
        var reading = new Arc { Writing = false, Reading = true, Values = writing.Values };
        vm.SerializeDefaults(reading, "defs", copy);
        Assert.Equal(sample.Name, copy.Name);
        Assert.Equal(sample.Args, copy.Args);
        Assert.Equal(3.5, copy.Param);
        Assert.Equal(1.0, copy.PosX);
        Assert.Equal(2.0, copy.PosY);
        Assert.Equal(4.0, copy.PosZ);
        Assert.Equal(VmDynLightData1343.PulseLight, copy.Type);
        Assert.Equal((sbyte)1, copy.Attenuate);
        Assert.Equal(8, copy.Flags);
        Assert.True(copy.Swapped);
        Assert.True(copy.Spot);
        Assert.True(copy.ExplicitPitch);
        Assert.Equal(11.0, copy.SpotInner);
        Assert.Equal(22.0, copy.SpotOuter);
        Assert.Equal(7.0, copy.Pitch);
        Assert.Equal(1.5, copy.LightDefIntensity);

        var emptyWrite = new Arc();
        vm.SerializeList(emptyWrite, "lights", new List<LightDefaults>());
        Assert.Empty(emptyWrite.Ops);

        var held = new LightDefaults(vm.Intern("Torch"));
        var abandoned = new List<LightDefaults> { held };
        var miss = new Arc { Writing = false, Reading = true, OpenArray = false };
        vm.SerializeList(miss, "lights", abandoned);
        Assert.Empty(abandoned);
        Assert.Equal(new[] { "A:lights" }, miss.Ops);
        Assert.Equal(vm.Intern("Torch"), held.Name);

        var fresh = new List<LightDefaults>();
        var blank = new Arc { Writing = false, Reading = true, OpenObject = false, ReadArraySize = 1 };
        vm.SerializeList(blank, "lights", fresh);
        Assert.Equal(new[] { "A:lights", "O", "a" }, blank.Ops);
        Assert.Equal(VmDynLightData1343.NameNone, fresh[0].Name);
        Assert.Equal(VmDynLightData1343.PointLight, fresh[0].Type);
        Assert.Equal((sbyte)-1, fresh[0].Attenuate);
        Assert.Equal(1.0, fresh[0].LightDefIntensity);
        Assert.Equal(10.0, fresh[0].SpotInner);
        Assert.Equal(25.0, fresh[0].SpotOuter);

        var kept = new LightDefaults(VmDynLightData1343.NameNone);
        var second = new LightDefaults(vm.Intern("Gone"));
        var shrunk = new List<LightDefaults> { kept, second };
        var keepRead = new Arc { Writing = false, Reading = true, OpenObject = false, ReadArraySize = 1 };
        vm.SerializeList(keepRead, "lights", shrunk);
        Assert.Same(kept, shrunk[0]);
        Assert.Single(shrunk);
        Assert.Equal(vm.Intern("Gone"), second.Name);

        var plain = new LightDefaults(vm.Intern("Lamp"))
        {
            Param = 4,
            PosX = 1,
            PosY = 2,
            PosZ = 3,
            LightDefIntensity = 1.25,
        };
        plain.Args[VmDynLightData1343.LightIntensity] = 10;
        var point = new DynLight { Yaw = 99, Radius = 99, Swapped = true, LastUpdate = -1, TickCount = 9 };
        plain.ApplyProperties(point);
        Assert.Equal(new[] { "assign", "offset" }, point.Trace);
        Assert.True(point.Active);
        Assert.Equal(0, point.TickCount);
        Assert.Equal(99, point.Yaw);
        Assert.Null(point.SpotOwner);
        Assert.True(point.Swapped);
        Assert.Equal(99f, point.Radius);
        Assert.Equal(-1, point.LastUpdate);
        Assert.Equal(4, point.Special);
        Assert.Equal(1.25, point.LightDefIntensity);
        Assert.Same(plain.Args, point.Args);
        plain.Args[VmDynLightData1343.LightIntensity] = 99;
        Assert.Equal(99, point.Args![VmDynLightData1343.LightIntensity]);
        Assert.Equal(1, point.OffX);
        Assert.Equal(2, point.OffY);
        Assert.Equal(3, point.OffZ);
        plain.PosX = 9;
        Assert.Equal(1, point.OffX);

        var crash = new LightDefaults(VmDynLightData1343.NameNone) { Flags = VmDynLightData1343.LfSpot };
        var crashed = new DynLight { TickCount = 7 };
        Assert.Throws<NullReferenceException>(() => crash.ApplyProperties(crashed));
        Assert.Equal(new[] { "assign", "spot" }, crashed.Trace);
        Assert.True(crashed.Active);
        Assert.Equal(7, crashed.TickCount);

        var pulsed = new LightDefaults(vm.Intern("Pulse"))
        {
            Type = VmDynLightData1343.PulseLight,
            Param = 36,
            Flags = VmDynLightData1343.LfSpot,
            Swapped = true,
            ExplicitPitch = false,
            Pitch = 7,
            PosX = 1,
            PosY = 2,
            PosZ = 3,
        };
        pulsed.Args[VmDynLightData1343.LightIntensity] = 10;
        pulsed.Args[VmDynLightData1343.LightSecondary] = 40;
        var target = new LightTarget { Yaw = 30, Pitch = 12 };
        var pulseLight = new DynLight { Target = target, Timer = 42, LightType = VmDynLightData1343.PointLight };
        pulseLight.Cycler.Cycle = 9;
        pulseLight.Cycler.Start = 0;
        pulseLight.Cycler.End = 10;
        pulseLight.Cycler.Current = 5;
        pulsed.ApplyProperties(pulseLight);
        Assert.Equal(new[] { "assign", "spot", "pulse", "offset" }, pulseLight.Trace);
        Assert.Equal(30, pulseLight.Yaw);
        Assert.Equal(12, pulseLight.Pitch);
        Assert.False(pulseLight.ExplicitPitch);
        target.Yaw = 1;
        Assert.Equal(30, pulseLight.Yaw);
        pulsed.SpotInner = 44;
        Assert.Equal(44, pulseLight.SpotOwner!.SpotInner);
        Assert.Equal(42, pulseLight.LastUpdate);
        Assert.Equal((double)(float)(36.0 / 35.0), pulseLight.Cycler.Cycle);
        Assert.Equal(40, pulseLight.Cycler.Start);
        Assert.Equal(10, pulseLight.Cycler.End);
        Assert.Equal(40, pulseLight.Cycler.Current);
        Assert.True(pulseLight.Cycler.Should);
        Assert.Equal(VmDynLightData1343.CycleSin, pulseLight.Cycler.CycleType);
        Assert.Equal(40f, pulseLight.Radius);
        Assert.True(pulseLight.Swapped);
        Assert.Equal(1, pulseLight.OffX);

        pulsed.ExplicitPitch = true;
        pulsed.ApplyProperties(pulseLight);
        Assert.Equal(7, pulseLight.Pitch);
        Assert.True(pulseLight.ExplicitPitch);

        var scaled = new DynLight
        {
            LightType = VmDynLightData1343.PointLight,
            Target = target,
        };
        scaled.Cycler.Cycle = (double)(float)(36.0 / 35.0);
        scaled.Cycler.Start = 0;
        scaled.Cycler.End = 10;
        scaled.Cycler.Current = 5;
        pulsed.Swapped = false;
        pulsed.ApplyProperties(scaled);
        Assert.Equal(10, scaled.Cycler.Current);

        var continued = new DynLight { LightType = VmDynLightData1343.PulseLight, Target = target };
        continued.Cycler.Cycle = (double)(float)(36.0 / 35.0);
        continued.Cycler.Start = 0;
        continued.Cycler.End = 10;
        continued.Cycler.Current = 5;
        pulsed.ApplyProperties(continued);
        Assert.Equal(25, continued.Cycler.Current);
        Assert.Equal(10, continued.Cycler.Start);
        Assert.Equal(40, continued.Cycler.End);

        var clamped = new LightDefaults(VmDynLightData1343.NameNone) { Type = VmDynLightData1343.PulseLight };
        clamped.Args[VmDynLightData1343.LightIntensity] = -2;
        var clampLight = new DynLight();
        clamped.ApplyProperties(clampLight);
        Assert.Equal(1f, clampLight.Radius);

        var zero = new LightDefaults(VmDynLightData1343.NameNone) { Type = VmDynLightData1343.PulseLight };
        var zeroLight = new DynLight();
        zero.ApplyProperties(zeroLight);
        Assert.Equal(1f, zeroLight.Radius);

        var unset = new LightDefaults(VmDynLightData1343.NameNone) { Flags = 9 };
        var fixedOff = new LightDefaults(VmDynLightData1343.NameNone) { Attenuate = 0, Flags = 8 };
        var fixedOn = new LightDefaults(VmDynLightData1343.NameNone) { Attenuate = 1 };
        vm.LightDefaults.Add(unset);
        vm.LightDefaults.Add(fixedOff);
        vm.LightDefaults.Add(fixedOn);
        vm.SetAttenuationForLevel(false);
        Assert.Equal(1, unset.Flags);
        Assert.Equal(8, fixedOff.Flags);
        Assert.Equal(0, fixedOn.Flags);
        Assert.Equal(0, vm.AttenuationIsSet);
        unset.Flags = 9;
        vm.SetAttenuationForLevel(false);
        Assert.Equal(9, unset.Flags);
        vm.SetAttenuationForLevel(true);
        Assert.Equal(9, unset.Flags);
        Assert.Equal(8, fixedOff.Flags);
        Assert.Equal(0, fixedOn.Flags);
        Assert.Equal(1, vm.AttenuationIsSet);
        unset.Flags = 1;
        vm.SetAttenuationForLevel(true);
        Assert.Equal(1, unset.Flags);
    }

    [Fact]
    public void DefaultsAssociationsAndStateLights()
    {
        var vm = new VmDynLightData1343();
        Assert.NotEqual(VmDynLightData1343.NameNone, vm.Intern(""));
        Assert.Equal(vm.Intern(""), vm.Intern(""));
        Assert.Equal(VmDynLightData1343.NameNone, vm.Intern(null));
        Assert.Equal(vm.Intern("Torch"), vm.Intern("torch"));

        var first = new LightDefaults(vm.Intern("torch"));
        var second = new LightDefaults(vm.Intern("torch"));
        var lamp = new LightDefaults(vm.Intern("lamp"));
        vm.LightDefaults.Add(first);
        vm.LightDefaults.Add(second);
        vm.LightDefaults.Add(lamp);
        var added = new LightDefaults(vm.Intern("Torch")) { Attenuate = 0 };
        added.Args[VmDynLightData1343.LightIntensity] = 10;
        added.Args[VmDynLightData1343.LightSecondary] = 3;
        vm.AddLightDefaults(added, 2);
        Assert.Same(second, vm.LightDefaults[0]);
        Assert.Same(lamp, vm.LightDefaults[1]);
        Assert.Same(added, vm.LightDefaults[2]);
        Assert.Equal(10, added.Args[VmDynLightData1343.LightIntensity]);
        Assert.Equal(3, added.Args[VmDynLightData1343.LightSecondary]);
        Assert.DoesNotContain(first, vm.LightDefaults);

        var scaled = new LightDefaults(vm.Intern("torch"));
        scaled.Args[VmDynLightData1343.LightIntensity] = 10;
        scaled.Args[VmDynLightData1343.LightSecondary] = 3;
        vm.AddLightDefaults(scaled, 1.5);
        Assert.Equal(15, scaled.Args[3]);
        Assert.Equal(4, scaled.Args[4]);
        Assert.Same(lamp, vm.LightDefaults[0]);
        Assert.Same(added, vm.LightDefaults[1]);
        Assert.Same(scaled, vm.LightDefaults[2]);
        Assert.DoesNotContain(second, vm.LightDefaults);

        var negative = new LightDefaults(vm.Intern("neg")) { Attenuate = 1 };
        negative.Args[3] = -3;
        negative.Args[4] = 3;
        vm.AddLightDefaults(negative, 1.5);
        Assert.Equal(-4, negative.Args[3]);
        Assert.Equal(4, negative.Args[4]);

        var wiped = new LightDefaults(vm.Intern("wipe")) { Attenuate = 1 };
        wiped.Args[3] = 10;
        wiped.Args[4] = 3;
        vm.AddLightDefaults(wiped, 0);
        Assert.Equal(0, wiped.Args[3]);
        Assert.Equal(0, wiped.Args[4]);

        vm.Sprites.Add("POSS");
        vm.Sprites.Add("TROO");
        vm.Sprites.Add("SPOS");
        var full = vm.Bind(new LightAssoc(0, "POSSA0", vm.Intern("TORCH")));
        Assert.Equal(0, full.Sprite);
        Assert.Equal(0, full.Frame);
        Assert.Same(added, full.Light);

        var rotation = vm.Bind(new LightAssoc(0, "TROOB2", vm.Intern("torch")));
        Assert.Equal(1, rotation.Sprite);
        Assert.Equal(-1, rotation.Frame);
        var zeroRot = vm.Bind(new LightAssoc(0, "TROOB0", vm.Intern("torch")));
        Assert.Equal(1, zeroRot.Frame);
        var shortName = vm.Bind(new LightAssoc(0, "SPOS", vm.Intern("torch")));
        Assert.Equal(2, shortName.Sprite);
        Assert.Equal(-1, shortName.Frame);
        var five = vm.Bind(new LightAssoc(0, "POSSA", vm.Intern("torch")));
        Assert.Equal(0, five.Sprite);
        Assert.Equal(0, five.Frame);
        var lower = vm.Bind(new LightAssoc(0, "possa", vm.Intern("missing")));
        Assert.Equal(-1, lower.Sprite);
        Assert.Equal(0, lower.Frame);
        Assert.Null(lower.Light);
        var digit = vm.Bind(new LightAssoc(0, "POSS0", vm.Intern("torch")));
        Assert.Equal('0' - 'A', digit.Frame);
        var packed = vm.Bind(new LightAssoc(0, "ABCDEFGH", vm.Intern("torch")));
        Assert.Equal(-1, packed.Sprite);
        Assert.Equal(-1, packed.Frame);
        var marked = vm.Bind(new LightAssoc(0, "ABCDE0XY", vm.Intern("torch")));
        Assert.Equal('E' - 'A', marked.Frame);
        var firstHit = vm.Bind(new LightAssoc(0, "POSSA0", vm.Intern("torch")));
        Assert.Same(added, firstHit.Light);

        var zombie = vm.AddActor("Zombie", false);
        var deh = vm.AddActor("ZombieDeh", true);
        zombie.Replacement = deh;
        var imp = vm.AddActor("Imp", false);
        var before = vm.AllocCount;
        var state = new ActorState();
        vm.AddStateLight(state, "Torch");
        Assert.Equal((short)1, state.Light);
        Assert.Equal(16, vm.ParsedCapacity);
        vm.AddStateLight(state, "Missing");
        Assert.Equal((short)1, state.Light);
        var stuck = new ActorState { Light = 5 };
        vm.AddStateLight(stuck, "Torch");
        Assert.Equal((short)5, stuck.Light);
        var noneState = new ActorState();
        vm.AddStateLight(noneState, "None");
        Assert.Equal((short)5, noneState.Light);
        var nullState = new ActorState();
        vm.AddStateLight(nullState, null);
        Assert.Equal((short)7, nullState.Light);
        Assert.Equal(VmDynLightData1343.NameNone, vm.ParsedStateLights[7]);

        vm.InitializeActorLights(new[]
        {
            new LightAssoc(zombie.Name, "POSSA0", vm.Intern("torch")),
            new LightAssoc(imp.Name, "TROOA0", vm.Intern("torch")),
            new LightAssoc(vm.Intern("Ghost"), "POSSA0", vm.Intern("torch")),
            new LightAssoc(zombie.Name, "POSSA0", vm.Intern("nosuch")),
        });
        Assert.Equal(before + 3, vm.AllocCount);
        Assert.Equal(1, vm.Orphans);
        Assert.Empty(zombie.Lights);
        Assert.Same(deh, zombie.Replacement);
        Assert.Single(deh.Lights);
        Assert.Equal(0, deh.Lights[0].Sprite);
        Assert.Equal(0, deh.Lights[0].Frame);
        Assert.Same(added, deh.Lights[0].Light);
        Assert.True(zombie.SawReplacement);
        Assert.Null(zombie.SeenLevel);
        Assert.False(zombie.SeenLook);
        Assert.Equal(2, deh.DescendantChecks);
        Assert.Equal(0, imp.DescendantChecks);
        Assert.Single(imp.Lights);
        Assert.Equal(1, imp.Lights[0].Sprite);
        Assert.Equal(0, imp.Lights[0].Frame);

        Assert.Equal(9, vm.StateLights.Count);
        Assert.Null(vm.StateLights[0]);
        Assert.Same(added, vm.StateLights[1]);
        Assert.Same(VmDynLightData1343.Missing, vm.StateLights[2]);
        Assert.Same(added, vm.StateLights[3]);
        Assert.Null(vm.StateLights[5]);
        Assert.Null(vm.StateLights[7]);
        Assert.Null(vm.StateLights[8]);
        Assert.Empty(vm.ParsedStateLights);
        Assert.Equal(0, vm.ParsedCapacity);

        var negativeLight = new ActorState { Light = -1 };
        vm.AddStateLight(negativeLight, "Torch");
        Assert.Equal((short)-1, negativeLight.Light);
        Assert.Equal(new[] { vm.Intern("Torch") }, vm.ParsedStateLights);
        Assert.Equal(16, vm.ParsedCapacity);
    }

    static LightDefaults Sample(VmDynLightData1343 vm)
    {
        var sample = new LightDefaults(vm.Intern("Torch"))
        {
            Param = 3.5,
            PosX = 1,
            PosY = 2,
            PosZ = 4,
            Type = VmDynLightData1343.PulseLight,
            Attenuate = 1,
            Flags = 8,
            Swapped = true,
            Spot = true,
            ExplicitPitch = true,
            SpotInner = 11,
            SpotOuter = 22,
            Pitch = 7,
            LightDefIntensity = 1.5,
        };
        sample.Args[0] = 1;
        sample.Args[1] = 2;
        sample.Args[2] = 3;
        sample.Args[3] = 4;
        sample.Args[4] = 5;
        return sample;
    }
}
