using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;

//GET API KEY FROM https://www.nexusmods.com/users/myaccount?tab=api+access
//LOOK AT API REQUESTS AT https://github.com/Nexus-Mods/node-nexus-api/tree/master/samples

public class Nexus
{
    static public void TestRequest()
    {
        Console.WriteLine("You can get your API key from: nexusmods.com/users/myaccount?tab=api+access");
        Console.WriteLine("Copy your Nexus API key to clipboard, and then press Y.\n");

        ConsoleKeyInfo key = new ConsoleKeyInfo();
        while(key.KeyChar != char.Parse("y"))
            key = Console.ReadKey(true);

        string apiKey = Clipboard.GetText();

        string downloadURL;

        HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://api.nexusmods.com/v1/games/morrowind/mods/43657/files/1000003873/download_link");
        req.ContentType = "application/json";
        req.Headers.Add("APIKEY", apiKey);
        using(HttpWebResponse response = (HttpWebResponse)req.GetResponse())
        using(Stream stream = response.GetResponseStream())
        using(StreamReader reader = new StreamReader(stream))
        {
            string s = reader.ReadToEnd();
            downloadURL = Regex.Match(s, @"""URI"":""(?<url>.+)""").Groups["url"].Value;
        }

        downloadURL = Regex.Unescape(downloadURL);
        Console.WriteLine(downloadURL);
        Clipboard.SetText(downloadURL);
        Console.WriteLine("Copied to clipboard.");
        Installer.Exit();
    }
}