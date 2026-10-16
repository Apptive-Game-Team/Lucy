# 생성 이미지 에셋

LUCY — 침묵의 병동의 병원 배경을 표현하기 위해 내장 `image_gen`으로 생성한 이미지의 제작 기록이다. 이 문서는 에셋의 파일 정보와 용도를 기록하며 빌드 성공이나 처음부터 엔딩까지의 플레이 검증 완료를 뜻하지 않는다.

## 병원 소품 아틀라스

- 파일: `Assets/Resources/Presentation/HospitalProps.png`
- Resources 경로: `Presentation/HospitalProps`
- 실제 저장 크기: **1254 × 1254 px**, PNG, RGBA 8비트. 최초 전달된 1280 × 1280 설명과 달리 현재 파일의 PNG IHDR 값은 1254 × 1254다.
- 배경: 투명. 소품 주변 여백을 포함한 2 × 2 배열.
- 구성: 왼쪽 위 병상, 오른쪽 위 의료 카트, 왼쪽 아래 간호 데스크, 오른쪽 아래 이동식 가림막.
- 미술 방향: 민트색·크림색·회색 금속을 사용한 병원 소품, 세부가 있는 픽셀 아트, 약간 위에서 내려다보는 게임 시점.
- 사용: `HospitalEnvironment.LoadProps()`에서 실제 텍스처 너비·높이를 절반으로 나눈 사분면을 기준으로 각 소품의 투명 여백을 제외하여 스프라이트를 만든다. 1280 또는 640 같은 고정 픽셀 크기에 의존하지 않는다.
- 현재 Unity import: Sprite / Single, 100 PPU, bilinear filtering, clamp, mipmap 비활성화, 최대 크기 2048, alpha transparency 활성화. 개별 소품은 런타임에서 분할한다.

생성 프롬프트 원문:

```text
Create a production 2D top-down adventure game sprite atlas, square canvas exactly 2 columns by 2 rows, four separated complete objects centered in equal quadrants with wide empty transparent padding. Hospital psychiatric ward props. Pixel art, crisp outlines, muted hospital mint cream and steel gray, restrained dark shadows, consistent orthographic top-down three-quarter view suitable RPG overhead room maps. TOP LEFT: one single empty hospital bed, white pillow and neatly folded pale mint blanket, metal rails. TOP RIGHT: one compact medical supply trolley with drawers and small medicine bottles. BOTTOM LEFT: a compact nurse reception desk with monitor and file folders. BOTTOM RIGHT: one folded three-panel hospital privacy screen on wheels. All isolated with transparent background, no floor, no people, no text, no numbers, no labels, no grid lines, no shared shadows across quadrants, no objects touching quadrant boundaries. Each object entirely inside its quadrant. Same scale and lighting style throughout. Actual game sprite assets, not a screenshot or mockup.
```

## 병원 리놀륨 바닥

- 파일: `Assets/Resources/Presentation/HospitalFloor.png`
- Resources 경로: `Presentation/HospitalFloor`
- 실제 저장 크기: **1254 × 1254 px**, PNG, RGB 8비트. 알파 채널 없음.
- 용도: 병동의 기존 바닥을 병원 리놀륨으로 표현하기 위한 민트색 바닥 텍스처.
- 사용: `HospitalEnvironment`가 `Presentation/HospitalFloor`를 읽어 바닥 표현에 적용한다.
- 현재 Unity import: Sprite / Single, 100 PPU, bilinear filtering, clamp, mipmap 비활성화, 최대 크기 2048. Import 설정의 alpha transparency는 켜져 있지만 원본 PNG는 RGB다.

생성 프롬프트 원문:

```text
A single seamless tileable square hospital vinyl floor texture for a top-down 2D pixel art psychiatric hospital game. Orthographic directly overhead flat surface only. Muted pale mint gray-green linoleum with subtle fine speckles and slight aging, one restrained hairline grout seam only along the outer tile edges, seamless repeat in both directions. Low contrast and even flat lighting so characters and props remain easy to read. Crisp pixel art texture at game asset scale. No perspective, no walls, no objects, no shadows, no text, no borders, no checkerboard pattern, no glossy diagonal highlights. Entire image filled with the floor material.
```

## 타이틀 이미지

`Assets/Resources/Presentation/LucyTitle.png`의 미술 방향과 실제 생성 프롬프트는 `docs/GAME_DESIGN.md`의 아트 항목에 기록되어 있다.

## 확인 범위

2026-10-16 기준 두 파일의 존재, PNG 헤더의 치수·색상 형식, Unity `.meta` 설정과 소품 분할 코드를 읽어 확인했다. 소품 이미지의 2 × 2 구성도 확인했다. 이미지 생성 결과만으로 반복 경계, 배치 충돌, 게임 화면에서의 가독성이나 플레이 완주 여부가 검증되지는 않는다.

## Visual repair, 2026-10-16
Generated with the built-in image generation tool. Files are project-local; alpha is preserved on the furniture atlas.
- `HospitalFurnitureV2.png`: transparent four-quadrant pixel-art atlas, horizontal examination bed / vertical bed / nurse medication counter / glass medicine cabinet. Prompt: flat orthographic top-down RPG perspective, limited desaturated gray-green and ivory palette, simple crisp silhouettes, no diagonal perspective, no labels or ground shadows. Furniture is fitted without rotating perspective art. Existing furniture colliders stay authoritative; the starting room has one explicitly placed patient bed with matching collision.
- `HospitalVinylV2.png`: seamless neutral gray vinyl tile, faint border grout, very restrained low-contrast scuffs, no marble streaks, no mint color, matte top-down pixel-art surface. Runtime floor visuals preserve original aspect ratios and navigation colliders.
Supersedes the bright HospitalFloor overlay and randomly placed HospitalProps dressing. Old files retained for history but no longer used by HospitalEnvironment.
- `FlashlightIcon.png`: built-in generated transparent pixel-art emergency flashlight icon, diagonal full silhouette, dark gray metal with ivory lens, no beam/text/background. Replaces a missing item-icon GUID.
- `Nurse.png`: built-in generated hospital night nurse with blue-gray scrubs, white cardigan and ID badge; full-body anime RPG sprite, transparent cutout. A second built-in edit extracted the silhouette. Replaces the test meme face and Lucy portrait reused for the nurse. Alpha was sampled outside the body (zero).
- `WardShadow.png`: built-in generated faceless shadow apparition in a charcoal orderly coat, restrained eyes, no pumpkin face/gore; transparent RPG enemy sprite. Existing AI, root transforms and trigger shapes remain unchanged.
