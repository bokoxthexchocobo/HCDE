/*
** i_net.cpp
**
** Low-level networking code. Uses BSD sockets for UDP networking.
**
**---------------------------------------------------------------------------
**
** Copyright 1993-1996 by id Software, Inc.
** Copyright 1999-2016 Marisa Heit
** Copyright 2009-2016 Christoph Oelckers
** Copyright 2017-2025 GZDoom Maintainers and Contributors
** Copyright 2025-2026 UZDoom Maintainers and Contributors
**
** SPDX-License-Identifier: GPL-3.0-or-later
**
**---------------------------------------------------------------------------
**
** Code written prior to 2026 is also licensed under:
**
** SPDX-License-Identifier: LicenseRef-Doom-Source-License
**
**---------------------------------------------------------------------------
**
*/

#include <stdlib.h>
#include <string.h>
#include <array>
#include <cerrno>
#include <cstdio>
#include <limits>
#include <mutex>

/* [Petteri] Use Winsock if compiling for Win32: */
#ifdef _WIN32
#	define WIN32_LEAN_AND_MEAN
#	define NOMINMAX
#	include <windows.h>
#	include <winsock2.h>
#	include <ws2tcpip.h>
#else
#	include <arpa/inet.h>
#	include <errno.h>
#	include <netdb.h>
#	include <netinet/in.h>
#	include <sys/ioctl.h>
#	include <sys/socket.h>
#	include <unistd.h>
#	ifdef __sun
#		include <fcntl.h>
#	endif
#endif

#include "c_cvars.h"
#include "doomstat.h"
#include "cmdlib.h"
#include "engineerrors.h"
#include "debugtrace.h"
#include "hcde_servermode.h"
#include "i_interface.h"
#include "i_net.h"
#include "m_argv.h"
#include "m_crc32.h"
#include "m_random.h"
#include "printf.h"
#include "version.h"
#include "d_net.h"
#include "d_net_blackbox.h"
#include "startupinfo.h"
#include "sv_master.h"
#include "widgets/netstartwindow.h"
#include "g_levellocals.h"
#include "playsim/d_player.h"
#include "playsim/playerstate_trace.h"
#include "filesystem.h"

#if defined(_WIN32) && defined(HCDE_DEDICATED_SERVER)
extern void I_PumpDedicatedServerConsoleWindow();
extern void I_SetDedicatedServerConsoleStatus(const char* status);
#endif

EXTERN_CVAR(Int, fraglimit)
EXTERN_CVAR(Float, timelimit)
EXTERN_CVAR(Int, sv_gametype)
CVAR(String, sv_hostname, GAMENAME " server", CVAR_ARCHIVE | CVAR_SERVERINFO)
CVAR(String, sv_motd, "Welcome to " GAMENAME, CVAR_ARCHIVE | CVAR_SERVERINFO)
// Dedicated servers are typically launched headless from tooling, so default
// to auto-starting once at least one playable client is connected.
CVAR(Bool, sv_dedicated_autostart, true, CVAR_ARCHIVE | CVAR_SERVERINFO)
CUSTOM_CVAR(Int, sv_maxplayers, 0, CVAR_ARCHIVE | CVAR_SERVERINFO)
{
	if (self < 0)
	{
		self = 0;
	}
	else if (self > MAXPLAYERS - 1)
	{
		self = MAXPLAYERS - 1;
	}
}

// Open-entry / late-join admission control. When true, a host (listen server)
// also accepts new connections while the match is already in progress, just
// like a dedicated server would. Default true so HCDE listen servers behave
// like an "open-entry listen dedicated" by default; the host can disable
// this CVar to enforce a closed roster after start.
//
// Dedicated servers (-server) always allow late-join admission regardless of
// this CVar; this knob only controls the listen-server (-host) path.
CVAR(Bool, sv_lateJoin, true, CVAR_ARCHIVE | CVAR_SERVERINFO)

/* [Petteri] Get more portable: */
#ifndef _WIN32
typedef int SOCKET;
#define SOCKET_ERROR		-1
#define INVALID_SOCKET		-1
#define closesocket			close
#define ioctlsocket			ioctl
#define Sleep(x)			usleep (x * 1000)
#define WSAEWOULDBLOCK		EWOULDBLOCK
#define WSAECONNRESET		ECONNRESET
#define WSAGetLastError()	errno
#endif

#ifndef IPPORT_USERRESERVED
#define IPPORT_USERRESERVED 5000
#endif

#ifdef _WIN32
# include "common/scripting/dap/GameEventEmit.h"
typedef int socklen_t;
const char* neterror(void);
#else
#define neterror() strerror(errno)
#endif

FARG(host, "Multiplayer", "Designates the machine as the host for a multiplayer game.", "x",
	"This machine will function as a host for a multiplayer game with x players (including this"
	" machine). It will wait for other machines to connect using the -join. parameter and then"
	" start the game when everyone is connected.");
FARG(join, "Multiplayer", "Connects to a multiplayer host.", "host's IP address[:host's port]",
	 "Connect to a host for a multiplayer game.");
FARG(server, "Multiplayer", "Starts a dedicated multiplayer server without the session window.", "x",
	"This machine will function as a dedicated multiplayer server with x players (including this"
	" machine). Use this for HCDE's separate server mode so the launcher can start a server process"
	" and a local join client without opening the interactive pregame window.");
FARG(netwaitsilent, "Multiplayer", "Suppresses the multiplayer connection status window.", "",
	"Run the pregame network handshake without opening the interactive pregame window. Launchers can"
	" use this when they intentionally want a silent dedicated-server join.");
FARG(dedicatedjoin, "Multiplayer", "Connects to a dedicated server with a reserved server authority slot.", "",
	"Treat the network arbitrator as a transport-only slot. This is used by launchers when joining"
	" HCDE's separate dedicated server executable so the server does not appear as an in-game player.");
FARG(joindedicated, "Multiplayer", "Legacy alias for dedicated server join.", "host's IP address[:host's port]",
	"Compatibility shim for older launcher args. Equivalent to -dedicatedjoin with an explicit"
	" host address; retained intentionally until every supported launcher has moved to -dedicatedjoin.");
FARG(dup, "Multiplayer", "Send less player movement commands over the network.", "x",
	"Causes " GAMENAME " to transmit fewer player movement commands across the network. Valid"
	" values range from 1–9. For example, -dup 2 would cause " GAMENAME " to send half as many"
	" movements as normal.");
FARG(port, "Multiplayer", "Specifies an alternative IP port for a network game.", "x",
	"Specifies an alternate IP port for this machine to use during a network game. By default,"
	" port 5029 is used.");
FARG(password, "", "", "",
	"");

// As per http://support.microsoft.com/kb/q192599/ the standard
// size for network buffers is 8k.
constexpr size_t MaxTransmitSize = 8000u;
// Small command packets are latency-sensitive and already fit comfortably under
// common MTUs. Compress larger snapshots, but do not spend zlib work on routine
// tic traffic every frame.
constexpr size_t MinCompressionSize = 512u;
constexpr size_t MaxPasswordSize = 256u;
// HCDE pregame service header layout:
// [0]     : CRC byte0
// [1]     : CRC byte1
// [2]     : CRC byte2
// [3]     : CRC byte3
// [4]     : command + session token prefix etc.
// [5..6]  : reserved for transport/session metadata
// [7..10] : reliable service sequence number
// [11..14]: reliable service acknowledgement number
// [15..]  : service payload
constexpr size_t HCDEServiceSequenceOffset = 7u;
constexpr size_t HCDEServiceAckOffset = 11u;
constexpr size_t HCDEServiceHeaderSize = 15u;
constexpr size_t MaxHCDEReliableServices = 16u;
constexpr uint64_t HCDEServiceResendMS = 250u;
// Minimum spacing between PRE_CONNECT_ACK admission packets for a runtime late
// joiner. DriveRuntimeSetupStateForClient runs from the live net loop and can
// execute hundreds of times between tics; without pacing it would emit a burst
// of identical connect-acks that competes with live game traffic and can starve
// the joiner's receive buffer before its user-info reply advances setup.
constexpr uint64_t HCDERuntimeConnectAckResendMS = 250u;
constexpr uint64_t HCDEServiceTimeoutMS = 15000u;
// Client-side stall ceiling. While joining (pregame or runtime late join) the
// guest treats an advancing reliable-service receive sequence as forward
// progress; READY-state host heartbeats keep that sequence climbing during a
// legitimate "waiting for the host to start" wait, so this only trips when the
// handshake is genuinely wedged (e.g. a setup packet is being lost on the path)
// and turns an indefinite hang into a clean, retryable error.
constexpr uint64_t HCDEGuestSetupProgressTimeoutMS = 30000u;
// Absolute ceiling on how long a single reliable service may stay unacked before
// the client is dropped, regardless of liveness. The per-service timeout above is
// softened by a liveness grace (a client still sending valid service traffic is not
// dropped at 15s), but that grace must not be open-ended: a peer that stays "alive"
// yet can never complete the handshake (e.g. replies are reaching a stale port and
// its acks never advance our peer-ack) would otherwise hold its slot and keep both
// nodes retransmitting -- and tracing -- forever. The oldest pending service's
// FirstSendTime resets whenever the client makes progress (acked services clear and
// fresh ones start), so this ceiling only fires on a genuinely wedged service.
constexpr uint64_t HCDEServiceHardTimeoutMS = 300000u;
constexpr uint32_t HCDEServiceMalformedStrikeLimit = 4u;
constexpr uint64_t HCDEServiceMalformedQuarantineMS = 3000u;
constexpr uint8_t HCDEConnectProtocolVersion = 1u;
constexpr uint8_t HCDEConnectMagic[4] = { 'H', 'C', 'D', '3' };

enum ENetConnectType : uint8_t
{
	// HCDE is an authority/server model with a single reliable pregame control
	// channel (PRE_HCDE_SERVICE). The legacy GZDoom peer-to-peer mesh handshake
	// types (PRE_HEARTBEAT / PRE_USER_INFO / PRE_USER_INFO_ACK / PRE_GAME_INFO /
	// PRE_GAME_INFO_ACK / PRE_GO) have been removed -- every setup step is now
	// carried as an EHCDEPregameService message instead.
	PRE_CONNECT,			// Sent from guest to host for initial connection
	PRE_CONNECT_ACK,		// Sent from host to guest to confirm they've been connected
	PRE_DISCONNECT,			// Sent from host to guest when another guest leaves

	PRE_FULL,				// Sent from host to guest if the server is full
	PRE_IN_PROGRESS,		// Sent from host to guest if the game has already started
	PRE_WRONG_PASSWORD,		// Sent from host to guest if their provided password was wrong
	PRE_VERIFICATION_ERROR,	// Sent from host to guest if something failed during the verification step.
	PRE_KICKED,				// Sent from host to guest if the host kicked them from the game
	PRE_BANNED,				// Sent from host to guest if the host banned them from the game
	PRE_PROTOCOL_ERROR,		// Sent from host to guest if HCDE service negotiation failed
	PRE_HCDE_SERVICE,		// Carries negotiated HCDE pregame service messages
	PRE_SETUP_TIMEOUT,		// Sent from host to guest if required HCDE setup messages timed out
};

enum EPreConnectAckFlags : uint8_t
{
	PRE_CONNECT_ACK_DEDICATED = 1u << 0,
	PRE_CONNECT_ACK_HCDE_SERVICE = 1u << 1,
	PRE_CONNECT_ACK_SERVER_AUTHORITY = 1u << 2,
};

enum EHCDEConnectFlags : uint8_t
{
	HCDE_CONNECT_DEDICATED_JOIN = 1u << 0,
	HCDE_CONNECT_SUPPRESS_ROOM_UI = 1u << 1,
	HCDE_CONNECT_SERVER_AUTHORITY = 1u << 2,
};

enum EConnectionStatus
{
	CSTAT_NONE,			// Guest isn't connected
	CSTAT_CONNECTING,	// Guest is trying to connect
	CSTAT_WAITING,		// Guest is waiting for game info
	CSTAT_READY,		// Guest is ready to start the game
};

enum ENetConnectFlow
{
	NCF_IDLE,
	NCF_SERVER_WAITING,
	NCF_CLIENT_AUTH,
	NCF_SYNCING,
};

enum EHCDEPregameService : uint8_t
{
	HPS_HEARTBEAT = 1,
	HPS_CLIENT_USER_INFO,
	HPS_USER_INFO_ACK,
	HPS_GAME_INFO,
	HPS_GAME_INFO_ACK,
	HPS_ROSTER,
	HPS_START_GAME,
	HPS_CONSOLE_PLAYER,
	HPS_MAP_LOAD,
	HPS_MAP_LOAD_ACK,
	HPS_START_GAME_ACK,
	HPS_ROSTER_ACK,
	HPS_BOOTSTRAP_BEGIN,
	HPS_BOOTSTRAP_ACK,
	HPS_RESYNC_REQUEST,
	HPS_RESYNC_BEGIN,
	HPS_RESYNC_ACK,
};

struct FHCDEPendingService
{
	// One queued HCDE pregame packet awaiting acknowledgement.
	bool Active = false;
	// Service opcode carried in the queued packet.
	EHCDEPregameService Service = HPS_HEARTBEAT;
	// Optional per-service identity to de-duplicate retries.
	uint8_t Key = 0u;
	// Reliable sequence number assigned at queue time.
	uint32_t Sequence = 0u;
	// First/last send timestamps for retry + timeout policy.
	uint64_t FirstSendTime = 0u;
	uint64_t LastSendTime = 0u;
	// Number of attempts sent for this packet since queueing.
	uint32_t SendCount = 0u;
	// Serialized packet body (header + payload) as it is transmitted.
	TArray<uint8_t> Packet = {};

	void Clear()
	{
		Active = false;
		Service = HPS_HEARTBEAT;
		Key = 0u;
		Sequence = 0u;
		FirstSendTime = 0u;
		LastSendTime = 0u;
		SendCount = 0u;
		Packet.Clear();
	}
};

// These need to be synced with the window backends so information about each
// client can be properly displayed.
enum EConnectionFlags : unsigned int
{
	CFL_NONE			= 0,
	CFL_CONSOLEPLAYER	= 1,
	CFL_HOST			= 1 << 1,
};

struct FConnection
{
	EConnectionStatus Status = CSTAT_NONE;
	sockaddr_in Address = {};
	uint64_t InfoAck = 0u;
	bool bHasGameInfo = false;
	bool bHasMapLoadInfo = false;
	bool bHasRosterInfo = false;
	bool bHasBootstrapInfo = false;
	bool bHasStartGameAck = false;
	uint32_t SessionToken = 0u;
	bool bHCDEConnect = false;
	uint8_t HCDEConnectVersion = 0u;
	uint8_t HCDEConnectFlags = 0u;
	uint32_t HCDEServiceTxSeq = 0u;
	uint32_t HCDEServiceRxSeq = 0u;
	uint32_t HCDEServicePeerAck = 0u;
	uint32_t HCDEServiceDuplicateCount = 0u;
	uint32_t HCDEServiceMalformedStrikes = 0u;
	uint64_t HCDEServiceMalformedUntil = 0u;
	uint64_t HCDEServiceLastValidRxTime = 0u;
	uint64_t HCDERuntimeLastConnectAckTime = 0u;
	// Throttle timestamps for the very chatty per-tic WAITING breadcrumbs. Without
	// these the WAITING loop emits thousands of identical lines per second, which
	// rolled the trace files over in ~30s and erased the connect/admission phase we
	// most need for diagnosing stuck joins.
	uint64_t HCDEServiceLastWaitLogTime = 0u;
	uint64_t HCDEServiceLastReuseLogTime = 0u;
	// True when this client was admitted while the host was already mid-match
	// (the dedicated runtime late-join path). Used to advertise dedicated /
	// server-authority ACK flags to the joiner even when the host itself is a
	// listen server, so the joiner takes the proper late-join sync path
	// instead of treating the session like a fresh pregame setup.
	bool bRuntimeJoin = false;
	FHCDEPendingService HCDEReliableServices[MaxHCDEReliableServices] = {};

	void Clear()
	{
		Status = CSTAT_NONE;
		Address = {};
		InfoAck = 0u;
		bHasGameInfo = false;
		bHasMapLoadInfo = false;
		bHasRosterInfo = false;
		bHasBootstrapInfo = false;
		bHasStartGameAck = false;
		SessionToken = 0u;
		bHCDEConnect = false;
		HCDEConnectVersion = 0u;
		HCDEConnectFlags = 0u;
		HCDEServiceTxSeq = 0u;
		HCDEServiceRxSeq = 0u;
		HCDEServicePeerAck = 0u;
		HCDEServiceDuplicateCount = 0u;
		HCDEServiceMalformedStrikes = 0u;
		HCDEServiceMalformedUntil = 0u;
		HCDEServiceLastValidRxTime = 0u;
		HCDERuntimeLastConnectAckTime = 0u;
		HCDEServiceLastWaitLogTime = 0u;
		HCDEServiceLastReuseLogTime = 0u;
		bRuntimeJoin = false;
		for (auto& service : HCDEReliableServices)
			service.Clear();
	}
};

struct FHCDEConnectInfo
{
	bool Present = false;
	uint8_t Version = 0u;
	uint8_t Flags = 0u;
};

static ENetConnectFlow NetConnectFlowState = NCF_IDLE;
static bool DedicatedServerMode = false;
static bool SilentNetStartMode = false;
static bool DedicatedJoinMode = false;
static bool DedicatedServerStartRequested = false;
static bool DedicatedServerAbortRequested = false;
static bool DedicatedLateJoinRetryAttempted = false;
static bool DedicatedLateJoinRetryPendingSend = false;
static uint64_t GuestHCDELastSetupProgressTime = 0u;
static uint32_t GuestHCDELastSetupRxSeq = 0u;

bool netgame = false;
bool multiplayer = false;
int consoleplayer = 0;
int Net_Arbitrator = 0;
FClientStack NetworkClients = {};

uint8_t	TicDup = 1u;
int	MaxClients = 1;
int RemoteClient = -1;

// Transport state ownership contract:
// All variables below (`NetBuffer`, `TransmitBuffer`, `Connected[]`, socket
// state, and the HCDE pregame profile) are owned by the main net pump. HCDE
// does not read or mutate them from worker threads; dedicated-server console
// pumping is interleaved on the same thread while `I_NetLoop()` is blocked.
// If networking is ever moved to an async thread, this block must become the
// synchronization boundary rather than sprinkling ad-hoc locks through packet
// parsers that assume a single mutable cursor.
size_t NetBufferLength = 0u;
uint8_t NetBuffer[MAX_MSGLEN] = {};

static FRandom		GameIDGen = {};
static uint8_t		GameID[8] = {};
static u_short		GamePort = (IPPORT_USERRESERVED + 29);
static SOCKET		MySocket = INVALID_SOCKET;
static FConnection	Connected[MAXPLAYERS] = {};
static uint8_t		TransmitBuffer[MaxTransmitSize] = {};
static TArray<sockaddr_in> BannedConnections = {};
static bool bGameStarted = false;
static FHCDEPregameServiceProfile HCDEPregameServiceProfile = {};

