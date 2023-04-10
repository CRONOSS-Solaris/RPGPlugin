using RPGPlugin;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using Sandbox.Game.World;

public static class ExpMiningManager
{
    public static void OnEntityAdd(IMyEntity entity)
    {
        if (!(entity is IMyFloatingObject floatingObject)) return;
        
        IMyInventoryItem inventoryItem = floatingObject.Components as IMyInventoryItem;
        if (inventoryItem == null || inventoryItem.Content.TypeId.ToString() != "MyObjectBuilder_Ore") return;

        // Reward the closest player, when more than one are near each other.
        double distance = 1000000;
        // Player closest after search
        MyPlayer rewardThisPlayer = null;
        foreach (KeyValuePair<ulong,MyPlayer> player in Roles.OnlinePlayersList)
        {
            // Sprawdź, czy gracz jest w pobliżu obiektu
            double distanceSquared = Vector3D.DistanceSquared(player.Value.GetPosition(), entity.GetPosition());
            double maxDistanceSquared = 100; // Ustal maksymalny dystans, na jakim gracz może zbierać surowce (np. 10 metrów)
            if (distanceSquared > maxDistanceSquared) continue;
            if (distanceSquared > distance)continue;
            distance = distanceSquared;
            rewardThisPlayer = player.Value;
        }
        
        if (rewardThisPlayer == null) return;

        UpdateExpMiningData(rewardThisPlayer.Id.SteamId, inventoryItem.Content.SubtypeName, (double)inventoryItem.Amount);
    }

    private static void UpdateExpMiningData(ulong steamID, string subtypeId, double amount)
    {
        // I would assume that different ores reward different amounts of experience.  Uranium should
        // reward more than Stone... correct?  Get the value and multiply by amount.  This would
        // probably be better in the shim/patch needed to get the ore date from the drills.  Otherwise
        // players could rip the system off with ejectors as they would spawn the entities on ejection.
        
        Dictionary<string, double> ExpRatio = new Dictionary<string, double>()
        {
            ["Stone"] = 0.2,
            ["Silicon"] = 0.3,
            ["Iron"] = 0.3,
            ["Nickel"] = 0.3,
            ["Cobalt"] = 0.4,
            ["Magnesium"] = 0.4,
            ["Silver"] = 0.5,
            ["Gold"] = 0.5,
            ["Platinum"] = 0.55,
            ["Uranium"] = 0.6,
            ["Ice"] = 0.35
        };

        foreach (RoleManager manager in Roles.PlayerManagers)
        {
            
            
        }
    }
}
