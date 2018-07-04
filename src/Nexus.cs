using System.IO;
using System.Net;
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

        static public string GetNexusDownloadURL(string url, string key)
        {
            string downloadURL;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.ContentType = "application/json";
            req.Headers.Add("APIKEY", key);
            using(HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            using(Stream stream = response.GetResponseStream())
            using(StreamReader reader = new StreamReader(stream))
            {
                string s = reader.ReadToEnd();
                downloadURL = Regex.Match(s, @"""URI"":""(?<url>.+)""").Groups["url"].Value;
            }

            return Regex.Unescape(downloadURL);
        }
    }
}