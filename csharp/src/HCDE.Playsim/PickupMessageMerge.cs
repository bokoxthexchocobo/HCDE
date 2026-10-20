namespace HCDE.Playsim;

/// <summary>Native <c>a_pickups.cpp</c> <c>PrintPickupMessage</c> merge gate.</summary>
public static class PickupMessageMerge
{
    public static bool ShouldPrint(
        bool localView,
        string message,
        int gameTic,
        bool noMergePickupMessages,
        ref int lastMessageTic,
        ref string? lastMessage,
        out string resolvedMessage)
    {
        resolvedMessage = message;
        if (string.IsNullOrEmpty(message) || !localView)
            return false;

        if (!noMergePickupMessages &&
            lastMessageTic == gameTic &&
            string.Equals(lastMessage, message, StringComparison.Ordinal))
            return false;

        lastMessageTic = gameTic;
        lastMessage = message;
        if (message.Length > 0 && message[0] == '$')
            resolvedMessage = message[1..];
        return resolvedMessage.Length > 0;
    }
}
