using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Lucy.LastNightRound;
using UnityEditor;
using UnityEngine;
using UnityPlayMcp;

namespace Lucy.Editor
{
    [InitializeOnLoad]
    public static class NightRoundPlaytest
    {
        private static readonly MethodInfo key = typeof(VirtualInput).GetMethod("ClickKey", BindingFlags.Static | BindingFlags.NonPublic);
        private static Queue<string> steps;
        private static Dictionary<string, Vector2> points;
        private static double next, deadline;
        private static string expected;
        static NightRoundPlaytest() { EditorApplication.update += Tick; }
        public static void Run(bool together)
        {
            if (NightRoundGame.Instance == null || NightRoundGame.Instance.State.metNurse) throw new Exception("Start a fresh story blockout first.");
            expected = together ? "together" : "alone";
            points = new Dictionary<string, Vector2> {
                {"bracelet",new Vector2(-5,3)}, {"flashlight",new Vector2(-5,-3)}, {"meet",new Vector2(5,0)},
                {"manifest",new Vector2(15,3)}, {"safe",new Vector2(22,-3)}, {"circuit-supply",new Vector2(34,3)},
                {"fuse",new Vector2(40,-3)}, {"circuit-light",new Vector2(31,3)}, {"circuit-door",new Vector2(37,3)},
                {"service-exit",new Vector2(42,0)}, {"lock-record",new Vector2(49,3)}, {"reunite",new Vector2(59,0)},
                {"power-together",new Vector2(68,3)}, {"power-alone",new Vector2(72,3)}, {"manual",new Vector2(75,-2)}, {"exit",new Vector2(79,0)} };
            var list = new List<string> { "bracelet","flashlight","meet","manifest","safe","circuit-supply","fuse","circuit-light","circuit-door","service-exit","lock-record","reunite",together?"power-together":"power-alone" };
            if (together) list.Add("manual"); list.Add("exit"); steps = new Queue<string>(list);
            next = EditorApplication.timeSinceStartup; deadline = next + 240;
            AudioListener.volume = 0;
            File.WriteAllText("Logs/NightRoundPlaytest-" + expected + ".txt", "START " + DateTime.UtcNow.ToString("o") + "\nVirtual keyboard only; no transform, inventory or state injection.\n");
        }
        private static void Press(KeyCode code, float seconds)
        { key.Invoke(null, new object[] { code, seconds }); next = EditorApplication.timeSinceStartup + seconds + .08; }
        private static void Write(string value) { File.AppendAllText("Logs/NightRoundPlaytest-" + expected + ".txt", value + "\n"); }
        private static void Tick()
        {
            if (steps == null || EditorApplication.timeSinceStartup < next) return;
            var game = NightRoundGame.Instance;
            if (!EditorApplication.isPlaying || game == null || EditorApplication.timeSinceStartup > deadline)
            { Write("FAIL stopped/missing game/timeout"); steps = null; return; }
            if (game.State.ending != "")
            { Write((game.State.ending == expected ? "PASS " : "FAIL ") + "actual ending=" + game.State.ending + " at " + DateTime.UtcNow.ToString("o")); ScreenCapture.CaptureScreenshot("Logs/NightRound-" + expected + ".png"); steps = null; return; }
            if (game.IsDead) { Write("FAIL caught at " + game.PlayerPosition); steps = null; return; }
            if (game.IsBlocked) { Press(KeyCode.Space, .12f); next += .3; return; }
            if (steps.Count == 0) { Press(KeyCode.E, .12f); return; }
            string id = steps.Peek(); Vector2 position = game.PlayerPosition, target = points[id];
            int fromRoom = Mathf.FloorToInt((position.x + 9) / 18), toRoom = Mathf.FloorToInt((target.x + 9) / 18);
            if (fromRoom != toRoom && Mathf.Abs(position.y) > .15f) target = new Vector2(position.x, 0);
            else if (fromRoom != toRoom) target = new Vector2(target.x, 0);
            var delta = target - position;
            if (Mathf.Abs(delta.x) > .2f)
            { Press(delta.x > 0 ? KeyCode.RightArrow : KeyCode.LeftArrow, Mathf.Clamp(Mathf.Abs(delta.x) / 3.4f, .03f, .18f)); return; }
            if (Mathf.Abs(delta.y) > .2f)
            { Press(delta.y > 0 ? KeyCode.UpArrow : KeyCode.DownArrow, Mathf.Clamp(Mathf.Abs(delta.y) / 3.4f, .03f, .18f)); return; }
            if (Vector2.Distance(position, points[id]) > .45f) return;
            Write("INPUT " + id + " at " + position);
            Press(KeyCode.E, id == "manual" ? 2.2f : .12f); steps.Dequeue();
        }
    }
}
