using System.Collections;
using System.IO;
using Lucy.Presentation;
using Portal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lucy.Editor
{
    public static class LucyMapPreview
    {
        public static void Run()
        {
            if (!UnityEditor.EditorApplication.isPlaying || GameSession.Instance == null)
                throw new System.InvalidOperationException("Enter Play before map preview.");
            GameSession.Instance.StartCoroutine(CaptureMaps());
        }

        private static IEnumerator CaptureMaps()
        {
            AudioListener.volume = 0;
            GameSession.Instance.StartNewGame();
            while (SceneManager.GetActiveScene().name != GameSession.FirstScene || GameSession.BlocksGameplayInput)
                yield return null;
            yield return new WaitForSecondsRealtime(2);
            yield return Capture();
            // Visual fixture equips a real inventory flashlight for later dark scenes.
            var torch = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObjects.ScriptableObject_items.Script.ItemData>("Assets/ScriptableObjects/ScriptableObject_items/Data/FlashLight.asset");
            if (torch != null)
            {
                var inventory = invertoryAndItem.Inventory.instance;
                inventory.AddItem(torch);
                inventory.SelectItem(0);
                inventory.OnEquipButton();
            }
            foreach (int id in new[] {1, 3, 5, 7, 9})
            {
                yield return PortalManager.Instance.TransitScene((PortalID)id);
                yield return new WaitForSecondsRealtime(2);
                yield return Capture();
            }
            File.WriteAllText("Logs/LucyMapPreview.txt", "Completed muted visual portal fixture at " + System.DateTime.UtcNow.ToString("o") + ". Does not solve puzzles or verify a manual full playthrough.");
        }

        private static IEnumerator Capture()
        {
            foreach (var guard in Object.FindObjectsOfType<Scripts_Creatures.Creatures.Guard>()) guard.StopPatrol();
            foreach (var canvas in Object.FindObjectsOfType<Canvas>())
                if (canvas.name.StartsWith("Unity Play MCP")) canvas.enabled = false;
            AudioListener.volume = 0;
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot("Logs/Map-" + SceneManager.GetActiveScene().name + ".png");
            yield return new WaitForSecondsRealtime(.3f);
        }
    }
}
