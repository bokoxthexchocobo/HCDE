#include "d_net.h"
#include "d_player.h"
#include "g_level.h"
#include "doomstat.h"
#include "common/engine/debugtrace.h"
#include "d_net_diag.h"
#include "c_cvars.h"
#include "c_dispatch.h"
#include "common/console/c_console.h"
#include "i_time.h"
#include "d_event.h"
#include "i_specialpaths.h"
#include "playsim/d_player.h"

#include <climits>

#ifdef _WIN32
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#endif

EXTERN_CVAR(Int, net_echo_debug)

// Length-prefixed (BE16 length + raw bytes) string helpers for the presentation
// echo. Class TypeName FName *indices* are NOT portable between the client and
// server processes - names are interned lazily in execution order, so the same
// class gets different indices in each process (verified in the field: the
// authority's "Pistol" index decoded as "Shell" on the client). Anything the
// client must resolve back to a concrete class/state therefore has to travel as
// the actual characters, exactly like the savegame serializer writes states as
// (class-name string + offset). Capped at 255 chars; class names are short.
static bool HCDEAppendEchoString(uint8_t* output, size_t outputCapacity, size_t& cursor, const char* str)
{
	const size_t len = (str != nullptr) ? strlen(str) : 0u;
	const uint16_t clamped = uint16_t(min<size_t>(len, 255u));
	if (!HCDEAppendBE16(output, outputCapacity, cursor, clamped))
		return false;
	if (clamped == 0u)
		return true;
	return HCDEAppendBytes(output, outputCapacity, cursor, reinterpret_cast<const uint8_t*>(str), clamped);
}

static bool HCDEReadEchoString(const uint8_t* data, size_t dataSize, size_t& cursor, FString& out)
{
	out = "";
	uint16_t len = 0u;
	if (!HCDEReadBE16Field(data, dataSize, cursor, len))
		return false;
	if (len == 0u)
		return true;
	// The writer (HCDEAppendEchoString) always clamps strings to 255 bytes, so
	// a larger length is a malformed/forged field. Reject it instead of letting
	// it drive a multi-kilobyte FString allocation per snapshot.
	if (len > 255u)
		return false;
	if (cursor > dataSize || dataSize - cursor < len)
		return false;
	out = FString(reinterpret_cast<const char*>(&data[cursor]), len);
	cursor += len;
	return true;
}

// HCDE: when nonzero, dedicated clients seat their weapon psprite onto the
// authority's state every snapshot (server-authoritative weapon). When zero the
// client falls back to the legacy free-running local weapon state machine
// (kept as an escape hatch in case a mod's weapon relies on client-local
// timing). Defaults on; this is the fix for "gun sprites don't always fire
// correctly", which was caused by the client and server running two
// independent, drifting weapon state machines.
CVAR(Bool, cl_follow_server_weapon, true, CVAR_ARCHIVE | CVAR_GLOBALCONFIG)

// Server-side: last weapon echo sent per recipient client (for change detection).
static FString HCDEClientLastEchoReadyWeap[MAXPLAYERS];
static FString HCDEClientLastEchoPspriteOwner[MAXPLAYERS];
static uint32_t HCDEClientLastEchoPspriteOffset[MAXPLAYERS] = {};
static uint16_t HCDEClientLastEchoWeaponState[MAXPLAYERS] = {};
static bool HCDEClientLastEchoInitialized[MAXPLAYERS] = {};

static uint8_t HCDEComputeWeaponChangeFlags(int recipientClient, const FString& readyWeapName,
	const FString& pspriteOwnerName, uint32_t pspriteStateOffset, uint16_t weaponState)
{
	uint8_t flags = 0u;
	if (recipientClient < 0 || recipientClient >= MAXPLAYERS)
		return flags;

	if (!HCDEClientLastEchoInitialized[recipientClient])
	{
		flags = HCDEWeaponChangeReadyClass | HCDEWeaponChangeForceReseat;
		return flags;
	}

	const FString& lastReady = HCDEClientLastEchoReadyWeap[recipientClient];
	const FString& lastOwner = HCDEClientLastEchoPspriteOwner[recipientClient];
	const uint32_t lastOffset = HCDEClientLastEchoPspriteOffset[recipientClient];
	const uint16_t lastWeaponState = HCDEClientLastEchoWeaponState[recipientClient];

	if (readyWeapName.Compare(lastReady) != 0)
		flags |= HCDEWeaponChangeReadyClass;

	bool forceReseat = (flags & HCDEWeaponChangeReadyClass) != 0u;
	if (!forceReseat && !pspriteOwnerName.IsEmpty() && readyWeapName.Compare(pspriteOwnerName) == 0
		&& pspriteOwnerName.Compare(lastOwner) != 0)
	{
		// Psprite owner just aligned with the ready weapon (switch completed).
		forceReseat = true;
	}
	if (!forceReseat && (weaponState & WF_WEAPONREADY) == 0 && (lastWeaponState & WF_WEAPONREADY) != 0)
	{
		// Authority forced a lower (lost ready state).
		forceReseat = true;
	}
	if (!forceReseat && !pspriteOwnerName.IsEmpty() && pspriteOwnerName.Compare(lastOwner) != 0
		&& pspriteStateOffset != lastOffset)
	{
		// Discrete psprite state transition on the same ready weapon.
		forceReseat = true;
	}

	if (forceReseat)
		flags |= HCDEWeaponChangeForceReseat;

	return flags;
}

static void HCDEUpdateClientLastEchoState(int recipientClient, const FString& readyWeapName,
	const FString& pspriteOwnerName, uint32_t pspriteStateOffset, uint16_t weaponState)
{
	if (recipientClient < 0 || recipientClient >= MAXPLAYERS)
		return;
	HCDEClientLastEchoReadyWeap[recipientClient] = readyWeapName;
	HCDEClientLastEchoPspriteOwner[recipientClient] = pspriteOwnerName;
	HCDEClientLastEchoPspriteOffset[recipientClient] = pspriteStateOffset;
	HCDEClientLastEchoWeaponState[recipientClient] = weaponState;
	HCDEClientLastEchoInitialized[recipientClient] = true;
}

