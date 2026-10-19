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
            string key = name + columns + ":" + rows + ":" + trim;
            if (cache.TryGetValue(key, out var existing)) return existing;
            var texture = Resources.Load<Texture2D>("Presentation/" + name);
            if (texture == null || (trim && !texture.isReadable))
                throw new InvalidOperationException("Missing/read-protected Night Round art: " + name);
            var pixels = trim ? texture.GetPixels32() : null;
            var sprites = new Sprite[columns * rows];
            for (int i = 0; i < sprites.Length; i++)
            {
                int x0 = i % columns * texture.width / columns, x1 = (i % columns + 1) * texture.width / columns;
                int y0 = (rows - 1 - i / columns) * texture.height / rows, y1 = (rows - i / columns) * texture.height / rows;
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
