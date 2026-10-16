using UnityEngine;

namespace Portal
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] PortalID portalID;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player") || PortalManager.Instance == null) return;
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
