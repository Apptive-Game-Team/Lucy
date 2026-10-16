using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Lucy.Presentation;
using Portal;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lucy.Editor
{
    /// <summary>
    /// Editor-driven integration checks, independent of game objects destroyed during a new run.
    /// This exercises lifecycle and portal APIs; it does not solve puzzles or simulate player navigation.
    /// </summary>
    public static class LucyPlaySmoke
    {
        private static IEnumerator routine;
        private static readonly Stack<IEnumerator> stack = new Stack<IEnumerator>();
        private static readonly List<string> lines = new List<string>();
        private static readonly List<string> runtimeErrors = new List<string>();
        private static int pluginErrors;
        private static bool hadBestRecord;
        private static float previousBestRecord;
        private static double overallDeadline;
        private static string logPath;

        [MenuItem("Tools/Lucy/Run Play Smoke")]
        public static void Run()
        {
            if (routine != null) throw new InvalidOperationException("Lucy play smoke is already running.");
            if (!EditorApplication.isPlaying || GameSession.Instance == null)
                throw new InvalidOperationException("Enter Play Mode and wait for Lucy Session before running smoke tests.");
            lines.Clear(); runtimeErrors.Clear(); stack.Clear();
            pluginErrors = 0;
            hadBestRecord = PlayerPrefs.HasKey("Lucy.BestClearSeconds");
            previousBestRecord = PlayerPrefs.GetFloat("Lucy.BestClearSeconds", 0);
            logPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "LucyPlaySmoke.txt"));
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            Write("START " + DateTime.UtcNow.ToString("o"));
            Write("Integration simulation: calls lifecycle/portal APIs directly. Does not verify movement, puzzle solutions, collision-safe destinations, or visual quality.");
            overallDeadline = EditorApplication.timeSinceStartup + 240;
            routine = Execute(); stack.Push(routine);
            Application.logMessageReceived += CaptureLog;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            try
            {
                Require(EditorApplication.isPlaying, "Play Mode ended before smoke completion.");
                Require(EditorApplication.timeSinceStartup < overallDeadline, "Overall smoke deadline exceeded (240 seconds).");
                if (stack.Count == 0) { Finish(true, null); return; }
                IEnumerator current = stack.Peek();
                if (!current.MoveNext()) { stack.Pop(); return; }
                if (current.Current is IEnumerator nested) stack.Push(nested);
            }
            catch (Exception error) { Finish(false, error.GetBaseException().Message); }
        }

        private static IEnumerator Execute()
        {
            GameSession.Instance.StartNewGame();
            yield return WaitFor(() => SceneManager.GetActiveScene().name == GameSession.FirstScene && !GameSession.BlocksGameplayInput && CharacterStat.instance != null, "new game", 30);
            yield return Settle(.35);
            RequireStatsReset("new game");
            Write("PASS new game: first scene, gameplay unblocked, stats reset");

            // Deterministically simulate depleted mental health through the public failure API.
            CharacterStat.instance.curMental = 0;
            CharacterStat.instance.UpdateStats();
            GameSession.Instance.Fail("자동 검증: 정신력 고갈");
            yield return null;
            Require(GameSession.BlocksGameplayInput && Time.timeScale == 0, "Failure did not block and freeze gameplay.");
            Write("PASS simulated mental depletion: failure blocks gameplay");
            GameSession.Instance.StartNewGame();
            yield return WaitFor(() => SceneManager.GetActiveScene().name == GameSession.FirstScene && !GameSession.BlocksGameplayInput && CharacterStat.instance != null && CharacterStat.instance.curMental > 0, "restart after failure", 30);
            yield return Settle(.35);
            RequireStatsReset("restart after failure");
            Write("PASS failure restart: stats and input restored");

            var portals = AssetDatabase.LoadAssetAtPath<PortalDataList>("Assets/ScriptableObjects/Portal/PortalDataList.asset");
            var scenes = AssetDatabase.LoadAssetAtPath<SceneDataList>("Assets/ScriptableObjects/Portal/SceneDataList.asset");
            Require(portals != null && scenes != null, "Portal/scene mapping assets missing.");
            foreach (int value in new[] { 1, 2, 1, 3, 5, 7, 6, 8, 9, 10, 9, 11 })
            {
                PortalID id = (PortalID)value;
                var data = portals.portalDatas.Find(p => p != null && p.portalID == id);
                Require(data != null, "Missing portal mapping: " + id);
                var target = scenes.sceneDatas.Find(s => s != null && s.sceneID == data.sceneID);
                Require(target != null, "Missing target scene mapping: " + id);
                var manager = PortalManager.Instance;
                Require(manager != null && !manager.IsTransitioning, "Portal manager unavailable/busy before " + id);
                Require(!GameSession.BlocksGameplayInput, "Gameplay blocked before portal " + id);
                manager.StartCoroutine(manager.TransitScene(id));
                yield return WaitFor(() => SceneManager.GetActiveScene().name == target.sceneName && manager != null && !manager.IsTransitioning, "portal " + id, 20);
                Require(Character.Instance != null, "Persistent character missing after " + id);
                Require(Vector3.Distance(Character.Instance.transform.position, data.destination) < .75f, "Portal destination mismatch: " + id);
                Write("PASS portal " + id + " -> " + target.sceneName);
            }

            Require(SceneManager.GetActiveScene().name == "Ending" && GameSession.BlocksGameplayInput && Time.timeScale == 0, "Ending did not block and freeze gameplay.");
            Write("PASS ending: blocked and frozen");
            GameSession.Instance.StartNewGame();
            yield return WaitFor(() => SceneManager.GetActiveScene().name == GameSession.FirstScene && !GameSession.BlocksGameplayInput && CharacterStat.instance != null, "replay from ending", 30);
            yield return Settle(.35);
            RequireStatsReset("replay from ending");
            Write("PASS ending replay: clean stats and gameplay");
            GameSession.Instance.ReturnToTitle();
            yield return WaitFor(() => SceneManager.GetActiveScene().name == "Title" && GameSession.BlocksGameplayInput, "return to title", 30);
            yield return Settle(.25);
            Write("PASS return to title");
            Require(runtimeErrors.Count == 0, runtimeErrors.Count + " runtime error(s) captured; see log.");
        }

        private static IEnumerator WaitFor(Func<bool> ready, string step, double seconds)
        {
            double deadline = EditorApplication.timeSinceStartup + seconds;
            while (!ready())
            {
                Require(EditorApplication.timeSinceStartup < deadline, "Timed out: " + step);
                yield return null;
            }
        }

        private static IEnumerator Settle(double seconds)
        {
            double until = EditorApplication.timeSinceStartup + seconds;
            while (EditorApplication.timeSinceStartup < until) yield return null;
        }

        private static void RequireStatsReset(string step)
        {
            var stats = CharacterStat.instance;
            Require(stats != null && stats.maxMental > 0 && stats.maxStamina > 0, "Missing/invalid character stats: " + step);
            Require(Mathf.Approximately(stats.curMental, stats.maxMental) && Mathf.Approximately(stats.curStamina, stats.maxStamina), "Stats not reset: " + step);
            Require(!GameSession.BlocksGameplayInput && Time.timeScale > 0 && !AudioListener.pause, "Gameplay/audio remained paused: " + step);
        }

        private static void CaptureLog(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            string entry = "RUNTIME " + type + ": " + message + "\n" + trace;
            if (trace.IndexOf("UnityPlay", StringComparison.OrdinalIgnoreCase) >= 0 ||
                trace.IndexOf("UnityMcpBridge", StringComparison.OrdinalIgnoreCase) >= 0 ||
                trace.IndexOf("com.unity.mcp", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                pluginErrors++;
                Write("PLUGIN " + entry);
                return;
            }
            runtimeErrors.Add(entry);
            Write(entry);
        }

        private static void Write(string message)
        {
            lines.Add(message);
            File.WriteAllLines(logPath, lines);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void Finish(bool success, string error)
        {
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= CaptureLog;
            routine = null;
            while (stack.Count > 0) (stack.Pop() as IDisposable)?.Dispose();
            if (hadBestRecord) PlayerPrefs.SetFloat("Lucy.BestClearSeconds", previousBestRecord);
            else PlayerPrefs.DeleteKey("Lucy.BestClearSeconds");
            PlayerPrefs.Save();
            Write((success ? "PASS smoke completed" : "FAIL smoke: " + error) + "\nGame runtime errors: " + runtimeErrors.Count + "\nPlugin runtime errors: " + pluginErrors + "\nEND " + DateTime.UtcNow.ToString("o"));
            Debug.Log("Lucy play smoke " + (success ? "passed" : "failed") + ". See " + logPath);
        }
    }
}