namespace
{
static uint32_t ReadBE32(const uint8_t* data)
{
	return (uint32_t(data[0]) << 24) | (uint32_t(data[1]) << 16) | (uint32_t(data[2]) << 8) | uint32_t(data[3]);
}

static void WriteBE32(uint8_t* data, uint32_t value)
{
	data[0] = uint8_t(value >> 24);
	data[1] = uint8_t(value >> 16);
	data[2] = uint8_t(value >> 8);
	data[3] = uint8_t(value);
}

static bool WriteSessionToken(size_t offset, uint32_t token);

static void BeginSetupPacket(uint8_t type, uint32_t token, size_t tokenOffset = 2u)
{
	NetBuffer[0] = NCMD_SETUP;
	NetBuffer[1] = type;
	if (!WriteSessionToken(tokenOffset, token))
		I_FatalError("Setup packet overflow");
}

static const char* HCDEServiceName(EHCDEPregameService service)
{
	switch (service)
	{
	case HPS_HEARTBEAT: return "heartbeat";
	case HPS_CLIENT_USER_INFO: return "client-user-info";
	case HPS_USER_INFO_ACK: return "user-info-ack";
	case HPS_GAME_INFO: return "game-info";
	case HPS_GAME_INFO_ACK: return "game-info-ack";
	case HPS_ROSTER: return "roster";
	case HPS_START_GAME: return "start-game";
	case HPS_CONSOLE_PLAYER: return "console-player";
	case HPS_MAP_LOAD: return "map-load";
	case HPS_MAP_LOAD_ACK: return "map-load-ack";
	case HPS_START_GAME_ACK: return "start-game-ack";
	case HPS_ROSTER_ACK: return "roster-ack";
	case HPS_BOOTSTRAP_BEGIN: return "bootstrap-begin";
	case HPS_BOOTSTRAP_ACK: return "bootstrap-ack";
	case HPS_RESYNC_REQUEST: return "resync-request";
	case HPS_RESYNC_BEGIN: return "resync-begin";
	case HPS_RESYNC_ACK: return "resync-ack";
	default: return "unknown";
	}
}

static void BeginHCDEPregameService(EHCDEPregameService service, FConnection& connection)
{
	BeginSetupPacket(PRE_HCDE_SERVICE, connection.SessionToken, 3u);
	NetBuffer[2] = uint8_t(service);
	const uint32_t seq = ++connection.HCDEServiceTxSeq;
	WriteBE32(&NetBuffer[HCDEServiceSequenceOffset], seq);
	WriteBE32(&NetBuffer[HCDEServiceAckOffset], connection.HCDEServiceRxSeq);
	NetBufferLength = HCDEServiceHeaderSize;
	DebugTrace::Markf("net", "write service %s seq=%u ack=%u", HCDEServiceName(service), seq, connection.HCDEServiceRxSeq);
}

static FHCDEPendingService* FindHCDEReliableService(FConnection& connection, EHCDEPregameService service, uint8_t key)
{
	for (auto& pending : connection.HCDEReliableServices)
	{
		if (pending.Active && pending.Service == service && pending.Key == key)
			return &pending;
	}
	return nullptr;
}

static FHCDEPendingService* FindFreeHCDEReliableService(FConnection& connection)
{
	for (auto& pending : connection.HCDEReliableServices)
	{
		if (!pending.Active)
			return &pending;
	}
	return nullptr;
}

static FHCDEPendingService* FindOldestHCDEReliableService(FConnection& connection)
{
	FHCDEPendingService* oldest = nullptr;
	for (auto& pending : connection.HCDEReliableServices)
	{
		if (pending.Active && (oldest == nullptr || pending.Sequence < oldest->Sequence))
			oldest = &pending;
	}
	return oldest;
}

static bool HasPendingHCDEReliableService(FConnection& connection)
{
	return FindOldestHCDEReliableService(connection) != nullptr;
}

static void ClearAckedHCDEReliableServices(FConnection& connection)
{
	for (auto& pending : connection.HCDEReliableServices)
	{
		if (pending.Active && pending.Sequence <= connection.HCDEServicePeerAck)
		{
			++HCDEPregameServiceProfile.ServiceQueueAcked;
			DebugTrace::Markf("net", "acked reliable service %s key=%u seq=%u", HCDEServiceName(pending.Service), pending.Key, pending.Sequence);
			pending.Clear();
		}
	}
}

static FHCDEPendingService* FindTimedOutHCDEReliableService(FConnection& connection, uint64_t now)
{
	ClearAckedHCDEReliableServices(connection);
	for (auto& pending : connection.HCDEReliableServices)
	{
		if (pending.Active && pending.SendCount > 0u && pending.FirstSendTime > 0u
			&& now - pending.FirstSendTime >= HCDEServiceTimeoutMS)
		{
			return &pending;
		}
	}
	return nullptr;
}

static uint32_t MakeSessionToken(const sockaddr_in& address, int client)
{
	uint32_t token = CalcCRC32(GameID, sizeof(GameID));
	token = AddCRC32(token, reinterpret_cast<const uint8_t*>(&address.sin_addr.s_addr), sizeof(address.sin_addr.s_addr));
	token = AddCRC32(token, reinterpret_cast<const uint8_t*>(&address.sin_port), sizeof(address.sin_port));
	token = AddCRC32(token, reinterpret_cast<const uint8_t*>(&client), sizeof(client));
	token ^= uint32_t(I_msTime() & 0xffffffffu);
	return token == 0u ? 1u : token;
}

// Repeated malformed setup/service traffic gets a few strikes before a short
// quarantine stops it from burning setup CPU.
static void NoteHCDEServiceMalformedTraffic(FConnection& connection, const char* context, const char* reason)
{
	++HCDEPregameServiceProfile.ServiceMalformedStrikes;
	++connection.HCDEServiceMalformedStrikes;
	DebugTrace::Markf("net", "%s malformed service traffic reason=%s strikes=%u", context, reason, connection.HCDEServiceMalformedStrikes);
	if (connection.HCDEServiceMalformedStrikes < HCDEServiceMalformedStrikeLimit)
		return;

	connection.HCDEServiceMalformedStrikes = 0u;
	connection.HCDEServiceMalformedUntil = I_msTime() + HCDEServiceMalformedQuarantineMS;
	++HCDEPregameServiceProfile.ServiceMalformedQuarantineActivations;
	DebugTrace::Markf("net", "%s service quarantine until=%llu", context, static_cast<unsigned long long>(connection.HCDEServiceMalformedUntil));
}

// Quarantine window used to keep repeated malformed service traffic from
// re-entering the expensive setup path every tic.
static bool HCDEServiceQuarantineActive(const FConnection& connection, uint64_t now)
{
	return connection.HCDEServiceMalformedUntil > now;
}

// Any valid token-bearing packet clears the temporary malformed-traffic backoff.
static void HCDEServiceClearQuarantine(FConnection& connection)
{
	connection.HCDEServiceMalformedStrikes = 0u;
	connection.HCDEServiceMalformedUntil = 0u;
}

static bool CheckSessionToken(FConnection& connection, uint32_t token, const char* context)
{
	if (connection.SessionToken != token)
	{
		++HCDEPregameServiceProfile.ServiceTokenMismatch;
		DebugTrace::Markf("net", "%s token mismatch expected=%08x got=%08x", context, connection.SessionToken, token);
		NoteHCDEServiceMalformedTraffic(connection, context, "token-mismatch");
		return false;
	}
	HCDEServiceClearQuarantine(connection);
	return true;
}

static bool CheckHCDEPregameService(size_t client, size_t minimumSize, const char* context)
{
	auto& connection = Connected[client];
	if (HCDEServiceQuarantineActive(connection, I_msTime()))
	{
		++HCDEPregameServiceProfile.ServiceMalformedQuarantineDrops;
		DebugTrace::Markf("net", "%s service packet dropped during quarantine until=%llu", context, static_cast<unsigned long long>(connection.HCDEServiceMalformedUntil));
		return false;
	}

	if (NetBufferLength < minimumSize)
	{
		++HCDEPregameServiceProfile.ServicePacketsTooShort;
		DebugTrace::Markf("net", "%s service packet too short len=%zu minimum=%zu", context, NetBufferLength, minimumSize);
		NoteHCDEServiceMalformedTraffic(connection, context, "too-short");
		return false;
	}
	if (!CheckSessionToken(connection, ReadBE32(&NetBuffer[3]), context))
		return false;
	connection.HCDEServiceLastValidRxTime = I_msTime();

	const uint32_t seq = ReadBE32(&NetBuffer[HCDEServiceSequenceOffset]);
	const uint32_t ack = ReadBE32(&NetBuffer[HCDEServiceAckOffset]);
	if (seq == 0u)
	{
		++HCDEPregameServiceProfile.ServiceSeqZero;
		DebugTrace::Markf("net", "%s service packet has invalid zero sequence", context);
		NoteHCDEServiceMalformedTraffic(connection, context, "zero-seq");
		return false;
	}
	if (ack > connection.HCDEServiceTxSeq)
	{
		++HCDEPregameServiceProfile.ServiceAckOutOfRange;
		DebugTrace::Markf("net", "%s service ack beyond sent range ack=%u sent=%u", context, ack, connection.HCDEServiceTxSeq);
	}
	else if (ack > connection.HCDEServicePeerAck)
	{
		connection.HCDEServicePeerAck = ack;
		ClearAckedHCDEReliableServices(connection);
	}
	if (seq <= connection.HCDEServiceRxSeq)
	{
		// A seq we have already accepted is an ordinary reliable-protocol
		// retransmission, NOT an attack. The sender simply has not yet seen our
		// ack for it (or our ack was lost), so it keeps resending its oldest
		// unacked service every HCDEServiceResendMS. We must treat this as
		// benign: drop the body so the handler does not re-run, but do NOT feed
		// the malformed-traffic strike counter.
		//
		// Crucially, the ack field above was already processed before this
		// point, so receiving a duplicate still advances our view of the peer's
		// ack and clears our own acked services. Combined with both peers
		// periodically retransmitting their oldest unacked service (each carrying
		// the latest ack), the duplicate storm self-resolves once the lost ack
		// is observed.
		//
		// Penalizing retransmits here (the previous behavior) was a latency- and
		// loss-triggered deadlock: 4 "replay" strikes quarantined the connection
		// for 3s, and the quarantine check at the top of this function drops
		// every packet -- including the map-load service a late-joiner needs --
		// before the ack field can be read, so the handshake never advanced. A
		// LAN client joining at server start rarely retransmits and slipped past
		// it; a higher-latency late-joiner reliably hit it and got stuck at
		// "Waiting for server start".
		++HCDEPregameServiceProfile.ServiceSeqReplayOrDuplicate;
		++connection.HCDEServiceDuplicateCount;
		DebugTrace::Markf("net", "%s duplicate retransmit service seq=%u last=%u count=%u (benign)", context, seq, connection.HCDEServiceRxSeq, connection.HCDEServiceDuplicateCount);
		return false;
	}

	connection.HCDEServiceRxSeq = seq;
	HCDEServiceClearQuarantine(connection);
	return true;
}

static bool FindStringEnd(size_t start, size_t limit, size_t& end)
{
	for (size_t i = start; i < limit; ++i)
	{
		if (NetBuffer[i] == 0u)
		{
			end = i + 1u;
			return true;
		}
	}
	return false;
}

static bool ReadHCDEConnectInfo(size_t offset, FHCDEConnectInfo& info)
{
	info = {};
	if (offset + sizeof(HCDEConnectMagic) + 2u > NetBufferLength)
		return false;
	if (memcmp(&NetBuffer[offset], HCDEConnectMagic, sizeof(HCDEConnectMagic)) != 0)
		return false;

	info.Present = true;
	info.Version = NetBuffer[offset + sizeof(HCDEConnectMagic)];
	info.Flags = NetBuffer[offset + sizeof(HCDEConnectMagic) + 1u];
	return true;
}

static void AppendHCDEConnectInfo(uint8_t flags)
{
	if (NetBufferLength + sizeof(HCDEConnectMagic) + 2u > MAX_MSGLEN)
		I_FatalError("HCDE connect packet overflow");

	memcpy(&NetBuffer[NetBufferLength], HCDEConnectMagic, sizeof(HCDEConnectMagic));
	NetBufferLength += sizeof(HCDEConnectMagic);
	NetBuffer[NetBufferLength++] = HCDEConnectProtocolVersion;
	NetBuffer[NetBufferLength++] = flags;
}

static uint8_t BuildLocalHCDEConnectFlags()
{
	uint8_t flags = 0u;
	if (DedicatedJoinMode)
	{
		flags |= HCDE_CONNECT_DEDICATED_JOIN;
		// Dedicated service-connect servers historically saw this bit together
		// with -dedicatedjoin. Keep the wire signature stable even when the
		// local pregame window stays visible.
		flags |= HCDE_CONNECT_SUPPRESS_ROOM_UI;
	}
	if (SilentNetStartMode)
		flags |= HCDE_CONNECT_SUPPRESS_ROOM_UI;
	if (DedicatedJoinMode)
		flags |= HCDE_CONNECT_SERVER_AUTHORITY;
	return flags;
}

static const char* ConnectFlowName(ENetConnectFlow flow)
{
	switch (flow)
	{
	case NCF_SERVER_WAITING: return DedicatedServerMode ? "server-waiting" : "host-waiting";
	case NCF_CLIENT_AUTH: return DedicatedJoinMode ? "client-auth" : "guest-contacting";
	case NCF_SYNCING: return "syncing";
	default: return "idle";
	}
}

static const char* ServerGameModeName(uint8_t gameMode, bool isDeathmatch, bool isTeamplay)
{
	switch (gameMode)
	{
	case 1: return "Deathmatch";
	case 2: return "Team Deathmatch";
	case 3: return "CTF";
	case 4: return "Invasion";
	default: break;
	}

	if (isDeathmatch)
		return isTeamplay ? "Deathmatch + Teamplay" : "Deathmatch";
	return isTeamplay ? "Co-op + Teamplay" : "Co-op";
}

static const char* ServerInvasionStateName(uint8_t invasionState)
{
	switch (invasionState)
	{
	case INVS_DISABLED: return "disabled";
	case INVS_WAITING: return "waiting";
	case INVS_COUNTDOWN: return "countdown";
	case INVS_SPAWNING: return "spawning";
	case INVS_CLEANUP: return "cleanup";
	case INVS_INTERMISSION: return "intermission";
	case INVS_VICTORY: return "victory";
	case INVS_FAILURE: return "failure";
	default: return "unknown";
	}
}

constexpr uint8_t INVSPAWNQF_FALLBACK = 1u << 0;
constexpr uint8_t INVSPAWNQF_SOURCE_SHIFT = 1u;
constexpr uint8_t INVSPAWNQF_SOURCE_MASK = 0x0Eu;
constexpr uint64_t QuerySnapshotCacheIntervalMs = 200u;

static int CountConnectedClients()
{
	int count = 0;
	const int firstClient = I_GetFirstPlayableClientSlot();
	for (int i = firstClient; i < MaxClients; ++i)
	{
		if (Connected[i].Status != CSTAT_NONE)
			++count;
	}
	return count;
}

static FServerQuerySnapshot BuildServerQuerySnapshot()
{
	FServerQuerySnapshot snapshot = {};
	const char* hostname = sv_hostname;
	snapshot.HostName = hostname != nullptr && hostname[0] != 0 ? FString(hostname) : FString(GAMENAME " server");
	// Query consumers depend on stable map ids for automation (for example map
	// transition validation), so prefer map lump names over display titles.
	if (primaryLevel != nullptr && primaryLevel->MapName.IsNotEmpty())
		snapshot.MapName = primaryLevel->MapName;
	else if (level.MapName.IsNotEmpty())
		snapshot.MapName = level.MapName;
	else if (primaryLevel != nullptr && primaryLevel->LevelName.IsNotEmpty())
		snapshot.MapName = primaryLevel->LevelName;
	else if (level.LevelName.IsNotEmpty())
		snapshot.MapName = level.LevelName;
	else
		snapshot.MapName = "unknown";
	snapshot.GameName = GameStartupInfo.Name.IsNotEmpty() ? GameStartupInfo.Name : FString(GAMENAME);
	snapshot.SessionState = ConnectFlowName(NetConnectFlowState);
	snapshot.Version = GetVersionString();
	snapshot.GitHash = GetGitHash();
	const int connectedClients = CountConnectedClients();
	const int visibleMaxClients = I_GetVisibleMaxClients();
	const int advertisedMaxClients = sv_maxplayers > 0 ? clamp<int>(sv_maxplayers, connectedClients, visibleMaxClients) : visibleMaxClients;
	snapshot.MaxPlayers = uint8_t(clamp<int>(advertisedMaxClients, 0, UINT8_MAX));
	// Keep this aligned with serialized player rows. During setup / late-join
	// sync, also expose connected client slots so launcher query consumers can
	// observe pending joins.
	snapshot.PlayerCount = 0u;
	snapshot.Skill = uint8_t(clamp<int>(gameskill, 0, UINT8_MAX));
	snapshot.Deathmatch = deathmatch != 0;
	snapshot.Teamplay = teamplay;
	snapshot.GameMode = uint8_t(clamp<int>(sv_gametype, 0, UINT8_MAX));
	snapshot.GameModeName = ServerGameModeName(snapshot.GameMode, snapshot.Deathmatch, snapshot.Teamplay);
	snapshot.InvasionState = uint8_t(Net_GetInvasionState());
	snapshot.InvasionStateTics = uint16_t(clamp<int>(Net_GetInvasionStateTics(), 0, UINT16_MAX));
	const char* invasionStateName = Net_GetInvasionStateName();
	if (invasionStateName != nullptr && invasionStateName[0] != 0)
		snapshot.InvasionStateName = invasionStateName;
	snapshot.InvasionWave = uint16_t(clamp<int>(Net_GetInvasionWave(), 0, UINT16_MAX));
	snapshot.InvasionMaxWaves = uint16_t(clamp<int>(Net_GetInvasionMaxWaves(), 0, UINT16_MAX));
	snapshot.InvasionWaveBudget = uint16_t(clamp<int>(Net_GetInvasionWaveBudget(), 0, UINT16_MAX));
	snapshot.InvasionWaveSpawned = uint16_t(clamp<int>(Net_GetInvasionWaveSpawned(), 0, UINT16_MAX));
	snapshot.InvasionWaveCleared = uint16_t(clamp<int>(Net_GetInvasionWaveCleared(), 0, UINT16_MAX));
	snapshot.InvasionWaveFlags = Net_IsInvasionBossWave() ? 1u : 0u;
	snapshot.InvasionSpawnSpotCount = uint16_t(clamp<int>(Net_GetInvasionSpawnSpotCount(), 0, UINT16_MAX));
	snapshot.InvasionSpawnActiveSpotCount = uint16_t(clamp<int>(Net_GetInvasionActiveSpawnSpotCount(), 0, UINT16_MAX));
	snapshot.InvasionSpawnPlanBudget = uint16_t(clamp<int>(Net_GetInvasionSpawnPlanBudget(), 0, UINT16_MAX));
	snapshot.InvasionSpawnActiveTag = uint16_t(clamp<int>(Net_GetInvasionSpawnActiveTag(), 0, UINT16_MAX));
	snapshot.InvasionActiveMonsters = uint16_t(clamp<int>(Net_GetInvasionActiveMonsterCount(), 0, UINT16_MAX));
	const uint8_t fallbackSource = uint8_t(clamp<int>(Net_GetInvasionSpawnFallbackSource(), 0, 7));
	uint8_t spawnFlags = 0u;
	if (Net_IsInvasionSpawnUsingFallback())
		spawnFlags |= INVSPAWNQF_FALLBACK;
	spawnFlags |= uint8_t((fallbackSource << INVSPAWNQF_SOURCE_SHIFT) & INVSPAWNQF_SOURCE_MASK);
	snapshot.InvasionSpawnFlags = spawnFlags;
	if (snapshot.GameMode == 4 && snapshot.InvasionStateName.IsNotEmpty())
		snapshot.SessionState.AppendFormat(" | invasion-%s", snapshot.InvasionStateName.GetChars());
	snapshot.FragLimit = fraglimit > 0 ? uint16_t(clamp<int>(fraglimit, 0, UINT16_MAX)) : 0u;
	if (timelimit > 0.f)
	{
		const int timeleft = (int)(timelimit - level.time / (TICRATE * 60));
		snapshot.TimeLeft = uint16_t(max(timeleft, 0));
	}

	snapshot.Players.Reserve(snapshot.PlayerCount);
	const int firstClient = I_GetFirstPlayableClientSlot();
	for (int i = firstClient; i < MaxClients; ++i)
	{
		const bool connectedSetupSlot = Connected[i].Status != CSTAT_NONE;
		if (!playeringame[i] && !connectedSetupSlot)
			continue;

		FServerQueryPlayer player = {};
		player.Name = players[i].userinfo.GetName(0u);
		player.Ping = uint16_t(clamp<unsigned int>(ClientStates[i].AverageLatency, 0u, UINT16_MAX));
		player.Frags = int16_t(clamp<int>(players[i].fragcount, INT16_MIN, INT16_MAX));
		player.Kills = int16_t(clamp<int>(players[i].killcount, INT16_MIN, INT16_MAX));
		player.Deaths = 0;
		snapshot.Players.Push(player);
	}
	snapshot.PlayerCount = uint8_t(clamp<size_t>(snapshot.Players.Size(), size_t(0), size_t(UINT8_MAX)));

	return snapshot;
}

static const FServerQuerySnapshot& GetCachedServerQuerySnapshot();

} // namespace

void I_GetLocalServerSnapshot(FServerQuerySnapshot& snapshot)
{
	snapshot = GetCachedServerQuerySnapshot();
}

namespace
{
struct FQueryWriter
{
	std::array<uint8_t, MAX_MSGLEN> Buffer = {};
	size_t Offset = 0u;

	bool WriteByte(uint8_t value)
	{
		if (Offset + 1u > Buffer.size())
			return false;
		Buffer[Offset++] = value;
		return true;
	}

	bool WriteShort(uint16_t value)
	{
		if (Offset + 2u > Buffer.size())
			return false;
		Buffer[Offset++] = uint8_t(value >> 8);
		Buffer[Offset++] = uint8_t(value);
		return true;
	}

	bool WriteLong(uint32_t value)
	{
		if (Offset + 4u > Buffer.size())
			return false;
		Buffer[Offset++] = uint8_t(value >> 24);
		Buffer[Offset++] = uint8_t(value >> 16);
		Buffer[Offset++] = uint8_t(value >> 8);
		Buffer[Offset++] = uint8_t(value);
		return true;
	}

	bool WriteString(const char* value)
	{
		const size_t len = strlen(value) + 1u;
		if (Offset + len > Buffer.size())
			return false;
		memcpy(&Buffer[Offset], value, len);
		Offset += len;
		return true;
	}
};

static const FServerQuerySnapshot& GetCachedServerQuerySnapshot()
{
	static FServerQuerySnapshot cachedSnapshot;
	static uint64_t lastSnapshotTime = 0;
	static std::mutex snapshotMutex;

	const uint64_t now = I_msTime();

	std::lock_guard<std::mutex> lock(snapshotMutex);
	if (now - lastSnapshotTime > QuerySnapshotCacheIntervalMs || lastSnapshotTime == 0)
	{
		cachedSnapshot = BuildServerQuerySnapshot();
		lastSnapshotTime = now;
	}
	return cachedSnapshot;
}

static bool SendLauncherInfo(const sockaddr_in& to, const uint8_t* request, int msgSize)
{
	FQueryWriter writer = {};
	// Take a value copy of the cached snapshot. GetCachedServerQuerySnapshot()
	// releases its internal mutex when it returns, so holding a reference to
	// the static cache while we encode would race with any other thread that
	// triggers a refresh (the cache strings are FString members that get
	// re-assigned). The copy is cheap and side-steps the lifetime question.
	const FServerQuerySnapshot snapshot = GetCachedServerQuerySnapshot();

	if (!writer.WriteLong(uint32_t(MSG_CHALLENGE)) ||
	    !writer.WriteLong(uint32_t(I_msTime() & 0xffffffffu)))
	{
		return false;
	}

	if (msgSize >= 8)
	{
		if (!writer.WriteLong(ReadBE32(request + 4u)))
			return false;
	}

	if (!writer.WriteString(snapshot.HostName.GetChars()) ||
	    !writer.WriteByte(snapshot.PlayerCount) ||
	    !writer.WriteByte(snapshot.MaxPlayers) ||
	    !writer.WriteString(snapshot.MapName.GetChars()) ||
	    !writer.WriteString(snapshot.SessionState.GetChars()) ||
	    !writer.WriteByte(snapshot.Deathmatch ? 1u : 0u) ||
	    !writer.WriteByte(snapshot.Skill) ||
	    !writer.WriteByte(snapshot.Teamplay ? 1u : 0u) ||
	    !writer.WriteShort(snapshot.TimeLeft) ||
	    !writer.WriteShort(snapshot.FragLimit) ||
	    !writer.WriteString(snapshot.Version.GetChars()) ||
	    !writer.WriteString(snapshot.GitHash.GetChars()) ||
	    !writer.WriteByte(uint8_t(snapshot.Players.Size())))
	{
		return false;
	}

	for (const auto& player : snapshot.Players)
	{
		if (!writer.WriteString(player.Name.GetChars()) ||
		    !writer.WriteShort(player.Ping) ||
		    !writer.WriteShort(uint16_t(player.Frags)) ||
		    !writer.WriteShort(uint16_t(player.Kills)) ||
		    !writer.WriteShort(uint16_t(player.Deaths)))
		{
			return false;
		}
	}

	// Keep the legacy query packet stable and append new mode metadata at the end.
	if (!writer.WriteString(snapshot.GameName.GetChars()) ||
	    !writer.WriteByte(snapshot.GameMode) ||
	    !writer.WriteString(snapshot.GameModeName.GetChars()) ||
	    !writer.WriteByte(snapshot.InvasionState) ||
	    !writer.WriteShort(snapshot.InvasionStateTics) ||
	    !writer.WriteString(snapshot.InvasionStateName.GetChars()) ||
	    !writer.WriteShort(snapshot.InvasionWave) ||
	    !writer.WriteShort(snapshot.InvasionMaxWaves) ||
	    !writer.WriteShort(snapshot.InvasionWaveBudget) ||
	    !writer.WriteShort(snapshot.InvasionWaveSpawned) ||
	    !writer.WriteShort(snapshot.InvasionWaveCleared) ||
	    !writer.WriteByte(snapshot.InvasionWaveFlags) ||
	    !writer.WriteShort(snapshot.InvasionSpawnSpotCount) ||
	    !writer.WriteShort(snapshot.InvasionSpawnActiveSpotCount) ||
	    !writer.WriteShort(snapshot.InvasionSpawnPlanBudget) ||
	    !writer.WriteShort(snapshot.InvasionSpawnActiveTag) ||
	    !writer.WriteByte(snapshot.InvasionSpawnFlags) ||
	    !writer.WriteShort(snapshot.InvasionActiveMonsters))
		return false;

	if (sendto(MySocket, reinterpret_cast<const char*>(writer.Buffer.data()), static_cast<int>(writer.Offset), 0,
	           reinterpret_cast<const sockaddr*>(&to), sizeof(to)) == SOCKET_ERROR)
	{
		Printf("Failed to send launcher response: %s\n", neterror());
		return false;
	}
	return true;
}

static bool WriteSessionToken(size_t offset, uint32_t token)
{
	// Buffer-overflow check: the session token is written into `NetBuffer`,
	// whose capacity is `MAX_MSGLEN`, not `MaxTransmitSize` (the wire-side
	// transmit cap, which is smaller). Wire-size enforcement happens in
	// `SendPacket`. Using the wrong ceiling here would have rejected valid
	// in-buffer writes on any future packet layout that grows past
	// `MaxTransmitSize` before compression/transmission.
	if (offset + 4u > MAX_MSGLEN)
		return false;

	WriteBE32(&NetBuffer[offset], token);
	return true;
}

static uint32_t ReadSessionToken(const uint8_t* data, size_t offset)
{
	return ReadBE32(&data[offset]);
}

static bool TryHandleServerQuery(const sockaddr_in& from, const uint8_t* request, int msgSize)
{
	if (msgSize < 4)
		return false;

	const uint32_t challenge = ReadBE32(request);
	if (challenge == uint32_t(LAUNCHER_CHALLENGE) || challenge == uint32_t(PROTO_CHALLENGE))
	{
		DebugTrace::Markf("net", "server query challenge=%u players=%d", challenge, CountConnectedClients());
		return SendLauncherInfo(from, request, msgSize);
	}

	if (((challenge >> 20) & 0x0FFFu) == ODAMEX_QUERY_TAG_ID)
	{
		DebugTrace::Markf("net", "server query tag=%u players=%d", challenge, CountConnectedClients());
		return SendLauncherInfo(from, request, msgSize);
	}

	return false;
}
}

