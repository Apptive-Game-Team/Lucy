using System;
using System.IO;
using Lucy.LastNightRound;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Lucy.Editor
{
    /// <summary>Batch entrypoint: project checks and real-keyboard endings.</summary>
    [InitializeOnLoad]
    public static class NightRoundArtValidation
    {
        private const string Key = "Lucy.ArtValidation.";
        private static double next;
        static NightRoundArtValidation() { EditorApplication.update += Tick; }

        public static void BuildAndPreview()
        {
            EditorSceneManager.OpenScene("Assets/Tilemap_jackupsil/Scenes/Title.unity");
            ValidateAssets();
            LucyValidation.BuildWindows();
            EditorApplication.isPlaying = true;
        }

        public static void Begin()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Run this isolated suite in batch mode.");
            EditorSceneManager.OpenScene("Assets/Tilemap_jackupsil/Scenes/Title.unity");
            LucyValidation.ValidateProject();
            ValidateAssets();
            StartSuite();
        }

        public static void RunInEditor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before the suite.");
            for (int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene changes before the suite.");
            EditorSceneManager.OpenScene("Assets/Tilemap_jackupsil/Scenes/Title.unity");
            LucyValidation.ValidateProject(); ValidateAssets(); StartSuite();
        }

        private static void ValidateAssets()
        {
            using (var art = new NightRoundArt())
            {
                art.Atlas("HospitalSurfaces",2,2,false); art.Atlas("HospitalFurnitureV2",2,2);
                art.Atlas("HospitalStorage",2,2); art.Atlas("HospitalWardProps",4,2);
                art.Atlas("WardDoors",2,2); art.Atlas("NurseRun",4,2); art.Atlas("WardOrderlyWalk",4,3);
                if (Resources.Load<Sprite>("Presentation/NurseWorld") == null) throw new Exception("Nurse world sprite missing.");
            }
        }

        private static void StartSuite()
        {
            string save = Path.Combine(Application.persistentDataPath,"last-night-round-blockout.json");
            SessionState.SetString(Key+"save",File.Exists(save) ? File.ReadAllText(save) : "");
            SessionState.SetInt(Key+"stage",1);
            SessionState.SetBool(Key+"ward",false); SessionState.SetBool(Key+"service",false);
            SessionState.SetString(Key+"start",DateTime.UtcNow.ToString("o"));
            File.WriteAllText("Logs/NightRoundArtValidation.txt", "START " + DateTime.UtcNow.ToString("o") + "\nProject checks and all atlas slices passed.\n");
            EditorApplication.isPlaying = true;
        }

        private static void Tick()
        {
            int stage = SessionState.GetInt(Key+"stage",0);
            if (stage == 0 || EditorApplication.isCompiling || EditorApplication.timeSinceStartup < next) return;
            next = EditorApplication.timeSinceStartup + .5;
            try
            {
                if ((DateTime.UtcNow-DateTime.Parse(SessionState.GetString(Key+"start",""))).TotalMinutes > 12)
                    throw new Exception("Playtest suite timed out.");
                if (stage == 1 || stage == 4)
                {
                    if (!EditorApplication.isPlaying || Presentation.GameSession.Instance == null) return;
                    AudioListener.volume = 0;
                    Presentation.GameSession.Instance.StartNewGame();
                    SessionState.SetInt(Key+"stage",stage+1);
                    next += 2;
                }
                else if (stage == 2 || stage == 5)
                {
                    if (NightRoundGame.Instance == null) return;
                    NightRoundPlaytest.Run(stage == 2);
                    SessionState.SetInt(Key+"stage",stage+1);
                }
                else if (stage == 3 || stage == 6)
                {
                    var game = NightRoundGame.Instance;
                    if (game == null) throw new Exception("Game disappeared during playtest.");
                    if (!Application.isBatchMode && !game.IsBlocked && stage == 3)
                    {
                        string capture = game.PlayerPosition.x > 28 && game.State.firstEscape ? "service" : game.State.flashlight && game.PlayerPosition.x < 5 ? "ward" : null;
                        if (capture != null && !SessionState.GetBool(Key+capture,false))
                        {
                            ScreenCapture.CaptureScreenshot("Logs/NightRoundRefinement-"+capture+".png");
                            SessionState.SetBool(Key+capture,true);
                        }
                    }
                    string path = "Logs/NightRoundPlaytest-"+(stage == 3 ? "together" : "alone")+".txt";
                    string result = File.ReadAllText(path);
                    if (result.Contains("FAIL")) throw new Exception(result);
                    if (!result.Contains("PASS")) return;
                    File.AppendAllText("Logs/NightRoundArtValidation.txt", result + "\n");
                    SessionState.SetInt(Key+"stage",stage == 3 ? 7 : 8);
                    next += 2; // allow the ending screenshot to reach the GPU
                }
                else if (stage == 7 || stage == 8)
                {
                    EditorApplication.isPlaying = false;
                    SessionState.SetInt(Key+"stage",stage == 7 ? 9 : 10);
                }
                else if (stage == 9)
                {
                    if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                    EditorSceneManager.OpenScene("Assets/Tilemap_jackupsil/Scenes/Title.unity");
                    SessionState.SetInt(Key+"stage",4); EditorApplication.isPlaying = true;
                }
                else if (stage == 10)
                {
                    if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                    Finish(0,"PASS both keyboard endings and runtime art loading.");
                }
            }
            catch (Exception error) { Finish(1,"FAIL " + error); }
        }

        private static void Finish(int code, string result)
        {
            string save = Path.Combine(Application.persistentDataPath,"last-night-round-blockout.json");
            string original = SessionState.GetString(Key+"save","");
            if (original.Length > 0) File.WriteAllText(save,original); else if (File.Exists(save)) File.Delete(save);
            SessionState.SetInt(Key+"stage",0);
            File.AppendAllText("Logs/NightRoundArtValidation.txt",result + "\n");
            if (Application.isBatchMode) EditorApplication.Exit(code);
            else { EditorApplication.isPlaying = false; Debug.Log("Night Round refinement verification: " + result); }
        }
    }
}
