using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class MedalSystem : MonoBehaviour
{
    [Header("Medal Settings")]
    public float rapidKillWindow = 3f; // time allowed between kills for double/triple kill etc.
    private Dictionary<string, List<float>> killTimestamps = new Dictionary<string, List<float>>();
    public Transform medalContainer;
    public float showDuration = 1;
    public GameObject medalPrefab;

    private void OnEnable()
    {
        GameEvents.OnPlayerKilled += HandlePlayerKilled;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerKilled -= HandlePlayerKilled;
    }

    private void HandlePlayerKilled(PlayerData killer, PlayerData victim)
    {
        if (killer == null || victim == null) return;

        if (!killTimestamps.ContainsKey(killer.playerId))
            killTimestamps[killer.playerId] = new List<float>();

        var timestamps = killTimestamps[killer.playerId];
        timestamps.Add(Time.time);

        // Remove old kills outside the rapid window
        timestamps.RemoveAll(t => Time.time - t > rapidKillWindow);

        int rapidKills = timestamps.Count;

        // medal rewards
        switch (rapidKills)
        {
            case 2:
                TriggerMedal(killer, "Double Kill");
                break;
            case 3:
                TriggerMedal(killer, "Triple Kill");
                break;
            case 4:
                TriggerMedal(killer, "Fury Kill");
                break;
            case 5:
                TriggerMedal(killer, "Five Not Alive");
                break;
        }

        // Kill streaks
        killer.currentKillStreak++;

        if (killer.currentKillStreak == 5)
            TriggerMedal(killer, "Bloodthirsty");
        if (killer.currentKillStreak == 10)
            TriggerMedal(killer, "Merciless");

        // Kingslayer (example: top player has highest score)
        /*if (victim.playerName == "TopPlayer") // placeholder condition
            TriggerMedal(killer, "Kingslayer");*/
    }

    private void TriggerMedal(PlayerData player, string medalName)
    {
        Debug.Log($"{player.playerId} earned medal: {medalName}");
        GameObject medal = Instantiate(medalPrefab, medalContainer);

        medal.GetComponentInChildren<TextMeshProUGUI>().text = medalName;

        medal.GetComponent<CanvasGroup>().DOFade(0, 0.3f)
            .SetDelay(showDuration)
            .OnComplete(() =>
            {
                Destroy(medal);
            });
    }
}
