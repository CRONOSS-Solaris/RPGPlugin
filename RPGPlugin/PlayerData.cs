using System.IO;
using System.Xml.Serialization;

public class PlayerData
{
    [XmlAttribute("SteamId")]
    public ulong SteamId { get; set; }

    [XmlElement("SelectedRole")]
    public string SelectedRole { get; set; }

    // Dodaj właściwości przechowujące doświadczenie i poziom.
    [XmlElement("Exp")]
    public int Exp { get; set; }

    [XmlElement("Level")]
    public int Level { get; set; }

    // Domyślny konstruktor.
    public PlayerData() { }

    // Konstruktor z wartościami początkowymi dla Level i Exp.
    public PlayerData(ulong steamId)
    {
        SteamId = steamId;
        SelectedRole = null;
        Level = 1;
        Exp = 0;
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
