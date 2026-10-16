# Lucy validation and Windows build

Use Unity **2022.3.34f1** with Windows Build Support installed. Wait for script compilation to finish and exit Play Mode before running these commands.

- **Tools > Lucy > Validate Project** runs read-only regression checks.
- **Tools > Lucy > Build Windows** validates first, then writes `Builds/Windows/Lucy.exe` and its companion files. Save modified open scenes before building; the command refuses to silently save them.

Validation refuses to run when an open scene has unsaved changes. It opens unloaded build scenes additively, closes only scenes it opened in a `finally` block without saving, and preserves the original active scene. Already loaded scenes remain open. It never saves or discards the user's unsaved scene changes. Save changes explicitly before validating.

Reports are written to `Logs/LucyValidation.json` and `Logs/LucyValidation.txt`. The Windows build summary is written to `Logs/LucyBuild.txt`. A failed check throws `BuildFailedException` after writing the validation reports, allowing batch mode to fail visibly.

With the interactive editor closed, the same methods can run from a terminal:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\2022.3.34f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\jys09\Projects\Lucy' -executeMethod Lucy.Editor.LucyValidation.ValidateProject -logFile 'Logs/ValidationEditor.log'
& 'C:\Program Files\Unity\Hub\Editor\2022.3.34f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\jys09\Projects\Lucy' -executeMethod Lucy.Editor.LucyValidation.BuildWindows -logFile 'Logs/BuildEditor.log'
```

The automated suite checks equal-cost pathfinding candidates, obstacle detours, repeated forward/reverse searches, map resizing, offset boundaries, same-cell paths, enabled build scene existence/order/unique names, complete portal and scene enum mappings, finite portal destinations, missing scripts in enabled scenes and their prefab dependencies, and assigned portal manager data references.

These checks do not establish that a full playthrough is bug-free. Before release, manually verify title/new game, movement and sprint, item pickup/inventory, each puzzle and portal in both directions, death/restart, pause/resume, ending and replay, audio controls, and the standalone Windows player. Check the Console for runtime exceptions throughout. Portal destination collision safety and puzzle solvability require runtime play verification.

## Runtime integration smoke

`Tools > Lucy > Run Play Smoke` (or `Lucy.Editor.LucyPlaySmoke.Run()`) exercises a fresh run, simulated mental depletion, failure restart, twelve portal transitions, ending, replay, and title return. It invokes lifecycle and portal APIs directly; it is not a recorded puzzle playthrough. Results are written to `Logs/LucyPlaySmoke.txt`. Game and diagnostic plugin exceptions are counted separately. Best-time preferences are restored after the test.

## Earlier verification on 2026-10-16 (before visual repair)

- Windows x64 release build succeeded with 0 errors and 0 warnings (289,194,022 bytes; 66.5 seconds). Output: `Builds/Windows/Lucy.exe`. Distribute the entire `Builds/Windows` folder, not the executable alone.
- Packaged player: `Builds/Lucy-Windows.zip` (debug-only Burst symbols excluded), archive contents checked for the executable and Unity runtime.
- After app-access authorization, the Windows player was launched and tested through actual mouse/keyboard input: title, new game, first-floor rendering/HUD, Escape pause, help, help return, and title return all worked. No exception/error was recorded in `Logs/LucyStandalonePlayer.log`. A non-fatal one-active-EventSystem warning appears during initial UI handover; subsequent input checks passed. The player is left open at its title screen.
- Unity 2022.3.34f1 compiled the revised game and embedded Unity Play MCP dependency.
- Static validation passed 66 checks, including actual navigation-map construction with different floor/wall grid sizes and independent tile bounds. The unused legacy `0101` scene has a missing script and is excluded; the authored `0101_09.18` scene is the first playable level.
- Independent C# pathfinding checks compared 300 searches against BFS, plus resized/isolated/same-cell cases.
- Real input checks in Unity: title new-game button, WASD movement, E door interaction, walking through the opened doorway, inventory opening/closing, Escape pause/help/resume. Mental resource remained unchanged while paused.
- Runtime integration completed with zero game exceptions and zero plugin exceptions after fixing maze navigation initialization, missing AI sensors, and room-revisit event registration.
- Scene-change listener exceptions are now logged as errors rather than hidden as warnings. The repeat run also passed after repairing destroyed/missing NPC dialogue references in maze and ending scenes.
- Final integration run against the built source finished at 2026-10-16 03:11:00 UTC: all lifecycle and twelve portal steps passed, game errors 0, plugin errors 0.
- Static level-data review found all six bridge switches connected to fourteen bridge segments. BFS over the corrected maze navigation map reached all 284 floor cells, including exit-adjacent cells. This does not replace physical collision testing.
- At this earlier stage, full manual puzzle solving, item combinations, and a complete start-to-exit playthrough remained outside the verified scope. The later actual-input completion is recorded below; exhaustive item combinations remain unverified.

The embedded dependency's optional IL action discovery is disabled because its upstream implementation caused a compiler stack overflow. See `Packages/dev.yunseong.unityplaymcp/LUCY_PATCH.md`. Scene readings, screenshots and actual input injection were tested.

## Setting correction — psychiatric hospital

Following the user's explicit correction, replaced the incorrectly inferred school title image, title copy, three location objectives, and design framing with the intended psychiatric-hospital setting. New generated art is saved at `Assets/Resources/Presentation/LucyTitle.png`; its exact prompt is recorded in `docs/GAME_DESIGN.md`. Existing authored level geometry, items, dialogue and gameplay logic are unchanged by this correction.

Rebuilt Windows player and replaced `Builds/Lucy-Windows.zip`: validation 66 passed, build 0 errors / 0 warnings, build duration 22.3 seconds. Visually confirmed the hospital corridor illustration, THE SILENT WARD heading and Korean psychiatric-hospital description in the actual Windows player. The player remains open on the corrected title screen.

## Visual repair regression scope (2026-10-16)
The previous ending run demonstrated progression, not visual quality. It does not certify the visual repair.
During visual repair the first room was inspected in Play, then traversed with actual movement/interact inputs through 0102 to 0201. This exposed an unlit corridor character, subsequently addressed with a serialized ambient-light prefab (sorting-layer targets must be serialized for standalone Unity players). Generated-atlas quadrant bleed was also found and removed from slicing bounds. Final compile/build and visual regression results are recorded separately below.

## Latest visual repair verification — 2026-10-16

- Actual-input gameplay ran from first-floor observation at 05:23:08 UTC to the first Ending observation at 05:33:59.101 UTC. The ending displayed 07:17, with all four records and six switches completed. One checkpoint retry followed capture during observation after the nurse dialogue. This was not a deathless run.
- The initial readings connection was `6e2d64159d904dfdb581a8f6645e0ff6`; reconnecting readings as `934bb…` did not restart Play Mode. The operator confirmed that duplicate inventory UI and placeholder dialogue did not reappear across scene transitions.
- After that completion, the nurse/enemy sprites, NPC portrait and nurse foot pivot were revised, and an Editor automation file-sharing exception guard was added. No route, AI or puzzle behavior was changed in those follow-up edits.
- Follow-up connection `bf6e3315…` used actual input from new game to 0201. The nurse world sprite and the WardShadow capture screen were visually checked. This is a short final-art check, not another complete playthrough.
- Latest static validation: timestamp `2026-10-16T05:40:17.8293177Z`, 66 passed, 0 failed.
- Final Windows x64 build: directly verified the newly written `Logs/LucyBuild.txt`, modified at **05:41:39 UTC**. Result **Succeeded**, errors **0**, warnings **0**, size **320,802,390 bytes**, duration **00:01:17.4045062**. Output: `Builds/Windows/Lucy.exe` with companion runtime/data files. This supersedes the 04:42:01 UTC build report; it does not by itself verify packaging or standalone launch.
- Archive refreshed at 14:42:35 KST: `Builds/Lucy-Windows.zip`, 115,617,987 bytes. Verified executable, resources.assets and Assembly-CSharp.dll entries.
- Final standalone player launched and its hospital title screen was visually confirmed. Further new-game interaction was not verified because Computer Use reported `unknown screenshotId screenshot-0` and then `Computer Use helper already has an active request`. This is title-screen smoke coverage, not standalone end-to-end coverage.
- See [visual repair evidence](VISUAL_REPAIR_2026-10-16.md) for screenshot links and the distinction between game runtime errors, Editor automation IOExceptions and the EventSystem warning.
