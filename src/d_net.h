/*
** d_net.h
**
** Networking stuff.
**
**---------------------------------------------------------------------------
**
** Copyright 1993-1996 id Software
** Copyright 1999-2016 Marisa Heit
** Copyright 2002-2016 Christoph Oelckers
** Copyright 2017-2025 GZDoom Maintainers and Contributors
** Copyright 2025-2026 UZDoom Maintainers and Contributors
**
** SPDX-License-Identifier: GPL-3.0-or-later
**
**---------------------------------------------------------------------------
**
*/

#ifndef __D_NET__
#define __D_NET__

#include "doomtype.h"
#include "doomdef.h"
#include "d_protocol.h"
#include "i_net.h"
#include <queue>

uint64_t I_msTime();
struct particle_t;
struct FMapThing;
struct FLevelLocals;
class PClassActor;
class FString;
class AActor;

enum EChatType
{
	CHAT_DISABLED,
	CHAT_TEAM_ONLY,
	CHAT_GLOBAL,
};

enum EInvasionState : uint8_t
{
	INVS_DISABLED = 0,
	INVS_WAITING,
	INVS_COUNTDOWN,
	INVS_SPAWNING,
	INVS_CLEANUP,
	INVS_INTERMISSION,
	INVS_VICTORY,
	INVS_FAILURE,
};

enum EInvasionControlAction : uint8_t
{
	INVCTL_NONE = 0,
	INVCTL_NEXTWAVE = 1,
	INVCTL_VICTORY = 2,
	INVCTL_FAILURE = 3,
};

enum EClientFlags
{
	CF_NONE = 0,
	CF_QUIT = 1,		// If set, this client sent an exit command and needs to be disconnected.
	CF_MISSING_SEQ = 1 << 1,	// If a sequence was missed/out of order, ask this client to send back over their info.
	CF_RETRANSMIT_SEQ = 1 << 2,	// If set, this client needs command data resent to them.
	CF_MISSING_CON = 1 << 3,	// If a consistency was missed/out of order, ask this client to send back over their info.
	CF_RETRANSMIT_CON = 1 << 4,	// If set, this client needs consistency data resent to them.
	CF_UPDATED = 1 << 5,	// Got an updated packet from this client.

	CF_RETRANSMIT = CF_RETRANSMIT_CON | CF_RETRANSMIT_SEQ,
	CF_MISSING = CF_MISSING_CON | CF_MISSING_SEQ,
};

class FDynamicBuffer
{
public:
	FDynamicBuffer();
	~FDynamicBuffer();

	// FDynamicBuffer owns a heap allocation and frees it in the destructor.
	// Without these, a default-generated copy/move would alias the same
	// `m_Data` pointer across two instances and the second destructor would
	// double-free. None of the FNetTic / FClientNetState plumbing actually
	// copies these (everything lives in the global `ClientStates[]` array),
	// but explicitly deleting the copy/move operations turns a future
	// accidental copy into a compile-time error instead of a runtime crash.
	FDynamicBuffer(const FDynamicBuffer&) = delete;
	FDynamicBuffer& operator=(const FDynamicBuffer&) = delete;
	FDynamicBuffer(FDynamicBuffer&&) = delete;
	FDynamicBuffer& operator=(FDynamicBuffer&&) = delete;

	void SetData(const uint8_t* data, int len);
	uint8_t* GetData(int* len = nullptr);
	TArrayView<uint8_t> GetTArrayView();

private:
	uint8_t* m_Data;
	int m_Len, m_BufferLen;
};

// New packet structure:
//
//  One byte for the net command flags.
//  Four bytes for the last sequence we got from that client.
//  Four bytes for the last consistency we got from that client.
//  If NCMD_QUITTERS set, one byte for the number of players followed by one byte for each player's consolenum.
//  One byte for the number of players.
//  One byte for the number of tics.
//   If > 0, four bytes for the base sequence being worked from.
//  One byte for the number of world tics ran.
//   If > 0, four bytes for the base consistency being worked from.
//  If from the host, one byte for how far ahead of the host we are.
//  For each player:
//   One byte for the player number.
//	 If from the host, two bytes for the latency to the host.
//   For each consistency:
//    One byte for the delta from the base consistency.
//    Two bytes for each consistency.
//   For each tic:
//    One byte for the delta from the base sequence.
//    The remaining command and event data for that player.
struct FClientNetState
{
	// Networked client data.
	struct FNetTic {
		FDynamicBuffer	Data;
		usercmd_t		Command;
	} Tics[BACKUPTICS] = {};

