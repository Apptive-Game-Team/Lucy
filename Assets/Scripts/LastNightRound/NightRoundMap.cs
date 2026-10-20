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
            // A central circulation spine with north/south departments.
            for (int i=0;i<5;i++)
            {
                offset=Centers[i]-new Vector2(i*18,0);
                float x=i*18;
                AddFloor(new Rect(x-8,-6,16,12),i==2 ? 2 : i==3 ? 1 : 0);
                if (i==0 || i==4)
                {
                    HorizontalWall(x-8,x+8,5.7f,i==4 ? x-2 : 1000);
                    Wall(new Rect(x-8,-6,16,.3f));
                    VerticalWall(x-8,-6,6,i==0 ? 100 : 0);
                    VerticalWall(x+7.7f,-6,6,0);
                }
                else
                {
                    VerticalWall(x-8,-6,6,100); VerticalWall(x+7.7f,-6,6,100);
                    HorizontalWall(x-8,x+8,-6,i==2 ? 1000 : x);
                    HorizontalWall(x-8,x+8,5.7f,i==2 ? x+5 : i==1 ? x+3 : 1000);
                }
                Furniture(i,x); DressRoom(i,x);
                if(i==0)
                {
                    HorizontalWall(-8,8,-1.7f,-5,5);
                    Wall(new Rect(1,-6,.25f,4.3f));
                }
                if(i==1)
                {
                    Wall(new Rect(19,-6,.25f,3.3f));
                    HorizontalWall(19,26,-1.7f,22);
                }
            }
            offset=Vector2.zero;
            AddFloor(new Rect(8,-2,38,4),1);
            HorizontalWall(8,46,2,18,36);
            HorizontalWall(8,46,-2.3f,23);
            Branch(18,2,6); Branch(36,2,6); Branch(23,-6,-2);
            AddFloor(new Rect(20,18,33,3),2);
            AddFloor(new Rect(51,6,2,12),2);
            Wall(new Rect(19.7f,18,.3f,3)); Wall(new Rect(20,21,33.3f,.3f));
            Wall(new Rect(53,6,.3f,15)); Wall(new Rect(50.7f,6,.3f,12));
            HorizontalWall(22,51,18,1000);
            Wall(new Rect(58.85f,-6,.3f,4.5f)); Wall(new Rect(58.85f,1.5f,.3f,4.5f));
            AddFloor(new Rect(62,-1.5f,5,3),0);
            Wall(new Rect(62,1.5f,5,.3f)); Wall(new Rect(62,-1.8f,5,.3f)); Wall(new Rect(67,-1.8f,.3f,3.6f));
            connectionDoor=Place("A / staff connection",Art.Doors[0],new Rect(51,5.1f,2,1.8f),40);
            Place("C / exterior doorway",Art.Doors[3],new Rect(61.7f,-1.5f,1.4f,3),120);
            Plaque("A",new Vector2(50.6f,5.25f)); Plaque("B",new Vector2(58.95f,2.1f)); Plaque("C",new Vector2(62,2.1f));
            Window(new Rect(39.8f,16.8f,2.4f,.85f));
            observationLamp=Box("Live response lamp",new Rect(42.35f,17.15f,.15f,.15f),InactiveColor,8);
            offset=Centers[0];
            Station("flashlight",-5,-3,""); Station("bracelet",-5,3,""); Station("meet",5,0,"");
            offset=Centers[1]-new Vector2(18,0);
            Station("manifest",15,3,""); Station("safe",22,-3,"");
            offset=Centers[2]-new Vector2(36,0);
            Station("circuit-light",31,3,""); Station("circuit-supply",34,3,""); Station("circuit-door",37,3,"");
            Station("fuse",40,-3,""); Station("service-exit",41,9,"");
            offset=Centers[3]-new Vector2(54,0);
            Station("lock-record",49,3,""); Station("recording",53,-4,""); Station("radio",54,3,""); Station("reunite",59,4,"");
            offset=Centers[4]-new Vector2(72,0);
            Station("power-together",68,3,""); Station("power-alone",72,3,""); Station("manual",75,-2,""); Station("exit",82,0,"");
            offset=Vector2.zero;
            MountedSign("병실  ←     간호실  ↑",new Vector2(12,1.5f));
            MountedSign("설비실  ↓",new Vector2(26,1.5f));
            Box("Service return intercom",new Rect(23.7f,-3.6f,.25f,.4f),InactiveColor,130);
            MountedSign("기록실  ↑     출구  →",new Vector2(41,1.5f));
        }

        public static readonly Vector2[] Centers={new Vector2(0,0),new Vector2(18,12),new Vector2(18,-12),new Vector2(36,12),new Vector2(54,0)};
        public static readonly Vector2[] GateCenters={new Vector2(8,0),new Vector2(23,-6),new Vector2(36,6),new Vector2(46,0),new Vector2(59,0)};
        public static readonly Rect[] GateBounds={new Rect(7.7f,-1.5f,.6f,3),new Rect(22,-6.3f,2,.6f),new Rect(35,5.7f,2,.6f),new Rect(45.7f,-1.5f,.6f,3),new Rect(58.7f,-1.5f,.6f,3)};
        private Vector2 offset;
        private Rect Shift(Rect r) { r.position+=offset; return r; }
        private SpriteRenderer Place(string name,Sprite sprite,Rect rect,int order)
        { return NightRoundArt.Place(Root,name,sprite,Shift(rect),order>=100 && order<500 ? order+2000 : order); }
        private void AddFloor(Rect rect,int style) { floors.Add(Shift(rect)); Floor(rect,style); }
        private void HorizontalWall(float left,float right,float y,params float[] doors)
        {
            float cursor=left;
            foreach(float door in doors)
            {
                if(door<left || door>right) continue;
                if(door-1>cursor) Wall(new Rect(cursor,y,door-1-cursor,.3f));
                cursor=door+1;
            }
            if(cursor<right) Wall(new Rect(cursor,y,right-cursor,.3f));
        }
        private void VerticalWall(float x,float bottom,float top,float door)
        {
            if(door<bottom || door>top) { Wall(new Rect(x,bottom,.3f,top-bottom)); return; }
            Wall(new Rect(x,bottom,.3f,door-1.5f-bottom)); Wall(new Rect(x,door+1.5f,.3f,top-door-1.5f));
        }
        private void Branch(float x,float bottom,float top)
        {
            AddFloor(new Rect(x-1,bottom,2,top-bottom),1);
            Wall(new Rect(x-1.3f,bottom,.3f,top-bottom)); Wall(new Rect(x+1,bottom,.3f,top-bottom));
        }
        private void MountedSign(string text,Vector2 point)
        {
            Box("Corridor sign backing",new Rect(point.x-2.6f,point.y-.25f,5.2f,.5f),new Color(.17f,.23f,.23f),130);
            var label=Label(text,point,.18f,new Color(.8f,.82f,.74f));
            if(label!=null) label.GetComponent<MeshRenderer>().sortingOrder=2131;
        }
        public static int RoomAt(Vector2 p)
        {
            for(int i=0;i<Centers.Length;i++) if(new Rect(Centers[i]-new Vector2(8,6),new Vector2(16,12)).Contains(p)) return i;
            return 5;
        }
        public static int DepthAt(Vector2 feet) => 1000-Mathf.RoundToInt(feet.y*10);
        public static bool StaffOnly(Vector2 p) => p.y>17.4f || (p.x>50.7f && p.x<53.3f && p.y>5.35f);

        private void Wall(Rect rect)
        {
            Walls.Add(Shift(rect));
            Box("Wall cap",rect,new Color(.22f,.24f,.26f),2);
            // A visible wall face gives notices and lights a physical mounting surface.
            if (rect.width>1 && rect.height<1)
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
            Place("Wall light",Art.Props[10],new Rect(x-3,5.2f,1.7f,.35f),3);
            if (room!=3) Place("Wall vent",Art.Props[9],new Rect(x+5,4.9f,1,.55f),3);
        }

        private void Station(string id, float x, float y, string title)
        {
            var position = new Vector2(x, y); Stations.Add(id, position+offset);
            Vector2 mounted = position;
            SpriteRenderer art = null;
            if (id=="manifest" || id=="lock-record")
            {
                mounted = new Vector2(x,5.12f);
                art = Place(id+" / wall notice",Art.Props[8],new Rect(x-.75f,4.65f,1.5f,.95f),5);
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
                art=Place("Wall speaker",Art.Props[9],new Rect(x-.5f,-5.5f,1,.5f),151);
            }
            else if (id=="manual")
            {
                mounted=new Vector2(76.85f,-2);
                art=Box("B / fixed handle",new Rect(76.65f,-2.35f,.18f,.7f),new Color(.65f,.62f,.51f),130);
            }
            if (id.StartsWith("circuit-") || id.StartsWith("power-"))
            {
                mounted=new Vector2(x,4.35f);
                art=Place(id+" / wall cabinet",Art.Props[2],new Rect(x-.55f,3.65f,1.1f,1.7f),20);
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
                Furnish("Archive aisle east",props[3],new Rect(x+2,-1,1.8f,1.5f));
                Furnish("Archive aisle west",props[3],new Rect(x-2,-1,1.8f,1.5f));
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
            var renderer = Place(name,sprite,rect,DepthAt(rect.position+offset));
            var b = renderer.bounds;
            Box(name+" / contact shadow",new Rect(b.min.x-offset.x+.06f,b.min.y-offset.y-.07f,Mathf.Max(.1f,b.size.x-.12f),.15f),new Color(.06f,.09f,.12f,.4f),-12);
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
                    breaker.transform.localPosition = new Vector3(point.x,point.y+1.3f + (on ? .10f : -.05f),0);
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
            go.transform.localPosition = rect.center+offset;
            go.transform.localScale = new Vector3(rect.width, rect.height, 1);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Pixel; renderer.color = color; renderer.sortingOrder = order>=100 ? order+2000 : order;
            return renderer;
        }

        private TextMesh Label(string text, Vector2 position, float size, Color color)
        {
            if (font == null) return null;
            var go = new GameObject("Label / " + text); go.transform.SetParent(Root, false);
            go.transform.localPosition = position+offset;
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
            if (label!=null) label.GetComponent<MeshRenderer>().sortingOrder=2131;
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

        public static bool Touches(Rect rect,Vector2 point,float radius=.25f)
        {
            return Vector2.Distance(point,new Vector2(Mathf.Clamp(point.x,rect.xMin,rect.xMax),Mathf.Clamp(point.y,rect.yMin,rect.yMax)))<radius;
        }

        // Half-unit navigation samples use the same solid furniture and walls as movement.
        // Four-way edges avoid cutting diagonally through door frames or desk corners.
        public List<Vector2> FindPath(Vector2 from,Vector2 to,Func<Vector2,bool> allowed)
        {
            var start=Vector2Int.RoundToInt(from*2); var end=Vector2Int.RoundToInt(to*2);
            var queue=new Queue<Vector2Int>(); var previous=new Dictionary<Vector2Int,Vector2Int>();
            queue.Enqueue(start); previous[start]=start;
            var directions=new[]{Vector2Int.right,Vector2Int.up,Vector2Int.left,Vector2Int.down};
            while(queue.Count>0)
            {
                var at=queue.Dequeue();
                if(at==end)
                {
                    var path=new List<Vector2>();
                    while(at!=start) { path.Add((Vector2)at*.5f); at=previous[at]; }
                    path.Reverse(); path.Add(to); return path;
                }
                foreach(var d in directions)
                {
                    var next=at+d; var point=(Vector2)next*.5f;
                    if(previous.ContainsKey(next) || point.x < -8 || point.x>67 || point.y < -18 || point.y>21 || !allowed(point)) continue;
                    previous[next]=at; queue.Enqueue(next);
                }
            }
            return new List<Vector2>();
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
