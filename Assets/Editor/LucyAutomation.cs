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
                    case "validate": LucyValidation.ValidateProject(); break;
                    case "build": LucyValidation.BuildWindows(); break;
                    case "smoke": LucyPlaySmoke.Run(); break;
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
    }
}
