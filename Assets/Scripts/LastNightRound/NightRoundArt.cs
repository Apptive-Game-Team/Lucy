using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lucy.LastNightRound
{
    /// <summary>Owns runtime atlas slices; imported textures remain owned by Resources.</summary>
    public sealed class NightRoundArt : IDisposable
    {
        private readonly Dictionary<string, Sprite[]> cache = new Dictionary<string, Sprite[]>();
        private Material material;
        private Sprite[] doors;
        // Explicit separators follow the delivered sheets' transparent gutters, not assumed equal cells.
        public Sprite[] Props => Slice("NightRoundProps",new[]{0,370,650,975,1274},new[]{0,370,670,935,1235},true,
            new[]{new[]{0,367,675,950,1274},new[]{0,350,617,950,1274},new[]{0,350,615,956,1274},new[]{0,350,615,950,1274}});
        public Sprite[] Actors => Slice("NightRoundActors",new[]{0,270,510,770,1020,1260,1536},new[]{0,275,520,765,1024});
        public Sprite[] Doors => doors ?? (doors = new[] { Props[12],Props[13],Props[14],Props[15] });
        public Material LitMaterial
        {
            get
            {
                if (material == null) material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
                return material;
            }
        }

        public Sprite[] Atlas(string name, int columns = 1, int rows = 1, bool trim = true)
        {
            var texture = Resources.Load<Texture2D>("Presentation/" + name);
            if (texture==null) throw new InvalidOperationException("Missing Night Round art: "+name);
            var xCuts=new int[columns+1]; var yCuts=new int[rows+1];
            for (int i=0;i<=columns;i++) xCuts[i]=i*texture.width/columns;
            for (int i=0;i<=rows;i++) yCuts[i]=i*texture.height/rows;
            return Slice(name,xCuts,yCuts,trim);
        }

        private Sprite[] Slice(string name,int[] xCuts,int[] yCuts,bool trim=true,int[][] rowCuts=null)
        {
            string key=name+":"+string.Join(",",xCuts)+":"+string.Join(",",yCuts)+":"+trim;
            if (rowCuts!=null) foreach(var row in rowCuts) key+=":"+string.Join(",",row);
            if (cache.TryGetValue(key,out var existing)) return existing;
            var texture=Resources.Load<Texture2D>("Presentation/"+name);
            if (texture == null || (trim && !texture.isReadable))
                throw new InvalidOperationException("Missing/read-protected Night Round art: " + name);
            if (xCuts[xCuts.Length-1]!=texture.width || yCuts[yCuts.Length-1]!=texture.height)
                throw new InvalidOperationException("Art sheet dimensions changed; update slice metadata: "+name);
            int columns=xCuts.Length-1, rows=yCuts.Length-1;
            var pixels = trim ? texture.GetPixels32() : null;
            var sprites = new Sprite[columns * rows];
            for (int i = 0; i < sprites.Length; i++)
            {
                var cuts=rowCuts==null ? xCuts : rowCuts[i/columns];
                int x0=cuts[i%columns], x1=cuts[i%columns+1];
                int y0=texture.height-yCuts[i/columns+1], y1=texture.height-yCuts[i/columns];
                int left = x1, right = x0 - 1, bottom = y1, top = y0 - 1;
                if (trim)
                {
                    for (int y = y0; y < y1; y++) for (int x = x0; x < x1; x++)
                        if (pixels[y * texture.width + x].a > 32)
                        { left = Mathf.Min(left, x); right = Mathf.Max(right, x); bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y); }
                }
                else { left = x0; right = x1 - 1; bottom = y0; top = y1 - 1; }
                if (right < left || top < bottom) throw new InvalidOperationException("Empty art cell: " + name + "/" + i);
                sprites[i] = Sprite.Create(texture, new Rect(left, bottom, right-left+1, top-bottom+1), new Vector2(.5f,.5f), 100, 0, SpriteMeshType.FullRect);
                sprites[i].name = name + "/" + i;
            }
            cache.Add(key, sprites);
            return sprites;
        }

        public static SpriteRenderer Place(Transform parent, string name, Sprite sprite, Rect bounds, int order, bool stretch = false)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            go.transform.localPosition = bounds.center;
            float sx = bounds.width / sprite.bounds.size.x, sy = bounds.height / sprite.bounds.size.y;
            go.transform.localScale = stretch ? new Vector3(sx,sy,1) : Vector3.one * Mathf.Min(sx,sy);
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = order;
            return renderer;
        }

        public void Dispose()
        {
            foreach (var sprites in cache.Values) foreach (var sprite in sprites) Release(sprite);
            cache.Clear();
            doors = null;
            if (material != null) Release(material);
            material = null;
        }

        private static void Release(UnityEngine.Object value)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(value);
            else UnityEngine.Object.DestroyImmediate(value);
        }
    }

}
