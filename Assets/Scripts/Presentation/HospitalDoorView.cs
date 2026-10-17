using UnityEngine;

namespace Lucy.Presentation
{
    public sealed class HospitalDoorView : MonoBehaviour
    {
        private SpriteRenderer closed, opened;
        private readonly Sprite[] sprites = new Sprite[2];

        public void Initialize(SpriteRenderer original, bool sideFacing)
        {
            var atlas = Resources.Load<Texture2D>("Presentation/WardDoors");
            if (atlas == null || original == null) return;
            var pixels = atlas.GetPixels32();
            int y0 = sideFacing ? 0 : atlas.height / 2;
            int y1 = sideFacing ? atlas.height / 2 : atlas.height;
            for (int i = 0; i < 2; i++)
            {
                int x0 = i * atlas.width / 2, x1 = (i+1) * atlas.width / 2;
                int left=x1, right=x0, bottom=y1, top=y0;
                for (int y=y0; y<y1; y++)
                    for (int x=x0; x<x1; x++)
                        if (pixels[y*atlas.width+x].a > 32)
                        { left=Mathf.Min(left,x); right=Mathf.Max(right,x); bottom=Mathf.Min(bottom,y); top=Mathf.Max(top,y); }
                if (right < left || top < bottom) return;
                var rect = new Rect(left,bottom,right-left+1,top-bottom+1);
                // Side opening stays anchored to the vertical jamb, not the swung leaf's centre.
                float pivotX = sideFacing && i == 1 ? 1f - .055f * rect.height / rect.width : .5f;
                sprites[i] = Sprite.Create(atlas,rect,new Vector2(Mathf.Clamp01(pivotX),.5f),rect.height / 2f,0,SpriteMeshType.FullRect);
            }
            closed = MakeRenderer("Closed ward door",sprites[0],original);
            opened = MakeRenderer("Open ward doorway",sprites[1],original);
            float height = original.sprite.bounds.size.y;
            float xScale = sideFacing ? 1f : original.sprite.bounds.size.x / sprites[0].bounds.size.x;
            transform.localScale = new Vector3(xScale,height/2f,1);
            original.enabled = false;
            SetOpenAmount(0);
        }

        private SpriteRenderer MakeRenderer(string name, Sprite sprite, SpriteRenderer original)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform,false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = original.sharedMaterial;
            renderer.sortingLayerID = original.sortingLayerID;
            renderer.sortingOrder = original.sortingOrder;
            return renderer;
        }

        public void SetOpenAmount(float amount)
        {
            if (closed == null || opened == null) return;
            closed.color = new Color(1,1,1,1-amount);
            opened.color = new Color(1,1,1,amount);
        }

        private void OnDestroy()
        {
            foreach (var sprite in sprites) if (sprite != null) Destroy(sprite);
        }
    }
}
