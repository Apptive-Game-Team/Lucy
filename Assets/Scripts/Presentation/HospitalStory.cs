using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lucy.Presentation
{
    [DefaultExecutionOrder(-100)]
    public sealed class HospitalStory : MonoBehaviour
    {
        private static readonly HashSet<int> found = new HashSet<int>();
        public static int FoundCount => found.Count;
        public static HospitalStory ActiveRecord { get; private set; }
        public static bool HasRecord(int index) => found.Contains(index);
        private int index;
        private Vector2 point;
        private GameObject marker;
        private static readonly string[] titles = {
            "01 / 침상 아래의 메모", "02 / 야간 간호 기록", "03 / 비상 전력 점검표", "04 / 루시에게 남긴 편지"
        };
        private static readonly string[] pages = {
            "손목의 입원 팔찌에 내 이름이 적혀 있다. 루시.\n어젯밤 정전 이후의 기억은 희미하다.\n\n침상 밑 메모에는 급하게 쓴 문장이 남아 있다.\n‘루시, 깨어나면 손전등을 챙겨. 2층 간호실에\n대피 기록을 남겼어. 혼자 어두운 복도로 뛰지 마.’\n\n오른쪽 서랍 앞에서 E로 열고, 다시 E를 눌러\n손전등을 꺼내자. 가방(I)에서 손전등을 장착한 뒤\n방을 나가 오른쪽 계단을 찾아야 한다.",
            "02:17 — 정전. 환자들은 동쪽 출구로 대피했다.\n루시를 찾으러 돌아간 간호사는 아직 합류하지 못했다.\n\n비상 출구의 보조 전력이 끊겼다.\n연결 병동의 스위치를 밟으면 통로가 이어진다.\n우측 1·2·3번 → 병동으로 복귀 → 좌측 4·5번 →\n다시 우측 6번. 마지막 점검함에 복구 기록이 있다.\n\n아래쪽 복도의 오른쪽 출입구부터 향하자.\n발판 밖에서는 빛과 정신력이 더 빨리 소모된다.",
            "비상 전력 계통 — 연결 확인.\n스위치에서 이어진 다리를 건너 점검함에 도착했다.\n이제 3층 비상 통로의 잠금을 해제할 수 있다.\n\n접힌 점검표 뒤에는 같은 필체가 남아 있다.\n‘내가 늦더라도 돌아오지 마. 네 잘못이 아니야.\n출구까지 벽의 안내를 따라가. 밖에서 기다릴게.’\n\n병동으로 돌아가 중앙 복도를 남쪽 끝까지 내려가자.\n서쪽 병실의 간호사와 이야기하고 E로 문을 연 뒤\n병실 안으로 들어가면 출구의 바리케이드를 치워 준다.\n그다음 중앙 복도로 돌아와 북동쪽 비상 통로로 가자.",
            "루시에게.\n네가 무서워하던 발소리를 나도 들었어.\n무엇인지 확인하려고 멈출 필요는 없어.\n\n대피 명단에서 네 이름을 지우지 않았어.\n네 자리는 아직 밖에 남아 있어.\n숨을 고르고, 출구 쪽 불빛을 따라와.\n\n편지를 접어 주머니에 넣었다.\n복도 너머의 아침을 향해, 이번에는 내가 걸어간다."
        };
        public static string Progress => $"대피 기록 {found.Count}/4  ·  J 기록장";
        public static void ResetRun() { found.Clear(); }
        public static string EndingText => "차가운 새벽 공기 너머로 누군가 내 이름을 불렀다.\n대피 명단에는 아직 ‘루시’가 남아 있었다.\n접힌 편지를 쥔 채, 나는 그 목소리 쪽으로 걸었다.";
        public static string JournalText()
        {
            string text = "정전된 병원에서 나에게 남겨진 대피 기록.\n\n";
            for (int i = 0; i < titles.Length; i++) text += (found.Contains(i) ? "● " + titles[i] : "○ 아직 찾지 못한 기록") + "\n";
            return text + "\n병동의 빛나는 기록 가까이에서 E로 읽을 수 있습니다.\n기록을 읽는 동안 시간과 자원 소모가 멈춥니다.";
        }
        public static void EnterScene(string name)
        {
            int id; Vector2 position;
            switch (name)
            {
                case GameSession.FirstScene: id = 0; position = new Vector2(-1, -3.2f); break;
                case "0201": id = 1; position = new Vector2(-7, 14.5f); break;
                case "Puzzle_2F": id = 2; position = new Vector2(29, 11); break;
                case "Maze_3F": id = 3; position = new Vector2(-6, -4); break;
                default: return;
            }
            var go = new GameObject("Hospital record " + (id + 1));
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
            var story = go.AddComponent<HospitalStory>();
            story.index = id; story.point = position;
            ActiveRecord = story;
            go.transform.position = position;
            story.BuildMarker();
        }
        private void BuildMarker()
        {
            marker = new GameObject("Record label"); marker.transform.SetParent(transform, false);
            var label = marker.AddComponent<TextMesh>();
            label.font = Resources.Load<Font>("Presentation/LucyFont");
            label.fontSize = 48; label.characterSize = .034f; label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.text = found.Contains(index) ? "기록" : "◆ 기록";
            label.color = new Color(.8f, 1f, .85f);
            var renderer = label.GetComponent<MeshRenderer>(); renderer.material = label.font.material;
            renderer.sortingOrder = 150;
        }
        private void Update()
        {
            if (GameSession.Instance == null || GameSession.BlocksGameplayInput || Time.timeScale <= 0) return;
            if (Portal.PortalManager.Instance != null && Portal.PortalManager.Instance.IsTransitioning) return;
            var player = Portal.Character.Instance;
            if (player == null || !player.gameObject.activeInHierarchy) return;
            float distance = Vector2.Distance(player.transform.position, point);
            if (marker != null)
            {
                var label = marker.GetComponent<TextMesh>();
                bool nearby = distance <= 1.6f;
                label.color = nearby ? new Color(1,.88f,.45f) : new Color(.65f,.85f,.75f);
                label.text = found.Contains(index) ? "기록" : "◆ 기록";
            }
            if (HospitalInteraction.Focus != this || (!Input.GetKeyDown(KeyCode.E) && !Input.GetKeyDown(KeyCode.Z))) return;
            bool first = found.Add(index);
            if (first) GameSession.Instance.RegisterCheckpoint(SceneManager.GetActiveScene().name, player.transform.position);
            if (first && CharacterStat.instance != null)
            {
                CharacterStat.instance.curMental = Mathf.Min(CharacterStat.instance.maxMental, CharacterStat.instance.curMental + 20);
                CharacterStat.instance.UpdateStats();
            }
            if (first && index > 0 && invertoryAndItem.FlashLight.instance != null)
                invertoryAndItem.FlashLight.instance.Refill(3);
            marker.GetComponent<TextMesh>().text = "기록";
            GameSession.Instance.ShowDocument(titles[index], pages[index] + (first ? "\n\n정신력 +20 · 이 기록에서 다시 이어갈 수 있습니다." + (index > 0 ? "\n대피 물품함에서 배터리를 최대 3칸 보충했다." : "") : ""));
            Debug.Log("[Lucy Story] Read record " + (index + 1) + "; total " + found.Count);
        }
    }
}
