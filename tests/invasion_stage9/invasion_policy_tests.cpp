#include "../../src/d_net_invasion_policy.h"

using namespace HCDEInvasionPolicy;
// Late join during the countdown for wave 5 must not announce wave 1.
static_assert(PendingWave(true, 4) == 5);
static_assert(PendingWave(true, 0) == 1);
static_assert(PendingWave(false, 5) == 0);
// A client's stale cutscene countdown cannot override a newer server snapshot.
static_assert(CountdownTics(false, true, 35, 350) == 35);
static_assert(CountdownTics(false, true, 0, 350) == 0);
static_assert(CountdownTics(false, false, 70, 350) == 70);
// The authority still supports map-controlled cutscene countdowns.
static_assert(CountdownTics(true, true, 35, 350) == 350);
static_assert(CountdownTics(true, false, 35, 350) == 35);
static_assert(CountdownTics(false, true, -1, 350) == 0);
