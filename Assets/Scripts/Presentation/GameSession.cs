using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lucy.Presentation
{
    /// <summary>Owns the complete run lifecycle without changing authored level layouts.</summary>
    public sealed class GameSession : MonoBehaviour
    {
        public const string FirstScene = "0101_09.18";
        public static GameSession Instance { get; private set; }
        public static bool BlocksGameplayInput => Instance != null && Instance.blocked;
        private bool blocked, loading, failed, paused, documentOpen;
        private bool capturePlaying;
        private float documentOpenedAt;
        private float elapsed, previousScale = 1;
        private GameObject screen, hud;
        private Font font, headingFont;
        private Canvas canvas;
        private string sceneName;
        private string checkpointScene;
        private Vector3 checkpointPosition;
        private bool restoringCheckpoint;
        public bool HasCheckpoint => !string.IsNullOrEmpty(checkpointScene);
        private readonly Color ink = new Color(.035f, .055f, .075f, .97f);
        private readonly Color gold = new Color(.85f, .73f, .48f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance == null) new GameObject("Lucy Session").AddComponent<GameSession>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            font = Resources.Load<Font>("Presentation/NotoSansKR-Regular");
            headingFont = Resources.Load<Font>("Presentation/NotoSansKR-Bold");
            AudioListener.volume = PlayerPrefs.GetFloat("Lucy.Volume", .8f);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) { Instance = null; Time.timeScale = 1; AudioListener.pause = false; }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            sceneName = scene.name;
            bool title = sceneName == "Title", ending = sceneName == "Ending";
            blocked = title || ending || restoringCheckpoint;
            failed = paused = documentOpen = false;
            Time.timeScale = ending || restoringCheckpoint ? 0 : 1;
            AudioListener.pause = false;
            if (title || ending)
            {
                foreach (var c in FindObjectsOfType<Canvas>())
                    if (c != canvas) c.gameObject.SetActive(false);
            }
            if (canvas == null) BuildCanvas();
            EnsureEventSystem();
            EnsureAudioListener();
            HospitalStory.EnterScene(sceneName);
            HospitalTutorial.EnterScene(sceneName);
            if (title) ShowTitle();
            else if (ending) ShowEnding();
            else ShowHud();
        }

        private void BuildCanvas()
        {
            var go = new GameObject("Session UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = .5f;
        }

        private void EnsureEventSystem()
        {
            var own = GetComponentInChildren<EventSystem>();
            if (own == null)
            {
                var go = new GameObject("Session Input", typeof(EventSystem), typeof(StandaloneInputModule));
                go.transform.SetParent(transform, false);
                own = go.GetComponent<EventSystem>();
            }
            foreach (var other in FindObjectsOfType<EventSystem>())
            {
                if (other == own) continue;
                other.enabled = false;
                foreach (var module in other.GetComponents<BaseInputModule>()) module.enabled = false;
            }
            EventSystem.current = own;
        }

        private void EnsureAudioListener()
        {
            var listeners = FindObjectsOfType<AudioListener>();
            var primary = Camera.main != null ? Camera.main.GetComponent<AudioListener>() : null;
            if (primary == null && listeners.Length > 0) primary = listeners[0];
            foreach (var listener in listeners) listener.enabled = listener == primary;
        }

        private void Update()
        {
            if (loading) return;
            HospitalTutorial.Observe(sceneName);
            if (!blocked && Time.timeScale>0 && HospitalTutorial.Visible(sceneName) && Input.GetKeyDown(KeyCode.T)) HospitalTutorial.Skip();
            if (documentOpen)
            {
                if (Time.unscaledTime - documentOpenedAt > .3f && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))) CloseDocument();
                return;
            }
            if (!blocked && Time.timeScale > 0) elapsed += Time.deltaTime;
            if (!blocked && Time.timeScale > 0 && Input.GetKeyDown(KeyCode.J)) ShowDocument("대피 기록장", HospitalStory.JournalText());
            if (Input.GetKeyDown(KeyCode.Escape) && sceneName != "Title" && sceneName != "Ending" && !failed &&
                (Portal.PortalManager.Instance == null || !Portal.PortalManager.Instance.IsTransitioning))
            {
                if (paused) Resume();
                else if (Time.timeScale > 0) Pause();
            }
        }

        public static string ObjectiveFor(string scene)
        {
            switch (scene)
            {
                case "0101": case FirstScene: return "01 / 눈을 뜬 병동\n기록과 손전등을 찾고 오른쪽 계단으로 향하세요.";
                case "0102": return "02 / 낯선 발소리\n계단을 올라 병원 안쪽을 탐색하세요.";
                case "0201": return HospitalStory.HasRecord(2) ? "03 / 복구된 비상 전력\n남쪽 끝 서쪽 병실 문을 열고 들어가세요.\n간호사의 도움을 받은 뒤 북동쪽 비상 통로로." : "03 / 닫힌 병동\n간호 기록을 읽고 아래쪽 우측 연결 통로를 찾으세요.";
                case "Puzzle_2F": return "04 / 끊어진 연결 통로\n스위치로 다리를 잇고 마지막 점검 기록을 찾으세요.";
                case "Maze_3F": return "05 / 마지막 복도\n발소리에 귀를 기울이며 미로의 출구를 찾으세요.";
                default: return "빛을 따라 주변을 조사하세요.";
            }
        }

        private void ClearUI()
        {
            if (screen != null) { screen.SetActive(false); Destroy(screen); }
            if (hud != null) { hud.SetActive(false); Destroy(hud); }
        }

        private RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 lo, Vector2 hi)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(parent, false); r.anchorMin = min; r.anchorMax = max; r.offsetMin = lo; r.offsetMax = hi;
            return r;
        }

        private Text Label(Transform parent, string name, string text, int size, Vector2 min, Vector2 max, Color color)
        {
            var r = Rect(name, parent, min, max, Vector2.zero, Vector2.zero);
            var t = r.gameObject.AddComponent<Text>(); t.font = size>=32 ? headingFont : font; t.text = text; t.fontSize = size; t.lineSpacing=1.15f;
            t.color = color; t.raycastTarget = false; t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private void Button(Transform parent, string text, float y, UnityEngine.Events.UnityAction action)
        {
            var r = Rect(text, parent, new Vector2(.08f,y), new Vector2(.35f,y+.065f), Vector2.zero, Vector2.zero);
            var im = r.gameObject.AddComponent<Image>(); im.color = new Color(.1f,.15f,.18f,.94f);
            var b = r.gameObject.AddComponent<Button>(); b.onClick.AddListener(action);
            var colors = b.colors; colors.highlightedColor = gold; colors.selectedColor = gold; b.colors = colors;
            var t = Label(r, "Label", text, 25, Vector2.zero, Vector2.one, Color.white); t.alignment = TextAnchor.MiddleCenter;
        }

        private Transform Panel(bool artwork)
        {
            ClearUI();
            screen = Rect("Menu", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            var bg = screen.AddComponent<Image>(); bg.color = ink;
            if (artwork)
            {
                var r = Rect("Illustration", screen.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var im = r.gameObject.AddComponent<RawImage>(); im.texture = Resources.Load<Texture2D>("Presentation/LucyTitle"); im.raycastTarget = false;
            }
            return screen.transform;
        }

        private void Heading(Transform panel, string eyebrow, string title, string body)
        {
            Label(panel,"Chapter",eyebrow,20,new Vector2(.08f,.80f),new Vector2(.70f,.85f),gold);
            Label(panel,"Heading",title,76,new Vector2(.075f,.65f),new Vector2(.8f,.79f),Color.white);
            Label(panel,"Description",body,25,new Vector2(.08f,.47f),new Vector2(.70f,.63f),new Color(.75f,.81f,.83f));
        }

        private void ShowTitle()
        {
            var p = Panel(true);
            Heading(p,"THE LAST NIGHT ROUND", "L U C Y", "마지막 야간 순회\n잠긴 병동에서, 함께 나갈 방법을 찾으세요.");
            Button(p,"새 게임",.38f,StartNewGame);
            if (Lucy.LastNightRound.NightRoundGame.HasSavedGame)
                Button(p,"이어하기",.30f,() => StartNightRound(true));
            Button(p,"조작 방법",.22f,ShowHelp);
            Button(p,"소리 설정",.14f,ShowSettings);
            Label(p,"Footer","탐색 · 퍼즐 · 생존     /     헤드폰 사용 권장",18,new Vector2(.08f,.07f),new Vector2(.8f,.12f),gold);
        }

        private void ShowHud()
        {
            ClearUI();
            hud = Rect("Hospital HUD", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            hud.AddComponent<HospitalHud>().Initialize(font, sceneName);
        }

        private void Pause()
        {
            previousScale = Time.timeScale; paused = blocked = true; Time.timeScale = 0; AudioListener.pause = true;
            var p = Panel(false);
            Heading(p,"TAKE A BREATH", "잠시 숨 고르기",ObjectiveFor(sceneName));
            Button(p,"계속하기",.34f,Resume); Button(p,"조작 방법",.25f,ShowHelp); Button(p,"소리 설정",.16f,ShowSettings); Button(p,"타이틀로",.07f,ReturnToTitle);
        }

        private void Resume()
        {
            paused = blocked = false; Time.timeScale = previousScale; AudioListener.pause = false; ShowHud();
        }

        public void ShowDocument(string title, string body)
        {
            if (blocked || loading || Time.timeScale <= 0) return;
            if (Portal.PortalManager.Instance != null && Portal.PortalManager.Instance.IsTransitioning) return;
            previousScale = Time.timeScale; blocked = documentOpen = true;
            documentOpenedAt = Time.unscaledTime; Time.timeScale = 0; AudioListener.pause = true;
            var p = Panel(false);
            Label(p,"Record heading",title,42,new Vector2(.10f,.83f),new Vector2(.90f,.92f),gold);
            Label(p,"Record text",body,25,new Vector2(.10f,.20f),new Vector2(.90f,.80f),Color.white);
            Button(p,"접어 두기  /  Space",.07f,CloseDocument);
        }

        private void CloseDocument()
        {
            if (!documentOpen) return;
            documentOpen = blocked = false; Time.timeScale = previousScale; AudioListener.pause = false; ShowHud();
        }

        private void ShowHelp()
        {
            var p = Panel(false);
            Heading(p,"THE LAST NIGHT ROUND", "어둠 속의 약속", "방향키 / WASD  이동     Shift  달리기\nE  조사·대화     I  소지품     J  증거     ESC  메뉴");
            Label(p,"Tips","Space로 문서를 닫습니다. 조사 중에는 추격도 멈춥니다.\n손전등을 찾고 팔찌와 이송 기록을 대조하세요.\n배전반은 세 회로 중 두 개에만 전원을 공급합니다.\n마지막 손잡이는 서연이 건널 때까지 E 유지 · F/클릭 유지 토글.\n붙잡히면 R로 사건 체크포인트에서 다시 시작합니다.",24,new Vector2(.08f,.23f),new Vector2(.90f,.46f),Color.white);
            Button(p,"돌아가기",.10f,() => { if (paused) { Time.timeScale = previousScale; Pause(); } else ShowTitle(); });
        }

        private void ShowSettings()
        {
            var p = Panel(false);
            Heading(p,"SOUND", "소리 설정", "작은 발소리도 놓치지 않도록 편안한 음량을 선택하세요.");
            var volume = Label(p,"Volume",$"전체 음량  {Mathf.RoundToInt(AudioListener.volume * 100)}%",28,new Vector2(.08f,.43f),new Vector2(.8f,.49f),gold);
            UnityEngine.Events.UnityAction<float> adjust = delta =>
            {
                AudioListener.volume = Mathf.Clamp01(AudioListener.volume + delta);
                PlayerPrefs.SetFloat("Lucy.Volume",AudioListener.volume); PlayerPrefs.Save();
                volume.text = $"전체 음량  {Mathf.RoundToInt(AudioListener.volume * 100)}%";
            };
            Button(p,"−  소리 줄이기",.32f,() => adjust(-.1f));
            Button(p,"+  소리 키우기",.23f,() => adjust(.1f));
            Button(p,"돌아가기",.10f,() => { if (paused) { Time.timeScale = previousScale; Pause(); } else ShowTitle(); });
        }

        public void Fail(string reason)
        {
            if (blocked || loading) return;
            failed = blocked = true; Time.timeScale = 0; AudioListener.pause = true;
            ShowFailure(reason);
        }

        public void Caught(Transform attacker)
        {
            if (blocked || loading || capturePlaying) return;
            failed = blocked = capturePlaying = true;
            Time.timeScale = 0; AudioListener.pause = true;
            ClearUI();
            StartCoroutine(PlayCapture(attacker));
        }

        private IEnumerator PlayCapture(Transform attacker)
        {
            var effect = gameObject.AddComponent<HospitalCaptureEffect>();
            var sequence = effect.Play(attacker);
            while (true)
            {
                bool next;
                try { next = sequence.MoveNext(); }
                catch (System.Exception error) { Debug.LogException(error); break; }
                if (!next) break;
                yield return sequence.Current;
            }
            (sequence as System.IDisposable)?.Dispose();
            Destroy(effect);
            capturePlaying = false;
            ShowFailure("어둠 속의 누군가에게 붙잡혔습니다.");
        }

        private void ShowFailure(string reason)
        {
            var p = Panel(false); Heading(p,"THE NIGHT IS NOT OVER", "다시, 눈을 뜨면",reason + "\n빛과 체력을 아끼며 다른 길을 찾아보세요.");
            if (HasCheckpoint)
            {
                Button(p,"최근 기록에서 계속",.34f,ContinueFromCheckpoint);
                Button(p,"처음부터 재도전",.25f,StartNewGame);
                Button(p,"타이틀로",.16f,ReturnToTitle);
            }
            else
            {
                Button(p,"처음부터 재도전",.34f,StartNewGame); Button(p,"타이틀로",.25f,ReturnToTitle);
            }
        }

        private void ShowEnding()
        {
            float best = PlayerPrefs.GetFloat("Lucy.BestClearSeconds",0);
            if (elapsed > 0 && (best <= 0 || elapsed < best)) { best = elapsed; PlayerPrefs.SetFloat("Lucy.BestClearSeconds",best); PlayerPrefs.Save(); }
            var p = Panel(true);
            Heading(p,"DAWN / EPILOGUE", "명단에 남은 이름", HospitalStory.EndingText);
            Label(p,"Run time",$"탈출 시간  {FormatTime(elapsed)}    ·    최고 기록  {FormatTime(best)}",23,new Vector2(.08f,.41f),new Vector2(.8f,.46f),gold);
            Button(p,"다시 플레이",.29f,StartNewGame); Button(p,"타이틀로",.20f,ReturnToTitle);
        }

        private static string FormatTime(float seconds) => $"{(int)seconds / 60:00}:{(int)seconds % 60:00}";
        public void StartStoryBlockout()
        {
            StartNightRound(false);
        }
        private void StartNightRound(bool resume)
        {
            if (loading || capturePlaying || !enabled || Lucy.LastNightRound.NightRoundGame.Instance != null) return;
            ClearUI();
            PersistentGameObjects.Clear();
            enabled = false;
            StartCoroutine(Lucy.LastNightRound.NightRoundGame.Launch(resume));
        }
        public void StartNewGame()
        {
            StartNightRound(false);
        }
        public void StartLegacyGame()
        {
            if (loading || capturePlaying) return;
            HospitalTutorial.Reset();
            checkpointScene = null;
            StartCoroutine(LoadFresh(FirstScene));
        }
        public void ReturnToTitle() { if (!loading && !capturePlaying) StartCoroutine(LoadFresh("Title")); }

        public void RegisterCheckpoint(string scene, Vector3 position)
        {
            if (loading || string.IsNullOrEmpty(scene) || scene == "Title" || scene == "Ending") return;
            checkpointScene = scene;
            checkpointPosition = position;
            Debug.Log($"[Lucy Checkpoint] Saved {scene} at {position}");
        }

        public void ContinueFromCheckpoint()
        {
            if (!loading && !capturePlaying && failed && HasCheckpoint && Application.CanStreamedLevelBeLoaded(checkpointScene))
                StartCoroutine(RestoreCheckpoint());
        }

        private IEnumerator RestoreCheckpoint()
        {
            loading = blocked = restoringCheckpoint = true;
            failed = paused = documentOpen = false;
            Time.timeScale = 0; AudioListener.pause = true;
            ClearUI();
            bool restored = false;
            try
            {
                // Reserve the retry via loading above, then let an in-flight portal finish
                // its unscaled fade/cancellation before sharing CameraEffector with it.
                while (Portal.PortalManager.Instance != null && Portal.PortalManager.Instance.IsTransitioning)
                    yield return null;
                var player = Portal.Character.Instance;
                if (player != null && player.TryGetComponent<Rigidbody2D>(out var oldBody)) oldBody.velocity = Vector2.zero;
                if (Portal.CameraEffector.Instance != null) yield return Portal.CameraEffector.Instance.FadeOut();
                // Keep persistent inventory, collected records, and bridge-switch state intact.
                yield return SceneManager.LoadSceneAsync(checkpointScene);
                player = Portal.Character.Instance;
                if (player == null)
                {
                    Debug.LogError("Checkpoint scene has no player: " + checkpointScene);
                    yield break;
                }
                player.gameObject.SetActive(true);
                player.transform.position = checkpointPosition;
                if (player.TryGetComponent<Rigidbody2D>(out var body))
                {
                    body.position = checkpointPosition;
                    body.velocity = Vector2.zero;
                    body.angularVelocity = 0;
                }
                Physics2D.SyncTransforms();
                yield return null;
                // PortalManager's notification routine is private. Use the existing public
                // listener contract after scene Start methods have registered their objects.
                foreach (var behaviour in FindObjectsOfType<MonoBehaviour>())
                {
                    if (!(behaviour is ISceneChangeListener listener)) continue;
                    try { listener.OnSceneChange(); }
                    catch (System.Exception exception) { Debug.LogException(exception); }
                }
                var inventory = invertoryAndItem.Inventory.instance;
                if (inventory != null)
                {
                    inventory.ClearSelectItemWindow();
                    if (inventory.inventoryWindow != null) inventory.inventoryWindow.SetActive(false);
                }
                var stats = CharacterStat.instance;
                if (stats != null)
                {
                    stats.curMental = Mathf.Min(stats.maxMental, Mathf.Max(stats.curMental, 60f));
                    stats.curStamina = stats.maxStamina;
                    stats.canRun = true; stats.isRun = false;
                    stats.UpdateStats();
                }
                var light = invertoryAndItem.FlashLight.instance;
                if (light != null) light.Refill(Mathf.Max(0, 3 - light.battery));
                bool equipped = light != null && inventory != null && inventory.IsItemEquipped(light.itemData);
                if (equipped && light.battery > 0)
                {
                    if (HandLightSwitch.instance != null) HandLightSwitch.instance.TurnOnHandLight();
                    light.StartConsumeBattery();
                    if (stats != null) stats.StopMentalReduce();
                }
                else if (stats != null) stats.StartMentalReduce();
                if (player.TryGetComponent<CharacterMove>(out var movement) && movement.playerMoveSpeed <= 0)
                    movement.ResumeMovement();
                if (InputSystem.InputManager.Instance != null) InputSystem.InputManager.Instance.SetMovementState(true);
                if (Portal.CameraEffector.Instance != null) yield return Portal.CameraEffector.Instance.FadeIn();
                restored = true;
                Debug.Log($"[Lucy Checkpoint] Restored {checkpointScene} at {checkpointPosition}");
            }
            finally
            {
                restoringCheckpoint = loading = blocked = false;
                Time.timeScale = previousScale = 1f; AudioListener.pause = false;
                if (restored) ShowHud();
                else Fail("기록 지점을 불러오지 못했습니다. 다시 시도하거나 새 게임을 시작하세요.");
            }
        }

        private IEnumerator LoadFresh(string target)
        {
            loading = blocked = true; Time.timeScale = 1; AudioListener.pause = false;
            documentOpen = false; HospitalStory.ResetRun();
            ClearUI();
            // Reset game-owned managers together; preserve external diagnostics hosts.
            PersistentGameObjects.Clear();
            yield return null;
            elapsed = 0;
            yield return SceneManager.LoadSceneAsync(target);
            loading = false;
        }
    }
}