// HCDE: event-driven correction of the LOCAL view player's weapon psprite.
//
// Weapon display is client-owned between corrections (Zandronum/Odamex model):
// the local psprite state machine runs for flash/bob/animation, and the server
// sends compact weapon-change signals instead of re-seating State/Tics every
// snapshot. Only retarget ReadyWeapon on class change and force a psprite reseat
// on discrete transitions (weapon switch confirmed, forced lower).
static void Net_FollowServerWeaponPSprite(int playerNum, const FString& readyWeapName,
	const FString& ownerName, uint32_t stateOffset, int16_t tics, uint16_t weaponState,
	uint8_t weaponChangeFlags)
{
	if (!cl_follow_server_weapon)
		return;
	if (playerNum != consoleplayer)
		return;
	if (playerNum < 0 || playerNum >= MAXPLAYERS)
		return;
	if (I_IsLocalHCDEServiceAuthority())
		return;
	if (weaponChangeFlags == 0u)
		return;

	player_t& player = players[playerNum];
	if (player.mo == nullptr)
		return;

	const bool readyClassChanged = (weaponChangeFlags & HCDEWeaponChangeReadyClass) != 0u;
	const bool forceReseat = (weaponChangeFlags & HCDEWeaponChangeForceReseat) != 0u;

	if (readyWeapName.IsEmpty())
		return;

	const char* localReadyName = (player.ReadyWeapon != nullptr)
		? player.ReadyWeapon->GetClass()->TypeName.GetChars() : "";
	if (readyWeapName.Compare(localReadyName) != 0 || readyClassChanged)
	{
		PClassActor* readyClass = PClass::FindActor(FName(readyWeapName.GetChars(), true));
		AActor* readyActor = readyClass != nullptr ? player.mo->FindInventory(readyClass, true) : nullptr;
		if (readyActor == nullptr)
			return;
		player.PendingWeapon = (AActor*)WP_NOCHANGE;
		player.ReadyWeapon = readyActor;
	}

	if (forceReseat || readyClassChanged)
		player.WeaponState = weaponState;

	if (!forceReseat)
		return;

	DPSprite* sp = player.FindPSprite(PSP_WEAPON);
	if (sp == nullptr)
		return;

	if (ownerName.IsEmpty())
		return;

	PClassActor* owner = PClass::FindActor(FName(ownerName.GetChars(), true));
	if (owner == nullptr || stateOffset >= uint32_t(owner->GetStateCount()))
		return;

	FState* serverState = owner->GetStates() + stateOffset;
	if (player.ReadyWeapon != nullptr)
		sp->SetCaller(player.ReadyWeapon);

	sp->State = serverState;
	sp->Tics = tics;
	if (serverState->sprite != SPR_FIXED)
	{
		if (!serverState->GetSameFrame())
			sp->Frame = serverState->GetFrame();
		if (serverState->sprite != SPR_NOCHANGE)
			sp->Sprite = serverState->sprite;
	}
	sp->ResetInterpolation();
}

// Implement Net_LogPingSample
void Net_LogPingSample(int clientNum, int leadTics, int leadMs, int rttMs, int ticDup, int extraTics, int delta)
{
	DebugTrace::Debugf("net.ping", "client=%d lead_tics=%d lead_ms=%d rtt_ms=%d TicDup=%d extratics=%d delta=%d",
		clientNum, leadTics, leadMs, rttMs, ticDup, extraTics, delta);
}

