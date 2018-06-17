using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

[DataContract]
internal class Package
{
    [DataMember] internal string name;
    [DataMember] internal string author;
    [DataMember] internal string fileURL;
    [DataMember] internal bool skimESPs;
    [DataMember] internal bool rebaseDataFiles; //unimplemented
    [DataMember] internal string filterWhitelist;
    [DataMember] internal string[] fileBlacklist;
    [DataMember] internal string[] directoryBlacklist;

    internal string fileName;

    public static Package Deserialize(string json)
    {
        var p = new Package();
        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var serializer = new DataContractJsonSerializer(p.GetType());
        p = serializer.ReadObject(ms) as Package;
        ms.Close();
        p.fileName = Path.GetFileName(p.fileURL);
        p.fileBlacklist = p.fileBlacklist == null ? new string[0] : p.fileBlacklist;
        p.directoryBlacklist = p.directoryBlacklist == null ? new string[0] : p.directoryBlacklist;

        return p;
    }
}

[DataContract]
internal class PackageList
{
    [DataMember] internal string name;
    [DataMember] internal string curator;
    [DataMember] internal string lastUpdated;
    [DataMember] internal string description;
    [DataMember] internal string[] packages;

    // TODO: generic deserialization?
    public static PackageList Deserialize(string json)
    {
        var p = new PackageList();
        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var serializer = new DataContractJsonSerializer(p.GetType());
        p = serializer.ReadObject(ms) as PackageList;
        ms.Close();
        return p;
    }
}