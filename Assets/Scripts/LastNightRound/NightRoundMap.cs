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
        public NightRoundArt Art { get; private set; }
        private Texture2D texture;
        private Font font;
        private readonly Dictionary<string, SpriteRenderer> markers = new Dictionary<string, SpriteRenderer>();
        private readonly Dictionary<string, SpriteRenderer> stationArt = new Dictionary<string, SpriteRenderer>();
        private readonly Dictionary<string, TextMesh> stationLabels = new Dictionary<string, TextMesh>();
        private readonly Dictionary<string, string> stationTitles = new Dictionary<string, string>();
        private readonly Dictionary<string, SpriteRenderer> switches = new Dictionary<string, SpriteRenderer>();
        private SpriteRenderer connectionDoor, observationLamp;
        private readonly List<Rect> floors = new List<Rect>();
        private static readonly Color ActiveColor = new Color(.80f, .72f, .40f);
        private static readonly Color InactiveColor = new Color(.27f, .41f, .40f);

        public void Build()
        {
            Dispose();
            Art = new NightRoundArt();
            Root = new GameObject("Last Night Round / Map").transform;
            texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "Night Round solid pixel";
            texture.filterMode = FilterMode.Point;
            texture.SetPixel(0, 0, Color.white); texture.Apply();
            Pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
            font = Resources.Load<Font>("Presentation/LucyFont");
            string[] names = { "01  입원 병동", "02  야간 간호실", "03  전력 설비실", "04  기록 보관실", "05  마지막 출구" };
            for (int i = 0; i < 5; i++)
            {
                float x = i * 18;
                var room = new Rect(x - 8, -6, 16, 12); floors.Add(room);
                Floor(room, i == 2 ? 2 : i == 3 ? 1 : 0);
                if (i == 1 || i == 4)
                {
                    float door = i == 1 ? 21 : 70;
                    Wall(new Rect(x - 8, 5.7f, door - 1 - (x - 8), .3f));
                    Wall(new Rect(door + 1, 5.7f, x + 8 - (door + 1), .3f));
                }
                else Wall(new Rect(x - 8, 5.7f, 16, .3f));
                Wall(new Rect(x - 8, -6, 16, .3f));
                SideWall(x - 8, i == 0);
                SideWall(x + 7.7f, false);
                Label(names[i], new Vector2(x, -5.4f), .25f, new Color(.71f,.79f,.77f));
                // Furniture is solid in the same rectangle used by CanMove. All stations
                // and the central east-west walking lane remain clear.
                Furniture(i, x);
                DressRoom(i, x);
                if (i == 4) continue;
                var hall = new Rect(x + 8, -1.5f, 2, 3); floors.Add(hall);
                Floor(hall, 1);
                Wall(new Rect(x + 8, 1.5f, 2, .3f));
                Wall(new Rect(x + 8, -1.8f, 2, .3f));
                Box("Door threshold", new Rect(x + 8.8f, -1.45f, .12f, 2.9f), new Color(.36f,.44f,.40f), -18);
            }
            StaffCorridor();
            // A joins the staff corridor; B divides the final room; C leads outside.
            Wall(new Rect(76.85f,-6,.3f,4.5f));
            Wall(new Rect(76.85f,1.5f,.3f,4.5f));
            var outside = new Rect(80,-1.5f,5,3); floors.Add(outside); Floor(outside,0);
            Wall(new Rect(80,1.5f,5,.3f)); Wall(new Rect(80,-1.8f,5,.3f)); Wall(new Rect(85,-1.8f,.3f,3.6f));
            connectionDoor = NightRoundArt.Place(Root,"A / staff connection",Art.Atlas("WardDoors",2,2)[0],new Rect(69,5.1f,2,1.8f),40);
            NightRoundArt.Place(Root,"C / exterior doorway",Art.Atlas("WardDoors",2,2)[3],new Rect(79.7f,-1.5f,1.4f,3),120);
            Label("A · 서연의 연결문",new Vector2(70,4.8f),.17f,new Color(.71f,.88f,.76f));
            Label("B · 내부 방화문",new Vector2(75.5f,1.9f),.17f,new Color(.85f,.75f,.55f));
            Label("C · 병원 밖",new Vector2(82,1.1f),.17f,new Color(.85f,.90f,.82f));
            Box("Observation glass",new Rect(57.8f,5.5f,2.4f,.35f),new Color(.27f,.46f,.48f),6);
            observationLamp = Box("Live response lamp",new Rect(58.9f,5.2f,.2f,.2f),InactiveColor,8);
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
            Station("recording",53,-3,"천장 방송");
            Station("radio",54,3,"무전기로 새 질문");
            Station("reunite", 59, 4, "관찰창의 서연");
            Station("power-together", 68, 3, "함께 전원 복구");
            Station("power-alone", 72, 3, "혼자 전원 복구");
            Station("manual", 75, -2, "수동 개방 장치");
            Station("exit", 82, 0, "병원 밖으로");
        }

        private void StaffCorridor()
        {
            var corridor = new Rect(20, 6, 51, 2.7f);
            floors.Add(corridor);
            Floor(corridor, 2);
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
            var wall = NightRoundArt.Place(Root, "Hospital wall", Art.Atlas("HospitalSurfaces", 2, 2, false)[3], rect, 2, true);
            wall.color = new Color(.65f,.69f,.68f);
        }

        private void Floor(Rect area, int cell)
        {
            var sprite = Art.Atlas("HospitalSurfaces", 2, 2, false)[cell];
            for (float x = area.xMin; x < area.xMax; x += 4)
                for (float y = area.yMin; y < area.yMax; y += 4)
                {
                    var tile = NightRoundArt.Place(Root, "Hospital floor", sprite,
                        new Rect(x, y, Mathf.Min(4,area.xMax-x), Mathf.Min(4,area.yMax-y)), -20, true);
                    tile.color = new Color(.42f,.48f,.49f);
                }
        }

        private void Furniture(int room, float x)
        {
            string[] types = { "Patient bed", "Nurses desk", "Electrical cabinet", "Archive shelves", "Waiting bench" };
            var rect = new Rect(x - 1.7f, -5.1f, 3.4f, 1.3f);
            Sprite sprite = room == 0 ? Art.Atlas("HospitalFurnitureV2",2,2)[0]
                : room == 1 ? Art.Atlas("HospitalFurnitureV2",2,2)[2]
                : room == 4 ? Art.Atlas("HospitalWardProps",4,2)[7]
                : Art.Atlas("HospitalStorage",2,2)[room == 2 ? 2 : 3];
            var furniture = NightRoundArt.Place(Root, types[room], sprite, rect, 150);
            var bounds = furniture.bounds;
            Walls.Add(new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y));
            // Wall dressing stays outside the central navigation and investigation lanes.
            for (int n = 0; n < 3; n++)
                NightRoundArt.Place(Root, "Ward fixture", Art.Atlas("HospitalWardProps",4,2)[n == 1 ? 4 : 6],
                    new Rect(x - 6 + n * 5, 4.6f, 1.2f, .8f), 3);
        }

        private void Station(string id, float x, float y, string title)
        {
            var position = new Vector2(x, y); Stations.Add(id, position);
            markers.Add(id, Box(id + " / status", new Rect(x -.32f,y -.5f,.64f,.06f), ActiveColor, 4));
            Sprite prop;
            if (id == "flashlight") prop = Resources.Load<Sprite>("Presentation/FlashlightIcon");
            else if (id == "radio") prop = Resources.Load<Sprite>("Presentation/RemoteIcon");
            else if (id == "recording") prop = Art.Atlas("HospitalWardProps",4,2)[6];
            else if (id == "manifest" || id == "lock-record" || id == "bracelet") prop = Art.Atlas("HospitalWardProps",4,2)[4];
            else if (id == "safe") prop = Art.Atlas("HospitalWardProps",4,2)[7];
            else if (id == "meet" || id == "reunite" || id == "service-exit" || id == "exit") prop = Art.Atlas("WardDoors",2,2)[1];
            else prop = Art.Atlas("HospitalStorage",2,2)[id == "fuse" ? 1 : 2];
            stationArt.Add(id, NightRoundArt.Place(Root, id + " / art", prop, new Rect(x-.55f,y-.35f,1.1f,1.2f), 5));
            stationTitles.Add(id, title);
            stationLabels.Add(id, Label(title, position + Vector2.up * 1.25f, .17f, new Color(.86f,.87f,.78f)));
            if (id.StartsWith("circuit-") || id.StartsWith("power-"))
            {
                // Give the reused steel cabinet a readable breaker face and physical switch.
                Box(id + " / breaker face",new Rect(x-.32f,y-.15f,.64f,.68f),new Color(.12f,.17f,.18f),6);
                switches.Add(id,Box(id + " / breaker",new Rect(x-.12f,y,.24f,.24f),InactiveColor,7));
            }
        }

        private void DressRoom(int room, float x)
        {
            var furniture = Art.Atlas("HospitalFurnitureV2",2,2);
            var props = Art.Atlas("HospitalWardProps",4,2);
            var storage = Art.Atlas("HospitalStorage",2,2);
            // All new solid furniture lives outside the investigation rows at y=+/-3
            // and the east-west escape lane. The visible footprint is the collider.
            if (room == 0)
            {
                Furnish("Bed 101",furniture[0],new Rect(x-6.7f,-5.15f,3.1f,1.3f));
                Furnish("Bed 103",furniture[0],new Rect(x+3.3f,-5.15f,3.1f,1.3f));
                Furnish("Privacy screen",props[2],new Rect(x+5.1f,3.9f,1.7f,1.5f));
                Furnish("Medicine trolley",props[3],new Rect(x-1.1f,3.9f,1.7f,1.5f));
                Label("101",new Vector2(x-5.15f,-5.5f),.13f,Color.gray);
                Label("103",new Vector2(x+4.85f,-5.5f),.13f,Color.gray);
            }
            else if (room == 1)
            {
                Furnish("Nursing counter",furniture[2],new Rect(x-6.5f,4.1f,3.1f,1.3f));
                Furnish("Medicine cupboard",storage[0],new Rect(x-.8f,3.8f,1.3f,1.8f));
                Furnish("Staff bench",props[7],new Rect(x+3.7f,-5.2f,2.7f,1.2f));
                Furnish("Records trolley",props[3],new Rect(x-6.2f,-5.1f,1.6f,1.4f));
                Box("Safe room mat",new Rect(x+2.6f,-3.7f,2.8f,1.3f),new Color(.18f,.29f,.27f),-18);
            }
            else if (room == 2)
            {
                for (int n=0;n<3;n++) Furnish("Service locker",storage[2],new Rect(x-6.4f+n*1.3f,-5.2f,1,1.7f));
                Box("Breaker safety stripe",new Rect(x-6,2.25f,8,.08f),new Color(.57f,.46f,.20f),-18);
                Label("비상 공급  /  최대 2회로",new Vector2(x-2,5.2f),.19f,new Color(.8f,.72f,.49f));
            }
            else if (room == 3)
            {
                for (int n=0;n<2;n++) Furnish("Records archive",storage[3],new Rect(x-1+n*2.6f,3.9f,1.8f,1.5f));
                Furnish("Sealed files",storage[3],new Rect(x-6,-5.2f,1.8f,1.5f));
                Furnish("Wheelchair",props[0],new Rect(x+4.4f,-5.2f,1.5f,1.5f));
            }
            else
            {
                Furnish("Waiting chairs",props[7],new Rect(x-6.6f,-5.1f,3,1.3f));
                Box("Exit guide",new Rect(x+1,-.65f,5.5f,.09f),new Color(.30f,.52f,.43f),-18);
                Label("외부 출입문 →",new Vector2(x+4.8f,1.9f),.19f,new Color(.67f,.85f,.72f));
            }
        }

        private void Furnish(string name, Sprite sprite, Rect rect)
        {
            var renderer = NightRoundArt.Place(Root,name,sprite,rect,100-Mathf.RoundToInt(rect.yMin*10));
            var b = renderer.bounds;
            Walls.Add(new Rect(b.min.x,b.min.y,b.size.x,b.size.y));
        }

        public void RefreshStations(NightRoundState state, string focus = null)
        {
            connectionDoor.sprite = Art.Atlas("WardDoors",2,2)[state.finalPower == 1 ? 1 : 0];
            observationLamp.color = state.radioAnswered ? new Color(.3f,.95f,.63f) : InactiveColor;
            foreach (var entry in stationArt)
            {
                string id = entry.Key;
                bool done = state.IsActionComplete(id);
                bool pickup = id == "flashlight" || id == "bracelet" || id == "fuse";
                entry.Value.enabled = (!pickup || !done) && id != "reunite" && id != "exit";
                markers[id].enabled = !done;
                var label = stationLabels[id];
                if (label != null)
                {
                    label.gameObject.SetActive(!done);
                    label.color = id == focus ? Color.white : new Color(.68f,.76f,.74f);
                    label.text = stationTitles[id] + (id.StartsWith("circuit-") ? (state.IsCircuitPowered(id) ? "  ON" : "  OFF") : "");
                }
                if (switches.TryGetValue(id,out var breaker))
                {
                    bool on = id.StartsWith("circuit-") ? state.IsCircuitPowered(id) : state.finalPower == (id == "power-together" ? 1 : 2);
                    breaker.color = on ? new Color(.35f,.85f,.62f) : new Color(.65f,.29f,.19f);
                    var point = Stations[id];
                    breaker.transform.localPosition = new Vector3(point.x, point.y + (on ? .27f : .05f), 0);
                }
            }
            SetStationActive("flashlight", !state.flashlight);
            SetStationActive("bracelet", !state.bracelet);
            SetStationActive("manifest", !state.transferMismatch);
            SetStationActive("fuse", !state.fuse);
            SetStationActive("lock-record", !state.truth);
            SetStationActive("circuit-light", (state.circuits & 1) != 0);
            SetStationActive("circuit-supply", (state.circuits & 2) != 0);
            SetStationActive("circuit-door", (state.circuits & 4) != 0);
            SetStationActive("power-together", state.finalPower == 1);
            SetStationActive("power-alone", state.finalPower == 2);
            SetStationActive("manual", state.manualDoor);
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

        private TextMesh Label(string text, Vector2 position, float size, Color color)
        {
            if (font == null) return null;
            var go = new GameObject("Label / " + text); go.transform.SetParent(Root, false);
            go.transform.localPosition = position;
            var label = go.AddComponent<TextMesh>();
            label.text = text; label.font = font; label.fontSize = 64;
            label.characterSize = size * .32f; label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center; label.color = color;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material; renderer.sortingOrder = 5;
            return label;
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
            Art?.Dispose(); Art = null;
            Root = null; Pixel = null; texture = null; font = null;
            Walls.Clear(); Stations.Clear(); markers.Clear(); floors.Clear();
            stationArt.Clear(); stationLabels.Clear(); stationTitles.Clear(); switches.Clear();
            connectionDoor = observationLamp = null;
        }
    }
}