CUSTOM_CVAR(String, net_password, "", CVAR_IGNORE)
{
	if (strlen(self) + 1 > MaxPasswordSize)
	{
		self = "";
		Printf(TEXTCOLOR_RED "Password cannot be greater than 255 characters\n");
	}
}

// Game-specific API
size_t Net_SetEngineInfo(uint8_t*& stream);
FVerificationError Net_VerifyEngine(uint8_t*& stream, size_t& offset, size_t packetLength);
void Net_SetupUserInfo();
const char* Net_GetClientName(int client, unsigned int charLimit);
void Net_SetUserInfo(int client, TArrayView<uint8_t>& stream);
void Net_ReadUserInfo(int client, TArrayView<uint8_t>& stream);
void Net_ReadMapLoadInfo(TArrayView<uint8_t>& stream);
void Net_SetMapLoadInfo(TArrayView<uint8_t>& stream);
void Net_ReadServerInfo(TArrayView<uint8_t>& stream);
void Net_SetServerInfo(TArrayView<uint8_t>& stream);

// Internal setup/connection helpers shared across startup and active-match
// dedicated late-join admission.
static int CountConnectedPlayers();
static bool TryProcessSetupConnectPacket(const sockaddr_in& from, bool hasPassword, bool rejectForInProgress, bool runtimeJoin, int* connectedPlayers);
static void RejectConnection(const sockaddr_in& to, ENetConnectType reason);
static void SendVerificationError(const sockaddr_in& to, const FVerificationError& error);
static void AddClientConnection(const sockaddr_in& from, int client, const FHCDEConnectInfo& connectInfo, bool runtimeJoin);
static void DriveRuntimeSetupStateForClient(int client, int connectedPlayers);

