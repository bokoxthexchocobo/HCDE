/*
** hw_k8vavoom_lighting.cpp
**
** k8vavoom-style lighting profile (Phase 2).
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

#include "hw_k8vavoom_lighting.h"
#include "hw_cvars.h"
#include "hwrenderer/postprocessing/hw_postprocess_cvars.h"
#include "c_dispatch.h"
#include "printf.h"
#include "v_video.h"
#include "d_main.h"

EXTERN_CVAR(Int, gl_shadowmap_quality)
EXTERN_CVAR(Bool, vk_raytrace)

namespace
{
	struct K8vavoomPresetSnapshot
	{
		bool applied = false;
		bool shadowmap = false;
		bool prioritize = false;
		int quality = 0;
		int maxlights = 0;
		int shadow_filter = 0;
		bool shadow_autobudget = false;
		bool shadow_autofallback = false;
		bool bloom = false;
		int tonemap = 0;
		int ssao = 0;
		bool shadow_boost = false;
		bool raylight_probe = false;
		bool vk_raytrace = false;
	};

	K8vavoomPresetSnapshot g_K8vavoomPresetState;

	const char* BackendNameForScreen()
	{
		if (screen == nullptr)
			return "unknown";

		switch (screen->Backend())
		{
		case BACKEND_VULKAN: return "vulkan";
		case BACKEND_OPENGL: return "opengl";
		default:             return "unknown";
		}
	}

	void HCDE_K8vavoomTryEnableRaylightPath()
	{
		if (screen == nullptr || !screen->SupportsRayQueries())
			return;

		if (!vk_raytrace)
		{
			vk_raytrace = true;
			Printf(PRINT_HIGH, "Enabled vk_raytrace for k8vavoom ray-style dynamic light shadows.\n");
		}
	}

	void HCDE_K8vavoomApplyLightingProfile(int profile, bool shadowBoost, bool raylightProbe)
	{
		K8vavoomPresetSnapshot &s = g_K8vavoomPresetState;
		s = K8vavoomPresetSnapshot{};
		s.applied = profile > 0;

		if (profile <= 0)
			return;

		gl_light_shadowmap = true;
		gl_shadowmap_prioritize = true;

		const int qualityFloor = shadowBoost ? 1024 : 512;
		if (gl_shadowmap_quality < qualityFloor)
			gl_shadowmap_quality = qualityFloor;

		gl_bloom = true;
		if (gl_tonemap == 0)
			gl_tonemap = 1;
		if (gl_ssao == 0)
			gl_ssao = 1;

		hcde_shadow_autofallback = true;
		hcde_shadow_autobudget = true;
		if (gl_shadowmap_maxlights == 0)
			gl_shadowmap_maxlights = shadowBoost ? 512 : 256;
		if (gl_shadowmap_filter == 0)
			gl_shadowmap_filter = 1;

		if (raylightProbe || shadowBoost)
			HCDE_K8vavoomTryEnableRaylightPath();

		s.shadowmap           = gl_light_shadowmap;
		s.prioritize          = gl_shadowmap_prioritize;
		s.quality             = gl_shadowmap_quality;
		s.maxlights           = gl_shadowmap_maxlights;
		s.shadow_filter       = gl_shadowmap_filter;
		s.shadow_autobudget   = hcde_shadow_autobudget;
		s.shadow_autofallback = hcde_shadow_autofallback;
		s.bloom               = gl_bloom;
		s.tonemap             = gl_tonemap;
		s.ssao                = gl_ssao;
		s.shadow_boost        = shadowBoost;
		s.raylight_probe      = raylightProbe || shadowBoost;
		s.vk_raytrace         = vk_raytrace;

		Printf(PRINT_HIGH,
			"hcde_k8vavoom_lighting_profile=1 applied: shadowmap=%s prioritize=%s quality=%d maxlights=%d filter=%d autobudget=%s autofallback=%s bloom=%s tonemap=%d ssao=%d boost=%s raylight=%s vk_raytrace=%s\n",
			s.shadowmap ? "on" : "off",
			s.prioritize ? "on" : "off",
			s.quality,
			s.maxlights,
			s.shadow_filter,
			s.shadow_autobudget ? "on" : "off",
			s.shadow_autofallback ? "on" : "off",
			s.bloom ? "on" : "off",
			s.tonemap,
			s.ssao,
			s.shadow_boost ? "on" : "off",
			s.raylight_probe ? "on" : "off",
			s.vk_raytrace ? "on" : "off");
	}
}

K8vavoomBackendCapabilities HCDE_ProbeK8vavoomBackendCapabilities()
{
	K8vavoomBackendCapabilities caps;
	caps.IsHardwareRenderer = V_IsHardwareRenderer();

	if (screen != nullptr)
	{
		caps.BackendName = BackendNameForScreen();
		caps.SupportsShadowmaps = caps.IsHardwareRenderer && screen->SupportsHardwareShadowmaps();
		caps.SupportsRayQueries = screen->SupportsRayQueries();
		caps.RaylightProbeActive = screen->RaytracingActive();
		caps.RaylightProbeMeaningful = caps.SupportsRayQueries && hcde_k8vavoom_raylight_probe;

		if (!caps.IsHardwareRenderer)
			caps.Notes = "software backend; shadowmap and raylight features inactive";
		else if (!caps.SupportsShadowmaps)
			caps.Notes = "hardware backend active but SSBO shadowmap path unavailable";
		else if (caps.RaylightProbeActive)
			caps.Notes = "Vulkan ray-query path active for dynamic light shadows";
		else if (caps.SupportsRayQueries)
			caps.Notes = "Vulkan ray-query extension available; enable hcde_k8vavoom_raylight_probe or profile with boost";
		else if (screen->Backend() == BACKEND_VULKAN)
			caps.Notes = "Vulkan backend without VK_KHR_ray_query; using shadowmap path";
		else
			caps.Notes = "OpenGL backend; shadowmap path (ray queries require Vulkan)";
		return caps;
	}

	caps.BackendName = caps.IsHardwareRenderer ? "hardware (screen pending)" : "software (screen pending)";
	caps.Notes = "screen not initialized; capability probe deferred";
	return caps;
}

void HCDE_K8vavoomFinalizeAfterVideoInit()
{
	if (screen == nullptr || !V_IsHardwareRenderer())
		return;

	const K8vavoomBackendCapabilities caps = HCDE_ProbeK8vavoomBackendCapabilities();

	if (hcde_k8vavoom_auto_profile && hcde_k8vavoom_lighting_profile == 0 && caps.SupportsShadowmaps)
	{
		hcde_k8vavoom_shadow_boost = caps.SupportsRayQueries;
		if (caps.SupportsRayQueries)
			hcde_k8vavoom_raylight_probe = true;
		hcde_k8vavoom_lighting_profile = 1;
		Printf(PRINT_HIGH, "Auto-enabled hcde_k8vavoom_lighting_profile on capable hardware.\n");
	}

	if (hcde_k8vavoom_lighting_profile > 0)
	{
		if (hcde_k8vavoom_raylight_probe || hcde_k8vavoom_shadow_boost)
			HCDE_K8vavoomTryEnableRaylightPath();
	}
}

CVAR(Bool, hcde_k8vavoom_auto_profile, true, CVAR_ARCHIVE | CVAR_GLOBALCONFIG)
CVAR(Bool, hcde_k8vavoom_shadow_boost, false, CVAR_ARCHIVE | CVAR_GLOBALCONFIG)
CVAR(Bool, hcde_k8vavoom_raylight_probe, false, CVAR_ARCHIVE | CVAR_GLOBALCONFIG)

CUSTOM_CVAR(Int, hcde_k8vavoom_lighting_profile, 0, CVAR_ARCHIVE | CVAR_GLOBALCONFIG)
{
	if (self < 0)
		self = 0;
	else if (self > 1)
		self = 1;

	HCDE_K8vavoomApplyLightingProfile(self, hcde_k8vavoom_shadow_boost, hcde_k8vavoom_raylight_probe);
}

CCMD(r_k8vavoom_status)
{
	const K8vavoomPresetSnapshot &s = g_K8vavoomPresetState;
	const K8vavoomBackendCapabilities caps = HCDE_ProbeK8vavoomBackendCapabilities();
	Printf(PRINT_HIGH, "k8vavoom lighting profile: %s\n",
		(hcde_k8vavoom_lighting_profile > 0) ? "ENABLED" : "disabled");
	Printf(PRINT_HIGH, "  auto_profile              = %s\n", hcde_k8vavoom_auto_profile ? "on" : "off");
	Printf(PRINT_HIGH, "  composed snapshot (last apply): applied=%s\n", s.applied ? "yes" : "no");
	Printf(PRINT_HIGH, "    gl_light_shadowmap        = %s (live=%s)\n", s.shadowmap ? "on" : "off",
		gl_light_shadowmap ? "on" : "off");
	Printf(PRINT_HIGH, "    gl_shadowmap_prioritize   = %s (live=%s)\n", s.prioritize ? "on" : "off",
		gl_shadowmap_prioritize ? "on" : "off");
	Printf(PRINT_HIGH, "    gl_shadowmap_quality      = %d (live=%d)\n", s.quality, *gl_shadowmap_quality);
	Printf(PRINT_HIGH, "    gl_shadowmap_maxlights    = %d (live=%d)\n", s.maxlights, *gl_shadowmap_maxlights);
	Printf(PRINT_HIGH, "    gl_shadowmap_filter       = %d (live=%d)\n", s.shadow_filter, *gl_shadowmap_filter);
	Printf(PRINT_HIGH, "    hcde_shadow_autobudget    = %s (live=%s)\n", s.shadow_autobudget ? "on" : "off",
		hcde_shadow_autobudget ? "on" : "off");
	Printf(PRINT_HIGH, "    hcde_shadow_autofallback  = %s (live=%s)\n", s.shadow_autofallback ? "on" : "off",
		hcde_shadow_autofallback ? "on" : "off");
	Printf(PRINT_HIGH, "    gl_bloom                  = %s (live=%s)\n", s.bloom ? "on" : "off",
		gl_bloom ? "on" : "off");
	Printf(PRINT_HIGH, "    gl_tonemap                = %d (live=%d)\n", s.tonemap, *gl_tonemap);
	Printf(PRINT_HIGH, "    gl_ssao                   = %d (live=%d)\n", s.ssao, *gl_ssao);
	Printf(PRINT_HIGH, "    vk_raytrace               = %s (live=%s)\n", s.vk_raytrace ? "on" : "off",
		vk_raytrace ? "on" : "off");
	Printf(PRINT_HIGH, "  sub-flags: shadow_boost=%s raylight_probe=%s\n",
		hcde_k8vavoom_shadow_boost ? "on" : "off",
		hcde_k8vavoom_raylight_probe ? "on" : "off");
	Printf(PRINT_HIGH, "  backend capabilities:\n");
	Printf(PRINT_HIGH, "    backend                   = %s\n", caps.BackendName);
	Printf(PRINT_HIGH, "    hardware-renderer         = %s\n", caps.IsHardwareRenderer ? "yes" : "no");
	Printf(PRINT_HIGH, "    supports-shadowmaps       = %s\n", caps.SupportsShadowmaps ? "yes" : "no");
	Printf(PRINT_HIGH, "    supports-ray-queries      = %s\n", caps.SupportsRayQueries ? "yes" : "no");
	Printf(PRINT_HIGH, "    raylight-path-active      = %s\n", caps.RaylightProbeActive ? "yes" : "no");
	Printf(PRINT_HIGH, "    raylight-probe-meaningful = %s\n", caps.RaylightProbeMeaningful ? "yes" : "no");
	Printf(PRINT_HIGH, "    notes                     = %s\n", caps.Notes);
}

CCMD(r_k8vavoom_reset)
{
	hcde_k8vavoom_lighting_profile = 0;
	hcde_k8vavoom_shadow_boost = false;
	hcde_k8vavoom_raylight_probe = false;
	hcde_k8vavoom_auto_profile = false;

	gl_light_shadowmap = false;
	gl_shadowmap_prioritize = true;
	gl_shadowmap_quality = 512;
	gl_shadowmap_maxlights = 0;
	gl_shadowmap_filter = 0;
	hcde_shadow_autobudget = false;
	hcde_shadow_autofallback = true;
	gl_bloom = false;
	gl_tonemap = 0;
	gl_ssao = 0;
	vk_raytrace = false;

	g_K8vavoomPresetState = K8vavoomPresetSnapshot{};
	Printf(PRINT_HIGH, "k8vavoom lighting preset reset to HCDE defaults.\n");
}
