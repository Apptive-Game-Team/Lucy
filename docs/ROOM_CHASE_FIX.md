# Room pursuit navigation

0201's west doorway is the only floor tile at x=-2 on row y=1; adjacent rows y=0 and y=2 have no floor at x=-2. Node.SetPosition rounds world coordinates, so fractional positions such as (-2.4,1.6) become the blocked node (-2,2). A pursuit ending at that position failed despite the open doorway.

PathFinder.FindPathToWorld preserves authored integer navigation points. Only blocked rounded endpoints are projected onto nearby walkable nodes within 1.1 world units, ordered by distance; each candidate still requires a complete A* path. Actor now uses this method. Existing valid endpoints and the maze's integer-centred routes are unchanged. Light-aware navigation also invalidates its cache when doors change, so it cannot retain a closed-door map after opening.

## Verification

- Regression validation checks a fractional doorway target, closed-door rejection and reopened reverse passage.
- Unity Play session b7b869599ef04b21b159f91da55f7271, muted: an explicit editor-only fixture placed Guard at (0,1) and player at (-4,1.6), removed the NPC event component, and expired arrival protection. It first verified that the closed door had no route, then invoked the normal door action and actual creature sensing/movement. The guard stayed in PURSUIT and moved through x=-0.50,-0.90,-1.20,-1.80,-2.20 to -2.70 inside the room. Log: Logs/LucyRoomChase.txt. The fixture stopped the guard after entry. This verifies same-scene room pursuit, not cross-scene pursuit or an end-to-end game run.
- Automatic project validation: 82 passed, 0 failed before the Play test.

Windows build succeeded at 2026-10-16 17:12:27 KST: 0 errors, 0 warnings, 82 validation checks passed. Local ZIP refreshed; existing GitHub release unchanged.
