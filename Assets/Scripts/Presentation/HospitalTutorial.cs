using UnityEngine;
using invertoryAndItem;
using ScriptableObjects.ScriptableObject_items.Script;

namespace Lucy.Presentation
{
    /// <summary>Run-local onboarding, completed by observed actions rather than elapsed time.</summary>
    public static class HospitalTutorial
    {
        private static readonly bool[] done = new bool[7];
        private static bool skipped, hasOrigin;
        private static Vector2 origin;
        private static float completedAt = -100;
        public static int Step { get { for (int i=0; i<done.Length; i++) if (!done[i]) return i; return done.Length; } }
        public static bool ProtectLearning => !skipped && Step<4 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name==GameSession.FirstScene;
        public static void Reset()
        {
            System.Array.Clear(done,0,done.Length);
            skipped=hasOrigin=false; completedAt=-100;
        }
        public static void Skip() { skipped=true; }
        public static bool Visible(string scene) => !skipped && (scene==GameSession.FirstScene || scene=="0102") && (Step<7 || Time.unscaledTime-completedAt<5);
        public static void DoorOpened(string scene)
        {
            int previous=Step;
            if (scene==GameSession.FirstScene) done[5]=true;
            if (previous<7 && Step==7) completedAt=Time.unscaledTime;
        }
        public static void EnterScene(string scene)
        {
            int previous=Step;
            if (scene=="0102") done[6]=true;
            if (previous<7 && Step==7) completedAt=Time.unscaledTime;
        }
        public static void Observe(string scene)
        {
            if (skipped || (scene!=GameSession.FirstScene && scene!="0102")) return;
            int previous=Step;
            var player=Portal.Character.Instance;
            if (player!=null && scene==GameSession.FirstScene && !GameSession.BlocksGameplayInput && Time.timeScale>0)
            {
                if (!hasOrigin) { origin=player.transform.position; hasOrigin=true; }
                if (Vector2.Distance(origin,player.transform.position)>=.7f) done[0]=true;
            }
            var inventory=Inventory.instance;
            if (inventory!=null)
            {
                if (inventory.inventoryWindow!=null && inventory.inventoryWindow.activeSelf) done[2]=true;
                if (inventory.slots!=null)
                    foreach (var slot in inventory.slots)
                    {
                        if (slot==null || slot.item==null || slot.quantity<=0 || slot.item.itemId!=ItemID.FLASHLIGHT) continue;
                        done[1]=true;
                        if (inventory.IsItemEquipped(slot.item)) done[3]=true;
                    }
            }
            if (HospitalStory.HasRecord(0)) done[4]=true;
            if (previous<7 && Step==7) completedAt=Time.unscaledTime;
        }
        public static string Heading => Step<7 ? "처음 플레이  " + (Step+1) + "/7    T 안내 끄기" : "기본 조작 완료 · J 기록장 / ESC 메뉴";
        public static string Instruction(string scene)
        {
            if (Step==7) return "이제 병동을 탐색하세요.\n달리면 체력, 어둠에서는 정신력이 줄어듭니다.";
            if (scene=="0102" && Step<6) return "아직 첫 방에서 배울 조작이 남았습니다.\n아래 계단 앞에서 E로 돌아가세요.";
            switch (Step)
            {
                case 0: return "방향키 또는 WASD로 움직여 보세요.\n밝은 침상 주변부터 살펴보세요.";
                case 1: return "오른쪽 서랍 앞에서 E로 여세요.\n다시 E를 눌러 손전등을 챙기세요.";
                case 2: return "I를 눌러 가방을 여세요.\n획득한 손전등이 들어 있습니다.";
                case 3: return "손전등 아이콘을 클릭하고 ‘장착’을 누르세요.\nI를 누르면 가방을 닫습니다.";
                case 4: return "침상 옆 ◆ 기록에 다가가 E로 읽으세요.\n기록은 다시 시작할 지점이 됩니다.";
                case 5: return "방 위쪽 문 앞에서 E로 여세요.\n문이 열린 뒤 복도로 나가세요.";
                default: return "복도 오른쪽 계단으로 향하세요.\n계단 앞에서 E를 눌러 이동하세요.";
            }
        }
    }
}
