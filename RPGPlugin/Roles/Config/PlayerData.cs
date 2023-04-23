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
    public int HunterLevel { get; set; }
    public double HunterExp { get; set; }
    public long AttackerId { get; set; }

    //test skill point system
    public int MinerSkillPoints { get; set; }
    public PlayerManager.FromRoles SelectedRole { get; set; }

    public PlayerData() {}

    public void CreateNew(ulong steamId, long playerId)
    {
        SteamId = steamId;
        PlayerID = playerId;
        MinerLevel = 1;
        MinerExp = 0.0;
        WarriorLevel = 1;
        WarriorExp = 0.0;
        HunterLevel = 1;
        HunterExp = 0;
        
        //test skill point system
        MinerSkillPoints = 0;

        SelectedRole = PlayerManager.FromRoles.NoRole;
    }
}