	// Local information about client.
	uint8_t		CurrentLatency = 0u;		// Current latency id the client is on. If the one the client sends back is > this, update RecvTime and mark a new SentTime.
	bool		bNewLatency = true;			// If the sequence was bumped, the next latency packet sent out should record the send time.
	uint16_t	AverageLatency = 0u;		// Calculate the average latency every second or so, that way it doesn't give huge variance in the scoreboard.
	uint64_t	SentTime[MAXSENDTICS] = {};	// Timestamp for when we sent out the packet to this client.
	uint64_t	RecvTime[MAXSENDTICS] = {};	// Timestamp for when the client acknowledged our last packet.
	uint64_t	LastPacketTimeMS = 0u;		// Last time any packet was heard from this client. Authorities use this to reap hard-closed ghost clients.

	int				Flags = 0;				// State of this client.

	uint8_t			StabilityBuffer = 0u;	// Account for if the client is trying to stabilize when measuring their performance.
	uint8_t			ResendID = 0u;			// Make sure that if the retransmit happened on a wait barrier, it can be properly resent back over.
	int				ResendSequenceFrom = -1; // If >= 0, send from this sequence up to the most recent one, capped to MAXSENDTICS.
	int				SequenceAck = -1;		// The last sequence the client reported from us.
	int 			CurrentSequence = -1;	// The last sequence we've gotten from this client.
	int 			AppliedSequence = -1;	// Authority cursor: the last command sequence actually fed to the think. The wall-clock authority can reach a gametic before that tic's command has arrived, so consumption is decoupled from gametic - this advances by at most one per tic toward CurrentSequence so every received command runs exactly once (no blank-command stalls, no dropped late commands).
	int 			InputGapStallTic = -1;	// Authority input-gap watchdog: gametic when CurrentSequence first stalled while the client kept sending higher sequences. -1 = not stalled. If a lost input tic is never resent (a client whose own SequenceAck is masked by the snapshot stream never re-requests it) this lets the authority resync the input stream forward instead of freezing the player's input forever. See UnwrapHCDELiveClientInputPayload.
	int64_t 		SnapshotGapStallMS = -1;	// Client snapshot-gap watchdog (mirror of InputGapStallTic for the inbound snapshot stream): I_msTime() stamp when this player's CurrentSequence first stalled far behind the authority snapshot's baseSequence. Stamped in wall-clock ms because the offending stall FREEZES gametic on the client (the world gate lowestSequence == this CurrentSequence), so a gametic-based timer would never advance. -1 = not stalled. Lets a late joiner whose seated CurrentSequence fell hopelessly behind the live snapshot frontier resync forward instead of freezing forever.

	// Every packet includes consistencies for tics that client ran. When
	// a world tic is ran, the local client will store all the consistencies
	// of the clients in their LocalConsistency. Then the consistencies will
	// be checked against retroactively as they come in.
	int ResendConsistencyFrom = -1;				// If >= 0, send from this consistency up to the most recent one, capped to MAXSENDTICS.
	int ConsistencyAck = -1;					// Last consistency the client reported from us.
	int LastVerifiedConsistency = -1;			// Last consistency we checked from this client. If < CurrentNetConsistency, run through them.
	int CurrentNetConsistency = -1;				// Last consistency we got from this client.
	int16_t NetConsistency[BACKUPTICS] = {};	// Consistencies we got from this client.
	int16_t LocalConsistency[BACKUPTICS] = {};	// Local consistency of the client to check against.
	uint32_t MalformedPacketStrikes = 0u;		// Recent malformed gameplay packets observed from this client.
	uint64_t MalformedWindowStartMS = 0u;		// Start time for strike coalescing.
};

extern FClientNetState ClientStates[MAXPLAYERS];

// Create any new ticcmds and broadcast to other players.
void NetUpdate(int tics);

EXTERN_CVAR(Int, net_echo_debug)
EXTERN_CVAR(Int, net_self_test_run_client)
EXTERN_CVAR(Int, net_invasion_latejoin_replay_test)

void HCDERecordLiveLaneTx(uint8_t lane, int client, size_t bytes);
void HCDERecordLiveLaneRx(uint8_t lane, int client, size_t bytes);
void HCDERecordLiveLaneDeferred(uint8_t lane, int client);
void HCDERecordLiveLaneBudgetClamp(uint8_t lane, int client);

bool HCDEAppendByte(uint8_t* output, size_t outputCapacity, size_t& cursor, uint8_t value);
bool HCDEAppendBE16(uint8_t* output, size_t outputCapacity, size_t& cursor, uint16_t value);
bool HCDEAppendBE32(uint8_t* output, size_t outputCapacity, size_t& cursor, uint32_t value);
bool HCDEAppendBytes(uint8_t* output, size_t outputCapacity, size_t& cursor, const uint8_t* data, size_t size);
bool HCDEReadByteField(const uint8_t* data, size_t dataSize, size_t& cursor, uint8_t& value);
bool HCDEReadBE16Field(const uint8_t* data, size_t dataSize, size_t& cursor, uint16_t& value);
bool HCDEReadBE32Field(const uint8_t* data, size_t dataSize, size_t& cursor, uint32_t& value);

