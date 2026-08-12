namespace HCDE.Net.Core;

public interface IPresentationEchoApplySink
{
    bool ReconcileInventory(byte playerSlot, PresentationEchoInventoryItem[] items);

    bool FollowWeapon(PresentationEchoPlayerRecord player);
}

public readonly struct PresentationEchoApplyResult
{
    public PresentationEchoApplyResult(
        bool inventoryApplied,
        int weaponFollowAttempts,
        int weaponFollowApplied)
    {
        InventoryApplied = inventoryApplied;
        WeaponFollowAttempts = weaponFollowAttempts;
        WeaponFollowApplied = weaponFollowApplied;
    }

    public bool InventoryApplied { get; }
    public int WeaponFollowAttempts { get; }
    public int WeaponFollowApplied { get; }
}

public sealed class PresentationEchoApplySession
{
    private readonly PresentationEchoLastState[] _lastStates;

    public PresentationEchoApplySession(int maxClients)
    {
        if (maxClients <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxClients));

        _lastStates = new PresentationEchoLastState[maxClients];
    }

    public void ResetClient(int clientSlot)
    {
        if (clientSlot < 0 || clientSlot >= _lastStates.Length)
            return;

        _lastStates[clientSlot] = default;
    }

    public void ResetAll()
    {
        for (var slot = 0; slot < _lastStates.Length; slot++)
            _lastStates[slot] = default;
    }

    public bool TryApply(
        int recipientClientSlot,
        PresentationEchoBlock block,
        IPresentationEchoApplySink sink,
        out PresentationEchoApplyResult result,
        out string? rejectReason)
    {
        result = default;
        rejectReason = null;
        if (sink == null)
        {
            rejectReason = "presentation-echo-sink-missing";
            return false;
        }

        if (recipientClientSlot < 0 || recipientClientSlot >= _lastStates.Length)
        {
            rejectReason = "presentation-echo-recipient-invalid";
            return false;
        }

        var inventoryApplied = true;
        if (block.InventoryPlayerSlot is byte inventorySlot)
        {
            inventoryApplied = sink.ReconcileInventory(inventorySlot, block.InventoryItems);
        }

        var followAttempts = 0;
        var followApplied = 0;
        foreach (var player in block.Players)
        {
            if (player.WeaponChangeFlags == 0)
                continue;

            followAttempts++;
            if (sink.FollowWeapon(player))
                followApplied++;

            if (player.PlayerNum == recipientClientSlot)
            {
                _lastStates[recipientClientSlot] = PresentationEchoWeaponChangePolicy.CreateLastState(player);
            }
        }

        result = new PresentationEchoApplyResult(inventoryApplied, followAttempts, followApplied);
        return true;
    }
}