static SOCKET CreateUDPSocket()
{
	SOCKET s = socket(PF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (s == INVALID_SOCKET)
		I_FatalError("Couldn't create socket: %s", neterror());

#ifdef _WIN32
	// Disable Windows' "UDP connection reset" behavior on the game socket.
	//
	// By default a Windows UDP socket makes recvfrom fail with WSAECONNRESET
	// whenever a *previous* sendto to a peer provoked an ICMP "port
	// unreachable" reply. UDP is connectionless, so this is not a real
	// disconnect -- but it routinely fires during a localhost reconnect: when
	// one client closes and a new client (or the same one) re-binds, in-flight
	// packets to the just-closed endpoint bounce an ICMP error that the OS then
	// reports against the shared socket on the next recvfrom. The engine's
	// GetPacket reset handler interprets a reset attributed to the authority
	// slot as a fatal "Authority unexpectedly disconnected" and aborts, which
	// is the exact crash seen when reconnecting to a localhost server.
	//
	// Clearing SIO_UDP_CONNRESET tells Winsock to swallow those ICMP errors so
	// recvfrom keeps returning WSAEWOULDBLOCK instead, matching how other Doom
	// source ports keep UDP sessions alive through transient peer churn. The
	// ioctl is available on Vista+ and is best-effort: if it ever fails we fall
	// back to the previous behavior rather than refusing to create the socket.
#ifndef SIO_UDP_CONNRESET
#define SIO_UDP_CONNRESET _WSAIOW(IOC_VENDOR, 12)
#endif
	BOOL reportUdpReset = FALSE;
	DWORD ioctlBytesReturned = 0;
	if (WSAIoctl(s, SIO_UDP_CONNRESET, &reportUdpReset, sizeof(reportUdpReset),
		nullptr, 0, &ioctlBytesReturned, nullptr, nullptr) == SOCKET_ERROR)
	{
		DebugTrace::Warningf("net", "SIO_UDP_CONNRESET disable failed err=%d (continuing)", WSAGetLastError());
	}
#endif

	return s;
}

static void BindToLocalPort(SOCKET s, u_short port)
{
	sockaddr_in address = {};
	address.sin_family = AF_INET;
	address.sin_addr.s_addr = INADDR_ANY;
	address.sin_port = htons(port);

	int v = bind(s, (sockaddr *)&address, sizeof(address));
	if (v == SOCKET_ERROR)
		I_FatalError("Couldn't bind to port: %s", neterror());
}

static bool TryBuildAddress(sockaddr_in& address, const char* addrName, FString* error)
{
	FString target = {};
	u_short port = GamePort;
	const char* portName = strchr(addrName, ':');
	if (portName != nullptr)
	{
		target = FString(addrName, (int)(portName - addrName));
		u_short portConversion = (u_short)atoi(portName + 1);
		if (!portConversion)
		{
			if (error != nullptr)
				error->Format("Malformed port: %s", portName + 1);
			return false;
		}
		else
			port = portConversion;
	}
	else
	{
		target = addrName;
	}

	addrinfo hints = {};
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_DGRAM;
	hints.ai_protocol = IPPROTO_UDP;

	addrinfo* result = nullptr;
	if (getaddrinfo(target.GetChars(), nullptr, &hints, &result) != 0)
	{
		if (error != nullptr)
			error->Format("getaddrinfo: Couldn't find %s (%s)", target.GetChars(), neterror());
		return false;
	}

	address = *reinterpret_cast<sockaddr_in*>(result->ai_addr);
	address.sin_port = htons(port);
	freeaddrinfo(result);

	return true;
}

static void BuildAddress(sockaddr_in& address, const char* addrName)
{
	FString error;
	if (!TryBuildAddress(address, addrName, &error))
		I_FatalError("%s", error.GetChars());
}

static bool ReadQueryByte(const uint8_t* data, size_t& offset, size_t limit, uint8_t& value)
{
	if (offset + 1u > limit)
		return false;

	value = data[offset++];
	return true;
}

static bool ReadQueryShort(const uint8_t* data, size_t& offset, size_t limit, uint16_t& value)
{
	if (offset + 2u > limit)
		return false;

	value = (uint16_t(data[offset]) << 8) | uint16_t(data[offset + 1u]);
	offset += 2u;
	return true;
}

static bool ReadQueryString(const uint8_t* data, size_t& offset, size_t limit, FString& value)
{
	if (offset >= limit)
		return false;

	const size_t start = offset;
	while (offset < limit && data[offset] != 0u)
		++offset;

	if (offset >= limit)
		return false;

	value = FString(reinterpret_cast<const char*>(&data[start]));
	++offset;
	return true;
}

static bool TryReadServerQuerySnapshot(const uint8_t* data, size_t length, FServerQuerySnapshot& snapshot, FString* error)
{
	if (length < 8u)
	{
		if (error != nullptr)
			error->Format("Query reply was too short");
		return false;
	}

	const uint32_t challenge = ReadBE32(data);
	if (challenge != uint32_t(MSG_CHALLENGE))
	{
		if (error != nullptr)
			error->Format("Unexpected query reply header: %u", challenge);
		return false;
	}

	size_t offset = 8u;
	uint8_t deathmatch = 0u;
	uint8_t skill = 0u;
	uint8_t teamplay = 0u;
	if (!ReadQueryString(data, offset, length, snapshot.HostName) ||
	    !ReadQueryByte(data, offset, length, snapshot.PlayerCount) ||
	    !ReadQueryByte(data, offset, length, snapshot.MaxPlayers) ||
	    !ReadQueryString(data, offset, length, snapshot.MapName) ||
	    !ReadQueryString(data, offset, length, snapshot.SessionState) ||
	    !ReadQueryByte(data, offset, length, deathmatch) ||
	    !ReadQueryByte(data, offset, length, skill) ||
	    !ReadQueryByte(data, offset, length, teamplay) ||
	    !ReadQueryShort(data, offset, length, snapshot.TimeLeft) ||
	    !ReadQueryShort(data, offset, length, snapshot.FragLimit) ||
	    !ReadQueryString(data, offset, length, snapshot.Version) ||
	    !ReadQueryString(data, offset, length, snapshot.GitHash))
	{
		if (error != nullptr)
			error->Format("Query reply was truncated");
		return false;
	}
	snapshot.Deathmatch = deathmatch != 0u;
	snapshot.Skill = skill;
	snapshot.Teamplay = teamplay != 0u;
	snapshot.GameMode = 0u;
	snapshot.GameModeName = ServerGameModeName(snapshot.GameMode, snapshot.Deathmatch, snapshot.Teamplay);
	snapshot.InvasionState = uint8_t(INVS_DISABLED);
	snapshot.InvasionStateTics = 0u;
	snapshot.InvasionStateName = ServerInvasionStateName(snapshot.InvasionState);
	snapshot.InvasionWave = 0u;
	snapshot.InvasionMaxWaves = 0u;
	snapshot.InvasionWaveBudget = 0u;
	snapshot.InvasionWaveSpawned = 0u;
	snapshot.InvasionWaveCleared = 0u;
	snapshot.InvasionWaveFlags = 0u;
	snapshot.InvasionSpawnSpotCount = 0u;
	snapshot.InvasionSpawnActiveSpotCount = 0u;
	snapshot.InvasionSpawnPlanBudget = 0u;
	snapshot.InvasionSpawnActiveTag = 0u;
	snapshot.InvasionSpawnFlags = 0u;
	snapshot.InvasionActiveMonsters = 0u;

	uint8_t playerCount = 0u;
	if (!ReadQueryByte(data, offset, length, playerCount))
	{
		if (error != nullptr)
			error->Format("Query reply was truncated");
		return false;
	}
	snapshot.PlayerCount = playerCount;

	snapshot.Players.Clear();
	snapshot.Players.Reserve(playerCount);
	for (uint8_t i = 0u; i < playerCount; ++i)
	{
		FServerQueryPlayer player = {};
		uint16_t ping = 0u;
		uint16_t frags = 0u;
		uint16_t kills = 0u;
		uint16_t deaths = 0u;
		if (!ReadQueryString(data, offset, length, player.Name) ||
		    !ReadQueryShort(data, offset, length, ping) ||
		    !ReadQueryShort(data, offset, length, frags) ||
		    !ReadQueryShort(data, offset, length, kills) ||
		    !ReadQueryShort(data, offset, length, deaths))
		{
			if (error != nullptr)
				error->Format("Query player list was truncated");
			return false;
		}

		player.Ping = ping;
		player.Frags = int16_t(frags);
		player.Kills = int16_t(kills);
		player.Deaths = int16_t(deaths);
		snapshot.Players.Push(player);
	}

	if (offset < length && !ReadQueryString(data, offset, length, snapshot.GameName))
	{
		if (error != nullptr)
			error->Format("Query game name was truncated");
		return false;
	}

	if (offset < length)
	{
		uint8_t gameMode = 0u;
		FString gameModeName = {};
		if (!ReadQueryByte(data, offset, length, gameMode) ||
		    !ReadQueryString(data, offset, length, gameModeName))
		{
			if (error != nullptr)
				error->Format("Query game mode was truncated");
			return false;
		}

		snapshot.GameMode = gameMode;
		snapshot.GameModeName = gameModeName.IsNotEmpty() ? gameModeName : FString(ServerGameModeName(snapshot.GameMode, snapshot.Deathmatch, snapshot.Teamplay));
	}

	if (offset < length)
	{
		uint8_t invasionState = uint8_t(INVS_DISABLED);
		uint16_t invasionStateTics = 0u;
		FString invasionStateName = {};
		if (!ReadQueryByte(data, offset, length, invasionState) ||
		    !ReadQueryShort(data, offset, length, invasionStateTics) ||
		    !ReadQueryString(data, offset, length, invasionStateName))
		{
			if (error != nullptr)
				error->Format("Query invasion state was truncated");
			return false;
		}

		snapshot.InvasionState = invasionState;
		snapshot.InvasionStateTics = invasionStateTics;
		snapshot.InvasionStateName = invasionStateName.IsNotEmpty() ? invasionStateName : FString(ServerInvasionStateName(invasionState));
	}

	if (offset < length)
	{
		uint16_t invasionWave = 0u;
		uint16_t invasionMaxWaves = 0u;
		uint16_t invasionWaveBudget = 0u;
		uint16_t invasionWaveSpawned = 0u;
		uint16_t invasionWaveCleared = 0u;
		uint8_t invasionWaveFlags = 0u;
		if (!ReadQueryShort(data, offset, length, invasionWave) ||
		    !ReadQueryShort(data, offset, length, invasionMaxWaves) ||
		    !ReadQueryShort(data, offset, length, invasionWaveBudget) ||
		    !ReadQueryShort(data, offset, length, invasionWaveSpawned) ||
		    !ReadQueryShort(data, offset, length, invasionWaveCleared) ||
		    !ReadQueryByte(data, offset, length, invasionWaveFlags))
		{
			if (error != nullptr)
				error->Format("Query invasion wave metadata was truncated");
			return false;
		}

		snapshot.InvasionWave = invasionWave;
		snapshot.InvasionMaxWaves = invasionMaxWaves;
		snapshot.InvasionWaveBudget = invasionWaveBudget;
		snapshot.InvasionWaveSpawned = invasionWaveSpawned;
		snapshot.InvasionWaveCleared = invasionWaveCleared;
		snapshot.InvasionWaveFlags = invasionWaveFlags;
	}

	if (offset < length)
	{
		uint16_t invasionSpawnSpotCount = 0u;
		uint16_t invasionSpawnActiveSpotCount = 0u;
		uint16_t invasionSpawnPlanBudget = 0u;
		uint16_t invasionSpawnActiveTag = 0u;
		uint8_t invasionSpawnFlags = 0u;
		if (!ReadQueryShort(data, offset, length, invasionSpawnSpotCount) ||
		    !ReadQueryShort(data, offset, length, invasionSpawnActiveSpotCount) ||
		    !ReadQueryShort(data, offset, length, invasionSpawnPlanBudget) ||
		    !ReadQueryShort(data, offset, length, invasionSpawnActiveTag) ||
		    !ReadQueryByte(data, offset, length, invasionSpawnFlags))
		{
			if (error != nullptr)
				error->Format("Query invasion spawn metadata was truncated");
			return false;
		}

		snapshot.InvasionSpawnSpotCount = invasionSpawnSpotCount;
		snapshot.InvasionSpawnActiveSpotCount = invasionSpawnActiveSpotCount;
		snapshot.InvasionSpawnPlanBudget = invasionSpawnPlanBudget;
		snapshot.InvasionSpawnActiveTag = invasionSpawnActiveTag;
		snapshot.InvasionSpawnFlags = invasionSpawnFlags;
	}

	if (offset < length)
	{
		uint16_t invasionActiveMonsters = 0u;
		if (!ReadQueryShort(data, offset, length, invasionActiveMonsters))
		{
			if (error != nullptr)
				error->Format("Query invasion active-monster metadata was truncated");
			return false;
		}
		snapshot.InvasionActiveMonsters = invasionActiveMonsters;
	}

	return true;
}

bool I_QueryServerInfo(const char* addrName, FServerQuerySnapshot& snapshot, FString* error)
{
	snapshot = {};
	if (error != nullptr)
		*error = "";

#ifdef _WIN32
	static std::once_flag wsaInit;
	static int wsaInitResult = 0;
	std::call_once(wsaInit, []() {
		WSADATA data;
		wsaInitResult = WSAStartup(MAKEWORD(2, 2), &data);
	});
	if (wsaInitResult != 0)
	{
		if (error != nullptr)
			error->Format("Couldn't initialize Windows sockets: %s", neterror());
		return false;
	}
#endif

	sockaddr_in address = {};
	if (!TryBuildAddress(address, addrName, error))
		return false;

	bool success = false;
	SOCKET socketHandle = INVALID_SOCKET;
	do
	{
		socketHandle = socket(PF_INET, SOCK_DGRAM, IPPROTO_UDP);
		if (socketHandle == INVALID_SOCKET)
		{
			if (error != nullptr)
				error->Format("Couldn't create socket: %s", neterror());
			break;
		}

		std::array<uint8_t, 4> request = {};
		WriteBE32(request.data(), uint32_t(LAUNCHER_CHALLENGE));
		if (sendto(socketHandle, reinterpret_cast<const char*>(request.data()), static_cast<int>(request.size()), 0,
		           reinterpret_cast<const sockaddr*>(&address), sizeof(address)) == SOCKET_ERROR)
		{
			if (error != nullptr)
				error->Format("Failed to send query: %s", neterror());
			break;
		}

		fd_set readset;
		FD_ZERO(&readset);
		FD_SET(socketHandle, &readset);
		timeval timeout = {};
		timeout.tv_sec = 0;
		timeout.tv_usec = 500000; // 500ms timeout
		const int selectResult = select(static_cast<int>(socketHandle + 1), &readset, nullptr, nullptr, &timeout);
		if (selectResult <= 0)
		{
			if (error != nullptr)
				error->Format(selectResult == 0 ? "Query timed out" : "Query wait failed: %s", neterror());
			break;
		}

		std::array<uint8_t, MAX_MSGLEN> reply = {};
		sockaddr_in from = {};
		socklen_t fromSize = sizeof(from);
		const int replySize = recvfrom(socketHandle, reinterpret_cast<char*>(reply.data()), static_cast<int>(reply.size()), 0,
		                               reinterpret_cast<sockaddr*>(&from), &fromSize);
		if (replySize <= 0)
		{
			if (error != nullptr)
				error->Format("Failed to read query reply: %s", neterror());
			break;
		}

		success = TryReadServerQuerySnapshot(reply.data(), static_cast<size_t>(replySize), snapshot, error);
	} while (false);

	if (socketHandle != INVALID_SOCKET)
		closesocket(socketHandle);

	return success;
}

static void StartNetwork(bool autoPort)
{
#ifdef _WIN32
	static std::once_flag wsaInit;
	std::call_once(wsaInit, []() {
		WSADATA data;
		if (WSAStartup(MAKEWORD(2, 2), &data))
			I_FatalError("Couldn't initialize Windows sockets");
	});
#endif

	netgame = true;
	multiplayer = true;
	MySocket = CreateUDPSocket();
	BindToLocalPort(MySocket, autoPort ? 0 : GamePort);

	u_long trueVal = 1u;
#ifndef __sun
	ioctlsocket(MySocket, FIONBIO, &trueVal);
#else
	fcntl(MySocket, F_SETFL, trueVal | O_NONBLOCK);
#endif
}

void CloseNetwork()
{
	DebugTrace::Mark("net", "close network");
	SV_ShutdownMasters();
	if (MySocket != INVALID_SOCKET)
	{
		closesocket(MySocket);
		MySocket = INVALID_SOCKET;
		netgame = false;
	}
	// Reset per-session admission/join state so a later host/join in the same
	// process cannot inherit a stale "game started" or dedicated late-join
	// handshake state. These are re-seeded by HostGame()/JoinGame(), but
	// clearing them here keeps a torn-down session from leaking flags into a
	// subsequent non-dedicated game.
	bGameStarted = false;
	DedicatedJoinMode = false;
	DedicatedLateJoinRetryAttempted = false;
	DedicatedLateJoinRetryPendingSend = false;
	GuestHCDELastSetupProgressTime = 0u;
	GuestHCDELastSetupRxSeq = 0u;
	DedicatedServerAbortRequested = false;
#ifdef _WIN32
	if (!DebugServer::RuntimeEvents::IsDebugServerRunning()){
		WSACleanup();
	}
#endif
}

static void GenerateGameID()
{
	const uint64_t val = GameIDGen.GenRand64();
	memcpy(GameID, &val, sizeof(val));
}

// Print a network-related message to the console. This doesn't print to the window so should
// not be used for that and is mainly for logging.
static void I_NetLog(const char* text, ...)
{
	// todo: use better abstraction once everything is migrated to in-game start screens.
#if defined _WIN32 || defined __APPLE__
	va_list ap;
	va_start(ap, text);
	VPrintf(PRINT_HIGH, text, ap);
	Printf("\n");
	va_end(ap);
#else
	FString str;
	va_list argptr;

	va_start(argptr, text);
	str.VFormat(text, argptr);
	va_end(argptr);
	fprintf(stderr, "\r%-40s\n", str.GetChars());
#endif
}

static void SetConnectFlow(ENetConnectFlow flow)
{
	if (NetConnectFlowState != flow)
	{
		NetConnectFlowState = flow;
		DebugTrace::Markf("net", "connect flow=%s", ConnectFlowName(flow));
	}
}

// Gracefully closes the net window so that any error messaging can be properly displayed.
static void I_NetError(const char* error)
{
	const char* message = error != nullptr ? error : "unknown network error";
	I_NetLog("Fatal network error: %s", message);
	DebugTrace::Warningf("net", "fatal network error: %s", message);
	if (!DedicatedServerMode && !SilentNetStartMode)
		NetStartWindow::NetClose();
	I_FatalError("%s", message);
}

static void I_NetInit(const char* msg, bool host)
{
	Printf("%s:: %s\n", DedicatedServerMode ? "NetServer" : "NetSession", msg);
#if defined(_WIN32) && defined(HCDE_DEDICATED_SERVER)
	if (DedicatedServerMode)
	{
		I_SetDedicatedServerConsoleStatus(msg);
	}
#endif
	if (!DedicatedServerMode && !SilentNetStartMode)
	{
		HCDE_ServerMode_GuardClientSubsystem("network session window");
		NetStartWindow::NetInit(msg, host);
	}
}

// Updates the general status of the pregame session flow. Interactive listen-host
// startup still uses NetStartWindow, while dedicated-server and silent launcher
// flows route the same messages to stdout / the dedicated status window. If this
// path is ever moved to a main-menu presenter, keep those two non-interactive
// sinks wired directly so headless startup remains UI-free.
static void I_NetMessage(const char* msg)
{
	Printf("%s:: %s\n", DedicatedServerMode ? "NetServer" : "NetSession", msg);
#if defined(_WIN32) && defined(HCDE_DEDICATED_SERVER)
	if (DedicatedServerMode)
	{
		I_SetDedicatedServerConsoleStatus(msg);
	}
#endif
	if (!DedicatedServerMode && !SilentNetStartMode)
	{
		HCDE_ServerMode_GuardClientSubsystem("network session message");
		NetStartWindow::NetMessage(msg);
	}
}

extern void HCDERconPollListener();

// Listen for incoming connections while the pregame server flow is active. This
// intentionally blocks engine startup until the connect/setup handshake reaches
// a terminal state:
//   * dedicated / silent launcher modes pump the callback in a small sleep loop
//     and never open NetStartWindow;
//   * interactive listen-host mode delegates to NetStartWindow::NetLoop so the
//     UI can keep processing messages while the same callback advances.
static bool I_NetLoop(bool (*loopCallback)(void*), void* data)
{
	if (DedicatedServerMode || SilentNetStartMode)
	{
		for (;;)
		{
			if (DedicatedServerMode)
			{
				HCDERconPollListener();
			}
			if (loopCallback(data))
				break;
#if defined(_WIN32) && defined(HCDE_DEDICATED_SERVER)
			if (DedicatedServerMode)
			{
				I_PumpDedicatedServerConsoleWindow();
				if (DedicatedServerAbortRequested)
					return false;
			}
#endif
			Sleep(1);
		}
#if defined(_WIN32) && defined(HCDE_DEDICATED_SERVER)
		if (DedicatedServerMode)
		{
			I_PumpDedicatedServerConsoleWindow();
		}
#endif
		return !DedicatedServerAbortRequested;
	}
	return NetStartWindow::NetLoop(loopCallback, data);
}

// A new client completed setup connect; update launcher UI and log the session.
// Player mobj/slot assignment happens later in the netgame start path.
static void I_NetClientConnected(int client, unsigned int charLimit = 0u)
{
	if (I_IsServerReservedSlot(client))
	{
		Printf("%s:: Dedicated server authority slot ready.\n", DedicatedServerMode ? "NetServer" : "NetSession");
		return;
	}

	Printf("%s:: Client '%s' connected.\n", DedicatedServerMode ? "NetServer" : "NetSession", Net_GetClientName(client, 0u));

	const char* name = Net_GetClientName(client, charLimit);
	unsigned int flags = CFL_NONE;
	if (I_IsHCDEServiceAuthoritySlot(client) && !I_IsServerReservedSlot(client))
		flags |= CFL_HOST;
	if (client == consoleplayer)
		flags |= CFL_CONSOLEPLAYER;

	NetStartWindow::NetConnect(client, name, flags, Connected[client].Status);
}

// A client changed ready state.
static void I_NetClientUpdated(int client)
{
	NetStartWindow::NetUpdate(client, Connected[client].Status);
}

static void I_NetClientDisconnected(int client, const char* reason = nullptr)
{
	const bool hasReason = reason != nullptr && reason[0] != '\0';
	Printf("%s:: Client '%s' disconnected%s%s%s.\n",
		DedicatedServerMode ? "NetServer" : "NetSession",
		Net_GetClientName(client, 0u),
		hasReason ? " (" : "",
		hasReason ? reason : "",
		hasReason ? ")" : "");
	if (hasReason)
		DebugTrace::Warningf("net", "client disconnected client=%d name=%s reason=%s", client, Net_GetClientName(client, 0u), reason);
	NetStartWindow::NetDisconnect(client);
}

static void I_NetUpdatePlayers(int current, int limit)
{
	if (I_UsesDedicatedServerSlot())
	{
		current = max(current - 1, 0);
		limit = max(limit - 1, 0);
	}
	NetStartWindow::NetProgress(current, limit);
}

static bool I_ShouldStartNetGame()
{
	if (DedicatedServerMode)
		return DedicatedServerStartRequested;
	return NetStartWindow::ShouldStartNet();
}

static void I_GetKickClients(TArray<int>& clients)
{
	clients.Clear();

	int c = -1;
	while ((c = NetStartWindow::GetNetKickClient()) != -1)
		clients.Push(c);
}

static void I_GetBanClients(TArray<int>& clients)
{
	clients.Clear();

	int c = -1;
	while ((c = NetStartWindow::GetNetBanClient()) != -1)
		clients.Push(c);
}

void I_NetDone()
{
	NetStartWindow::NetDone();
}

void I_ClearClient(size_t client)
{
	Connected[client].Clear();
}

static int FindClient(const sockaddr_in& address)
{
	int i = 0;
	for (; i < MaxClients; ++i)
	{
		if (Connected[i].Status == CSTAT_NONE)
			continue;

		if (address.sin_addr.s_addr == Connected[i].Address.sin_addr.s_addr
			&& address.sin_port == Connected[i].Address.sin_port)
		{
			break;
		}
	}

	return i >= MaxClients ? -1 : i;
}

static void SendPacket(const sockaddr_in& to)
{
	// Huge packets should be sent out as sequences, not as one big packet, otherwise it's prone
	// to high amounts of congestion and reordering needed.
	if (NetBufferLength > MAX_MSGLEN)
		I_FatalError("Netbuffer overflow: Tried to send %lu bytes of data", NetBufferLength);

	assert(!(NetBuffer[0] & NCMD_COMPRESSED));

	uint8_t* dataStart = &TransmitBuffer[4];
	uLong size = MaxTransmitSize - 5u;
	if (NetBufferLength >= MinCompressionSize)
	{
		*dataStart = NetBuffer[0] | NCMD_COMPRESSED;
		const int res = compress2(dataStart + 1, &size, NetBuffer + 1, NetBufferLength - 1u, Z_BEST_SPEED);
		if (res != Z_OK)
			I_Error("Net compression failed (zlib error %d)", res);

		++size;
	}
	else
	{
		memcpy(dataStart, NetBuffer, NetBufferLength);
		size = NetBufferLength;
	}

	if (size + 4 > MaxTransmitSize)
		I_Error("Failed to compress data down to acceptable transmission size");

	// If a connection packet, don't check the game id since they might not have it yet.
	const uint32_t crc = (NetBuffer[0] & NCMD_SETUP) ? CalcCRC32(dataStart, size) : AddCRC32(CalcCRC32(dataStart, size), GameID, std::extent_v<decltype(GameID)>);
	TransmitBuffer[0] = crc >> 24;
	TransmitBuffer[1] = crc >> 16;
	TransmitBuffer[2] = crc >> 8;
	TransmitBuffer[3] = crc;

	const int sendResult = sendto(MySocket, (const char*)TransmitBuffer, size + 4, 0, (const sockaddr*)&to, sizeof(to));
	if (sendResult == SOCKET_ERROR)
	{
		// sendto failures were previously swallowed silently. That made a join
		// that never reached the server impossible to diagnose: the guest would
		// "send" a connect packet every tic, but if the OS rejected each one
		// (no route to host, firewall block, bad/unreachable resolved address)
		// there was zero record of it. Logging the failure with the destination
		// localizes the fault to the local send path; if instead these succeed
		// but the server's inbound trace never fires, the loss is in transit
		// (NAT/hairpin). Errors are rare, so this never floods.
		DebugTrace::Warningf("net", "sendto FAILED dest=%s:%u len=%lu cmd=0x%02x err=%s",
			inet_ntoa(to.sin_addr), unsigned(ntohs(to.sin_port)),
			static_cast<unsigned long>(size + 4u), unsigned(NetBuffer[0]), neterror());
	}
	else if (NetBuffer[0] & NCMD_SETUP)
	{
		// Handshake breadcrumb: confirms the node is actually emitting setup/
		// connect/service packets to the resolved peer address during the
		// pregame handshake. Scoped to NCMD_SETUP so in-game gameplay traffic
		// does not flood the stream once the match is live.
		DebugTrace::Markf("net", "sendto ok dest=%s:%u len=%lu setup-type=%u",
			inet_ntoa(to.sin_addr), unsigned(ntohs(to.sin_port)),
			static_cast<unsigned long>(size + 4u), unsigned(NetBuffer[1]));
	}
	Net_BlackboxRecordPacket(0, RemoteClient, 0u, 0u, 0u, 0u, NetBuffer, NetBufferLength);
}

static bool FlushHCDEReliableServices(const sockaddr_in& to, FConnection& connection, bool force = false)
{
	ClearAckedHCDEReliableServices(connection);
	auto* pending = FindOldestHCDEReliableService(connection);
	if (pending == nullptr)
		return false;

	const uint64_t now = I_msTime();
	if (!force && pending->SendCount > 0u && now - pending->LastSendTime < HCDEServiceResendMS)
		return true;

	if (pending->Packet.Size() < HCDEServiceHeaderSize)
	{
		++HCDEPregameServiceProfile.ServiceQueueMalformed;
		DebugTrace::Markf("net", "dropping malformed retained service %s key=%u seq=%u len=%u", HCDEServiceName(pending->Service), pending->Key, pending->Sequence, pending->Packet.Size());
		pending->Clear();
		return false;
	}

	NetBufferLength = pending->Packet.Size();
	memcpy(NetBuffer, pending->Packet.Data(), NetBufferLength);
	WriteBE32(&NetBuffer[HCDEServiceAckOffset], connection.HCDEServiceRxSeq);
	WriteBE32(&pending->Packet[HCDEServiceAckOffset], connection.HCDEServiceRxSeq);
	SendPacket(to);

	if (pending->SendCount == 0u)
	{
		pending->FirstSendTime = now;
		++HCDEPregameServiceProfile.ServiceQueueSent;
	}
	else
	{
		++HCDEPregameServiceProfile.ServiceQueueRetransmit;
	}
	pending->LastSendTime = now;
	++pending->SendCount;
	DebugTrace::Markf("net", "sent reliable service %s key=%u seq=%u ack=%u count=%u", HCDEServiceName(pending->Service), pending->Key, pending->Sequence, connection.HCDEServiceRxSeq, pending->SendCount);
	return true;
}

static bool BeginReliableHCDEPregameService(EHCDEPregameService service, FConnection& connection, uint8_t key)
{
	if (FindHCDEReliableService(connection, service, key) != nullptr)
	{
		++HCDEPregameServiceProfile.ServiceQueueReused;
		// Throttle to at most once per second per client: this fires for every
		// already-queued service on every tic and every received packet during
		// WAITING, and was a major contributor to the trace-file churn that
		// erased the connect phase. The profiling counter above is unthrottled.
		const uint64_t nowReuseLog = I_msTime();
		if (nowReuseLog - connection.HCDEServiceLastReuseLogTime >= 1000u)
		{
			connection.HCDEServiceLastReuseLogTime = nowReuseLog;
			DebugTrace::Markf("net", "reusing pending reliable service %s key=%u peerAck=%u tx=%u rx=%u",
				HCDEServiceName(service), key, connection.HCDEServicePeerAck,
				connection.HCDEServiceTxSeq, connection.HCDEServiceRxSeq);
		}
		return false;
	}
	if (FindFreeHCDEReliableService(connection) == nullptr)
	{
		++HCDEPregameServiceProfile.ServiceQueueFullAdd;
		auto* oldest = FindOldestHCDEReliableService(connection);
		DebugTrace::Warningf("net", "reliable service queue full while adding %s key=%u oldest=%s oldest-key=%u oldest-seq=%u oldest-sends=%u peerAck=%u tx=%u rx=%u",
			HCDEServiceName(service), key,
			oldest != nullptr ? HCDEServiceName(oldest->Service) : "<none>",
			oldest != nullptr ? oldest->Key : 0u,
			oldest != nullptr ? oldest->Sequence : 0u,
			oldest != nullptr ? oldest->SendCount : 0u,
			connection.HCDEServicePeerAck, connection.HCDEServiceTxSeq, connection.HCDEServiceRxSeq);
		return false;
	}

	BeginHCDEPregameService(service, connection);
	return true;
}

static bool CommitReliableHCDEPregameService(const sockaddr_in& to, FConnection& connection, EHCDEPregameService service, uint8_t key)
{
	auto* pending = FindFreeHCDEReliableService(connection);
	if (pending == nullptr)
	{
		++HCDEPregameServiceProfile.ServiceQueueFullCommit;
		DebugTrace::Markf("net", "reliable service queue full while committing %s key=%u", HCDEServiceName(service), key);
		return false;
	}

	pending->Active = true;
	pending->Service = service;
	pending->Key = key;
	pending->Sequence = ReadBE32(&NetBuffer[HCDEServiceSequenceOffset]);
	pending->FirstSendTime = 0u;
	pending->LastSendTime = 0u;
	pending->SendCount = 0u;
	pending->Packet.Resize(NetBufferLength);
	memcpy(pending->Packet.Data(), NetBuffer, NetBufferLength);

	DebugTrace::Markf("net", "queued reliable service %s key=%u seq=%u len=%zu", HCDEServiceName(service), key, pending->Sequence, NetBufferLength);
	FlushHCDEReliableServices(to, connection);
	return true;
}

static void WriteBE16(uint8_t* data, uint16_t value)
{
	data[0] = uint8_t(value >> 8);
	data[1] = uint8_t(value);
}

static uint16_t ReadBE16(const uint8_t* data)
{
	return uint16_t((uint16_t(data[0]) << 8) | uint16_t(data[1]));
}

static bool QueueHCDERosterService(const sockaddr_in& to, FConnection& connection, int targetClient)
{
	if (!BeginReliableHCDEPregameService(HPS_ROSTER, connection, 0u))
		return false;

	size_t rosterCountOffset = NetBufferLength++;
	uint8_t rosterCount = 0u;
	const size_t addrSize = sizeof(sockaddr_in);
	for (int i = 0; i < MaxClients; ++i)
	{
		if (i == targetClient || Connected[i].Status == CSTAT_NONE || Connected[i].Status < CSTAT_WAITING)
			continue;

		const size_t fixedBytes = 1u + (i > 0 ? addrSize : 0u) + 2u;
		if (NetBufferLength + fixedBytes >= MAX_MSGLEN)
		{
			DebugTrace::Warningf("net", "roster service overflow before slot=%d len=%zu fixed=%zu", i, NetBufferLength, fixedBytes);
			return false;
		}

		NetBuffer[NetBufferLength++] = uint8_t(i);
		if (i > 0)
		{
			memcpy(&NetBuffer[NetBufferLength], &Connected[i].Address, addrSize);
			NetBufferLength += addrSize;
		}

		const size_t infoSizeOffset = NetBufferLength;
		NetBufferLength += 2u;
		const size_t infoStart = NetBufferLength;
		TArrayView<uint8_t> stream = TArrayView(&NetBuffer[NetBufferLength], MAX_MSGLEN - NetBufferLength);
		Net_SetUserInfo(i, stream);
		const size_t infoSize = stream.Data() - &NetBuffer[infoStart];
		if (infoSize > UINT16_MAX)
		{
			DebugTrace::Warningf("net", "roster service userinfo too large slot=%d size=%zu", i, infoSize);
			return false;
		}
		NetBufferLength = infoStart + infoSize;
		WriteBE16(&NetBuffer[infoSizeOffset], uint16_t(infoSize));
		++rosterCount;
	}

	NetBuffer[rosterCountOffset] = rosterCount;
	DebugTrace::Markf("net", "queued authority roster entries=%u target=%d", unsigned(rosterCount), targetClient);
	return CommitReliableHCDEPregameService(to, connection, HPS_ROSTER, 0u);
}

static bool QueueHCDEBootstrapControlService(const sockaddr_in& to, FConnection& connection, EHCDEPregameService service, const char* reason)
{
	if (!BeginReliableHCDEPregameService(service, connection, 0u))
		return false;

	const uint8_t roomId = Net_GetCurrentRoomID();
	NetBuffer[HCDEServiceHeaderSize] = roomId;
	WriteBE32(&NetBuffer[HCDEServiceHeaderSize + 1u], uint32_t(max<int>(gametic, 0)));
	WriteBE32(&NetBuffer[HCDEServiceHeaderSize + 5u], uint32_t(max<int>(ClientTic, 0)));
	WriteBE32(&NetBuffer[HCDEServiceHeaderSize + 9u], 0u);
	NetBufferLength = HCDEServiceHeaderSize + 13u;
	DebugTrace::Markf("net", "queued %s room=%u gametic=%d clienttic=%d reason=%s",
		HCDEServiceName(service), unsigned(roomId), gametic, ClientTic,
		reason != nullptr ? reason : "unknown");
	return CommitReliableHCDEPregameService(to, connection, service, 0u);
}

static void AckHCDEControlService(EHCDEPregameService service)
{
	if (BeginReliableHCDEPregameService(service, Connected[0], 0u))
	{
		NetBufferLength = HCDEServiceHeaderSize;
		CommitReliableHCDEPregameService(Connected[0].Address, Connected[0], service, 0u);
		FlushHCDEReliableServices(Connected[0].Address, Connected[0], true);
	}
}

static void HandleGuestRuntimeHCDEService()
{
	if (I_IsLocalHCDEServiceAuthority())
		return;
	if (RemoteClient != 0 || NetBufferLength < HCDEServiceHeaderSize || NetBuffer[0] != NCMD_SETUP || NetBuffer[1] != PRE_HCDE_SERVICE)
		return;
	if (!Connected[0].bHCDEConnect)
		return;

	const auto service = EHCDEPregameService(NetBuffer[2]);
	switch (service)
	{
	case HPS_RESYNC_BEGIN:
		if (!CheckHCDEPregameService(0u, HCDEServiceHeaderSize + 13u, "guest runtime service resync begin"))
			break;
		I_NetMessage("Receiving HCDE resync");
		DebugTrace::Markf("net", "guest runtime resync begin room=%u gametic=%u clienttic=%u consistency=%u",
			unsigned(NetBuffer[HCDEServiceHeaderSize]),
			unsigned(ReadBE32(&NetBuffer[HCDEServiceHeaderSize + 1u])),
			unsigned(ReadBE32(&NetBuffer[HCDEServiceHeaderSize + 5u])),
			unsigned(ReadBE32(&NetBuffer[HCDEServiceHeaderSize + 9u])));
		AckHCDEControlService(HPS_RESYNC_ACK);
		break;
	case HPS_HEARTBEAT:
		CheckHCDEPregameService(0u, HCDEServiceHeaderSize, "guest runtime service heartbeat");
		break;
	default:
		DebugTrace::Markf("net", "ignored guest runtime HCDE service %u", unsigned(NetBuffer[2]));
		break;
	}
}

static void GetPacket(sockaddr_in* const from = nullptr)
{
	sockaddr_in fromAddress;
	socklen_t fromSize = sizeof(fromAddress);

	int msgSize = recvfrom(MySocket, (char *)TransmitBuffer, MaxTransmitSize, 0,
				  (sockaddr *)&fromAddress, &fromSize);

	int client = FindClient(fromAddress);
	if (client >= 0 && msgSize == SOCKET_ERROR)
	{
		int err = WSAGetLastError();
		if (err == WSAECONNRESET)
		{
			if (consoleplayer == -1)
			{
				client = -1;
				msgSize = 0;
			}
			else
			{
				// The remote node aborted unexpectedly, so pretend it sent an exit packet. If it was the
				// authority, the game is too bricked to continue because authority migration owns recovery.
				I_NetLog("Connection reset by client %d '%s'", client, Net_GetClientName(client, 0u));
				DebugTrace::Warningf("net", "connection reset client=%d name=%s", client, Net_GetClientName(client, 0u));
				const bool resetFromReservedServerSlot = DedicatedServerMode
					&& I_IsServerReservedSlot(client)
					&& I_IsLocalHCDEServiceAuthority();
				if (resetFromReservedServerSlot)
				{
					// Windows can report a UDP reset against the socket even when the
					// dedicated server's reserved authority slot is not a remote peer.
					// Treating that pseudo-client as a disconnected authority tears down
					// the whole session during localhost reconnect tests.
					client = -1;
					msgSize = 0;
				}
				else if (I_IsHCDEServiceAuthoritySlot(client))
					I_NetError("Authority unexpectedly disconnected");
				else
				{
					NetBuffer[0] = NCMD_EXIT;
					msgSize = 1;
				}
			}
		}
		else if (err != WSAEWOULDBLOCK)
		{
			I_Error("Failed to get packet: %s", neterror());
		}
		else
		{
			client = -1;
			msgSize = 0;
		}
	}
		else if (msgSize > 0)
		{
			++HCDEPregameServiceProfile.PacketReceived;
			if (TryHandleServerQuery(fromAddress, TransmitBuffer, msgSize))
			{
			RemoteClient = -1;
			NetBufferLength = 0u;
			if (from != nullptr)
			*from = fromAddress;
			return;
		}
		if (msgSize < 5)
		{
			++HCDEPregameServiceProfile.PacketTooShort;
			DebugTrace::Markf("net", "ignored undersized packet from %s:%u len=%d", inet_ntoa(fromAddress.sin_addr), ntohs(fromAddress.sin_port), msgSize);
			client = -1;
			msgSize = 0;
		}
		else
		{
			const uint8_t* dataStart = &TransmitBuffer[4];
			const int payloadSize = msgSize - 4;
			if (client == -1 && !( *dataStart & NCMD_SETUP))
			{
				msgSize = 0;
			}
			else
			{
				const uint32_t check = (*dataStart & NCMD_SETUP) ? CalcCRC32(dataStart, payloadSize) : AddCRC32(CalcCRC32(dataStart, payloadSize), GameID, std::extent_v<decltype(GameID)>);
				const uint32_t crc = (TransmitBuffer[0] << 24) | (TransmitBuffer[1] << 16) | (TransmitBuffer[2] << 8) | TransmitBuffer[3];
				if (check != crc)
				{
					++HCDEPregameServiceProfile.PacketBadCrc;
					DPrintf(DMSG_NOTIFY, "Checksum on packet failed: expected %u, got %u", check, crc);
					client = -1;
					msgSize = 0;
				}
				else
				{
					NetBuffer[0] = (*dataStart & ~NCMD_COMPRESSED);
					if (*dataStart & NCMD_COMPRESSED)
					{
						if (payloadSize <= 1)
						{
							++HCDEPregameServiceProfile.PacketCompressedMalformed;
							DebugTrace::Markf("net", "ignored malformed compressed packet from %s:%u len=%d", inet_ntoa(fromAddress.sin_addr), ntohs(fromAddress.sin_port), msgSize);
							client = -1;
							msgSize = 0;
						}
						else
						{
							uLongf size = MAX_MSGLEN - 1;
							const int err = uncompress(NetBuffer + 1, &size, dataStart + 1, msgSize - 5);
							if (err != Z_OK)
							{
								++HCDEPregameServiceProfile.PacketCompressedDecompressFailure;
								Printf("Net decompression failed (zlib error %s)\n", M_ZLibError(err).GetChars());
								client = -1;
								msgSize = 0;
							}
							else
							{
								msgSize = static_cast<int>(size) + 1;
							}
						}
					}
					else
					{
						const size_t copySize = size_t(payloadSize - 1);
						if (copySize >= MAX_MSGLEN || copySize >= MaxTransmitSize)
						{
								++HCDEPregameServiceProfile.PacketOversized;
								DebugTrace::Markf("net", "ignored oversized uncompressed packet from %s:%u payload=%zu", inet_ntoa(fromAddress.sin_addr), ntohs(fromAddress.sin_port), copySize);
								client = -1;
								msgSize = 0;
							}
						else
						{
							msgSize = payloadSize;
							if (copySize > 0u)
								memcpy(NetBuffer + 1, dataStart + 1, copySize);
						}
					}

					// Inbound handshake breadcrumb (server side). A decoded setup
					// packet from an address with no assigned client slot is the
					// counterpart to the guest's "sendto ok" trace: if a stuck
					// joiner's packets physically reach us, they appear here. If
					// the guest logs sends but this line never fires, the loss is
					// in transit (NAT/hairpin/firewall), not in admission logic.
					// Rare by nature (only unrecognized peers), so no flood risk.
					if (client == -1 && (NetBuffer[0] & NCMD_SETUP) && msgSize >= 2)
					{
						DebugTrace::Markf("net", "inbound setup from unknown peer %s:%u type=%u len=%d started=%d",
							inet_ntoa(fromAddress.sin_addr), unsigned(ntohs(fromAddress.sin_port)),
							unsigned(NetBuffer[1]), msgSize, bGameStarted ? 1 : 0);
					}

					// During an active match, allow setup/connect packets to enter
					// the dedicated runtime late-join admission path instead of
					// rejecting unknown peers immediately as PRE_IN_PROGRESS.
					//
					// Admission is allowed when this node is the HCDE service
					// authority AND either (a) we were launched as a true
					// dedicated server (-server) or (b) we are a listen-server
					// host (-host) with sv_lateJoin enabled. The latter is what
					// makes a listen server feel like an "open-entry dedicated"
					// from the joiner's perspective.
					if (client == -1 && bGameStarted)
					{
						const bool authority = I_IsLocalHCDEServiceAuthority();
						const bool listenLateJoinAllowed = !DedicatedServerMode && *sv_lateJoin;
						const bool admissionAllowed = authority && (DedicatedServerMode || listenLateJoinAllowed);
						// TryProcessSetupConnectPacket reads the module-level
						// NetBufferLength, but GetPacket normally writes it only
						// after this branch. Publish the just-decoded setup packet
						// length before late-join admission, or the parser sees a
						// stale/zero length and falls back to PRE_IN_PROGRESS.
						NetBufferLength = max<int>(msgSize, 0);
						const bool processed = admissionAllowed
							&& TryProcessSetupConnectPacket(fromAddress, strlen(net_password) > 0, false, true, nullptr);
						if (processed)
						{
							client = FindClient(fromAddress);
						}
						if (client == -1 && !processed)
						{
							NetBuffer[0] = NCMD_SETUP;
							NetBuffer[1] = PRE_IN_PROGRESS;
							NetBufferLength = 2u;
							SendPacket(fromAddress);
							msgSize = 0;
						}
					}
				}
			}
		}
	}
	else
	{
		client = -1;
	}

	RemoteClient = client;
	NetBufferLength = max<int>(msgSize, 0);
	if (NetBufferLength > MAX_MSGLEN)
	{
		// Track silent truncation so soak/stress tools can see when peers
		// are sending payloads beyond the configured ceiling. Without this
		// telemetry, a misbehaving peer or buggy mod can flood the buffer
		// and the only symptom is mysterious parse failures downstream.
		++HCDEPregameServiceProfile.PacketOversized;
		DebugTrace::Markf("net",
			"clamped oversized packet from %s:%u length=%d max=%u client=%d",
			inet_ntoa(fromAddress.sin_addr), unsigned(ntohs(fromAddress.sin_port)),
			NetBufferLength, unsigned(MAX_MSGLEN), client);
		NetBufferLength = MAX_MSGLEN;
	}
	if (from != nullptr)
		*from = fromAddress;
}

void I_NetCmd(ENetCommand cmd)
{
	if (cmd == CMD_SEND)
	{
		if (RemoteClient >= 0)
			SendPacket(Connected[RemoteClient].Address);
	}
	else if (cmd == CMD_GET)
	{
		GetPacket();
	}
}

static void SetClientAck(size_t client, size_t from, bool add)
{
	if (client >= static_cast<size_t>(MAXPLAYERS) || from >= static_cast<size_t>(MAXPLAYERS) || from >= 64u)
	{
		DebugTrace::Markf("net", "ignored invalid client ack update client=%zu from=%zu add=%u",
			client, from, add ? 1u : 0u);
		return;
	}

	const uint64_t bit = (uint64_t)1u << from;
	if (add)
		Connected[client].InfoAck |= bit;
	else
		Connected[client].InfoAck &= ~bit;
}

static bool ClientGotAck(size_t client, size_t from)
{
	if (client >= static_cast<size_t>(MAXPLAYERS) || from >= static_cast<size_t>(MAXPLAYERS) || from >= 64u)
		return false;

	return (Connected[client].InfoAck & ((uint64_t)1u << from));
}

static bool GetConnection(sockaddr_in& from)
{
	GetPacket(&from);
	return NetBufferLength > 0;
}

static bool TryParseStrictInt(const char* text, int& out)
{
	if (text == nullptr || text[0] == '\0')
		return false;

	errno = 0;
	char* end = nullptr;
	const long parsed = strtol(text, &end, 10);
	if (errno == ERANGE || end == text || *end != '\0')
		return false;
	if (parsed < std::numeric_limits<int>::min() || parsed > std::numeric_limits<int>::max())
		return false;

	out = static_cast<int>(parsed);
	return true;
}

static int CountConnectedPlayers()
{
	int connected = 0;
	for (int i = 0; i < MaxClients; ++i)
	{
		if (Connected[i].Status != CSTAT_NONE)
			++connected;
	}
	return connected;
}

static bool TryProcessSetupConnectPacket(const sockaddr_in& from, bool hasPassword, bool rejectForInProgress, bool runtimeJoin, int* connectedPlayers)
{
	if (NetBufferLength < 2u || NetBuffer[0] != NCMD_SETUP || NetBuffer[1] != PRE_CONNECT)
		return false;

	uint8_t* engineInfo = &NetBuffer[2];
	if (NetBufferLength < 9u)
	{
		// Truncated PRE_CONNECT is a wire-protocol problem, not a credential
		// failure. Reporting `PRE_WRONG_PASSWORD` here actively misleads
		// operators trying to diagnose mismatched launchers / corrupted
		// envelopes; switch to the protocol-error reason that the client
		// surface ("HCDE service protocol negotiation") already covers.
		DebugTrace::Markf("net", "malformed connect packet from %s (len=%zu)", inet_ntoa(from.sin_addr), NetBufferLength);
		RejectConnection(from, PRE_PROTOCOL_ERROR);
		return true;
	}

	size_t passwordOffset = 0u;
	size_t banned = 0u;
	FVerificationError error = {};
	for (; banned < BannedConnections.Size(); ++banned)
	{
		if (BannedConnections[banned].sin_addr.s_addr == from.sin_addr.s_addr)
			break;
	}

	if (banned < BannedConnections.Size())
	{
		RejectConnection(from, PRE_BANNED);
		return true;
	}
	if ((error = Net_VerifyEngine(engineInfo, passwordOffset, NetBufferLength - 2u)).Error != FVerificationError::VE_NONE)
	{
		SendVerificationError(from, error);
		return true;
	}
	if (2u + passwordOffset >= NetBufferLength)
	{
		// Out-of-range offset / unterminated password are wire-protocol
		// failures, not credential mismatches; report them as such so the
		// client UI surfaces the correct error.
		DebugTrace::Markf("net", "malformed connect password from %s (offset=%zu len=%zu)", inet_ntoa(from.sin_addr), passwordOffset, NetBufferLength);
		RejectConnection(from, PRE_PROTOCOL_ERROR);
		return true;
	}

	const size_t passwordStart = 2u + passwordOffset;
	size_t passwordEnd = 0u;
	if (!FindStringEnd(passwordStart, NetBufferLength, passwordEnd))
	{
		DebugTrace::Markf("net", "unterminated connect password from %s (offset=%zu len=%zu)", inet_ntoa(from.sin_addr), passwordOffset, NetBufferLength);
		RejectConnection(from, PRE_PROTOCOL_ERROR);
		return true;
	}

	FHCDEConnectInfo connectInfo = {};
	ReadHCDEConnectInfo(passwordEnd, connectInfo);
	if (connectInfo.Present && connectInfo.Version != HCDEConnectProtocolVersion)
	{
		DebugTrace::Markf("net", "unsupported HCDE service connect version %u from %s", connectInfo.Version, inet_ntoa(from.sin_addr));
		RejectConnection(from, PRE_PROTOCOL_ERROR);
		return true;
	}
	// Phase 3 (UZDoom legacy removal): HCDE service is mandatory. A peer that
	// does not advertise HCDE connect info in its PRE_CONNECT packet is either
	// a stock UZDoom/ZDoom client or a HCDE client that bypassed the
	// `-join` / `-dedicatedjoin` launcher path. Either way the peer cannot
	// participate in the HCDE Live snapshot stream, and admitting them would
	// only re-enable the legacy P2P lockstep code paths we are removing.
	// Reject them as a protocol-error here so the server stays HCDE-only.
	if (!connectInfo.Present)
	{
		Printf("NetServer:: Rejecting non-HCDE setup connect from %s (HCDE service is required; relaunch with -join or -dedicatedjoin).\n", inet_ntoa(from.sin_addr));
		DebugTrace::Warningf("net", "rejecting non-HCDE PRE_CONNECT from %s; HCDE service required", inet_ntoa(from.sin_addr));
		RejectConnection(from, PRE_PROTOCOL_ERROR);
		return true;
	}

	if (hasPassword && strcmp(net_password, (const char*)&NetBuffer[passwordStart]))
	{
		RejectConnection(from, PRE_WRONG_PASSWORD);
		return true;
	}

	const int currentConnected = connectedPlayers != nullptr ? *connectedPlayers : CountConnectedPlayers();
	if (currentConnected >= MaxClients)
	{
		RejectConnection(from, PRE_FULL);
		return true;
	}
	if (rejectForInProgress)
	{
		RejectConnection(from, PRE_IN_PROGRESS);
		return true;
	}

	int free = 1;
	for (; free < MaxClients; ++free)
	{
		if (Connected[free].Status == CSTAT_NONE)
			break;
	}
	if (free >= MaxClients)
	{
		RejectConnection(from, PRE_FULL);
		return true;
	}

	AddClientConnection(from, free, connectInfo, runtimeJoin);
	const int updatedConnected = currentConnected + 1;
	if (connectedPlayers != nullptr)
		*connectedPlayers = updatedConnected;
	I_NetUpdatePlayers(updatedConnected, MaxClients);
	return true;
}

static void RejectConnection(const sockaddr_in& to, ENetConnectType reason)
{
	NetBuffer[0] = NCMD_SETUP;
	NetBuffer[1] = reason;
	NetBufferLength = 2u;

	SendPacket(to);
}

static void SendVerificationError(const sockaddr_in& to, const FVerificationError& error)
{
	NetBuffer[0] = NCMD_SETUP;
	NetBuffer[1] = PRE_VERIFICATION_ERROR;
	NetBuffer[2] = error.Error;
	if (error.Error == FVerificationError::VE_ENGINE)
	{
		NetBuffer[3] = error.Major;
		NetBuffer[4] = error.Minor;
		NetBuffer[5] = error.Revision;
		NetBuffer[6] = error.NetMajor;
		NetBuffer[7] = error.NetMinor;
		NetBuffer[8] = error.NetRevision;
		NetBufferLength = 9u;
	}
	else
	{
		const TArray<FString>* ar = nullptr;
		if (error.Error == FVerificationError::VE_FILE_UNKNOWN)
			ar = &error.UnknownFiles;
		else if (error.Error == FVerificationError::VE_FILE_ORDER)
			ar = &error.ExpectedOrder;
		else if (error.Error == FVerificationError::VE_FILE_MISSING)
			ar = &error.MissingFiles;

		if (ar == nullptr)
		{
			DebugTrace::Markf("net", "verification error payload type %u has no list", error.Error);
			NetBuffer[3] = NetBuffer[4] = NetBuffer[5] = NetBuffer[6] = 0u;
			NetBufferLength = 7u;
			SendPacket(to);
			return;
		}

		size_t count = 0u;
		size_t i = 7u;
		for (auto& file : *ar)
		{
			const size_t len = static_cast<size_t>(file.Len()) + 1u;
			if (i + len > MAX_MSGLEN)
			{
				DebugTrace::Markf("net", "verification error truncated after %zu entries", count);
				Printf("Verification error reply truncated to fit packet size\n");
				break;
			}

			memcpy(&NetBuffer[i], file.GetChars(), file.Len() + 1u);
			i += len;
			++count;
		}
		NetBuffer[3] = (count >> 24);
		NetBuffer[4] = (count >> 16);
		NetBuffer[5] = (count >> 8);
		NetBuffer[6] = count;
		NetBufferLength = i;
	}

	SendPacket(to);
}

static void AddClientConnection(const sockaddr_in& from, int client, const FHCDEConnectInfo& connectInfo, bool runtimeJoin)
{
	Net_ResetClientState(client);
	// Net_ResetClientState() scrubs the d_net live state (ClientStates[], live
	// peers, replicated-actor baselines) but does NOT touch the i_net transport
	// slot Connected[client], which owns the reliable pregame-service sequence
	// (HCDEServiceTxSeq/RxSeq/PeerAck), the bHasGameInfo/bHasMapLoadInfo/
	// bHasRosterInfo/bHasStartGameAck ack flags, and the HCDEReliableServices[] queue. Reusing a
	// slot whose Connected[] state survived (a path that frees the slot for reuse
	// without routing through I_ClearClient) would make the WAITING setup driver
	// skip map-load/game-info because their stale ack flags still read true, and
	// continue the old TxSeq so the joiner -- which starts a fresh RxSeq at 0 --
	// gets out-of-step service packets. Clearing here makes the transport reset
	// symmetric with the live-state reset above and independent of the disconnect
	// path that vacated the slot. The fresh connection fields are set immediately
	// below.
	Connected[client].Clear();
	Connected[client].Status = CSTAT_CONNECTING;
	Connected[client].Address = from;
	Connected[client].SessionToken = MakeSessionToken(from, client);
	Connected[client].bHCDEConnect = connectInfo.Present;
	Connected[client].HCDEConnectVersion = connectInfo.Version;
	Connected[client].HCDEConnectFlags = connectInfo.Flags;
	Connected[client].bRuntimeJoin = runtimeJoin;
	NetworkClients += client;
	if (runtimeJoin && !I_IsServerReservedSlot(client))
	{
		// Runtime join admission is two-stage. AddClientConnection creates the
		// transport slot immediately so reliable setup packets can flow, but the
		// player must not participate in the world until ClientConnecting sees
		// CSTAT_READY and explicitly marks the slot PST_ENTER/playeringame=true.
		// Reconnects can reuse a slot whose gameplay flags still look live from
		// the prior session; if we leave playeringame true here, the next normal
		// authority tic can run P_PlayerThink before a pawn has been respawned and
		// abort with "No player N start" while the client is still stuck on
		// "Sending player information".
		playeringame[client] = false;
		players[client].waiting = false;
		players[client].inconsistant = false;
		players[client].settings_controller = false;
		if (players[client].mo != nullptr || players[client].camera != nullptr)
			SET_PLAYER_STATE(&players[client], client, PST_GONE, "runtime_connect_transport_reset");
		else
			SET_PLAYER_STATE(&players[client], client, PST_DEAD, "runtime_connect_transport_reset");
	}
	// HCDE-only admission gate: TryProcessSetupConnectPacket rejects every
	// peer that does not advertise HCDE service connect info (Phase 3),
	// so `connectInfo.Present` is true here for every admitted client. The
	// else-branch log that used to claim "legacy setup" was misleading: it
	// could only fire if the admission policy regressed. Treat that as a
	// hard programming error so we notice immediately instead of silently
	// admitting a non-HCDE peer.
	if (connectInfo.Present)
	{
		// Log the resolved source endpoint we will reply to. On a stuck rejoin the
		// server can latch onto a stale port from the prior connection; this single
		// line is the ground truth for "where will map-load/game-info be sent" and
		// is meant to be cross-checked against the joining client's local bind port.
		I_NetLog("Client %u connected to server with HCDE service connect v%u flags=0x%02x%s dest=%s:%u",
			client, connectInfo.Version, connectInfo.Flags, runtimeJoin ? " (runtime join)" : "",
			inet_ntoa(from.sin_addr), unsigned(ntohs(from.sin_port)));
	}
	else
	{
		assert(false && "AddClientConnection: HCDE service connect info missing - admission policy bug");
		I_NetLog("Client %u admitted without HCDE service info (admission policy bug)%s",
			client, runtimeJoin ? " (runtime join)" : "");
	}
	I_NetClientUpdated(client);

	// Make sure any ready clients are marked as needing the new client's info.
	if (!runtimeJoin)
	{
		for (int i = 1; i < MaxClients; ++i)
		{
			if (Connected[i].Status == CSTAT_READY)
			{
				Connected[i].Status = CSTAT_WAITING;
				I_NetClientUpdated(i);
			}
		}
	}
}

static void RemoveClientConnection(int client, const char* reason = nullptr)
{
	I_NetClientDisconnected(client, reason);
	players[client].settings_controller = false;
	Net_ClearRuntimeClientJoinState(client);
	I_ClearClient(client);
	NetworkClients -= client;
	if (reason != nullptr && reason[0] != '\0')
		I_NetLog("Client %u %s: %s", client, DedicatedServerMode ? "disconnected from server" : "left the host", reason);
	else
		I_NetLog("Client %u %s", client, DedicatedServerMode ? "disconnected from server" : "left the host");

	// Let everyone else know the user left as well.
	NetBuffer[0] = NCMD_SETUP;
	NetBuffer[1] = PRE_DISCONNECT;
	NetBuffer[2] = client;

	for (int i = 1; i < MaxClients; ++i)
	{
		if (Connected[i].Status == CSTAT_NONE)
			continue;

		SetClientAck(i, client, false);
		WriteBE32(&NetBuffer[3], Connected[i].SessionToken);
		NetBufferLength = 7u;
		SendPacket(Connected[i].Address);
	}
}

static bool DropClientForHCDETimeout(int client, int* connectedPlayers, const char* context)
{
	if (client <= 0 || client >= MaxClients || Connected[client].Status == CSTAT_NONE || !Connected[client].bHCDEConnect)
		return false;

	// Only the pregame setup handshake may drop a client for an unacked reliable
	// service. A fully-admitted live client (READY + roster + start-game ack) is
	// in the game already; its liveness is owned by CheckDeadClients() in d_net,
	// not by this setup-timeout path. Runtime control services (e.g. the on-demand
	// resync RESYNC_BEGIN) are queued on the same reliable channel and make
	// HasPendingHCDEReliableService() true, which routes a healthy live slot into
	// the maintenance timeout loop. Without this guard a delayed resync ack on a
	// momentarily service-silent peer would satisfy the hard-timeout clause below
	// and disconnect a healthy in-game player. Defer such slots to the live
	// dead-client reaper instead.
	if (Connected[client].Status == CSTAT_READY
		&& Connected[client].bHasRosterInfo
		&& Connected[client].bHasStartGameAck)
		return false;

	const uint64_t now = I_msTime();
	auto* pending = FindTimedOutHCDEReliableService(Connected[client], now);
	if (pending == nullptr)
		return false;

	auto& connection = Connected[client];
	const uint64_t pendingElapsed = now - pending->FirstSendTime;
	if (connection.HCDEServiceLastValidRxTime > 0u
		&& now - connection.HCDEServiceLastValidRxTime < HCDEServiceTimeoutMS
		&& pendingElapsed < HCDEServiceHardTimeoutMS)
	{
		// Runtime late join can be slow under heavy debug tracing or packet
		// loss: a client may still be actively retransmitting valid service
		// packets while our oldest pending service has not yet been acked. Do
		// not drop a live peer just because one retained service crossed the
		// resend timeout; require peer silence as well. If the peer really
		// died, LastValidRxTime stops advancing and the normal timeout applies.
		//
		// The HardTimeout clause bounds the opposite failure: a peer that keeps
		// chattering valid traffic but can never advance the handshake (the
		// stale-port stuck-rejoin) is dropped once a single service has been
		// unacked past the hard ceiling, so it cannot hold its slot forever.
		return false;
	}

	const auto service = pending->Service;
	const uint8_t key = pending->Key;
	const uint32_t sequence = pending->Sequence;
	const uint32_t sends = pending->SendCount;
	const uint64_t elapsed = pendingElapsed;
	const sockaddr_in timedOutAddress = Connected[client].Address;

	I_NetLog("Client %d timed out during %s on HCDE service %s key=%u seq=%u after %llu ms (%u sends)",
		client, context, HCDEServiceName(service), key, sequence, (unsigned long long)elapsed, sends);
	DebugTrace::Markf("net", "dropping client %d after HCDE service timeout context=%s service=%s key=%u seq=%u elapsed=%llu sends=%u",
		client, context, HCDEServiceName(service), key, sequence, (unsigned long long)elapsed, sends);

	RejectConnection(timedOutAddress, PRE_SETUP_TIMEOUT);
	RemoveClientConnection(client, "HCDE service setup timeout");
	if (connectedPlayers != nullptr && *connectedPlayers > 0)
	{
		--*connectedPlayers;
		I_NetUpdatePlayers(*connectedPlayers, MaxClients);
	}
	++HCDEPregameServiceProfile.ServiceTimeoutDrops;
	return true;
}

static void DriveRuntimeSetupStateForClient(int client, int connectedPlayers)
{
	if (client <= 0 || client >= MaxClients)
		return;

	auto& con = Connected[client];
	if (con.Status == CSTAT_NONE)
		return;

	if (con.Status == CSTAT_CONNECTING)
	{
		const uint64_t now = I_msTime();
		if (con.HCDERuntimeLastConnectAckTime == 0u
			|| now - con.HCDERuntimeLastConnectAckTime >= HCDERuntimeConnectAckResendMS)
		{
			con.HCDERuntimeLastConnectAckTime = now;
			BeginSetupPacket(PRE_CONNECT_ACK, con.SessionToken, 5u);
			NetBuffer[2] = client;
			NetBuffer[3] = connectedPlayers;
			NetBuffer[4] = MaxClients;
			// Runtime setup is driven from the live net loop and may run many
			// times between tics while packets are being drained. Pace the
			// admission ACK like the reliable service resend path so a rejoiner
			// does not receive hundreds of duplicate connect ACKs before its
			// user-info packet can advance the setup state.
			const bool advertiseDedicated = DedicatedServerMode || con.bRuntimeJoin;
			uint8_t ackFlags = advertiseDedicated ? PRE_CONNECT_ACK_DEDICATED : 0u;
			ackFlags |= PRE_CONNECT_ACK_HCDE_SERVICE;
			if (advertiseDedicated)
				ackFlags |= PRE_CONNECT_ACK_SERVER_AUTHORITY;
			NetBuffer[9] = ackFlags;
			NetBufferLength = 10u;
			NetBuffer[NetBufferLength++] = HCDEConnectProtocolVersion;
			NetBuffer[NetBufferLength++] = HCDE_CONNECT_SERVER_AUTHORITY;
			SendPacket(con.Address);
		}

		if (BeginReliableHCDEPregameService(HPS_CONSOLE_PLAYER, con, uint8_t(client)))
		{
			NetBuffer[HCDEServiceHeaderSize] = uint8_t(client);
			NetBuffer[HCDEServiceHeaderSize + 1u] = uint8_t(connectedPlayers);
			NetBuffer[HCDEServiceHeaderSize + 2u] = uint8_t(MaxClients);
			NetBuffer[HCDEServiceHeaderSize + 3u] = HCDE_CONNECT_SERVER_AUTHORITY;
			NetBufferLength = HCDEServiceHeaderSize + 4u;
			CommitReliableHCDEPregameService(con.Address, con, HPS_CONSOLE_PLAYER, uint8_t(client));
		}

		FlushHCDEReliableServices(con.Address, con);
		return;
	}

	if (con.Status == CSTAT_WAITING)
	{
		bool clientReady = true;
		if (!ClientGotAck(client, client))
		{
			if (BeginReliableHCDEPregameService(HPS_USER_INFO_ACK, con, uint8_t(client)))
			{
				NetBuffer[HCDEServiceHeaderSize] = uint8_t(client);
				NetBufferLength = HCDEServiceHeaderSize + 1u;
				CommitReliableHCDEPregameService(con.Address, con, HPS_USER_INFO_ACK, uint8_t(client));
			}
			clientReady = false;
		}

		if (!con.bHasMapLoadInfo)
		{
			if (BeginReliableHCDEPregameService(HPS_MAP_LOAD, con, 0u))
			{
				TArrayView<uint8_t> stream = TArrayView(&NetBuffer[NetBufferLength], MAX_MSGLEN - NetBufferLength);
				Net_SetMapLoadInfo(stream);
				NetBufferLength += stream.Data() - &NetBuffer[NetBufferLength];
				CommitReliableHCDEPregameService(con.Address, con, HPS_MAP_LOAD, 0u);
			}
			clientReady = false;
		}

		if (!con.bHasGameInfo)
		{
			if (BeginReliableHCDEPregameService(HPS_GAME_INFO, con, 0u))
			{
				NetBuffer[HCDEServiceHeaderSize] = TicDup;
				memcpy(&NetBuffer[HCDEServiceHeaderSize + 1u], GameID, 8);
				NetBufferLength = HCDEServiceHeaderSize + 9u;

				TArrayView<uint8_t> stream = TArrayView(&NetBuffer[NetBufferLength], MAX_MSGLEN - NetBufferLength);
				Net_SetServerInfo(stream);
				NetBufferLength += stream.Data() - &NetBuffer[NetBufferLength];
				CommitReliableHCDEPregameService(con.Address, con, HPS_GAME_INFO, 0u);
			}
			clientReady = false;
		}

		if (!con.bHasRosterInfo)
		{
			QueueHCDERosterService(con.Address, con, client);
			clientReady = false;
		}

		if (con.bRuntimeJoin && !con.bHasBootstrapInfo)
		{
			QueueHCDEBootstrapControlService(con.Address, con, HPS_BOOTSTRAP_BEGIN, "runtime-join");
			clientReady = false;
		}

		if (clientReady)
		{
			con.Status = CSTAT_READY;
			I_NetClientUpdated(client);
			DebugTrace::Markf("net", "runtime late-join setup reached ready slot=%d", client);
		}
		else
		{
			// Throttle to at most once per second per client. This breadcrumb is
			// otherwise emitted on every tic and every received packet during the
			// WAITING phase, producing thousands of identical lines per second that
			// roll the trace files before the connect/admission phase can be read.
			const uint64_t nowWaitLog = I_msTime();
			if (nowWaitLog - con.HCDEServiceLastWaitLogTime >= 1000u)
			{
				con.HCDEServiceLastWaitLogTime = nowWaitLog;
				auto* oldest = FindOldestHCDEReliableService(con);
				DebugTrace::Debugf("net", "runtime setup waiting slot=%d status=WAITING has-map=%d has-game=%d ack-self=%d pending=%s key=%u seq=%u sends=%u peerAck=%u",
					client, con.bHasMapLoadInfo ? 1 : 0, con.bHasGameInfo ? 1 : 0,
					ClientGotAck(client, client) ? 1 : 0,
					oldest != nullptr ? HCDEServiceName(oldest->Service) : "<none>",
					oldest != nullptr ? oldest->Key : 0u,
					oldest != nullptr ? oldest->Sequence : 0u,
					oldest != nullptr ? oldest->SendCount : 0u,
					con.HCDEServicePeerAck);
			}
		}
		FlushHCDEReliableServices(con.Address, con);
		return;
	}

	if (con.Status == CSTAT_READY)
	{
		if (!con.bHasStartGameAck && BeginReliableHCDEPregameService(HPS_START_GAME, con, 0u))
			CommitReliableHCDEPregameService(con.Address, con, HPS_START_GAME, 0u);
	}

	FlushHCDEReliableServices(con.Address, con);
}

void HandleIncomingConnection()
{
	if (!I_IsLocalHCDEServiceAuthority())
	{
		HandleGuestRuntimeHCDEService();
		return;
	}

	if (RemoteClient < 0 || RemoteClient >= MaxClients)
		return;

	auto& con = Connected[RemoteClient];

	if (NetBuffer[1] == PRE_HCDE_SERVICE)
	{
		if (RemoteClient <= 0 || !con.bHCDEConnect)
		{
			DebugTrace::Markf("net", "ignored runtime HCDE service packet from unnegotiated client %d", RemoteClient);
			return;
		}
		if (NetBufferLength < HCDEServiceHeaderSize)
		{
			DebugTrace::Markf("net", "runtime HCDE service packet too short len=%zu", NetBufferLength);
			return;
		}

		const auto service = EHCDEPregameService(NetBuffer[2]);
		switch (service)
		{
		case HPS_CLIENT_USER_INFO:
		{
			if (con.Status != CSTAT_CONNECTING)
				break;
			if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host runtime service userinfo"))
				break;

			TArrayView<uint8_t> stream = TArrayView(&NetBuffer[HCDEServiceHeaderSize], MAX_MSGLEN - HCDEServiceHeaderSize);
			Net_ReadUserInfo(RemoteClient, stream);
			con.Status = CSTAT_WAITING;
			I_NetClientConnected(RemoteClient, 16u);
			break;
		}
		case HPS_USER_INFO_ACK:
			if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize + 1u, "host runtime service userinfo ack"))
				break;
			if (NetBuffer[HCDEServiceHeaderSize] >= MaxClients || NetBuffer[HCDEServiceHeaderSize] >= MAXPLAYERS)
			{
				DebugTrace::Markf("net", "ignored invalid runtime HCDE userinfo ack slot=%u max=%d",
					static_cast<unsigned>(NetBuffer[HCDEServiceHeaderSize]), MaxClients);
				break;
			}
			SetClientAck(RemoteClient, NetBuffer[HCDEServiceHeaderSize], true);
			break;
		case HPS_GAME_INFO_ACK:
			if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host runtime service gameinfo ack"))
				break;
			con.bHasGameInfo = true;
			break;
		case HPS_MAP_LOAD_ACK:
			if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host runtime service mapload ack"))
				break;
			con.bHasMapLoadInfo = true;
			break;
		case HPS_ROSTER_ACK:
			if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host runtime service roster ack"))
				break;
			con.bHasRosterInfo = true;
			break;
		case HPS_BOOTSTRAP_ACK:
			if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host runtime service bootstrap ack"))
				break;
			if (!con.bHasBootstrapInfo)
				Net_BeginRuntimeBootstrap(RemoteClient, "hcde-service-bootstrap");
			con.bHasBootstrapInfo = true;
			break;
		case HPS_RESYNC_REQUEST:
			if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host runtime service resync request"))
				break;
			Net_RequestRuntimeResync(RemoteClient, "hcde-service-request");
			QueueHCDEBootstrapControlService(con.Address, con, HPS_RESYNC_BEGIN, "runtime-resync");
			break;
		case HPS_RESYNC_ACK:
			if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host runtime service resync ack"))
				break;
			DebugTrace::Markf("net", "runtime resync ack client=%d", RemoteClient);
			break;
		case HPS_START_GAME_ACK:
			if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host runtime service start ack"))
				break;
			con.bHasStartGameAck = true;
			break;
		case HPS_HEARTBEAT:
			CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host runtime service heartbeat");
			break;
		default:
			++HCDEPregameServiceProfile.ServiceUnsupported;
			DebugTrace::Markf("net", "ignored unsupported runtime HCDE service %u", unsigned(NetBuffer[2]));
			break;
		}
	}

	// Drive the same setup state machine used at startup, but scoped to the
	// active-match dedicated join path.
	DriveRuntimeSetupStateForClient(RemoteClient, CountConnectedPlayers());
}