// Implement Net_CompareEchoToLocal
void Net_CompareEchoToLocal(int clientNum, uint32_t serverTic, int playerNum,
	const FString& readyWeapName, const FString& pspriteOwnerName,
	uint32_t pspriteStateOffset, int16_t pspriteTics,
	uint16_t weaponState, uint8_t playerState, int16_t viewHeight)
{
	if (playerNum < 0 || playerNum >= MAXPLAYERS)
		return;

	const player_t& player = players[playerNum];

	// Local ReadyWeapon by portable class name.
	const char* localReadyName = (player.ReadyWeapon != nullptr)
		? player.ReadyWeapon->GetClass()->TypeName.GetChars() : "";

	// Local weapon-layer psprite state + tics.
	FState* localPspriteState = nullptr;
	int16_t localPspriteTics = 0;
	if (player.psprites != nullptr)
	{
		DPSprite* sp = const_cast<player_t&>(player).psprites;
		while (sp != nullptr && sp->ID != PSP_WEAPON)
		{
			sp = sp->Next;
		}
		if (sp != nullptr && sp->GetState() != nullptr)
		{
			localPspriteState = sp->GetState();
			localPspriteTics = sp->Tics;
		}
	}

	// Reconstruct the authority's psprite state from the portable owner-name +
	// offset so the comparison reflects the actual states, not process-local
	// FName indices.
	FState* serverPspriteState = nullptr;
	if (!pspriteOwnerName.IsEmpty())
	{
		PClassActor* owner = PClass::FindActor(FName(pspriteOwnerName.GetChars(), true));
		if (owner != nullptr && pspriteStateOffset < uint32_t(owner->GetStateCount()))
			serverPspriteState = owner->GetStates() + pspriteStateOffset;
	}

	uint16_t localWeaponState = player.WeaponState;
	uint8_t localPlayerState = player.playerstate;
	int16_t localViewHeight = int16_t(player.viewheight);

	// Log desyncs
	bool desync = false;
	if (readyWeapName.Compare(localReadyName) != 0)
	{
		desync = true;
		DebugTrace::Warningf("net.desync", "[WEAPON DESYNC] player=%d server ReadyWeapon=%s local ReadyWeapon=%s tic=%u",
			playerNum, readyWeapName.IsEmpty() ? "None" : readyWeapName.GetChars(),
			(localReadyName[0] != '\0') ? localReadyName : "None", serverTic);
	}
	if (localPspriteState != serverPspriteState
		|| (serverPspriteState != nullptr && localPspriteTics != pspriteTics))
	{
		desync = true;
		// StaticGetStateName returns an FString by value; binding .GetChars() to a
		// const char* leaves a dangling pointer once the temporary dies at the end
		// of the statement. Hold the FStrings in locals so the buffers outlive the
		// log call. This crashed on death, when the weapon psprite goes null/invalid
		// and this desync line fires.
		const FString sName = serverPspriteState != nullptr
			? FState::StaticGetStateName(serverPspriteState) : FString("None");
		const FString lName = localPspriteState != nullptr
			? FState::StaticGetStateName(localPspriteState) : FString("None");
		DebugTrace::Warningf("net.desync", "[PSPRITE STATE DESYNC] player=%d server state=%s (tics=%d) local state=%s (tics=%d) tic=%u",
			playerNum, sName.GetChars(), pspriteTics, lName.GetChars(), localPspriteTics, serverTic);
	}
	if (localWeaponState != weaponState)
	{
		desync = true;
		DebugTrace::Warningf("net.desync", "[WEAPON STATE DESYNC] player=%d server WeaponState=0x%04x local WeaponState=0x%04x tic=%u",
			playerNum, weaponState, localWeaponState, serverTic);
	}
	if (localPlayerState != playerState)
	{
		desync = true;
		DebugTrace::Warningf("net.desync", "[PLAYER STATE DESYNC] player=%d server playerstate=%d local playerstate=%d tic=%u",
			playerNum, playerState, localPlayerState, serverTic);
	}
	// Viewheight only drives the LOCAL first-person camera; remote players are
	// drawn from their world actor, never through their eyes, so their view-bob
	// curve legitimately differs and a mismatch there is invisible. Restrict the
	// diagnostic to the local view player to avoid hundreds of false warnings
	// (the remote player's landing/step bob spamming the desync log).
	if (playerNum == consoleplayer && abs(localViewHeight - (viewHeight / 256.0)) > 1.0)
	{
		desync = true;
		DebugTrace::Warningf("net.desync", "[VIEWHEIGHT DESYNC] player=%d server viewheight=%.2f local viewheight=%.2f tic=%u",
			playerNum, viewHeight / 256.0, double(localViewHeight), serverTic);
	}
}

// One replicated inventory entry (echo v4+ local-inventory block).
struct HCDEReplicatedInvItem
{
	FString ClassName;
	uint32_t Amount = 0u;
	bool IsWeapon = false;
	bool IsArmor = false;
	uint16_t HexenSlots[5] = {};
};

static void HCDEWriteHexenArmorSlots(const AActor* item, uint16_t outSlots[5])
{
	for (int i = 0; i < 5; ++i)
		outSlots[i] = 0u;
	if (item == nullptr || !item->IsKindOf(NAME_HexenArmor))
		return;
	double* slots = (double*)const_cast<AActor*>(item)->ScriptVar(NAME_Slots, nullptr);
	if (slots == nullptr)
		return;
	for (int i = 0; i < 5; ++i)
	{
		const double slot = max<double>(0.0, slots[i]);
		outSlots[i] = uint16_t(min<double>(slot, 65535.0));
	}
}

static bool Net_ShouldReplicateInventoryItem(const AActor* item)
{
	if (item == nullptr)
		return false;
	return item->IsKindOf(NAME_Weapon)
		|| item->IsKindOf(NAME_Ammo)
		|| item->IsKindOf(NAME_Armor)
		|| item->IsKindOf(NAME_Key)
		|| item->IsKindOf(NAME_Powerup)
		|| item->IsKindOf(NAME_CustomInventory);
}

static uint8_t Net_ReplicatedInventoryItemFlags(const AActor* item)
{
	uint8_t flags = 0u;
	if (item->IsKindOf(NAME_Weapon))
		flags |= 0x01u;
	if (item->IsKindOf(NAME_Armor))
		flags |= 0x02u;
	if (item->IsKindOf(NAME_Key))
		flags |= 0x04u;
	if (item->IsKindOf(NAME_Powerup))
		flags |= 0x08u;
	if (item->IsKindOf(NAME_CustomInventory))
		flags |= 0x10u;
	return flags;
}

static void HCDEApplyReplicatedArmorState(AActor* inv, const HCDEReplicatedInvItem& item)
{
	if (inv == nullptr || !item.IsArmor)
		return;
	if (inv->IsKindOf(NAME_HexenArmor))
	{
		double* slots = (double*)inv->ScriptVar(NAME_Slots, nullptr);
		if (slots != nullptr)
		{
			for (int i = 0; i < 5; ++i)
				slots[i] = double(item.HexenSlots[i]);
		}
		return;
	}
	const uint32_t raw = item.Amount;
	inv->IntVar(NAME_Amount) = raw > uint32_t(INT_MAX) ? INT_MAX : int(raw);
}

