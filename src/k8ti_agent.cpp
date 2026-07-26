// K8ti agent — loopback HTTP sight/motor for her HCDE body.
// Player-honest FOV + LOS only. No aim assist.

#include "k8ti_agent.h"

#include "c_cvars.h"
#include "c_dispatch.h"
#include "cmdlib.h"
#include "d_player.h"
#include "d_protocol.h"
#include "doomstat.h"
#include "g_game.h"
#include "g_level.h"
#include "g_levellocals.h"
#include "m_argv.h"
#include "p_local.h"
#include "p_linetracedata.h"
#include "p_maputl.h"
#include "p_lnspec.h"
#include "p_spec.h"
#include "p_trace.h"
#include "a_keys.h"
#include "playsim/actor.h"
#include "printf.h"
#include "v_text.h"

#include <algorithm>
#include <cmath>
#include <cstring>
#include <errno.h>
#include <mutex>
#include <string>
#include <vector>

#ifdef _WIN32
#ifndef NOMINMAX
#define NOMINMAX
#endif
#include <winsock2.h>
#include <ws2tcpip.h>
using K8tiSock = SOCKET;
constexpr K8tiSock K8TI_INVALID = INVALID_SOCKET;
#else
#include <arpa/inet.h>
#include <fcntl.h>
#include <netinet/in.h>
#include <sys/socket.h>
#include <unistd.h>
using K8tiSock = int;
constexpr K8tiSock K8TI_INVALID = -1;
#endif

// Not archived — sticky ini true was locking human clients. Enable via launch argv only.
CVAR(Bool, k8ti_agent, false, 0)
CVAR(Int, k8ti_agent_port, 8794, CVAR_ARCHIVE | CVAR_GLOBALCONFIG)
CVAR(Float, k8ti_agent_fov, 90.f, CVAR_ARCHIVE | CVAR_GLOBALCONFIG)
CVAR(Float, k8ti_agent_max_turn_deg, 12.f, CVAR_ARCHIVE | CVAR_GLOBALCONFIG)

EXTERN_CVAR(Bool, sv_singleplayerrespawn)
EXTERN_CVAR(Int, deathmatch)

