using UnityEngine;

namespace Portal
{
    public class Portal : InteractableObject
    {
        [SerializeField] PortalID portalID;

        public bool IsStairway => portalID >= PortalID.FLOOR_FIRST_TO_STAIR &&
                                  portalID <= PortalID.FLOOR_SECOND_TO_STAIR_SECOND;
        public string InteractionLabel => portalID == PortalID.STAIR_FIRST_TO_FLOOR_FIRST
            ? "1층 병동으로 이동" : portalID == PortalID.STAIR_FIRST_TO_FLOOR_SECOND
            ? "2층 병동으로 이동" : "계단으로 이동";

        protected override void Start()
        {
            if (!IsStairway) return;
            if (portalID == PortalID.FLOOR_FIRST_TO_STAIR && TryGetComponent<BoxCollider2D>(out var entrance))
            {
                // Reachable from the corridor; no need to push into the stair artwork/wall.
                entrance.offset = new Vector2(-.55f, -.3f);
                entrance.size = new Vector2(1.8f, 2f);
            }
            base.Start();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (IsStairway) base.OnTriggerEnter2D(collision);
            else ActOnTrigger(collision);
        }

        protected override void ActOnTrigger(Collider2D collision)
        {
            if (!collision.CompareTag("Player") || PortalManager.Instance == null) return;
            if (PortalManager.Instance.IsTransitioning || Lucy.Presentation.GameSession.BlocksGameplayInput) return;
            if ((int)portalID == 9 && !Lucy.Presentation.HospitalStory.HasRecord(2))
            {
                Lucy.Presentation.GameSession.Instance.ShowDocument("비상 통로 — 전력 차단", "비상 통로가 잠겨 있다.\n\n2층 연결 병동의 마지막 점검함에서\n비상 전력 복구 기록을 찾아야 한다.\n아래쪽 복도의 오른쪽 출입구로 향하자.");
                return;
            }
            if ((int)portalID == 11 && Lucy.Presentation.HospitalStory.FoundCount < 4)
            {
                Lucy.Presentation.GameSession.Instance.ShowDocument("남겨진 기록", "아직 찾지 못한 대피 기록이 있다.\n\n1층 병동, 2층 간호실, 연결 통로 점검함,\n3층 비상 통로 입구를 확인하자.\nJ 키로 수집한 기록을 확인할 수 있다.");
                return;
            }
            PortalManager.Instance.StartCoroutine(PortalManager.Instance.TransitScene(portalID));
        }
    }
}
