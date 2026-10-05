namespace HCDE.Playsim.Tests;

public class SpawnActivationCallbackTests
{
    [Theory]
    [InlineData(false, false, 0)]
    [InlineData(true, false, 1)]
    [InlineData(false, true, 1)]
    [InlineData(true, true, 2)]
    public void SpawnDormancyInvokesOverrideWithoutConsumingActivationFlags(bool classDormant, bool mapDormant, int calls)
    {
        var actor = new RecordingActor { Dormant = classDormant, ActivationType = 1536, IsMonster = true };
        ThingActivation.InitializeSpawn(actor, mapDormant);
        Assert.Equal(calls, actor.Calls);
        Assert.Equal(classDormant || mapDormant, actor.Dormant);
        Assert.Equal(1536, actor.ActivationType);
        Assert.False(actor.SawNonNullTrigger);
        Assert.False(actor.FirstCallWasDormant);
    }

    private sealed class RecordingActor : Actor
    {
        public int Calls { get; private set; }
        public bool SawNonNullTrigger { get; private set; }
        public bool FirstCallWasDormant { get; private set; }
        public override void Deactivate(Actor? activator)
        {
            if (Calls == 0) FirstCallWasDormant = Dormant;
            Calls++; SawNonNullTrigger |= activator != null;
            base.Deactivate(activator);
        }
    }
}
