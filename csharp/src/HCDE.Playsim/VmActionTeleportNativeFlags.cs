namespace HCDE.Playsim;

/// <summary><c>T_Flags</c> for <c>A_Teleport</c> from <c>p_actionfunctions.cpp</c> (3237–3251).</summary>
public static class VmActionTeleportNativeFlags
{
    public const int TfTelefrag = 0x00000001;
    public const int TfRandomDecide = 0x00000002;
    public const int TfForced = 0x00000004;
    public const int TfKeepVelocity = 0x00000008;
    public const int TfKeepAngle = 0x00000010;
    public const int TfUseSpotZ = 0x00000020;
    public const int TfNoSrcFog = 0x00000040;
    public const int TfNoDestFog = 0x00000080;
    public const int TfUseActorFog = 0x00000100;
    public const int TfNoJump = 0x00000200;
    public const int TfOverride = 0x00000400;
    public const int TfSensitiveZ = 0x00000800;

    public static bool UsesTelefrag(int flags) => (flags & TfTelefrag) != 0;

    public static bool UsesRandomDecide(int flags) => (flags & TfRandomDecide) != 0;

    public static bool ForcesOriginWhenBlocked(int flags) => (flags & TfForced) != 0;

    public static bool KeepsVelocity(int flags) => (flags & TfKeepVelocity) != 0;

    public static bool KeepsAngle(int flags) => (flags & TfKeepAngle) != 0;

    public static bool UsesSpotZ(int flags) => (flags & TfUseSpotZ) != 0;

    public static bool SuppressesSourceFog(int flags) => (flags & TfNoSrcFog) != 0;

    public static bool SuppressesDestFog(int flags) => (flags & TfNoDestFog) != 0;

    public static bool UsesActorFogTypes(int flags) => (flags & TfUseActorFog) != 0;

    public static bool SuppressesStateJump(int flags) => (flags & TfNoJump) != 0;

    public static bool OverridesNoteleport(int flags) => (flags & TfOverride) != 0;

    public static bool SensitiveZCheck(int flags) => (flags & TfSensitiveZ) != 0;
}
