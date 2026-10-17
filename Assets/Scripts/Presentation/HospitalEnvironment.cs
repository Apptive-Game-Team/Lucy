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
        private Sprite[] storage;
        private Material furnitureMaterial;
        private Texture2D floorTexture;
        private Texture2D wallTexture;

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
            // Unrendered space outside the authored hospital is darkness, not the editor camera's purple clear color.
            if (Camera.main != null) Camera.main.backgroundColor = new Color(.006f, .009f, .012f, 1);
            LoadFurniture();
            LoadStorage();
            LoadArchitecturalSurfaces();
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
                foreach (var creature in root.GetComponentsInChildren<Scripts_Creatures.Creatures.Creature>(true))
                    if (creature.GetComponent<HospitalCreatureView>() == null)
                        creature.gameObject.AddComponent<HospitalCreatureView>();
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
                    if (gameObject.scene.name == "Puzzle_2F" && map.name.Equals("Void", System.StringComparison.OrdinalIgnoreCase))
                    {
                        map.color = new Color(.08f,.10f,.12f,1f);
                        continue;
                    }
                    if (IsStaircase(map))
                    {
                        AddStairLighting(map);
                        continue;
                    }
                    if (IsFloor(map))
                    {
                        // Preserve authored tile proportions, seams, sorting and collider geometry.
                        ReplaceFloorSurface(map);
                        continue;
                    }
                    if (IsDesk(map.name) || (map.transform.parent != null && IsDesk(map.transform.parent.name)))
                        ReplaceDeskTiles(map);
                    else if (ReplaceStorageTiles(map)) { }
                    else if (IsWall(map))
                        ReplaceFloorSurface(map, true);
                }
                foreach (var source in root.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if (gameObject.scene.name == "0201" && source.name == "NPC")
                    {
                        // World art uses compact overhead proportions; dialogue keeps its illustration.
                        var nurse = Resources.Load<Sprite>("Presentation/NurseWorld");
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
                    if (!source.enabled || source.sprite == null) continue;
                    if (source.GetComponentInParent<invertoryAndItem.ItemObject>() != null ||
                        source.GetComponentInParent<Canvas>() != null || source.GetComponentInParent<Tilemap>() != null ||
                        source.name == "Hospital furniture visual") continue;
                    int storageIndex = StorageIndex(source.name + " " + source.sprite.name);
                    if (storageIndex >= 0 && storage != null && storage[storageIndex] != null)
                    {
                        var footprint = source.drawMode == SpriteDrawMode.Simple ? source.sprite.bounds.size : (Vector3)source.size;
                        FitFurniture(source.transform, source.sprite.bounds.center, footprint, Quaternion.identity,
                            Vector3.one, source.sortingLayerID, source.sortingOrder, storage[storageIndex]);
                        hidden.Add(source); source.enabled = false;
                        continue;
                    }
                    if (furniture == null) continue;
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
            if (gameObject.scene.name == GameSession.FirstScene)
            {
                if (furniture != null) AddPatientBed();
                AddFirstRoomGlow();
            }
        }

        private static bool IsStaircase(Tilemap map)
        {
            for (var t = map.transform; t != null; t = t.parent)
                if (t.name.ToLowerInvariant().Contains("stair")) return true;
            return false;
        }

        private void AddStairLighting(Tilemap map)
        {
            var prefab = Resources.Load<GameObject>("Presentation/FirstRoomGlow");
            if (prefab == null) return;
            // Use occupied artwork, not the tilemap's saved bounds (which include empty cells).
            foreach (var cell in map.cellBounds.allPositionsWithin)
            {
                var sprite = map.GetSprite(cell);
                if (sprite == null) continue;
                var local = map.CellToLocalInterpolated((Vector3)cell + map.tileAnchor)
                    + map.GetTransformMatrix(cell).MultiplyPoint3x4(sprite.bounds.center);
                var glow = Instantiate(prefab, transform, false);
                glow.name = "Stairwell emergency light";
                glow.transform.position = map.transform.TransformPoint(local);
                var light = glow.GetComponent<Light2D>();
                light.color = new Color(.72f, .86f, .82f);
                light.intensity = .48f;
                light.pointLightInnerRadius = 1.2f;
                light.pointLightOuterRadius = 3.2f;
                break;
            }
        }

        private void AddFirstRoomGlow()
        {
            // Serialized Default sorting-layer coverage also works in standalone builds.
            // Two small, feathered pools reveal the bedside and drawer without lighting the corridor.
            var prefab = Resources.Load<GameObject>("Presentation/FirstRoomGlow");
            if (prefab == null) return;
            var bedside = Instantiate(prefab, transform, false);
            bedside.name = "Bedside emergency glow";
            bedside.transform.localPosition = new Vector3(-1.5f, -2.8f, 0);
            var drawer = Instantiate(prefab, transform, false);
            drawer.name = "Drawer emergency glow";
            drawer.transform.localPosition = new Vector3(1.6f, -2.2f, 0);
            var light = drawer.GetComponent<Light2D>();
            light.intensity = .13f;
            light.pointLightOuterRadius = 2.3f;
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

        private void ReplaceFloorSurface(Tilemap map, bool wall = false)
        {
            var surface = wall ? wallTexture : floorTexture;
            if (surface == null) { map.color *= new Color(.75f,.77f,.78f); return; }
            var source = map.GetComponent<TilemapRenderer>();
            if (source == null || !source.enabled) return;
            var go = new GameObject(wall ? "Hospital plaster and skirting" : "Hospital ward floor", typeof(Tilemap), typeof(TilemapRenderer));
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
            renderer.mode = source.mode;
            renderer.sortOrder = source.sortOrder;
            var tiles = new Dictionary<Sprite,Tile>();
            foreach (var cell in map.cellBounds.allPositionsWithin)
            {
                var original = map.GetSprite(cell);
                if (original == null) continue;
                if (!tiles.TryGetValue(original, out var tile))
                {
                    // Keep doorway trims and irregular silhouettes rather than filling their empty pixels.
                    bool preserveTrim = wall && !IsRectangular(original);
                    var texture = wall && !preserveTrim ? RepeatWallPanel(surface, original.bounds.size) : surface;
                    var sprite = preserveTrim ? original : Sprite.Create(texture, new Rect(0,0,texture.width,texture.height),
                        original.pivot / original.rect.size, texture.width / original.bounds.size.x, 0, SpriteMeshType.FullRect);
                    tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sprite; tile.colliderType = Tile.ColliderType.None; tile.flags = TileFlags.None;
                    tiles.Add(original,tile); if (sprite != original) owned.Add(sprite); owned.Add(tile);
                }
                visual.SetTile(cell,tile);
                // Preserve non-square original footprints instead of stretching square tiles over walls.
                float correction = tile.sprite == original ? 1 : original.bounds.size.y / tile.sprite.bounds.size.y;
                var aspect = Matrix4x4.Scale(new Vector3(1,correction,1));
                visual.SetTransformMatrix(cell,map.GetTransformMatrix(cell)*aspect);
                visual.SetColor(cell,map.GetColor(cell));
            }
            hidden.Add(source); source.enabled = false; owned.Add(go);
        }

        private void LoadArchitecturalSurfaces()
        {
            floorTexture = Resources.Load<Texture2D>("Presentation/HospitalVinylV2");
            var atlas = Resources.Load<Texture2D>("Presentation/HospitalSurfaces");
            if (atlas == null || !atlas.isReadable) return;
            string scene = gameObject.scene.name;
            int floor = scene == "Maze_3F" || scene == "Puzzle_2F" ? 2 : scene == "0201" ? 1 : 0;
            floorTexture = SurfaceCell(atlas, floor);
            wallTexture = SurfaceCell(atlas, 3);
        }

        private Texture2D SurfaceCell(Texture2D atlas, int index)
        {
            int width = atlas.width / 2, height = atlas.height / 2;
            int left = index % 2 * width, bottom = (1-index/2)*height;
            if (index < 3)
            {
                // The atlas paints an 8x8 floor grid. A map cell gets one real-size slab, not 64 tiny ones.
                left += width * 3 / 8; bottom += height * 3 / 8;
                width /= 8; height /= 8;
            }
            var texture = new Texture2D(width,height,TextureFormat.RGBA32,false);
            texture.name = "Hospital surface " + index;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(atlas.GetPixels(left,bottom,width,height));
            texture.Apply(false,false);
            owned.Add(texture);
            return texture;
        }

        private Texture2D RepeatWallPanel(Texture2D panel, Vector3 footprint)
        {
            int repeats = Mathf.Clamp(Mathf.RoundToInt(footprint.x / Mathf.Max(.1f, footprint.y)),1,8);
            repeats = Mathf.Min(repeats, Mathf.Max(1,SystemInfo.maxTextureSize / panel.width));
            if (repeats == 1) return panel;
            var texture = new Texture2D(panel.width*repeats,panel.height,TextureFormat.RGBA32,false);
            texture.name = "Repeated hospital wall";
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            var pixels = panel.GetPixels();
            for (int i = 0; i < repeats; i++) texture.SetPixels(i*panel.width,0,panel.width,panel.height,pixels);
            texture.Apply(false,false);
            owned.Add(texture);
            return texture;
        }

        private static bool IsRectangular(Sprite sprite)
        {
            var vertices = sprite.vertices;
            if (vertices.Length != 4) return false;
            var bounds = sprite.bounds;
            foreach (var vertex in vertices)
                if (Mathf.Abs(Mathf.Abs(vertex.x - bounds.center.x) - bounds.extents.x) > .01f ||
                    Mathf.Abs(Mathf.Abs(vertex.y - bounds.center.y) - bounds.extents.y) > .01f) return false;
            return true;
        }

        private static bool IsWall(Tilemap map)
        {
            for (var t = map.transform; t != null; t = t.parent)
            {
                string name = t.name.ToLowerInvariant();
                if (name.Contains("door") || name.Contains("stair")) return false;
                if (name == "wall" || name == "mazewall" || name.EndsWith("wall_tilemap")) return true;
            }
            return false;
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

        private static int StorageIndex(string name)
        {
            string n = name.ToLowerInvariant();
            if (n.Contains("door") || n.Contains("hospital furniture visual")) return -1;
            if (n.Contains("drawer") || n.Contains("chest")) return 1;
            if (n.Contains("locker") || n.Contains("closet") || n.Contains("wardrobe")) return 2;
            if (n.Contains("bookcase") || n.Contains("bookshelf") || n.Contains("shelving") || n.Contains("shelf")) return 3;
            if (n.Contains("cabinet")) return 0;
            return -1;
        }

        private void LoadStorage()
        {
            var atlas = Resources.Load<Texture2D>("Presentation/HospitalStorage");
            if (atlas == null || !atlas.isReadable) return;
            storage = new Sprite[4];
            var pixels = atlas.GetPixels32();
            int width = atlas.width/2, height = atlas.height/2;
            for (int i = 0; i < storage.Length; i++)
            {
                int left = i%2*width, bottom = (1-i/2)*height;
                int minX = width, minY = height, maxX = -1, maxY = -1;
                for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    if (pixels[(bottom+y)*atlas.width+left+x].a < 26) continue;
                    minX = Mathf.Min(minX,x); minY = Mathf.Min(minY,y);
                    maxX = Mathf.Max(maxX,x); maxY = Mathf.Max(maxY,y);
                }
                if (maxX < minX || maxY < minY) continue;
                storage[i] = Sprite.Create(atlas,new Rect(left+minX,bottom+minY,maxX-minX+1,maxY-minY+1),
                    new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                owned.Add(storage[i]);
            }
        }

        private bool ReplaceStorageTiles(Tilemap map)
        {
            if (storage == null) return false;
            var renderer = map.GetComponent<TilemapRenderer>();
            if (renderer == null || !renderer.enabled) return false;
            int common = StorageIndex(map.name + " " + (map.transform.parent != null ? map.transform.parent.name : ""));
            bool found = false;
            foreach (var cell in map.cellBounds.allPositionsWithin)
            {
                var sprite = map.GetSprite(cell);
                if (sprite != null && (common >= 0 || StorageIndex(sprite.name) >= 0)) { found = true; break; }
            }
            if (!found) return false;
            foreach (var cell in map.cellBounds.allPositionsWithin)
            {
                var original = map.GetSprite(cell);
                if (original == null) continue;
                int index = StorageIndex(original.name);
                if (index < 0) index = common;
                var replacement = index >= 0 && storage[index] != null ? storage[index] : original;
                var matrix = map.GetTransformMatrix(cell);
                var center = map.CellToLocalInterpolated((Vector3)cell + map.tileAnchor) + matrix.MultiplyPoint3x4(original.bounds.center);
                // Mixed maps retain all unmatched art; only the identified storage pieces change.
                FitFurniture(map.transform,center,original.bounds.size,matrix.rotation,matrix.lossyScale,
                    renderer.sortingLayerID,renderer.sortingOrder,replacement);
            }
            hidden.Add(renderer); renderer.enabled = false;
            return true;
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
            Vector3 scale, int layer, int order, Sprite replacement = null)
        {
            // Use a correctly drawn horizontal counter or vertical bed, never rotate perspective art.
            var sprite = replacement != null ? replacement : furniture[size.x >= size.y ? 2 : 1];
            if (sprite == null) return;
            var go = new GameObject("Hospital furniture visual");
            go.transform.SetParent(parent, false);
            float fit = Mathf.Min(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y);
            go.transform.localPosition = center - rotation * Vector3.Scale(scale * fit, sprite.bounds.center);
            go.transform.localRotation = rotation;
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
