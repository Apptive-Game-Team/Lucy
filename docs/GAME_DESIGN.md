# LUCY — 침묵의 병동

## 의도

정신병원을 배경으로 하는 한국어 2D 공포 어드벤처다. 기존 맵, 캐릭터, 상호작용과 퍼즐을 유지하며 시작부터 탈출까지 이어지는 플레이를 만든다. 핵심 감정은 불 꺼진 병동에서 느끼는 고립과 불안, 출구에 도달했을 때의 안도감이다. 배경은 2026-10-16 사용자가 직접 확정한 설정이며, 기존 타일의 외형으로 다른 장소를 추정하지 않는다.

## 플레이 루프

1. 병동에서 깨어나 가구와 단서를 조사한다.
2. 손전등·배터리·정신력을 관리하면서 계단과 병실을 탐색한다.
3. 연결 병동 스위치를 우측 1·2·3 → 좌측 4·5 → 우측 6 순서로 켜고 마지막 점검 기록을 회수한다. 이 기록으로 비상 통로가 열린다. 슬라이드 퍼즐은 보존된 레거시 기능이며 현재 배포 씬에는 배치되지 않는다.
4. 소리를 감지하는 적을 피해 마지막 미로를 통과한다.
5. 출구에서 에필로그와 클리어 시간을 확인한다. 실패 시 최근 기록 지점에서 계속하거나 새 게임 또는 타이틀로 돌아갈 수 있다.

달리기는 빠르지만 체력과 발소리의 대가가 있다. 빛은 정신력을 보존하지만 배터리가 필요하다. 퍼즐/일시정지 중에는 자원이 소모되지 않는다. 각 공간의 짧은 목표 안내는 길을 찾을 실마리를 제공하면서 퍼즐 답은 드러내지 않는다.

## 조작

- 방향키 / WASD: 이동
- Shift: 달리기
- Z / E: 조사·대화
- I: 인벤토리
- 마우스: UI 및 퍼즐
- ESC: 일시정지 및 조작 도움말. 기존 퍼즐의 닫기 버튼은 유지한다.
- E / Z: 빛나는 기록 가까이에서 읽기. Space / ESC: 기록 접기. J: 수집 목록.

## 야간 대피 이야기

정전 뒤 병동에서 깨어난 루시는 자신을 찾으러 돌아온 간호사의 기록을 따라간다. 1층 침상 메모는 손전등과 간호실로 안내한다. 2층 야간 기록은 대피 상황과 연결 통로 스위치 순서를 알려준다. 연결 통로 끝 점검표를 얻으면 비상 전력을 복구한 것으로 처리되어 3층 출입이 열린다. 3층 입구의 편지는 대피 명단에 루시의 자리가 남아 있음을 알려주고, 네 기록을 모은 채 출구에 도착하면 그 이름을 부르는 목소리로 이야기가 끝난다.

각 기록을 처음 읽을 때 정신력 20 회복(최대 정신력 한도 적용)과 함께 그때의 씬·플레이어 위치를 체크포인트로 등록한다. 2·3·4번 기록은 최초 읽기 때 손전등 배터리도 최대 3칸 보충하며 용량을 넘기지 않는다. 같은 기록을 다시 읽으면 보상과 체크포인트 등록이 중복되지 않는다. 독서 중 시간·입력·자원소모가 정지한다. 기록 상태는 새 게임에서 초기화한다. 기록을 보지 않고 마지막 출구에 도착하면 빠진 기록의 위치를 안내한다.

인게임 맵은 입원 병동, 간호실·치료 병동, 비상 연결 통로, 폐쇄 병동으로 구별한다. 바닥을 생성한 민트색 병원 리놀륨으로 교체하고, 실제 빈 공간에 병상·의료카트·간호데스크·가림막과 표지를 배치한다. 기존 문, 충돌체, 스위치와 포털 위치는 보존한다.

## 런과 기록

‘처음부터 재도전’과 새 게임은 영속 매니저, 플레이어, 인벤토리, 퍼즐·수집 기록·체크포인트를 초기화한다. ‘최근 기록에서 계속’은 마지막으로 처음 읽은 기록 지점으로 돌아가며 인벤토리, 수집 기록, 다리 스위치 상태와 누적 플레이 시간을 유지한다. 복구 시 정신력은 현재 최대치 이내에서 최소 60, 배터리는 최소 3칸, 체력은 최대치로 회복한다. 이는 실행 중인 런의 메모리 체크포인트이며 게임을 종료한 뒤 이어지는 디스크 저장 기능은 아니다. 최고 탈출 시간은 `Lucy.BestClearSeconds` PlayerPrefs로 저장한다.

정신력은 기본적으로 5초마다 2 감소(빛 없이 약 250초), 손전등은 9칸 × 45초로 약 405초를 제공한다. 다리 밖 위험 구역은 자원 소모가 3배다. 새 씬의 적은 플레이어가 도착 위치에서 1.5 이상 벗어난 뒤 게임 시간 8초가 지나야 탐지·포획을 시작한다. 기록 읽기와 일시정지 중에는 이 보호 시간이 줄어들지 않는다. 전체 음량은 `Lucy.Volume`에 저장한다.

체크포인트 복구, 기록 보급과 도착 보호는 현재 구현된 동작을 기술한 것이며, 이 문서 갱신 자체가 실제 플레이 검증 완료를 뜻하지 않는다.

## 아트

기존 레벨의 타일과 캐릭터는 보존했다. 새 타이틀 배경은 `Assets/Resources/Presentation/LucyTitle.png`이며 내장 image_gen으로 생성했다. 새 UI는 프로젝트에 이미 있던 Maplestory Light 폰트를 사용한다.

타이틀에는 병동 관찰창, 벽 손잡이, 간호 스테이션, 의료 카트, 환자복을 사용해 장소가 명확히 드러나도록 한다. 공포의 중심은 비어 있는 병원 공간과 알 수 없는 발소리다. 기존 침대·약병 에셋과 원래 대사는 유지한다.

생성 방식: 내장 image_gen. 최종 생성 프롬프트:

“Use case: stylized-concept. Asset type: finished 16:9 landscape title background for LUCY, a Korean 2D psychological horror adventure set in a psychiatric hospital at night. The setting must read clearly as a hospital ward, not a school. Restrained detailed pixel art with fine dithering matching a dark 2D game, desaturated teal and navy with a small warm flashlight. Hospital corridor with pale mint lower walls, cream upper walls, protective wall handrails, clinical doors with narrow observation windows, a recessed nurses' station and a medical trolley, worn vinyl floor, dim fluorescent ceiling fixtures. A lone small young female protagonist seen from behind wears a simple pale hospital patient gown and holds a flashlight on the right half. Atmospheric mystery comes from the empty institutional space and darkness. No monstrous patients, no restraints, no gore. Keep the left 42 percent near-black low-detail negative space for the existing title and menu. Strong perspective into the right-center, cinematic light, polished game illustration. No text, letters, logo, watermark, school desks, lockers, school uniforms, or classroom chalkboards.”

## 검증 기준

타이틀→새 게임, 조작 도움말, 이동·조사·인벤토리, 일시정지 중 자원 보존, 모든 포털 데이터와 빌드 씬의 연결, 퍼즐 정답/리셋, 실패→재시작, 탈출→에필로그를 확인한다. 코드 검증·자동 경로 검사와 실제 플레이 검증은 구분하여 기록한다. 무결점은 보증하지 않으며 실제 확인한 범위만 완료로 보고한다.
