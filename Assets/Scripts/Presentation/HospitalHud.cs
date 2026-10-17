using UnityEngine;
using UnityEngine.UI;
using invertoryAndItem;

namespace Lucy.Presentation
{
    /// <summary>Presentation only: reads existing stats and keeps their legacy UI bindings alive.</summary>
    public sealed class HospitalHud : MonoBehaviour
    {
        private readonly Text[] values = new Text[3];
        private readonly RectTransform[] fills = new RectTransform[3];
        private Text objective, progress;
        private RectTransform interactionPanel;
        private Text interactionText;
        private Font font;
        private string sceneName;
        private float nextLegacyCheck;
        private static readonly Color TextColor = new Color(.88f, .91f, .89f);
        private static readonly Color Muted = new Color(.65f, .72f, .70f);

        public void Initialize(Font uiFont, string currentScene)
        {
            font = uiFont;
            sceneName = currentScene;
            var status = Panel("Status", new Vector2(0, 1), new Vector2(24, -24), new Vector2(292, 112));
            string[] labels = { "정신력", "체력", "배터리" };
            Color[] colors = { new Color(.58f, .73f, .68f), new Color(.64f, .70f, .76f), new Color(.77f, .71f, .51f) };
            for (int i = 0; i < labels.Length; i++)
            {
                float y = -12 - i * 31;
                TextAt(status, labels[i], new Vector2(14, y), new Vector2(78, 23), 18, Muted);
                values[i] = TextAt(status, "—", new Vector2(206, y), new Vector2(70, 23), 18, TextColor);
                values[i].alignment = TextAnchor.MiddleRight;
                var track = Box(status, "Track", new Vector2(96, y - 9), new Vector2(99, 5));
                track.gameObject.AddComponent<Image>().color = new Color(.25f, .31f, .30f, .8f);
                var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fills[i] = fill.GetComponent<RectTransform>(); fills[i].SetParent(track, false);
                fills[i].anchorMin = Vector2.zero; fills[i].anchorMax = Vector2.one;
                fills[i].offsetMin = fills[i].offsetMax = Vector2.zero;
                fill.GetComponent<Image>().color = colors[i];
            }
            var task = Panel("Objective", new Vector2(1, 1), new Vector2(-24, -24), new Vector2(430, 112));
            objective = TextAt(task, "", new Vector2(16, -12), new Vector2(398, 58), 21, TextColor);
            objective.lineSpacing = 1.08f;
            progress = TextAt(task, "", new Vector2(16, -79), new Vector2(398, 22), 16, Muted);
            interactionPanel = Panel("Interaction prompt", new Vector2(.5f, 0), new Vector2(0, 38), new Vector2(460, 48));
            interactionText = TextAt(interactionPanel, "", new Vector2(16, -8), new Vector2(428, 32), 22, TextColor);
            interactionText.alignment = TextAnchor.MiddleCenter;
            foreach (var graphic in GetComponentsInChildren<Graphic>()) graphic.raycastTarget = false;
            Refresh();
            HideLegacyIndicators();
        }

        private RectTransform Panel(string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            var rect = panel.GetComponent<RectTransform>(); rect.SetParent(transform, false);
            rect.anchorMin = rect.anchorMax = anchor; rect.pivot = anchor;
            rect.anchoredPosition = position; rect.sizeDelta = size;
            panel.GetComponent<Image>().color = new Color(.035f, .055f, .055f, .78f);
            var group = panel.GetComponent<CanvasGroup>(); group.blocksRaycasts = false; group.interactable = false;
            return rect;
        }

        private static RectTransform Box(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position; rect.sizeDelta = size;
            return rect;
        }

        private Text TextAt(Transform parent, string text, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var value = Box(parent, "Label", position, size).gameObject.AddComponent<Text>();
            value.font = font; value.fontSize = fontSize; value.text = text; value.color = color;
            value.alignment = TextAnchor.UpperLeft;
            value.horizontalOverflow = HorizontalWrapMode.Wrap;
            value.verticalOverflow = VerticalWrapMode.Truncate;
            value.raycastTarget = false;
            return value;
        }

        private void LateUpdate()
        {
            Refresh();
            if (Time.unscaledTime >= nextLegacyCheck)
            {
                HideLegacyIndicators();
                nextLegacyCheck = Time.unscaledTime + .5f;
            }
        }

        private void Refresh()
        {
            var stats = CharacterStat.instance;
            var light = FlashLight.instance;
            SetValue(0, stats != null ? stats.curMental : 0, stats != null ? stats.maxMental : 100, stats != null);
            SetValue(1, stats != null ? stats.curStamina : 0, stats != null ? stats.maxStamina : 100, stats != null);
            SetValue(2, light != null ? light.battery : 0, light != null ? light.Capacity : 9, light != null);
            if (objective != null) objective.text = CompactObjective();
            if (progress != null) progress.text = "기록 " + HospitalStory.FoundCount + "/4    J 기록장    ESC 메뉴";
            if (HospitalTutorial.Visible(sceneName))
            {
                objective.fontSize=18;
                objective.text=HospitalTutorial.Instruction(sceneName);
                progress.text=HospitalTutorial.Heading;
            }
            else if (objective!=null) objective.fontSize=21;
            if (interactionPanel != null)
            {
                var focus = HospitalInteraction.Focus;
                interactionPanel.gameObject.SetActive(focus != null);
                if (focus != null) interactionText.text = "[E / Z]   " + HospitalInteraction.Label(focus);
            }
        }

        private void SetValue(int index, float value, float maximum, bool available)
        {
            if (values[index] == null) return;
            values[index].text = available ? Mathf.CeilToInt(value) + "/" + Mathf.CeilToInt(maximum) : "—";
            fills[index].anchorMax = new Vector2(available ? Mathf.Clamp01(value / Mathf.Max(1, maximum)) : 0, 1);
        }

        private string CompactObjective()
        {
            switch (sceneName)
            {
                case "0101": case GameSession.FirstScene: return "기록과 손전등을 찾으세요.\n오른쪽 계단으로 향하세요.";
                case "0102": return "계단을 올라\n병원 안쪽을 탐색하세요.";
                case "0201": return HospitalStory.HasRecord(2)
                    ? "남서쪽 병실에서 간호사를 만난 뒤\n북동쪽 비상 통로로 향하세요."
                    : "간호 기록을 읽고\n남쪽 우측 연결 통로로 향하세요.";
                case "Puzzle_2F": return "스위치로 다리를 연결하세요.\n마지막 점검 기록을 찾으세요.";
                case "Maze_3F": return "발소리에 귀를 기울이며\n미로의 출구를 찾으세요.";
                default: return "빛을 따라\n주변을 조사하세요.";
            }
        }

        private static void HideLegacyIndicators()
        {
            if (UIController.Instance != null)
            {
                var stats = UIController.Instance.transform.Find("StatCanvas 1");
                if (stats != null)
                {
                    var group = stats.GetComponent<CanvasGroup>();
                    if (group == null) group = stats.gameObject.AddComponent<CanvasGroup>();
                    group.alpha = 0; group.blocksRaycasts = false; group.interactable = false;
                }
            }
            var light = FlashLight.instance;
            if (light == null) return;
            HideImages(light.activeBatteries);
            HideImages(light.inactiveBatteries);
        }

        private static void HideImages(Image[] images)
        {
            if (images == null) return;
            foreach (var image in images) if (image != null) image.enabled = false;
        }
    }
}
