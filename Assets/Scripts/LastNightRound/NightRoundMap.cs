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
            font = Resources.Load<Font>("Presentation/NotoSansKR-Regular");
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
            connectionDoor = NightRoundArt.Place(Root,"A / staff connection",Art.Doors[0],new Rect(69,5.1f,2,1.8f),40);
            NightRoundArt.Place(Root,"C / exterior doorway",Art.Doors[3],new Rect(79.7f,-1.5f,1.4f,3),120);
            Plaque("A",new Vector2(68.6f,5.25f));
            Plaque("B",new Vector2(76.95f,2.1f));
            Plaque("C",new Vector2(80,2.1f));
            Window(new Rect(57.8f,4.8f,2.4f,.85f));
            observationLamp = Box("Live response lamp",new Rect(60.35f,5.15f,.15f,.15f),InactiveColor,8);
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
            Station("recording",53,-4,"천장 방송");
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
            Box("Wall cap",rect,new Color(.22f,.24f,.26f),2);
            // A visible wall face gives notices and lights a physical mounting surface.
            if (rect.width>1 && rect.height<1 && rect.yMin>4)
            {
                Box("Wall plaster",new Rect(rect.xMin,rect.yMin-1.05f,rect.width,1.05f),new Color(.50f,.49f,.48f),-8);
                Box("Wall wainscot",new Rect(rect.xMin,rect.yMin-1.05f,rect.width,.42f),new Color(.30f,.36f,.35f),-7);
                Box("Wall skirting",new Rect(rect.xMin,rect.yMin-1.10f,rect.width,.07f),new Color(.17f,.21f,.23f),-6);
                Box("Wall contact shadow",new Rect(rect.xMin,rect.yMin-1.25f,rect.width,.15f),new Color(.10f,.14f,.17f,.30f),-10);
            }
        }

        private void Floor(Rect area, int cell)
        {
            Color tint = cell==2 ? new Color(.23f,.27f,.30f) : cell==1 ? new Color(.30f,.34f,.33f) : new Color(.38f,.39f,.38f);
            var grout=tint*.93f; grout.a=1;
            Box("Floor grout",area,grout,-22);
            for (float x = area.xMin; x < area.xMax; x += 1)
                for (float y = area.yMin; y < area.yMax; y += 1)
                {
                    float shade=((Mathf.FloorToInt(x)+Mathf.FloorToInt(y))&1)==0 ? 1 : .97f;
                    var color=tint*shade; color.a=1;
                    Box("Vinyl tile",new Rect(x+.015f,y+.015f,Mathf.Min(1,area.xMax-x)-.03f,Mathf.Min(1,area.yMax-y)-.03f),color,-20);
                }
        }

        private void Furniture(int room, float x)
        {
            string[] types = { "Patient bed", "Nurses desk", "Electrical cabinet", "Archive shelves", "Waiting bench" };
            var rect = new Rect(x - 1.7f, room<2 ? 1.1f : -5.1f,3.4f,1.3f);
            Sprite sprite = Art.Props[room==4 ? 7 : room];
            if (room!=3) Furnish(types[room],sprite,rect);
            NightRoundArt.Place(Root,"Wall light",Art.Props[10],new Rect(x-3,5.2f,1.7f,.35f),3);
            if (room!=3) NightRoundArt.Place(Root,"Wall vent",Art.Props[9],new Rect(x+5,4.9f,1,.55f),3);
        }

        private void Station(string id, float x, float y, string title)
        {
            var position = new Vector2(x, y); Stations.Add(id, position);
            Vector2 mounted = position;
            SpriteRenderer art = null;
            if (id=="manifest" || id=="lock-record")
            {
                mounted = new Vector2(x,5.12f);
                art = NightRoundArt.Place(Root,id+" / wall notice",Art.Props[8],new Rect(x-.75f,4.65f,1.5f,.95f),5);
                Furnish(id+" / records desk",Art.Props[1],new Rect(x-1.25f,3.6f,2.5f,1));
            }
            else if (id=="bracelet" || id=="flashlight" || id=="radio" || id=="fuse")
            {
                float deskY=y>0 ? 3.65f : id=="flashlight" ? -4.35f : -4.6f;
                Furnish(id+" / bedside cabinet",Art.Props[id=="fuse" ? 5 : 1],new Rect(x-.85f,deskY,1.7f,id=="flashlight" ? .85f : 1));
                mounted=new Vector2(x,deskY+.75f);
                art=Box(id+" / item",new Rect(mounted.x-.17f,mounted.y-.09f,.34f,.18f),id=="bracelet" ? new Color(.83f,.81f,.72f) : new Color(.24f,.28f,.32f),150);
            }
            else if (id=="recording")
            {
                mounted=new Vector2(x,-5.25f);
                Box("South wall speaker mounting",new Rect(x-.7f,-5.65f,1.4f,.8f),new Color(.30f,.36f,.35f),150);
                art=NightRoundArt.Place(Root,"Wall speaker",Art.Props[9],new Rect(x-.5f,-5.5f,1,.5f),151);
            }
            else if (id=="manual")
            {
                mounted=new Vector2(76.85f,-2);
                art=Box("B / fixed handle",new Rect(76.65f,-2.35f,.18f,.7f),new Color(.65f,.62f,.51f),130);
            }
            if (id.StartsWith("circuit-") || id.StartsWith("power-"))
            {
                mounted=new Vector2(x,4.35f);
                art=NightRoundArt.Place(Root,id+" / wall cabinet",Art.Props[2],new Rect(x-.55f,3.65f,1.1f,1.7f),20);
                Box(id+" / breaker face",new Rect(x-.3f,4,.6f,.65f),new Color(.12f,.17f,.18f),21);
                switches.Add(id,Box(id+" / breaker",new Rect(x-.10f,4.2f,.20f,.20f),InactiveColor,22));
            }
            // The world has no floating object names. A small glint appears only on the focused prop.
            if (art!=null) stationArt.Add(id,art);
            markers.Add(id,Box(id+" / focus glint",new Rect(mounted.x-.06f,mounted.y+.18f,.12f,.12f),ActiveColor,180));
        }

        private void DressRoom(int room, float x)
        {
            var props = Art.Props;
            // All new solid furniture lives outside the investigation rows at y=+/-3
            // and the east-west escape lane. The visible footprint is the collider.
            if (room == 0)
            {
                Window(new Rect(x+1.5f,4.8f,2.7f,.85f));
                Box("Treatment bay floor",new Rect(x-2.1f,.8f,5,2),new Color(.33f,.37f,.36f),-18);
                Furnish("Treatment privacy screen",props[6],new Rect(x+1.9f,1.1f,1,1.4f));
                Furnish("Bed 101",props[0],new Rect(x-6.7f,-5.55f,3.1f,1.1f));
                Furnish("Bed 103",props[0],new Rect(x+3.3f,-5.55f,3.1f,1.1f));
                Furnish("Privacy screen",props[6],new Rect(x+5.1f,3.9f,1.7f,1.5f));
                Furnish("Medicine trolley",props[5],new Rect(x-1.1f,3.9f,1.7f,1.5f));
            }
            else if (room == 1)
            {
                Box("Nursing work area",new Rect(x-2.2f,.8f,4.5f,2),new Color(.27f,.33f,.32f),-18);
                Furnish("Desk supplies trolley",props[5],new Rect(x+1.7f,1.1f,.8f,1.3f));
                Furnish("Medicine cupboard",props[11],new Rect(x-.8f,3.8f,1.3f,1.8f));
                Furnish("Staff bench",props[7],new Rect(x+3.7f,-5.2f,2.7f,1.2f));
                Furnish("Records trolley",props[5],new Rect(x-6.2f,-5.1f,1.6f,1.4f));
                Box("Safe room mat",new Rect(x+2.6f,-3.7f,2.8f,1.3f),new Color(.18f,.29f,.27f),-18);
            }
            else if (room == 2)
            {
                Furnish("Service tool trolley",props[5],new Rect(x+2,1.1f,1.4f,1.3f));
                Box("Cable trunk",new Rect(x+5,1.3f,.08f,3.3f),new Color(.15f,.19f,.21f),-17);
                for (int n=0;n<3;n++) Furnish("Service locker",props[2],new Rect(x-6.4f+n*1.3f,-5.2f,1,1.7f));
                Box("Breaker safety stripe",new Rect(x-6,2.25f,8,.08f),new Color(.57f,.46f,.20f),-18);
            }
            else if (room == 3)
            {
                Box("Records work area",new Rect(x-4.3f,.9f,2.4f,1.8f),new Color(.27f,.31f,.30f),-18);
                Furnish("Records reading desk",props[1],new Rect(x-4,1.2f,1.6f,1.1f));
                Furnish("Records archive",props[3],new Rect(x+1.8f,3.9f,1.8f,1.5f));
                Furnish("Sealed files",props[3],new Rect(x-6,-5.2f,1.8f,1.5f));
                Furnish("Wheelchair",props[4],new Rect(x+4.4f,-5.2f,1.5f,1.5f));
            }
            else
            {
                Furnish("Waiting alcove",props[7],new Rect(x-7,1.2f,2.5f,1.2f));
                Furnish("Waiting chairs",props[7],new Rect(x-6.6f,-5.1f,3,1.3f));
                Box("Exit guide",new Rect(x+1,-.65f,5.5f,.09f),new Color(.30f,.52f,.43f),-18);
            }
        }

        private void Furnish(string name, Sprite sprite, Rect rect)
        {
            var renderer = NightRoundArt.Place(Root,name,sprite,rect,100-Mathf.RoundToInt(rect.yMin*10));
            var b = renderer.bounds;
            Box(name+" / contact shadow",new Rect(b.min.x+.06f,b.min.y-.07f,Mathf.Max(.1f,b.size.x-.12f),.15f),new Color(.06f,.09f,.12f,.4f),-12);
            Walls.Add(new Rect(b.min.x,b.min.y,b.size.x,b.size.y));
        }

        public void RefreshStations(NightRoundState state, string focus = null)
        {
            connectionDoor.sprite = Art.Doors[state.finalPower == 1 ? 1 : 0];
            observationLamp.color = state.radioAnswered ? new Color(.3f,.95f,.63f) : InactiveColor;
            foreach (var entry in stationArt)
            {
                string id = entry.Key;
                bool done = state.IsActionComplete(id);
                bool pickup = id == "flashlight" || id == "bracelet" || id == "fuse";
                entry.Value.enabled = !pickup || !done;
                if (switches.TryGetValue(id,out var breaker))
                {
                    bool on = id.StartsWith("circuit-") ? state.IsCircuitPowered(id) : state.finalPower == (id == "power-together" ? 1 : 2);
                    breaker.color = on ? new Color(.35f,.85f,.62f) : new Color(.65f,.29f,.19f);
                    var point = Stations[id];
                    breaker.transform.localPosition = new Vector3(point.x,4.3f + (on ? .10f : -.05f),0);
                }
            }
            foreach (var marker in markers) marker.Value.enabled = marker.Key==focus && !state.IsActionComplete(marker.Key);
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

        private void Plaque(string text,Vector2 at)
        {
            Box("Mounted door plate / "+text,new Rect(at.x-.18f,at.y-.17f,.36f,.34f),new Color(.17f,.23f,.23f),130);
            var label=Label(text,at,.20f,new Color(.79f,.80f,.71f));
            if (label!=null) label.GetComponent<MeshRenderer>().sortingOrder=131;
        }

        private void Window(Rect rect)
        {
            Box("Recessed window frame",rect,new Color(.16f,.21f,.24f),4);
            Box("Window glass",new Rect(rect.x+.08f,rect.y+.08f,rect.width-.16f,rect.height-.16f),new Color(.27f,.38f,.41f),5);
            Box("Window mullion",new Rect(rect.center.x-.035f,rect.y,.07f,rect.height),new Color(.16f,.21f,.24f),6);
            Box("Window sill",new Rect(rect.x-.08f,rect.y-.08f,rect.width+.16f,.1f),new Color(.54f,.56f,.53f),7);
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
            stationArt.Clear(); switches.Clear();
            connectionDoor = observationLamp = null;
        }
    }
}
