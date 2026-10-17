using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Lucy.Presentation
{
    /// <summary>Small wall-mounted landmarks; never creates navigation obstacles or raises ambient light.</summary>
    [DefaultExecutionOrder(200)]
    public sealed class HospitalWardDetails : MonoBehaviour
    {
        private readonly List<Object> owned = new List<Object>();
        private readonly List<Tilemap> walls = new List<Tilemap>();
        private readonly List<Vector2> occupied = new List<Vector2>();
        private readonly List<Vector2> portals = new List<Vector2>();
        private readonly Sprite[] sprites = new Sprite[8];
        private Material lit;
        private TMP_FontAsset font;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneLoaded += SceneLoaded;
        }

        private static void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            switch (scene.name)
            {
                case GameSession.FirstScene: case "0102": case "0201": case "Puzzle_2F": case "Maze_3F": break;
                default: return;
            }
            foreach (var root in scene.GetRootGameObjects())
                if (root.GetComponent<HospitalWardDetails>() != null) return;
            var host = new GameObject("Hospital Ward Details");
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<HospitalWardDetails>();
        }

        private IEnumerator Start()
        {
            // Environment.Start finishes its material replacements before these renderers exist.
            yield return null;
            var sourceFont = Resources.Load<Font>("Presentation/LucyFont");
            if (sourceFont != null)
            {
                font = TMP_FontAsset.CreateFontAsset(sourceFont);
                owned.Add(font);
                if (font.material != null) owned.Add(font.material);
                foreach (var texture in font.atlasTextures) if (texture != null) owned.Add(texture);
            }
            LoadAtlas();
            foreach (var root in gameObject.scene.GetRootGameObjects())
            {
                foreach (var map in root.GetComponentsInChildren<Tilemap>())
                    for (var node = map.transform; node != null; node = node.parent)
                        if (node.name == "Wall" || node.name == "MazeWall" || node.name == "Wall_tilemap")
                        { walls.Add(map); break; }
                foreach (var portal in root.GetComponentsInChildren<global::Portal.Portal>())
                    portals.Add(portal.transform.position);
            }
            switch (gameObject.scene.name)
            {
                case GameSession.FirstScene:
                    DressBed();
                    // Authored north wall, clear of the central door and the two storage banks.
                    AddLandmark(new Vector3(-1.6f, .65f, 0), "입원 병동 · 101", 4, 1.0f, true, 0, 4);
                    AddLandmark(new Vector3(1.35f, .65f, 0), "", 5, .55f, false, 0, 4);
                    break;
                case "0102":
                    Landmark(new Vector2(-2.48f, -3.04f), "1F  병동 / 2F  간호실", 6, .85f, true);
                    Landmark(new Vector2(3.53f, 2.07f), "", 5, .55f, false);
                    break;
                case "0201":
                    Landmark(new Vector2(-7, 14.5f), "2F · 간호 병동", 4, 1.05f, true);
                    Landmark(new Vector2(3.01f, 7.15f), "연결 병동 · 설비실", 6, .8f, true);
                    Landmark(new Vector2(10.02f, 14.63f), "비상 통로", 5, .55f, false);
                    break;
                case "Puzzle_2F":
                    MarkServicePlatforms();
                    // This scene has sprite platforms, not wall tilemaps. Mount at the edge
                    // of an existing platform, never on a narrow bridge or a switch.
                    PlatformLandmark(new Vector2(-6.1f, .4f), "치료동 · 비상 전력");
                    PlatformLandmark(new Vector2(29, 11), "전력 점검실");
                    break;
                case "Maze_3F":
                    Landmark(new Vector2(-7, -4), "3F · 폐쇄 병동", 4, .85f, true);
                    Landmark(new Vector2(29.98f, -3.75f), "대피 출구", 6, .8f, true);
                    break;
            }
        }

        private void LoadAtlas()
        {
            var atlas = Resources.Load<Texture2D>("Presentation/HospitalWardProps");
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
            if (shader != null) { lit = new Material(shader); owned.Add(lit); }
            if (atlas == null) return;
            if (!atlas.isReadable)
            {
                Debug.LogWarning("HospitalWardProps must have Read/Write enabled for alpha-trimmed cells.");
                return;
            }
            Color32[] pixels = atlas.GetPixels32();
            // Top row: wheelchair, IV, curtain, trolley. Bottom: noticeboard, vent, fixture, bench.
            for (int i = 0; i < 8; i++)
            {
                int col = i % 4, row = 1 - i / 4;
                int left = col * atlas.width / 4, right = (col + 1) * atlas.width / 4;
                int bottom = row * atlas.height / 2, top = (row + 1) * atlas.height / 2;
                int minX = right, maxX = left - 1, minY = top, maxY = bottom - 1;
                for (int y = bottom; y < top; y++)
                    for (int x = left; x < right; x++)
                    {
                        if (pixels[y * atlas.width + x].a <= 32) continue;
                        minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                        minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                    }
                if (maxX < minX || maxY < minY) continue;
                sprites[i] = Sprite.Create(atlas, new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1),
                    new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
                owned.Add(sprites[i]);
            }
        }

        private void MarkServicePlatforms()
        {
            var pixel = new Texture2D(1, 1); pixel.SetPixel(0, 0, Color.white); pixel.Apply(); owned.Add(pixel);
            var line = Sprite.Create(pixel, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1); owned.Add(line);
            foreach (var root in gameObject.scene.GetRootGameObjects())
                foreach (var floor in root.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (!floor.name.StartsWith("Floor_") || floor.sprite == null) continue;
                    var b = floor.bounds;
                    for (int side = 0; side < 4; side++)
                    {
                        var edge = new GameObject("Service platform safety edging"); edge.transform.SetParent(transform, false);
                        edge.transform.position = new Vector3(side < 2 ? b.center.x : side == 2 ? b.min.x : b.max.x,
                            side < 2 ? side == 0 ? b.min.y : b.max.y : b.center.y, b.center.z);
                        edge.transform.localScale = side < 2 ? new Vector3(b.size.x, .025f, 1) : new Vector3(.025f, b.size.y, 1);
                        var renderer = edge.AddComponent<SpriteRenderer>(); renderer.sprite = line;
                        renderer.color = new Color(.12f, .15f, .14f); renderer.sortingLayerID = floor.sortingLayerID;
                        renderer.sortingOrder = floor.sortingOrder + 1;
                    }
                    var prefab = Resources.Load<GameObject>("Presentation/FirstRoomGlow");
                    if (prefab == null) continue;
                    var pool = Instantiate(prefab, transform, false); pool.name = "Service platform emergency light";
                    pool.transform.position = b.center;
                    var light = pool.GetComponent<Light2D>(); light.intensity = .16f;
                    light.color = new Color(.72f, .78f, .66f); light.pointLightInnerRadius = .6f;
                    light.pointLightOuterRadius = Mathf.Min(3, Mathf.Max(b.extents.x, b.extents.y));
                }
        }

        private void DressBed()
        {
            // Accessories stay within the existing bed collision footprint.
            foreach (var root in gameObject.scene.GetRootGameObjects())
                foreach (var bed in root.GetComponentsInChildren<BoxCollider2D>())
                {
                    if (bed.name != "Patient bed") continue;
                    var bounds = bed.bounds;
                    // A thin bedside stand sits at the head-side margin, never over the mattress.
                    BedAccessory(bounds, 1, "Bedside IV stand", .28f, .12f, .65f, .90f, 2);
                    return;
                }
        }

        private void BedAccessory(Bounds bounds, int index, string label, float x, float y,
            float width, float height, int order)
        {
            var sprite = sprites[index];
            if (sprite == null || lit == null) return;
            var go = new GameObject(label); go.transform.SetParent(transform, false);
            float scale = Mathf.Min(bounds.size.x * width / sprite.bounds.size.x,
                bounds.size.y * height / sprite.bounds.size.y);
            var size = sprite.bounds.size * scale;
            var position = bounds.center + new Vector3(bounds.size.x * x, bounds.size.y * y, 0);
            position.x = Mathf.Clamp(position.x, bounds.min.x + size.x * .5f, bounds.max.x - size.x * .5f);
            position.y = Mathf.Clamp(position.y, bounds.min.y + size.y * .5f, bounds.max.y - size.y * .5f);
            if (index == 1) position.x = bounds.max.x + size.x * .5f + .04f;
            go.transform.position = position; go.transform.localScale = Vector3.one * scale;
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite;
            renderer.sharedMaterial = lit; renderer.sortingOrder = order;
        }

        private bool Reserved(Vector2 position)
        {
            foreach (var portal in portals) if (Vector2.Distance(portal, position) < 1.25f) return true;
            foreach (var prior in occupied) if (Vector2.Distance(prior, position) < 1.4f) return true;
            return false;
        }

        private void Landmark(Vector2 desired, string title, int spriteIndex, float width, bool glow)
        {
            float nearest = 3.5f * 3.5f;
            Vector3 point = default;
            Tilemap selected = null;
            foreach (var wall in walls)
                foreach (var cell in wall.cellBounds.allPositionsWithin)
                {
                    var sprite = wall.GetSprite(cell);
                    if (sprite == null) continue;
                    var matrix = wall.transform.localToWorldMatrix
                        * Matrix4x4.Translate(wall.CellToLocalInterpolated((Vector3)cell + wall.tileAnchor))
                        * wall.GetTransformMatrix(cell);
                    var b = sprite.bounds;
                    Vector3 a = matrix.MultiplyPoint3x4(b.min), z = matrix.MultiplyPoint3x4(b.max);
                    var min = Vector3.Min(a, z); var max = Vector3.Max(a, z);
                    bool strip = max.x - min.x >= 1.4f && max.y - min.y <= 2.5f
                        && max.x - min.x >= (max.y - min.y) * 2;
                    bool joined = wall.HasTile(cell + Vector3Int.left) && wall.HasTile(cell + Vector3Int.right)
                        && max.x - min.x <= 1.5f && max.y - min.y <= 2.5f;
                    if (!strip && !joined) continue;
                    Vector3 candidate = (min + max) * .5f;
                    if (strip) candidate.x = Mathf.Clamp(desired.x, min.x + .65f, max.x - .65f);
                    float distance = ((Vector2)candidate - desired).sqrMagnitude;
                    if (distance >= nearest || Reserved(candidate)) continue;
                    selected = wall; point = candidate; nearest = distance;
                }
            if (selected == null) return; // Missing safe wall is preferable to decoration over an aisle.
            var source = selected.GetComponent<TilemapRenderer>();
            AddLandmark(point, title, spriteIndex, width, glow,
                source != null ? source.sortingLayerID : 0, source != null ? source.sortingOrder + 1 : 2);
        }

        private void PlatformLandmark(Vector2 desired, string title)
        {
            SpriteRenderer selected = null;
            foreach (var root in gameObject.scene.GetRootGameObjects())
                foreach (var sprite in root.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (!sprite.name.StartsWith("Floor_") || !sprite.enabled || sprite.sprite == null) continue;
                    var b = sprite.bounds;
                    if (b.size.x < 3 || b.size.y < 2 || !b.Contains(new Vector3(desired.x, desired.y, b.center.z))) continue;
                    selected = sprite; break;
                }
            if (selected == null) return;
            var bounds = selected.bounds;
            var point = new Vector3(bounds.center.x, bounds.max.y - .25f, bounds.center.z);
            if (Reserved(point)) return;
            AddLandmark(point, title, 6, .75f, true, selected.sortingLayerID, selected.sortingOrder + 1);
        }

        private void AddLandmark(Vector3 point, string title, int spriteIndex, float width, bool glow, int layer, int order)
        {
            occupied.Add(point);
            var host = new GameObject(string.IsNullOrEmpty(title) ? "Ward wall detail" : title);
            host.transform.SetParent(transform, false); host.transform.position = point;
            var sprite = sprites[spriteIndex];
            if (sprite != null && lit != null)
            {
                var art = new GameObject("Wall mounted artwork"); art.transform.SetParent(host.transform, false);
                var renderer = art.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sharedMaterial = lit;
                renderer.sortingLayerID = layer; renderer.sortingOrder = order;
                float scale = Mathf.Min(width / sprite.bounds.size.x, .55f / sprite.bounds.size.y);
                art.transform.localScale = Vector3.one * scale;
            }
            if (!string.IsNullOrEmpty(title) && font != null)
            {
                var go = new GameObject("Room identification"); go.transform.SetParent(host.transform, false);
                go.transform.localPosition = new Vector3(0, .38f, 0);
                var text = go.AddComponent<TextMeshPro>(); text.font = font; text.text = title;
                text.fontSize = 2.0f; text.alignment = TextAlignmentOptions.Center;
                text.rectTransform.sizeDelta = new Vector2(2.4f, .42f);
                text.enableWordWrapping = false; text.overflowMode = TextOverflowModes.Truncate;
                text.color = new Color(.64f, .73f, .70f); text.raycastTarget = false;
                text.GetComponent<MeshRenderer>().sortingLayerID = layer;
                text.GetComponent<MeshRenderer>().sortingOrder = order + 1;
            }
            if (!glow) return;
            var prefab = Resources.Load<GameObject>("Presentation/FirstRoomGlow");
            if (prefab == null) return;
            var pool = Instantiate(prefab, host.transform, false);
            var light = pool.GetComponent<Light2D>();
            if (light == null) return;
            light.color = new Color(.65f, .79f, .72f); light.intensity = .12f;
            light.pointLightInnerRadius = .2f; light.pointLightOuterRadius = 1.35f;
        }

        private void OnDestroy()
        {
            foreach (var asset in owned) if (asset != null) Destroy(asset);
        }
    }
}