void HandleIncomingConnectionMaintenance()
{
	if (!I_IsLocalHCDEServiceAuthority())
		return;

	// Run runtime setup retries/timeouts out-of-band from packet parsing so an
	// acknowledgement packet can be processed before timeout enforcement.
	int connectedPlayers = CountConnectedPlayers();
	for (int client = 1; client < MaxClients; ++client)
	{
		auto& con = Connected[client];
		if (con.Status == CSTAT_NONE || !con.bHCDEConnect)
			continue;

		const bool runtimeSetupInProgress = con.Status != CSTAT_READY
			|| !con.bHasRosterInfo
			|| !con.bHasStartGameAck
			|| HasPendingHCDEReliableService(con);
		if (!runtimeSetupInProgress)
			continue;

		if (DropClientForHCDETimeout(client, &connectedPlayers, "runtime setup maintenance"))
			continue;

		DriveRuntimeSetupStateForClient(client, connectedPlayers);
	}
}

static bool Host_CheckForConnections(void* connected)
{
	const bool hasPassword = strlen(net_password) > 0;
	int* connectedPlayers = (int*)connected;
	bool forceStarting = I_ShouldStartNetGame();
	if (DedicatedServerMode && !forceStarting && sv_dedicated_autostart && *connectedPlayers > 1)
	{
		DedicatedServerStartRequested = true;
		forceStarting = true;
		Printf("NetServer:: Auto-start requested (%d/%d playable clients connected).\n",
			max(*connectedPlayers - 1, 0), max(MaxClients - 1, 0));
	}
	if (DedicatedServerMode && forceStarting && *connectedPlayers <= 1)
	{
		DedicatedServerStartRequested = false;
		forceStarting = false;
		Printf("NetServer:: Start requested, but no playable clients are connected yet.\n");
	}

	TArray<int> toBoot = {};
	I_GetKickClients(toBoot);
	for (auto client : toBoot)
	{
		// Bound-check the UI-sourced slot before indexing Connected[]; mirrors
		// the disconnect handler's MAXPLAYERS guard so a bad index cannot read
		// or mutate past the array.
		if (client <= 0 || client >= MaxClients || client >= int(MAXPLAYERS) || Connected[client].Status == CSTAT_NONE)
			continue;

		sockaddr_in booted = Connected[client].Address;

		RemoveClientConnection(client, "kicked during setup");
		--*connectedPlayers;
		I_NetUpdatePlayers(*connectedPlayers, MaxClients);

		RejectConnection(booted, PRE_KICKED);
	}

	I_GetBanClients(toBoot);
	for (auto client : toBoot)
	{
		// Same bound check as the kick loop above before indexing Connected[].
		if (client <= 0 || client >= MaxClients || client >= int(MAXPLAYERS) || Connected[client].Status == CSTAT_NONE)
			continue;

		sockaddr_in booted = Connected[client].Address;
		BannedConnections.Push(booted);

		RemoveClientConnection(client, "banned during setup");
		--*connectedPlayers;
		I_NetUpdatePlayers(*connectedPlayers, MaxClients);

		RejectConnection(booted, PRE_BANNED);
	}

	sockaddr_in from;
	while (GetConnection(from))
	{
		if (NetBuffer[0] == NCMD_EXIT)
		{
			if (RemoteClient >= 0)
			{
				RemoveClientConnection(RemoteClient, "exit packet during setup");
				--*connectedPlayers;
				I_NetUpdatePlayers(*connectedPlayers, MaxClients);
			}

			continue;
		}

		if (NetBuffer[0] != NCMD_SETUP)
			continue;

		if (NetBuffer[1] == PRE_CONNECT)
		{
			if (RemoteClient >= 0)
				continue;

			TryProcessSetupConnectPacket(from, hasPassword, forceStarting, false, connectedPlayers);
		}
		else if (NetBuffer[1] == PRE_HCDE_SERVICE)
		{
			if (RemoteClient < 0 || RemoteClient >= MaxClients || !Connected[RemoteClient].bHCDEConnect)
			{
				DebugTrace::Markf("net", "ignored HCDE service packet from unnegotiated client %d", RemoteClient);
				continue;
			}
			if (NetBufferLength < HCDEServiceHeaderSize)
			{
				DebugTrace::Markf("net", "host HCDE service packet too short len=%zu", NetBufferLength);
				continue;
			}

			const auto service = EHCDEPregameService(NetBuffer[2]);
			switch (service)
			{
			case HPS_CLIENT_USER_INFO:
			{
				if (Connected[RemoteClient].Status != CSTAT_CONNECTING)
					break;
				if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host service userinfo"))
					break;

				TArrayView<uint8_t> stream = TArrayView(&NetBuffer[HCDEServiceHeaderSize], MAX_MSGLEN - HCDEServiceHeaderSize);
				Net_ReadUserInfo(RemoteClient, stream);
				Connected[RemoteClient].Status = CSTAT_WAITING;
				I_NetClientConnected(RemoteClient, 16u);
				break;
			}
				case HPS_USER_INFO_ACK:
					if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize + 1u, "host service userinfo ack"))
						break;
					if (NetBuffer[HCDEServiceHeaderSize] >= MaxClients || NetBuffer[HCDEServiceHeaderSize] >= MAXPLAYERS)
					{
						DebugTrace::Markf("net", "ignored invalid HCDE userinfo ack slot=%u max=%d",
							static_cast<unsigned>(NetBuffer[HCDEServiceHeaderSize]), MaxClients);
						break;
					}
					SetClientAck(RemoteClient, NetBuffer[HCDEServiceHeaderSize], true);
					break;
			case HPS_GAME_INFO_ACK:
				if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host service gameinfo ack"))
					break;
				Connected[RemoteClient].bHasGameInfo = true;
				break;
			case HPS_MAP_LOAD_ACK:
				if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host service mapload ack"))
					break;
				Connected[RemoteClient].bHasMapLoadInfo = true;
				break;
			case HPS_ROSTER_ACK:
				if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host service roster ack"))
					break;
				Connected[RemoteClient].bHasRosterInfo = true;
				break;
			case HPS_BOOTSTRAP_ACK:
				if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host service bootstrap ack"))
					break;
				if (!Connected[RemoteClient].bHasBootstrapInfo)
					Net_BeginRuntimeBootstrap(RemoteClient, "hcde-service-bootstrap");
				Connected[RemoteClient].bHasBootstrapInfo = true;
				break;
			case HPS_RESYNC_REQUEST:
				if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host service resync request"))
					break;
				Net_RequestRuntimeResync(RemoteClient, "hcde-service-request");
				QueueHCDEBootstrapControlService(Connected[RemoteClient].Address, Connected[RemoteClient], HPS_RESYNC_BEGIN, "runtime-resync");
				break;
			case HPS_RESYNC_ACK:
				if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host service resync ack"))
					break;
				DebugTrace::Markf("net", "resync ack client=%d", RemoteClient);
				break;
			case HPS_START_GAME_ACK:
				if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host service start ack"))
					break;
				if (!Connected[RemoteClient].bHasStartGameAck)
					I_NetLog("Client %d acknowledged HCDE service start", RemoteClient);
				Connected[RemoteClient].bHasStartGameAck = true;
				break;
			case HPS_HEARTBEAT:
				CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host service heartbeat");
				break;
			default:
				++HCDEPregameServiceProfile.ServiceUnsupported;
				DebugTrace::Markf("net", "ignored unsupported host HCDE service %u", unsigned(NetBuffer[2]));
				break;
			}
		}
	}

	SV_UpdateMaster();
	bool ready = true;
	NetBuffer[0] = NCMD_SETUP;
	for (int client = 1; client < MaxClients; ++client)
	{
		auto& con = Connected[client];
		if (DropClientForHCDETimeout(client, connectedPlayers, "pregame setup"))
		{
			ready = false;
			continue;
		}
		FlushHCDEReliableServices(con.Address, con);

		// If we're starting before the server is full, only check against connected clients.
		if (con.Status != CSTAT_READY && (!forceStarting || con.Status != CSTAT_NONE))
			ready = false;

		if (con.Status == CSTAT_CONNECTING)
		{
			// Pace the admission ACK exactly like DriveRuntimeSetupStateForClient.
			// This pregame loop also runs many times between tics (the dedicated
			// path spins on Sleep(1)), so without a guard a connecting client
			// would receive a burst of identical connect-acks; the first send is
			// immediate (timestamp 0) and the rest are spaced by the resend
			// interval. The reliable console-player service below self-paces and
			// stays outside the gate so it is queued promptly.
			const uint64_t now = I_msTime();
			if (con.HCDERuntimeLastConnectAckTime == 0u
				|| now - con.HCDERuntimeLastConnectAckTime >= HCDERuntimeConnectAckResendMS)
			{
				con.HCDERuntimeLastConnectAckTime = now;
				BeginSetupPacket(PRE_CONNECT_ACK, con.SessionToken, 5u);
				NetBuffer[2] = client;
				NetBuffer[3] = *connectedPlayers;
				NetBuffer[4] = MaxClients;
				// Mirror DriveRuntimeSetupStateForClient: a listen-server's late
				// joiner (bRuntimeJoin) needs to be told this is a dedicated /
				// server-authority connection so its client-side state machine
				// takes the late-join code path.
				const bool advertiseDedicated = DedicatedServerMode || con.bRuntimeJoin;
				uint8_t ackFlags = advertiseDedicated ? PRE_CONNECT_ACK_DEDICATED : 0u;
				ackFlags |= PRE_CONNECT_ACK_HCDE_SERVICE;
				if (advertiseDedicated)
					ackFlags |= PRE_CONNECT_ACK_SERVER_AUTHORITY;
				NetBuffer[9] = ackFlags;
				NetBufferLength = 10u;
				NetBuffer[NetBufferLength++] = HCDEConnectProtocolVersion;
				NetBuffer[NetBufferLength++] = HCDE_CONNECT_SERVER_AUTHORITY;
				SendPacket(con.Address);
			}
			if (BeginReliableHCDEPregameService(HPS_CONSOLE_PLAYER, con, uint8_t(client)))
			{
				NetBuffer[HCDEServiceHeaderSize] = uint8_t(client);
				NetBuffer[HCDEServiceHeaderSize + 1u] = uint8_t(*connectedPlayers);
				NetBuffer[HCDEServiceHeaderSize + 2u] = uint8_t(MaxClients);
				NetBuffer[HCDEServiceHeaderSize + 3u] = HCDE_CONNECT_SERVER_AUTHORITY;
				NetBufferLength = HCDEServiceHeaderSize + 4u;
				CommitReliableHCDEPregameService(con.Address, con, HPS_CONSOLE_PLAYER, uint8_t(client));
			}
		}
		else if (con.Status == CSTAT_WAITING)
		{
			bool clientReady = true;
			if (!ClientGotAck(client, client))
			{
				if (BeginReliableHCDEPregameService(HPS_USER_INFO_ACK, con, uint8_t(client)))
				{
					NetBuffer[HCDEServiceHeaderSize] = uint8_t(client);
					NetBufferLength = HCDEServiceHeaderSize + 1u;
					CommitReliableHCDEPregameService(con.Address, con, HPS_USER_INFO_ACK, uint8_t(client));
				}
				clientReady = false;
			}

			if (!con.bHasMapLoadInfo)
			{
				if (BeginReliableHCDEPregameService(HPS_MAP_LOAD, con, 0u))
				{
					TArrayView<uint8_t> stream = TArrayView(&NetBuffer[NetBufferLength], MAX_MSGLEN - NetBufferLength);
					Net_SetMapLoadInfo(stream);
					NetBufferLength += stream.Data() - &NetBuffer[NetBufferLength];
					CommitReliableHCDEPregameService(con.Address, con, HPS_MAP_LOAD, 0u);
				}
				clientReady = false;
			}

			if (!con.bHasGameInfo)
			{
				if (BeginReliableHCDEPregameService(HPS_GAME_INFO, con, 0u))
				{
					NetBuffer[HCDEServiceHeaderSize] = TicDup;
					memcpy(&NetBuffer[HCDEServiceHeaderSize + 1u], GameID, 8);
					NetBufferLength = HCDEServiceHeaderSize + 9u;

					TArrayView<uint8_t> stream = TArrayView(&NetBuffer[NetBufferLength], MAX_MSGLEN - NetBufferLength);
					Net_SetServerInfo(stream);
					NetBufferLength += stream.Data() - &NetBuffer[NetBufferLength];
					CommitReliableHCDEPregameService(con.Address, con, HPS_GAME_INFO, 0u);
				}
				clientReady = false;
			}

			if (!con.bHasRosterInfo)
			{
				QueueHCDERosterService(con.Address, con, client);
				clientReady = false;
			}

			if (con.bRuntimeJoin && !con.bHasBootstrapInfo)
			{
				QueueHCDEBootstrapControlService(con.Address, con, HPS_BOOTSTRAP_BEGIN, "runtime-join");
				clientReady = false;
			}

			if (clientReady)
			{
				con.Status = CSTAT_READY;
				I_NetClientUpdated(client);
			}
		}
		else if (con.Status == CSTAT_READY)
		{
			if (!HasPendingHCDEReliableService(con))
			{
				BeginHCDEPregameService(HPS_HEARTBEAT, con);
				NetBuffer[HCDEServiceHeaderSize] = *connectedPlayers;
				NetBuffer[HCDEServiceHeaderSize + 1u] = MaxClients;
				NetBufferLength = HCDEServiceHeaderSize + 2u;
				SendPacket(con.Address);
			}
		}
	}

	const bool shouldStart = ready && (*connectedPlayers >= MaxClients || forceStarting);
	if (shouldStart && forceStarting)
	{
		DedicatedServerStartRequested = false;
	}
	return shouldStart;
}

