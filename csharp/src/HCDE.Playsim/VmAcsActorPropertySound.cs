namespace HCDE.Playsim;

/// <summary>ACS actor property dispatch and sound lookup from <c>p_acs.cpp</c> (4062–4164, 4740–4757).</summary>
public static class VmAcsActorPropertySound
{
    public const int SoundSee = 0;
    public const int SoundAttack = 1;
    public const int SoundPain = 2;
    public const int SoundDeath = 3;
    public const int SoundActive = 4;
    public const int SoundUse = 5;
    public const int SoundBounce = 6;
    public const int SoundWallBounce = 7;
    public const int SoundCrushPain = 8;
    public const int SoundHowl = 9;
    public const int SoundPush = 10;
    public const int NoSound = -1;

    public static int DoGetMasterTidNative(bool hasMaster, int masterTid, int friendPlayer, bool playerMoValid, int playerMoTid) =>
        hasMaster ? masterTid : friendPlayer > 0 && playerMoValid ? playerMoTid : 0;

    public static int GetActorSoundNative(
        int soundType,
        int seeSound,
        int attackSound,
        int painSound,
        int deathSound,
        int activeSound,
        int useSound,
        int bounceSound,
        int wallBounceSound,
        int crushPainSound,
        int howlSound,
        int pushSound) =>
        soundType switch
        {
            SoundSee => seeSound,
            SoundAttack => attackSound,
            SoundPain => painSound,
            SoundDeath => deathSound,
            SoundActive => activeSound,
            SoundUse => useSound,
            SoundBounce => bounceSound,
            SoundWallBounce => wallBounceSound,
            SoundCrushPain => crushPainSound,
            SoundHowl => howlSound,
            SoundPush => pushSound,
            _ => NoSound,
        };

    public static void SetActorPropertyByTidNative(
        int tid,
        int property,
        int value,
        Action<int, int> setActivatorProperty,
        Func<int?> nextActorFromIterator,
        Action<int, int> setActorProperty)
    {
        if (tid == 0)
        {
            setActivatorProperty(property, value);
            return;
        }

        while (true)
        {
            var actor = nextActorFromIterator();
            if (actor is null)
                break;
            setActorProperty(actor.Value, property, value);
        }
    }
}
