using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Portal;
using Scripts_Creatures;
using Scripts_Creatures.Util;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Lucy.Editor
{
    /// <summary>Read-only project checks; saved scenes are opened additively and closed without saving.</summary>
    public static class LucyValidation
    {
        [Serializable]
        private class Result
        {
            public string timestampUtc;
            public int passed;
            public int failed;
            public List<string> checks = new List<string>();
            public List<string> errors = new List<string>();
        }

        private static Result result;
        private static readonly string ProjectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        [MenuItem("Tools/Lucy/Validate Project")]
        public static void ValidateProject()
        {
            if (!RunChecks()) throw new BuildFailedException("Lucy validation failed. See Logs/LucyValidation.txt.");
        }

        [MenuItem("Tools/Lucy/Build Windows")]
        public static void BuildWindows()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new BuildFailedException("Exit Play Mode before building Lucy.");
            // Building must never silently save or discard an open scene.
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new BuildFailedException("Save your modified scenes before building Lucy.");
            if (!RunChecks()) throw new BuildFailedException("Fix validation errors before building Lucy.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                throw new BuildFailedException("Install Windows Build Support for this Unity editor in Unity Hub.");
            string output = Path.Combine(ProjectRoot, "Builds", "Windows", "Lucy.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            string summary = "Result: " + report.summary.result + "\nErrors: " + report.summary.totalErrors +
                             "\nWarnings: " + report.summary.totalWarnings + "\nBytes: " + report.summary.totalSize +
                             "\nDuration: " + report.summary.totalTime + "\nOutput: " + output;
            File.WriteAllText(Path.Combine(ProjectRoot, "Logs", "LucyBuild.txt"), summary);
            if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException(summary);
            Debug.Log(summary);
        }

        public static bool RunChecks()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before project validation.");
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save modified scenes before validation. Existing unsaved changes have been left untouched.");
            result = new Result { timestampUtc = DateTime.UtcNow.ToString("o") };
            Check("Pathfinder: equal-cost candidates remain distinct", () =>
            {
                var candidates = new SortedSet<Node> { new Node(1, 0, true), new Node(0, 1, true) };
                Require(candidates.Count == 2, "Equal F/H costs discarded a distinct coordinate.");
            });
            Check("Pathfinder: obstacle detour", () =>
            {
                int[,] map = OpenMap(5, 5);
                for (int y = 0; y < 4; y++) map[2, y] = 0;
                var finder = new PathFinder(map, Vector3Int.zero);
                AssertPath(finder.FindPath(new Node(0, 0, true), new Node(4, 0, true)), map, 0, 0, 4, 0, 12);
            });
            Check("Pathfinder: repeated forward and reverse searches", () =>
            {
                int[,] map = OpenMap(6, 4);
                var finder = new PathFinder(map, Vector3Int.zero);
                var start = new Node(0, 0, true);
                var end = new Node(5, 3, true);
                for (int i = 0; i < 20; i++)
                {
                    AssertPath(finder.FindPath(start, end), map, 0, 0, 5, 3, 8);
                    AssertPath(finder.FindPath(end, start), map, 5, 3, 0, 0, 8);
                }
            });
            Check("Pathfinder: map enlargement and shrink", () =>
            {
                var finder = new PathFinder(OpenMap(2, 2), Vector3Int.zero);
                int[,] large = OpenMap(7, 6);
                finder.SetMap(large);
                AssertPath(finder.FindPath(new Node(0, 0, true), new Node(6, 5, true)), large, 0, 0, 6, 5, 11);
                int[,] small = OpenMap(1, 2);
                finder.SetMap(small);
                AssertPath(finder.FindPath(new Node(0, 0, true), new Node(0, 1, true)), small, 0, 0, 0, 1, 1);
            });
            Check("Pathfinder: offset bounds and same-cell search", () =>
            {
                var finder = new PathFinder(OpenMap(3, 3), new Vector3Int(-4, 7, 0));
                var path = finder.FindPath(new Node(-4, 7, true), new Node(-2, 9, true));
                Require(path != null && path.Count == 4 && path.All(n => n.X >= -4 && n.X <= -2 && n.Y >= 7 && n.Y <= 9), "Offset path escaped bounds.");
                MethodInfo lookup = typeof(PathFinder).GetMethod("GetNode", BindingFlags.Instance | BindingFlags.NonPublic);
                Require(lookup != null, "GetNode lookup unavailable.");
                foreach (var point in new[] { new Vector2Int(-5, 7), new Vector2Int(-1, 7), new Vector2Int(-4, 6), new Vector2Int(-4, 10) })
                    Require(!((Node)lookup.Invoke(finder, new object[] { point.x, point.y })).IsWalkable, "Out-of-bounds cell is walkable: " + point);
                Require(finder.FindPath(new Node(-4, 7, true), new Node(-4, 7, true)).Count == 0, "Same-cell path should be empty.");
            });
            Check("Navigation: wall grid conversion and independent bounds", ValidateNavigationLayers);
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            Check("Build scenes: present, unique, title first", () =>
            {
                Require(scenes.Length > 0, "No enabled build scenes.");
                Require(Path.GetFileNameWithoutExtension(scenes[0]) == "Title", "Title must be the first enabled scene.");
                Require(scenes.Distinct().Count() == scenes.Length, "Duplicate build scene paths.");
                Require(scenes.Select(Path.GetFileNameWithoutExtension).Distinct().Count() == scenes.Length, "Duplicate scene names make portal loading ambiguous.");
                foreach (string path in scenes) Require(AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null, "Missing scene: " + path);
            });
            Check("Portal data: complete and loadable", () => ValidatePortalData(scenes));
            foreach (string path in scenes)
            {
                string scenePath = path;
                Check("Scene references: " + scenePath, () =>
                {
                    var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                    var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(scenePath);
                    bool alreadyLoaded = scene.IsValid() && scene.isLoaded;
                    try
                    {
                        if (!alreadyLoaded) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                        foreach (GameObject root in scene.GetRootGameObjects()) ValidateHierarchy(root, scenePath);
                    }
                    finally
                    {
                        if (!alreadyLoaded && scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
                        if (activeScene.IsValid() && activeScene.isLoaded) UnityEngine.SceneManagement.SceneManager.SetActiveScene(activeScene);
                    }
                });
            }
            foreach (string path in AssetDatabase.GetDependencies(scenes, true).Where(p => p.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)).Distinct())
            {
                string prefabPath = path;
                Check("Prefab scripts: " + prefabPath, () =>
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    Require(prefab != null, "Cannot load prefab.");
                    ValidateHierarchy(prefab, prefabPath);
                });
            }
            string logs = Path.Combine(ProjectRoot, "Logs");
            Directory.CreateDirectory(logs);
            File.WriteAllText(Path.Combine(logs, "LucyValidation.json"), JsonUtility.ToJson(result, true));
            File.WriteAllText(Path.Combine(logs, "LucyValidation.txt"), result.timestampUtc + "\nPassed: " + result.passed + " Failed: " + result.failed + "\n" + string.Join("\n", result.checks.Concat(result.errors)));
            Debug.Log("Lucy validation: " + result.passed + " passed, " + result.failed + " failed. Logs/LucyValidation.txt");
            return result.failed == 0;
        }

        private static void ValidatePortalData(string[] scenes)
        {
            var sceneLists = AssetDatabase.FindAssets("t:SceneDataList").Select(g => AssetDatabase.LoadAssetAtPath<SceneDataList>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
            var portalLists = AssetDatabase.FindAssets("t:PortalDataList").Select(g => AssetDatabase.LoadAssetAtPath<PortalDataList>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
            Require(sceneLists.Length > 0 && portalLists.Length > 0, "Missing scene/portal data assets.");
            var names = new HashSet<string>(scenes.Select(Path.GetFileNameWithoutExtension));
            foreach (var list in sceneLists)
            {
                Require(list.sceneDatas != null && list.sceneDatas.All(d => d != null), "Null scene data in " + list.name);
                Require(list.sceneDatas.Select(d => d.sceneID).Distinct().Count() == list.sceneDatas.Count, "Duplicate scene IDs in " + list.name);
                foreach (SceneID id in Enum.GetValues(typeof(SceneID)))
                    if (id != SceneID.NONE) Require(list.sceneDatas.Any(d => d.sceneID == id && names.Contains(d.sceneName)), "Missing/unbuilt scene mapping: " + id);
            }
            foreach (var list in portalLists)
            {
                Require(list.portalDatas != null && list.portalDatas.All(d => d != null), "Null portal data in " + list.name);
                Require(list.portalDatas.Select(d => d.portalID).Distinct().Count() == list.portalDatas.Count, "Duplicate portal IDs in " + list.name);
                foreach (PortalID id in Enum.GetValues(typeof(PortalID)))
                    if (id != PortalID.NONE) Require(list.portalDatas.Any(d => d.portalID == id), "Missing portal: " + id);
                foreach (var data in list.portalDatas)
                {
                    Require(data.sceneID != SceneID.NONE && sceneLists.All(l => l.sceneDatas.Any(s => s.sceneID == data.sceneID)), "Unknown portal destination: " + data.portalID);
                    Require(IsFinite(data.destination.x) && IsFinite(data.destination.y) && IsFinite(data.destination.z), "Invalid destination position: " + data.portalID);
                }
            }
        }

        private static void ValidateHierarchy(GameObject root, string assetPath)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0, "Missing script: " + assetPath + " / " + child.name);
                foreach (var portal in child.GetComponents<global::Portal.Portal>())
                {
                    var id = new SerializedObject(portal).FindProperty("portalID");
                    Require(id != null && id.intValue != 0 && Enum.IsDefined(typeof(PortalID), id.intValue), "Invalid portal ID: " + child.name);
                }
                foreach (var manager in child.GetComponents<PortalManager>())
                {
                    var serialized = new SerializedObject(manager);
                    Require(serialized.FindProperty("sceneDataList").objectReferenceValue != null && serialized.FindProperty("portalDataList").objectReferenceValue != null, "Portal manager data references missing: " + child.name);
                }
            }
        }

        private static bool IsFinite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }

        private static void ValidateNavigationLayers()
        {
            // Use a disposable additive scene so creating/destroying fixtures never dirties an authored scene.
            var original = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var scratch = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            GameObject floorRoot = null, wallRoot = null, managerObject = null;
            Tile tile = null;
            try
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(scratch);
                floorRoot = new GameObject("Lucy validation floor", typeof(Grid));
                wallRoot = new GameObject("Lucy validation wall", typeof(Grid));
                floorRoot.hideFlags = wallRoot.hideFlags = HideFlags.HideAndDontSave;
                floorRoot.GetComponent<Grid>().cellSize = Vector3.one;
                wallRoot.GetComponent<Grid>().cellSize = new Vector3(1, .5f, 1);
                var floorObject = new GameObject("Floor", typeof(Tilemap));
                floorObject.transform.SetParent(floorRoot.transform, false);
                var wallObject = new GameObject("Walls", typeof(Tilemap));
                wallObject.transform.SetParent(wallRoot.transform, false);
                var floor = floorObject.GetComponent<Tilemap>();
                var wall = wallObject.GetComponent<Tilemap>();
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.hideFlags = HideFlags.HideAndDontSave;
                for (int y = 0; y <= 5; y++) floor.SetTile(new Vector3Int(0, y, 0), tile);
                wall.SetTile(new Vector3Int(0, 2, 0), tile);
                wall.SetTile(new Vector3Int(0, 10, 0), tile);
                floor.CompressBounds(); wall.CompressBounds();
                // Keep only the manager inactive. Tilemap/Grid transforms need active objects
                // for native coordinate caches; the manager must not register global references.
                managerObject = new GameObject("Lucy validation manager");
                managerObject.hideFlags = HideFlags.HideAndDontSave;
                managerObject.SetActive(false);
                var manager = managerObject.AddComponent<CreatureManager>();
                var layers = (List<Tilemap>)typeof(CreatureManager).GetField("tilemaps", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager);
                layers.Add(floor); layers.Add(wall);
                typeof(CreatureManager).GetMethod("InitMap", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager, null);
                int[,] map = manager.GetMap();
                Vector3Int offset = manager.GetMapOffset();
                Require(map[-offset.x, 1 - offset.y] == 0, "Half-height wall cell y=2 must block floor cell y=1.");
                Require(map[-offset.x, 5 - offset.y] == 0, "Wall cell y=10 lies outside floor cell bounds but must block world/floor y=5.");
                foreach (int y in new[] { 0, 2, 3, 4 })
                    Require(map[-offset.x, y - offset.y] == 1, "Valid floor cell incorrectly blocked at y=" + y +
                        "; tile=" + floor.HasTile(new Vector3Int(0, y, 0)) + "; floorWorld=" + floor.CellToWorld(new Vector3Int(0, y, 0)) +
                        "; floorRoundTrip=" + floor.WorldToCell(floor.CellToWorld(new Vector3Int(0, y, 0))) +
                        "; wall2ToFloor=" + floor.WorldToCell(wall.CellToWorld(new Vector3Int(0, 2, 0))) +
                        "; wall10ToFloor=" + floor.WorldToCell(wall.CellToWorld(new Vector3Int(0, 10, 0))) + "; offset=" + offset);
                Require(map[1 - offset.x, 3 - offset.y] == 0, "Empty space outside floor must remain unwalkable.");
            }
            finally
            {
                if (floorRoot != null) UnityEngine.Object.DestroyImmediate(floorRoot);
                if (wallRoot != null) UnityEngine.Object.DestroyImmediate(wallRoot);
                if (managerObject != null) UnityEngine.Object.DestroyImmediate(managerObject);
                if (tile != null) UnityEngine.Object.DestroyImmediate(tile);
                EditorSceneManager.CloseScene(scratch, true);
                if (original.IsValid() && original.isLoaded) UnityEngine.SceneManagement.SceneManager.SetActiveScene(original);
            }
        }

        private static int[,] OpenMap(int width, int height)
        {
            var map = new int[width, height];
            for (int x = 0; x < width; x++) for (int y = 0; y < height; y++) map[x, y] = 1;
            return map;
        }

        private static void AssertPath(List<Node> path, int[,] map, int sx, int sy, int ex, int ey, int length)
        {
            Require(path != null && path.Count == length, "Unexpected path length; expected " + length + ".");
            int x = sx, y = sy;
            foreach (Node node in path)
            {
                Require(node.X >= 0 && node.Y >= 0 && node.X < map.GetLength(0) && node.Y < map.GetLength(1), "Path escaped map bounds.");
                Require(map[node.X, node.Y] == 1 && Math.Abs(node.X - x) + Math.Abs(node.Y - y) == 1, "Path crossed a wall or skipped a cell.");
                x = node.X; y = node.Y;
            }
            Require(x == ex && y == ey, "Path did not reach destination.");
        }

        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void Check(string name, Action check)
        {
            try { check(); result.passed++; result.checks.Add("PASS " + name); }
            catch (Exception error) { result.failed++; result.errors.Add("FAIL " + name + ": " + error.GetBaseException().Message); }
        }
    }
}
