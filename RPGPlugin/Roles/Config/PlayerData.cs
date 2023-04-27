using System;
using System.Collections.Generic;

public class PlayerData
{
    public ulong SteamId { get; set; }
    public long PlayerID { get; set; }
    public Dictionary<string, Tuple<int, double>> ClassInfo = new Dictionary<string, Tuple<int, double>>();  // CLASS, <Level, Exp>
    public string SelectedRole { get; set; }

    public PlayerData() {}

    public void CreateNew(ulong steamId, long playerId)
    {
        SteamId = steamId;
        PlayerID = playerId;
    }
}
