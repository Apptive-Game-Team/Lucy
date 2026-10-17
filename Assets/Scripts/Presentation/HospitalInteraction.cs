using UnityEngine;
using InputSystem;
using invertoryAndItem;
using Interactable;

namespace Lucy.Presentation
{
    /// <summary>One displayed target, shared by the HUD and interaction dispatch.</summary>
    public static class HospitalInteraction
    {
        public static MonoBehaviour Focus
        {
            get
            {
                var player = Portal.Character.Instance;
                var inventory = Inventory.instance;
                if (inventory != null && inventory.inventoryWindow != null && inventory.inventoryWindow.activeInHierarchy)
                    return null;
                if (player == null || !player.gameObject.activeInHierarchy || Time.timeScale <= 0 ||
                    (Portal.PortalManager.Instance != null && Portal.PortalManager.Instance.IsTransitioning) ||
                    GameSession.BlocksGameplayInput || InputManager.Instance == null ||
                    !InputManager.Instance.GetKeyActive(ActionCode.Interaction)) return null;
                MonoBehaviour best = null;
                float score = float.PositiveInfinity;
                foreach (var candidate in InteractableObject.Nearby)
                {
                    if (candidate == null || !candidate.CanInteract) continue;
                    float distance = Vector2.Distance(player.transform.position, candidate.transform.position);
                    // A revealed item takes precedence over the drawer that contained it.
                    if (candidate is ItemObject pickup)
                        distance += inventory != null && inventory.CanAddItem(pickup.item) ? -100 : 100;
                    if (distance >= score) continue;
                    best = candidate; score = distance;
                }
                var record = HospitalStory.ActiveRecord;
                if (record != null && record.isActiveAndEnabled)
                {
                    float distance = Vector2.Distance(player.transform.position, record.transform.position);
                    if (distance <= 1.6f && distance < score) best = record;
                }
                return best;
            }
        }

        public static string Label(MonoBehaviour target)
        {
            if (target is Portal.Portal portal) return portal.InteractionLabel;
            if (target is ItemObject item)
                return Inventory.instance != null && Inventory.instance.CanAddItem(item.item)
                    ? (item.item != null ? item.item.displayName : "물건") + " 줍기"
                    : "가방이 가득 찼습니다 · I 소지품";
            if (target is HospitalStory) return "대피 기록 읽기";
            if (target is Furniture furniture && furniture.furniture != null)
            {
                switch (furniture.furniture.furnitureType)
                {
                    case FurnitureType.Drawer:
                        var controller = FurnitureController.Instance;
                        return controller != null && controller.flashlight != null && controller.flashlight.activeInHierarchy
                            ? "서랍에서 손전등 꺼내기" : "서랍 조사";
                    case FurnitureType.Cabinet: return "캐비닛에 숨기";
                    case FurnitureType.Bookshelf: return furniture.gameObject.scene.name == GameSession.FirstScene
                        ? "병동 비상 안내 읽기" : "책장 조사";
                }
            }
            if (target is Scripts_door.DoorObject door) return door.InteractionLabel;
            if (target is Slide_Puzzle.PuzzleInteract) return "퍼즐 조사";
            if (target is Dialogue.NpcDialogue) return "대화하기";
            return "조사하기";
        }
    }
}
