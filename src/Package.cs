using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace MWInstaller
{
    [DataContract]
    internal class Package
    {
        [DataMember] public string name { get; set; }
        [DataMember] public string author { get; set; }
        [DataMember] internal string fileURL;
        [DataMember] internal string filterWhitelist;
        [DataMember] internal string[] fileBlacklist;
        [DataMember] internal string[] directoryBlacklist;
        [DataMember(Name = "specialExtract")] internal Dictionary<string, string> specialExtract;

        public bool requiresNexus { get; set; }
        internal string fileName;

        public static Package Deserialize(string json)
        {
            var p = new Package();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var settings = new DataContractJsonSerializerSettings();
            settings.UseSimpleDictionaryFormat = true;
            var serializer = new DataContractJsonSerializer(p.GetType(), settings);
            p = serializer.ReadObject(ms) as Package;
            ms.Close();
            p.fileBlacklist = p.fileBlacklist == null ? new string[0] : p.fileBlacklist;
            p.directoryBlacklist = p.directoryBlacklist == null ? new string[0] : p.directoryBlacklist;
            p.specialExtract = p.specialExtract == null ? new Dictionary<string, string>(0) : p.specialExtract;
            p.requiresNexus = p.RequiresNexusAPI();
            p.fileName = Path.GetFileName(p.fileURL);

            return p;
        }

        private bool RequiresNexusAPI()
        {
            if(fileURL.Contains("nexusmods.com"))
                return true;

            return false;
        }
    }
}