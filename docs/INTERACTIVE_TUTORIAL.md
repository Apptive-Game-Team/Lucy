# First-room interactive tutorial

The existing objective HUD now guides seven observed actions: move 0.7 units, collect the drawer flashlight, open inventory, equip flashlight, read the first record, open the room door, and reach the stair scene. Instructions stay in the upper-right objective panel; the bottom interaction prompt remains unchanged. Earlier actions are remembered even when performed out of order. There are no timers that automatically complete a step and no mandatory tutorial locks.

T dismisses the tutorial during gameplay. New Game resets it. Checkpoint retries preserve learned actions; existing inventory/record state also counts. Guidance is shown only in the first room/first stair scene. Reaching stairs early asks the player to return for missing steps. Completion shows a short resource-management tip before restoring normal objectives.

Before flashlight equipment is learned, mental drain is suspended only in the first scene. Skipping, leaving the scene, or equipping the flashlight ends that protection. Existing flashlight/battery rules still apply.

No new image assets were needed. Existing objective typography and interaction affordances are reused.

## Validation

Muted Unity Play session 4da87e247f8a4db0866f4a00212a2478: started a fresh game, then used actual movement/E/I inputs and inventory mouse clicks. No items were injected and no player/scene teleport fixture was used. Observed step 0 at spawn with mental=100; movement advanced to step 1. The drawer opened and yielded one flashlight; inventory opening advanced to equipment instruction. Clicking the actual Equip button completed equipment. Reading record 01 advanced to door instruction; opening the door advanced to stairs. Walking through the corridor and pressing E at the stairs entered 0102 with tutorial=7, records=1, mental=100 and battery=6 at 2026-10-16T08:53:38Z. Screenshots confirmed readable objective instructions alongside inventory and the separate interaction prompt.

Editor regression check covers skipping, new-run reset, out-of-order door/stair actions not completing earlier steps, and hiding the tutorial in later scenes. Checkpoint retry preservation follows run-local state; it was not separately replayed in this pass.

Windows build including tutorial and nurse departure succeeded on 2026-10-17 at 10:16 KST: 0 errors, 0 warnings. Fresh editor checks: 85 passed, 0 failed. Builds/Lucy-Windows.zip updated; existing GitHub release is unchanged.
