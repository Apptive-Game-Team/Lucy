using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Lucy.Presentation
{
    /// <summary>Hospital visuals fitted to authored furniture footprints and navigation.</summary>
    public sealed class HospitalEnvironment : MonoBehaviour
    {
        private readonly List<Object> owned = new List<Object>();
        private readonly List<Renderer> hidden = new List<Renderer>();
        private readonly Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
        private Sprite[] furniture;
        private Material furnitureMaterial;
        private Texture2D floorTexture;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            switch (scene.name)
            {
                case "0101": case "0101_09.18": case "0102": case "0201": case "Puzzle_2F": case "Maze_3F": break;
                default: return;
            }
            foreach (var root in scene.GetRootGameObjects())
                if (root.GetComponent<HospitalEnvironment>() != null) return;
            var go = new GameObject("Hospital Environment");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<HospitalEnvironment>();
        }

        private void Start()
        {
            LoadFurniture();
            floorTexture = Resources.Load<Texture2D>("Presentation/HospitalVinylV2");
            bool hasAmbient = false;
            foreach (var root in gameObject.scene.GetRootGameObjects())
            {
                // Several authored maps used unlit sprite materials, bypassing every 2D light.
                // World sprites must share the lit shader so darkness and the torch affect them.
                if (furnitureMaterial != null)
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                        if (renderer is SpriteRenderer || renderer is TilemapRenderer)
                        {
                            originalMaterials[renderer] = renderer.sharedMaterial;
                            renderer.sharedMaterial = furnitureMaterial;
                        }
                var shadow = Resources.Load<Sprite>("Presentation/WardShadow");
                if (shadow != null)
                    foreach (var creature in root.GetComponentsInChildren<Scripts_Creatures.Creatures.Creature>(true))
                    {
                        var body = creature.GetComponent<SpriteRenderer>();
                        if (body != null) body.sprite = shadow;
                    }
                foreach (var light in root.GetComponentsInChildren<Light2D>(true))
                {
                    if (light.lightType != Light2D.LightType.Global) continue;
                    hasAmbient |= light.isActiveAndEnabled;
                    // Only a faint silhouette outside local lights; never brighten authored darkness.
                    light.intensity = Mathf.Min(light.intensity, .015f);
                    light.color = new Color(.86f, .91f, 1f);
                }
                foreach (var map in root.GetComponentsInChildren<Tilemap>(true))
                {
                    if (IsFloor(map))
                    {
                        // Preserve authored tile proportions, seams, sorting and collider geometry.
                        ReplaceFloorSurface(map);
                        continue;
                    }
                    if (IsDesk(map.name) || (map.transform.parent != null && IsDesk(map.transform.parent.name)))
                        ReplaceDeskTiles(map);
                    else if (map.name.ToLowerInvariant().Contains("wall"))
                        map.color *= new Color(.72f,.75f,.77f);
                }
                foreach (var source in root.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if (gameObject.scene.name == "0201" && source.name == "NPC")
                    {
                        var nurse = Resources.Load<Sprite>("Presentation/Nurse");
                        if (nurse != null) source.sprite = nurse;
                    }
                    if (gameObject.scene.name == "Puzzle_2F")
                    {
                        if (source.GetComponentInParent<Puzzle.Bridge_Puzzle2F>() != null)
                            source.color = new Color(.43f,.52f,.54f);
                        else if (source.GetComponent<Puzzle.PuzzleSwitch2F>() != null)
                            source.color = new Color(.75f,.63f,.30f);
                        else if (source.name.StartsWith("Floor_") && source.sprite != null && floorTexture != null)
                            ReplacePlatform(source);
                    }
                    if (!source.enabled || source.sprite == null || furniture == null) continue;
                    if (!IsDesk(source.name) && !IsDesk(source.sprite.name)) continue;
                    var size = source.drawMode == SpriteDrawMode.Simple ? source.sprite.bounds.size : (Vector3)source.size;
                    FitFurniture(source.transform, source.sprite.bounds.center, size, Quaternion.identity,
                        Vector3.one, source.sortingLayerID, source.sortingOrder);
                    hidden.Add(source);
                    source.enabled = false;
                }
            }
            if (!hasAmbient)
            {
                // Serialized target sorting layers are required in standalone players; AddComponent
                // only initializes those automatically in the Unity editor.
                var ambient = Resources.Load<GameObject>("Presentation/WardAmbient");
                if (ambient != null) Instantiate(ambient, transform, false);
            }
            if (gameObject.scene.name == GameSession.FirstScene && furniture != null)
                AddPatientBed();
        }

        private void ReplacePlatform(SpriteRenderer source)
        {
            var original = source.sprite;
            var size = source.drawMode == SpriteDrawMode.Simple ? (Vector2)original.bounds.size : source.size;
            var sprite = Sprite.Create(floorTexture,new Rect(0,0,floorTexture.width,floorTexture.height),
                original.pivot / original.rect.size, floorTexture.width,0,SpriteMeshType.FullRect);
            owned.Add(sprite);
            source.sprite = sprite;
            source.drawMode = SpriteDrawMode.Tiled;
            source.tileMode = SpriteTileMode.Continuous;
            source.size = size;
            source.color = new Color(.85f,.9f,.92f);
        }

        private void AddPatientBed()
        {
            // Authored empty bedside area, away from the record, drawer and exit routes.
            var go = new GameObject("Patient bed");
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(-3.1f, -3.1f, 0);
            var sprite = furniture[1];
            if (sprite == null) { Destroy(go); return; }
            float scale = 1.65f / sprite.bounds.size.y;
            go.transform.localScale = Vector3.one * scale;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = furnitureMaterial;
            renderer.sortingOrder = 1;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = sprite.bounds.size * .84f;
        }

        private void ReplaceFloorSurface(Tilemap map)
        {
            if (floorTexture == null) { map.color *= new Color(.75f,.77f,.78f); return; }
            var source = map.GetComponent<TilemapRenderer>();
            if (source == null || !source.enabled) return;
            var go = new GameObject("Hospital vinyl surface", typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(map.transform, false);
            var visual = go.GetComponent<Tilemap>();
            visual.tileAnchor = map.tileAnchor;
            visual.orientation = map.orientation;
            visual.orientationMatrix = map.orientationMatrix;
            visual.color = map.color;
            var renderer = go.GetComponent<TilemapRenderer>();
            renderer.sharedMaterials = source.sharedMaterials;
            renderer.sortingLayerID = source.sortingLayerID;
            renderer.sortingOrder = source.sortingOrder;
            var tiles = new Dictionary<Sprite,Tile>();
            foreach (var cell in map.cellBounds.allPositionsWithin)
            {
                var original = map.GetSprite(cell);
                if (original == null) continue;
                if (!tiles.TryGetValue(original, out var tile))
                {
                    var sprite = Sprite.Create(floorTexture, new Rect(0,0,floorTexture.width,floorTexture.height),
                        original.pivot / original.rect.size, floorTexture.width / original.bounds.size.x, 0, SpriteMeshType.FullRect);
                    tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sprite; tile.colliderType = Tile.ColliderType.None; tile.flags = TileFlags.None;
                    tiles.Add(original,tile); owned.Add(sprite); owned.Add(tile);
                }
                visual.SetTile(cell,tile);
                // Preserve non-square original footprints instead of stretching square tiles over walls.
                var aspect = Matrix4x4.Scale(new Vector3(1,original.bounds.size.y/original.bounds.size.x,1));
                visual.SetTransformMatrix(cell,map.GetTransformMatrix(cell)*aspect);
                visual.SetColor(cell,map.GetColor(cell));
            }
            hidden.Add(source); source.enabled = false; owned.Add(go);
        }

        private static bool IsDesk(string name)
        {
            string n = name.ToLowerInvariant();
            return n.Contains("table") || n.Contains("desk");
        }

        private static bool IsFloor(Tilemap map)
        {
            for (var t = map.transform; t != null; t = t.parent)
            {
                string n = t.name.ToLowerInvariant();
                if (n.Contains("floor_tilemap") || n.Contains("mazefloor") || n == "floor") return true;
            }
            var tiles = new TileBase[map.GetUsedTilesCount()];
            map.GetUsedTilesNonAlloc(tiles);
            foreach (var tile in tiles)
                if (tile != null && tile.name.ToLowerInvariant().Contains("floor")) return true;
            return false;
        }

        private void LoadFurniture()
        {
            var atlas = Resources.Load<Texture2D>("Presentation/HospitalFurnitureV2");
            if (atlas == null) return;
            furniture = new Sprite[4];
            var pixels = atlas.GetPixels32();
            int width = atlas.width / 2, height = atlas.height / 2;
            for (int i = 0; i < 4; i++)
            {
                // Trim only transparent padding; preserve each generated object's aspect ratio.
                int left = (i % 2) * width, bottom = (1 - i / 2) * height;
                int xMin = width, yMin = height, xMax = 0, yMax = 0;
                for (int y = 0; y < height; y++)
                // Generated horizontal art reaches the cell edge; ignore that sliver in the next column.
                for (int x = (i % 2 == 1 ? width / 8 : 0); x < width; x++)
                {
                    if (pixels[(bottom+y)*atlas.width+left+x].a < 26) continue;
                    xMin = Mathf.Min(xMin, x); xMax = Mathf.Max(xMax, x);
                    yMin = Mathf.Min(yMin, y); yMax = Mathf.Max(yMax, y);
                }
                if (xMax < xMin || yMax < yMin) continue;
                furniture[i] = Sprite.Create(atlas, new Rect(left+xMin,bottom+yMin,xMax-xMin+1,yMax-yMin+1),
                    new Vector2(.5f,.5f), 100, 0, SpriteMeshType.FullRect);
                owned.Add(furniture[i]);
            }
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
            if (shader != null) { furnitureMaterial = new Material(shader); owned.Add(furnitureMaterial); }
        }

        private void ReplaceDeskTiles(Tilemap map)
        {
            if (furniture == null) return;
            var source = map.GetComponent<TilemapRenderer>();
            if (source == null || !source.enabled) return;
            foreach (var cell in map.cellBounds.allPositionsWithin)
            {
                var original = map.GetSprite(cell);
                if (original == null) continue;
                var matrix = map.GetTransformMatrix(cell);
                var center = map.CellToLocalInterpolated((Vector3)cell + map.tileAnchor) + matrix.MultiplyPoint3x4(original.bounds.center);
                FitFurniture(map.transform, center, original.bounds.size, matrix.rotation, matrix.lossyScale,
                    source.sortingLayerID, source.sortingOrder);
            }
            hidden.Add(source);
            source.enabled = false;
        }

        private void FitFurniture(Transform parent, Vector3 center, Vector3 size, Quaternion rotation,
            Vector3 scale, int layer, int order)
        {
            // Use a correctly drawn horizontal counter or vertical bed, never rotate perspective art.
            var sprite = furniture[size.x >= size.y ? 2 : 1];
            if (sprite == null) return;
            var go = new GameObject("Hospital furniture visual");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.transform.localRotation = rotation;
            float fit = Mathf.Min(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y);
            go.transform.localScale = scale * fit;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            if (furnitureMaterial != null) renderer.sharedMaterial = furnitureMaterial;
            renderer.sortingLayerID = layer;
            renderer.sortingOrder = order;
            owned.Add(go);
            // Authored colliders remain on the source; no pass-through decorative obstacles are added.
        }

        private void OnDestroy()
        {
            foreach (var entry in originalMaterials)
                if (entry.Key != null && entry.Key.sharedMaterial == furnitureMaterial)
                    entry.Key.sharedMaterial = entry.Value;
            foreach (var renderer in hidden) if (renderer != null) renderer.enabled = true;
            foreach (var asset in owned) if (asset != null) Destroy(asset);
        }
    }
}
