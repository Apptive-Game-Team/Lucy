using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lucy.Presentation
{
    /// <summary>Visual receipt for the already committed remote-to-battery inventory transaction.</summary>
    public sealed class RemoteDisassemblyView : MonoBehaviour
    {
        private readonly Sprite[] stages = new Sprite[3];
        private Text heading, caption;
        private Image artwork;
        private GameObject confirm;
        private bool finished, acknowledged;

        public static RemoteDisassemblyView Create(Transform parent)
        {
            var go = new GameObject("Remote battery extraction", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(.015f,.025f,.035f,.86f);
            var view = go.AddComponent<RemoteDisassemblyView>();
            view.Build();
            return view;
        }

        private void Build()
        {
            var font = Resources.Load<Font>("Presentation/LucyFont");
            var panel = Rect("Extraction panel", transform, Vector2.zero, new Vector2(860,700));
            panel.gameObject.AddComponent<Image>().color = new Color(.075f,.105f,.12f,1);
            heading = Label(panel, font, new Vector2(0,290), new Vector2(780,55), 30);
            caption = Label(panel, font, new Vector2(0,-225), new Vector2(780,62), 22);
            var artRect = Rect("Remote illustration", panel, new Vector2(0,30), new Vector2(690,440));
            artwork = artRect.gameObject.AddComponent<Image>();
            artwork.preserveAspect = true; artwork.raycastTarget = false;
            var atlas = Resources.Load<Texture2D>("Presentation/RemoteDisassembly");
            if (atlas != null)
            {
                // Authored panels are unequal widths; equal thirds would clip the open remote.
                float[] edges = { 0f, .26f, .61f, 1f };
                for (int i = 0; i < stages.Length; i++)
                {
                    float left = edges[i] * atlas.width;
                    float width = (edges[i+1] - edges[i]) * atlas.width;
                    stages[i] = Sprite.Create(atlas, new Rect(left,0,width,atlas.height), new Vector2(.5f,.5f), atlas.height, 0, SpriteMeshType.FullRect);
                }
            }
            else artwork.enabled = false;
            var buttonRect = Rect("Confirm extraction", panel, new Vector2(0,-310), new Vector2(280,48));
            confirm = buttonRect.gameObject;
            confirm.AddComponent<Image>().color = new Color(.22f,.35f,.34f);
            var button = confirm.AddComponent<Button>();
            button.onClick.AddListener(() => { if (finished && !GameSession.BlocksGameplayInput) acknowledged = true; });
            var text = Label(buttonRect, font, Vector2.zero, new Vector2(270,46), 21);
            text.text = "확인  ·  SPACE";
            confirm.SetActive(false);
        }

        public IEnumerator Show()
        {
            var titles = new[] { "01  /  리모컨을 뒤집는다", "02  /  배터리 덮개를 연다", "03  /  배터리 +1" };
            var descriptions = new[] { "뒷면의 배터리 덮개를 확인합니다.", "안쪽에 아직 쓸 수 있는 배터리가 있습니다.", "꺼낸 배터리를 가방에 넣었습니다.\n손전등이 필요할 때 사용하세요." };
            for (int i = 0; i < stages.Length; i++)
            {
                heading.text = titles[i]; caption.text = descriptions[i];
                if (stages[i] != null) artwork.sprite = stages[i];
                if (i == 2) break;
                float elapsed = 0;
                while (elapsed < .9f)
                {
                    if (!GameSession.BlocksGameplayInput) elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
            finished = true;
            confirm.SetActive(true);
            // Wait a frame so the key which triggered an action cannot dismiss its receipt.
            yield return null;
            while (!acknowledged)
            {
                if (!GameSession.BlocksGameplayInput && Input.GetKeyDown(KeyCode.Space)) acknowledged = true;
                yield return null;
            }
        }

        public void Dismiss()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,.5f);
            rect.anchoredPosition = position; rect.sizeDelta = size;
            return rect;
        }

        private static Text Label(Transform parent, Font font, Vector2 position, Vector2 size, int fontSize)
        {
            var text = Rect("Label", parent, position, size).gameObject.AddComponent<Text>();
            text.font = font; text.fontSize = fontSize; text.color = new Color(.88f,.95f,.92f);
            text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
            return text;
        }

        private void OnDestroy()
        {
            foreach (var sprite in stages) if (sprite != null) Destroy(sprite);
        }
    }
}
