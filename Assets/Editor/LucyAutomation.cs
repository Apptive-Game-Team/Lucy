using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Lucy.Editor
{
    /// <summary>Local, fixed-command validation entrypoint for an already open editor.</summary>
    [InitializeOnLoad]
    public static class LucyAutomation
    {
        private const string Command = "Logs/lucy-command.txt";
        static LucyAutomation() { EditorApplication.update += Poll; }
        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Command)) return;
            string command;
            try { command = File.ReadAllText(Command).Trim(); File.Delete(Command); }
            catch (IOException) { return; } // The command writer may still be publishing the file.
            try
            {
                switch (command)
                {
                    case "refresh": AssetDatabase.Refresh(); break;
                    case "icons": ApplyIcons(); break;
                    case "preview-items":
                        if (!EditorApplication.isPlaying || invertoryAndItem.Inventory.instance == null)
                            throw new InvalidOperationException("Start a disposable Play session before the item UI fixture.");
                        foreach (var guid in AssetDatabase.FindAssets("t:ItemData"))
                        {
                            var item = AssetDatabase.LoadAssetAtPath<ScriptableObjects.ScriptableObject_items.Script.ItemData>(AssetDatabase.GUIDToAssetPath(guid));
                            if (item != null && item.itemId != ScriptableObjects.ScriptableObject_items.Script.ItemID.NONE)
                                if (!invertoryAndItem.Inventory.instance.AddItem(item))
                                    throw new InvalidOperationException("Item preview inventory is full.");
                        }
                        break;
                    case "preview-ward":
                        if (!EditorApplication.isPlaying || Portal.PortalManager.Instance == null)
                            throw new InvalidOperationException("Start a disposable Play session before the ward fixture.");
                        Portal.PortalManager.Instance.StartCoroutine(Portal.PortalManager.Instance.TransitScene(Portal.PortalID.STAIR_FIRST_TO_FLOOR_SECOND));
                        break;
                    case "pause-guards":
                    case "resume-guards":
                        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play before the guard regression fixture.");
                        foreach (var guard in UnityEngine.Object.FindObjectsOfType<Scripts_Creatures.Creatures.Guard>())
                            if (command == "pause-guards") guard.StopPatrol(); else guard.StartPatrol();
                        break;
                    case "isolate-side-door":
                        if (!EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "0201")
                            throw new InvalidOperationException("Enter the ward fixture before isolating its door.");
                        foreach (var creature in UnityEngine.Object.FindObjectsOfType<Scripts_Creatures.Creatures.Creature>())
                            creature.gameObject.SetActive(false);
                        Portal.Character.Instance.transform.position = new Vector3(-.9f,1.5f,0);
                        Physics2D.SyncTransforms();
                        break;
                    case "capture-probe": LucyRoomChaseProbe.BeginCapture(); break;
                    case "capture-flow": LucyRoomChaseProbe.BeginCaptureFlow(); break;
                    case "new-game": Lucy.Presentation.GameSession.Instance.StartNewGame(); break;
                    case "story-blockout": Lucy.Presentation.GameSession.Instance.StartStoryBlockout(); break;
                    case "story-check": NightRoundStateChecks.Run(); break;
                    case "story-test-together": NightRoundPlaytest.Run(true); break;
                    case "story-test-alone": NightRoundPlaytest.Run(false); break;
                    case "nurse-flow": LucyRoomChaseProbe.BeginNurseFlow(); break;
                    case "map-preview": LucyMapPreview.Run(); break;
                    case "room-chase":
                        if (!EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "0201")
                            throw new InvalidOperationException("Enter the ward fixture before room chase.");
                        LucyRoomChaseProbe.Begin();
                        break;
                    case "validate": LucyValidation.ValidateProject(); break;
                    case "door-audit": LucyDoorAudit.Run(); break;
                    case "build": LucyValidation.BuildWindows(); break;
                    case "smoke": LucyPlaySmoke.Run(); break;
                    case "hide-input-overlay":
                        foreach (var canvas in UnityEngine.Object.FindObjectsOfType<Canvas>())
                            if (canvas.name == "Unity Play MCP Keyboard Status Canvas" ||
                                canvas.name == "Unity Play MCP Virtual Cursor Canvas") canvas.enabled = false;
                        break;
                    case "capture":
                        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play before capture.");
                        ScreenCapture.CaptureScreenshot("Logs/LucyVisual-" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + ".png");
                        break;
                    case "play":
                        if (EditorApplication.isPlaying) break;
                        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                                throw new InvalidOperationException("Save scene edits before automated Play.");
                        EditorSceneManager.OpenScene("Assets/Tilemap_jackupsil/Scenes/Title.unity");
                        EditorApplication.isPlaying = true;
                        break;
                    case "stop": EditorApplication.isPlaying = false; break;
                    default: throw new InvalidOperationException("Unknown Lucy validation command.");
                }
                File.WriteAllText("Logs/lucy-command-result.txt", command + ": accepted");
            }
            catch (Exception error)
            {
                File.WriteAllText("Logs/lucy-command-result.txt", command + ": failed\n" + error);
                Debug.LogException(error);
            }
        }

        private static void ApplyIcons()
        {
            const string path = "Assets/Resources/Presentation/LucyAppIcon.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (icon == null) throw new InvalidOperationException("Lucy application icon is missing.");
            foreach (var target in new[] { BuildTargetGroup.Unknown, BuildTargetGroup.Standalone })
            {
                var sizes = PlayerSettings.GetIconSizesForTargetGroup(target);
                var icons = new Texture2D[sizes.Length];
                for (int i = 0; i < icons.Length; i++) icons[i] = icon;
                PlayerSettings.SetIconsForTargetGroup(target, icons);
                var saved = PlayerSettings.GetIconsForTargetGroup(target);
                if (saved.Length == 0 || Array.Exists(saved, texture => texture != icon))
                    throw new InvalidOperationException("Icon assignment failed for " + target);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