namespace
{
constexpr int K8TI_MAX_REQ = 8192;
constexpr int K8TI_MAX_ACTORS = 16;
constexpr int K8TI_MAX_USEABLES = 8;
constexpr double K8TI_USE_RANGE = 64.0;
// Automap-range awareness (same class of info as doors on the minimap).
constexpr double K8TI_USEABLE_RANGE = 220.0;
constexpr double K8TI_USEABLE_FOV = 110.0;
// Automap fog-edge scan (minimap-style navigation, not wallhack).
constexpr double K8TI_NAV_RANGE = 480.0;
constexpr double K8TI_NAV_EDGE = 170.0;

struct FCmdOverride
{
	bool Active = false;
	int TicsLeft = 0;
	double Forward = 0; // -1..1
	double Side = 0;
	double YawDeg = 0;   // remaining degrees to apply (signed)
	double PitchDeg = 0;
	bool Fire = false;
	bool Use = false;
	bool Jump = false;
	bool Respawn = false;
	bool HoldMove = false; // keep Forward/Side for HoldTicsLeft
	int HoldTicsLeft = 0;  // hard cap so she cannot cruise forever
	int WeaponSlot = -1; // 0..NUM_WEAPON_SLOTS-1 or -1
	std::string LastReject;
};

struct FUseable
{
	FString Kind;
	double YawDeg = 0;
	double Dist = 0;
	bool Locked = false;
};

struct FSeenActor
{
	FString Name;
	FString Kind;
	double YawDeg = 0;
	double PitchDeg = 0;
	double Dist = 0;
	bool Visible = true;
};

struct FTransport
{
	K8tiSock Listen = K8TI_INVALID;
	K8tiSock Client = K8TI_INVALID;
	int BoundPort = 0;
	bool Listening = false;
	char RecvBuf[K8TI_MAX_REQ]{};
	int RecvUsed = 0;
	uint64_t Requests = 0;
	int LosRejects = 0;
	int FovRejects = 0;
};

FTransport GTransport;
FCmdOverride GCmd;
std::mutex GCmdMutex;
std::vector<FString> GEvents;
std::mutex GEventsMutex;

bool CmdLineBool(const char* name, bool fallback)
{
	if (Args == nullptr)
		return fallback;
	for (int i = 1; i < Args->NumArgs() - 1; ++i)
	{
		const char* a = Args->GetArg(i);
		const char* b = Args->GetArg(i + 1);
		if (a == nullptr || b == nullptr)
			continue;
		FString plus;
		plus.Format("+%s", name);
		if (!stricmp(a, plus.GetChars()) || (!stricmp(a, "+set") && !stricmp(b, name) && i + 2 < Args->NumArgs()))
		{
			const char* val = (!stricmp(a, "+set")) ? Args->GetArg(i + 2) : b;
			return atoi(val) != 0;
		}
	}
	return fallback;
}

int CmdLineInt(const char* name, int fallback)
{
	if (Args == nullptr)
		return fallback;
	for (int i = 1; i < Args->NumArgs() - 1; ++i)
	{
		const char* a = Args->GetArg(i);
		const char* b = Args->GetArg(i + 1);
		if (a == nullptr || b == nullptr)
			continue;
		FString plus;
		plus.Format("+%s", name);
		if (!stricmp(a, plus.GetChars()) || (!stricmp(a, "+set") && !stricmp(b, name) && i + 2 < Args->NumArgs()))
		{
			const char* val = (!stricmp(a, "+set")) ? Args->GetArg(i + 2) : b;
			return atoi(val);
		}
	}
	return fallback;
}

bool EffectiveEnabled()
{
	// Exclusive seat is opt-in per process via launch argv only.
	// Do not fall back to the archived cvar — that locked Daniel's Doom Connector
	// client whenever a shared HCDE.json (or leftover ini) had k8ti_agent 1.
	if (Args == nullptr)
		return false;
	for (int i = 1; i < Args->NumArgs() - 1; ++i)
	{
		const char* a = Args->GetArg(i);
		const char* b = Args->GetArg(i + 1);
		if (a == nullptr || b == nullptr)
			continue;
		if (!stricmp(a, "+k8ti_agent"))
			return atoi(b) != 0;
		if (!stricmp(a, "+set") && !stricmp(b, "k8ti_agent") && i + 2 < Args->NumArgs())
		{
			const char* val = Args->GetArg(i + 2);
			return val != nullptr && atoi(val) != 0;
		}
	}
	return false;
}

int EffectivePort()
{
	return clamp(CmdLineInt("k8ti_agent_port", *k8ti_agent_port), 1, 65535);
}

void CloseSock(K8tiSock& s)
{
	if (s == K8TI_INVALID)
		return;
#ifdef _WIN32
	closesocket(s);
#else
	close(s);
#endif
	s = K8TI_INVALID;
}

void SetNonBlocking(K8tiSock s)
{
#ifdef _WIN32
	u_long mode = 1;
	ioctlsocket(s, FIONBIO, &mode);
#else
	int flags = fcntl(s, F_GETFL, 0);
	if (flags >= 0)
		fcntl(s, F_SETFL, flags | O_NONBLOCK);
#endif
}

bool EnsureWinsock()
{
#ifdef _WIN32
	static bool ok = false;
	if (!ok)
	{
		WSADATA data;
		if (WSAStartup(MAKEWORD(2, 2), &data) != 0)
			return false;
		ok = true;
	}
#endif
	return true;
}

void JsonEscape(const char* in, FString& out)
{
	out = "\"";
	if (in == nullptr)
	{
		out += "\"";
		return;
	}
	for (const char* p = in; *p; ++p)
	{
		char c = *p;
		if (c == '"' || c == '\\')
		{
			out += '\\';
			out += c;
		}
		else if (c == '\n')
			out += "\\n";
		else if (c == '\r')
			out += "\\r";
		else if ((unsigned char)c < 0x20)
			continue;
		else
			out += c;
	}
	out += "\"";
}

FString JsonStr(const char* s)
{
	FString out;
	JsonEscape(s, out);
	return out;
}

FString JsonStr(const FString& s)
{
	return JsonStr(s.GetChars());
}

double NormalizeDeg(double deg)
{
	while (deg > 180.0)
		deg -= 360.0;
	while (deg < -180.0)
		deg += 360.0;
	return deg;
}

const char* StateName(const player_t& p)
{
	switch (p.playerstate)
	{
	case PST_LIVE: return "live";
	case PST_DEAD: return "dead";
	case PST_REBORN: return "reborn";
	case PST_GONE: return "spectating";
	default: return "unknown";
	}
}

// Player-honest walk clearance: hitscan walls/geometry ahead (skip actors).
// Same info a human gets by bumping / seeing a wall — not a navmesh cheat.
double ProbeClearance(AActor* mo, double yawOffsetDeg, double maxDist = 256.0)
{
	if (mo == nullptr)
		return maxDist;
	FLineTraceData data{};
	const DAngle ang = mo->Angles.Yaw + DAngle::fromDeg(yawOffsetDeg);
	const double eyeZ = mo->Height * 0.55;
	const int hit = P_LineTrace(
		mo, ang, maxDist, nullAngle, TRF_THRUACTORS, eyeZ, 0.0, 0.0, &data);
	if (!hit)
		return maxDist;
	if (data.HitType == TRACE_HitNone)
		return maxDist;
	return clamp(data.Distance, 0.0, maxDist);
}

void AppendClearanceJson(AActor* mo, FString& json)
{
	json += "\"clearance\":{";
	if (mo == nullptr)
	{
		json += "\"forward\":256,\"left\":256,\"right\":256,\"forward_left\":256,\"forward_right\":256},";
		return;
	}
	const double fwd = ProbeClearance(mo, 0);
	const double left = ProbeClearance(mo, 90);
	const double right = ProbeClearance(mo, -90);
	const double fwdL = ProbeClearance(mo, 35);
	const double fwdR = ProbeClearance(mo, -35);
	json.AppendFormat(
		"\"forward\":%.1f,\"left\":%.1f,\"right\":%.1f,\"forward_left\":%.1f,\"forward_right\":%.1f},",
		fwd, left, right, fwdL, fwdR);
}

bool CanRespawnNow(const player_t& p, FLevelLocals* level, double& readyIn)
{
	readyIn = 0;
	if (p.playerstate != PST_DEAD)
		return false;
	const bool multi = netgame || multiplayer || *deathmatch != 0;
	const bool spOk = *sv_singleplayerrespawn;
	if (!multi && !spOk)
		return false;
	if (level == nullptr)
		return false;
	const int wait = p.respawn_time - level->time;
	if (wait > 0)
	{
		readyIn = wait / double(TICRATE);
		return false;
	}
	return true;
}

int PlayerArmorAmount(AActor* mo)
{
	if (mo == nullptr)
		return 0;
	AActor* armor = mo->FindInventory(NAME_BasicArmor, true);
	if (armor == nullptr)
		return 0;
	return armor->IntVar(NAME_Amount);
}

FString ClassifyActor(AActor* ac, AActor* self)
{
	if (ac == nullptr)
		return "decoration";
	if (ac->player != nullptr)
		return "player";
	if (ac->IsKindOf(NAME_Key))
		return "key";
	if (ac->IsKindOf(NAME_Weapon))
		return "weapon";
	if (ac->IsKindOf(NAME_Ammo))
		return "ammo";
	if (ac->IsKindOf(NAME_Health))
		return "health";
	if (ac->IsKindOf(NAME_Armor) || ac->IsKindOf(NAME_BasicArmorPickup) || ac->IsKindOf(NAME_BasicArmorBonus))
		return "armor";
	if (ac->flags & MF_SPECIAL)
		return "item";
	if ((ac->flags3 & MF3_ISMONSTER) || ((ac->flags & MF_SHOOTABLE) && ac->health > 0 && ac != self))
	{
		if (self != nullptr && ac->IsFriend(self))
			return "friendly";
		return "hostile";
	}
	return "decoration";
}

bool InFov(AActor* self, AActor* other, double fovDeg, double& yawDeg, double& pitchDeg, double& dist)
{
	if (self == nullptr || other == nullptr)
		return false;
	dist = self->Distance2D(other);
	const DAngle absYaw = self->AngleTo(other);
	yawDeg = NormalizeDeg((absYaw - self->Angles.Yaw).Degrees());
	const double dz = other->Z() + other->Height * 0.5 - (self->Z() + self->Height * 0.5);
	pitchDeg = NormalizeDeg(DAngle::fromRad(atan2(-dz, dist)).Degrees() - self->Angles.Pitch.Degrees());
	const double half = fovDeg * 0.5;
	return fabs(yawDeg) <= half;
}

void CollectVisible(AActor* self, bool debugView, std::vector<FSeenActor>& out, int& losRejects, int& fovRejects)
{
	out.clear();
	losRejects = 0;
	fovRejects = 0;
	if (self == nullptr || self->Level == nullptr)
		return;
	const double fov = clamp(double(*k8ti_agent_fov), 30.0, 180.0);
	auto it = self->Level->GetThinkerIterator<AActor>();
	AActor* ac;
	std::vector<FSeenActor> candidates;
	while ((ac = it.Next()) != nullptr)
	{
		if (ac == self || (ac->health <= 0 && !(ac->flags & MF_SPECIAL)))
			continue;
		if (ac->IsKindOf(NAME_PlayerPawn) && ac->player == nullptr)
			continue;
		FString kind = ClassifyActor(ac, self);
		if (kind.Compare("decoration") == 0 && !debugView)
			continue;
		double yaw = 0, pitch = 0, dist = 0;
		const bool inFov = InFov(self, ac, fov, yaw, pitch, dist);
		if (!inFov)
		{
			++fovRejects;
			if (!debugView)
				continue;
		}
		const bool sight = P_CheckSight(self, ac, 0);
		if (!sight)
		{
			++losRejects;
			if (!debugView)
				continue;
		}
		FSeenActor s;
		s.Name = ac->GetClass()->TypeName.GetChars();
		if (ac->player != nullptr)
		{
			FString pn = ac->player->userinfo.GetName();
			if (pn.IsNotEmpty())
				s.Name = pn;
		}
		s.Kind = kind;
		s.YawDeg = yaw;
		s.PitchDeg = pitch;
		s.Dist = dist;
		s.Visible = inFov && sight;
		candidates.push_back(s);
	}
	std::sort(candidates.begin(), candidates.end(), [](const FSeenActor& a, const FSeenActor& b) {
		if (a.Visible != b.Visible)
			return a.Visible > b.Visible;
		if (fabs(a.YawDeg) != fabs(b.YawDeg))
			return fabs(a.YawDeg) < fabs(b.YawDeg);
		return a.Dist < b.Dist;
	});
	for (size_t i = 0; i < candidates.size() && (int)out.size() < K8TI_MAX_ACTORS; ++i)
	{
		if (!debugView && !candidates[i].Visible)
			continue;
		out.push_back(candidates[i]);
	}
}

bool IsDoorSpecial(int special)
{
	switch (special)
	{
	case Door_Close:
	case Door_Open:
	case Door_Raise:
	case Door_LockedRaise:
	case Door_Animated:
	case Door_WaitRaise:
	case Door_WaitClose:
	case Door_CloseWaitOpen:
	case Door_AnimatedClose:
	case Generic_Door:
		return true;
	default:
		return false;
	}
}

bool IsExitSpecial(int special)
{
	return special == Exit_Normal || special == Exit_Secret;
}

bool DoorLineLocked(AActor* mo, const line_t* ld)
{
	if (mo == nullptr || ld == nullptr)
		return false;
	if (ld->special == Door_LockedRaise)
		return !P_CheckKeys(mo, ld->args[3], false, true);
	if (ld->special == Generic_Door)
		return !P_CheckKeys(mo, ld->args[4], false, true);
	return false;
}

bool DoorLineActivatable(AActor* mo, line_t* ld)
{
	if (mo == nullptr || ld == nullptr)
		return false;
	const int side = P_PointOnLineSide(mo->Pos().XY(), ld);
	return P_TestActivateLine(ld, mo, side, SPAC_Use)
		|| P_TestActivateLine(ld, mo, side, SPAC_Push)
		|| P_TestActivateLine(ld, mo, side, SPAC_UseThrough)
		|| P_TestActivateLine(ld, mo, side, SPAC_UseBack);
}

line_t* ProbeHitLine(AActor* mo, double yawOffsetDeg, double maxDist, double& hitDist)
{
	hitDist = maxDist;
	if (mo == nullptr)
		return nullptr;
	FLineTraceData data{};
	const DAngle ang = mo->Angles.Yaw + DAngle::fromDeg(yawOffsetDeg);
	const double eyeZ = mo->Height * 0.55;
	const int hit = P_LineTrace(
		mo, ang, maxDist, nullAngle, TRF_THRUACTORS, eyeZ, 0.0, 0.0, &data);
	if (!hit || data.HitType != TRACE_HitWall || data.HitLine == nullptr)
		return nullptr;
	hitDist = clamp(data.Distance, 0.0, maxDist);
	return data.HitLine;
}

void ConsiderUseableLine(AActor* self, line_t* ld, double yawDeg, double dist, std::vector<FUseable>& out)
{
	if (self == nullptr || ld == nullptr || ld->special == 0)
		return;
	const bool door = IsDoorSpecial(ld->special);
	const bool exitLine = IsExitSpecial(ld->special);
	if (!door && !exitLine)
		return;
	if (dist < 1.0 || dist > K8TI_USEABLE_RANGE)
		return;
	if (fabs(yawDeg) > K8TI_USEABLE_FOV * 0.5)
		return;

	const bool locked = door ? DoorLineLocked(self, ld) : false;
	if (door && !locked && !DoorLineActivatable(self, ld))
		return;
	if (exitLine && !DoorLineActivatable(self, ld))
		return;

	for (const auto& u : out)
	{
		if (fabs(u.YawDeg - yawDeg) < 3.0 && fabs(u.Dist - dist) < 12.0)
			return;
	}

	FUseable u;
	u.Kind = exitLine ? "exit" : "door";
	u.YawDeg = yawDeg;
	u.Dist = dist;
	u.Locked = locked;
	out.push_back(u);
}

void CollectUseables(AActor* self, std::vector<FUseable>& out)
{
	out.clear();
	if (self == nullptr || self->Level == nullptr)
		return;

	// Same class of info as doors/exits on the automap/minimap: mapped linedefs nearby.
	auto& lines = self->Level->lines;
	for (unsigned i = 0; i < lines.Size(); ++i)
	{
		line_t* ld = &lines[i];
		if (ld->special == 0 || !(ld->flags & ML_MAPPED))
			continue;
		if (!IsDoorSpecial(ld->special) && !IsExitSpecial(ld->special))
			continue;

		const DVector2 mid = (ld->v1->fPos() + ld->v2->fPos()) * 0.5;
		const DVector2 delta = mid - self->Pos().XY();
		const double dist = delta.Length();
		if (dist < 1.0 || dist > K8TI_USEABLE_RANGE)
			continue;
		const double yawDeg = NormalizeDeg((delta.Angle() - self->Angles.Yaw).Degrees());
		ConsiderUseableLine(self, ld, yawDeg, dist, out);
	}

	// Bumped / facing a door that isn't mapped yet still counts (she can see the wall).
	double hitDist = 0;
	for (double yawOff : {0.0, 18.0, -18.0})
	{
		line_t* hit = ProbeHitLine(self, yawOff, 96.0, hitDist);
		if (hit == nullptr)
			continue;
		const double yawDeg = NormalizeDeg(yawOff);
		ConsiderUseableLine(self, hit, yawDeg, hitDist, out);
	}

	std::sort(out.begin(), out.end(), [](const FUseable& a, const FUseable& b) {
		if (a.Locked != b.Locked)
			return a.Locked < b.Locked; // unlocked first
		if (fabs(a.YawDeg) != fabs(b.YawDeg))
			return fabs(a.YawDeg) < fabs(b.YawDeg);
		return a.Dist < b.Dist;
	});
	if ((int)out.size() > K8TI_MAX_USEABLES)
		out.resize(K8TI_MAX_USEABLES);
}

void AppendUseablesJson(const std::vector<FUseable>& useables, FString& json)
{
	json += "\"useables\":[";
	for (size_t i = 0; i < useables.size(); ++i)
	{
		if (i)
			json += ",";
		json += "{";
		json.AppendFormat(
			"\"kind\":%s,\"yaw_deg\":%.1f,\"dist\":%.1f,\"locked\":%s",
			JsonStr(useables[i].Kind).GetChars(),
			useables[i].YawDeg,
			useables[i].Dist,
			useables[i].Locked ? "true" : "false");
		json += "}";
	}
	json += "],";
}

// Minimap-style explore hint: bearing to the fog edge (unmapped line next to mapped geometry).
bool FindAutomapFrontier(AActor* self, double& yawDeg, double& distOut)
{
	yawDeg = 0;
	distOut = 0;
	if (self == nullptr || self->Level == nullptr)
		return false;

	std::vector<DVector2> mappedMids;
	mappedMids.reserve(256);
	auto& lines = self->Level->lines;
	for (unsigned i = 0; i < lines.Size(); ++i)
	{
		const line_t& ld = lines[i];
		if (ld.flags & ML_MAPPED)
			mappedMids.push_back((ld.v1->fPos() + ld.v2->fPos()) * 0.5);
	}
	if (mappedMids.empty())
		return false;

	const double edge2 = K8TI_NAV_EDGE * K8TI_NAV_EDGE;
	auto nearMapped = [&](const DVector2& pt) -> bool {
		for (const DVector2& m : mappedMids)
		{
			if ((m - pt).LengthSquared() <= edge2)
				return true;
		}
		return false;
	};

	bool found = false;
	double bestScore = 1e12;
	for (unsigned i = 0; i < lines.Size(); ++i)
	{
		line_t* ld = &lines[i];
		if (ld->flags & ML_MAPPED)
			continue;

		const DVector2 mid = (ld->v1->fPos() + ld->v2->fPos()) * 0.5;
		const DVector2 delta = mid - self->Pos().XY();
		const double dist = delta.Length();
		if (dist < 48.0 || dist > K8TI_NAV_RANGE)
			continue;
		if (!nearMapped(mid))
			continue;

		const double yaw = NormalizeDeg((delta.Angle() - self->Angles.Yaw).Degrees());
		// Prefer openings into new space; slight bias toward ahead (how you'd glance a minimap).
		double score = dist + fabs(yaw) * 1.2;
		if (ld->flags & ML_TWOSIDED)
			score -= 80.0;
		if (fabs(yaw) > 125.0)
			score += 40.0;
		if (score < bestScore)
		{
			bestScore = score;
			yawDeg = yaw;
			distOut = dist;
			found = true;
		}
	}
	return found;
}

void AppendMapNavJson(AActor* mo, FString& json)
{
	json += "\"map_nav\":{";
	double yaw = 0, dist = 0;
	if (FindAutomapFrontier(mo, yaw, dist))
	{
		json.AppendFormat(
			"\"frontier_yaw_deg\":%.1f,\"frontier_dist\":%.1f,\"hint\":\"unexplored\"",
			yaw, dist);
	}
	else
	{
		json += "\"hint\":\"none\"";
	}
	json += "},";
}

void AppendInventory(const player_t& p, FString& json)
{
	json += "\"weapons_owned\":[";
	bool first = true;
	if (p.mo != nullptr)
	{
		for (AActor* inv = p.mo->Inventory; inv != nullptr; inv = inv->Inventory)
		{
			if (!inv->IsKindOf(NAME_Weapon))
				continue;
			if (!first)
				json += ",";
			first = false;
			json += JsonStr(inv->GetClass()->TypeName.GetChars());
		}
	}
	json += "],\"keys\":[";
	first = true;
	if (p.mo != nullptr)
	{
		for (AActor* inv = p.mo->Inventory; inv != nullptr; inv = inv->Inventory)
		{
			if (!inv->IsKindOf(NAME_Key))
				continue;
			if (!first)
				json += ",";
			first = false;
			json += JsonStr(inv->GetClass()->TypeName.GetChars());
		}
	}
	json += "]";
}

FString BuildWorldJson(bool debugView)
{
	FString json = "{";
	json.AppendFormat("\"ok\":true,\"view\":%s,", debugView ? "\"debug\"" : "\"play\"");
	json.AppendFormat("\"tic\":%d,", gametic);

	if ((unsigned)consoleplayer >= MAXPLAYERS || !playeringame[consoleplayer])
	{
		json += "\"state\":\"no_player\"}";
		return json;
	}

	player_t& p = players[consoleplayer];
	AActor* mo = p.mo;
	FLevelLocals* level = (mo != nullptr) ? mo->Level : primaryLevel;

	const char* mode = "solo";
	if (netgame || multiplayer)
		mode = (*deathmatch != 0) ? "dm" : "coop";

	double readyIn = 0;
	const bool canRespawn = CanRespawnNow(p, level, readyIn);
	const char* respawnAction = canRespawn || (p.playerstate == PST_DEAD && readyIn > 0) ? "use" : nullptr;
	if (p.playerstate == PST_DEAD)
	{
		if (!(netgame || multiplayer || *deathmatch != 0 || *sv_singleplayerrespawn))
			respawnAction = nullptr;
	}

	json.AppendFormat("\"mode\":%s,", JsonStr(mode).GetChars());
	if (level != nullptr)
		json.AppendFormat("\"map\":%s,", JsonStr(level->MapName.GetChars()).GetChars());
	else
		json += "\"map\":\"\",";

	json.AppendFormat("\"state\":%s,", JsonStr(StateName(p)).GetChars());
	json.AppendFormat("\"can_respawn\":%s,", canRespawn ? "true" : "false");
	json.AppendFormat("\"respawn_ready_in_s\":%.2f,", readyIn);
	if (respawnAction != nullptr)
		json.AppendFormat("\"respawn_action\":%s,", JsonStr(respawnAction).GetChars());
	else
		json += "\"respawn_action\":null,";

	json += "\"self\":{";
	if (mo != nullptr)
	{
		json.AppendFormat("\"x\":%.2f,\"y\":%.2f,\"z\":%.2f,", mo->X(), mo->Y(), mo->Z());
		json.AppendFormat("\"angle\":%.2f,\"pitch\":%.2f,", mo->Angles.Yaw.Degrees(), mo->Angles.Pitch.Degrees());
	}
	json.AppendFormat("\"hp\":%d,\"armor\":%d,", mo ? mo->health : p.health, PlayerArmorAmount(mo));
	json.AppendFormat("\"alive\":%s,", p.playerstate == PST_LIVE ? "true" : "false");
	if (p.ReadyWeapon != nullptr)
		json.AppendFormat("\"weapon\":%s,", JsonStr(p.ReadyWeapon->GetClass()->TypeName.GetChars()).GetChars());
	else
		json += "\"weapon\":null,";
	AppendInventory(p, json);
	json += "},";
	AppendClearanceJson(mo, json);

	std::vector<FSeenActor> seen;
	int losR = 0, fovR = 0;
	CollectVisible(mo, debugView, seen, losR, fovR);
	GTransport.LosRejects = losR;
	GTransport.FovRejects = fovR;

	json += "\"actors\":[";
	for (size_t i = 0; i < seen.size(); ++i)
	{
		if (i)
			json += ",";
		json += "{";
		json.AppendFormat("\"name\":%s,\"kind\":%s,", JsonStr(seen[i].Name).GetChars(), JsonStr(seen[i].Kind).GetChars());
		json.AppendFormat("\"yaw_deg\":%.1f,\"pitch_deg\":%.1f,\"dist\":%.1f", seen[i].YawDeg, seen[i].PitchDeg, seen[i].Dist);
		if (debugView)
			json.AppendFormat(",\"visible\":%s", seen[i].Visible ? "true" : "false");
		json += "}";
	}
	json += "],";

	std::vector<FUseable> useables;
	CollectUseables(mo, useables);
	AppendUseablesJson(useables, json);
	AppendMapNavJson(mo, json);

	json += "\"events\":[";
	{
		std::lock_guard<std::mutex> lock(GEventsMutex);
		for (size_t i = 0; i < GEvents.size(); ++i)
		{
			if (i)
				json += ",";
			json += JsonStr(GEvents[i]);
		}
		GEvents.clear();
	}
	json += "]";

	if (debugView)
	{
		json.AppendFormat(",\"debug\":{\"los_rejects\":%d,\"fov_rejects\":%d,\"port\":%d,\"listening\":%s}",
			losR, fovR, GTransport.BoundPort, GTransport.Listening ? "true" : "false");
	}
	json += "}";
	return json;
}

FString BuildDebugSelfJson()
{
	FString json = "{";
	json.AppendFormat("\"ok\":true,\"enabled\":%s,\"listening\":%s,\"port\":%d,\"requests\":%llu,",
		EffectiveEnabled() ? "true" : "false",
		GTransport.Listening ? "true" : "false",
		GTransport.BoundPort,
		(unsigned long long)GTransport.Requests);
	json.AppendFormat("\"los_rejects\":%d,\"fov_rejects\":%d,", GTransport.LosRejects, GTransport.FovRejects);
	if ((unsigned)consoleplayer < MAXPLAYERS && playeringame[consoleplayer])
	{
		player_t& p = players[consoleplayer];
		json.AppendFormat("\"playerstate\":%s,\"hp\":%d", JsonStr(StateName(p)).GetChars(), p.health);
	}
	json += "}";
	return json;
}

void SendHttp(K8tiSock s, int code, const char* contentType, const FString& body)
{
	FString hdr;
	hdr.AppendFormat(
		"HTTP/1.1 %d OK\r\nContent-Type: %s\r\nContent-Length: %d\r\nConnection: close\r\nAccess-Control-Allow-Origin: *\r\n\r\n",
		code, contentType, body.Len());
	FString all = hdr + body;
#ifdef _WIN32
	send(s, all.GetChars(), (int)all.Len(), 0);
#else
	send(s, all.GetChars(), (size_t)all.Len(), 0);
#endif
}

void SendJson(K8tiSock s, int code, const FString& body)
{
	SendHttp(s, code, "application/json", body);
}

double ClampUnit(double v)
{
	return clamp(v, -1.0, 1.0);
}

FString HandleCmdBody(const char* body)
{
	// Minimal JSON field scrape (no full parser).
	auto findNum = [&](const char* key, double def) -> double {
		FString pat;
		pat.Format("\"%s\"", key);
		const char* p = strstr(body, pat.GetChars());
		if (p == nullptr)
			return def;
		p = strchr(p + pat.Len(), ':');
		if (p == nullptr)
			return def;
		return atof(p + 1);
	};
	auto findBool = [&](const char* key) -> bool {
		FString pat;
		pat.Format("\"%s\"", key);
		const char* p = strstr(body, pat.GetChars());
		if (p == nullptr)
			return false;
		return strstr(p, "true") != nullptr && (strstr(p, "true") - p) < 48;
	};
	auto findInt = [&](const char* key, int def) -> int {
		return int(findNum(key, def));
	};

	std::lock_guard<std::mutex> lock(GCmdMutex);
	GCmd.LastReject = "";

	if ((unsigned)consoleplayer < MAXPLAYERS && playeringame[consoleplayer]
		&& players[consoleplayer].playerstate == PST_DEAD)
	{
		const bool wantRespawn = findBool("respawn") || findBool("use");
		if (!wantRespawn && (findBool("fire") || fabs(findNum("forward", 0)) > 0.01 || fabs(findNum("side", 0)) > 0.01))
		{
			GCmd.LastReject = "dead";
			return FString("{\"ok\":false,\"rejected_reason\":\"dead\",\"hint\":\"hcde_respawn when can_respawn\"}");
		}
	}

	GCmd.Active = true;
	// Default burst ~1.1s. Optional hold is capped (~1.5s) so she cannot cruise forever.
	int tics = findInt("tics", 40);
	if (tics < 1)
		tics = 1;
	if (tics > 105)
		tics = 105;
	GCmd.TicsLeft = tics;
	GCmd.Forward = ClampUnit(findNum("forward", 0));
	GCmd.Side = ClampUnit(findNum("side", 0));
	GCmd.YawDeg = findNum("yaw_deg", 0);
	GCmd.PitchDeg = findNum("pitch_deg", 0);
	GCmd.Fire = findBool("fire");
	GCmd.Use = findBool("use");
	GCmd.Jump = findBool("jump");
	GCmd.Respawn = findBool("respawn");
	GCmd.HoldMove = findBool("hold");
	if (GCmd.Respawn)
		GCmd.Use = true;
	GCmd.WeaponSlot = findInt("weapon_slot", -1);
	if (fabs(GCmd.Forward) < 0.01 && fabs(GCmd.Side) < 0.01)
	{
		GCmd.HoldMove = false;
		GCmd.HoldTicsLeft = 0;
	}
	else if (GCmd.HoldMove)
	{
		int holdTics = findInt("hold_tics", 52); // ~1.5s at 35Hz
		if (holdTics < 1)
			holdTics = 1;
		if (holdTics > 70)
			holdTics = 70;
		GCmd.HoldTicsLeft = holdTics;
	}
	else
		GCmd.HoldTicsLeft = 0;

	const double yawReq = GCmd.YawDeg;
	const double maxTurn = double(*k8ti_agent_max_turn_deg);
	// Budget must cover the full requested yaw, not only pulse tics.
	const int yawTicsNeeded = int(ceil(fabs(yawReq) / max(maxTurn, 0.1)));
	double yawAppliedPlan = clamp(yawReq, -maxTurn * max(tics, yawTicsNeeded), maxTurn * max(tics, yawTicsNeeded));
	if (fabs(yawReq) > 0.01 && tics < yawTicsNeeded)
		GCmd.TicsLeft = yawTicsNeeded;

	FString resp;
	resp.AppendFormat(
		"{\"ok\":true,\"yaw_requested\":%.2f,\"yaw_applied_budget\":%.2f,\"tics\":%d,\"forward\":%.2f,\"side\":%.2f,"
		"\"hold\":%s,\"hold_tics\":%d,\"fire\":%s,\"use\":%s,\"respawn\":%s,\"weapon_slot\":%d,\"rejected_reason\":null}",
		yawReq, yawAppliedPlan, GCmd.TicsLeft, GCmd.Forward, GCmd.Side,
		GCmd.HoldMove ? "true" : "false", GCmd.HoldTicsLeft,
		GCmd.Fire ? "true" : "false", GCmd.Use ? "true" : "false", GCmd.Respawn ? "true" : "false",
		GCmd.WeaponSlot);
	return resp;
}

void HandleRequest(K8tiSock s, const char* req)
{
	++GTransport.Requests;
	const bool isGet = strnicmp(req, "GET ", 4) == 0;
	const bool isPost = strnicmp(req, "POST ", 5) == 0;
	const char* pathStart = strchr(req, ' ');
	if (pathStart == nullptr)
	{
		SendJson(s, 400, FString("{\"ok\":false,\"error\":\"bad request\"}"));
		return;
	}
	++pathStart;
	const char* pathEnd = strchr(pathStart, ' ');
	if (pathEnd == nullptr)
		pathEnd = pathStart + strlen(pathStart);
	FString path(pathStart, (int)(pathEnd - pathStart));

	if (isGet && (path.Compare("/") == 0 || path.Compare("/health") == 0))
	{
		SendJson(s, 200, FString("{\"ok\":true,\"service\":\"k8ti_agent\"}"));
		return;
	}
	if (isGet && path.IndexOf("/world") == 0)
	{
		const bool debugView = path.IndexOf("view=debug") >= 0;
		SendJson(s, 200, BuildWorldJson(debugView));
		return;
	}
	if (isGet && path.IndexOf("/debug/self") == 0)
	{
		SendJson(s, 200, BuildDebugSelfJson());
		return;
	}
	if (isPost && path.IndexOf("/cmd") == 0)
	{
		const char* body = strstr(req, "\r\n\r\n");
		if (body == nullptr)
			body = "";
		else
			body += 4;
		SendJson(s, 200, HandleCmdBody(body));
		return;
	}
	SendJson(s, 404, FString("{\"ok\":false,\"error\":\"not found\"}"));
}

void StartListener()
{
	if (!EffectiveEnabled())
	{
		CloseSock(GTransport.Client);
		CloseSock(GTransport.Listen);
		GTransport.Listening = false;
		return;
	}
	if (GTransport.Listening && GTransport.BoundPort == EffectivePort())
		return;

	CloseSock(GTransport.Client);
	CloseSock(GTransport.Listen);
	GTransport.Listening = false;
	GTransport.RecvUsed = 0;

	if (!EnsureWinsock())
		return;

	K8tiSock ls = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (ls == K8TI_INVALID)
		return;
	int yes = 1;
	setsockopt(ls, SOL_SOCKET, SO_REUSEADDR, (const char*)&yes, sizeof(yes));
	sockaddr_in addr{};
	addr.sin_family = AF_INET;
	addr.sin_port = htons((u_short)EffectivePort());
	addr.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
	if (bind(ls, (sockaddr*)&addr, sizeof(addr)) != 0 || listen(ls, 2) != 0)
	{
		CloseSock(ls);
		Printf(TEXTCOLOR_ORANGE "k8ti_agent: bind 127.0.0.1:%d failed\n", EffectivePort());
		return;
	}
	SetNonBlocking(ls);
	GTransport.Listen = ls;
	GTransport.BoundPort = EffectivePort();
	GTransport.Listening = true;
	Printf(TEXTCOLOR_GREEN "k8ti_agent: listening on 127.0.0.1:%d\n", GTransport.BoundPort);
}

void PollAcceptAndRead()
{
	if (!GTransport.Listening)
		return;

	if (GTransport.Client == K8TI_INVALID)
	{
		sockaddr_in peer{};
#ifdef _WIN32
		int plen = sizeof(peer);
#else
		socklen_t plen = sizeof(peer);
#endif
		K8tiSock c = accept(GTransport.Listen, (sockaddr*)&peer, &plen);
		if (c != K8TI_INVALID)
		{
			// Enforce loopback peer.
#ifdef _WIN32
			if (peer.sin_addr.S_un.S_addr != htonl(INADDR_LOOPBACK))
#else
			if (peer.sin_addr.s_addr != htonl(INADDR_LOOPBACK))
#endif
			{
				CloseSock(c);
			}
			else
			{
				SetNonBlocking(c);
				GTransport.Client = c;
				GTransport.RecvUsed = 0;
			}
		}
	}

	if (GTransport.Client == K8TI_INVALID)
		return;

	int space = K8TI_MAX_REQ - 1 - GTransport.RecvUsed;
	if (space <= 0)
	{
		CloseSock(GTransport.Client);
		GTransport.RecvUsed = 0;
		return;
	}
	int n = recv(GTransport.Client, GTransport.RecvBuf + GTransport.RecvUsed, space, 0);
	if (n == 0)
	{
		CloseSock(GTransport.Client);
		GTransport.RecvUsed = 0;
		return;
	}
	if (n < 0)
	{
#ifdef _WIN32
		int err = WSAGetLastError();
		if (err != WSAEWOULDBLOCK)
#else
		if (errno != EAGAIN && errno != EWOULDBLOCK)
#endif
		{
			CloseSock(GTransport.Client);
			GTransport.RecvUsed = 0;
		}
		return;
	}
	GTransport.RecvUsed += n;
	GTransport.RecvBuf[GTransport.RecvUsed] = 0;
	if (strstr(GTransport.RecvBuf, "\r\n\r\n") == nullptr)
		return;

	HandleRequest(GTransport.Client, GTransport.RecvBuf);
	CloseSock(GTransport.Client);
	GTransport.RecvUsed = 0;
}

} // namespace

