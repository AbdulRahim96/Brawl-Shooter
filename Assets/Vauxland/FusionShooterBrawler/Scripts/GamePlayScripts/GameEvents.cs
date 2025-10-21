using System;
using UnityEngine;

public static class GameEvents
{
    // Triggered when a player kills another player
    public static Action<PlayerData> OnPlayerKilled;

}
