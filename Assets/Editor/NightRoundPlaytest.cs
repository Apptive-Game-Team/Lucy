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
        private static List<Vector2> route;
        private static string routeId;
        static NightRoundPlaytest() { EditorApplication.update += Tick; }
        public static void Run(bool together)
        {
            if (NightRoundGame.Instance == null || NightRoundGame.Instance.State.metNurse) throw new Exception("Start a fresh story blockout first.");
            expected = together ? "together" : "alone";
            manualPhase=0; verifyRecovery=false;
            epiloguePhase=0;
            points=new Dictionary<string,Vector2>(); route=null; routeId=null;
            foreach(var id in new[]{"bracelet","flashlight","meet","manifest","safe","circuit-supply","fuse","circuit-light","circuit-door","service-exit","lock-record","recording","radio","reunite","power-together","power-alone","manual","exit"})
                points[id]=NightRoundGame.Instance.StationPosition(id);
            points["cross-b"]=new Vector2(60.4f,0);
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
        private static void ValidateLocks(string step,NightRoundGame game)
        {
            string blocked=step=="bracelet" ? "manifest" : step=="manifest" ? "circuit-supply" : step=="circuit-supply" ? "lock-record" : step=="lock-record" ? "power-together" : step=="power-together" ? "cross-b" : null;
            if(blocked==null) return;
            if(game.PlayerPath(points[blocked]).Count!=0 || game.PlayerPath(new Vector2(41,19.5f)).Count!=0)
            { Write("FAIL gate or staff-only corridor bypass at "+step); steps=null; return; }
            Write("GATE blocked premature "+blocked+" and staff corridor");
        }
        private static void Walk(KeyCode code,float distance)
        {
            float seconds=Mathf.Clamp(distance/5.2f,.03f,.18f);
            key.Invoke(null,new object[]{KeyCode.LeftShift,seconds});
            Press(code,seconds); next=EditorApplication.timeSinceStartup+seconds+.015;
        }
        private static void Write(string value) { File.AppendAllText("Logs/NightRoundPlaytest-" + expected + ".txt", value + "\n"); }
        private static void Tick()
        {
            if (steps == null || EditorApplication.timeSinceStartup < next) return;
            var game = NightRoundGame.Instance;
            if (!EditorApplication.isPlaying || game == null || EditorApplication.timeSinceStartup > deadline)
            { Write("FAIL stopped/missing game/timeout"); steps = null; return; }
            if (epiloguePhase==3)
            {
                if (game.EpilogueBeat!=-1 || game.State.ending!="" || game.State.finalPower!=0 || !game.State.reunited || game.IsBlocked || game.CurrentRoom!=3 || game.NursePosition.y<18)
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
                if (!game.State.reunited || game.State.nurseCrossed || game.State.manualDoor || game.State.nurseOutside || game.NursePosition.y<18)
                { Write("FAIL checkpoint did not restore final choice and staff-side nurse"); steps=null; return; }
                Write("RECOVERY checkpoint restored, chase cleared, choices reset, nurse behind A"); steps.Dequeue(); verifyRecovery=true; route=null; routeId=null;
            }
            if (game.IsBlocked) { Press(KeyCode.Space, .12f); next += .3; return; }
            if (steps.Count == 0) { Press(KeyCode.E, .12f); return; }
            string id = steps.Peek();
            if (id=="capture-retry") return;
            Vector2 position=game.PlayerPosition, target=points[id];
            if(routeId!=id || route==null)
            {
                ValidateLocks(id,game);
                if(steps==null) return;
                route=game.PlayerPath(target); routeId=id;
                if(route.Count==0) { Write("FAIL no walkable route to "+id+" from "+position); steps=null; return; }
            }
            while(route.Count>0 && Vector2.Distance(position,route[0])<.2f) route.RemoveAt(0);
            if(route.Count>0) target=route[0];
            var delta = target - position;
            if (Mathf.Abs(delta.x) > .2f)
            { Walk(delta.x > 0 ? KeyCode.RightArrow : KeyCode.LeftArrow,Mathf.Abs(delta.x)); return; }
            if (Mathf.Abs(delta.y) > .2f)
            { Walk(delta.y > 0 ? KeyCode.UpArrow : KeyCode.DownArrow,Mathf.Abs(delta.y)); return; }
            if (Vector2.Distance(position, points[id]) > .45f) return;
            if (id=="reunite" && Vector2.Distance(game.NursePosition,new Vector2(41,19.5f))>.5f) return;
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
                if (game.NursePosition.x<59.8f || game.State.nurseOutside) { Write("FAIL nurse crossing was not physical"); steps=null; return; }
                Write("HOLD nurse crossed B and took far handle"); steps.Dequeue(); return;
            }
            if (id=="cross-b") { Write("CROSS player physically passed B"); steps.Dequeue(); return; }
            if (id=="power-alone" && verifyRecovery) Write("RECOVERY replaying final choice through keyboard");
            Write("INPUT " + id + " at " + position);
            Press(KeyCode.E, id == "manual" ? 2.2f : .12f); steps.Dequeue();
        }
    }
}
