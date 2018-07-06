using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

//GET API KEY FROM https://www.nexusmods.com/users/myaccount?tab=api+access
//LOOK AT API REQUESTS AT https://github.com/Nexus-Mods/node-nexus-api/tree/master/samples

namespace MWInstaller
{
    static class Nexus
    {
        static public string apiKey;

        static public bool ValidateAPIKey(string key)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://api.nexusmods.com/v1/users/validate");
            req.ContentType = "application/json";
            req.Headers.Add("APIKEY", key);

            try
            {
                using(HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                using(Stream stream = response.GetResponseStream())
                using(StreamReader reader = new StreamReader(stream))
                {
                    apiKey = key;
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        static public string GetNexusDownloadURL(string url)
        {
            string downloadURL;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.ContentType = "application/json";
            req.Headers.Add("APIKEY", apiKey);
            using(HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            using(Stream stream = response.GetResponseStream())
            using(StreamReader reader = new StreamReader(stream))
            {
                string s = reader.ReadToEnd();
                downloadURL = Regex.Match(s, @"""URI"":""(?<url>.+)""").Groups["url"].Value;
            }

            return Regex.Unescape(downloadURL);
        }

        //from https://www.nexusmods.com/morrowind/mods/45712
        //to https://api.nexusmods.com/v1/games/morrowind/mods/45712
        static public string NexusURLtoAPI(string url)
        {
            var match = Regex.Match(url, "nexusmods.com/(morrowind/mods/[0-9]+)");
            return "https://api.nexusmods.com/v1/games/" + match.Groups[1].Value;
        }

        static public string GetNexusFileList(string url)
        {
            url += "/files";
            string files = "it didnt work fuck";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.ContentType = "application/json";
            req.Headers.Add("APIKEY", apiKey);
            using(HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            using(Stream stream = response.GetResponseStream())
            using(StreamReader reader = new StreamReader(stream))
            {
                string s = reader.ReadToEnd();
                files = s;
            }

            return files;
        }
    }

    [DataContract]
    internal class NexusFileList
    {
        [DataMember] public NexusFiles[] files;

        public static NexusFileList Deserialize(string json)
        {
            var list = new NexusFileList();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var settings = new DataContractJsonSerializerSettings();
            settings.UseSimpleDictionaryFormat = true;
            var serializer = new DataContractJsonSerializer(list.GetType(), settings);
            list = serializer.ReadObject(ms) as NexusFileList;
            ms.Close();

            return list;
        }
    }

    [DataContract]
    internal class NexusFiles
    {
        [DataMember] public string file_id { get; set; }
        [DataMember] public string version { get; set; }
        [DataMember] public string name { get; set; }
        [DataMember] public string file_name { get; set; }

        public static NexusFiles Deserialize(string json)
        {
            var files = new NexusFiles();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var settings = new DataContractJsonSerializerSettings();
            settings.UseSimpleDictionaryFormat = true;
            var serializer = new DataContractJsonSerializer(files.GetType(), settings);
            files = serializer.ReadObject(ms) as NexusFiles;
            ms.Close();

            return files;
        }
    }
}