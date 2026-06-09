#pragma once

#include <cstdint>
#include "zstring.h"

constexpr uint8_t HCDEPresentationEchoMagic[4] = { 'E', 'C', 'H', 'O' };
// Version 3 carries a portable weapon-psprite state identifier as
// (owner-class name STRING + state offset) plus the ReadyWeapon name STRING, so
// a dedicated client can follow the authority's weapon psprite exactly instead
// of running its own divergent weapon state machine. Strings are required
// because class TypeName FName *indices* are not portable between processes
// (the same class interns to different indices), so v2's index-based id
// resolved to the wrong class on the client. Bumping the version forces client
// and server to agree on the per-player record; mixed builds reject the echo.
//
// Version 4 prepends an authoritative local-player inventory block (weapon and
// ammo class NAMES + amounts) for the receiving client's own player slot. HCDE
// never replicated inventory to clients, so a dedicated client only ever owned
// its spawn-default loadout (Fist+Pistol). A picked-up Shotgun lived only on the
// server, so FindInventory(Shotgun) on the client returned null and the weapon
// follow could not retarget ReadyWeapon - the gun stayed stuck on Fist/Pistol.
// The inventory block is read FIRST (before the per-player follow loop) so the
// client materialises the weapon locally and the follow below resolves it.
//
// v5 adds Armor (BasicArmor/HexenArmor) to the replicated inventory block. The
// local player's health is already server-authoritative via the world delta,
// but armor lived only as a client inventory item that nothing ever decremented
// (the client never simulates damage on its own pawn), so armor never went down
// when hit. Mirroring the Armor item's amount fixes the HUD armor readout.
constexpr uint8_t HCDEPresentationEchoProtocolVersion = 5u;

// Forward declarations
struct player_t;

// Ping/Latency metrics helper
void Net_LogPingSample(int clientNum, int leadTics, int leadMs, int rttMs, int ticDup, int extraTics, int delta);

// Presentation echo helper. Weapon/psprite identity is compared via portable
// class-name strings + state offset (FName indices are not portable between
// processes); the remaining scalar fields compare directly.
void Net_CompareEchoToLocal(int clientNum, uint32_t serverTic, int playerNum,
	const FString& readyWeapName, const FString& pspriteOwnerName,
	uint32_t pspriteStateOffset, int16_t pspriteTics,
	uint16_t weaponState, uint8_t playerState, int16_t viewHeight);

bool HCDEAppendPresentationEcho(int client, uint8_t* output, size_t outputCapacity, size_t& cursor, const uint8_t* playerNums, size_t playerCount);
bool HCDEReadPresentationEcho(int clientNum, const uint8_t* body, size_t bodyBytes, size_t& cursor);

struct usercmd_t;
void Net_ApplySelfTestInputs(usercmd_t* cmd, int clientTic);
