using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Lucy.LastNightRound
{
    /// <summary>In-world dawn sequence. Begins only after the exterior crossing is proven.</summary>
    public sealed class NightRoundEpilogue
    {
        private readonly NightRoundMap map;
        private readonly Transform lucy, seoyeon, owner;
        private readonly Camera camera;
        private readonly bool together;
        private Transform exterior;
        private float time;
        private int beat;
        public bool Complete => beat == 5;
        public bool Moving => beat == 1 || beat == 4;
        public int Beat => beat;
        public NightRoundEpilogue(NightRoundMap map, Transform owner, Transform lucy, Transform seoyeon, Camera camera, bool together)
        {
            this.map=map; this.owner=owner; this.lucy=lucy; this.seoyeon=seoyeon; this.camera=camera; this.together=together;
        }

        public void Tick(bool advance)
        {
            if (Complete) return;
            time += Time.deltaTime;
            if (beat == 0 && time >= .8f) { PrepareExterior(); Next(); }
            else if (beat == 1)
            {
                Walk(lucy,new Vector2(121,-.15f),2.1f);
                if (together) Walk(seoyeon,new Vector2(118.6f,.65f),1.5f);
                if (Vector2.Distance(lucy.position,new Vector2(121,-.15f)) < .05f) Next();
            }
            else if (beat == 2 || beat == 3)
            {
                if (time >= 4.5f || (advance && time >= 1.2f)) Next();
            }
            else if (beat == 4)
            {
                Walk(lucy,new Vector2(125,-.15f),1.4f);
                if (together) Walk(seoyeon,new Vector2(122.8f,.65f),1.4f);
                if (time >= 4.5f) Next();
            }
        }

        private void Next() { beat++; time=0; }
        private static void Walk(Transform actor, Vector2 to, float speed)
        { actor.position=Vector3.MoveTowards(actor.position,new Vector3(to.x,to.y,0),speed*Time.deltaTime); }

        private void PrepareExterior()
        {
            map.Root.gameObject.SetActive(false);
            exterior=new GameObject("Dawn / hospital exterior").transform;
            exterior.SetParent(owner,false); exterior.position=new Vector3(110,0,0);
            Patch("Dawn pavement",new Rect(-4,-8,42,16),new Color(.39f,.43f,.43f),-20);
            Patch("Hospital facade",new Rect(-4,-8,7,16),new Color(.23f,.29f,.29f),-18);
            Patch("Entrance path",new Rect(3,-2,18,4),new Color(.59f,.58f,.52f),-17);
            for (int i=0;i<12;i++)
                Patch("Paving joint",new Rect(3+i*1.5f,-2,.025f,4),new Color(.49f,.50f,.46f),-16);
            Patch("Paving joint",new Rect(3,0,18,.025f),new Color(.49f,.50f,.46f),-16);
            Patch("Entrance garden",new Rect(3,3,16,3),new Color(.32f,.39f,.33f),-17);
            Patch("Garden border",new Rect(3,2.85f,16,.15f),new Color(.65f,.64f,.55f),-16);
            var bench=map.Art.Props[7];
            NightRoundArt.Place(exterior,"Morning bench",bench,new Rect(7,3.4f,3.5f,1.5f),40);
            NightRoundArt.Place(exterior,"Morning bench",bench,new Rect(14,3.4f,3.5f,1.5f),40);
            Patch("Morning road",new Rect(21,-8,17,16),new Color(.25f,.29f,.32f),-17);
            Patch("Road curb",new Rect(20.7f,-8,.3f,16),new Color(.72f,.70f,.62f),-16);
            for (int i=-3;i<4;i++) Patch("Road marking",new Rect(29,i*3, .12f,1.5f),new Color(.72f,.70f,.58f),-16);
            for (int i=-3;i<4;i++)
            {
                if (i==0) continue;
                Patch("Facade window",new Rect(1,i*2,.8f,1),new Color(.34f,.45f,.47f),-15);
            }
            NightRoundArt.Place(exterior,"C / left open",map.Art.Doors[3],new Rect(2.2f,-1.65f,1.5f,3.3f),120);
            var plant=Resources.Load<Sprite>("Image_BSY/plantpot1");
            if (plant != null)
                for (int i=0;i<4;i++) NightRoundArt.Place(exterior,"Entrance planter",plant,new Rect(5+i*4,-4.2f,1.3f,1.5f),145);
            var glow=Object.Instantiate(Resources.Load<GameObject>("Presentation/FirstRoomGlow"),exterior).GetComponent<Light2D>();
            glow.transform.localPosition=new Vector3(12,0,0); glow.pointLightOuterRadius=30; glow.pointLightInnerRadius=15;
            glow.intensity=.85f; glow.color=new Color(1,.89f,.69f);
            lucy.position=new Vector3(114,-.15f,0);
            seoyeon.gameObject.SetActive(together);
            if (together) seoyeon.position=new Vector3(113,.65f,0);
            camera.transform.position=new Vector3(124,0,-10);
            camera.backgroundColor=new Color(.39f,.43f,.43f);
        }

        private void Patch(string name,Rect rect,Color color,int order)
        { NightRoundArt.Place(exterior,name,map.Pixel,rect,order,true).color=color; }

        public void Draw(GUIStyle text,GUIStyle heading)
        {
            if (beat==0)
            {
                GUI.color=new Color(0,0,0,Mathf.Clamp01(time/.7f));
                GUI.DrawTexture(new Rect(0,0,1600,900),Texture2D.whiteTexture); GUI.color=Color.white;
                return;
            }
            GUI.color=new Color(.02f,.04f,.05f,.9f);
            GUI.DrawTexture(new Rect(0,0,1600,105),Texture2D.whiteTexture); GUI.color=Color.white;
            GUI.Label(new Rect(60,28,1200,60),Complete ? (together ? "함께 밖으로" : "먼저 밖으로") : "새벽 / 병원 밖",heading);
            if (beat==2 || beat==3 || beat==4)
            {
                GUI.color=new Color(.025f,.04f,.045f,.96f); GUI.DrawTexture(new Rect(170,640,1260,185),Texture2D.whiteTexture); GUI.color=Color.white;
                string line=beat==2 ? (together ? "루시\n이번에는 문을 잠그지 마세요." : "무전기 — 잡음\n안에서는 아무 대답도 돌아오지 않는다.")
                    : beat==3 ? (together ? "서연\n네. 열어 둘게요." : "루시\n루시. 아직 여기 있어요.")
                    : together ? "루시가 먼저 걸어간다.\n뒤에 남은 문은 닫히지 않는다." : "루시는 팔찌를 확인하고 기록을 품에 넣는다.\n자신의 이름을 말한 뒤, 아침 도로를 향해 걸어간다.";
                GUI.Label(new Rect(220,670,1150,110),line,text);
                if (beat!=4) GUI.Label(new Rect(1120,782,260,35),"Space · 다음",text);
            }
            if (Complete)
            {
                GUI.color=new Color(.025f,.04f,.045f,.93f); GUI.DrawTexture(new Rect(170,590,1260,280),Texture2D.whiteTexture); GUI.color=Color.white;
                GUI.Label(new Rect(230,615,1120,70),"LUCY · 마지막 야간 순회\n끝까지 플레이해 주셔서 감사합니다.",text);
            }
        }

        public void Dispose()
        {
            if (exterior!=null) { exterior.gameObject.SetActive(false); Object.Destroy(exterior.gameObject); }
            if (map.Root!=null) map.Root.gameObject.SetActive(true);
            if (seoyeon!=null) seoyeon.gameObject.SetActive(true);
            if (camera!=null) camera.backgroundColor=new Color(.018f,.025f,.03f);
        }
    }
}
