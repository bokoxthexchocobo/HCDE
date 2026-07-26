#pragma once

// K8ti body bridge — loopback HTTP for player-honest world sight + ticcmd motor.
// Bind is 127.0.0.1 only. Enable with +k8ti_agent 1 (+k8ti_agent_port N).

struct usercmd_t;

void K8tiAgentPoll();
void K8tiAgentMergeTiccmd(usercmd_t* cmd);
bool K8tiAgentEnabled();