bool K8tiAgentEnabled()
{
	return EffectiveEnabled();
}

void K8tiAgentPoll()
{
	StartListener();
	PollAcceptAndRead();
}

void K8tiAgentMergeTiccmd(usercmd_t* cmd)
{
	if (cmd == nullptr || !EffectiveEnabled())
		return;

	// Her seat while k8ti_agent is on: discard keyboard/mouse. Idle = stand still.
	// Only sticky POST /cmd motor may move/look/fire.
	cmd->forwardmove = 0;
	cmd->sidemove = 0;
	cmd->upmove = 0;
	cmd->yaw = 0;
	cmd->pitch = 0;
	cmd->roll = 0;
	cmd->buttons = 0;

	std::lock_guard<std::mutex> lock(GCmdMutex);
	const bool stickyWalk = GCmd.HoldMove && GCmd.HoldTicsLeft > 0
		&& (fabs(GCmd.Forward) > 0.01 || fabs(GCmd.Side) > 0.01);
	const bool turning = fabs(GCmd.YawDeg) > 0.01 || fabs(GCmd.PitchDeg) > 0.01;
	if (!GCmd.Active || (GCmd.TicsLeft <= 0 && !stickyWalk && !turning))
	{
		GCmd.Active = false;
		GCmd.HoldMove = false;
		GCmd.HoldTicsLeft = 0;
		return;
	}

	// After G_BuildTiccmd's <<= 8, magnitudes are shifted.
	constexpr int MaxMove = 50 << 8;
	if (stickyWalk || GCmd.TicsLeft > 0)
	{
		cmd->forwardmove = short(ClampUnit(GCmd.Forward) * MaxMove);
		cmd->sidemove = short(ClampUnit(GCmd.Side) * MaxMove);
	}

	const double maxTurn = double(*k8ti_agent_max_turn_deg);
	double stepYaw = clamp(GCmd.YawDeg, -maxTurn, maxTurn);
	GCmd.YawDeg -= stepYaw;
	double stepPitch = clamp(GCmd.PitchDeg, -maxTurn, maxTurn);
	GCmd.PitchDeg -= stepPitch;

	// ~182 BAM>>16 units per degree
	cmd->yaw = short(int(stepYaw * 182.044444));
	cmd->pitch = short(int(stepPitch * 182.044444));

	if (GCmd.Fire)
		cmd->buttons |= BT_ATTACK;
	if (GCmd.Use || GCmd.Respawn)
		cmd->buttons |= BT_USE;
	if (GCmd.Jump)
		cmd->buttons |= BT_JUMP;

	if (GCmd.WeaponSlot >= 0 && GCmd.WeaponSlot < NUM_WEAPON_SLOTS)
	{
		SendWeaponSlot = GCmd.WeaponSlot;
		GCmd.WeaponSlot = -1;
	}

	if (GCmd.HoldTicsLeft > 0)
		--GCmd.HoldTicsLeft;
	if (GCmd.HoldTicsLeft <= 0)
	{
		GCmd.HoldMove = false;
		GCmd.HoldTicsLeft = 0;
	}

	if (GCmd.TicsLeft > 0)
		--GCmd.TicsLeft;
	if (GCmd.TicsLeft <= 0)
	{
		GCmd.Fire = false;
		GCmd.Use = false;
		GCmd.Jump = false;
		GCmd.Respawn = false;
		// Do NOT wipe remaining yaw/pitch — keep draining until aimed.
		const bool stillTurning = fabs(GCmd.YawDeg) > 0.01 || fabs(GCmd.PitchDeg) > 0.01;
		const bool stillHold = GCmd.HoldMove && GCmd.HoldTicsLeft > 0
			&& (fabs(GCmd.Forward) > 0.01 || fabs(GCmd.Side) > 0.01);
		if (stillHold || stillTurning)
		{
			GCmd.TicsLeft = 0;
			GCmd.Active = true;
		}
		else
		{
			GCmd.Active = false;
			GCmd.HoldMove = false;
			GCmd.HoldTicsLeft = 0;
			GCmd.Forward = 0;
			GCmd.Side = 0;
		}
	}
}

CCMD(k8ti_agent_status)
{
	Printf("k8ti_agent enabled=%d listening=%d port=%d requests=%llu\n",
		EffectiveEnabled() ? 1 : 0,
		GTransport.Listening ? 1 : 0,
		GTransport.BoundPort,
		(unsigned long long)GTransport.Requests);
}
