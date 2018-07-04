using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using System.Net;

namespace MWInstaller
{
    [DataContract]
    internal class Package
    {
        [DataMember] public string name { get; set; }
        [DataMember] public string author { get; set; }
                     public bool requiresNexus { get; set; }
        [DataMember] internal string fileURL;
        [DataMember] internal string filterWhitelist;
        [DataMember] internal string[] fileBlacklist;
        [DataMember] internal string[] directoryBlacklist;
        [DataMember] internal string[] specialExtract;

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
            p.specialExtract = p.specialExtract == null ? new string[0] : p.specialExtract;
            p.requiresNexus = p.RequiresNexusAPI();

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