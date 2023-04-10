using System.Collections.Generic;
using System.Reflection;
using RPGPlugin;
using Sandbox.Game.World;

public class PlayerData
{
    public ulong SteamId { get; set; }
    public long PlayerID { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public RoleManager.FromRoles SelectedRole { get; set; }
    public List<int> ExpPerLevel { get; set; }

    public PlayerData(ulong steamId)
    {
        SteamId = steamId;
        Level = 1;
        Exp = 0;
        SelectedRole = RoleManager.FromRoles.NoRole;
        ExpPerLevel = new List<int> { 0, 100, 200, 400, 700, 1200 }; // Przykładowe wartości, można dostosować według potrzeb
    }
}
