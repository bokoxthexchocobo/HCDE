namespace HCDE.Net.Core;

public readonly struct LivePeerRoutingState
{
    public LivePeerRoutingState(
        int consolePlayer,
        int maxClients,
        int authoritySlot,
        bool isLocalAuthority,
        bool usesHcdeService)
    {
        ConsolePlayer = consolePlayer;
        MaxClients = maxClients;
        AuthoritySlot = authoritySlot;
        IsLocalAuthority = isLocalAuthority;
        UsesHcdeService = usesHcdeService;
    }

    public int ConsolePlayer { get; }
    public int MaxClients { get; }
    public int AuthoritySlot { get; }
    public bool IsLocalAuthority { get; }
    public bool UsesHcdeService { get; }

    public bool IsAuthoritySlot(int client) => client == AuthoritySlot;

    public bool IsRoutablePeer(int client, Func<int, bool>? isClientSetupInProgress = null)
    {
        if (client < 0 || client >= MaxClients || client == ConsolePlayer || !UsesHcdeService)
            return false;
        if (IsLocalAuthority && isClientSetupInProgress?.Invoke(client) == true)
            return false;
        return true;
    }

    public bool ShouldSendControlTo(int client, Func<int, bool>? isClientSetupInProgress = null)
    {
        if (!IsRoutablePeer(client, isClientSetupInProgress))
            return false;

        return IsLocalAuthority
            ? !IsAuthoritySlot(client)
            : IsAuthoritySlot(client);
    }

    public bool ShouldSendClientInputTo(int client, Func<int, bool>? isClientSetupInProgress = null) =>
        IsRoutablePeer(client, isClientSetupInProgress) && !IsLocalAuthority && IsAuthoritySlot(client);

    public bool ShouldSendServerSnapshotTo(int client, Func<int, bool>? isClientSetupInProgress = null) =>
        IsRoutablePeer(client, isClientSetupInProgress) && IsLocalAuthority && !IsAuthoritySlot(client);

    public bool ShouldAcceptClientInputFrom(int client, Func<int, bool>? isClientSetupInProgress = null) =>
        IsRoutablePeer(client, isClientSetupInProgress) && IsLocalAuthority && !IsAuthoritySlot(client);

    public bool ShouldAcceptServerSnapshotFrom(int client, Func<int, bool>? isClientSetupInProgress = null) =>
        IsRoutablePeer(client, isClientSetupInProgress) && !IsLocalAuthority && IsAuthoritySlot(client);
}
