using System;
using System.Collections;
using System.Collections.Generic;
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
        public bool EpilogueComplete => epilogue != null && epilogue.Complete;
        public bool IsEpilogueMoving => epilogue != null && epilogue.Moving && !paused;
        public bool IsManualHeld => manualHeld;
        public float ManualProgress => hold;
        public int EpilogueBeat => epilogue == null ? -1 : epilogue.Beat;
        public Vector2 NursePosition => nurse.position;
        public bool IsBlocked => document != null || paused || dead || State.ending != "";
        private NightRoundMap map;
        private Transform player, nurse, pursuer;
        private Camera cameraView;
        private Font font, headingFont;
        private Vector2 documentScroll;
        private string focus, document, documentTitle, notice = "";
        private float openedAt, noticeUntil, warning, captureAt, hold, tension;
        private bool paused, dead, chasing;
        private int pursuit;
        private SaveData checkpoint;
        private GUIStyle text, heading, button;
        private readonly SpriteRenderer[] gates = new SpriteRenderer[5];
        private Sprite[] doorArt;
        private Light2D flashlight, serviceLight;
        private Texture2D defeatArt;
        private AudioSource ambience, footsteps;
        private float nextFootstep;
        private Vector3 cameraVelocity;
        private int nurseLeg;
        private readonly Dictionary<Transform,List<Vector2>> paths=new Dictionary<Transform,List<Vector2>>();
        private float repathAt;
        private static readonly Vector2[] NurseRoute={new Vector2(21,12),new Vector2(21,19.5f),new Vector2(41,19.5f),new Vector2(52,19.5f),new Vector2(52,0),new Vector2(56.5f,0),new Vector2(60.2f,-.5f),new Vector2(64,-.5f)};
        public Vector2 StationPosition(string id) => map.Stations[id];
        public List<Vector2> PlayerPath(Vector2 target) => map.FindPath(player.position,target,p=>Allowed(p,p));
        public int CurrentRoom => NightRoundMap.RoomAt(player.position);
        private int currentRoom;
        private float roomTitleUntil;
        private bool objectiveVisible;
        private bool manualHeld, assistedHold;
        private NightRoundEpilogue epilogue;
        private bool FinalDoorOpen => State.reunited && (State.finalPower == 2 || State.nurseCrossed || (State.manualDoor && manualHeld));
        private static readonly string[] RoomNames = { "입원 병동", "야간 간호실", "전력 설비실", "기록 보관실", "마지막 출구", "중앙 복도" };
        public static bool HasSavedGame => File.Exists(Path.Combine(Application.persistentDataPath, "last-night-round-blockout.json"));
        [Serializable] private class SaveData
        {
            public int version = 2;
            public int nurseLeg;
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
            font = Resources.Load<Font>("Presentation/NotoSansKR-Regular");
            headingFont = Resources.Load<Font>("Presentation/NotoSansKR-Bold");
            map = new NightRoundMap(); map.Build();
            doorArt = map.Art.Doors;
            for (int i = 0; i < gates.Length; i++)
            {
                var gate = new GameObject("Progress gate " + i); gate.transform.SetParent(transform);
                gate.transform.position = (Vector3)NightRoundMap.GateCenters[i];
                gates[i] = gate.AddComponent<SpriteRenderer>(); gates[i].sprite = doorArt[i==1 || i==2 ? 0 : 2]; gates[i].sortingOrder = NightRoundMap.DepthAt(NightRoundMap.GateCenters[i]-Vector2.up*1.5f);
                gate.transform.localScale = Vector3.one * (i==1 || i==2 ? 2 / doorArt[0].bounds.size.x : 3 / doorArt[2].bounds.size.y);
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
            cameraView.transform.position = CameraTarget(player.position);
            Time.timeScale = 1; AudioListener.pause = false;
            SaveCheckpoint(false);
            roomTitleUntil=Time.unscaledTime+4;
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
            if (epilogue != null)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) paused = !paused;
                if (!paused) epilogue.Tick(Input.GetKeyDown(KeyCode.Space));
                if (epilogue.Complete && Input.GetKeyDown(KeyCode.R)) Restore(false);
                return;
            }
            flashlight.enabled = State.flashlight;
            serviceLight.intensity = (State.circuits & 1) != 0 ? .65f : .08f;
            ambience.volume = IsBlocked ? .04f : .12f;
            if (IsBlocked && footsteps.isPlaying) footsteps.Stop();
            bool[] open = { State.CanLeaveWard, State.CanLeaveNursing, State.CanLeaveService, State.CanLeaveIsolation, FinalDoorOpen };
            for (int i = 0; i < gates.Length; i++)
            {
                bool front=i==1 || i==2;
                gates[i].sprite=doorArt[(front ? 0 : 2)+(open[i] ? 1 : 0)];
                gates[i].color=open[i] ? Color.white : new Color(.85f,.75f,.65f);
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
            if (Input.GetKeyDown(KeyCode.Tab)) objectiveVisible=!objectiveVisible;
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
            manualHeld = false;
            if (focus == "manual" && State.finalPower == 1 && !State.nurseCrossed)
            {
                if (Input.GetKeyDown(KeyCode.F)) assistedHold = !assistedHold;
                if (direction.sqrMagnitude > 0) assistedHold = false;
                manualHeld = direction.sqrMagnitude == 0 && (Input.GetKey(KeyCode.E) || assistedHold);
                if (manualHeld)
                {
                    hold = Mathf.Min(2,hold + Time.deltaTime);
                    if (hold >= 2 && !State.manualDoor && State.Apply("manual",out var message)) Notify(message + "\n계속 유지하세요. 서연이 반대편 손잡이를 잡으면 놓아도 됩니다.");
                }
            }
            else { assistedHold = false; if (focus != null && Input.GetKeyDown(KeyCode.E)) Interact(focus); }
            if (IsBlocked) return;
            UpdateNurse(); UpdatePursuer();
            if (State.CanCrossFinalDoor && player.position.x > 62.4f && Mathf.Abs(player.position.y) < 1.3f && (State.finalPower == 2 || State.nurseOutside)) Interact("exit");
            int nextRoom=CurrentRoom;
            if (nextRoom!=currentRoom) { currentRoom=nextRoom; roomTitleUntil=Time.unscaledTime+3; }
        }

        private Vector3 CameraTarget(Vector2 point)
        {
            float halfWidth=cameraView.orthographicSize*cameraView.aspect;
            return new Vector3(Mathf.Clamp(point.x,-8+halfWidth,67-halfWidth),Mathf.Clamp(point.y,-11,14),-10);
        }
        private void LateUpdate()
        {
            if(cameraView==null || player==null || IsBlocked) return;
            cameraView.transform.position=Vector3.SmoothDamp(cameraView.transform.position,CameraTarget(player.position),ref cameraVelocity,.22f);
        }
        private bool Allowed(Vector2 target,Vector2 from)
        {
            if(NightRoundMap.StaffOnly(target) || !map.CanMove(target)) return false;
            for(int i=0;i<5;i++) if(!PlayerGateOpen(i) && NightRoundMap.Touches(NightRoundMap.GateBounds[i],target)) return false;
            return true;
        }

        private bool PlayerGateOpen(int gate)
        {
            switch(gate)
            {
                case 0: return State.CanLeaveWard;
                case 1: return State.CanLeaveNursing;
                case 2: return State.CanLeaveService;
                case 3: return State.CanLeaveIsolation;
                default: return State.CanCrossFinalDoor;
            }
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
                lamp.transform.position = (Vector3)(NightRoundMap.Centers[i]+Vector2.up*2);
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
            if (CanMoveActor(actor,next,start,gated)) actor.position = next;
            start = actor.position; next = start + new Vector2(0, delta.y);
            if (CanMoveActor(actor,next,start,gated)) actor.position = next;
        }

        private bool CanMoveActor(Transform actor,Vector2 next,Vector2 from,bool gated)
        {
            if (gated) return Allowed(next,from);
            if (!map.CanMove(next)) return false;
            if(actor==pursuer && NightRoundMap.StaffOnly(next)) return false;
            if(actor==nurse && State.finalPower!=1 && NightRoundMap.Touches(new Rect(51,5.5f,2,.6f),next)) return false;
            if(!FinalDoorOpen && NightRoundMap.Touches(NightRoundMap.GateBounds[4],next)) return false;
            return true;
        }

        private void Navigate(Transform actor,Vector2 target,float speed)
        {
            if(!paths.TryGetValue(actor,out var path) || path.Count==0 || Vector2.Distance(path[path.Count-1],target)>.8f)
            {
                // Failed routes are retried after gates change; successful paths are followed physically.
                if(path!=null && path.Count==0 && Time.time<repathAt) return;
                path=map.FindPath(actor.position,target,p=>CanMoveActor(actor,p,p,false));
                paths[actor]=path; repathAt=Time.time+.4f;
            }
            if(path.Count==0) return;
            var delta=path[0]-(Vector2)actor.position;
            if(delta.magnitude<.08f) { path.RemoveAt(0); return; }
            var before=actor.position;
            Move(actor,Vector2.ClampMagnitude(delta,speed*Time.deltaTime),false);
            if((before-actor.position).sqrMagnitude<.000001f) paths.Remove(actor);
        }

        public void Interact(string id)
        {
            if (IsBlocked) return;
            // All interactions, including test calls, must be reached physically.
            if (!map.Stations.TryGetValue(id, out var point) || Vector2.Distance(player.position, point) > 1.6f) return;
            if (id == "reunite" && Vector2.Distance(nurse.position,new Vector2(41,19.5f)) > .5f)
            { Notify("관찰창 안쪽에서 서연이 다가오는 발소리가 들립니다. 잠시 기다리세요."); return; }
            if (id == "manual" && State.reunited && State.finalPower == 1 && !State.nurseCrossed)
            { assistedHold = !assistedHold; Notify(assistedHold ? "손잡이 유지 시작 · 이동하거나 F를 누르면 해제" : "손잡이를 놓았습니다. 조작 진행은 유지됩니다."); return; }
            if (id == "exit" && player.position.x <= 62.4f) { Notify("외부 출입문 C의 바깥으로 걸어가세요."); return; }
            bool firstManifest = !State.transferMismatch, firstPower = !State.powerRestored;
            if (!State.Apply(id, out var message)) { Notify(message); return; }
            map.SetStationActive(id, true);
            if (id == "manifest" && firstManifest) BeginPursuit(1, new Vector2(18,6.8f));
            if (firstPower && State.powerRestored) BeginPursuit(2, new Vector2(12,-12));
            if (id == "power-together" || id == "power-alone")
            {
                assistedHold = manualHeld = false; hold = 0;
                if (pursuit != 3) BeginPursuit(3, new Vector2(47,0));
            }
            if (id == "safe" || id == "service-exit") { chasing = false; pursuer.gameObject.SetActive(false); }
            if (id == "meet" || id == "safe" || id == "service-exit" || id == "lock-record" || id == "reunite") SaveCheckpoint();
            if (State.ending != "")
            {
                chasing = false; pursuer.gameObject.SetActive(false); flashlight.enabled = false; footsteps.Stop(); ambience.Stop();
                epilogue = new NightRoundEpilogue(map,transform,player,nurse,cameraView,State.ending == "together");
                Debug.Log("[LastNightRound] ending=" + State.ending + " exterior crossings verified");
            }
            else if (id.StartsWith("circuit-") || id == "manual") Notify(message);
            else Tell(StationLabel(id), message);
        }

        private void UpdateNurse()
        {
            if(State.nurseCrossed)
            {
                if(player.position.x<=59.45f) return;
                Navigate(nurse,NurseRoute[7],3.4f);
                if(nurse.position.x>62.4f && !State.nurseOutside) State.Apply("nurse-outside",out _);
            }
            else if(State.transferMismatch)
            {
                if(nurseLeg==3 && !State.reunited) return;
                if(nurseLeg==4 && State.finalPower!=1) return;
                if(nurseLeg>=6 && (!State.manualDoor || !manualHeld || State.finalPower!=1)) return;
                Navigate(nurse,NurseRoute[Mathf.Min(nurseLeg,6)],4.3f);
                if(Vector2.Distance(nurse.position,NurseRoute[Mathf.Min(nurseLeg,6)])<.15f) { nurseLeg++; paths.Remove(nurse); }
                if(nurse.position.x>=59.9f && State.manualDoor && manualHeld && State.finalPower==1)
                { State.Apply("nurse-crossed",out var message); assistedHold=manualHeld=false; Notify(message); }
            }
            else if(State.metNurse)
            {
                if(Vector2.Distance(nurse.position,player.position)>2) Navigate(nurse,player.position,3.8f);
            }
        }

        private void BeginPursuit(int number, Vector2 at)
        {
            pursuit = number; chasing = true; warning = number == 3 ? 10 : 3;
            pursuer.position = at; paths.Remove(pursuer); pursuer.gameObject.SetActive(true);
            Notify("호출등이 켜졌습니다. 복도에서 빠른 발소리가 다가옵니다.");
            Debug.Log("[LastNightRound] pursuit=" + number);
        }
        private void UpdatePursuer()
        {
            if (!chasing) { tension = Mathf.Max(0, tension - Time.deltaTime * 10); return; }
            if (warning > 0) { warning -= Time.deltaTime; return; }
            Navigate(pursuer,player.position,2.7f);
            float distance = Vector2.Distance(player.position, pursuer.position);
            tension = Mathf.Clamp(tension + Time.deltaTime * (distance < 5 ? 15 : -6), 0, 100);
            if (distance < .65f) { dead = true; captureAt = Time.unscaledTime; Debug.Log("[LastNightRound] caught"); }
        }

        private void Notify(string value) { notice = value; noticeUntil = Time.unscaledTime + 5; }
        private void Tell(string title, string body) { documentTitle = title; document = body; documentScroll=Vector2.zero; openedAt = Time.unscaledTime; }
        private void SaveCheckpoint(bool disk = true)
        {
            checkpoint = new SaveData { nurseLeg=nurseLeg, state = JsonUtility.FromJson<NightRoundState>(JsonUtility.ToJson(State)), player = player.position, nurse = nurse.position };
            if (disk) try { File.WriteAllText(SavePath, JsonUtility.ToJson(checkpoint, true)); }
                catch (Exception e) { Notify("저장 실패: " + e.Message); Debug.LogWarning(e.Message); }
        }
        private void Restore(bool disk)
        {
            try
            {
                var saved = disk ? JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath)) : checkpoint;
                if (saved == null || (saved.version != 1 && saved.version != 2) || saved.state == null) throw new Exception("호환되지 않는 개발 저장입니다.");
                epilogue?.Dispose(); epilogue = null;
                State = JsonUtility.FromJson<NightRoundState>(JsonUtility.ToJson(saved.state));
                if(saved.version==1)
                {
                    // Migrate old horizontal-blockout coordinates once, preserving inventory/story.
                    int oldRoom=Mathf.Clamp(Mathf.FloorToInt((saved.player.x+9)/18),0,4);
                    saved.player+=NightRoundMap.Centers[oldRoom]-new Vector2(oldRoom*18,0);
                    if(State.reunited) { saved.nurse=NurseRoute[3]; saved.nurseLeg=4; State.radioAnswered=true; }
                    else if(State.transferMismatch) { saved.nurse=NurseRoute[2]; saved.nurseLeg=3; }
                    else saved.nurse=saved.player+Vector2.right;
                    saved.state=JsonUtility.FromJson<NightRoundState>(JsonUtility.ToJson(State));
                    saved.version=2;
                }
                player.position=saved.player; nurse.position=saved.nurse; nurseLeg=saved.nurseLeg; checkpoint=saved;
                paths.Clear(); repathAt=0;
                cameraView.transform.position=CameraTarget(player.position); cameraVelocity=Vector3.zero;
                currentRoom=CurrentRoom;
                focus = null;
                assistedHold = manualHeld = false;
                chasing = dead = paused = false; document = null; hold = tension = warning = 0; pursuit = 0;
                pursuer.gameObject.SetActive(false); Time.timeScale = 1;
                if (!ambience.isPlaying) ambience.Play();
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
                case "circuit-door": return "연결문 회로"; case "service-exit": return "설비실 출구 확인";
                case "lock-record": return "외부 잠금 이력"; case "reunite": return "관찰창의 서연";
                case "recording": return "천장 방송에 질문하기"; case "radio": return "무전으로 새 질문하기";
                case "power-together": return "연결문 유지 / 함께 탈출"; case "power-alone": return "주 출구 우선 / 먼저 탈출";
                case "manual": return "B문 손잡이 — E 유지 / F·클릭 유지 토글"; case "exit": return "C문 밖으로 걸어가기";
                default: return id;
            }
        }

        private void OnGUI()
        {
            if (State == null) return;
            if (text == null)
            {
                text = new GUIStyle(GUI.skin.label) { font = font, fontSize = 22, wordWrap = true };
                heading = new GUIStyle(text) { font = headingFont, fontSize = 32 };
                button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 22, wordWrap = true };
            }
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1600f, Screen.height / 900f, 1));
            if (epilogue != null)
            {
                epilogue.Draw(text,heading);
                if (paused)
                {
                    GUI.Box(new Rect(450,300,700,180),""); GUI.Label(new Rect(500,330,600,60),"잠시 숨 고르기",heading);
                    if (GUI.Button(new Rect(550,410,500,55),"계속하기",button)) paused=false;
                }
                else if (epilogue.Complete)
                {
                    if (GUI.Button(new Rect(220,730,350,70),"최종 선택 다시 · R",button)) Restore(false);
                    if (GUI.Button(new Rect(625,730,350,70),"새로 시작",button)) StartCoroutine(Launch());
                    if (GUI.Button(new Rect(1030,730,350,70),"타이틀로",button)) ReturnToTitle();
                }
                GUI.matrix=Matrix4x4.identity; return;
            }
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
            if (!IsBlocked)
            {
                if (Time.unscaledTime<roomTitleUntil)
                {
                    GUI.color=new Color(1,1,1,Mathf.Clamp01(roomTitleUntil-Time.unscaledTime));
                    GUI.Label(new Rect(40,30,650,50),RoomNames[currentRoom],heading); GUI.color=Color.white;
                }
                if (GUI.Button(new Rect(1390,24,175,44),"목표 · Tab",button)) objectiveVisible=!objectiveVisible;
                if (objectiveVisible)
                {
                    GUI.Box(new Rect(870,82,695,135),"");
                    GUI.Label(new Rect(890,98,650,110),State.Objective,text);
                }
                GUI.color=new Color(1,1,1,.6f);
                GUI.Label(new Rect(35,852,400,36),"I 소지품  ·  J 기록  ·  Esc 메뉴",text); GUI.color=Color.white;
                if (State.firstEscape && focus!=null && focus.StartsWith("circuit-"))
                {
                    GUI.Box(new Rect(410,724,780,54), "");
                    GUI.Label(new Rect(430,733,750,42), "전력 " + State.PoweredCircuitCount + "/2    " + ((State.circuits&1)!=0 ? "조명 ON   " : "조명 OFF   ") + ((State.circuits&2)!=0 ? "물품 ON   " : "물품 OFF   ") + ((State.circuits&4)!=0 ? "문 ON" : "문 OFF"), text);
                }
            }
            if (!IsBlocked && focus != null)
            {
                GUI.Box(new Rect(350, 790, 900, 80), "");
                string action = StationLabel(focus);
                if (focus.StartsWith("circuit-")) action += State.IsCircuitPowered(focus) ? " 끄기" : " 켜기";
                if (GUI.Button(new Rect(370, 804, 860, 50), "E  " + action + (focus=="manual" && hold>0 ? "  " + Mathf.CeilToInt(hold / 2 * 100) + "%" : ""), button)) Interact(focus);
            }
            if (Time.unscaledTime < noticeUntil) GUI.Label(new Rect(280, 710, 1100, 70), notice, text);
            if (chasing && !IsBlocked)
            {
                Vector2 danger=pursuer.position-player.position;
                string bearing=Mathf.Abs(danger.x)>Mathf.Abs(danger.y) ? (danger.x<0 ? "← 왼쪽" : "오른쪽 →") : (danger.y>0 ? "↑ 위쪽" : "↓ 아래쪽");
                GUI.color = new Color(.40f,.10f,.08f,.95f); GUI.DrawTexture(new Rect(850,128,710,76),Texture2D.whiteTexture); GUI.color = Color.white;
                GUI.Label(new Rect(870,137,670,66),warning > 0 ? "복도 끝에서 문이 열립니다…" : "순회자 접근  /  " + bearing + "\n열린 통로로 이동하세요.",text);
            }
            if (document != null)
            {
                GUI.Box(new Rect(230, 175, 1140, 550), "");
                GUI.Label(new Rect(280, 210, 1040, 70), documentTitle, heading);
                float bodyHeight=Mathf.Max(325,text.CalcHeight(new GUIContent(document),1010));
                documentScroll=GUI.BeginScrollView(new Rect(280,280,1040,325),documentScroll,new Rect(0,0,1010,bodyHeight));
                GUI.Label(new Rect(0,0,1010,bodyHeight),document,text);
                GUI.EndScrollView();
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
        private void OnDestroy() { if (Instance == this) Instance = null; epilogue?.Dispose(); map?.Dispose(); }
    }
}
