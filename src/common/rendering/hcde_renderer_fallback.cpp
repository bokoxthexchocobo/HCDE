/*
** hcde_renderer_fallback.cpp
**
** HCDE renderer preference migration and software-renderer fallback.
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

#include "hcde_renderer_fallback.h"
#include "v_video.h"
#include "c_dispatch.h"
#include "printf.h"
#include "v_text.h"

#ifndef NO_SWRENDERER
EXTERN_CVAR(Int, vid_rendermode)
#endif
EXTERN_CVAR(Int, vid_preferbackend)

void HCDE_MigrateRendererCvars()
{
	// OpenGL ES was removed; configs that still request backend 2 should use Vulkan.
	if (vid_preferbackend == 2)
	{
		Printf("Migrating vid_preferbackend from OpenGL ES (2) to Vulkan (1).\n");
		vid_preferbackend = BACKEND_VULKAN;
	}
	else if (vid_preferbackend < 0 || vid_preferbackend >= NUM_BACKEND)
	{
		vid_preferbackend = BACKEND_DEFAULT;
	}
}

bool HCDE_UsingSoftwareRenderer()
{
#ifndef NO_SWRENDERER
	return vid_rendermode != 4;
#else
	return false;
#endif
}

void HCDE_ForceDesktopOpenGLFallback(const char *reason)
{
	HCDE_MigrateRendererCvars();

	if (vid_preferbackend != BACKEND_OPENGL)
	{
		Printf(TEXTCOLOR_ORANGE "Falling back to desktop OpenGL: %s\n",
			reason != nullptr ? reason : "Vulkan unavailable");
		vid_preferbackend = BACKEND_OPENGL;
	}
}

void HCDE_ActivateSoftwareRendererFallback(const char *reason)
{
	HCDE_MigrateRendererCvars();

#ifndef NO_SWRENDERER
	if (vid_rendermode != 1)
	{
		Printf(TEXTCOLOR_ORANGE "Falling back to software renderer: %s\n",
			reason != nullptr ? reason : "hardware renderer unavailable");
		vid_rendermode = 1; // truecolor software scene drawer
	}
#endif
}

CCMD(r_hcde_renderer_status)
{
	HCDE_MigrateRendererCvars();
	Printf(PRINT_HIGH, "HCDE renderer status:\n");
	Printf(PRINT_HIGH, "  vid_preferbackend = %d (%s)\n", *vid_preferbackend,
		*vid_preferbackend == BACKEND_VULKAN ? "vulkan" :
		(*vid_preferbackend == BACKEND_OPENGL ? "opengl" : "unknown"));
#ifndef NO_SWRENDERER
	Printf(PRINT_HIGH, "  vid_rendermode    = %d (%s)\n", *vid_rendermode,
		HCDE_UsingSoftwareRenderer() ? "software" : "hardware");
#else
	Printf(PRINT_HIGH, "  vid_rendermode    = hardware (software renderer disabled at build time)\n");
#endif
	if (screen != nullptr)
	{
		Printf(PRINT_HIGH, "  active backend    = %d\n", screen->Backend());
	}
	else
	{
		Printf(PRINT_HIGH, "  active backend    = (screen not initialized)\n");
	}
}
