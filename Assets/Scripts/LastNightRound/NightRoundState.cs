using System;

namespace Lucy.LastNightRound
{
    /// <summary>Serializable story facts; world controllers supply actual arrival/crossing events.</summary>
    [Serializable]
    public sealed class NightRoundState
    {
        public bool flashlight, bracelet, metNurse, transferMismatch, firstEscape;
        public bool fuse, powerRestored, secondEscape, truth, reunited, nurseCrossed, manualDoor;
        public int circuits = 1; // Light = 1, supply = 2, internal door = 4; two supply slots.
        public string ending = "";
        public int finalPower; // 0 unset, 1 connecting door / manual exit, 2 main exit.

        public int PoweredCircuitCount => ((circuits & 1) != 0 ? 1 : 0) + ((circuits & 2) != 0 ? 1 : 0) + ((circuits & 4) != 0 ? 1 : 0);

        // Shared by world art and focus selection; circuits and final routing remain reversible.
        public bool IsActionComplete(string id)
        {
            switch (id)
            {
                case "flashlight": return flashlight;
                case "bracelet": return bracelet;
                case "meet": return metNurse;
                case "manifest": return transferMismatch;
                case "safe": return firstEscape;
                case "fuse": return fuse;
                case "service-exit": return secondEscape;
                case "lock-record": return truth;
                case "reunite": return reunited;
                case "manual": return manualDoor;
                default: return false;
            }
        }

        public bool IsCircuitPowered(string id)
        {
            int bit = id == "circuit-light" ? 1 : id == "circuit-supply" ? 2 : id == "circuit-door" ? 4 : 0;
            return (circuits & bit) != 0;
        }

        public bool CanLeaveWard { get { return flashlight && metNurse; } }
        public bool CanLeaveNursing { get { return firstEscape; } }
        public bool CanLeaveService { get { return secondEscape; } }
        public bool CanLeaveIsolation { get { return reunited; } }
        public bool CanCrossFinalDoor
        {
            get { return reunited && (finalPower == 2 || (finalPower == 1 && manualDoor && nurseCrossed)); }
        }

        public string Objective
        {
            get
            {
                if (!string.IsNullOrEmpty(ending)) return ending == "together" ? "함께 밖으로 — 열린 문 너머로 나왔다." : "먼저 밖으로 — 내 이름과 기록을 갖고 나왔다.";
                if (!flashlight) return "침상 옆 서랍에서 손전등을 챙기세요.";
                if (!metNurse) return "병실 문에서 서연의 목소리를 확인하세요.";
                if (!bracelet) return "팔찌의 이름과 병실 번호를 확인하세요.";
                if (!transferMismatch) return "간호사실에서 101번 이송 명단과 팔찌를 대조하세요.";
                if (!firstEscape) return "호출등이 켜졌습니다. 직원 휴게실로 피하세요.";
                if (!fuse) return (circuits & 2) == 0 ? "배전반에서 물품실 회로를 켜세요. 공급은 두 회로까지 가능합니다." : "물품실에서 연결문 제어 퓨즈를 확보하세요.";
                if (!powerRestored) return "회로 하나를 끄고 연결 방화문 제어에 전원을 공급하세요.";
                if (!secondEscape && (circuits & 4) == 0) return "연결 방화문 회로가 꺼져 있습니다. 배전반에서 전원을 다시 공급하세요.";
                if (!secondEscape) return "열어 둔 유지 보수 통로로 빠져나가세요.";
                if (!truth) return "폐쇄 병동 제어실에서 101호 잠금 이력을 확인하세요.";
                if (!reunited) return "무전의 실제 응답을 따라 초록 표시등의 연결문에서 서연을 찾으세요.";
                if (finalPower == 0) return "제어반에서 연결문 유지 또는 주 출구 우선을 선택하세요.";
                if (finalPower == 2) return "주 출구에 전원이 공급됩니다. 출구를 넘기 전에는 선택을 바꿀 수 있습니다.";
                if (!manualDoor) return "서연의 연결문 전력을 유지하며 내부 방화문 수동 손잡이를 조작하세요.";
                if (!nurseCrossed) return "서연이 연결문과 내부 방화문을 건널 때까지 기다리세요.";
                return "서연이 반대편 손잡이를 잡았습니다. 열린 출구로 함께 나가세요.";
            }
        }

