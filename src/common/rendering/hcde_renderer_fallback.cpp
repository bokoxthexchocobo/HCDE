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

#ifndef NO_SWRENDERER
EXTERN_CVAR(Int, vid_rendermode)
#endif
EXTERN_CVAR(Int, vid_preferbackend)
EXTERN_CVAR(Int, hcde_nanobsp_loader)

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

void HCDE_ActivateSoftwareRendererFallback(const char *reason)
{
	HCDE_MigrateRendererCvars();

#ifndef NO_SWRENDERER
	if (vid_rendermode != 1)
	{
		Printf(TEXTCOLOR_ORANGE "Falling back to software renderer (NanoBSP path): %s\n",
			reason != nullptr ? reason : "hardware renderer unavailable");
		vid_rendermode = 1; // truecolor software scene drawer
	}
#endif

	if (hcde_nanobsp_loader == 0)
	{
		hcde_nanobsp_loader = 1;
		Printf("Enabled hcde_nanobsp_loader for software renderer node building.\n");
	}
}
