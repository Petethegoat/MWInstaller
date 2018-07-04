using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using System.Net;

namespace MWInstaller
{
    [DataContract]
    internal class PackageList
    {
        [DataMember] internal string name;
        [DataMember] internal string curator;
        [DataMember] internal string lastUpdated;
        [DataMember] internal string description;
        [DataMember] internal string[] packages;

        public static PackageList Deserialize(string json)
        {
            var p = new PackageList();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var serializer = new DataContractJsonSerializer(p.GetType());
            p = serializer.ReadObject(ms) as PackageList;
            ms.Close();
            return p;
        }

        public List<Package> GetPackages()
        {
            var paks = new List<Package>();

            var webClient = new WebClient();
            foreach(string s in packages)
            {
                paks.Add(Package.Deserialize(webClient.DownloadString(s)));
            }

            return paks;
        }
    }
}
