# Last Night Round — visual cohesion

2026-10-20. Original Lucy remains the reference: small pixel clusters, muted ivory and cool gray, simple shading and compact RPG proportions. Her original twelve sprites are unchanged.

## Room and interaction presentation

- Removed persistent world-space room names, prop names and completion bars. Only small A/B/C plates remain attached to the final doors.
- Investigation prompts appear for the nearest actionable object. A small focus glint is attached to that object's visible location.
- Noticeboards sit on a visible wall above records desks. Small evidence and tools sit on bedside cabinets or trolleys. Breakers are mounted on the wall; the manual handle is attached to B.
- Wall faces, wainscoting, skirting and contact shadows connect the furniture to the room. Quiet vinyl tiles replace the noisy, fine-grained surface textures.
- The large permanent objective HUD is replaced by a Tab/button objective panel. Room names appear briefly on arrival. Circuit status appears only while inspecting a breaker; pursuit warnings remain readable.
- Story and ending gates are unchanged. The wall-speaker investigation point moved from `(53,-3)` to `(53,-4)` to bring interaction closer to its mounting.
- The ward has a treatment bay with a privacy screen, nursing has a central work area, and records has a reading desk. The staff route at x=21 is kept clear of the nursing trolley.

## Typography

The current title/session screens, gameplay HUD, dialogue, door plates and epilogues use Noto Sans KR. Headings use Bold and body/controls use Regular. The original Maplestory font remains for legacy assets. Dialogue body text is 22 px at the 1600×900 reference size, with a separate 32 px heading and a scrollable body area for longer records. Session UI uses 1.15 line spacing.

The unmodified static font files were copied from the installed Noto Sans KR family into `Assets/Resources/Presentation/NotoSansKR-Regular.ttf` and `NotoSansKR-Bold.ttf`. The [official SIL OFL 1.1 license](https://raw.githubusercontent.com/google/fonts/main/ofl/notosanskr/OFL.txt) is included as `NotoSansKR-OFL.txt`; the Windows build also copies the notice beside the executable.

## Art provenance and slicing

Generated with the built-in image generation tool, using the original Lucy sprite as the style reference. Previous presentation sheets remain available to legacy scenes.

- `Assets/Resources/Presentation/NightRoundActors.png`: nurse and orderly, four facing directions with three frames each. Consumed by `NightRoundActorView`.
- `Assets/Resources/Presentation/NightRoundProps.png`: sixteen hospital furniture, wall fixture and door sprites. Consumed by the map and dawn epilogue.

The delivered atlases have uneven gutters. `NightRoundArt` records explicit separators, including per-row column separators for the prop sheet. Each separator was inspected against the alpha channel so sprites are not cut at assumed equal-cell boundaries. Dimensions are checked before slicing. Both imports use point filtering, alpha transparency, uncompressed textures, no mipmaps and readable pixels for trimming. The original sprites and generated source images were not modified.

### Actor generation prompt

Use case: style-transfer. Project game asset, not concept art. Create ONE transparent PNG sprite animation atlas 6 columns by 4 rows, precisely equal cells with generous transparent gutters and NO grid lines or text. Use first reference Lucy as strict pixel-art style: chunky visible pixel clusters, small muted palette, dark brown outlines, simple cool-gray/lavender shading, 3-head-tall RPG proportions, no smooth anti-aliased anime rendering or realistic textures. Second reference only identifies nurse Seoyeon: short brown bob, ivory coat over muted blue scrubs, black shoes, adult woman; redraw in Lucy's exact visual language. Each row LEFT THREE cells is this same nurse, RIGHT THREE cells a tall hospital orderly in desaturated dark teal work uniform, shadowed face, no gore. Four rows from TOP: front-facing, facing RIGHT, back-facing, facing LEFT. Within each trio: idle feet together, walk left foot forward, walk right foot forward. All 24 complete figures separately contained in their own equal cell, consistent scale and feet baseline; nurse 3 heads tall and orderly 3.5 heads tall. Every frame retains same clothing, silhouette, face and palette. Transparent background, no ground, no shadow, no props, no captions. Aim logical pixel detail of ~48x80 per figure enlarged with hard nearest-neighbor pixels. This must visually belong alongside reference Lucy, not be a detailed illustration.

References: `Lucy_ver SH/sprite/lucy_sprite_front.png`, `Presentation/NurseWorld.png` under Resources.

### Prop generation prompt

Use case: style-transfer. Create a production transparent PNG sprite atlas for a 2D hospital RPG matching FIRST reference Lucy, with thick crisp pixel clusters and limited muted ivory / gray lavender / dusty sage / dark brown palette. References 2 and 3 identify objects ONLY; replace their fine realistic rendering with Lucy's small simple pixel-art language. STRICT 4 columns x 4 rows of sixteen equal cells, each isolated object centered fully inside its cell, transparent gutters, no grid, no captions, no text. Top row left to right: 0 horizontal hospital bed with pillow and sage blanket, 1 nursing desk with blank papers on top, 2 standing electrical control cabinet, 3 archive bookcase. Row2: 4 wheelchair, 5 medicine trolley, 6 privacy screen curtain, 7 two-seat waiting bench. Row3: 8 wall-mounted noticeboard with four blank cream paper rectangles, 9 wall ventilation grille, 10 small wall light fixture, 11 glass medicine cupboard. Row4: 12 front-view closed hospital door, 13 matching front-view door open showing dark doorway, 14 narrow SIDE-view closed door for east-west corridor, 15 SAME side-view door open with visible hinges and thin leaf. Every object same gentle top-down RPG viewpoint as Lucy; furniture shows a little top surface and front face, wall items straight on. Low logical resolution ~64-96 pixels per object enlarged with nearest-neighbor hard square pixels, no gradients or photorealistic detail, no gritty stippling, no soft anti-aliasing. Quiet atmospheric hospital, readable silhouettes. Real alpha transparency outside objects. Do not include people or scenery.

References: original Lucy, `Presentation/HospitalFurnitureV2.png`, `Presentation/HospitalWardProps.png` under Resources.

## 검증

- 2026-10-20: 프로젝트 검사 87개 통과, 신규 아틀라스/한글 폰트 로딩 통과.
- Unity Editor 가상 키보드 입력으로 함께 탈출/먼저 탈출 모두 통과. 실제 문 통과, 붙잡힘 후 체크포인트 복구, 엔딩 일시정지/재개 및 R 재시작 확인.
- 병실/간호 구역/설비/기록실/최종 구역과 긴 기록 화면을 캡처해 배치 및 한글 표시 확인.
- Windows 빌드 성공: 오류 0, 경고 0. 배포 폴더에 OFL 라이선스 포함 확인. 독립 실행 파일에서 전체 경로를 다시 플레이한 것은 아님.
