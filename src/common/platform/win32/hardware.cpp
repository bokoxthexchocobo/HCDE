/*
** hardware.cpp
**
** Somewhat OS-independent interface to the screen, mouse, keyboard, and stick
**
**---------------------------------------------------------------------------
**
** Copyright 1998-2016 Marisa Heit
** Copyright 2008-2016 Christoph Oelckers
** Copyright 2017-2025 GZDoom Maintainers and Contributors
** Copyright 2025-2026 UZDoom Maintainers and Contributors
**
** SPDX-License-Identifier: GPL-3.0-or-later
**
**---------------------------------------------------------------------------
**
** Code written prior to 2026 is also licensed under:
**
** SPDX-License-Identifier: BSD-3-Clause
**
**---------------------------------------------------------------------------
**
*/

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <mmsystem.h>

#include "hardware.h"
#include "c_dispatch.h"
#include "v_text.h"
#include "basics.h"
#include "m_argv.h"
#include "version.h"
#include "printf.h"
#include "win32glvideo.h"
#include "hcde_renderer_fallback.h"
#include "engineerrors.h"
#include "i_system.h"
#include "i_mainwindow.h"

IVideo *Video;

// do not include GL headers here, only declare the necessary functions.
IVideo *gl_CreateVideo();

void I_RestartRenderer();
int currentcanvas = -1;
bool changerenderer;

void I_ShutdownGraphics ()
{
	if (screen)
	{
		DFrameBuffer *s = screen;
		screen = NULL;
		delete s;
	}
	if (Video)
		delete Video, Video = NULL;
}

void I_InitGraphics ()
{
	// [HCDE] Ensure the window is focused and in the foreground on startup.
	// Some systems or launchers might leave the window backgrounded, which
	// can cause a black screen if the engine's AppActive logic is too strict.
	if (GetFocus() == NULL)
	{
		SetForegroundWindow(mainwindow.GetHandle());
		SetFocus(mainwindow.GetHandle());
	}
	else if (GetActiveWindow() == mainwindow.GetHandle())
	{
		SetForegroundWindow(mainwindow.GetHandle());
		SetFocus(mainwindow.GetHandle());
	}

	HCDE_MigrateRendererCvars();

	// Win32GLVideo::CreateFrameBuffer tries Vulkan first when requested,
	// then falls back to desktop OpenGL automatically.
	Video = new Win32GLVideo();

	if (Video == NULL)
		I_FatalError ("Failed to initialize display");

}
