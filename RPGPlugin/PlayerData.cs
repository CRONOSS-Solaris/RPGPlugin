using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public class PlayerData
{
    [XmlAttribute("SteamId")]
    public ulong SteamId { get; set; }

    [XmlElement("Level")]
    public int Level { get; set; }

    [XmlElement("Exp")]
    public int Exp { get; set; }

    [XmlElement("SelectedRole")]
    public string SelectedRole { get; set; }

    public List<int> ExpPerLevel { get; set; }

    public PlayerData(ulong steamId)
    {
        SteamId = steamId;
        Level = 1;
        Exp = 0;
        SelectedRole = null;
        ExpPerLevel = new List<int> { 0, 100, 200, 400, 700, 1200 }; // Przykładowe wartości, można dostosować według potrzeb
    }

    public void Save(string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        {
            var serializer = new XmlSerializer(typeof(PlayerData));
            serializer.Serialize(writer, this);
        }
    }
}