// Broadcasts special packets to other players
//	to notify of game exit
void D_QuitNetGame (const char* reason = nullptr);

//? how many ticks to run?
void TryRunTics (void);
// Dedicated clients: upper bound on P_PredictClient replay (SequenceAck + lead).
int HCDEGetClientPredictionEndCapTic();

// [RH] Functions for making and using special "ticcmds"
void Net_NewClientTic();
void Net_Initialize();
uint8_t Net_GetCurrentRoomID();
void Net_AdoptServerRoomID(int room);
void Net_BeginRuntimeBootstrap(int client, const char* reason = nullptr);
void Net_RequestRuntimeResync(int client, const char* reason = nullptr);
void Net_WriteInt8(uint8_t);
void Net_WriteInt16(int16_t);
void Net_WriteInt32(int32_t);
void Net_WriteInt64(int64_t);
void Net_WriteFloat(float);
void Net_WriteDouble(double);
void Net_WriteString(const char *);
void Net_WriteBytes(const uint8_t *, int len);

void Net_DoCommand(int cmd, TArrayView<uint8_t>& stream, int player);
void Net_SkipCommand(int cmd, TArrayView<uint8_t>& stream);

bool Net_CheckCutsceneReady();
void Net_AdvanceCutscene();
void Net_StartCutscene();
void Net_TickCutsceneClientRecovery();
void Net_PlayerReadiedUp(int player);
EInvasionState Net_GetInvasionState();
const char* Net_GetInvasionStateName();
int Net_GetInvasionStateTics();
int Net_GetClassicInvasionState();
int Net_GetInvasionWave();
int Net_GetInvasionMaxWaves();
int Net_GetInvasionWaveBudget();
int Net_GetInvasionWaveSpawned();
int Net_GetInvasionWaveCleared();
int Net_GetInvasionActiveMonsterCount();
int Net_GetInvasionArchvileCount();
bool Net_IsInvasionBossWave();
int Net_GetInvasionSpawnSpotCount();
int Net_GetInvasionActiveSpawnSpotCount();
int Net_GetInvasionSpawnPlanBudget();
int Net_GetInvasionSpawnActiveTag();
bool Net_IsInvasionSpawnUsingFallback();
int Net_GetInvasionSpawnFallbackSource();
bool Net_IsInvasionClientMirrorActor(const AActor* actor);
bool Net_IsInvasionClientMirrorBlockingActor(const AActor* actor);
bool Net_IsCoopAuthorityVisualActor(const AActor* actor);
bool Net_IsCoopAuthorityVisualBlockingActor(const AActor* actor);
void Net_RegisterInvasionReplicatedMissile(AActor* missile, const AActor* source);
void Net_RegisterCoopReplicatedMissile(AActor* missile, const AActor* source);
// Authority-side co-op replication for PLAYER-fired projectiles (plasma/rocket/
// BFG, etc.). Separate from the monster path so the replicated set always matches
// the client's local-spawn suppression set. Called only from P_SpawnPlayerMissile.
void Net_RegisterCoopReplicatedPlayerMissile(AActor* missile, const AActor* source);
// Returns true when a player-fired projectile should NOT be spawned locally on
// this process because the authority owns it and replicates a visual mirror
// (dedicated co-op client firing its own projectile weapon). See
// cl_coop_mirror_own_projectiles / sv_coop_replicate_player_projectiles.
bool Net_ShouldSuppressLocalPlayerMissile(const AActor* source, PClassActor* type);
// Returns true when a player hitscan trace should NOT run locally on this
// process because the authority owns damage and replicates cosmetic impacts.
bool Net_ShouldSuppressLocalPlayerHitscan(const AActor* source);
// Authority-side: replicate a one-shot cosmetic puff/blood spawn to clients.
void Net_RecordCoopHitscanCosmetic(const AActor* source, PClassActor* effectClass,
	const DVector3& pos, DAngle yaw, DAngle pitch);
