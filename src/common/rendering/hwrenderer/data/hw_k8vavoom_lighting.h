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

// Apply k8vavoom profile / vk_raytrace before framebuffer InitializeState() so Vulkan
// descriptor layouts and shader defines agree on ray-query support.
void HCDE_K8vavoomPrepareBeforeInitializeState(DFrameBuffer *framebuffer);
