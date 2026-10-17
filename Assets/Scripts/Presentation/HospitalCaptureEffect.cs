using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lucy.Presentation
{
    [DefaultExecutionOrder(10000)]
    public sealed class HospitalCaptureEffect : MonoBehaviour
    {
        private GameObject overlay, bodyObject;
        private SpriteRenderer original, body, attackerArt;
        private Sprite[] frames;
        private Camera cameraView;
        private Vector3 cameraPosition, attackerPosition, attackerScale;
        private float originalZoom, progress;
        private bool running, restored;
        private AudioSource impact, cry;
        private Material attackerMaterial, attackFlashMaterial;
        public int PhaseIndex { get; private set; }

        public IEnumerator Play(Transform attacker)
        {
            var player = Portal.Character.Instance;
            original = player != null ? player.GetComponentInChildren<SpriteRenderer>() : null;
            cameraView = Camera.main;
            if (cameraView != null) { originalZoom = cameraView.orthographicSize; cameraPosition = cameraView.transform.position; }
            if (attacker != null)
                foreach (var renderer in attacker.GetComponentsInChildren<SpriteRenderer>())
                    if (renderer.enabled && renderer.sprite != null) { attackerArt = renderer; break; }
            if (attackerArt != null)
            {
                attackerPosition = attackerArt.transform.localPosition;
                attackerScale = attackerArt.transform.localScale;
                attackerMaterial = attackerArt.sharedMaterial;
                attackFlashMaterial = new Material(Shader.Find("Sprites/Default"));
                attackerArt.sharedMaterial = attackFlashMaterial;
            }
            BuildBody();
            overlay = new GameObject("Capture impact",typeof(Canvas),typeof(CanvasScaler));
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 31000;
            var shadeObject = new GameObject("Impact and blackout",typeof(RectTransform),typeof(Image));
            shadeObject.transform.SetParent(overlay.transform,false);
            var rect = shadeObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var shade = shadeObject.GetComponent<Image>(); shade.raycastTarget = true;
            impact = PlaySound("Sound/Horror Elements/Hits/Hit_stress",.65f);
            cry = PlaySound("Sound/Horror Elements/Misc/Misc_Cry_solo",.65f);
            running = true;
            Debug.Log("[Lucy Capture] impact started; volume=" + AudioListener.volume);
            try
            {
                progress = 0;
                while (progress < 1.65f)
                {
                    // One dark-red impact pulse, then a steady fade; no repeating strobe.
                    if (progress < .25f) shade.color = new Color(.45f,.015f,.025f,.48f*(1-progress/.25f));
                    else shade.color = new Color(0,0,0,Mathf.Clamp01((progress-1.05f)/.6f));
                    if (body != null && frames != null)
                    {
                        int frame = progress < .25f ? 0 : progress < .53f ? 1 : progress < .85f ? 2 : 3;
                        PhaseIndex = frame;
                        body.sprite = frames[frame];
                    }
                    if (attackerArt != null)
                    {
                        float lunge = Mathf.Sin(Mathf.Clamp01(progress/.32f)*Mathf.PI);
                        attackerArt.transform.localScale = attackerScale*(1+.65f*lunge);
                        if (progress > .4f) attackerArt.sharedMaterial = attackerMaterial;
                        if (original != null)
                        {
                            Vector3 direction = (original.bounds.center-attackerArt.bounds.center).normalized;
                            var parent = attackerArt.transform.parent;
                            attackerArt.transform.localPosition = attackerPosition + (parent != null ? parent.InverseTransformVector(direction*.28f*lunge) : direction*.28f*lunge);
                        }
                    }
                    if (cry != null && progress > 1.05f) cry.volume = .65f*Mathf.Clamp01((1.5f-progress)/.45f);
                    yield return null;
                    // A slow frame must not skip the recoil and collapse poses entirely.
                    progress += Mathf.Min(Time.unscaledDeltaTime,.05f);
                }
                Debug.Log("[Lucy Capture] collapse and blackout complete");
            }
            finally { Restore(); }
        }

        private void BuildBody()
        {
            var atlas = Resources.Load<Texture2D>("Presentation/LucyDefeat");
            if (original == null || atlas == null || !atlas.isReadable) return;
            var pixels = atlas.GetPixels32();
            // Generated poses have unequal widths; cut at the inspected transparent gaps.
            float[] cuts = {0,.215f,.43f,.69f,1};
            var rects = new Rect[4];
            for (int i=0; i<4; i++)
            {
                int x0=Mathf.RoundToInt(cuts[i]*atlas.width), x1=Mathf.RoundToInt(cuts[i+1]*atlas.width);
                int left=x1,right=x0,bottom=atlas.height,top=0;
                for (int y=0; y<atlas.height; y++)
                    for (int x=x0; x<x1; x++)
                        if (pixels[y*atlas.width+x].a>32) { left=Mathf.Min(left,x); right=Mathf.Max(right,x); bottom=Mathf.Min(bottom,y); top=Mathf.Max(top,y); }
                rects[i]=new Rect(left,bottom,Mathf.Max(1,right-left+1),Mathf.Max(1,top-bottom+1));
            }
            float ppu = rects[0].height/Mathf.Max(.1f,original.bounds.size.y);
            frames = new Sprite[4];
            for (int i=0; i<4; i++) frames[i] = Sprite.Create(atlas,rects[i],new Vector2(.5f,0),ppu);
            bodyObject = new GameObject("Lucy collapse");
            bodyObject.transform.position = new Vector3(original.bounds.center.x,original.bounds.min.y,original.transform.position.z);
            body = bodyObject.AddComponent<SpriteRenderer>();
            body.sprite = frames[0]; body.sortingLayerID=original.sortingLayerID; body.sortingOrder=original.sortingOrder+1;
            // Keep the silhouette readable during the impact even outside the flashlight cone.
            body.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            body.flipX = attackerArt != null && attackerArt.bounds.center.x > original.bounds.center.x;
            original.enabled = false;
        }

        private AudioSource PlaySound(string path, float volume)
        {
            if (AudioListener.volume <= 0) return null;
            var clip = Resources.Load<AudioClip>(path);
            if (clip == null) return null;
            var source = gameObject.AddComponent<AudioSource>();
            source.clip=clip; source.volume=volume; source.ignoreListenerPause=true; source.spatialBlend=0; source.Play();
            return source;
        }

        private void LateUpdate()
        {
            if (!running || cameraView == null) return;
            float force = .085f*Mathf.Clamp01(1-progress/.5f);
            cameraView.transform.position = cameraPosition + new Vector3(Mathf.Sin(progress*83),Mathf.Cos(progress*67),0)*force;
            cameraView.orthographicSize = originalZoom*(1-.18f*Mathf.Clamp01(1-progress/.65f));
        }

        private void Restore()
        {
            if (restored) return;
            restored=true; running=false;
            if (cameraView != null) { cameraView.transform.position=cameraPosition; cameraView.orthographicSize=originalZoom; }
            if (original != null) original.enabled=true;
            if (attackerArt != null) { attackerArt.transform.localPosition=attackerPosition; attackerArt.transform.localScale=attackerScale; attackerArt.sharedMaterial=attackerMaterial; }
            if (attackFlashMaterial != null) Destroy(attackFlashMaterial);
            if (impact != null) { impact.Stop(); Destroy(impact); }
            if (cry != null) { cry.Stop(); Destroy(cry); }
            if (body != null) Destroy(body.sharedMaterial);
            if (bodyObject != null) Destroy(bodyObject);
            if (overlay != null) Destroy(overlay);
            if (frames != null) foreach (var frame in frames) if (frame != null) Destroy(frame);
        }
        private void OnDestroy() { Restore(); }
    }
}
