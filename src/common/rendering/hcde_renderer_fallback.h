/*
** hcde_renderer_fallback.h
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

#pragma once

// Normalize legacy renderer CVAR values (e.g. removed OpenGL ES backend id).
void HCDE_MigrateRendererCvars();

// Request desktop OpenGL instead of Vulkan for the next framebuffer creation.
void HCDE_ForceDesktopOpenGLFallback(const char *reason);

// Switch to the software scene drawer. Safe to call multiple times.
void HCDE_ActivateSoftwareRendererFallback(const char *reason);

// True when the software scene drawer should be used instead of the HW path.
bool HCDE_UsingSoftwareRenderer();
