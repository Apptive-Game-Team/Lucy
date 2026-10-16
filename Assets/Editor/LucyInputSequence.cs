using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityPlayMcp;

namespace Lucy.Editor
{
    // Editor-only input playback: uses the same virtual keyboard as Unity Play MCP.
    // Never writes gameplay state, transforms, inventory, or progression.
    [InitializeOnLoad]
    public static class LucyInputSequence
    {
        private static readonly Queue<string> pending = new Queue<string>();
        private static double next;
        private const string Path = "Logs/lucy-input.txt";
        private static readonly MethodInfo clickKey = typeof(VirtualInput).GetMethod("ClickKey", BindingFlags.Static | BindingFlags.NonPublic);
        static LucyInputSequence() { EditorApplication.update += Tick; }
        private static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling)
            { pending.Clear(); return; }
            if (File.Exists(Path) && pending.Count == 0)
            {
                string[] lines;
                try { lines = File.ReadAllLines(Path); File.Delete(Path); }
                catch (IOException) { return; } // A writer may still be publishing the next sequence.
                foreach (var line in lines)
                    if (!string.IsNullOrWhiteSpace(line)) pending.Enqueue(line.Trim());
                next = EditorApplication.timeSinceStartup;
            }
            if (pending.Count == 0 || EditorApplication.timeSinceStartup < next) return;
            string input = pending.Dequeue();
            var parts = input.Split(' ');
            if (parts.Length != 2 || !float.TryParse(parts[1], NumberStyles.Float,
                CultureInfo.InvariantCulture, out float seconds) || seconds <= 0 || seconds > 30)
            { pending.Clear(); Debug.LogError("Invalid input sequence line: " + input); return; }
            if (parts[0] != "WAIT")
            {
                if (!Enum.TryParse(parts[0], out KeyCode key))
                { pending.Clear(); Debug.LogError("Invalid keyboard key: " + parts[0]); return; }
                if (clickKey == null) { pending.Clear(); Debug.LogError("MCP keyboard API unavailable"); return; }
                clickKey.Invoke(null, new object[] { key, seconds });
            }
            File.AppendAllText("Logs/LucyInputTrace.txt", DateTime.UtcNow.ToString("o") + " " + input + "\n");
            next = EditorApplication.timeSinceStartup + seconds + .06;
        }
    }
}