// Reconcile the local view player's Weapon/Ammo/Armor inventory to the
// authority's, using the echo local-inventory block. This is the missing half of
// server-authoritative weapons: the client can now OWN the weapons the server
// gave it, so the weapon follow can retarget ReadyWeapon and the HUD shows the
// correct ammo. The server stays authoritative for real ammo/damage; this only
// makes the client mirror what the server already decided.
//
// We give (never silently skip) missing items and set every amount to the
// authority's value. We deliberately do NOT remove client-side extras here:
// the common failure is "client is MISSING a picked-up weapon", and aggressive
// removal risks nuking inventory the client legitimately simulates between
// snapshots. Removal can be a later, separately-tested step.
static void Net_ReconcileLocalInventory(int invForPlayer, const TArray<HCDEReplicatedInvItem>& items)
{
	// The authority IS the source of truth and must not rewrite its own pawn.
	if (I_IsLocalHCDEServiceAuthority())
		return;
	// The block only ever carries the receiving client's own slot; on the client
	// that is consoleplayer. Guard against a slot mismatch (stale/forged packet).
	if (invForPlayer != consoleplayer || invForPlayer < 0 || invForPlayer >= MAXPLAYERS)
		return;

	player_t& player = players[invForPlayer];
	if (player.mo == nullptr)
		return;
	AActor* mo = player.mo;

	// Pass 1: ensure ownership. GiveInventoryType runs the standard pickup path
	// (CallTryPickup) so weapons get their sister ammo + correct setup; we then
	// override the amount in pass 2 so any pickup-default amount cannot drift the
	// HUD away from the authority's count.
	for (unsigned i = 0u; i < items.Size(); ++i)
	{
		PClassActor* cls = PClass::FindActor(FName(items[i].ClassName.GetChars(), true));
		// Only ever materialise Inventory-derived classes. The writer only emits
		// weapons/ammo/armor, but a forged echo could name an arbitrary actor
		// class; GiveInventoryType on a non-inventory class is undefined intent.
		if (cls == nullptr || !cls->IsDescendantOf(NAME_Inventory))
			continue;
		if (mo->FindInventory(cls, true) == nullptr)
			mo->GiveInventoryType(cls);
	}

	// Pass 2: pin every amount to the authority's value.
	for (unsigned i = 0u; i < items.Size(); ++i)
	{
		PClassActor* cls = PClass::FindActor(FName(items[i].ClassName.GetChars(), true));
		if (cls == nullptr || !cls->IsDescendantOf(NAME_Inventory))
			continue;
		AActor* inv = mo->FindInventory(cls, true);
		if (inv != nullptr)
		{
			if (items[i].IsArmor)
				HCDEApplyReplicatedArmorState(inv, items[i]);
			else
			{
				// Amount is a uint32 on the wire; values above INT_MAX would wrap to a
				// negative count and corrupt the HUD/ammo logic. Clamp into int range.
				const uint32_t raw = items[i].Amount;
				inv->IntVar(NAME_Amount) = raw > uint32_t(INT_MAX) ? INT_MAX : int(raw);
			}
		}
	}
}

