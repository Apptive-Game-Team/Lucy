using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lucy.LastNightRound
{
    /// <summary>Self-contained world-space blockout. No input, physics components or game state.</summary>
    public sealed class NightRoundMap : IDisposable
    {
        public readonly List<Rect> Walls = new List<Rect>();
        public readonly Dictionary<string, Vector2> Stations = new Dictionary<string, Vector2>();
        public Transform Root;
        public Sprite Pixel;
        private Texture2D texture;
        private Font font;
        private readonly Dictionary<string, SpriteRenderer> markers = new Dictionary<string, SpriteRenderer>();
        private readonly List<Rect> floors = new List<Rect>();
        private static readonly Color ActiveColor = new Color(.80f, .72f, .40f);
        private static readonly Color InactiveColor = new Color(.27f, .41f, .40f);

        public void Build()
        {
            Dispose();
            Root = new GameObject("Last Night Round / Map").transform;
            texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "Night Round solid pixel";
            texture.filterMode = FilterMode.Point;
            texture.SetPixel(0, 0, Color.white); texture.Apply();
            Pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
            font = Resources.Load<Font>("Presentation/LucyFont");
            string[] names = { "01  입원 병동", "02  야간 간호실", "03  전력 설비실", "04  기록 보관실", "05  마지막 출구" };
            Color[] colors = {
                new Color(.13f,.20f,.21f), new Color(.16f,.22f,.22f),
                new Color(.19f,.20f,.22f), new Color(.20f,.19f,.22f), new Color(.15f,.23f,.22f)
            };
            for (int i = 0; i < 5; i++)
            {
                float x = i * 18;
                var room = new Rect(x - 8, -6, 16, 12); floors.Add(room);
                Box(names[i] + " / floor", room, colors[i], -20);
                for (int gx = -7; gx <= 7; gx++)
                    Box("Floor joint", new Rect(x + gx, -5.7f, .018f, 11.4f), colors[i] * .86f, -19);
                for (int gy = -5; gy <= 5; gy++)
                    Box("Floor joint", new Rect(x - 7.7f, gy, 15.4f, .018f), colors[i] * .86f, -19);
                if (i == 1 || i == 4)
                {
                    float door = i == 1 ? 21 : 70;
                    Wall(new Rect(x - 8, 5.7f, door - 1 - (x - 8), .3f));
                    Wall(new Rect(door + 1, 5.7f, x + 8 - (door + 1), .3f));
                }
                else Wall(new Rect(x - 8, 5.7f, 16, .3f));
                Wall(new Rect(x - 8, -6, 16, .3f));
                SideWall(x - 8, i == 0);
                SideWall(x + 7.7f, i == 4);
                Label(names[i], new Vector2(x, -5.4f), .25f, new Color(.71f,.79f,.77f));
                // Furniture is solid in the same rectangle used by CanMove. All stations
                // and the central east-west walking lane remain clear.
                Furniture(i, x);
                if (i == 4) continue;
                var hall = new Rect(x + 8, -1.5f, 2, 3); floors.Add(hall);
                Box("Connecting corridor", hall, new Color(.17f,.25f,.24f), -20);
                Wall(new Rect(x + 8, 1.5f, 2, .3f));
                Wall(new Rect(x + 8, -1.8f, 2, .3f));
                Box("Door threshold", new Rect(x + 8.8f, -1.45f, .12f, 2.9f), new Color(.36f,.44f,.40f), -18);
            }
            StaffCorridor();
            Station("flashlight", -5, -3, "손전등");
            Station("bracelet", -5, 3, "입원 팔찌");
            Station("meet", 5, 0, "간호사와 합류");
            Station("manifest", 15, 3, "대피 명단");
            Station("safe", 22, -3, "직원 휴게실");
            Station("circuit-light", 31, 3, "조명 회로");
            Station("circuit-supply", 34, 3, "물품실 회로");
            Station("circuit-door", 37, 3, "문 제어 회로");
            Station("fuse", 40, -3, "예비 퓨즈");
            Station("service-exit", 42, 0, "설비실 출입문");
            Station("lock-record", 49, 3, "봉인된 기록");
            Station("reunite", 59, 0, "다시 만나는 곳");
            Station("power-together", 68, 3, "함께 전원 복구");
            Station("power-alone", 72, 3, "혼자 전원 복구");
            Station("manual", 75, -2, "수동 개방 장치");
            Station("exit", 79, 0, "병원 밖으로");
        }

        private void StaffCorridor()
        {
            var corridor = new Rect(20, 6, 51, 2.7f);
            floors.Add(corridor);
            Box("Staff service corridor", corridor, new Color(.12f,.19f,.20f), -20);
            Wall(new Rect(19.7f, 6, .3f, 3));
            Wall(new Rect(71, 6, .3f, 3));
            Wall(new Rect(19.7f, 8.7f, 51.6f, .3f));
            Wall(new Rect(22, 6, 47, .3f));
            Box("Staff route stripe", new Rect(20.6f, 7.65f, 49.8f, .08f), new Color(.28f,.40f,.37f), -18);
            Label("직원 전용 점검 통로", new Vector2(45, 8.2f), .19f, new Color(.62f,.72f,.68f));
        }

        private void SideWall(float x, bool closed)
        {
            if (closed) { Wall(new Rect(x, -6, .3f, 12)); return; }
            Wall(new Rect(x, -6, .3f, 4.5f));
            Wall(new Rect(x, 1.5f, .3f, 4.5f));
        }

        private void Wall(Rect rect)
        {
            Walls.Add(rect);
            Box("Solid wall", rect, new Color(.38f,.46f,.44f), 2);
        }

        private void Furniture(int room, float x)
        {
            string[] types = { "Patient bed", "Nurses desk", "Electrical cabinet", "Archive shelves", "Waiting bench" };
            var rect = new Rect(x - 1.7f, -5.1f, 3.4f, 1.3f);
            Walls.Add(rect);
            Box(types[room], rect, new Color(.30f,.39f,.39f), 0);
            if (room == 0)
            {
                Box("Bed pillow", new Rect(x - 1.45f,-4.9f,.65f,.9f), new Color(.66f,.71f,.67f), 1);
                Box("Bed blanket", new Rect(x -.6f,-4.95f,2.05f,1), new Color(.32f,.51f,.48f), 1);
            }
            else
            {
                for (int n = 0; n < 3; n++)
                    Box("Furniture inset", new Rect(x - 1.4f + n, -4.9f, .8f, .85f), new Color(.21f,.29f,.30f), 1);
            }
        }

        private void Station(string id, float x, float y, string title)
        {
            var position = new Vector2(x, y); Stations.Add(id, position);
            Box(id + " / border", new Rect(x -.42f,y -.42f,.84f,.84f), new Color(.10f,.14f,.16f), 3);
            markers.Add(id, Box(id + " / marker", new Rect(x -.32f,y -.32f,.64f,.64f), ActiveColor, 4));
            Label(title, position + Vector2.up * .8f, .17f, new Color(.86f,.87f,.78f));
        }

        public void SetStationActive(string id, bool active)
        {
            if (markers.TryGetValue(id, out var marker) && marker != null)
                marker.color = active ? ActiveColor : InactiveColor;
        }

        private SpriteRenderer Box(string name, Rect rect, Color color, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(Root, false);
            go.transform.localPosition = rect.center;
            go.transform.localScale = new Vector3(rect.width, rect.height, 1);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Pixel; renderer.color = color; renderer.sortingOrder = order;
            return renderer;
        }

        private void Label(string text, Vector2 position, float size, Color color)
        {
            if (font == null) return;
            var go = new GameObject("Label / " + text); go.transform.SetParent(Root, false);
            go.transform.localPosition = position;
            var label = go.AddComponent<TextMesh>();
            label.text = text; label.font = font; label.fontSize = 64;
            label.characterSize = size * .32f; label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center; label.color = color;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material; renderer.sortingOrder = 5;
        }

        public bool CanMove(Vector2 position, float radius = .25f)
        {
            if (Root == null || float.IsNaN(position.x) || float.IsNaN(position.y)
                || float.IsInfinity(position.x) || float.IsInfinity(position.y)
                || float.IsNaN(radius) || float.IsInfinity(radius)) return false;
            radius = Mathf.Max(0, radius);
            bool inside = false;
            foreach (var floor in floors) if (floor.Contains(position)) { inside = true; break; }
            if (!inside) return false;
            foreach (var wall in Walls)
            {
                float dx = position.x - Mathf.Clamp(position.x, wall.xMin, wall.xMax);
                float dy = position.y - Mathf.Clamp(position.y, wall.yMin, wall.yMax);
                if (wall.Contains(position) || dx * dx + dy * dy < radius * radius) return false;
            }
            return true;
        }

        public void Dispose()
        {
            if (Root != null) { Root.gameObject.SetActive(false); UnityEngine.Object.Destroy(Root.gameObject); }
            if (Pixel != null) UnityEngine.Object.Destroy(Pixel);
            if (texture != null) UnityEngine.Object.Destroy(texture);
            Root = null; Pixel = null; texture = null; font = null;
            Walls.Clear(); Stations.Clear(); markers.Clear(); floors.Clear();
        }
    }
}
