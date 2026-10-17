# Nurse artwork separation

The NPC dialogue controller and HospitalEnvironment both loaded Presentation/Nurse, causing a full-height dialogue illustration to appear on the map. Dialogue retains Nurse.png; the map now loads NurseWorld.png, a separate compact pixel-art character with matching hair and clothing. The original NPC transform and interaction trigger are preserved.

Generated using the built-in image_gen tool, with Nurse.png as an identity reference. Final asset: Assets/Resources/Presentation/NurseWorld.png.

## Prompt

Create an IN-GAME NPC sprite for a dark top-down 2D hospital RPG. Use the attached nurse only as character identity reference: adult woman, short brown bob, blue scrubs, white cardigan, dark shoes. Redraw completely as compact crisp pixel-art game character, 3.5-head-tall RPG proportions, viewed slightly from above, facing down toward player, neutral standing pose, entire body visible. One single character centered, no variants. Readable pixel clusters, restrained gray-blue palette, natural gentle face. Transparent background with absolutely no glow, halo, gradient, floor, shadow backdrop, text or scenery. Do not reproduce the tall detailed visual-novel illustration. Intended to stand beside a small pixel-art player on a tile map. Leave small transparent padding.

Validation: 81 checks passed, including distinct portrait/world textures. Windows build succeeded at 2026-10-16 17:01:28 KST with 0 errors and 0 warnings. Generated artwork inspected directly; this pass does not claim a full NPC interaction playthrough or a new end-to-end run. Build ZIP updated locally; GitHub release unchanged.
