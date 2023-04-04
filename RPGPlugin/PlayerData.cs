using System.IO;
using System.Xml.Serialization;

public class PlayerData
{
    [XmlAttribute("SteamId")]
    public ulong SteamId { get; set; }

    [XmlElement("SelectedRole")]
    public string SelectedRole { get; set; }
     
    [XmlElement("Exp")]
    public int Exp { get; set; }

    [XmlElement("Level")]
    public int Level { get; set; }

    public PlayerData() { }

    public PlayerData(ulong steamId)
    {
        SteamId = steamId;
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
