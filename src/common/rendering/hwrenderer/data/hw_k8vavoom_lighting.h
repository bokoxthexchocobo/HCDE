/*
** hw_k8vavoom_lighting.h
**
** k8vavoom-style lighting profile (Phase 2): capability probing, ray-style
** light path wiring, and optional auto-enable on capable hardware.
**
**---------------------------------------------------------------------------
**
** Copyright 2025-2026 UZDoom Maintainers and Contributors
**
** SPDX-License-Identifier: GPL-3.0-or-later
**
**---------------------------------------------------------------------------
**
*/

#pragma once

struct K8vavoomBackendCapabilities
{
	const char* BackendName = "unknown";
	bool        IsHardwareRenderer = false;
	bool        SupportsShadowmaps = false;
	bool        SupportsRayQueries = false;
	bool        RaylightProbeActive = false;
	bool        RaylightProbeMeaningful = false;
	const char* Notes = "";
};

K8vavoomBackendCapabilities HCDE_ProbeK8vavoomBackendCapabilities();

// Call after the real video framebuffer exists (end of V_Init2).
void HCDE_K8vavoomFinalizeAfterVideoInit();
