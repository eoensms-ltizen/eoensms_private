using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

public class UserData : ScriptableObject
{
    public User m_user;
    
    public void SaveToFile(string _filePath)
    {
        var serializer = new XmlSerializer(typeof(UserData));
        var stream = new FileStream(_filePath, FileMode.Create);
        StreamWriter sw = new StreamWriter(stream, Encoding.UTF8);

        serializer.Serialize(sw, this);
        stream.Close();
    }

    public string GetXmlString()
    {
        return MUtilsSerialize.Serialize<UserData>(this);
    }
}
