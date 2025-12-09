# WebGL CI/CD Setup Guide

이 문서는 Lucy 프로젝트의 WebGL 빌드 및 배포를 위한 CI/CD 파이프라인 설정 방법을 설명합니다.

## 개요

GitHub Actions를 사용하여 Unity WebGL 프로젝트를 자동으로 빌드하고 GitHub Pages에 배포합니다.

## 필요한 설정

### 1. Unity License 설정

GitHub Actions에서 Unity 프로젝트를 빌드하기 위해서는 Unity 라이선스 정보가 필요합니다.

#### Repository Secrets 추가

다음 secrets를 GitHub repository settings에 추가해야 합니다:

1. **UNITY_LICENSE**: Unity 라이선스 파일의 내용
2. **UNITY_EMAIL**: Unity 계정 이메일
3. **UNITY_PASSWORD**: Unity 계정 비밀번호

#### Unity License 생성 방법

1. 로컬에서 Unity를 실행하고 로그인합니다
2. 다음 명령어를 실행하여 라이선스 파일을 생성합니다:
   ```bash
   # Windows
   Unity.exe -batchmode -createManualActivationFile -quit
   
   # Mac
   /Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -createManualActivationFile -quit
   ```
3. 생성된 `.alf` 파일을 Unity license server에 업로드하여 `.ulf` 파일을 받습니다
4. `.ulf` 파일의 내용을 `UNITY_LICENSE` secret에 추가합니다

또는 더 쉬운 방법으로 [Unity - Request activation file](https://license.unity3d.com/manual) 워크플로우를 사용할 수 있습니다.

### 2. GitHub Pages 설정

1. Repository Settings > Pages로 이동
2. Source를 "GitHub Actions"로 설정
3. 이제 main 브랜치에 push할 때마다 자동으로 빌드 및 배포가 실행됩니다

## Workflow 구조

### 빌드 Job (`build`)

- Ubuntu 환경에서 실행
- Unity Library 폴더 캐싱으로 빌드 속도 향상
- Unity Builder action을 사용하여 WebGL 빌드
- 빌드 결과물을 artifact로 업로드

### 배포 Job (`deploy`)

- 빌드 job이 성공한 후 실행
- main 브랜치에 push된 경우에만 실행
- GitHub Pages에 빌드 결과물 배포

## Workflow 트리거

- `push`: main 브랜치에 push될 때
- `pull_request`: main 브랜치로의 PR이 생성/업데이트될 때
- `workflow_dispatch`: 수동으로 실행

## 배포된 사이트 접근

빌드 및 배포가 완료되면 다음 URL에서 게임을 플레이할 수 있습니다:
```
https://apptive-game-team.github.io/Lucy/
```

## 문제 해결

### 빌드 실패 시

1. Actions 탭에서 실패한 workflow 확인
2. 로그를 확인하여 에러 메시지 확인
3. Unity 버전이 `ProjectVersion.txt`와 일치하는지 확인
4. Secrets가 올바르게 설정되었는지 확인

### 배포 실패 시

1. Repository Settings > Pages에서 GitHub Actions가 source로 설정되어 있는지 확인
2. Repository Settings > Actions > General에서 Workflow permissions가 "Read and write permissions"로 설정되어 있는지 확인

## 추가 개선 사항

향후 다음과 같은 개선을 고려할 수 있습니다:

- 빌드 최적화 (압축 설정, 코드 스트리핑)
- 자동 버전 태깅
- Slack/Discord 알림 통합
- 빌드 성능 모니터링
