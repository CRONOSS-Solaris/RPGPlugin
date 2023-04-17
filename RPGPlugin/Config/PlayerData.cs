using System;
using System.Collections.Generic;
using RPGPlugin;
using Sandbox.Game.GameSystems.Conveyors;

public class PlayerData
{
    public ulong SteamId { get; set; }
    public long PlayerID { get; set; }
    public int MinerLevel { get; set; }
    public double MinerExp { get; set; }
    public int WarriorLevel { get; set; }
    public double WarriorExp { get; set; }
    public long AttackerId { get; set; }
    public PlayerManager.FromRoles SelectedRole { get; set; }

    public PlayerData() {}

    public void CreateNew(ulong steamId, long playerId, long attackerId)
    {
        SteamId = steamId;
        PlayerID = playerId;
        AttackerId = attackerId;
        MinerLevel = 1;
        MinerExp = 0.0;
        WarriorLevel = 1;
        WarriorExp = 0.0;
        SelectedRole = PlayerManager.FromRoles.NoRole;
    }
}
