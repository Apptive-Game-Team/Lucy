# 마지막 야간 순회 — 아트 통합

2026-10-19 · `codex/last-night-round-blockout`

## 기본 진입

타이틀의 새 게임은 마지막 야간 순회를 시작한다. 사건 저장이 있으면 이어하기가 표시된다. 기존 병원 버전의 메뉴 진입은 제거하고 소스와 씬은 보존했다. 저장 경로와 버전 1 형식을 유지하여 기존 PoC 사건 저장을 읽는다. 새 게임을 누르는 것만으로 이전 저장을 덮어쓰지 않는다.

## 적용한 에셋

| 대상 | 기존 Resources 에셋 / 처리 |
|---|---|
| 다섯 구역·직원 통로 | HospitalSurfaces의 바닥 3종과 벽 마감 |
| 병상·간호 데스크 | HospitalFurnitureV2 |
| 설비 보관함·서류장·퓨즈함 | HospitalStorage |
| 표지판·조명 기구·대기 벤치 | HospitalWardProps |
| 통행문 | WardDoors 측면 닫힘/열림, 실제 통과 조건에 연결, 문틀 위치 유지 |
| 조사 지점 | 손전등 아이콘·기록판·보관함·출입문, 진행/회로 상태 표시등 |
| 루시 | Lucy_ver SH의 4방향 대기·보행 12프레임 |
| 서연 | NurseWorld 대기와 NurseRun 측면/뒷면 이동. 아래 방향은 정면 월드 포즈 사용 |
| 순회자 | WardOrderlyWalk의 정면·측면·뒷면 12프레임 |
| 조명 | WardAmbient와 FirstRoomGlow 기반 환경광·구역등·이동 방향 손전등. 설비 조명은 회로에 연결 |
| 소리 | Amb_darkroom 환경음과 타일 발소리. 메뉴 음량 설정 사용 |
| 포획·타이틀 | LucyDefeat / 기존 LucyTitle |

런타임 아틀라스는 셀별 투명 여백을 잘라 비율을 유지한다. 런타임 생성 Sprite/Material은 맵 종료 시 해제하며 Resources 텍스처는 파괴하지 않는다. 인물은 이동량을 따라 방향과 프레임이 바뀌며 발 기준점을 맞춘다. 문서·메뉴·포획·엔딩 중 보행 프레임은 정지한다. 가구 충돌은 실제 그림의 경계에 맞춘다.

상호작용 지점과 사건 선행조건, 두 결말, 추격 경로는 기존 PoC를 기준으로 한다. 전체 대본 확장, 최종 A/B/C 문 협동 연출, 조사·문 조작 전용 캐릭터 프레임과 전용 배전반 아트까지 완료했다는 뜻은 아니다. 현재 배전반은 재사용한 금속 설비함과 상태 표시로 표현한다.

## 검증

`Lucy.Editor.NightRoundArtValidation.Begin`은 별도 Unity batch 세션에서 타이틀 씬을 열고 전체 프로젝트 검사 및 아틀라스 로딩을 검사한 다음, 기본 새 게임 진입으로 두 결말을 가상 키보드만으로 완주한다. 실제 좌표·인벤토리·사건 상태를 주입하지 않는다. 테스트 전 사건 저장을 보관하고 종료 시 복구한다. 검증 세션은 무음이다.

결과: `Logs/NightRoundArtValidation.txt`, `Logs/NightRoundArtPlaytest.log`. 배치 실행은 화면 캡처를 지원하지 않는다. 일반 Editor 실행에서 타이틀과 첫 병동, 손전등 획득 후 조명까지 별도로 확인했다. 첫 병동 캡처는 `Logs/LucyVisual-LastNightRound_Blockout.png`다.

## 2026-10-19 검증 결과

- 프로젝트 검사 87개 통과, 실패 0. 진행 상태 검사 153개 포함.
- 함께 탈출: 10:54:06~10:55:32 KST, 키보드 완주 PASS.
- 먼저 탈출: 10:55~10:56:59 KST, 키보드 완주 PASS.
- 아틀라스 셀 로딩 검사 통과. 편집 모드 검사의 런타임 Sprite 해제를 DestroyImmediate로 수정하고 빌드 전 재검사.
- Windows 빌드 성공, 오류 0·경고 0. 실행 파일: Builds/Windows/Lucy.exe.
- 두 결말 완주는 Unity Editor에서 검증했다. 독립 실행 파일의 전체 완주는 별도 미실시.

## 후속 개선

구역별 배치·조사 완료 피드백·배전반·HUD·카메라의 추가 개선은 [탐색·가독성 개선](LAST_NIGHT_ROUND_REFINEMENT.md)을 참고하세요.
