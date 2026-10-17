using System.Collections;
using UnityEngine;

namespace Lucy.Presentation
{
    public sealed class NurseDeparture : MonoBehaviour
    {
        private readonly Sprite[] frames = new Sprite[8];
        private SpriteRenderer visual;
        private AudioSource footsteps;
        public int FrameIndex { get; private set; }
        public IEnumerator Run()
        {
            visual=GetComponent<SpriteRenderer>();
            var atlas=Resources.Load<Texture2D>("Presentation/NurseRun");
            if (visual!=null && atlas!=null && atlas.isReadable)
            {
                var pixels=atlas.GetPixels32();
                var rects=new Rect[8];
                int tallest=1;
                for (int row=0; row<2; row++)
                    for (int col=0; col<4; col++)
                    {
                        int x0=col*atlas.width/4,x1=(col+1)*atlas.width/4;
                        int y0=(1-row)*atlas.height/2,y1=(2-row)*atlas.height/2;
                        int left=x1,right=x0,bottom=y1,top=y0;
                        for (int y=y0; y<y1; y++) for (int x=x0; x<x1; x++)
                            if (pixels[y*atlas.width+x].a>32) { left=Mathf.Min(left,x); right=Mathf.Max(right,x); bottom=Mathf.Min(bottom,y); top=Mathf.Max(top,y); }
                        rects[row*4+col]=new Rect(left,bottom,Mathf.Max(1,right-left+1),Mathf.Max(1,top-bottom+1));
                        tallest=Mathf.Max(tallest,top-bottom+1);
                    }
                float ppu=tallest/Mathf.Max(.1f,visual.sprite.bounds.size.y);
                for (int i=0; i<8; i++) frames[i]=Sprite.Create(atlas,rects[i],new Vector2(.5f,0),ppu);
            }
            foreach (var hit in GetComponents<Collider2D>()) hit.enabled=false;
            footsteps=gameObject.AddComponent<AudioSource>();
            footsteps.playOnAwake=false; footsteps.spatialBlend=0;
            var step=Resources.Load<AudioClip>("Sound/Footsteps - Essentials/Footsteps_Tile/Footsteps_Tile_Run/Footsteps_Tile_Run_01");
            var points=new[]{new Vector3(-6.56f,1.5f),new Vector3(-2,1.5f),new Vector3(0,1.5f),new Vector3(0,14)};
            float animation=0,nextStep=0;
            Debug.Log("[Lucy Nurse] departure started");
            foreach (var point in points)
            {
                var destination=new Vector3(point.x,point.y,transform.position.z);
                while (Vector2.Distance(transform.position,destination)>.025f)
                {
                    if (Time.timeScale<=0 || GameSession.BlocksGameplayInput) { yield return null; continue; }
                    var direction=destination-transform.position;
                    transform.position=Vector3.MoveTowards(transform.position,destination,4.8f*Time.deltaTime);
                    animation+=Time.deltaTime;
                    FrameIndex=(Mathf.Abs(direction.y)>Mathf.Abs(direction.x)?4:0)+(int)(animation*12)%4;
                    if (visual!=null && frames[FrameIndex]!=null) { visual.sprite=frames[FrameIndex]; visual.flipX=direction.x<-.01f; }
                    if (animation>=nextStep)
                    {
                        nextStep=animation+.16f;
                        var player=Portal.Character.Instance;
                        float distance=player!=null?Vector2.Distance(transform.position,player.transform.position):0;
                        if (step!=null && AudioListener.volume>0) footsteps.PlayOneShot(step,.35f*Mathf.Clamp01(1-distance/14));
                    }
                    yield return null;
                }
            }
            Debug.Log("[Lucy Nurse] reached corridor exit at " + transform.position);
        }
        private void OnDestroy() { foreach (var frame in frames) if (frame!=null) Destroy(frame); }
    }
}
