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
        private static int manualPhase;
        private static bool verifyRecovery;
        private static int epiloguePhase;
        private static Vector2 pausedPlayer, pausedNurse;
        static NightRoundPlaytest() { EditorApplication.update += Tick; }
        public static void Run(bool together)
        {
            if (NightRoundGame.Instance == null || NightRoundGame.Instance.State.metNurse) throw new Exception("Start a fresh story blockout first.");
            expected = together ? "together" : "alone";
            manualPhase=0; verifyRecovery=false;
            epiloguePhase=0;
            points = new Dictionary<string, Vector2> {
                {"bracelet",new Vector2(-5,3)}, {"flashlight",new Vector2(-5,-3)}, {"meet",new Vector2(5,0)},
                {"manifest",new Vector2(15,3)}, {"safe",new Vector2(22,-3)}, {"circuit-supply",new Vector2(34,3)},
                {"fuse",new Vector2(40,-3)}, {"circuit-light",new Vector2(31,3)}, {"circuit-door",new Vector2(37,3)},
                {"service-exit",new Vector2(42,0)}, {"lock-record",new Vector2(49,3)}, {"recording",new Vector2(53,-3)}, {"radio",new Vector2(54,3)}, {"reunite",new Vector2(59,4)},
                {"power-together",new Vector2(68,3)}, {"power-alone",new Vector2(72,3)}, {"manual",new Vector2(75,-2)}, {"cross-b",new Vector2(78.4f,0)}, {"exit",new Vector2(82,0)} };
            var list = new List<string> { "bracelet","flashlight","meet","manifest","safe","circuit-supply","fuse","circuit-light","circuit-door","service-exit","lock-record","recording","radio","reunite","power-together" };
            if (together) list.Add("manual");
            else list.AddRange(new[]{"power-alone","capture-retry","power-alone"});
            list.Add("cross-b"); list.Add("exit"); steps = new Queue<string>(list);
            next = EditorApplication.timeSinceStartup; deadline = next + 360;
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
            if (epiloguePhase==3)
            {
                if (game.EpilogueBeat!=-1 || game.State.ending!="" || game.State.finalPower!=0 || !game.State.reunited || game.IsBlocked || game.PlayerPosition.x>62 || game.NursePosition.y<5.8f)
                { Write("FAIL ending retry did not restore playable final choice"); steps=null; return; }
                Write("PASS actual ending="+expected+"; dawn complete, pause/resume and R replay verified at "+DateTime.UtcNow.ToString("o")); steps=null; return;
            }
            if (game.EpilogueComplete)
            {
                if (game.State.ending!=expected) { Write("FAIL wrong ending="+game.State.ending); steps=null; return; }
                if (epiloguePhase!=4 && epiloguePhase!=5) { Write("FAIL epilogue pause was not verified"); steps=null; return; }
                if (epiloguePhase==5) { Press(KeyCode.R,.12f); next+=1; epiloguePhase=3; return; }
                Write("ENDING dawn epilogue complete="+expected);
                if (!Application.isBatchMode) ScreenCapture.CaptureScreenshot("Logs/NightRound-"+expected+".png");
                epiloguePhase=5; next+=.5; return;
            }
            if (game.EpilogueBeat==1 && epiloguePhase==0)
            { Press(KeyCode.Escape,.12f); epiloguePhase=1; return; }
            if (epiloguePhase==1)
            { pausedPlayer=game.PlayerPosition; pausedNurse=game.NursePosition; epiloguePhase=2; next+=1; return; }
            if (epiloguePhase==2 && game.EpilogueBeat==1)
            {
                if (game.PlayerPosition!=pausedPlayer || game.NursePosition!=pausedNurse) { Write("FAIL epilogue moved while paused"); steps=null; return; }
                Write("EPILOGUE pause froze both actors; resume"); Press(KeyCode.Escape,.12f); epiloguePhase=4; return;
            }
            if (game.IsDead)
            {
                if (steps.Count>0 && steps.Peek()=="capture-retry")
                { Write("RECOVERY caught naturally; pressing R"); Press(KeyCode.R,.12f); next+=1.2; return; }
                Write("FAIL caught at " + game.PlayerPosition); steps=null; return;
            }
            if (steps.Count>0 && steps.Peek()=="capture-retry" && game.State.finalPower==0)
            {
                if (!game.State.reunited || game.State.nurseCrossed || game.State.manualDoor || game.State.nurseOutside || game.NursePosition.y<5.8f)
                { Write("FAIL checkpoint did not restore final choice and staff-side nurse"); steps=null; return; }
                Write("RECOVERY checkpoint restored, chase cleared, choices reset, nurse behind A"); steps.Dequeue(); verifyRecovery=true;
            }
            if (game.IsBlocked) { Press(KeyCode.Space, .12f); next += .3; return; }
            if (steps.Count == 0) { Press(KeyCode.E, .12f); return; }
            string id = steps.Peek();
            if (id=="capture-retry") return;
            Vector2 position = game.PlayerPosition, target = points[id];
            int fromRoom = Mathf.FloorToInt((position.x + 9) / 18), toRoom = Mathf.FloorToInt((target.x + 9) / 18);
            if (fromRoom != toRoom && Mathf.Abs(position.y) > .15f) target = new Vector2(position.x, 0);
            else if (fromRoom != toRoom) target = new Vector2(target.x, 0);
            if ((id=="cross-b" || id=="exit") && Mathf.Abs(position.y)>.15f) target=new Vector2(position.x,0);
            var delta = target - position;
            if (Mathf.Abs(delta.x) > .2f)
            { Press(delta.x > 0 ? KeyCode.RightArrow : KeyCode.LeftArrow, Mathf.Clamp(Mathf.Abs(delta.x) / 3.4f, .03f, .18f)); return; }
            if (Mathf.Abs(delta.y) > .2f)
            { Press(delta.y > 0 ? KeyCode.UpArrow : KeyCode.DownArrow, Mathf.Clamp(Mathf.Abs(delta.y) / 3.4f, .03f, .18f)); return; }
            if (Vector2.Distance(position, points[id]) > .45f) return;
            if (id=="reunite" && Vector2.Distance(game.NursePosition,new Vector2(59,7))>.5f) return;
            if (id=="manual")
            {
                if (manualPhase==0) { Write("HOLD partial E, then release"); Press(KeyCode.E,.7f); next+=.4; manualPhase=1; return; }
                if (manualPhase==1)
                {
                    if (game.IsManualHeld || game.ManualProgress<.5f || game.State.manualDoor || game.State.nurseCrossed)
                    { Write("FAIL partial hold/release invariants"); steps=null; return; }
                    Write("HOLD progress retained; F accessibility toggle"); Press(KeyCode.F,.12f); manualPhase=2; return;
                }
                if (!game.State.nurseCrossed) return;
                if (game.NursePosition.x<77.8f || game.State.nurseOutside) { Write("FAIL nurse crossing was not physical"); steps=null; return; }
                Write("HOLD nurse crossed B and took far handle"); steps.Dequeue(); return;
            }
            if (id=="cross-b") { Write("CROSS player physically passed B"); steps.Dequeue(); return; }
            if (id=="power-alone" && verifyRecovery) Write("RECOVERY replaying final choice through keyboard");
            Write("INPUT " + id + " at " + position);
            Press(KeyCode.E, id == "manual" ? 2.2f : .12f); steps.Dequeue();
        }
    }
}
