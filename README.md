# Lucy
Lucy (24.08.01 ~) 
> 공포 어드벤처 게임으로, 플레이어는 어둠 속에서 정신력과 체력을 관리하며 퍼즐을 풀고 생존해야 합니다.

## 🎮 Game Play 
🎮 [Play Lucy on GitHub Pages](https://apptive-game-team.github.io/Lucy/)
자동 빌드 및 배포는 GitHub Actions를 통해 이루어집니다.  
자세한 CI/CD 설정 방법은 [CICD_SETUP.md](docs/CICD_SETUP.md)를 참고하세요.

## 🎮 Game Features

- **정신력(Mental) 시스템**: 어둠 속에서 정신력이 감소하며, 낮은 정신력 상태에서는 환각 효과가 나타남
- **체력(Stamina) 시스템**: 달리기 시 소모되며, 일정 수치 이하로 떨어지면 달리기 불가
- **인벤토리 시스템**: 아이템 수집, 장착, 사용 기능
- **포털 시스템**: 씬 전환 및 플레이어 위치 이동
- **크리처 AI**: 다양한 행동 패턴을 가진 몬스터 (Guard, Avoider, Stunnee)
- **퍼즐 시스템**: 다양한 상호작용 가능한 퍼즐

## 🏗️ Architecture Overview

### Core Systems

#### 1. Singleton Pattern (`SingletonObject<T>`)
게임의 핵심 매니저들은 싱글톤 패턴을 사용하여 씬 전환 시에도 유지됩니다:
- `InputManager`: 입력 관리 및 키 바인딩
- `PortalManager`: 씬 전환 및 포털 관리
- `ReferenceManager`: 게임 오브젝트 참조 관리
- `SoundManager`: 사운드 재생 관리
- `CreatureManager`: 크리처 AI 및 맵 관리

#### 2. Event System
- `ISceneChangeListener`: 씬 변경 이벤트 리스너 인터페이스
- `IKeyInputListener`: 키 입력 이벤트 리스너 인터페이스
- `EventScheduler`: 게임 이벤트 스케줄링

#### 3. Player Systems
- **CharacterStat**: 플레이어의 정신력과 체력 관리
- **CharacterMove**: 플레이어 이동 및 애니메이션
- **Inventory**: 아이템 인벤토리 관리

#### 4. AI & Pathfinding
- **CreatureManager**: 맵 데이터 관리 및 경로 탐색
- **PathFinder**: A* 기반 경로 탐색 알고리즘
- **Creature**: 다양한 AI 행동 패턴 (추격, 회피, 기절 등)

### Directory Structure

```
Assets/Scripts/
├── Lucy/                    # 플레이어 관련 스크립트
├── InputSystem/             # 입력 관리 시스템
├── Portal/                  # 씬 전환 및 포털
├── ReferenceSystem/         # 참조 관리 시스템
├── soundSystem]/            # 사운드 시스템
├── Scripts_Creatures/       # 크리처 AI
├── invertoryAndItem/        # 인벤토리 및 아이템
├── Puzzle/                  # 퍼즐 시스템
├── Dialogue/                # 대화 시스템
├── Event/                   # 이벤트 시스템
└── Password_Object/         # 비밀번호 오브젝트
```

## 💡 Code Examples

### Using the Singleton Pattern
```csharp
// Accessing a singleton manager
InputManager.Instance.SetMovementState(true);
PortalManager.Instance.SetSceneChangeListener(this);

// Creating a custom singleton
public class MyManager : SingletonObject<MyManager> {
    protected override void Awake() {
        base.Awake();
        // Your initialization code
    }
}
```

### Scene Change Listener
```csharp
public class MyScript : MonoBehaviour, ISceneChangeListener {
    void Start() {
        // Register to receive scene change notifications
        PortalManager.Instance.SetSceneChangeListener(this);
    }
    
    void ISceneChangeListener.OnSceneChange() {
        // Handle scene change
        Debug.Log("Scene changed!");
    }
}
```

### Managing Character Stats
```csharp
// Reduce stamina when running
CharacterStat.instance.ChangeStamina(-10);

// Stop mental reduction when equipped with flashlight
CharacterStat.instance.StopMentalReduce();

// Check if player can run
if (CharacterStat.instance.canRun) {
    // Enable running
}
```

### Adding Items to Inventory
```csharp
// Add item to inventory
bool success = Inventory.instance.AddItem(itemData);
if (success) {
    Debug.Log("Item added successfully");
}

// Check if item is equipped
bool isEquipped = Inventory.instance.IsItemEquipped(flashlightItem);
```

### Registering Objects with ReferenceManager
```csharp
// Register an object for global access
ReferenceManager.Instance.SetReferableObject("MainCamera", cameraComponent, false);

// Find object by key
CameraMove camera = ReferenceManager.Instance.FindComponentByName<CameraMove>("MainCamera");
```

## commit convention

|태그|설명|
|---|---|
|feat|새로운 코드 추가|
|fix|문제점 수정|
|refact|코드 리팩토링|
|comment|주석 추가(코드 변경X) 혹은 오타 수정|
|docs|README와 같은 문서 수정|
|merge|merge|
|rename|파일, 폴더명 수정 혹은 이동|
```
ex) feat(파일명)
    시작 이벤트 구현
```
## contributor

<table>
  <tr>
    <td align="center" width="200px">
      <a href="https://github.com/Monolong" target="_blank">
        <img src="https://avatars.githubusercontent.com/u/83206119?v=4" alt="문성필 프로필" />
      </a>
    </td>
    <td align="center" width="200px">
      <a href="https://github.com/Gimlocal" target="_blank">
        <img src="https://avatars.githubusercontent.com/u/127363458?v=4" alt="김현진 프로필" />
      </a>
    </td>
    <td align="center" width="200px">
      <a href="https://github.com/YunseongJeong" target="_blank">
        <img src="https://avatars.githubusercontent.com/u/88422717?v=4" alt="정윤성 프로필" />
      </a>
    </td>
    <td align="center" width="200px">
      <a href="https://github.com/Jinwook700" target="_blank">
        <img src="https://avatars.githubusercontent.com/u/127014921?v=4" alt="정진욱 프로필" />
      </a>
    </td>
    <td align="center" width="200px">
      <a href="https://github.com/hwanginseop" target="_blank">
        <img src="https://avatars.githubusercontent.com/u/163392234?v=4" alt="황인섭 프로필" />
      </a>
    </td>
  </tr>
  <tr>
    <td align="center">
      <a href="https://github.com/Monolong" target="_blank">
        문성필
      </a>
    </td>
    <td align="center">
      <a href="https://github.com/Gimlocal" target="_blank">
        김현진
      </a>
    </td>
    <td align="center">
      <a href="https://github.com/YunseongJeong" target="_blank">
        정윤성
      </a>
    </td>
    <td align="center">
      <a href="https://github.com/Jinwook700" target="_blank">
        정진욱
      </a>
    </td>
    <td align="center">
      <a href="https://github.com/hwanginseop" target="_blank">
        황인섭
      </a>
    </td>
  </tr>
</table>
