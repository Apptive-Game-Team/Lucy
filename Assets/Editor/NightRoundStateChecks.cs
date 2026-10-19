using System;
using Lucy.LastNightRound;
using UnityEngine;

public static class NightRoundStateChecks
{
    public static void Run()
    {
        int checks = 0;
        var state = new NightRoundState();
        Check(state.circuits == 1 && state.finalPower == 0 && state.ending == "", "New game defaults", ref checks);
        Check(!state.CanLeaveWard && !state.CanCrossFinalDoor, "New game doors", ref checks);
        Check(!state.IsActionComplete("flashlight") && state.PoweredCircuitCount == 1, "Fresh pickup and power presentation", ref checks);
        string[] premature = { "meet", "manifest", "safe", "circuit-light", "circuit-supply", "circuit-door", "fuse", "service-exit", "lock-record", "radio", "recording", "reunite", "power-together", "power-alone", "manual", "nurse-crossed", "nurse-outside", "exit", "unknown", null };
        foreach (string action in premature) RejectUnchanged(state, action, ref checks);
        Accept(state, "flashlight", ref checks);
        Check(state.IsActionComplete("flashlight"), "Collected pickup removed from focus", ref checks);
        RejectUnchanged(state, "flashlight", ref checks);
        Accept(state, "meet", ref checks);
        Check(state.CanLeaveWard, "Ward unlock after actual meeting", ref checks);
        RejectUnchanged(state, "manifest", ref checks);
        Accept(state, "bracelet", ref checks);
        Accept(state, "manifest", ref checks);
        RejectUnchanged(state, "manifest", ref checks);
        RejectUnchanged(state, "fuse", ref checks);
        Accept(state, "safe", ref checks);
        Check(state.CanLeaveNursing, "Nursing exit after safety arrival", ref checks);
        RejectUnchanged(state, "safe", ref checks);
        RejectUnchanged(state, "circuit-door", ref checks);
        RejectUnchanged(state, "fuse", ref checks);
        Accept(state, "circuit-supply", ref checks);
        Check(state.IsCircuitPowered("circuit-supply") && state.PoweredCircuitCount == 2 && !state.IsActionComplete("circuit-supply"), "Powered circuit remains reversible and focusable", ref checks);
        Accept(state, "fuse", ref checks);
        RejectUnchanged(state, "fuse", ref checks);
        RejectUnchanged(state, "circuit-door", ref checks);
        Check(state.circuits == 3 && !state.powerRestored, "Rejected third circuit has no reward", ref checks);
        Accept(state, "circuit-supply", ref checks);
        Accept(state, "circuit-door", ref checks);
        Check(state.circuits == 5 && state.powerRestored, "Light and door route", ref checks);
        var lightRoute = JsonUtility.FromJson<NightRoundState>(JsonUtility.ToJson(state));
        Accept(lightRoute, "service-exit", ref checks);
        Check(lightRoute.CanLeaveService, "Light and door permits service escape", ref checks);
        Accept(state, "circuit-door", ref checks);
        Check(state.powerRestored, "Restoration is latched when power switched off", ref checks);
        RejectUnchanged(state, "service-exit", ref checks);
        Check(state.Objective.Contains("전원을 다시 공급"), "Door off objective explains restoration", ref checks);
        string restoredText;
        Check(state.Apply("circuit-door", out restoredText), "Door power can be restored", ref checks);
        Check(!restoredText.Contains("호출등") && state.powerRestored, "Restoring power does not repeat first pursuit narrative", ref checks);
        Accept(state, "circuit-light", ref checks);
        Accept(state, "circuit-supply", ref checks);
        Check(state.circuits == 6, "Supply and door alternate route", ref checks);
        RejectUnchanged(state, "circuit-light", ref checks);
        RejectUnchanged(state, "lock-record", ref checks);
        Accept(state, "service-exit", ref checks);
        Check(state.CanLeaveService, "Service exit after arrival event", ref checks);
        Accept(state, "lock-record", ref checks);
        RejectUnchanged(state,"reunite",ref checks);
        Accept(state,"recording",ref checks);
        Check(!state.radioAnswered && !state.reunited,"Recording cannot count as a live answer",ref checks);
        Accept(state,"radio",ref checks);
        Accept(state, "reunite", ref checks);
        Check(state.CanLeaveIsolation, "Isolation exit after reunion", ref checks);
        var checkpoint = JsonUtility.ToJson(state);
        var alone = JsonUtility.FromJson<NightRoundState>(checkpoint);
        Check(JsonUtility.ToJson(alone) == checkpoint && !ReferenceEquals(state, alone), "Independent JSON checkpoint", ref checks);
        RejectUnchanged(state, "exit", ref checks);
        Accept(state, "power-together", ref checks);
        RejectUnchanged(state, "power-together", ref checks);
        RejectUnchanged(state, "nurse-crossed", ref checks);
        Accept(state, "manual", ref checks);
        RejectUnchanged(state, "exit", ref checks);
        Accept(state, "power-alone", ref checks);
        Check(!state.manualDoor && state.CanCrossFinalDoor, "Power reversal releases manual hold", ref checks);
        Accept(state, "power-together", ref checks);
        RejectUnchanged(state, "nurse-crossed", ref checks);
        Accept(state, "manual", ref checks);
        Accept(state, "nurse-crossed", ref checks);
        RejectUnchanged(state, "nurse-crossed", ref checks);
        RejectUnchanged(state, "power-alone", ref checks);
        Check(state.CanCrossFinalDoor, "Together requires actual nurse crossing", ref checks);
        RejectUnchanged(state,"exit",ref checks);
        Accept(state,"nurse-outside",ref checks);
        Accept(state, "exit", ref checks);
        Check(state.ending == "together", "Together ending", ref checks);
        RejectUnchanged(state, "exit", ref checks);
        RejectUnchanged(state, "circuit-light", ref checks);
        Check(alone.finalPower == 0 && alone.ending == "", "Checkpoint unaffected by live state", ref checks);
        Accept(alone, "power-alone", ref checks);
        RejectUnchanged(alone, "manual", ref checks);
        RejectUnchanged(alone, "nurse-crossed", ref checks);
        RejectUnchanged(alone,"nurse-outside",ref checks);
        Accept(alone, "exit", ref checks);
        Check(alone.ending == "alone" && !alone.nurseCrossed, "Alone ending without false NPC crossing", ref checks);
        RejectUnchanged(alone, "power-together", ref checks);
        var restoredEnding = JsonUtility.FromJson<NightRoundState>(JsonUtility.ToJson(alone));
        Check(restoredEnding.ending == "alone" && restoredEnding.truth && restoredEnding.fuse, "Ending and evidence serialize", ref checks);
        Check(restoredEnding.IsActionComplete("fuse") && restoredEnding.IsActionComplete("lock-record"), "Restored save derives completed visuals", ref checks);
        Debug.Log("NightRoundStateChecks: " + checks + " checks passed.");
    }

    private static void Accept(NightRoundState state, string action, ref int checks)
    {
        string text;
        Check(state.Apply(action, out text), "Expected success: " + action, ref checks);
        Check(!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(state.Objective), "Narrative/objective: " + action, ref checks);
    }

    private static void RejectUnchanged(NightRoundState state, string action, ref int checks)
    {
        string before = JsonUtility.ToJson(state);
        string text;
        Check(!state.Apply(action, out text), "Expected rejection: " + action, ref checks);
        Check(!string.IsNullOrEmpty(text) && before == JsonUtility.ToJson(state), "Rejected action must not mutate: " + action, ref checks);
    }

    private static void Check(bool condition, string message, ref int checks)
    {
        if (!condition) throw new InvalidOperationException("NightRoundStateChecks: " + message);
        checks++;
    }
}