        public bool Apply(string action, out string text)
        {
            text = "아직 할 수 없는 행동입니다.";
            if (!string.IsNullOrEmpty(ending)) { text = "이미 병원 밖으로 나왔습니다."; return false; }
            switch (action)
            {
                case "flashlight":
                    return Mark(ref flashlight, true, "손전등을 챙겼다. 이제 문 쪽을 살펴볼 수 있다.", "손전등은 이미 챙겼습니다.", out text);
                case "bracelet":
                    return Mark(ref bracelet, true, "팔찌: 101 / 루시. 이송 명단과 대조할 수 있다.", "팔찌는 이미 확인했습니다.", out text);
                case "meet":
                    return Mark(ref metNurse, flashlight, "서연: 밖으로 나갔어요. 같이 나가요.\n서연이 바깥 해제 손잡이를 돌려 병실 문을 열었다.", "손전등을 챙긴 뒤 병실 문을 확인하세요.", out text);
                case "manifest":
                    return Mark(ref transferMismatch, bracelet && metNurse, "101 / 루시 — 이송 완료 02:05, 담당 서연.\n차량 인계 확인은 공란이다. 마지막 차량은 02:18에 떠났다.\n단말과 함께 호출등이 켜졌다. 직원 휴게실로 피해야 한다.", "서연과 만난 뒤 팔찌의 병실 번호를 확인하세요.", out text);
                case "safe":
                    return Mark(ref firstEscape, transferMismatch, "안전실 문을 닫았다. 발소리가 멀어진다.\n서연: 잘못 적었습니다. 이번에는 확인하고 같이 나가요.", "먼저 이송 명단의 불일치를 확인해야 합니다.", out text);
                case "circuit-light": return ToggleCircuit(1, out text);
                case "circuit-supply": return ToggleCircuit(2, out text);
                case "circuit-door": return ToggleCircuit(4, out text);
                case "fuse":
                    return Mark(ref fuse, firstEscape && (circuits & 2) != 0, "연결문 제어 퓨즈를 확보해 표시된 소켓에 장착했다.", "첫 추격을 벗어난 뒤 물품실 전원을 켜야 합니다.", out text);
                case "service-exit":
                    return Mark(ref secondEscape, powerRestored && (circuits & 4) != 0, "유지 보수 통로에 도착했다. 순회자의 발소리가 멀어진다.", "연결 방화문 회로에 전원을 공급한 상태로 통로에 진입하세요.", out text);
                case "lock-record":
                    return Mark(ref truth, secondEscape, "02:17 / 101호 / 외부 보호 잠금 / 수동 입력: 서연\n02:18 / 복도 접근 차단\n02:19 / 외부 해제 요청 / 현장 조작 필요\n서연: 제가 잠갔어요. 제가 잘못했습니다.", "유지 보수 통로를 지나 폐쇄 병동에 도착해야 합니다.", out text);
                case "reunite":
                    return Mark(ref reunited, truth, "무전의 새 질문에 서연이 답하고 연결문 손잡이를 두드렸다.\n루시: 기록은 제가 갖고 갈게요.", "잠금 이력을 확인하고 실제 서연의 응답을 찾아야 합니다.", out text);
                case "power-together": return SelectFinalPower(1, out text);
                case "power-alone": return SelectFinalPower(2, out text);
                case "manual":
                    return Mark(ref manualDoor, reunited && finalPower == 1, "내부 방화문의 수동 손잡이를 작동했다. 서연이 건너올 길이 열렸다.", "서연의 연결문에 전력을 유지해야 수동 탈출을 준비할 수 있습니다.", out text);
                case "nurse-crossed":
                    return Mark(ref nurseCrossed, reunited && manualDoor && finalPower == 1, "서연이 두 문을 지나 반대편 손잡이를 잡았다. 이제 루시가 건널 수 있다.", "연결문 전력과 수동 해제를 유지한 뒤 서연의 실제 통과를 기다리세요.", out text);
                case "exit":
                    if (!CanCrossFinalDoor) { text = "출구 전원을 선택하거나, 수동 해제 후 서연이 건너올 때까지 기다리세요."; return false; }
                    ending = finalPower == 1 ? "together" : "alone";
                    text = ending == "together" ? "함께 밖으로\n루시: 이번에는 문을 잠그지 마세요.\n서연: 네. 열어 둘게요." : "먼저 밖으로\n루시: 루시. 아직 여기 있어요.\n기록을 주머니에 넣고 아침 도로로 걸어 나왔다.";
                    return true;
                default: text = "알 수 없는 행동입니다."; return false;
            }
        }

        private static bool Mark(ref bool flag, bool allowed, string success, string blocked, out string text)
        {
            if (flag) { text = "이미 확인하거나 완료한 일입니다."; return false; }
            if (!allowed) { text = blocked; return false; }
            flag = true;
            text = success;
            return true;
        }

        private bool ToggleCircuit(int bit, out string text)
        {
            if (!firstEscape) { text = "직원 휴게실로 피한 뒤 설비 구역 배전반을 조작하세요."; return false; }
            bool enabling = (circuits & bit) == 0;
            if (enabling && bit == 4 && !fuse) { text = "연결문 제어 퓨즈가 필요합니다."; return false; }
            int next = circuits ^ bit;
            if (((next & 1) != 0 ? 1 : 0) + ((next & 2) != 0 ? 1 : 0) + ((next & 4) != 0 ? 1 : 0) > 2)
            { text = "비상 공급은 두 회로까지입니다. 다른 회로 하나를 먼저 끄세요."; return false; }
            circuits = next;
            string name = bit == 1 ? "처치실 조명" : bit == 2 ? "물품실 잠금 해제" : "연결 방화문 제어";
            text = name + (enabling ? " 회로에 전원을 공급했습니다." : " 회로 전원을 껐습니다.");
            if (bit == 4 && enabling && !powerRestored)
            {
                powerRestored = true; // First restoration schedules the pursuit exactly once.
                text += "\n호출등까지 켜졌다. 서연: 물품실 뒤 점검 통로로 가세요.";
            }
            return true;
        }

        private bool SelectFinalPower(int value, out string text)
        {
            if (!reunited) { text = "먼저 실제 서연과 재회하세요."; return false; }
            if (nurseCrossed) { text = "서연이 이미 건너 손잡이를 잡고 있습니다. 열린 출구로 이동하세요."; return false; }
            if (finalPower == value) { text = "이미 이 경로에 전력을 공급하고 있습니다."; return false; }
            finalPower = value;
            manualDoor = false; // Switching power releases Lucy's previous manual hold.
            text = value == 1 ? "연결문 유지 / 수동 출구\n서연의 문이 열렸다. 루시는 내부 방화문을 수동으로 열어야 한다." : "주 출구 우선 / 연결문 닫힘\n루시의 통로가 열리고 서연의 연결문이 닫혔다. 나가기 전에는 선택을 바꿀 수 있다.";
            return true;
        }
    }
}
