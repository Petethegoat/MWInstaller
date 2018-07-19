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
        [DataMember(IsRequired = true)] internal string name;
        [DataMember(IsRequired = true)] internal string curator;
        [DataMember] internal string lastUpdated;
        [DataMember] internal string description;
        [DataMember(IsRequired = true)] internal string[] packages;

        public static PackageList Deserialize(string json)
        {
            var p = new PackageList();
            try
            {
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));

                var serializer = new DataContractJsonSerializer(typeof(PackageList));
                p = serializer.ReadObject(ms) as PackageList;
                ms.Close();
                return p;
            }
            catch(System.Exception e)
            {
                Log.Write(e);
                return null;
            }
        }

        public List<Package> GetPackages()
        {
            var paks = new List<Package>();

            var webClient = new WebClient();
            foreach(string s in packages)
            {
                try
                {
                    paks.Add(Package.Deserialize(s, webClient.DownloadString(s), this));
                }
                catch(System.Exception e)
                {
                    Log.Write(e);
                    return null;
                }
            }

            return paks;
        }
    }
}
