// Pure presentation policy shared by snapshot application and the countdown HUD.
#pragma once

namespace HCDEInvasionPolicy
{
constexpr int CountdownTics(bool localAuthority, bool countdown, int stateTics, int cutsceneTics)
{
    const int authoritativeTics = stateTics > 0 ? stateTics : 0;
    return localAuthority && countdown && cutsceneTics > authoritativeTics
        ? cutsceneTics : authoritativeTics;
}

constexpr int PendingWave(bool countdown, int activeWave)
{
    return !countdown ? 0 : activeWave < 0 ? 1 : activeWave < 2147483647 ? activeWave + 1 : activeWave;
}
}