static bool Host_CheckStartGameAcks(void* connected)
{
	int* connectedPlayers = (int*)connected;

	sockaddr_in from;
	while (GetConnection(from))
	{
		if (NetBuffer[0] == NCMD_EXIT)
		{
			if (RemoteClient > 0)
			{
				RemoveClientConnection(RemoteClient, "exit packet while waiting for start acknowledgement");
				--*connectedPlayers;
				I_NetUpdatePlayers(*connectedPlayers, MaxClients);
			}
			continue;
		}

		if (NetBuffer[0] != NCMD_SETUP || NetBuffer[1] != PRE_HCDE_SERVICE)
			continue;
		if (RemoteClient <= 0 || !Connected[RemoteClient].bHCDEConnect)
		{
			DebugTrace::Markf("net", "ignored HCDE start-ack packet from unnegotiated client %d", RemoteClient);
			continue;
		}

		const auto service = EHCDEPregameService(NetBuffer[2]);
		switch (service)
		{
		case HPS_START_GAME_ACK:
			if (!CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host service start ack"))
				break;
			if (!Connected[RemoteClient].bHasStartGameAck)
				I_NetLog("Client %d acknowledged HCDE service start", RemoteClient);
			Connected[RemoteClient].bHasStartGameAck = true;
			break;
		case HPS_HEARTBEAT:
			CheckHCDEPregameService(RemoteClient, HCDEServiceHeaderSize, "host service start heartbeat");
			break;
		default:
			DebugTrace::Markf("net", "ignored HCDE service %u while waiting for start ack", unsigned(NetBuffer[2]));
			break;
		}
	}

	bool allAcked = true;
	int acknowledged = 1;
	for (int client = 1; client < MaxClients; ++client)
	{
		auto& con = Connected[client];
		if (con.Status == CSTAT_NONE)
			continue;
		if (DropClientForHCDETimeout(client, connectedPlayers, "start acknowledgement"))
			continue;

		FlushHCDEReliableServices(con.Address, con);
		if (!con.bHasStartGameAck)
		{
			allAcked = false;
			continue;
		}
		++acknowledged;
	}
	I_NetUpdatePlayers(acknowledged, *connectedPlayers);
	return allAcked;
}

static void SendAbort()
{
	NetBuffer[0] = NCMD_EXIT;

	if (consoleplayer == 0)
	{
		// Authority-side abort (host == authority pre-game). The receiver's
		// `GetNetBufferSize()` returns `1 + I_IsHCDEServiceAuthoritySlot(sender)`
		// for `NCMD_EXIT`, so an authority must always emit 2 bytes or the
		// peer's `HGetPacket()` will treat the size as a mismatch and drop
		// the packet -- leaving guests stuck waiting on a host that is
		// actually tearing down. Mirror `D_QuitNetGame`'s authority path and
		// include a (zero) next-authority placeholder.
		NetBuffer[1] = 0u;
		NetBufferLength = 2u;
		for (int client = 1; client < MaxClients; ++client)
		{
			if (Connected[client].Status != CSTAT_NONE)
				SendPacket(Connected[client].Address);
		}
	}
	else
	{
		// Guest abort: receiver is the authority, so it expects 1 byte.
		NetBufferLength = 1u;
		SendPacket(Connected[0].Address);
	}
}

static bool HostGame(int arg)
{
	DebugTrace::Markf("net", "host request arg=%d", arg);
	DedicatedServerStartRequested = false;
	if (DedicatedServerAbortRequested)
	{
		DebugTrace::Mark("net", "host request cancelled before network start");
		throw CExitEvent(0);
	}
	int requestedClients = 0;
	if (arg < Args->NumArgs())
	{
		const char* rawClientCount = Args->GetArg(arg);
		if (!TryParseStrictInt(rawClientCount, requestedClients))
		{
			DebugTrace::Warningf("net", "invalid host client count '%s', using default", rawClientCount != nullptr ? rawClientCount : "<null>");
			requestedClients = 0;
		}
	}
	if (DedicatedServerMode)
	{
		// A standalone dedicated server's real playable capacity is the larger
		// of the explicit -server count and the configured sv_maxplayers. This
		// lets an operator open late-join co-op slots purely through the server
		// config (sv_maxplayers) without passing a client count on the command
		// line, and stops a small "-server N" from permanently capping the
		// roster below the configured maximum. sv_maxplayers is a CVAR_ARCHIVE
		// server setting restored by GameConfig->DoGameSetup() before this runs.
		int playableSlots = requestedClients > 0 ? requestedClients : 0;
		if (*sv_maxplayers > 0)
			playableSlots = max<int>(playableSlots, *sv_maxplayers);
		if (playableSlots <= 0)
			playableSlots = 1;
		if ((unsigned)playableSlots >= MAXPLAYERS)
			I_FatalError("Cannot host a dedicated game with %u client slots. The limit is currently %lu", playableSlots, MAXPLAYERS - 1u);
		MaxClients = playableSlots + 1;
	}
	else if (!(MaxClients = requestedClients))
	{	// No player count specified, assume 2
		MaxClients = 2u;
	}

	if ((unsigned)MaxClients > MAXPLAYERS)
		I_FatalError("Cannot host a game with %u players. The limit is currently %lu", MaxClients, MAXPLAYERS);

	// Startup breadcrumbs. These are intentionally fine-grained because the
	// dedicated-server host path has crashed at startup on some Linux runtimes
	// (Debian 13) in a way that does not reproduce under a debugger. Combined
	// with HCDE_TRACE_FLUSH_ALWAYS=1 (which fsyncs every trace line), the last
	// breadcrumb in the stream file localizes the exact failing call.
	DebugTrace::Markf("net", "host slots resolved max=%d consoleplayer=%d", MaxClients, consoleplayer);

	HCDE_ServerMode_SetNetworkDetails(I_GetVisibleMaxClients(), MaxClients, GamePort, DedicatedServerMode, DedicatedServerMode ? "server-init" : "host-init");

	GenerateGameID();
	DebugTrace::Mark("net", "host gameid generated");
	NetworkClients += 0;
	Connected[consoleplayer].Status = CSTAT_READY;
	Net_SetupUserInfo();
	DebugTrace::Mark("net", "host userinfo ready");

	// If only 1 player, don't bother starting the network
	if (MaxClients == 1)
	{
		TicDup = 1u;
		multiplayer = true;
		return true;
	}

	DebugTrace::Markf("net", "host starting network port=%u", static_cast<unsigned>(GamePort));
	StartNetwork(false);
	DebugTrace::Markf("net", "host network ready port=%u", static_cast<unsigned>(GamePort));
	SV_InitMasters();
	DebugTrace::Mark("net", "host masters initialized");
	HCDE_ServerMode_SetMasterState(SV_IsMasterAdvertisingEnabled(), SV_GetMasterCount());
	HCDE_ServerMode_SetNetworkDetails(I_GetVisibleMaxClients(), MaxClients, GamePort, DedicatedServerMode, DedicatedServerMode ? "server-waiting" : "host-waiting");
	HCDE_ServerMode_PrintDiagnostics(DedicatedServerMode ? "dedicated-host" : "host");
	SetConnectFlow(NCF_SERVER_WAITING);
	I_NetInit(DedicatedServerMode ? "Starting dedicated server..." : "Hosting game...", true);
	I_NetUpdatePlayers(1, MaxClients);
	I_NetClientConnected(0u, 16u);
	I_NetMessage(DedicatedServerMode ? "Dedicated server accepting clients" : "Waiting for players");

	// Wait for the session to be full.
	int connectedPlayers = 1;
	if (!I_NetLoop(Host_CheckForConnections, (void*)&connectedPlayers))
	{
		SendAbort();
		SV_ShutdownMasters();
		DebugTrace::Mark("net", "host session aborted");
		throw CExitEvent(0);
	}

	// Now go
	SetConnectFlow(NCF_SYNCING);
	HCDE_ServerMode_SetNetworkDetails(I_GetVisibleMaxClients(), MaxClients, GamePort, DedicatedServerMode, DedicatedServerMode ? "server-syncing" : "syncing");
	I_NetMessage(DedicatedServerMode ? "Starting dedicated game" : "Starting game");

	// If the player force started with only themselves in the session, start the session
	// immediately.
	if (connectedPlayers == 1)
	{
		I_NetDone();
		CloseNetwork();
		MaxClients = 1;
		TicDup = 1u;
		return true;
	}

	I_NetLog("Go");

	for (size_t client = 1u; client < (size_t)MaxClients; ++client)
	{
		if (Connected[client].Status != CSTAT_NONE)
		{
			if (BeginReliableHCDEPregameService(HPS_START_GAME, Connected[client], 0u))
				CommitReliableHCDEPregameService(Connected[client].Address, Connected[client], HPS_START_GAME, 0u);
		}
	}

	if (!I_NetLoop(Host_CheckStartGameAcks, (void*)&connectedPlayers))
	{
		SendAbort();
		SV_ShutdownMasters();
		DebugTrace::Mark("net", "host start acknowledgement wait aborted");
		throw CExitEvent(0);
	}

	I_NetDone();
	I_NetLog("Total players: %d", I_GetReservedServerSlot() >= 0 ? max(connectedPlayers - 1, 0) : connectedPlayers);

	return true;
}

uint16_t I_GetGamePort()
{
	return GamePort;
}

static FString ReadVerificationError(TArrayView<uint8_t> stream)
{
	if (stream.Size() < 5u)
		return "Unknown error";

	if (stream[0] == FVerificationError::VE_ENGINE)
	{
		if (stream.Size() < 7u)
			return "Unknown error";

		return FStringf("Engine mismatch: host expected %d.%d.%d, got %d.%d.%d",
						stream[1], stream[2], stream[3], stream[4], stream[5], stream[6]);
	}

	TMap<FString, FString> files = {};
	for (size_t i = 0u; i < fileSystem.GetNumWads(); ++i)
	{
		if (!fileSystem.IsOptionalResource(i))
		{
			const FString crc = fileSystem.GetResourceHash(i);
			FString name = fileSystem.GetResourceFileName(i);
			FixPathSeperator(name);
			auto a = name.Split('/', FString::TOK_SKIPEMPTY);
			files[crc] = a.Last();
		}
	}

	const size_t size = (stream[1] << 24) | (stream[2] << 16) | (stream[3] << 8) | stream[4];
	size_t offset = 5;
	if (stream[0] == FVerificationError::VE_FILE_UNKNOWN)
	{
		FString er = "Host rejected extra or unknown files:";
		if (size == 0u)
			return "Host rejected extra or unknown files, but did not identify which ones. Check for duplicate IWAD/PWAD entries.";
		for (size_t i = 0; i < size; ++i)
		{
			FString crc = {};
			if (!ReadQueryString(stream.Data(), offset, stream.Size(), crc))
				return "Unknown error";

			auto file = files.CheckKey(crc);
			if (file != nullptr)
				er.AppendFormat("\n* %s (hash %s)", file->GetChars(), crc.GetChars());
			else
				er.AppendFormat("\n* <? Unknown file ?> (hash %s)", crc.GetChars());
		}
		er.Append("\nEnsure every player copied the same build output folder (for example build\\RelWithDebInfo) with matching hcde.exe, hcdeserv.exe, and hcde.pk3.");
		return er;
	}
	else if (stream[0] == FVerificationError::VE_FILE_ORDER)
	{
		FString er = "Wrong file order. Expected:";
		for (size_t i = 0; i < size; ++i)
		{
			FString crc = {};
			if (!ReadQueryString(stream.Data(), offset, stream.Size(), crc))
				return "Unknown error";

			auto file = files.CheckKey(crc);
			if (file != nullptr)
				er.AppendFormat("\n* %s", file->GetChars());
			else
				er.AppendFormat("\n* <? Unknown file ?>");
		}
		return er;
	}
	else if (stream[0] == FVerificationError::VE_FILE_MISSING)
	{
		FString er = "Host was expecting missing files:";
		for (size_t i = 0; i < size; ++i)
		{
			FString file = {};
			if (!ReadQueryString(stream.Data(), offset, stream.Size(), file))
				return "Unknown error";

			er.AppendFormat("\n* %s", file.GetChars());
		}
		return er;
	}

	return "Unknown error";
}

static bool Guest_ContactHost(void* unused)
{
	// Listen for a reply.
	const size_t addrSize = sizeof(sockaddr_in);
	sockaddr_in from;
	while (GetConnection(from))
	{
		const size_t msgSize = NetBufferLength;

		if (RemoteClient != 0)
			continue;

		if (NetBuffer[0] == NCMD_EXIT)
			I_NetError("The host cancelled the game");

		if (NetBuffer[0] != NCMD_SETUP)
			continue;

		if (NetBuffer[1] == PRE_DISCONNECT)
		{
			if (NetBufferLength < 7u || !CheckSessionToken(Connected[0], ReadSessionToken(NetBuffer, 3u), "host disconnect"))
				continue;

			const int disconnectedClient = NetBuffer[2];
			if (disconnectedClient < 0 || disconnectedClient >= MaxClients || disconnectedClient >= MAXPLAYERS)
			{
				DebugTrace::Markf("net", "ignored invalid disconnect slot=%d max=%d", disconnectedClient, MaxClients);
				continue;
			}
			I_ClearClient(disconnectedClient);
			NetworkClients -= disconnectedClient;
			SetClientAck(consoleplayer, disconnectedClient, false);
			I_NetClientDisconnected(disconnectedClient, "host reported disconnect");
		}
		else if (NetBuffer[1] == PRE_FULL)
		{
			I_NetError("The game is full");
		}
		else if (NetBuffer[1] == PRE_IN_PROGRESS)
		{
			if (DedicatedJoinMode && DedicatedLateJoinRetryPendingSend)
			{
				// Ignore duplicate in-progress replies until the retry connect
				// packet is actually emitted at the end of this loop tick.
				DebugTrace::Mark("net", "ignoring PRE_IN_PROGRESS while dedicated late-join retry is pending send");
				continue;
			}
			if (!DedicatedJoinMode && !DedicatedLateJoinRetryAttempted)
			{
				// Dedicated late-join fallback: if a server is already mid-match,
				// retry once with explicit dedicated-join service flags so launchers
				// that used plain -join can still attach to dedicated runtime sessions.
				DedicatedLateJoinRetryAttempted = true;
				DedicatedLateJoinRetryPendingSend = true;
				DedicatedJoinMode = true;
				Connected[0].SessionToken = 0u;
				Connected[0].bHCDEConnect = false;
				Connected[0].HCDEConnectVersion = 0u;
				Connected[0].HCDEConnectFlags = 0u;
				I_NetMessage("Server is mid-match. Retrying dedicated late join...");
				I_NetLog("Retrying connect with dedicated late-join flags after PRE_IN_PROGRESS");
				DebugTrace::Mark("net", "guest connect fallback: retrying as dedicated late-join");
				continue;
			}
			if (DedicatedJoinMode)
			{
				I_NetError("The dedicated server rejected late join. The server may be running an older build.");
			}
			else
			{
				I_NetError("The game has already started");
			}
		}
		else if (NetBuffer[1] == PRE_WRONG_PASSWORD)
		{
			I_NetError("Invalid password");
		}
		else if (NetBuffer[1] == PRE_VERIFICATION_ERROR)
		{
			if (NetBufferLength < 3u)
			{
				I_NetError("Malformed verification error response");
				continue;
			}

			I_NetError(ReadVerificationError(TArrayView{ &NetBuffer[2], static_cast<unsigned>(NetBufferLength - 2u) }).GetChars());
		}
		else if (NetBuffer[1] == PRE_KICKED)
		{
			I_NetError("You have been kicked from the game");
		}
		else if (NetBuffer[1] == PRE_BANNED)
		{
			I_NetError("You have been banned from the game");
		}
		else if (NetBuffer[1] == PRE_PROTOCOL_ERROR)
		{
			I_NetError("The server rejected HCDE service protocol negotiation");
		}
		else if (NetBuffer[1] == PRE_SETUP_TIMEOUT)
		{
			I_NetError("The server timed out during HCDE setup");
		}
		else if (NetBuffer[1] == PRE_CONNECT_ACK)
		{
			if (consoleplayer == -1)
			{
				if (msgSize < 9)
					continue;

				Connected[0].SessionToken = ReadSessionToken(NetBuffer, 5u);
				DedicatedLateJoinRetryPendingSend = false;
				const uint8_t ackFlags = msgSize >= 10 ? NetBuffer[9] : 0u;
				if ((ackFlags & PRE_CONNECT_ACK_DEDICATED) != 0)
				{
					DedicatedJoinMode = true;
				}
				const bool serviceConnect = (ackFlags & PRE_CONNECT_ACK_HCDE_SERVICE) != 0;
				if (serviceConnect)
				{
					if (msgSize < 12)
						I_NetError("Malformed HCDE service connect acknowledgement");
					if (NetBuffer[10] != HCDEConnectProtocolVersion)
						I_NetError("Unsupported HCDE service connect version from server");

					Connected[0].bHCDEConnect = true;
					Connected[0].HCDEConnectVersion = NetBuffer[10];
					Connected[0].HCDEConnectFlags = NetBuffer[11];
					Printf("NetSession:: HCDE service connect negotiated v%u flags=0x%02x\n", NetBuffer[10], NetBuffer[11]);
				}

				const int announcedMaxClients = NetBuffer[4];
				if (announcedMaxClients < 1 || announcedMaxClients > int(MAXPLAYERS))
				{
					DebugTrace::Markf("net", "ignored connect ack: invalid max-clients=%d", announcedMaxClients);
					continue;
				}
				MaxClients = announcedMaxClients;
				if (Connected[0].Status != CSTAT_WAITING)
				{
					NetworkClients += 0;
					Connected[0].Status = CSTAT_WAITING;
					I_NetClientUpdated(0);
				}
				I_NetUpdatePlayers(NetBuffer[3], MaxClients);
				// HCDE is HCDE-service only. A connect acknowledgement that does
				// not carry the service flag means the host is not an HCDE server
				// (or is an incompatible build), so there is no legacy mesh
				// handshake to fall back to -- fail with a clear, retryable error
				// instead of silently stalling.
				if (!serviceConnect)
					I_NetError("Server did not negotiate HCDE service. The host is not running HCDE or is an incompatible build.");

				I_NetMessage("Waiting for server assignment");
			}
		}
		else if (NetBuffer[1] == PRE_HCDE_SERVICE)
		{
			if (!Connected[0].bHCDEConnect)
			{
				DebugTrace::Markf("net", "ignored HCDE service packet before negotiation");
				continue;
			}
			if (NetBufferLength < HCDEServiceHeaderSize)
			{
				DebugTrace::Markf("net", "guest HCDE service packet too short len=%zu", NetBufferLength);
				continue;
			}

			const auto service = EHCDEPregameService(NetBuffer[2]);
			switch (service)
			{
			case HPS_CONSOLE_PLAYER:
			{
				if (!CheckHCDEPregameService(0u, HCDEServiceHeaderSize + 4u, "guest service console player"))
					break;

				const int assigned = NetBuffer[HCDEServiceHeaderSize];
				const int connectedPlayers = NetBuffer[HCDEServiceHeaderSize + 1u];
				const int announcedMaxClients = NetBuffer[HCDEServiceHeaderSize + 2u];
				const int firstPlayable = I_GetFirstPlayableClientSlot();
				if (assigned < firstPlayable || assigned >= announcedMaxClients || announcedMaxClients > int(MAXPLAYERS))
				{
					DebugTrace::Markf("net", "ignored invalid HCDE console player assignment slot=%d max=%d", assigned, announcedMaxClients);
					break;
				}

				MaxClients = announcedMaxClients;
				if (consoleplayer == -1)
				{
					consoleplayer = assigned;
					NetworkClients += consoleplayer;
					Connected[consoleplayer].Status = CSTAT_CONNECTING;
					Connected[consoleplayer].SessionToken = Connected[0].SessionToken;
					Connected[consoleplayer].bHCDEConnect = true;
					Connected[consoleplayer].HCDEConnectVersion = Connected[0].HCDEConnectVersion;
					Connected[consoleplayer].HCDEConnectFlags = Connected[0].HCDEConnectFlags | NetBuffer[HCDEServiceHeaderSize + 3u];
					Net_SetupUserInfo();

					I_NetMessage("Sending player information");
					I_NetLog("Received HCDE console player %d", consoleplayer);
					I_NetClientConnected(consoleplayer, 16u);
				}
				I_NetUpdatePlayers(connectedPlayers, MaxClients);
				break;
			}
			case HPS_HEARTBEAT:
				if (consoleplayer < 0)
					break;
				if (!CheckHCDEPregameService(0u, HCDEServiceHeaderSize + 2u, "guest service heartbeat"))
					break;
				{
					const int announcedMaxClients = NetBuffer[HCDEServiceHeaderSize + 1u];
					if (announcedMaxClients < 1 || announcedMaxClients > int(MAXPLAYERS))
					{
						DebugTrace::Markf("net", "ignored HCDE service heartbeat: invalid max-clients=%d", announcedMaxClients);
						break;
					}
					MaxClients = announcedMaxClients;
					I_NetUpdatePlayers(NetBuffer[HCDEServiceHeaderSize], MaxClients);
				}
				break;
			case HPS_USER_INFO_ACK:
				if (consoleplayer < 0)
					break;
				if (!CheckHCDEPregameService(0u, HCDEServiceHeaderSize + 1u, "guest service userinfo ack"))
					break;

				if (NetBuffer[HCDEServiceHeaderSize] == consoleplayer)
					SetClientAck(consoleplayer, consoleplayer, true);
				if (Connected[consoleplayer].Status == CSTAT_CONNECTING)
				{
					Connected[consoleplayer].Status = CSTAT_WAITING;
					I_NetClientUpdated(consoleplayer);
					I_NetMessage("Waiting for server start");
				}

				if (BeginReliableHCDEPregameService(HPS_USER_INFO_ACK, Connected[0], uint8_t(consoleplayer)))
				{
					NetBuffer[HCDEServiceHeaderSize] = uint8_t(consoleplayer);
					NetBufferLength = HCDEServiceHeaderSize + 1u;
					CommitReliableHCDEPregameService(from, Connected[0], HPS_USER_INFO_ACK, uint8_t(consoleplayer));
				}
				break;
			case HPS_MAP_LOAD:
				if (consoleplayer < 0)
					break;
				if (!CheckHCDEPregameService(0u, HCDEServiceHeaderSize, "guest service mapload"))
					break;
				if (!Connected[consoleplayer].bHasMapLoadInfo)
				{
					TArrayView<uint8_t> stream = TArrayView(&NetBuffer[HCDEServiceHeaderSize], MAX_MSGLEN - HCDEServiceHeaderSize);
					Net_ReadMapLoadInfo(stream);
					Connected[consoleplayer].bHasMapLoadInfo = true;
					I_NetLog("Received HCDE map load");
				}

				if (BeginReliableHCDEPregameService(HPS_MAP_LOAD_ACK, Connected[0], 0u))
					CommitReliableHCDEPregameService(from, Connected[0], HPS_MAP_LOAD_ACK, 0u);
				break;
			case HPS_GAME_INFO:
				if (consoleplayer < 0)
					break;
				if (!CheckHCDEPregameService(0u, HCDEServiceHeaderSize + 9u, "guest service gameinfo"))
					break;
				if (!Connected[consoleplayer].bHasGameInfo)
				{
					TicDup = clamp<int>(NetBuffer[HCDEServiceHeaderSize], 1, MAXTICDUP);
					memcpy(GameID, &NetBuffer[HCDEServiceHeaderSize + 1u], 8);
					const size_t payloadOffset = HCDEServiceHeaderSize + 9u;
					const size_t payloadSize = NetBufferLength > payloadOffset ? NetBufferLength - payloadOffset : 0u;
					size_t streamSize = payloadSize;
					if (payloadSize > 0u && payloadOffset + payloadSize < MAX_MSGLEN)
					{
						NetBuffer[payloadOffset + payloadSize] = 0u;
						streamSize = payloadSize + 1u;
					}
					TArrayView<uint8_t> stream = TArrayView(&NetBuffer[payloadOffset], streamSize);
					Net_ReadServerInfo(stream);
					Connected[consoleplayer].bHasGameInfo = true;
					I_NetLog("Received HCDE server info");
				}

				if (BeginReliableHCDEPregameService(HPS_GAME_INFO_ACK, Connected[0], 0u))
					CommitReliableHCDEPregameService(from, Connected[0], HPS_GAME_INFO_ACK, 0u);
				break;
			case HPS_ROSTER:
			{
				if (consoleplayer < 0)
					break;
				if (!CheckHCDEPregameService(0u, HCDEServiceHeaderSize + 1u, "guest service roster"))
					break;

				size_t cursor = HCDEServiceHeaderSize;
				const uint8_t rosterCount = NetBuffer[cursor++];
				bool rosterOk = true;
				for (uint8_t entry = 0u; entry < rosterCount; ++entry)
				{
					if (cursor >= NetBufferLength)
					{
						DebugTrace::Markf("net", "HCDE roster truncated before entry=%u count=%u", unsigned(entry), unsigned(rosterCount));
						rosterOk = false;
						break;
					}
					const int c = NetBuffer[cursor++];
					if (c < 0 || c >= MaxClients || c >= MAXPLAYERS)
					{
						DebugTrace::Markf("net", "ignored HCDE roster entry for invalid client %d max=%d", c, MaxClients);
						rosterOk = false;
						break;
					}
					if (c > 0)
					{
						if (NetBufferLength < cursor + addrSize)
						{
							DebugTrace::Markf("net", "HCDE roster entry missing address for client %d", c);
							rosterOk = false;
							break;
						}
						Connected[c].Status = CSTAT_WAITING;
						memcpy(&Connected[c].Address, &NetBuffer[cursor], addrSize);
						cursor += addrSize;
					}
					else
					{
						Connected[c].Status = CSTAT_READY;
					}

					if (NetBufferLength < cursor + 2u)
					{
						DebugTrace::Markf("net", "HCDE roster entry missing userinfo length for client %d", c);
						rosterOk = false;
						break;
					}
					const size_t infoSize = ReadBE16(&NetBuffer[cursor]);
					cursor += 2u;
					if (NetBufferLength < cursor + infoSize)
					{
						DebugTrace::Markf("net", "HCDE roster entry short userinfo for client %d len=%zu need=%zu", c, NetBufferLength - cursor, infoSize);
						rosterOk = false;
						break;
					}
					size_t streamSize = infoSize;
					if (infoSize > 0u && cursor + infoSize == NetBufferLength && cursor + infoSize < MAX_MSGLEN)
					{
						NetBuffer[cursor + infoSize] = 0u;
						streamSize = infoSize + 1u;
					}
					NetworkClients += c;
					TArrayView<uint8_t> stream = TArrayView(&NetBuffer[cursor], streamSize);
					Net_ReadUserInfo(c, stream);
					I_NetClientConnected(c, 16u);
					cursor += infoSize;
				}

				if (!rosterOk)
					break;
				Connected[consoleplayer].bHasRosterInfo = true;
				if (BeginReliableHCDEPregameService(HPS_ROSTER_ACK, Connected[0], 0u))
				{
					NetBufferLength = HCDEServiceHeaderSize;
					CommitReliableHCDEPregameService(from, Connected[0], HPS_ROSTER_ACK, 0u);
				}
				break;
			}
			case HPS_BOOTSTRAP_BEGIN:
				if (consoleplayer < 0)
					break;
				if (!CheckHCDEPregameService(0u, HCDEServiceHeaderSize + 13u, "guest service bootstrap begin"))
					break;
				Connected[consoleplayer].bHasBootstrapInfo = true;
				I_NetMessage("Receiving world bootstrap");
				DebugTrace::Markf("net", "guest bootstrap begin room=%u gametic=%u clienttic=%u consistency=%u",
					unsigned(NetBuffer[HCDEServiceHeaderSize]),
					unsigned(ReadBE32(&NetBuffer[HCDEServiceHeaderSize + 1u])),
					unsigned(ReadBE32(&NetBuffer[HCDEServiceHeaderSize + 5u])),
					unsigned(ReadBE32(&NetBuffer[HCDEServiceHeaderSize + 9u])));
				AckHCDEControlService(HPS_BOOTSTRAP_ACK);
				break;
			case HPS_START_GAME:
				if (consoleplayer < 0)
					break;
				if (!CheckHCDEPregameService(0u, HCDEServiceHeaderSize, "guest service start"))
					break;

				if (BeginReliableHCDEPregameService(HPS_START_GAME_ACK, Connected[0], 0u))
				{
					CommitReliableHCDEPregameService(from, Connected[0], HPS_START_GAME_ACK, 0u);
					FlushHCDEReliableServices(from, Connected[0], true);
				}
				I_NetMessage("Starting game");
				I_NetLog("Received HCDE service start");
				return true;
			default:
				++HCDEPregameServiceProfile.ServiceUnsupported;
				DebugTrace::Markf("net", "ignored unsupported guest HCDE service %u", unsigned(NetBuffer[2]));
				break;
			}
		}
	}

	// Stall watchdog for the HCDE join handshake. Progress is defined as the
	// reliable-service receive sequence advancing: every console-player,
	// user-info-ack, map-load, game-info, roster and (in a lobby)
	// heartbeat the host sends carries an incrementing sequence. As long as that
	// keeps moving the timer is rearmed, so a guest that has finished setup and
	// is merely waiting for the host to press start is never timed out. If the
	// sequence freezes for the timeout window the handshake is wedged (a setup
	// packet is being dropped on the path), so fail with a retryable error rather
	// than hang forever on "waiting for server start".
	if (Connected[0].bHCDEConnect)
	{
		const uint64_t now = I_msTime();
		const uint32_t rxSeq = Connected[0].HCDEServiceRxSeq;
		if (GuestHCDELastSetupProgressTime == 0u || rxSeq != GuestHCDELastSetupRxSeq)
		{
			GuestHCDELastSetupProgressTime = now;
			GuestHCDELastSetupRxSeq = rxSeq;
		}
		else if (now - GuestHCDELastSetupProgressTime >= HCDEGuestSetupProgressTimeoutMS)
		{
			DebugTrace::Warningf("net", "guest HCDE setup timed out waiting for progress last-rx=%u elapsed=%llu",
				GuestHCDELastSetupRxSeq,
				static_cast<unsigned long long>(now - GuestHCDELastSetupProgressTime));
			I_NetError("Timed out waiting for HCDE late-join setup. Try reconnecting.");
		}
	}

	if (Connected[0].bHCDEConnect)
		FlushHCDEReliableServices(Connected[0].Address, Connected[0]);

	NetBuffer[0] = NCMD_SETUP;
	if (consoleplayer == -1)
	{
		if (Connected[0].bHCDEConnect && Connected[0].SessionToken != 0u)
		{
			if (!HasPendingHCDEReliableService(Connected[0]))
			{
				BeginHCDEPregameService(HPS_HEARTBEAT, Connected[0]);
				SendPacket(Connected[0].Address);
			}
		}
		else
		{
			NetBuffer[1] = PRE_CONNECT;
			uint8_t* engineInfo = &NetBuffer[2];
			const size_t end = 2u + Net_SetEngineInfo(engineInfo);
			const size_t passSize = strlen(net_password) + 1;
			memcpy(&NetBuffer[end], net_password, passSize);
			NetBufferLength = end + passSize;
			// Phase 3 (UZDoom legacy removal): always advertise HCDE connect
			// info on outgoing PRE_CONNECT. The legacy gate
			// `DedicatedJoinMode || SilentNetStartMode` only emitted it on
			// dedicated-join / silent-launcher flows, which meant a plain
			// `-join` guest looked indistinguishable from a stock UZDoom/
			// ZDoom client and would now be rejected by the host's
			// HCDE-mandatory admission check. Every HCDE binary always
			// supports HCDE service, so we just always declare it.
			AppendHCDEConnectInfo(BuildLocalHCDEConnectFlags());
			SendPacket(Connected[0].Address);
			if (DedicatedLateJoinRetryPendingSend)
			{
				DedicatedLateJoinRetryPendingSend = false;
				DebugTrace::Mark("net", "dedicated late-join retry packet sent");
			}
		}
	}
	else
	{
		auto& con = Connected[consoleplayer];
		if (con.Status == CSTAT_CONNECTING)
		{
			if (BeginReliableHCDEPregameService(HPS_CLIENT_USER_INFO, Connected[0], uint8_t(consoleplayer)))
			{
				TArrayView<uint8_t> stream = TArrayView(&NetBuffer[NetBufferLength], MAX_MSGLEN - NetBufferLength);
				Net_SetUserInfo(consoleplayer, stream);
				NetBufferLength += stream.Data() - &NetBuffer[NetBufferLength];
				CommitReliableHCDEPregameService(Connected[0].Address, Connected[0], HPS_CLIENT_USER_INFO, uint8_t(consoleplayer));
			}
		}
		else if (con.Status == CSTAT_WAITING)
		{
			if (!HasPendingHCDEReliableService(Connected[0]))
			{
				BeginHCDEPregameService(HPS_HEARTBEAT, Connected[0]);
				SendPacket(Connected[0].Address);
			}
		}
	}

	return false;
}

static bool JoinGame(int arg)
{
	DebugTrace::Markf("net", "join request arg=%d", arg);
	if (arg >= Args->NumArgs()
		|| Args->GetArg(arg)[0] == '-' || Args->GetArg(arg)[0] == '+')
	{
		I_FatalError("You need to specify the host machine's address");
	}

	consoleplayer = -1;
	DedicatedLateJoinRetryAttempted = false;
	DedicatedLateJoinRetryPendingSend = false;
	GuestHCDELastSetupProgressTime = 0u;
	GuestHCDELastSetupRxSeq = 0u;
	// Fully reset the host connection slot before starting a new join. On a
	// fresh process Connected[0] is already zero-initialized, but an in-process
	// rejoin (joining a server via the menu after a prior session, or any second
	// JoinGame in the same run) would otherwise carry the previous session's
	// reliable-service state: a non-zero HCDEServiceRxSeq, leftover bHasGameInfo/
	// bHasMapLoadInfo/bHasRosterInfo/bHasStartGameAck flags, a stale SessionToken, and undrained
	// HCDEReliableServices[] entries. A stale RxSeq is fatal to the handshake:
	// the new host restarts its service sequence at 1, and the seq<=RxSeq guard
	// in CheckHCDEPregameService silently drops every console-player/map-load/
	// game-info as a "benign duplicate", so HCDEServiceRxSeq never advances and
	// the guest stalls at "Sending player information" until the setup watchdog
	// fires "Timed out waiting for HCDE late-join setup". Clear() restores the
	// fresh-connection defaults; Address/Status are re-populated immediately
	// below and the connect negotiation re-establishes SessionToken/bHCDEConnect.
	Connected[0].Clear();
	StartNetwork(true);
	DebugTrace::Markf("net", "join network ready port=%u", static_cast<unsigned>(GamePort));

	// Host is always client 0.
	BuildAddress(Connected[0].Address, Args->GetArg(arg));
	// Log the raw join argument alongside the address it actually resolved to.
	// This is the ground truth for "what did the client dial", independent of
	// what the launcher claims it passed: a LAN target here means a direct
	// connect, a WAN target that equals our own public IP means a hairpin path
	// (the historic stuck-rejoin cause). Pairs with the "sendto ok" trace.
	DebugTrace::Markf("net", "join target arg='%s' resolved=%s:%u",
		Args->GetArg(arg), inet_ntoa(Connected[0].Address.sin_addr),
		unsigned(ntohs(Connected[0].Address.sin_port)));
	Connected[0].Status = CSTAT_CONNECTING;

	SetConnectFlow(NCF_CLIENT_AUTH);
	I_NetInit("Joining server...", false);
	I_NetUpdatePlayers(0u, MaxClients);
	I_NetClientUpdated(0);

	if (!I_NetLoop(Guest_ContactHost, nullptr))
	{
		fprintf(stderr, "[netdiag] Join setup aborted before start dedicatedjoin=%d silent=%d consoleplayer=%d flow=%s\n",
			DedicatedJoinMode ? 1 : 0, SilentNetStartMode ? 1 : 0, consoleplayer, ConnectFlowName(NetConnectFlowState));
		fflush(stderr);
		Printf("NetSession:: Join setup aborted before start; sending exit packet (dedicatedjoin=%d silent=%d)\n",
			DedicatedJoinMode ? 1 : 0, SilentNetStartMode ? 1 : 0);
		DebugTrace::Warningf("net", "join setup aborted before start dedicatedjoin=%d silent=%d consoleplayer=%d flow=%s",
			DedicatedJoinMode ? 1 : 0, SilentNetStartMode ? 1 : 0, consoleplayer, ConnectFlowName(NetConnectFlowState));
		SendAbort();
		throw CExitEvent(0);
	}

	fprintf(stderr, "[netdiag] Join setup completed - proceeding to syncing phase\n");
	fflush(stderr);
	Printf("NetSession:: Join setup completed - proceeding to syncing phase\n");

	for (int i = 1u; i < MaxClients; ++i)
	{
		if (Connected[i].Status != CSTAT_NONE)
			Connected[i].Status = CSTAT_READY;
	}

	SetConnectFlow(NCF_SYNCING);
	HCDE_ServerMode_SetNetworkDetails(I_GetVisibleMaxClients(), MaxClients, GamePort, DedicatedJoinMode, DedicatedJoinMode ? "client-syncing" : "syncing");
	HCDE_ServerMode_PrintDiagnostics(DedicatedJoinMode ? "dedicated-join" : "join");
	I_NetLog("Total players: %d", I_GetVisibleMaxClients());
	I_NetDone();

	return true;
}

//
// I_InitNetwork
//
bool I_InitNetwork()
{
	HCDE_ServerMode_InitFromArgs();
	DedicatedServerMode = HCDE_ServerMode_IsDedicatedServer();
	DedicatedJoinMode = HCDE_ServerMode_IsDedicatedJoin();
	DedicatedLateJoinRetryAttempted = false;
	DedicatedLateJoinRetryPendingSend = false;
	GuestHCDELastSetupProgressTime = 0u;
	GuestHCDELastSetupRxSeq = 0u;
	// This controls only the pregame room/status UI. Dedicated join protocol
	// flags are still emitted through DedicatedJoinMode when the UI is visible.
	SilentNetStartMode = HCDE_ServerMode_ShouldSuppressRoomUI();

	// set up for network
	const char* v = Args->CheckValue(FArg_dup);
	if (v != nullptr)
	{
		int parsedDup = 0;
		if (TryParseStrictInt(v, parsedDup))
		{
			TicDup = clamp<int>(parsedDup, 1, MAXTICDUP);
		}
		else
		{
			DebugTrace::Warningf("net", "invalid -dup value '%s', keeping default", v);
		}
	}

	v = Args->CheckValue(FArg_port);
	if (v != nullptr)
	{
		int parsedPort = 0;
		if (TryParseStrictInt(v, parsedPort) && parsedPort > 0 && parsedPort <= 65535)
		{
			GamePort = parsedPort;
			Printf("Using alternate port %d\n", GamePort);
		}
		else
		{
			DebugTrace::Warningf("net", "invalid -port value '%s', keeping default %u", v, static_cast<unsigned>(GamePort));
		}
	}

	net_password = Args->CheckValue(FArg_password);

	// parse network game options,
	//		player 1: -host <numplayers>
	//		player 1: -server <numplayers> (dedicated server mode)
	//		player x: -join <player 1's address>
	int arg = -1;
	if (DedicatedServerMode && (arg = Args->CheckParm(FArg_server)))
	{
		if (!HostGame(arg + 1))
			return false;
	}
	else if ((arg = Args->CheckParm(FArg_host)))
	{
		if (!HostGame(arg + 1))
			return false;
	}
	else if ((arg = Args->CheckParm(FArg_join)))
	{
		if (!JoinGame(arg + 1))
			return false;
	}
	else if ((arg = Args->CheckParm(FArg_joindedicated)))
	{
		// Backwards-compatible spelling for older launchers. Keep this branch
		// functionally identical to -dedicatedjoin until the external launchers
		// that shipped it are no longer supported.
		DedicatedJoinMode = true;
		if (!JoinGame(arg + 1))
			return false;
	}
	else if ((arg = Args->CheckParm(FArg_dedicatedjoin)))
	{
		// Allow -dedicatedjoin <host> for direct CLI joins in addition to
		// the launcher-style -join <host> -dedicatedjoin combination.
		DedicatedJoinMode = true;
		if (!JoinGame(arg + 1))
			return false;
	}
	else
	{
		// single player game
		GenerateGameID();
		TicDup = 1;
		NetworkClients += 0;
		Connected[0].Status = CSTAT_READY;
		Net_SetupUserInfo();
	}

	bGameStarted = true;
	return true;
}

bool I_IsDedicatedServerMode()
{
	return DedicatedServerMode;
}

void I_DedicatedServerRequestStart()
{
	if (!DedicatedServerMode)
	{
		Printf("NetServer:: Start request ignored because this is not a dedicated server.\n");
		return;
	}
	DedicatedServerStartRequested = true;
}

void I_DedicatedServerRequestAbort()
{
	if (!DedicatedServerMode)
	{
		DedicatedServerAbortRequested = true;
		Printf("NetServer:: Stop request queued while dedicated server mode initializes.\n");
		return;
	}
	DedicatedServerAbortRequested = true;
}

bool I_UsesDedicatedServerSlot()
{
	return DedicatedServerMode || DedicatedJoinMode;
}

int I_GetReservedServerSlot()
{
	const int authoritySlot = I_GetHCDEServiceAuthoritySlot();
	if (authoritySlot < 0)
		return -1;

	// Dedicated server / dedicated late-join: the arbitrator slot is the
	// transport-only server endpoint, never a player.
	if (I_UsesDedicatedServerSlot())
		return authoritySlot;

	// Client view: HCDE's authority is always a dedicated (non-player) server,
	// so when the authority is a separate remote process (not the local player)
	// its slot is the server -- never a player -- even if the dedicated
	// connect-ack flag was not negotiated on this client. Without this the
	// client gives the server a real pawn at player start #1, counts it as a
	// second player, and the intermission ready vote sees humanParticipants==2
	// (the ready latch degrades into a toggle and the cutscene deadlocks).
	if (I_IsRemoteHCDEServiceAuthority(authoritySlot))
		return authoritySlot;

	return -1;
}

bool I_IsServerReservedSlot(int client)
{
	const int reservedSlot = I_GetReservedServerSlot();
	return reservedSlot >= 0 && client == reservedSlot;
}

int I_GetFirstPlayableClientSlot()
{
	const int reservedSlot = I_GetReservedServerSlot();
	return reservedSlot >= 0 ? reservedSlot + 1 : 0;
}

int I_GetVisibleMaxClients()
{
	return I_GetReservedServerSlot() >= 0 ? max(MaxClients - 1, 0) : MaxClients;
}

int I_ToVisibleClientSlot(int client)
{
	const int reservedSlot = I_GetReservedServerSlot();
	if (reservedSlot >= 0 && client > reservedSlot)
		return client - 1;
	return client;
}

int I_ToInternalClientSlot(int visibleClient)
{
	const int reservedSlot = I_GetReservedServerSlot();
	if (reservedSlot >= 0 && visibleClient >= reservedSlot)
		return visibleClient + 1;
	return visibleClient;
}

bool I_ClientUsesHCDEService(int client)
{
	return client >= 0 && client < MaxClients && Connected[client].bHCDEConnect;
}

int I_GetHCDEServiceAuthoritySlot()
{
	return I_UsesDedicatedServerSlot() && HCDE_ServerMode_HasAuthorityState()
		? HCDE_ServerMode_GetAuthoritySlot()
		: Net_Arbitrator;
}

bool I_IsLocalHCDEServiceAuthority()
{
	if (DedicatedServerMode && HCDE_ServerMode_HasAuthorityState() && !HCDE_ServerMode_IsAuthorityPlayerBacked())
		return true;

	return consoleplayer == I_GetHCDEServiceAuthoritySlot();
}

bool I_IsHCDEServiceAuthoritySlot(int client)
{
	return client >= 0 && client < MaxClients && client == I_GetHCDEServiceAuthoritySlot();
}

bool I_IsRemoteHCDEServiceAuthority(int client)
{
	return !I_IsLocalHCDEServiceAuthority() && I_IsHCDEServiceAuthoritySlot(client);
}

int I_GetHCDELiveAuthoritySlot()
{
	return I_GetHCDEServiceAuthoritySlot();
}

const FHCDEPregameServiceProfile& I_GetHCDEPregameServiceProfile()
{
	return HCDEPregameServiceProfile;
}

void I_ResetHCDEPregameServiceProfile()
{
	HCDEPregameServiceProfile.Clear();
}

int I_CountHCDEPregameServiceQuarantines()
{
	// Exposed to diagnostics so soak logs can show when the server is actively
	// shedding malformed setup/service traffic.
	const uint64_t now = I_msTime();
	int quarantined = 0;
	for (int client = 0; client < MAXPLAYERS; ++client)
	{
		if (Connected[client].HCDEServiceMalformedUntil > now)
			++quarantined;
	}
	return quarantined;
}

bool I_RequestHCDEResync(const char* reason)
{
	if (!netgame || demoplayback || I_IsLocalHCDEServiceAuthority())
		return false;
	if (!Connected[0].bHCDEConnect || Connected[0].Status == CSTAT_NONE)
		return false;
	if (!BeginReliableHCDEPregameService(HPS_RESYNC_REQUEST, Connected[0], 0u))
		return false;

	NetBufferLength = HCDEServiceHeaderSize;
	DebugTrace::Warningf("net", "guest requesting HCDE resync reason=%s", reason != nullptr ? reason : "manual");
	CommitReliableHCDEPregameService(Connected[0].Address, Connected[0], HPS_RESYNC_REQUEST, 0u);
	FlushHCDEReliableServices(Connected[0].Address, Connected[0], true);
	return true;
}

bool I_IsLocalHCDELiveAuthority()
{
	return I_IsLocalHCDEServiceAuthority();
}

bool I_IsHCDELiveAuthoritySlot(int client)
{
	return I_IsHCDEServiceAuthoritySlot(client);
}

bool I_IsHCDEClientSetupInProgress(int client)
{
	// True only on the authority while a (re)joining client slot is still
	// completing the fragile part of its pregame reliable handshake. A runtime
	// joiner is inserted into NetworkClients the instant its connect packet is
	// accepted, long before the handshake finishes, so callers that drive
	// live-game participation must consult this to avoid acting on a half-open
	// slot. Scoped to the authority: a guest evaluates I_IsLocalHCDEServiceAuthority
	// as false and returns early, so this never affects guest->authority routing
	// (the guest's view of the authority slot is not driven through these fields).
	if (!I_IsLocalHCDEServiceAuthority())
		return false;
	if (client < 0 || client >= MaxClients || client >= int(MAXPLAYERS))
		return false;
	auto& con = Connected[client];
	if (con.Status == CSTAT_NONE || !con.bHCDEConnect)
		return false;
	// Live participation opens at CSTAT_READY. By then every pregame reliable
	// service (console-player, user-info-ack, map-load, game-info, roster)
	// has been delivered and acked, so the joiner is out of the multi-step
	// user-info exchange that the live packet flood was wedging. We deliberately
	// do NOT also require the start-game ack: the guest enters the game the moment
	// it receives start-game (it does not wait for its own ack to be confirmed),
	// and once in the live loop it no longer re-acks retransmitted start-game
	// setup packets. Gating live acceptance/admission on that ack would strand a
	// guest whose start-game ack was lost -- it would be in-game while the
	// authority refused its input forever. READY is the correct, race-free line.
	return con.Status != CSTAT_READY;
}

static bool I_IsHCDELiveRoutablePeer(int client)
{
	if (client < 0
		|| client >= MaxClients
		|| client == consoleplayer
		|| !I_ClientUsesHCDEService(client))
		return false;

	// A runtime joiner must not participate in live traffic (server snapshots,
	// live control, client input -- send or accept) until its pregame handshake
	// has fully completed. Routing live packets to a still-handshaking slot
	// floods the joiner with data it cannot use yet and was observed to wedge
	// the handshake itself: the joiner never advances past its user-info
	// exchange while the authority drowns it with heartbeats and rejected
	// snapshots. Treat such a slot as non-routable until setup ends. This only
	// suppresses on the authority (see I_IsHCDEClientSetupInProgress).
	if (I_IsHCDEClientSetupInProgress(client))
		return false;

	return true;
}

bool I_ShouldSendHCDELiveControlTo(int client)
{
	if (!I_IsHCDELiveRoutablePeer(client))
		return false;

	return I_IsLocalHCDELiveAuthority()
		? !I_IsHCDELiveAuthoritySlot(client)
		: I_IsHCDELiveAuthoritySlot(client);
}

bool I_ShouldSendHCDELiveClientInputTo(int client)
{
	return I_IsHCDELiveRoutablePeer(client)
		&& !I_IsLocalHCDELiveAuthority()
		&& I_IsHCDELiveAuthoritySlot(client);
}

bool I_ShouldSendHCDELiveServerSnapshotTo(int client)
{
	return I_IsHCDELiveRoutablePeer(client)
		&& I_IsLocalHCDELiveAuthority()
		&& !I_IsHCDELiveAuthoritySlot(client);
}

bool I_ShouldAcceptHCDELiveClientInputFrom(int client)
{
	return I_IsHCDELiveRoutablePeer(client)
		&& I_IsLocalHCDELiveAuthority()
		&& !I_IsHCDELiveAuthoritySlot(client);
}

bool I_ShouldAcceptHCDELiveServerSnapshotFrom(int client)
{
	return I_IsHCDELiveRoutablePeer(client)
		&& !I_IsLocalHCDELiveAuthority()
		&& I_IsHCDELiveAuthoritySlot(client);
}

#ifdef _WIN32
const char* neterror()
{
	static char neterr[16];
	int			code;

	switch (code = WSAGetLastError()) {
		case WSAEACCES:				return "EACCES";
		case WSAEADDRINUSE:			return "EADDRINUSE";
		case WSAEADDRNOTAVAIL:		return "EADDRNOTAVAIL";
		case WSAEAFNOSUPPORT:		return "EAFNOSUPPORT";
		case WSAEALREADY:			return "EALREADY";
		case WSAECONNABORTED:		return "ECONNABORTED";
		case WSAECONNREFUSED:		return "ECONNREFUSED";
		case WSAECONNRESET:			return "ECONNRESET";
		case WSAEDESTADDRREQ:		return "EDESTADDRREQ";
		case WSAEFAULT:				return "EFAULT";
		case WSAEHOSTDOWN:			return "EHOSTDOWN";
		case WSAEHOSTUNREACH:		return "EHOSTUNREACH";
		case WSAEINPROGRESS:		return "EINPROGRESS";
		case WSAEINTR:				return "EINTR";
		case WSAEINVAL:				return "EINVAL";
		case WSAEISCONN:			return "EISCONN";
		case WSAEMFILE:				return "EMFILE";
		case WSAEMSGSIZE:			return "EMSGSIZE";
		case WSAENETDOWN:			return "ENETDOWN";
		case WSAENETRESET:			return "ENETRESET";
		case WSAENETUNREACH:		return "ENETUNREACH";
		case WSAENOBUFS:			return "ENOBUFS";
		case WSAENOPROTOOPT:		return "ENOPROTOOPT";
		case WSAENOTCONN:			return "ENOTCONN";
		case WSAENOTSOCK:			return "ENOTSOCK";
		case WSAEOPNOTSUPP:			return "EOPNOTSUPP";
		case WSAEPFNOSUPPORT:		return "EPFNOSUPPORT";
		case WSAEPROCLIM:			return "EPROCLIM";
		case WSAEPROTONOSUPPORT:	return "EPROTONOSUPPORT";
		case WSAEPROTOTYPE:			return "EPROTOTYPE";
		case WSAESHUTDOWN:			return "ESHUTDOWN";
		case WSAESOCKTNOSUPPORT:	return "ESOCKTNOSUPPORT";
		case WSAETIMEDOUT:			return "ETIMEDOUT";
		case WSAEWOULDBLOCK:		return "EWOULDBLOCK";
		case WSAHOST_NOT_FOUND:		return "HOST_NOT_FOUND";
		case WSANOTINITIALISED:		return "NOTINITIALISED";
		case WSANO_DATA:			return "NO_DATA";
		case WSANO_RECOVERY:		return "NO_RECOVERY";
		case WSASYSNOTREADY:		return "SYSNOTREADY";
		case WSATRY_AGAIN:			return "TRY_AGAIN";
		case WSAVERNOTSUPPORTED:	return "VERNOTSUPPORTED";
		case WSAEDISCON:			return "EDISCON";

		default:
			mysnprintf(neterr, countof(neterr), "%d", code);
			return neterr;
	}
}
#endif