// Append HCDEPresentationEcho
bool HCDEAppendPresentationEcho(int client, uint8_t* output, size_t outputCapacity, size_t& cursor, const uint8_t* playerNums, size_t playerCount)
{
	const size_t startCursor = cursor;
	// Decide which per-player weapon/psprite records to send.
	//
	// The receiving client's OWN record is sent only on discrete weapon changes
	// (ready-weapon class change or forced lower/switch), not every snapshot.
	// Between corrections the client owns local weapon animation (Zandronum/Odamex
	// display model). Remote players' records remain debug-only.
	uint8_t echoNums[MAXPLAYERS];
	uint8_t echoChangeFlags[MAXPLAYERS];
	uint8_t echoPlayerCount = 0u;
	const bool ownEchoValid = client >= 0 && client < MAXPLAYERS
		&& !I_IsServerReservedSlot(client) && players[client].mo != nullptr;
	if (*net_echo_debug != 0)
	{
		for (size_t i = 0u; i < playerCount && echoPlayerCount < MAXPLAYERS; ++i)
		{
			echoNums[echoPlayerCount] = playerNums[i];
			echoChangeFlags[echoPlayerCount] = HCDEWeaponChangeReadyClass | HCDEWeaponChangeForceReseat;
			++echoPlayerCount;
		}
		if (ownEchoValid)
		{
			bool found = false;
			for (uint8_t i = 0u; i < echoPlayerCount; ++i)
			{
				if (echoNums[i] == uint8_t(client)) { found = true; break; }
			}
			if (!found && echoPlayerCount < MAXPLAYERS)
			{
				echoNums[echoPlayerCount] = uint8_t(client);
				echoChangeFlags[echoPlayerCount] = HCDEWeaponChangeReadyClass | HCDEWeaponChangeForceReseat;
				++echoPlayerCount;
			}
		}
	}
	else if (ownEchoValid)
	{
		const player_t& ownPlayer = players[client];
		const char* pspriteOwnerName = "";
		uint32_t pspriteStateOffset = 0u;
		if (ownPlayer.psprites != nullptr)
		{
			DPSprite* sp = const_cast<player_t&>(ownPlayer).psprites;
			while (sp != nullptr && sp->ID != PSP_WEAPON)
				sp = sp->Next;
			if (sp != nullptr && sp->GetState() != nullptr)
			{
				FState* state = sp->GetState();
				PClassActor* owner = FState::StaticFindStateOwner(state);
				if (owner != nullptr)
				{
					pspriteOwnerName = owner->TypeName.GetChars();
					pspriteStateOffset = uint32_t(state - owner->GetStates());
				}
			}
		}
		const char* readyWeapName = (ownPlayer.ReadyWeapon != nullptr)
			? ownPlayer.ReadyWeapon->GetClass()->TypeName.GetChars() : "";
		const uint8_t changeFlags = HCDEComputeWeaponChangeFlags(client,
			FString(readyWeapName), FString(pspriteOwnerName), pspriteStateOffset, ownPlayer.WeaponState);
		if (changeFlags != 0u)
		{
			echoNums[echoPlayerCount] = uint8_t(client);
			echoChangeFlags[echoPlayerCount] = changeFlags;
			++echoPlayerCount;
			HCDEUpdateClientLastEchoState(client, FString(readyWeapName), FString(pspriteOwnerName),
				pspriteStateOffset, ownPlayer.WeaponState);
		}
	}

	if (!HCDEAppendBytes(output, outputCapacity, cursor, HCDEPresentationEchoMagic, sizeof(HCDEPresentationEchoMagic))
		|| !HCDEAppendByte(output, outputCapacity, cursor, HCDEPresentationEchoProtocolVersion)
		|| !HCDEAppendByte(output, outputCapacity, cursor, echoPlayerCount))
	{
		return false;
	}

	// Echo v4 local-inventory block for the receiving client's OWN player slot.
	// HCDE does not replicate inventory through the world delta, so a dedicated
	// client only ever owns its spawn-default loadout (Fist+Pistol) - a Shotgun
	// picked up on the server never exists on the client, so FindInventory()
	// fails and the weapon follow below cannot retarget ReadyWeapon. Mirror this
	// player's Weapon and Ammo items (class NAME + amount) so the client can
	// materialise + display them and show correct HUD ammo. Only the receiving
	// client's own slot is sent; remote players' guns render from their world
	// actor sprite, not a local psprite. Written first (before the per-player
	// records) so the reader reconciles inventory before running the follow.
	{
		const bool localInvValid = client >= 0 && client < MAXPLAYERS
			&& !I_IsServerReservedSlot(client) && players[client].mo != nullptr;
		if (!HCDEAppendByte(output, outputCapacity, cursor, localInvValid ? uint8_t(client) : uint8_t(0xFFu)))
			return false;
		if (localInvValid)
		{
			AActor* invMo = players[client].mo;
			// Count first so the wire carries an exact item count (the apply side
			// pre-sizes its parse and we never have to backpatch the cursor).
			uint16_t itemCount = 0u;
			for (AActor* item = invMo->Inventory; item != nullptr && itemCount < 255u; item = item->Inventory)
			{
				if (Net_ShouldReplicateInventoryItem(item))
					++itemCount;
			}
			if (!HCDEAppendBE16(output, outputCapacity, cursor, itemCount))
				return false;
			uint16_t emitted = 0u;
			for (AActor* item = invMo->Inventory; item != nullptr && emitted < itemCount; item = item->Inventory)
			{
				if (!Net_ShouldReplicateInventoryItem(item))
					continue;
				const bool isArmor = item->IsKindOf(NAME_Armor);
				const uint8_t flags = Net_ReplicatedInventoryItemFlags(item);
				const uint32_t amount = uint32_t(max<int>(0, item->IntVar(NAME_Amount)));
				uint16_t hexenSlots[5] = {};
				if (isArmor)
					HCDEWriteHexenArmorSlots(item, hexenSlots);
				if (!HCDEAppendByte(output, outputCapacity, cursor, flags)
					|| !HCDEAppendBE32(output, outputCapacity, cursor, amount)
					|| !HCDEAppendEchoString(output, outputCapacity, cursor, item->GetClass()->TypeName.GetChars()))
				{
					return false;
				}
				if (isArmor)
				{
					for (int slot = 0; slot < 5; ++slot)
					{
						if (!HCDEAppendBE16(output, outputCapacity, cursor, hexenSlots[slot]))
							return false;
					}
				}
				++emitted;
			}
		}
	}

	for (size_t i = 0u; i < echoPlayerCount; ++i)
	{
		const uint8_t playerNum = echoNums[i];
		if (playerNum >= MAXPLAYERS)
			return false;

		const player_t& player = players[playerNum];
		const uint8_t weaponChangeFlags = echoChangeFlags[i];

		uint32_t readyWeapNameIndex = 0;
		if (player.ReadyWeapon != nullptr)
			readyWeapNameIndex = player.ReadyWeapon->GetClass()->TypeName.GetIndex();

		uint32_t pendingWeapNameIndex = 0;
		if (player.PendingWeapon != nullptr && player.PendingWeapon != WP_NOCHANGE)
			pendingWeapNameIndex = player.PendingWeapon->GetClass()->TypeName.GetIndex();
		else if (player.PendingWeapon == WP_NOCHANGE)
			pendingWeapNameIndex = 0xFFFFFFFF; // sentinel for WP_NOCHANGE

		uint32_t pspriteStateNameIndex = 0;
		int16_t pspriteTics = 0;
		// Portable weapon-psprite state id (echo v3): owner-class NAME + state
		// offset within that class's state table. The constructed state-name
		// index above is process-local and kept only for the legacy diagnostic
		// trace. Names travel as strings because FName indices are not portable.
		const char* pspriteOwnerName = "";
		uint32_t pspriteStateOffset = 0;
		if (player.psprites != nullptr)
		{
			DPSprite* sp = const_cast<player_t&>(player).psprites;
			while (sp != nullptr && sp->ID != PSP_WEAPON)
			{
				sp = sp->Next;
			}
			if (sp != nullptr && sp->GetState() != nullptr)
			{
				FState* state = sp->GetState();
				pspriteStateNameIndex = FName(FState::StaticGetStateName(state)).GetIndex();
				pspriteTics = sp->Tics;

				PClassActor* owner = FState::StaticFindStateOwner(state);
				if (owner != nullptr)
				{
					pspriteOwnerName = owner->TypeName.GetChars();
					pspriteStateOffset = uint32_t(state - owner->GetStates());
				}
			}
		}

		const char* readyWeapName = (player.ReadyWeapon != nullptr)
			? player.ReadyWeapon->GetClass()->TypeName.GetChars() : "";

		uint16_t weaponState = player.WeaponState;
		uint8_t playerState = player.playerstate;
		int16_t viewHeight = int16_t(clamp<int>(player.viewheight * 256.0, INT16_MIN, INT16_MAX));

		if (!HCDEAppendByte(output, outputCapacity, cursor, playerNum)
			|| !HCDEAppendBE32(output, outputCapacity, cursor, readyWeapNameIndex)
			|| !HCDEAppendBE32(output, outputCapacity, cursor, pendingWeapNameIndex)
			|| !HCDEAppendBE32(output, outputCapacity, cursor, pspriteStateNameIndex)
			|| !HCDEAppendBE16(output, outputCapacity, cursor, uint16_t(pspriteTics))
			|| !HCDEAppendBE16(output, outputCapacity, cursor, weaponState)
			|| !HCDEAppendByte(output, outputCapacity, cursor, playerState)
			|| !HCDEAppendBE16(output, outputCapacity, cursor, uint16_t(viewHeight))
			|| !HCDEAppendBE32(output, outputCapacity, cursor, pspriteStateOffset)
			|| !HCDEAppendEchoString(output, outputCapacity, cursor, pspriteOwnerName)
			|| !HCDEAppendEchoString(output, outputCapacity, cursor, readyWeapName)
			|| !HCDEAppendByte(output, outputCapacity, cursor, weaponChangeFlags))
		{
			return false;
		}
	}

	HCDERecordLiveLaneTx(HLANE_PRESENTATION_ECHO, client, cursor - startCursor);
	return true;
}

