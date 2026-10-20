namespace HCDE.Playsim;

/// <summary><c>aim_t</c> helpers, <c>P_AimLineAttack</c>, and <c>CheckForActor</c> from <c>p_map.cpp</c> (3943–4689).</summary>
public static class VmPMapAimLineAttack
{
    public const int AlfCheck3D = 1;
    public const int AlfNofriends = 1;
    public const int AlfForcenosmart = 1;
    public const int AlfNoweaponcheck = 1;
    public const int WifNoautoaim = 1;
    public const int DfNoFreelook = 1;
    public const int Mf4Spectral = 1;
    public const int Mf3Ghost = 1;
    public const int Mf3Ismonster = 1;

    public static bool CloneCopiesTrace(bool clone) => clone;

    public static bool SetResultWhenCloser(double frac, double existing) => frac < existing;

    public static bool ResultPrefersLine(bool lineNull, bool otherNull, bool friendNull) =>
        !lineNull || otherNull && friendNull;

    public static bool NullVrangeUsesDefault(bool nullAngle) => nullAngle;

    public static bool FreelookAllowed(bool allowed) => allowed;

    public static bool WeaponNoAutoaim(bool weapon, int weaponFlags) =>
        weapon && (weaponFlags & WifNoautoaim) != 0;

    public static bool ClampAimDist(double dist, double bound) => dist >= 0.5 && dist <= bound;

    public static bool DmNoFreelookBound(bool df) => df;

    public static bool AimDirUpDown(int dir) => (dir & 3) != 0;

    public static bool ShadowPitchOverride(bool handled) => handled;

    public static bool ReturnActorPitch(bool hasTarget) => hasTarget;

    public static bool TraceNotActor(int hitType) => hitType != 2;

    public static bool ThruActorsSkip(bool thru) => thru;

    public static bool SpectralSkip(bool puffSpectral, bool actorSpectral) =>
        !puffSpectral && actorSpectral;

    public static bool MthruSpeciesSkip(bool flag, bool sameSpecies) => flag && sameSpecies;

    public static bool ThruSpeciesSkip(bool flag, bool sameSpecies) => flag && sameSpecies;

    public static bool GhostSkip(bool hitGhosts, bool ghost) => hitGhosts && ghost;

    public static bool ThruBitsSkip(bool useBits, bool overlap) => useBits && overlap;

    public static bool PlayerPassSkip(bool playerCaller, bool pass) => playerCaller && pass;

    public static bool SmartAimFriend(int smart, bool isFriend) => smart < 2 && isFriend;

    public static bool SmartAimOther(int smart, bool monster, bool player) =>
        smart < 3 && !monster && !player;

    public static bool Check3dDistance(bool flag, double d3, double range) =>
        flag && d3 > range;

    public static double CosineGuard(double cosine) => cosine != 0 ? cosine : 1;

    public static bool NoFriendsFlag(int flags) => (flags & AlfNofriends) != 0;

    public static bool ForceNoSmart(int flags) => (flags & AlfForcenosmart) != 0;

    public static bool FrienderDefault(bool frienderNull) => frienderNull;

    public static bool PitchMinusVrange(bool subtract) => subtract;

    public static bool PitchPlusVrange(bool add) => add;

    public static bool AttackRangeSet(double dist) => dist > 0;

    public static bool StartFracZero(bool zero) => zero;

    public static bool UnlinkedDefault(bool unlinked) => !unlinked;

    public static bool AimTraverseCalled(bool called) => called;

    public static bool LineTargetOut(bool hasPtr) => hasPtr;

    public static bool CopyLineTarget(bool copy) => copy;

    public static bool ShadowHandlingPitch(bool handled) => handled;

    public static bool FallbackPitch(bool noTarget) => noTarget;

    public static bool HitTypeActor(int t) => t == 2;

    public static bool CallerPlayer(bool p) => p;

    public static bool PassThroughPlayer(bool pass) => pass;

    public static bool TraceStop(bool stop) => stop;

    public static bool TraceSkip(bool skip) => skip;

    public static bool AimTargetNull(bool aimNull) => aimNull;

    public static bool SmartAimLevel(int level, bool cond) => cond;

    public static bool DebugAimPrint(bool debug) => debug;
}
