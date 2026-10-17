# Hospital map overhaul

## Design

Replace the uniform generic map skin with ivory patient-room surfaces, sage nursing-wing floors, and charcoal utility/isolation floors. Generated hospital wall panels include plaster, lower painted dado and skirting. Add lit wall fixtures, ventilation, noticeboards and room identification at safe architectural landmarks. Keep established portal, puzzle, staircase, collision and nurse-route coordinates consistent; this pass rebuilds presentation and environmental dressing rather than changing the puzzle graph.

Generated assets use the builtin image generator. Final project assets are Assets/Resources/Presentation/HospitalSurfaces.png and HospitalWardProps.png. Surface atlas is opaque 2x2; prop atlas is transparent 4x2. Runtime crops/slices only; no external image processing.

## Surface generation prompt

Create a production-ready 2D top-down psychological horror hospital TILE TEXTURE ATLAS, exactly 2 columns by 2 rows with four equally sized square panels filling the entire canvas edge to edge. NO borders between panels, NO lettering, no objects, no perspective, no lighting gradients. Crisp restrained pixel-art texture matching a serious 16-bit hospital RPG. TOP LEFT: old ivory and cool gray hospital linoleum floor, fine square seams in a regular 8 by 8 tile grid, faint restrained scuffs, mostly clean readable surface. TOP RIGHT: faded sage-green hospital linoleum with regular small square seams, subtle wear. BOTTOM LEFT: dark charcoal blue-gray service corridor square floor tiles with restrained small cracks. BOTTOM RIGHT: straight-on hospital wall surface, upper two thirds aged warm off-white painted plaster, lower third muted sage green paint, thin horizontal dark baseboard along bottom edge. Architectural texture only. Flat albedo, no baked dramatic shadows, high material clarity, neutral mid brightness so game engine lighting supplies darkness. No blood, no grime blobs, no gradients, no diagonal or isometric lines.

## Props generation prompt

Transparent background production GAME PROP SPRITE ATLAS. Exactly 4 columns x2 rows, 8 equal cells, each isolated object fully within its cell with 20% empty padding. All in one coherent restrained pixel art style, top-down RPG three-quarter view, crisp clusters, cold ivory/sage hospital palette, no text, no floor, no shadows outside silhouettes. Top row left to right: 1 abandoned folded wheelchair, 2 tall IV drip stand with small bag, 3 sage hospital privacy curtain on compact frame, 4 medical trolley with medicine bottles. Bottom row left to right: 5 rectangular wall noticeboard with tiny illegible paper sheets, 6 rectangular metal wall ventilation grille, 7 slim horizontal fluorescent wall lamp in metal casing OFF no glow, 8 two-seat waiting-room bench dark sage vinyl. Consistent scale logic, entire objects visible, do not cross cell boundaries, no connecting background. Serious eerie abandoned hospital not cartoon toy. Wall mounted objects shown straight frontal, floor objects shown slight top-down. Alpha background fully transparent.

## Storage generation prompt

Production 2D horror hospital game sprite atlas, exactly2 columns by2 rows four equally sized cells. Transparent background outside objects. Four isolated FRONT-FACING standing hospital storage furniture sprites with slight top visible matching top-down RPG, straight vertical edges no isometric perspective. Top left tall ivory metal GLASS MEDICAL CABINET with sterile bottles and folded supplies visible, top right low wide cream BEDSIDE DRAWER CHEST three drawers with dark handles, bottom left tall narrow faded sage PATIENT LOCKER with vents and label slot, bottom right wide MEDICAL RECORDS SHELVING CABINET with paper files and small locked lower doors. Restrained crisp pixel-art, ivory and faded sage paintedmetal agedbutfunctional, serious psychiatric hospital after blackout. No red furniture, no books that look like home library, no household decor, no text. Entire object fits each cell with 15percent fully transparent padding. No shadows/glows/floor/background. Distinct clean silhouettes with consistent pixel scale.

Final generated asset: `Assets/Resources/Presentation/HospitalStorage.png`, transparent 2x2 atlas.

## Verification

Muted Play visual captures were inspected for the first room, stairwell, nursing wing, service puzzle and isolation maze. First-room storage was replaced after the first inspection exposed the old household furniture; the IV stand was then moved beside the bed and the ward noticeboard explicitly mounted on the north wall. The unused purple camera background was replaced with near-black. Service platforms gained restrained local emergency light and narrow safety edging.

Map-preview is a muted fixture using PortalManager transitions, with a real inventory flashlight injected/equipped for visibility after the first-room capture. It is not a manual puzzle completion run. Original scene geometry, puzzle graph, colliders and portal destinations are unchanged. Some safe-placement wall details are omitted when no suitable wall is found. Only selected generated prop cells are used; there is no random scatter of furniture. Drawer interaction state is preserved, but its new chest artwork currently has one visual pose.

Final build and integration results follow below.

2026-10-17 01:29:36 UTC: lifecycle/portal smoke passed through all configured route transitions, failure restart, ending and return to title; game runtime errors 0, plugin runtime errors 0. A separate new game then used actual movement/E/I inputs to navigate around the cabinet to the drawer at (2.79, -1.16), open it, collect the flashlight and open inventory. Observed tutorial=3 and mental=100 at 01:31:18 UTC confirm acquisition/inventory steps. No item injection was used in this drawer check.


Final Windows build succeeded 2026-10-17 10:31:52 KST: 0 errors, 0 warnings, 381769574 bytes. Project validation: 86 passed, 0 failed. Builds/Lucy-Windows.zip refreshed. These local changes are not yet published to GitHub release.