// Flush spawn events queued while client prediction was active.
void Net_FlushPendingAuthoritySpawnEvents();
void Net_RecordInvasionActorAttack(AActor* attacker, AActor* target);
void Net_RecordCoopActorAttack(AActor* attacker, AActor* target);
int Net_GetCompatDuelLimit();
int Net_ControlInvasion(int action, const char* reason = nullptr);
void Net_BeginInvasionSpawnRegistration(FLevelLocals* level);
void Net_BeginCoopMapSpawnRegistration(FLevelLocals* level);
void Net_NoteCoopMapSpawnIndex(AActor* actor, int index);
int Net_GetCoopMapSpawnIndex(const AActor* actor);
bool Net_RegisterInvasionSpawnSpotFromMapThing(FLevelLocals* level, const FMapThing* mapThing, PClassActor* spotClass);
// If `spotClass` is an invasion pickup/weapon spot, replace it with the actual
// item class so the regular map-thing spawn path drops the item at the spot.
// Returns true when a substitution was made. Must be called before
// Net_RegisterInvasionSpawnSpotFromMapThing so item spots aren't swallowed by
// the monster registrar.
bool Net_TryReplaceInvasionPickupSpot(PClassActor*& spotClass);
void Net_RecordInvasionItemSpot(const DVector3& pos, PClassActor* itemClass, int delayTics);
void Net_ProcessInvasionItemRespawns();
void Net_ClearInvasionItemSpots();
void Net_ResetCommands(bool midTic);
void Net_SetWaiting();
bool Net_LocalCanControlSettings();
void Net_ClearBuffers();
void Net_ResetClientState(int client);
void Net_ClearRuntimeClientJoinState(int clientNum);
bool Net_IsWaiting();
double Net_ModifyFrac(double ticFrac);
double Net_ModifyObjectFrac(DObject* obj, double ticFrac);
double Net_ModifyParticleFrac(particle_t* part, double ticFrac);
const char* Net_GetClientName(int client, unsigned int charLimit = 0u);
void Net_GetKickableClientList(TArray<int>& clients, TArray<FString>& labels);

void Net_TraceSetSvGametype(int value, const char* reason);
void Net_TraceSetDeathmatch(int value, const char* reason);
void Net_TraceSetTeamplay(int value, const char* reason);

enum EHCDELiveLane : uint8_t
{
	HLANE_CONTROL = 0,
	HLANE_COMMAND,
	HLANE_AUTHORITY,
	HLANE_PLAYER_SNAPSHOT,
	HLANE_ACTOR_DELTA,
	HLANE_QUERY_REGISTRY,
	HLANE_PRESENTATION_ECHO,
	HLANE_COUNT,
};

struct FHCDELagHUDMetrics
{
	int Gametic = 0;
	int ClientTic = 0;
	int CommandBacklog = 0;
	int AvailableTics = 0;
	int RunTics = 0;
	int TotalTics = 0;
	int WorldSteps = 0;
	int StabilityBuffer = 0;
	int SimStaleTics = 0;
	bool TicGateStalled = false;
	bool DedicatedClient = false;
	bool InvasionAuthority = false;
	const char* LagState = "none";
	const char* InvasionState = "disabled";
	int InvasionWave = 0;
	int TrackedMirrors = 0;
	int PendingSpawns = 0;
	int PendingEvents = 0;
	double LastMirrorMS = 0.0;
	double AvgMirrorMS = 0.0;
	double MaxMirrorMS = 0.0;
	double AvgWorldMS = 0.0;
	double MaxWorldMS = 0.0;
};

void Net_GetLagHUDMetrics(FHCDELagHUDMetrics& out);
bool Net_ShouldDrawLagHUD();
void Net_DrawLagHUD(F2DDrawer* drawer);

// Zandronum-style prediction base: latest authoritative local-player pose from
// the server snapshot plus the command sequence the server had processed when
// that pose was captured. P_PredictClient seats the pawn here and replays only
// unacknowledged commands on top.
struct FHCDELocalAuthoritativeBase
{
	DVector3 Pos = {};
	DVector3 Vel = {};
	// Body/movement-facing yaw the server owns. Reseated on prediction restart so
	// the unacked turn is reconstructed by the replay (see the seat helper).
	uint32_t YawBam = 0u;
	bool OnGround = false;
	// Server command sequence (SequenceAck) the captured pose was produced from.
	// P_PredictClient uses it as the first replay tic so only unacked commands are
	// re-simulated on top of the authoritative base.
	int BaseSequence = -1;
	bool Valid = false;
	// NOTE: view pitch is intentionally NOT stored here. Pitch is a client-owned
	// free-look axis that is never reseated to the server value (doing so forced
	// the camera down); it stays driven by the local G_Ticker integration plus the
	// unacked-tail replay in P_PredictClient. See HCDESeatLocalPlayerToAuthoritativeBase.
};

extern FHCDELocalAuthoritativeBase HCDELocalAuthoritativeBase;

class player_t;

void HCDECaptureLocalAuthoritativeBase(const DVector3& pos, const DVector3& vel,
	uint32_t yawBam, bool onGround, int baseSequence);
bool HCDESeatLocalPlayerToAuthoritativeBase(player_t& player);

// Netgame stuff (buffers and pointers, i.e. indices).

extern usercmd_t			LocalCmds[LOCALCMDTICS];
extern int					ClientTic;
// `ClientStates[]` is declared at the top of this header next to the
// `FClientNetState` struct definition. The duplicate `extern` that lived
// here was removed.

class DObject;

#endif