// Read HCDEPresentationEcho
bool HCDEReadPresentationEcho(int clientNum, const uint8_t* body, size_t bodyBytes, size_t& cursor)
{
	if (cursor > bodyBytes || bodyBytes - cursor < 6)
		return false;

	const size_t startCursor = cursor;

	if (memcmp(&body[cursor], HCDEPresentationEchoMagic, sizeof(HCDEPresentationEchoMagic)) != 0)
		return false;

	cursor += sizeof(HCDEPresentationEchoMagic);
	const uint8_t version = body[cursor++];
	const uint8_t playerCount = body[cursor++];

	if (version != HCDEPresentationEchoProtocolVersion)
		return false;

	// Echo v4 local-inventory block (read first, before the per-player follow
	// loop, so the client owns the authority's weapons before the follow tries
	// to retarget ReadyWeapon onto one of them).
	if (cursor >= bodyBytes)
		return false;
	const uint8_t invForPlayer = body[cursor++];
	if (invForPlayer != 0xFFu)
	{
		uint16_t itemCount = 0u;
		if (!HCDEReadBE16Field(body, bodyBytes, cursor, itemCount))
			return false;
		// Each item is at least flags(1) + amount(4) + string length(2) = 7 bytes
		// (an empty class-name string body is the minimum). Armor entries add
		// five BE16 slot values (10 bytes). Reject an itemCount the remaining
		// body cannot possibly satisfy BEFORE allocating.
		const size_t kMinBytesPerItem = 7u;
		if (cursor > bodyBytes || itemCount > (bodyBytes - cursor) / kMinBytesPerItem)
			return false;
		TArray<HCDEReplicatedInvItem> invItems;
		invItems.Resize(itemCount);
		for (uint16_t i = 0u; i < itemCount; ++i)
		{
			uint8_t flags = 0u;
			uint32_t amount = 0u;
			FString className;
			if (!HCDEReadByteField(body, bodyBytes, cursor, flags)
				|| !HCDEReadBE32Field(body, bodyBytes, cursor, amount)
				|| !HCDEReadEchoString(body, bodyBytes, cursor, className))
			{
				return false;
			}
			HCDEReplicatedInvItem& it = invItems[i];
			it.ClassName = className;
			it.Amount = amount;
			it.IsWeapon = (flags & 0x01u) != 0u;
			it.IsArmor = (flags & 0x02u) != 0u;
			if (it.IsArmor)
			{
				for (int slot = 0; slot < 5; ++slot)
				{
					uint16_t slotValue = 0u;
					if (!HCDEReadBE16Field(body, bodyBytes, cursor, slotValue))
						return false;
					it.HexenSlots[slot] = slotValue;
				}
			}
		}
		Net_ReconcileLocalInventory(invForPlayer, invItems);
	}

	uint32_t serverTic = max<int>(gametic, 0); // fallback or reference tic

	for (uint8_t i = 0u; i < playerCount; ++i)
	{
		// v8 per-player fixed prefix: playerNum(1) + readyWeap(4) +
		// pendingWeap(4) + pspriteStateName(4) + pspriteTics(2) + weaponState(2)
		// + playerState(1) + viewHeight(2) + pspriteStateOffset(4) = 24, then two
		// length-prefixed strings (owner name, ready-weapon name) and weaponChangeFlags(1).
		if (cursor > bodyBytes || bodyBytes - cursor < 25)
			return false;

		uint8_t playerNum = body[cursor++];
		uint32_t readyWeapNameIndex = 0;
		uint32_t pendingWeapNameIndex = 0;
		uint32_t pspriteStateNameIndex = 0;
		uint16_t pspriteTicsRaw = 0;
		uint16_t weaponState = 0;
		uint8_t playerState = 0;
		uint16_t viewHeightRaw = 0;
		uint32_t pspriteStateOffset = 0;
		FString pspriteOwnerName;
		FString readyWeapName;
		uint8_t weaponChangeFlags = 0u;

		if (!HCDEReadBE32Field(body, bodyBytes, cursor, readyWeapNameIndex)
			|| !HCDEReadBE32Field(body, bodyBytes, cursor, pendingWeapNameIndex)
			|| !HCDEReadBE32Field(body, bodyBytes, cursor, pspriteStateNameIndex)
			|| !HCDEReadBE16Field(body, bodyBytes, cursor, pspriteTicsRaw)
			|| !HCDEReadBE16Field(body, bodyBytes, cursor, weaponState)
			|| !HCDEReadByteField(body, bodyBytes, cursor, playerState)
			|| !HCDEReadBE16Field(body, bodyBytes, cursor, viewHeightRaw)
			|| !HCDEReadBE32Field(body, bodyBytes, cursor, pspriteStateOffset)
			|| !HCDEReadEchoString(body, bodyBytes, cursor, pspriteOwnerName)
			|| !HCDEReadEchoString(body, bodyBytes, cursor, readyWeapName)
			|| !HCDEReadByteField(body, bodyBytes, cursor, weaponChangeFlags))
		{
			return false;
		}

		int16_t pspriteTics = int16_t(pspriteTicsRaw);
		int16_t viewHeight = int16_t(viewHeightRaw);

		// Event-driven weapon correction for the local view player.
		Net_FollowServerWeaponPSprite(playerNum, readyWeapName,
			pspriteOwnerName, pspriteStateOffset, pspriteTics, weaponState, weaponChangeFlags);

		Net_CompareEchoToLocal(clientNum, serverTic, playerNum,
			readyWeapName, pspriteOwnerName, pspriteStateOffset, pspriteTics,
			weaponState, playerState, viewHeight);
	}

	HCDERecordLiveLaneRx(HLANE_PRESENTATION_ECHO, clientNum, cursor - startCursor);
	return true;
}

