using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Harmony;

namespace YoutubeDLUpdater
{
    public static class BuildInfo
    {
        public const string Name = "YoutubeDLUpdater";
        public const string Author = "Herp Derpinstine";
        public const string Company = "Lava Gang";
        public const string Version = "1.0.2";
        public const string DownloadLink = "https://github.com/HerpDerpinstine/YoutubeDLUpdater";
    }

    public class YoutubeDLUpdater : MelonMod
    {
        private static HarmonyInstance harmonyInstance = null;

        public override void OnApplicationStart()
        {
            harmonyInstance = HarmonyInstance.Create("YoutubeDLUpdater");
            harmonyInstance.Patch(typeof(YoutubeDLControl).GetMethod("Method_Public_IEnumerator_7", BindingFlags.Public | BindingFlags.Instance), new HarmonyMethod(typeof(YoutubeDLUpdater).GetMethod("YoutubeDLControl_Method_Public_IEnumerator_7", BindingFlags.NonPublic | BindingFlags.Static)));
        }

        private static bool YoutubeDLControl_Method_Public_IEnumerator_7(YoutubeDLControl __instance)
        {
            string filePath = Application.streamingAssetsPath + "/youtube-dl.exe";
            string exeVersion = null;
            if (File.Exists(filePath))
                exeVersion = FileVersionInfo.GetVersionInfo(filePath).ProductVersion;
            UnityWebRequest githubWWW = UnityWebRequest.Get("http://api.github.com/repos/ytdl-org/youtube-dl/releases/latest");
            githubWWW.SendWebRequest();
            while (!githubWWW.isDone) ;
            if (githubWWW.responseCode == 200)
            {
                JObject data = (JObject)JsonConvert.DeserializeObject(System.Text.Encoding.UTF8.GetString(githubWWW.downloadHandler.data));
                string githubVersion = data["tag_name"].Value<string>();
                if (string.IsNullOrEmpty(exeVersion) || (!string.IsNullOrEmpty(githubVersion) && !exeVersion.Equals(githubVersion)))
                {
                    UnityWebRequest fileDownload = UnityWebRequest.Get("http://github.com/ytdl-org/youtube-dl/releases/download/" + githubVersion + "/youtube-dl.exe");
                    fileDownload.SendWebRequest();
                    while (!fileDownload.isDone) ;
                    if (fileDownload.responseCode == 200)
                        File.WriteAllBytes(filePath, fileDownload.downloadHandler.data);
                }
            }
            __instance.YoutubeDLVersion = GetChecksum(filePath);
            return true;
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