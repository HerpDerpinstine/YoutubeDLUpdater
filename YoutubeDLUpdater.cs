using System;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace YoutubeDLUpdater
{
    public static class BuildInfo
    {
        public const string Name = "YoutubeDLUpdater";
        public const string Author = "Herp Derpinstine";
        public const string Company = "Lava Gang";
        public const string Version = "1.0.1";
        public const string DownloadLink = "https://github.com/HerpDerpinstine/YoutubeDLUpdater";
    }

    public class YoutubeDLUpdater : MelonMod
    {
        private static bool HasRun = false;
        private static string Version = null;

        public override void OnApplicationStart()
        {
            string filePath = Application.streamingAssetsPath + "/youtube-dl.exe";
            bool should_download = false;

            if (!File.Exists(filePath))
                should_download = true;
            else
            {
                string exeVersion = FileVersionInfo.GetVersionInfo(filePath).ProductVersion;
                UnityWebRequest githubWWW = UnityWebRequest.Get("http://api.github.com/repos/ytdl-org/youtube-dl/releases/latest");
                githubWWW.SendWebRequest();
                while (!githubWWW.isDone) ;
                if (githubWWW.responseCode == 200)
                {
                    JObject data = (JObject)JsonConvert.DeserializeObject(System.Text.Encoding.UTF8.GetString(githubWWW.downloadHandler.data));
                    string githubVersion = data["tag_name"].Value<string>();
                    if (!exeVersion.Equals(githubVersion))
                        should_download = true;
                }
            }
            if (should_download)
            {
                UnityWebRequest githubWWW = UnityWebRequest.Get("http://api.github.com/repos/ytdl-org/youtube-dl/releases/latest");
                githubWWW.SendWebRequest();
                while (!githubWWW.isDone) ;
                if (githubWWW.responseCode == 200)
                {
                    JObject data = (JObject)JsonConvert.DeserializeObject(System.Text.Encoding.UTF8.GetString(githubWWW.downloadHandler.data));
                    string githubVersion = data["tag_name"].Value<string>();

                    UnityWebRequest fileDownload = UnityWebRequest.Get("http://github.com/ytdl-org/youtube-dl/releases/download/" + githubVersion + "/youtube-dl.exe");
                    fileDownload.SendWebRequest();
                    while (!fileDownload.isDone) ;
                    if (fileDownload.responseCode == 200)
                        File.WriteAllBytes(filePath, fileDownload.downloadHandler.data);
                }
            }
            Version = GetChecksum(filePath);
        }

        public override void OnUpdate()
        {
            if (!HasRun)
            {
                YoutubeDLControl control = YoutubeDLControl.field_Internal_Static_YoutubeDLControl_0;
                if (control != null)
                {
                    control.YoutubeDLVersion = Version;
                    HasRun = true;
                }
            }
        }

        public static string GetChecksum(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum);
            }
        }
    }
}