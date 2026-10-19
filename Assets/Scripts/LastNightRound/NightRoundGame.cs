using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace Lucy.LastNightRound
{
    /// <summary>Playable development blockout. Independent from legacy bridge/maze progression.</summary>
    public sealed class NightRoundGame : MonoBehaviour
    {
        public static NightRoundGame Instance { get; private set; }
        public NightRoundState State { get; private set; }
        public Vector2 PlayerPosition => player.position;
        public string Focus => focus;
        public bool IsDead => dead;
        public Vector2 NursePosition => nurse.position;
        public bool IsBlocked => document != null || paused || dead || State.ending != "";
        private NightRoundMap map;
        private Transform player, nurse, pursuer;
        private Camera cameraView;
        private Font font;
        private string focus, document, documentTitle, notice = "";
        private float openedAt, noticeUntil, warning, captureAt, hold, tension;
        private bool paused, dead, chasing, nurseWalking;
        private int pursuit;
        private SaveData checkpoint;
        private GUIStyle text, heading, button;
        private readonly SpriteRenderer[] gates = new SpriteRenderer[5];
        private Sprite[] doorArt;
        private Light2D flashlight, serviceLight;
        private Texture2D defeatArt;
        private AudioSource ambience, footsteps;
        private float nextFootstep;
        private float cameraVelocity;
        private int currentRoom;
        private static readonly string[] RoomNames = { "입원 병동", "야간 간호실", "전력 설비실", "기록 보관실", "마지막 출구" };
        public static bool HasSavedGame => File.Exists(Path.Combine(Application.persistentDataPath, "last-night-round-blockout.json"));
        [Serializable] private class SaveData
        {
            public int version = 1;
            public NightRoundState state;
            public Vector2 player, nurse;
        }
        private string SavePath => Path.Combine(Application.persistentDataPath, "last-night-round-blockout.json");

        public static IEnumerator Launch(bool resume = false)
        {
            yield return null;
            var old = SceneManager.GetActiveScene();
            var scene = SceneManager.CreateScene("LastNightRound_Blockout");
            SceneManager.SetActiveScene(scene);
            foreach (var root in old.GetRootGameObjects()) root.SetActive(false);
            var host = new GameObject("Last Night Round — Development Blockout");
            var game = host.AddComponent<NightRoundGame>();
            if (resume) game.Restore(true);
            if (old.IsValid()) yield return SceneManager.UnloadSceneAsync(old);
        }

        private void Awake()
        {
            Instance = this; State = new NightRoundState();
            font = Resources.Load<Font>("Presentation/LucyFont");
            map = new NightRoundMap(); map.Build();
            doorArt = map.Art.Atlas("WardDoors", 2, 2);
            float[] gateX = { 8, 26, 44, 62, 77 };
            for (int i = 0; i < gates.Length; i++)
            {
                var gate = new GameObject("Progress gate " + i); gate.transform.SetParent(transform);
                gate.transform.position = new Vector3(gateX[i], 0, 0);
                gates[i] = gate.AddComponent<SpriteRenderer>(); gates[i].sprite = doorArt[2]; gates[i].sortingOrder = 120;
                gate.transform.localScale = Vector3.one * (3 / doorArt[2].bounds.size.y);
            }
            player = Actor("루시", new Vector2(-3, 0), new Color(.69f, .79f, .88f));
            nurse = Actor("서연", new Vector2(6, 0), new Color(.56f, .78f, .65f));
            pursuer = Actor("순회자", new Vector2(10, 0), new Color(.51f, .25f, .25f));
            pursuer.gameObject.SetActive(false);
            SetupLighting();
            ambience = gameObject.AddComponent<AudioSource>(); ambience.playOnAwake = false;
            ambience.clip = Resources.Load<AudioClip>("Sound/Horror Elements/Ambient/Amb_darkroom");
            ambience.loop = true; ambience.volume = .12f; ambience.Play();
            footsteps = gameObject.AddComponent<AudioSource>(); footsteps.playOnAwake = false;
            footsteps.clip = Resources.Load<AudioClip>("Sound/Footsteps - Essentials/Footsteps_Tile/Footsteps_Tile_Run/Footsteps_Tile_Run_01");
            footsteps.volume = .2f;
            defeatArt = Resources.Load<Texture2D>("Presentation/LucyDefeat");
            var view = new GameObject("Blockout Camera", typeof(Camera), typeof(AudioListener));
            cameraView = view.GetComponent<Camera>(); cameraView.orthographic = true; cameraView.orthographicSize = 7.3f;
            cameraView.backgroundColor = new Color(.018f, .025f, .03f); cameraView.clearFlags = CameraClearFlags.SolidColor;
            cameraView.transform.position = new Vector3(CameraX(player.position.x), 0, -10);
            Time.timeScale = 1; AudioListener.pause = false;
            SaveCheckpoint(false);
            Tell("마지막 야간 순회", "눈을 뜬 병동에는 아무도 없습니다. 문밖에서 누군가 당신의 이름을 부릅니다.\n\nWASD / 방향키 이동 · Shift 달리기 · E 조사\nSpace 대화 닫기 · I 소지품 · J 증거 · Esc 일시정지\n\n입원 팔찌와 손전등부터 확인하세요.\n무음 상태에서도 모든 정보와 위험 경고를 읽을 수 있습니다.");
        }

        private Transform Actor(string name, Vector2 at, Color color)
        {
            var go = new GameObject(name); go.transform.SetParent(transform); go.transform.position = at;
            go.AddComponent<NightRoundActorView>().Initialize(map.Art, name == "루시" ? 0 : name == "서연" ? 1 : 2);
            return go.transform;
        }

        private void Update()
        {
            flashlight.enabled = State.flashlight;
            serviceLight.intensity = (State.circuits & 1) != 0 ? .65f : .08f;
            ambience.volume = IsBlocked ? .04f : .12f;
            if (IsBlocked && footsteps.isPlaying) footsteps.Stop();
            bool[] open = { State.CanLeaveWard, State.CanLeaveNursing, State.CanLeaveService, State.CanLeaveIsolation, State.CanCrossFinalDoor };
            for (int i = 0; i < gates.Length; i++)
            {
                gates[i].sprite = doorArt[open[i] ? 3 : 2];
                gates[i].color = open[i] ? Color.white : new Color(.85f,.75f,.65f);
                float x = i == 4 ? 77 : 8 + i * 18;
                float hingeOffset = open[i] ? (doorArt[3].bounds.size.x - doorArt[2].bounds.size.x) * gates[i].transform.localScale.x * .5f : 0;
                gates[i].transform.position = new Vector3(x - hingeOffset, 0, 0);
            }
            map.RefreshStations(State, focus);
            if (document != null)
            {
                if (Time.unscaledTime - openedAt > .25f && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))) document = null;
                return;
            }
            if (Input.GetKeyDown(KeyCode.Escape) && !dead && State.ending == "") paused = !paused;
            if ((dead && Time.unscaledTime - captureAt > 1 || State.ending != "") && Input.GetKeyDown(KeyCode.R)) { Restore(false); return; }
            if (paused && Input.GetKeyDown(KeyCode.L)) { Restore(true); return; }
            if (IsBlocked || Time.timeScale <= 0) return;
            if (Input.GetKeyDown(KeyCode.I)) { Tell("소지품", (State.flashlight ? "손전등 — 장착됨\n" : "손전등 없음\n") + (State.bracelet ? "입원 팔찌 — 101 / 루시\n" : "") + (State.fuse ? "제어 퓨즈 확보" : "")); return; }
            if (Input.GetKeyDown(KeyCode.J)) { Tell("확인한 증거", (State.transferMismatch ? "병동 이송 완료 02:05 / 서연 서명\n차량 실물 인계 확인은 공란.\n\n" : "아직 대조한 기록이 없습니다.\n") + (State.truth ? "02:17 서연이 외부 잠금 조작.\n02:18 복도 차단. 02:19 외부 해제 요청.\n서연: 제가 잠갔어요. 잘못했습니다." : "")); return; }
            var direction = new Vector2((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0),
                (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0));
            float speed = Input.GetKey(KeyCode.LeftShift) ? 5.2f : 3.4f;
            var beforeMove = player.position;
            Move(player, direction.normalized * speed * Time.deltaTime, true);
            if ((player.position - beforeMove).sqrMagnitude > .000001f && Time.time >= nextFootstep)
            { footsteps.Play(); nextFootstep = Time.time + (speed > 4 ? .24f : .38f); }
            if (direction.sqrMagnitude > 0)
                flashlight.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90);
            focus = null; float closest = 1.55f;
            foreach (var station in map.Stations)
            {
                if (State.IsActionComplete(station.Key)) continue;
                float distance = Vector2.Distance(player.position, station.Value);
                if (distance < closest) { closest = distance; focus = station.Key; }
            }
            if (focus == "manual" && State.finalPower == 1 && !State.manualDoor)
            {
                hold = Input.GetKey(KeyCode.E) ? hold + Time.deltaTime : 0;
                if (hold >= 2) { Interact(focus); hold = 0; }
            }
            else { hold = 0; if (focus != null && Input.GetKeyDown(KeyCode.E)) Interact(focus); }
            if (IsBlocked) return;
            UpdateNurse(); UpdatePursuer();
            if (State.reunited && player.position.x > 78.5f && Mathf.Abs(player.position.y) < 1.3f) Interact("exit");
            currentRoom = Mathf.Clamp(Mathf.FloorToInt((player.position.x + 9) / 18), 0, 4);
        }

        private float CameraX(float x)
        {
            float halfWidth = cameraView.orthographicSize * cameraView.aspect;
            return halfWidth >= 44 ? 36 : Mathf.Clamp(x, -8 + halfWidth, 80 - halfWidth);
        }

        private void LateUpdate()
        {
            if (cameraView == null || player == null || IsBlocked) return;
            float at = cameraView.transform.position.x;
            float offset = player.position.x - at;
            float target = CameraX(at + Mathf.Sign(offset) * Mathf.Max(0,Mathf.Abs(offset)-2));
            cameraView.transform.position = new Vector3(Mathf.SmoothDamp(at,target,ref cameraVelocity,.22f),0,-10);
        }

        private bool Allowed(Vector2 target, Vector2 from)
        {
            if (target.y > 5.35f || !map.CanMove(target)) return false;
            float[] barriers = { 8, 26, 44, 62, 77 };
            bool[] open = { State.CanLeaveWard, State.CanLeaveNursing, State.CanLeaveService, State.CanLeaveIsolation, State.CanCrossFinalDoor };
            for (int i = 0; i < barriers.Length; i++)
                if (!open[i] && from.x <= barriers[i] && target.x > barriers[i] - .25f) return false;
            return true;
        }

        private void SetupLighting()
        {
            foreach (var renderer in map.Root.GetComponentsInChildren<SpriteRenderer>()) renderer.sharedMaterial = map.Art.LitMaterial;
            foreach (var gate in gates) gate.sharedMaterial = map.Art.LitMaterial;
            var ambient = Instantiate(Resources.Load<GameObject>("Presentation/WardAmbient"), transform);
            ambient.GetComponent<Light2D>().intensity = .65f;
            var glow = Resources.Load<GameObject>("Presentation/FirstRoomGlow");
            for (int i = 0; i < 5; i++)
            {
                var lamp = Instantiate(glow, transform).GetComponent<Light2D>();
                lamp.transform.position = new Vector3(i * 18, 2, 0);
                lamp.intensity = .65f; lamp.pointLightInnerRadius = 1; lamp.pointLightOuterRadius = 8;
                if (i == 2) serviceLight = lamp;
            }
            flashlight = Instantiate(glow, player).GetComponent<Light2D>();
            flashlight.name = "Lucy flashlight"; flashlight.transform.localPosition = Vector3.up * .4f;
            flashlight.intensity = 1.4f; flashlight.pointLightInnerRadius = .5f; flashlight.pointLightOuterRadius = 6;
            flashlight.pointLightInnerAngle = 30; flashlight.pointLightOuterAngle = 75;
            flashlight.color = new Color(1,.94f,.78f);
            flashlight.enabled = false;
        }

        private void Move(Transform actor, Vector2 delta, bool gated)
        {
            var start = (Vector2)actor.position;
            var next = start + new Vector2(delta.x, 0);
            if (gated ? Allowed(next, start) : map.CanMove(next)) actor.position = next;
            start = actor.position; next = start + new Vector2(0, delta.y);
            if (gated ? Allowed(next, start) : map.CanMove(next)) actor.position = next;
        }

        public void Interact(string id)
        {
            if (IsBlocked) return;
            // All interactions, including test calls, must be reached physically.
            if (!map.Stations.TryGetValue(id, out var point) || Vector2.Distance(player.position, point) > 1.6f) return;
            bool firstManifest = !State.transferMismatch, firstPower = !State.powerRestored;
            if (!State.Apply(id, out var message)) { Notify(message); return; }
            map.SetStationActive(id, true);
            if (id == "manifest" && firstManifest) BeginPursuit(1, new Vector2(10, 0));
            if (firstPower && State.powerRestored) BeginPursuit(2, new Vector2(28, 0));
            if (id == "power-together" || id == "power-alone") { if (pursuit != 3) BeginPursuit(3, new Vector2(63, 0)); }
            if (id == "safe" || id == "service-exit") { chasing = false; pursuer.gameObject.SetActive(false); }
            if (id == "manual") nurseWalking = true;
            if (id == "meet" || id == "safe" || id == "service-exit" || id == "lock-record" || id == "reunite") SaveCheckpoint();
            if (State.ending != "") { chasing = false; pursuer.gameObject.SetActive(false); Debug.Log("[LastNightRound] ending=" + State.ending); }
            else if (id.StartsWith("circuit-") || id == "manual") Notify(message);
            else Tell(StationLabel(id), message);
        }

        private void UpdateNurse()
        {
            Vector2 position = nurse.position;
            bool atFinalRoom = position.x > 70.05f || (position.x >= 69.95f && position.y <= -2.9f);
            if (nurseWalking && State.manualDoor && !State.nurseCrossed && atFinalRoom)
            {
                var target = new Vector2(78, -.8f);
                Move(nurse, (target - (Vector2)nurse.position).normalized * 3.2f * Time.deltaTime, false);
                if (Vector2.Distance(nurse.position, target) < .25f)
                {
                    State.Apply("nurse-crossed", out var message); Notify(message); nurseWalking = false;
                }
            }
            else if (State.transferMismatch && !State.nurseCrossed)
            {
                // The position itself identifies the leg, including after checkpoint restore.
                // Travel stays on the physical staff corridor; reunion never moves the actor.
                Vector2 target;
                if (position.x < 20.95f) target = new Vector2(21, 0);
                else if (position.x <= 21.05f && position.y < 7.65f) target = new Vector2(21, 7.7f);
                else if (position.x < 69.95f) target = new Vector2(70, 7.7f);
                else target = new Vector2(70, -3);
                if (!atFinalRoom)
                {
                    Vector2 delta = target - position;
                    Move(nurse, Vector2.ClampMagnitude(delta, 4.3f * Time.deltaTime), false);
                }
            }
            else if (State.metNurse && !State.transferMismatch)
            {
                var target = new Vector2(Mathf.Min(19, player.position.x + 1.4f), 0);
                Move(nurse, (target - (Vector2)nurse.position).normalized * Mathf.Min(3, Vector2.Distance(nurse.position, target) * 4) * Time.deltaTime, false);
            }
        }

        private void BeginPursuit(int number, Vector2 at)
        {
            pursuit = number; chasing = true; warning = number == 3 ? 6 : 3;
            pursuer.position = at; pursuer.gameObject.SetActive(true);
            Notify("호출등이 켜졌습니다. 복도에서 빠른 발소리가 다가옵니다.");
            Debug.Log("[LastNightRound] pursuit=" + number);
        }
        private void UpdatePursuer()
        {
            if (!chasing) { tension = Mathf.Max(0, tension - Time.deltaTime * 10); return; }
            if (warning > 0) { warning -= Time.deltaTime; return; }
            Vector2 target = player.position;
            int enemyRoom = Mathf.FloorToInt((pursuer.position.x + 9) / 18), playerRoom = Mathf.FloorToInt((player.position.x + 9) / 18);
            if (enemyRoom != playerRoom) target = new Vector2(pursuer.position.x + Mathf.Sign(player.position.x - pursuer.position.x) * 2, 0);
            Move(pursuer, (target - (Vector2)pursuer.position).normalized * 2.7f * Time.deltaTime, false);
            float distance = Vector2.Distance(player.position, pursuer.position);
            tension = Mathf.Clamp(tension + Time.deltaTime * (distance < 5 ? 15 : -6), 0, 100);
            if (distance < .65f) { dead = true; captureAt = Time.unscaledTime; Debug.Log("[LastNightRound] caught"); }
        }

        private void Notify(string value) { notice = value; noticeUntil = Time.unscaledTime + 5; }
        private void Tell(string title, string body) { documentTitle = title; document = body; openedAt = Time.unscaledTime; }
        private void SaveCheckpoint(bool disk = true)
        {
            checkpoint = new SaveData { state = JsonUtility.FromJson<NightRoundState>(JsonUtility.ToJson(State)), player = player.position, nurse = nurse.position };
            if (disk) try { File.WriteAllText(SavePath, JsonUtility.ToJson(checkpoint, true)); }
                catch (Exception e) { Notify("저장 실패: " + e.Message); Debug.LogWarning(e.Message); }
        }
        private void Restore(bool disk)
        {
            try
            {
                var saved = disk ? JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath)) : checkpoint;
                if (saved == null || saved.version != 1 || saved.state == null) throw new Exception("호환되지 않는 개발 저장입니다.");
                State = JsonUtility.FromJson<NightRoundState>(JsonUtility.ToJson(saved.state));
                player.position = saved.player; nurse.position = saved.nurse; checkpoint = saved;
                cameraView.transform.position = new Vector3(CameraX(player.position.x),0,-10); cameraVelocity = 0;
                currentRoom = Mathf.Clamp(Mathf.FloorToInt((player.position.x+9)/18),0,4);
                focus = null;
                chasing = dead = paused = nurseWalking = false; document = null; hold = tension = warning = 0; pursuit = 0;
                pursuer.gameObject.SetActive(false); Time.timeScale = 1;
                Debug.Log("[LastNightRound] restored " + (disk ? "disk" : "checkpoint") + " player=" + player.position + " ending=" + State.ending);
                Notify("사건 체크포인트에서 이어갑니다.");
            }
            catch (Exception e) { Notify("이어하기 실패: " + e.Message); }
        }
        private void ReturnToTitle()
        {
            var session = Presentation.GameSession.Instance;
            if (session != null) { session.enabled = true; session.ReturnToTitle(); }
        }

        public static string StationLabel(string id)
        {
            switch (id)
            {
                case "flashlight": return "손전등 챙기기"; case "bracelet": return "101호 팔찌";
                case "meet": return "문밖의 서연"; case "manifest": return "이송 기록 대조";
                case "safe": return "직원 휴게실로 피하기"; case "fuse": return "제어 퓨즈 확보";
                case "circuit-light": return "처치실 조명 회로"; case "circuit-supply": return "물품실 회로";
                case "circuit-door": return "연결문 회로"; case "service-exit": return "유지 보수 통로";
                case "lock-record": return "외부 잠금 이력"; case "reunite": return "관찰창의 서연";
                case "power-together": return "연결문 유지 / 함께 탈출"; case "power-alone": return "주 출구 우선 / 먼저 탈출";
                case "manual": return "내부문 손잡이 — E 2초 / 클릭 대체"; case "exit": return "외부 출입문";
                default: return id;
            }
        }

        private void OnGUI()
        {
            if (State == null) return;
            if (text == null)
            {
                text = new GUIStyle(GUI.skin.label) { font = font, fontSize = 24, wordWrap = true };
                heading = new GUIStyle(text) { fontSize = 34 };
                button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 23, wordWrap = true };
            }
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1600f, Screen.height / 900f, 1));
            if (dead && defeatArt != null)
            {
                GUI.color = new Color(.45f,.45f,.45f,1);
                GUI.DrawTexture(new Rect(0,0,1600,900), defeatArt, ScaleMode.ScaleAndCrop);
                GUI.color = Color.white;
            }
            if (document != null || dead || paused || State.ending != "")
            {
                GUI.color = new Color(.025f, .04f, .045f, .97f);
                GUI.DrawTexture(new Rect(220, 165, 1160, 570), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
            GUI.Box(new Rect(20, 18, 1560, 102), "");
            GUI.Label(new Rect(40, 25, 650, 36), "마지막 야간 순회", heading);
            GUI.Label(new Rect(850, 28, 690, 82), State.Objective, text);
            GUI.Label(new Rect(40, 70, 700, 38), "긴장 " + Mathf.RoundToInt(tension) + "   |   E 조사 · Shift 달리기 · Esc 메뉴", text);
            if (!IsBlocked)
            {
                GUI.Label(new Rect(40,128,650,42), "0" + (currentRoom+1) + " / " + RoomNames[currentRoom] + "    ·    J 확인한 증거", text);
                if (State.firstEscape && currentRoom == 2)
                {
                    GUI.Box(new Rect(25,178,770,55), "");
                    GUI.Label(new Rect(40,185,750,42), "전력 " + State.PoweredCircuitCount + "/2    " + ((State.circuits&1)!=0 ? "조명 ON   " : "조명 OFF   ") + ((State.circuits&2)!=0 ? "물품 ON   " : "물품 OFF   ") + ((State.circuits&4)!=0 ? "문 ON" : "문 OFF"), text);
                }
            }
            if (!IsBlocked && focus != null)
            {
                GUI.Box(new Rect(350, 790, 900, 80), "");
                string action = StationLabel(focus);
                if (focus.StartsWith("circuit-")) action += State.IsCircuitPowered(focus) ? " 끄기" : " 켜기";
                if (GUI.Button(new Rect(370, 804, 860, 50), "E  " + action + (hold > 0 ? "  " + Mathf.CeilToInt(hold / 2 * 100) + "%" : ""), button)) Interact(focus);
            }
            if (Time.unscaledTime < noticeUntil) GUI.Label(new Rect(280, 710, 1100, 70), notice, text);
            if (chasing && !IsBlocked)
            {
                GUI.color = new Color(.40f,.10f,.08f,.95f); GUI.DrawTexture(new Rect(850,128,710,76),Texture2D.whiteTexture); GUI.color = Color.white;
                GUI.Label(new Rect(870,137,670,66),warning > 0 ? "복도 끝에서 문이 열립니다…" : "순회자 접근  /  " + (pursuer.position.x < player.position.x ? "← 왼쪽" : "오른쪽 →") + "\n열린 통로로 이동하세요.",text);
            }
            if (document != null)
            {
                GUI.Box(new Rect(230, 175, 1140, 550), "");
                GUI.Label(new Rect(280, 210, 1040, 70), documentTitle, heading);
                GUI.Label(new Rect(280, 300, 1040, 300), document, text);
                if (GUI.Button(new Rect(550, 630, 500, 60), "Space · 계속", button)) document = null;
            }
            else if (dead)
            {
                float elapsed = Time.unscaledTime - captureAt;
                GUI.color = new Color(.5f, .05f, .05f, Mathf.Clamp01(1 - elapsed / 2)); GUI.DrawTexture(new Rect(0, 0, 1600, 900), Texture2D.whiteTexture); GUI.color = Color.white;
                GUI.Box(new Rect(400, 260, 800, 370), ""); GUI.Label(new Rect(450, 300, 700, 80), "순회자에게 붙잡혔습니다.", heading);
                if (elapsed > 1 && GUI.Button(new Rect(500, 430, 600, 65), "사건 체크포인트에서 재시도", button)) Restore(false);
                if (GUI.Button(new Rect(500, 520, 600, 60), "타이틀로", button)) ReturnToTitle();
            }
            else if (State.ending != "")
            {
                GUI.Box(new Rect(230, 175, 1140, 550), "");
                GUI.Label(new Rect(290, 230, 1020, 70), State.ending == "together" ? "함께 밖으로" : "먼저 밖으로", heading);
                GUI.Label(new Rect(290, 340, 1020, 180), State.ending == "together" ? "루시가 먼저 새벽 공기 속으로 걸어 나옵니다.\n“이번에는 문을 잠그지 마세요.”\n“네. 열어 둘게요.”" : "무전기는 잡음만 냅니다.\n루시는 기록을 챙기고 자신의 이름을 말합니다.\n“루시. 아직 여기 있어요.”", text);
                if (GUI.Button(new Rect(350, 590, 430, 70), "R · 최종 선택에서 다시", button)) Restore(false);
                if (GUI.Button(new Rect(820, 590, 430, 70), "타이틀로", button)) ReturnToTitle();
            }
            else if (paused)
            {
                GUI.Box(new Rect(400, 250, 800, 400), ""); GUI.Label(new Rect(450, 280, 700, 60), "잠시 숨 고르기", heading);
                if (GUI.Button(new Rect(500, 370, 600, 60), "계속하기", button)) paused = false;
                if (GUI.Button(new Rect(500, 450, 600, 60), "저장한 사건 이어하기", button)) Restore(true);
                if (GUI.Button(new Rect(500, 530, 600, 60), "타이틀로", button)) ReturnToTitle();
            }
            GUI.matrix = Matrix4x4.identity;
        }
        private void OnDestroy() { if (Instance == this) Instance = null; map?.Dispose(); }
    }
}
