namespace HCDE.Playsim.Tests;

public class VmAcsActorPropertySoundTests
{
    [Fact]
    public void DoGetMasterTidNative_PrefersMaster()
    {
        Assert.Equal(42, VmAcsActorPropertySound.DoGetMasterTidNative(true, 42, 1, true, 99));
    }

    [Fact]
    public void GetActorSoundNative_MapsSoundSlots()
    {
        Assert.Equal(7, VmAcsActorPropertySound.GetActorSoundNative(
            VmAcsActorPropertySound.SoundPain, 1, 2, 7, 3, 4, 5, 6, 7, 8, 9, 10));
    }

    [Fact]
    public void SetActorPropertyByTidNative_IteratesActorsWhenTidNonZero()
    {
        var hits = new List<int>();
        VmAcsActorPropertySound.SetActorPropertyByTidNative(
            5,
            VmAcsActorPropertySound.SoundSee,
            1,
            (_, _) => hits.Add(-1),
            () => hits.Count switch { 0 => 10, 1 => 11, _ => null },
            (actor, prop) => hits.Add(actor));
        Assert.Equal(new[] { 10, 11 }, hits);
    }
}
