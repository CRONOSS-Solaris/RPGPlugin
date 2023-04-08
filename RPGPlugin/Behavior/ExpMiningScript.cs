using RPGPlugin;
using Sandbox.ModAPI;
using System;
using System.IO;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
public class ExpMiningScript : MySessionComponentBase
{
    private bool _isInitialized;
    private MemoryStream _stream;
    private BinaryWriter _writer;

    public ExpMiningScript()
    {
        _stream = new MemoryStream();
        _writer = new BinaryWriter(_stream, Encoding.UTF8);
    }

    public override void UpdateBeforeSimulation()
    {
        base.UpdateBeforeSimulation();

        if (!_isInitialized)
        {
            if (MyAPIGateway.Session?.Player == null)
            {
                return;
            }

            MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
            _isInitialized = true;
        }
    }

    protected override void UnloadData()
    {
        MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;

        _writer.Dispose();
        _stream.Dispose();
    }

    private void OnEntityAdd(IMyEntity entity)
    {
        if (entity is IMyFloatingObject floatingObject)
        {
            IMyInventoryItem inventoryItem = (IMyInventoryItem)floatingObject.Components;
            if (inventoryItem != null && inventoryItem.Content.TypeId.ToString() == "MyObjectBuilder_Ore")
            {
                IMyPlayer localPlayer = MyAPIGateway.Session.LocalHumanPlayer;
                if (localPlayer == null)
                {
                    return;
                }

                // Sprawdź, czy gracz jest w pobliżu obiektu
                double distanceSquared = Vector3D.DistanceSquared(localPlayer.GetPosition(), entity.GetPosition());
                double maxDistanceSquared = 100; // Ustal maksymalny dystans, na jakim gracz może zbierać surowce (np. 10 metrów)
                if (distanceSquared <= maxDistanceSquared)
                {
                    SendExpMiningData(inventoryItem.Content.TypeId.ToString(), inventoryItem.Content.SubtypeName, (int)inventoryItem.Amount);
                }
            }
        }
    }

    private void SendExpMiningData(string typeId, string subtypeId, int amount)
    {
        try
        {
            _stream.Position = 0;
            _writer.Write(typeId);
            _writer.Write(subtypeId);
            _writer.Write(amount);

            MyAPIGateway.Multiplayer.SendMessageToServer(Roles.ExpMiningHandlerId, _stream.ToArray());
        }
        catch (Exception e)
        {
            Roles.Log.Warn(e);
        }
    }
}
