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
        [DataMember(IsRequired = true)] public string name { get; set; }
        [DataMember(IsRequired = true)] public string author { get; set; }
        [DataMember(IsRequired = true)] internal string fileURL;
        [DataMember] internal string filterWhitelist;
        [DataMember] internal string[] fileBlacklist;
        [DataMember] internal string[] directoryBlacklist;
        [DataMember(Name = "specialExtract")] internal Dictionary<string, string> specialExtract;

        public bool requiresNexus { get; set; }
        public bool malformed { get; set; } = false;
        internal string fileName;
        internal string url;
        internal PackageList list;

        public static string CreatePackageString(string name, string author, string fileURL)
        {
            return string.Format("{{\n    \"name\": \"{0}\",\n    \"author\": \"{1}\",\n    \"fileURL\": \"{2}\"\n}}", name, author, fileURL);
        }

        public static Package Deserialize(string url, string json, PackageList list)
        {
            var p = new Package();
            try
            {
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
                var settings = new DataContractJsonSerializerSettings
                {
                    UseSimpleDictionaryFormat = true
                };

                var serializer = new DataContractJsonSerializer(typeof(Package), settings);
                p = serializer.ReadObject(ms) as Package;
                ms.Close();

                p.fileBlacklist = p.fileBlacklist == null ? new string[0] : p.fileBlacklist;
                p.directoryBlacklist = p.directoryBlacklist == null ? new string[0] : p.directoryBlacklist;
                p.specialExtract = p.specialExtract == null ? new Dictionary<string, string>(0) : p.specialExtract;
                p.requiresNexus = p.RequiresNexusAPI();
                p.fileName = Path.GetFileName(p.fileURL);
                p.url = url;
                p.list = list;  //TODO

                return p;
            }
            catch(System.Exception e)
            {
                Log.Write(e);
                return GetErrorPackage(Path.GetFileName(url));
            }
        }

        private bool RequiresNexusAPI()
        {
            if(fileURL.Contains("nexusmods.com"))
                return true;

            return false;
        }

        private static Package GetErrorPackage(string url)
        {
            var p = new Package();
            p.name = "Malformed JSON in: " + url;
            p.url = url;
            p.malformed = true;
            return p;
        }
    }
}