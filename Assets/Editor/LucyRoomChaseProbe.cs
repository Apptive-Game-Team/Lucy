using System.Collections;
using System.IO;
using System.Reflection;
using Scripts_Creatures.Creatures;
using Scripts_Creatures.Util;
using UnityEngine;
using static UnityEngine.Object;

namespace Lucy.Editor
{
    // Disposable Play-only fixture: real sensing, door action and movement after setup.
    public static class LucyRoomChaseProbe
    {
        public static void Begin() { Lucy.Presentation.GameSession.Instance.StartCoroutine(Run()); }
        public static void BeginCapture() { Lucy.Presentation.GameSession.Instance.StartCoroutine(Capture()); }
        public static void BeginNurseFlow() { Lucy.Presentation.GameSession.Instance.StartCoroutine(NurseFlow()); }
        private static IEnumerator NurseFlow()
        {
            AudioListener.volume=0;
            Lucy.Presentation.GameSession.Instance.StartNewGame();
            while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!=Lucy.Presentation.GameSession.FirstScene || Portal.PortalManager.Instance==null) yield return null;
            yield return new WaitForSecondsRealtime(1);
            yield return Portal.PortalManager.Instance.TransitScene(Portal.PortalID.STAIR_FIRST_TO_FLOOR_SECOND);
            yield return new WaitForSecondsRealtime(1);
            var player=Portal.Character.Instance;
            player.transform.position=new Vector3(-3,1.5f,0);
            Physics2D.SyncTransforms();
            var controller=FindObjectOfType<Event.NpcEventController>();
            var npc=GameObject.Find("NPC");
            File.WriteAllText("Logs/LucyNurseProbe.txt","Muted ward fixture; synthetic room-entry callback; duplicate callback sent.\n");
            controller.SendMessage("OnTriggerExit2D",player.GetComponent<Collider2D>());
            controller.SendMessage("OnTriggerExit2D",player.GetComponent<Collider2D>());
            float deadline=Time.realtimeSinceStartup+25;
            bool captured=false;
            while (npc!=null && Time.realtimeSinceStartup<deadline)
            {
                var view=npc.GetComponent<Lucy.Presentation.NurseDeparture>();
                File.AppendAllText("Logs/LucyNurseProbe.txt","position="+npc.transform.position+" frame="+(view!=null?view.FrameIndex:-1)+" move="+InputSystem.InputManager.Instance.GetKeyActive(InputSystem.ActionCode.MoveLeft)+"\n");
                if (!captured && npc.transform.position.x>-5 && view!=null)
                {
                    ScreenCapture.CaptureScreenshot("Logs/LucyNurse-Running.png"); captured=true;
                }
                yield return new WaitForSecondsRealtime(.2f);
            }
            File.AppendAllText("Logs/LucyNurseProbe.txt","finished npc="+(npc!=null)+" move="+InputSystem.InputManager.Instance.GetKeyActive(InputSystem.ActionCode.MoveLeft)+" interaction="+InputSystem.InputManager.Instance.GetKeyActive(InputSystem.ActionCode.Interaction)+" barrier="+(GameObject.Find("Barricade")!=null)+"\n");
        }
        public static void BeginCaptureFlow() { Lucy.Presentation.GameSession.Instance.StartCoroutine(CaptureFlow()); }
        private static IEnumerator CaptureFlow()
        {
            Lucy.Presentation.GameSession.Instance.StartNewGame();
            while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!=Lucy.Presentation.GameSession.FirstScene || Portal.PortalManager.Instance==null) yield return null;
            yield return new WaitForSecondsRealtime(1);
            yield return Portal.PortalManager.Instance.TransitScene(Portal.PortalID.STAIR_FIRST_TO_FLOOR_SECOND);
            yield return new WaitForSecondsRealtime(1);
            yield return Capture();
        }
        private static IEnumerator Capture()
        {
            AudioListener.volume=0;
            var guard=FindObjectOfType<Guard>();
            var player=Portal.Character.Instance;
            guard.StopPatrol();
            typeof(Creature).GetField("arrivalGraceRemaining",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(guard,0f);
            guard.transform.position=player.GetComponent<Collider2D>().bounds.center;
            Physics2D.SyncTransforms();
            float timeout=Time.realtimeSinceStartup+4;
            while (!Lucy.Presentation.GameSession.BlocksGameplayInput && Time.realtimeSinceStartup<timeout) yield return null;
            File.WriteAllText("Logs/LucyCaptureProbe.txt","Muted real collider overlap fixture\n");
            int wanted=0;
            foreach (string phase in new[]{"impact","knees","fall","collapse"})
            {
                var effect=FindObjectOfType<Lucy.Presentation.HospitalCaptureEffect>();
                while (effect!=null && effect.PhaseIndex<wanted) yield return null;
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot("Logs/LucyCapture-"+phase+".png");
                File.AppendAllText("Logs/LucyCaptureProbe.txt",phase+" effects="+FindObjectsOfType<Lucy.Presentation.HospitalCaptureEffect>().Length+" scale="+Time.timeScale+" blocked="+Lucy.Presentation.GameSession.BlocksGameplayInput+" volume="+AudioListener.volume+"\n");
                wanted++;
            }
            while (FindObjectOfType<Lucy.Presentation.HospitalCaptureEffect>()!=null) yield return null;
            yield return null;
            File.AppendAllText("Logs/LucyCaptureProbe.txt","finished effects="+FindObjectsOfType<Lucy.Presentation.HospitalCaptureEffect>().Length+" overlay="+(GameObject.Find("Capture impact")!=null)+"\n");
            ScreenCapture.CaptureScreenshot("Logs/LucyCapture-result.png");
        }
        private static IEnumerator Run()
        {
            AudioListener.volume = 0;
            var guard = FindObjectOfType<Guard>();
            var door = FindObjectOfType<Scripts_door.DoorObject>();
            var player = Portal.Character.Instance;
            if (guard == null || door == null || player == null) { File.WriteAllText("Logs/LucyRoomChase.txt","FAIL missing ward fixture"); yield break; }
            foreach (var e in FindObjectsOfType<Event.NpcEventController>()) { e.StopAllCoroutines(); Destroy(e); }
            yield return null;
            guard.StopPatrol();
            guard.transform.position = new Vector3(0,1,0);
            player.transform.position = new Vector3(-4,1.6f,0);
            Physics2D.SyncTransforms();
            var playerCollider = player.GetComponent<Collider2D>();
            File.WriteAllText("Logs/LucyRoomChase.txt","Muted Play fixture; NPC event disabled, actors placed beside ward doorway.\nPlayer body=" + playerCollider.bounds.center + "\n");
            var manager = FindObjectOfType<Scripts_Creatures.CreatureManager>();
            try
            {
                manager.pathFinders[0].FindPathToWorld(guard.transform.position,playerCollider.bounds.center);
                File.AppendAllText("Logs/LucyRoomChase.txt","FAIL closed door allowed route\n");
            }
            catch { File.AppendAllText("Logs/LucyRoomChase.txt","PASS closed door has no route\n"); }
            door.SendMessage("ActOnTrigger",playerCollider);
            yield return new WaitForSeconds(.4f);
            typeof(Creature).GetField("arrivalGraceRemaining",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(guard,0f);
            guard.StartPatrol();
            guard.GetComponent<Detector>().SetLookingDirection(Vector3.left);
            typeof(Creature).GetMethod("DetectPlayer",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(guard,null);
            float deadline = Time.realtimeSinceStartup + 8;
            while (Time.realtimeSinceStartup < deadline)
            {
                File.AppendAllText("Logs/LucyRoomChase.txt", "guard=" + guard.transform.position + " state=" + guard.CurrentStatus + " open=" + door.IsOpen + "\n");
                if (guard.transform.position.x < -2.5f)
                {
                    File.AppendAllText("Logs/LucyRoomChase.txt","PASS guard pursued through doorway into room\n");
                    guard.StopPatrol();
                    yield break;
                }
                yield return new WaitForSecondsRealtime(.25f);
            }
            guard.StopPatrol();
            File.AppendAllText("Logs/LucyRoomChase.txt","FAIL guard did not enter room\n");
        }
    }
}
