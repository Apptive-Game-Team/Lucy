using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lucy.Editor
{
    // Read-only telemetry for actual input playtests. Never changes game state.
    [InitializeOnLoad]
    public static class LucyPlayObserver
    {
        private static double next;
        private static string previousScene;
        static LucyPlayObserver() { EditorApplication.update += Observe; }
        private static void Observe()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.timeSinceStartup < next) return;
            next = EditorApplication.timeSinceStartup + 1;
            var player = UnityEngine.Object.FindObjectOfType<Lucy.CharacterMove>();
            var stats = UnityEngine.Object.FindObjectOfType<CharacterStat>();
            string scene = SceneManager.GetActiveScene().name;
            string line = DateTime.UtcNow.ToString("o") + " scene=" + scene + " scale=" + Time.timeScale +
                " pos=" + (player != null ? player.transform.position.ToString("F2") : "none") +
                " mental=" + (stats != null ? stats.curMental.ToString("F0") : "none") +
                " battery=" + (invertoryAndItem.FlashLight.instance != null ? invertoryAndItem.FlashLight.instance.battery.ToString() : "none") +
                " records=" + Lucy.Presentation.HospitalStory.FoundCount +
                " move=" + (InputSystem.InputManager.Instance != null ? InputSystem.InputManager.Instance.GetKeyActive(InputSystem.ActionCode.MoveLeft).ToString() : "none") +
                " speed=" + (player != null ? player.playerMoveSpeed.ToString() : "none") +
                " inv=" + (invertoryAndItem.Inventory.instance != null ? invertoryAndItem.Inventory.instance.inventoryWindow.activeSelf.ToString() : "none") +
                " bridges=" + string.Join(",",UnityEngine.Object.FindObjectsOfType<Puzzle.Bridge_Puzzle2F>().Select(b=>b.bridgeID.ToString()).Distinct()) +
                " furniture=" + UnityEngine.Object.FindObjectsOfType<SpriteRenderer>().Count(r=>r.name=="Ward furniture");
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/LucyObservedState.txt", line);
            File.AppendAllText("Logs/LucyActualPlayTrace.txt", line + "\n");
            if (previousScene != scene) Debug.Log("[Lucy Play Observation] " + line);
            previousScene = scene;
        }
    }
}
