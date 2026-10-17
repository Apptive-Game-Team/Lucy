using UnityEngine;
using Scripts_Creatures.Creatures;

namespace Lucy.Presentation
{
    /// <summary>Directional walk frames driven by actual movement, independent of AI colliders.</summary>
    public sealed class HospitalCreatureView : MonoBehaviour
    {
        private readonly Sprite[] frames = new Sprite[12];
        private Creature creature;
        private SpriteRenderer source, visual;
        private Transform art;
        private Vector3 lastPosition, restPosition;
        private float sampleTime, lastMotion = -10f, animationTime;
        private int row;
        public int FrameIndex { get; private set; }

        private void Start()
        {
            creature = GetComponent<Creature>();
            source = GetComponent<SpriteRenderer>();
            var atlas = Resources.Load<Texture2D>("Presentation/WardOrderlyWalk");
            if (source == null || atlas == null) { enabled = false; return; }
            var pixels = atlas.GetPixels32();
            var rects = new Rect[12];
            int maxHeight = 1;
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 4; c++)
                {
                    int x0 = c * atlas.width / 4, x1 = (c + 1) * atlas.width / 4;
                    int y0 = (2 - r) * atlas.height / 3, y1 = (3 - r) * atlas.height / 3;
                    int left = x1, right = x0, bottom = y1, top = y0;
                    for (int y = y0; y < y1; y++)
                        for (int x = x0; x < x1; x++)
                            if (pixels[y * atlas.width + x].a > 32)
                            { left = Mathf.Min(left,x); right = Mathf.Max(right,x); bottom = Mathf.Min(bottom,y); top = Mathf.Max(top,y); }
                    if (right < left || top < bottom) { enabled = false; return; }
                    rects[r * 4 + c] = new Rect(left,bottom,right-left+1,top-bottom+1);
                    maxHeight = Mathf.Max(maxHeight,top-bottom+1);
                }
            for (int i = 0; i < frames.Length; i++)
                frames[i] = Sprite.Create(atlas,rects[i],new Vector2(.5f,0),maxHeight / 1.7f,0,SpriteMeshType.FullRect);
            art = new GameObject("Animated ward orderly").transform;
            art.SetParent(transform,false);
            var scale = transform.lossyScale;
            art.localScale = new Vector3(1 / Mathf.Max(.001f,Mathf.Abs(scale.x)),1 / Mathf.Max(.001f,Mathf.Abs(scale.y)),1);
            restPosition = new Vector3(0,-.5f / Mathf.Max(.001f,Mathf.Abs(scale.y)),0);
            art.localPosition = restPosition;
            visual = art.gameObject.AddComponent<SpriteRenderer>();
            visual.sharedMaterial = source.sharedMaterial;
            visual.sortingLayerID = source.sortingLayerID;
            visual.sortingOrder = source.sortingOrder;
            visual.sprite = frames[0];
            source.enabled = false;
            lastPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (visual == null || Time.timeScale <= 0 || GameSession.BlocksGameplayInput) return;
            sampleTime += Time.deltaTime;
            if (sampleTime >= .08f)
            {
                var delta = transform.position - lastPosition;
                if (delta.sqrMagnitude > .0002f && delta.sqrMagnitude < 4f)
                {
                    lastMotion = Time.time;
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) { row = 1; visual.flipX = delta.x < 0; }
                    else { row = delta.y > 0 ? 2 : 0; visual.flipX = false; }
                }
                lastPosition = transform.position;
                sampleTime = 0;
            }
            bool moving = Time.time - lastMotion < .18f;
            bool chasing = creature != null && creature.IsChasingPlayer;
            animationTime = moving ? animationTime + Time.deltaTime * (chasing ? 11f : 7f) : 0;
            FrameIndex = row * 4 + (moving ? (int)animationTime % 4 : 1);
            visual.sprite = frames[FrameIndex];
            visual.color = chasing ? Color.white : new Color(.86f,.93f,.89f);
            art.localPosition = restPosition + (moving ? Vector3.zero : Vector3.up * (Mathf.Sin(Time.time * 2.2f) * .025f * art.localScale.y));
        }

        private void OnDestroy()
        {
            if (source != null) source.enabled = true;
            if (art != null) Destroy(art.gameObject);
            foreach (var frame in frames) if (frame != null) Destroy(frame);
        }
    }
}