// Clear the weapon-change echo baseline for a single recipient slot. Must run
// when a client disconnects so a reused slot (a "second connect") re-sends the
// initial ready-class/force-reseat echo instead of treating the new client as
// already initialized from the previous session.
void Net_ResetPresentationEchoStateForClient(int clientNum)
{
	if (clientNum < 0 || clientNum >= MAXPLAYERS)
		return;
	HCDEClientLastEchoReadyWeap[clientNum] = "";
	HCDEClientLastEchoPspriteOwner[clientNum] = "";
	HCDEClientLastEchoPspriteOffset[clientNum] = 0u;
	HCDEClientLastEchoWeaponState[clientNum] = 0u;
	HCDEClientLastEchoInitialized[clientNum] = false;
}

void Net_ResetPresentationEchoState()
{
	for (int i = 0; i < MAXPLAYERS; ++i)
		Net_ResetPresentationEchoStateForClient(i);
}

// CCMD(net_echo_dump)
CCMD(net_echo_dump)
{
	if (consoleplayer < 0 || consoleplayer >= MAXPLAYERS)
		return;

	const player_t& player = players[consoleplayer];

	uint32_t readyWeapNameIndex = 0;
	if (player.ReadyWeapon != nullptr)
		readyWeapNameIndex = player.ReadyWeapon->GetClass()->TypeName.GetIndex();

	uint32_t pendingWeapNameIndex = 0;
	if (player.PendingWeapon != nullptr && player.PendingWeapon != WP_NOCHANGE)
		pendingWeapNameIndex = player.PendingWeapon->GetClass()->TypeName.GetIndex();
	else if (player.PendingWeapon == WP_NOCHANGE)
		pendingWeapNameIndex = 0xFFFFFFFF;

	uint32_t pspriteStateNameIndex = 0;
	int16_t pspriteTics = 0;
	if (player.psprites != nullptr)
	{
		DPSprite* sp = const_cast<player_t&>(player).psprites;
		while (sp != nullptr && sp->ID != PSP_WEAPON)
		{
			sp = sp->Next;
		}
		if (sp != nullptr && sp->GetState() != nullptr)
		{
			pspriteStateNameIndex = FName(FState::StaticGetStateName(sp->GetState())).GetIndex();
			pspriteTics = sp->Tics;
		}
	}

	const char* readyName = FName(ENamedName(readyWeapNameIndex)).IsValidName() ? FName(ENamedName(readyWeapNameIndex)).GetChars() : "None";
	const char* pendingName = (pendingWeapNameIndex == 0xFFFFFFFF) ? "WP_NOCHANGE" : (FName(ENamedName(pendingWeapNameIndex)).IsValidName() ? FName(ENamedName(pendingWeapNameIndex)).GetChars() : "None");
	const char* pspStateName = FName(ENamedName(pspriteStateNameIndex)).IsValidName() ? FName(ENamedName(pspriteStateNameIndex)).GetChars() : "None";

	Printf("Local Echo Dump (player=%d, gametic=%d, ClientTic=%d):\n"
		"  ReadyWeapon: %s (0x%x)\n"
		"  PendingWeapon: %s (0x%x)\n"
		"  PSprite PSP_WEAPON state: %s (tics=%d, 0x%x)\n"
		"  WeaponState: 0x%04x\n"
		"  playerstate: %d\n"
		"  viewheight: %.2f\n",
		consoleplayer, gametic, ClientTic,
		readyName, readyWeapNameIndex,
		pendingName, pendingWeapNameIndex,
		pspStateName, pspriteTics, pspriteStateNameIndex,
		player.WeaponState,
		player.playerstate,
		player.viewheight);
}

#include <fstream>
#include <string>

EXTERN_CVAR(Int, net_self_test_run_client)

// Implement Net_ApplySelfTestInputs
void Net_ApplySelfTestInputs(usercmd_t* cmd, int clientTic)
{
	if (*net_self_test_run_client == 0)
		return;

	// Reset all cmd fields so it is 100% deterministic
	cmd->buttons = 0;
	cmd->pitch = 0;
	cmd->yaw = 0;
	cmd->roll = 0;
	cmd->forwardmove = 0;
	cmd->sidemove = 0;
	cmd->upmove = 0;

	// 35 tics = 1 second of Doom gameplay
	const int phase = clientTic % 250;

	if (phase < 35)
	{
		// 1. Move forward
		cmd->forwardmove = 0x3200;
	}
	else if (phase < 70)
	{
		// 2. Fire weapon
		cmd->buttons |= BT_ATTACK;
	}
	else if (phase < 105)
	{
		// 3. Switch weapon
		cmd->buttons |= BT_ZOOM;
	}
	else if (phase < 140)
	{
		// 4. Move backward
		cmd->forwardmove = -0x3200;
	}
	else if (phase < 175)
	{
		// 5. Die / Respawn
		cmd->buttons |= BT_USE;
	}
	else if (phase < 210)
	{
		// 6. Fire again
		cmd->buttons |= BT_ATTACK;
	}
	else
	{
		cmd->buttons |= BT_USE;
	}
}

