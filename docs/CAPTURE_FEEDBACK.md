# Capture feedback

Creature contact now calls GameSession.Caught instead of opening the failure menu immediately. Gameplay and repeated contact stop immediately. An unscaled sequence reveals and lunges the attacker, pulses dark red once, briefly zooms/shakes the camera, plays four Lucy recoil/collapse poses and fades to black before showing retry controls. Visual time advances by at most 50 ms per rendered frame so a slow frame cannot skip the whole animation. Normal speed duration is 1.65 seconds.

Impact and cry reuse Hit_stress and Misc_Cry_solo from the project's licensed sound assets. They bypass paused gameplay audio but respect master volume. At volume 0 no effect AudioSource is created. Audio was not auditioned during testing, per the user's mute instruction. Temporary sprites, materials, camera changes and audio sources are cleaned up before retry.

Asset: Assets/Resources/Presentation/LucyDefeat.png. Generated with built-in image_gen using lucy_sprite_left.png as the identity/style reference. The generated poses have unequal spacing; inspected transparent gaps at normalized x=.215,.43,.69 define the four slices. All poses share the first frame's pixels-per-unit and bottom pivot.

## Generation prompt

Create a production pixel-art animation sprite sheet for the attached game's Lucy character. Identity reference: short dark bob, pale blue-white hospital gown with small blue ribbon, dark ankle boots. Preserve her pixel-art style and proportions. Exactly FOUR equal square cells in one horizontal row, transparent background. Chronological non-graphic defeat animation, facing LEFT: 1 startled recoil with mouth open and arms flinching, 2 knees buckling and body crouching, 3 falling sideways with one hand reaching for floor, 4 lying collapsed on her side, eyes closed. No blood, no wounds, no monster. Same character scale across all cells; same ground baseline near bottom of each cell. Frame 4 body horizontal. Each pose fully contained with generous padding, no adjacent overlap. No text, grid lines, background, lighting aura, decorative effects. Clean readable silhouettes at small game scale. 4x1 atlas.

## Play verification

Muted actual collider-overlap fixture in 0201, session 4677ccfaf0ec403a94ef4b86611cbe41. Each phase had exactly one effect, timeScale=0, blocked=True, volume=0. At completion there were zero effects and no overlay. Phase-synchronized screenshots saved as Logs/LucyCapture-impact.png, -knees.png, -fall.png, -collapse.png and -result.png; impact and knees images inspected. The first iteration skipped poses under slow frames; capped visual delta fixed this in the second run. The actual retry UI button was clicked, returning to 0101_09.18 with timeScale=1 and movement enabled. This is a capture/retry test, not an end-to-end walkthrough.

Windows build succeeded 2026-10-16 17:38:54 KST, 0 errors and 0 warnings; 83 project checks passed. Retry movement confirmed from (-0.10,-3.89) to (0.94,-3.89). Local ZIP updated; GitHub release not updated.
