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

// Normalize legacy renderer CVAR values and apply the software + NanoBSP
// fallback path when hardware rendering is unavailable.
void HCDE_MigrateRendererCvars();

// Switch to the software scene drawer and enable the NanoBSP loader path.
// Safe to call multiple times.
void HCDE_ActivateSoftwareRendererFallback(const char *reason);