// CCMD(net_self_test)
CCMD(net_self_test)
{
	Printf("Starting HCDE self-test harness...\n");

#ifdef _WIN32
	char exePath[MAX_PATH];
	if (GetModuleFileNameA(NULL, exePath, MAX_PATH) == 0)
	{
		Printf("Self-test error: unable to get current executable path.\n");
		return;
	}
	std::string currentExe = exePath;
	size_t lastSlash = currentExe.find_last_of("\\/");
	std::string binDir = (lastSlash != std::string::npos) ? currentExe.substr(0, lastSlash + 1) : "";
	std::string serverExe = binDir + "hcdeserv.exe";

	Printf("Parent executable: %s\n", currentExe.c_str());
	Printf("Server executable: %s\n", serverExe.c_str());

	// Clean up any old log files first to avoid reading stale test runs
	const FString appData = M_GetAppDataPath(true);
	std::string clientLogPath = std::string(appData.GetChars()) + "/hcde_trace.hcde.latest.log";
	std::string serverLogPath = std::string(appData.GetChars()) + "/hcde_trace.hcdeserv.latest.log";
	std::remove(clientLogPath.c_str());
	std::remove(serverLogPath.c_str());

	// Spawn hcdeserv
	STARTUPINFOA siServer;
	memset(&siServer, 0, sizeof(siServer));
	siServer.cb = sizeof(siServer);

	PROCESS_INFORMATION piServer;
	memset(&piServer, 0, sizeof(piServer));

	std::string serverCmd = "\"" + serverExe + "\" -server 2 -port 5029 -iwad doom2.wad +map map01 +net_echo_debug 1 +debugtrace_enable 1";

	Printf("Spawning server process...\n");
	BOOL okServer = CreateProcessA(NULL, &serverCmd[0], NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &siServer, &piServer);
	if (!okServer)
	{
		Printf("Self-test error: failed to spawn server (error code %lu).\n", GetLastError());
		return;
	}

	// Wait 2 seconds for server to start up and bind to port
	Sleep(2000);

	// Spawn client
	STARTUPINFOA siClient;
	memset(&siClient, 0, sizeof(siClient));
	siClient.cb = sizeof(siClient);

	PROCESS_INFORMATION piClient;
	memset(&piClient, 0, sizeof(piClient));

	std::string clientCmd = "\"" + currentExe + "\" -join 127.0.0.1:5029 -iwad doom2.wad +net_self_test_run_client 1 +net_echo_debug 1 +debugtrace_enable 1";

	Printf("Spawning client process...\n");
	BOOL okClient = CreateProcessA(NULL, &clientCmd[0], NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &siClient, &piClient);
	if (!okClient)
	{
		Printf("Self-test error: failed to spawn client (error code %lu).\n", GetLastError());
		TerminateProcess(piServer.hProcess, 0);
		CloseHandle(piServer.hProcess);
		CloseHandle(piServer.hThread);
		return;
	}

	Printf("Running deterministic simulation for 12 seconds...\n");
	Sleep(12000);

	Printf("Terminating test processes...\n");
	TerminateProcess(piClient.hProcess, 0);
	TerminateProcess(piServer.hProcess, 0);

	CloseHandle(piClient.hProcess);
	CloseHandle(piClient.hThread);
	CloseHandle(piServer.hProcess);
	CloseHandle(piServer.hThread);

	Printf("Analyzing trace logs...\n");

	// Now parse logs
	std::ifstream clientFile(clientLogPath);
	std::ifstream serverFile(serverLogPath);

	if (!clientFile.is_open())
	{
		Printf("Self-test FAILED: Client trace log not found at %s.\n", clientLogPath.c_str());
		return;
	}
	if (!serverFile.is_open())
	{
		Printf("Self-test FAILED: Server trace log not found at %s.\n", serverLogPath.c_str());
		return;
	}

	bool desyncFound = false;
	std::string line;
	int desyncCount = 0;
	int clientEvents = 0;
	int serverEvents = 0;

	while (std::getline(clientFile, line))
	{
		if (line.find("playsim.psprite") != std::string::npos || line.find("playsim.playerstate") != std::string::npos)
			clientEvents++;

		if (line.find("[PLAYER STATE DESYNC]") != std::string::npos ||
			line.find("[WEAPON DESYNC]") != std::string::npos ||
			line.find("[PSPRITE DESYNC]") != std::string::npos ||
			line.find("[VIEWHEIGHT DESYNC]") != std::string::npos)
		{
			desyncFound = true;
			desyncCount++;
			Printf("Desync found in client log: %s\n", line.c_str());
		}
	}

	while (std::getline(serverFile, line))
	{
		if (line.find("playsim.psprite") != std::string::npos || line.find("playsim.playerstate") != std::string::npos)
			serverEvents++;
	}

	Printf("Test results summary:\n");
	Printf("  Client events logged: %d\n", clientEvents);
	Printf("  Server events logged: %d\n", serverEvents);
	Printf("  Desyncs detected: %d\n", desyncCount);

	if (clientEvents == 0)
	{
		Printf("Self-test FAILED: Client logged 0 simulation events. Connection may have failed.\n");
		return;
	}

	if (desyncFound)
	{
		Printf("Self-test FAILED: Desynchronization detected between client and server.\n");
	}
	else
	{
		Printf("Self-test PASSED: All states successfully synchronized with zero desyncs!\n");
	}
#else
	Printf("Self-test is only supported on Windows platform currently.\n");
#endif
}
