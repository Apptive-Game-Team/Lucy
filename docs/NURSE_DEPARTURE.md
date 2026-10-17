# Nurse departure

The ward nurse now waits for the existing conversation, says she will open the exit, opens the room door, and runs through the room into the north corridor. The former global fade/deletion is removed. The actor is removed only after reaching (0, 14), beyond the stationary player's view. Movement and interaction are temporarily held and restored, including interrupted cleanup; duplicate departure triggers are ignored.

Eight generated poses provide right-facing and back-facing four-frame cycles at 12 fps, moving at 4.8 units/second. Tile running footsteps play every 0.16 seconds with distance attenuation and honor master mute. The hand-authored route passes (-6.56, 1.5), (-2, 1.5), (0, 1.5), (0, 14); this scripted actor disables its interaction collider during departure.

## Validation

2026-10-17 muted Unity Play fixture: a fresh run entered ward 0201 through PortalManager, placed the player near the room entrance, and sent the actual room-exit callback twice. This is a targeted fixture, not a complete manual playthrough. Logs/LucyNurseProbe.txt records changing positions and directional frames, with movement held during the run and final npc=False, move=True, interaction=True, barrier=False. Logs/LucyNurse-Running.png was visually inspected: the running nurse remains visible in the room, with transparent sprite background. The nurse reaches the north corridor exit before destruction. Footstep audio was not auditioned because tests remain muted.

## Generated art

Final game asset: `Assets/Resources/Presentation/NurseRun.png`. Generated with the builtin image generator, using NurseWorld as character reference, then edited with transparent_background=true to remove halos. Runtime alpha-trims the eight equal atlas cells.

Generation prompt:

> Pixel-art game sprite animation atlas of the SAME nurse as reference: adult woman short brown bob, blue scrubs, white cardigan and dark shoes. Exactly 4 columns x 2 rows, eight equal-sized cells, transparent background. Top row: 4-frame fast RUNNING cycle facing RIGHT, legs and arms visibly alternate, slightly leaning forward urgently. Bottom row: 4-frame fast RUNNING cycle facing AWAY / upward on a top-down RPG map, matching back view. Same small RPG proportions and crisp pixel clusters as reference, not a tall illustration. Every full-body pose fits strictly INSIDE its equal cell with 20 percent transparent padding and identical ground baseline, scale consistent in ALL cells. Do not let limbs cross cell boundaries. No props, no scenery, no shadows on ground, no glow, no text, no grid lines. Keep white cardigan, blue scrubs and short brown hair recognizable.

Background correction prompt:

> Remove the entire background and ALL soft halos/shadows from this sprite atlas. Keep the eight nurse running characters exactly unchanged, same position, same size and 4 columns by 2 rows. Everything outside the hard pixel silhouettes of the characters must be 100 percent transparent alpha, including the gaps between arms/legs. No gray/brown gradient, no glow, no floor. This is a game cutout sheet; only character pixels remain.

Windows build succeeded on 2026-10-17 at 10:16 KST: 0 errors, 0 warnings, 364405854 bytes. Updated Builds/Lucy-Windows.zip (125829091 bytes). Fresh editor validation after import: 85 passed, 0 failed, including the eight-pose transparency/footstep check. These changes are local and have not been published to the existing GitHub release.
