using System.Text;

namespace HCDE.Client;

/// <summary>Native <c>g_taunt.cpp</c> cosmetic taunt CCMD, network opcode, and pawn playback.</summary>
public static class VmHcdeTaunt
{
    public const ulong CooldownMilliseconds = 1500;
    public const byte DemoCommandTaunt = 13;

    public static string MakeTauntSoundName(string? variant) =>
        string.IsNullOrEmpty(variant) ? "*taunt" : $"*taunt-{variant}";

    public static void PlayOnPawn(object? pawn, string? variant, IVmHcdeTauntSoundSink soundSink)
    {
        if (pawn is null)
            return;
        soundSink.PlayVoiceSound(pawn, MakeTauntSoundName(variant), volume: 1f, attenuation: 1);
    }

    public static VmHcdeTauntCommandResult TryRunTauntCommand(
        VmHcdeTauntCommandContext context,
        string? variant,
        ref ulong cooldownExpiryMilliseconds,
        IVmHcdeTauntCommandSink sink)
    {
        if (!context.IsInLevel)
            return VmHcdeTauntCommandResult.Fail("taunt: not in a level.\n");

        if (context.LocalPawn is null)
            return VmHcdeTauntCommandResult.Fail("taunt: no local pawn.\n");

        if (!TryBeginTaunt(context.NowMilliseconds, ref cooldownExpiryMilliseconds, out var remain, out var blockReason))
            return VmHcdeTauntCommandResult.Fail(blockReason!);

        WriteTauntDemoCommand(sink, variant);
        return VmHcdeTauntCommandResult.Ok();
    }

    public static void WriteTauntDemoCommand(IVmHcdeTauntCommandSink sink, string? variant)
    {
        sink.WriteInt8(DemoCommandTaunt);
        sink.WriteInt8(0);
        sink.WriteString(variant ?? string.Empty);
    }

    public static bool TryBeginTaunt(
        ulong nowMilliseconds,
        ref ulong cooldownExpiryMilliseconds,
        out ulong remainingMilliseconds,
        out string? blockReason)
    {
        if (nowMilliseconds < cooldownExpiryMilliseconds)
        {
            remainingMilliseconds = cooldownExpiryMilliseconds - nowMilliseconds;
            blockReason = $"taunt: cooldown ({remainingMilliseconds}ms remaining).\n";
            return false;
        }

        cooldownExpiryMilliseconds = nowMilliseconds + CooldownMilliseconds;
        remainingMilliseconds = 0;
        blockReason = null;
        return true;
    }

    public static byte[] EncodeTauntDemoPayload(string? variant)
    {
        var variantBytes = Encoding.UTF8.GetBytes(variant ?? string.Empty);
        var payload = new byte[2 + variantBytes.Length + 1];
        payload[0] = DemoCommandTaunt;
        payload[1] = 0;
        variantBytes.CopyTo(payload.AsSpan(2));
        return payload;
    }
}

public readonly record struct VmHcdeTauntCommandContext(bool IsInLevel, object? LocalPawn, ulong NowMilliseconds);

public readonly record struct VmHcdeTauntCommandResult(bool Succeeded, string? Message)
{
    public static VmHcdeTauntCommandResult Ok() => new(true, null);
    public static VmHcdeTauntCommandResult Fail(string message) => new(false, message);
}

public interface IVmHcdeTauntSoundSink
{
    void PlayVoiceSound(object pawn, string soundName, float volume, int attenuation);
}

public interface IVmHcdeTauntCommandSink
{
    void WriteInt8(byte value);
    void WriteString(string value);
}
