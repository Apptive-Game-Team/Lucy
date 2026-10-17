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
                " tutorial=" + Lucy.Presentation.HospitalTutorial.Step +
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
            string enemies = string.Join("\n",UnityEngine.Object.FindObjectsOfType<Scripts_Creatures.Creatures.Creature>().Select(c =>
            {
                var view = c.GetComponent<Lucy.Presentation.HospitalCreatureView>();
                return DateTime.UtcNow.ToString("o") + " scene=" + scene + " enemy=" + c.name +
                    " pos=" + c.transform.position.ToString("F2") + " status=" + c.CurrentStatus +
                    " grace=" + c.ArrivalGraceRemaining.ToString("F1") +
                    " frame=" + (view != null ? view.FrameIndex.ToString() : "none") +
                    " player=" + (player != null ? player.transform.position.ToString("F2") : "none");
            }));
            File.WriteAllText("Logs/LucyEnemyState.txt",enemies);
            string doors = string.Join("\n",UnityEngine.Object.FindObjectsOfType<Scripts_door.DoorObject>().Select(d =>
                DateTime.UtcNow.ToString("o") + " scene=" + scene + " pos=" + d.transform.position.ToString("F2") +
                " open=" + d.IsOpen + " label=" + d.InteractionLabel +
                " player=" + (player != null ? player.transform.position.ToString("F2") : "none")));
            File.WriteAllText("Logs/LucyDoorState.txt",doors);
            if (doors.Length > 0) File.AppendAllText("Logs/LucyDoorTrace.txt",doors + "\n");
            if (enemies.Length > 0) File.AppendAllText("Logs/LucyEnemyTrace.txt",enemies + "\n");
            if (previousScene != scene) Debug.Log("[Lucy Play Observation] " + line);
            previousScene = scene;
        }
    }
}
