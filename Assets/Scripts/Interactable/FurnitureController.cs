using System.Collections;
using System.Collections.Generic;
using InputSystem;
using invertoryAndItem;
using Portal;
using UnityEngine;

namespace Interactable
{
    public class FurnitureController : SingletonObject<FurnitureController>
    {
        public Dictionary<FurnitureType, Furnitures> furnitures;
        public GameObject player;
        public GameObject flashlight;
        public List<Sprite> drawerImages;
        private Coroutine bookRoutine;

        public void ReadBook(GameObject page)
        {
            if (bookRoutine == null && page != null) bookRoutine = StartCoroutine(ReadBookUntilClosed(page));
        }

        private IEnumerator ReadBookUntilClosed(GameObject page)
        {
            float scale = Time.timeScale;
            var input = InputManager.Instance;
            input.SetMovementState(false);
            input.SetKeyActive(ActionCode.Interaction, false);
            input.SetKeyActive(ActionCode.OpenInventory, false);
            page.SetActive(true);
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(.25f);
            yield return new WaitUntil(() => !Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.Z));
            yield return new WaitUntil(() => page == null || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Escape));
            if (page != null) page.SetActive(false);
            Time.timeScale = scale;
            yield return new WaitForEndOfFrame();
            if (input != null)
            {
                input.SetMovementState(true);
                input.SetKeyActive(ActionCode.Interaction, true);
                input.SetKeyActive(ActionCode.OpenInventory, true);
            }
            bookRoutine = null;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            player = Character.Instance != null ? Character.Instance.gameObject : null;
            if (flashlight != null) flashlight.SetActive(false);
            furnitures = new Dictionary<FurnitureType, Furnitures>()
            {
                { FurnitureType.Cabinet, new Cabinet(player)},
                { FurnitureType.Bookshelf, new Bookshelf()},
                { FurnitureType.Drawer, new Drawer(flashlight,drawerImages)}
            };
        }

        /*void Update()
    {
        if (!player.activeSelf && InputManager.Instance.GetKeyDown(ActionCode.Interaction))
        {
            player.SetActive(true);
        }
    }*/
    }

    public abstract class Furnitures
    {
        public abstract void Interact(Furniture furniture);
    }

    public class Cabinet : Furnitures
    {
        private readonly GameObject player;
        private bool isHidden = false;

        public Cabinet(GameObject player)
        {
            this.player = player;
        }

        private IEnumerator Reveal()
        {
            InputManager.Instance.GetKeyDown(ActionCode.Interaction);
            yield return new WaitUntil(() =>
            {
                if (InputManager.Instance.GetKeyDown(ActionCode.Interaction))
                {
                    if (player == null) return true;
                    player.SetActive(true);
                    // Deactivating the player stops its stat coroutines; restore protection consistently.
                    var stats = player.GetComponent<CharacterStat>();
                    if (stats != null)
                    {
                        bool protectedByLight = FlashLight.instance != null && FlashLight.instance.battery > 0 &&
                            Inventory.instance != null && Inventory.instance.IsItemEquipped(FlashLight.instance.itemData);
                        if (protectedByLight) stats.StopMentalReduce();
                        else stats.StartMentalReduce();
                    }
                    return true;
                }
                return false;
            });
            yield return new WaitForSeconds(0.2f);
            isHidden = false;
        }

        public override void Interact(Furniture furniture)
        {
            if (!isHidden && player != null && FurnitureController.Instance != null)
            {
                if (player.TryGetComponent<Rigidbody2D>(out var body)) body.velocity = Vector2.zero;
                player.SetActive(false);
                isHidden = true;
                FurnitureController.Instance.StartCoroutine(Reveal());
            }
        }
    }

    public class Bookshelf : Furnitures
    {
        public override void Interact(Furniture furniture)
        {
            var session = Lucy.Presentation.GameSession.Instance;
            if (session != null && furniture.gameObject.scene.name == Lucy.Presentation.GameSession.FirstScene)
            {
                session.ShowDocument("병동 비상 안내",
                    "야간 정전 시 병실의 비상등은 약한 밝기로 유지됩니다.\n\n" +
                    "이동 전 오른쪽 서랍에서 손전등을 꺼내세요.\n가방(I)에서 손전등을 선택하고 장착할 수 있습니다.\n\n" +
                    "배터리가 다 떨어지면 손전등이 꺼집니다.\n가방에서 예비 배터리를 사용해 보충하세요.\n\n" +
                    "병실을 나와 오른쪽 계단으로 이동하세요.\n2층 간호실의 대피 기록을 확인하세요.\n\n" +
                    "안내문 아래에 작은 글씨가 덧붙어 있다.\n‘루시, 침상 아래 메모를 먼저 읽어 줘.’");
                return;
            }
            var bookPage = furniture.transform.Find("Canvas");
            if (bookPage != null && FurnitureController.Instance != null)
                FurnitureController.Instance.ReadBook(bookPage.gameObject);
        }
    }

    public class Drawer : Furnitures
    {
        private GameObject flashlight;
        private readonly List<Sprite> drawerImages;


        public Drawer(GameObject flashlight,List<Sprite> drawerImages)
        {
            this.flashlight = flashlight;
            this.drawerImages = drawerImages;

        }

        public override void Interact(Furniture furniture)
        {
            var spriteRenderer = furniture.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || drawerImages == null || drawerImages.Count < 2) return;
            if (flashlight == null || !flashlight.activeSelf)
            {
                if (spriteRenderer.sprite == drawerImages[0])
                {
                    spriteRenderer.sprite = drawerImages[1];
                    if (flashlight != null)
                    {
                        flashlight.SetActive(true);
                    }
                }
                else if (spriteRenderer.sprite == drawerImages[1])
                {
                    spriteRenderer.sprite = drawerImages[0];
                }
            }
            else
            {
                var pickup = flashlight.GetComponent<ItemObject>();
                if (pickup != null && Inventory.instance != null && Inventory.instance.AddItem(pickup.item))
                {
                    flashlight.SetActive(false);
                    flashlight = null;
                }
            }
        }
    }
}
