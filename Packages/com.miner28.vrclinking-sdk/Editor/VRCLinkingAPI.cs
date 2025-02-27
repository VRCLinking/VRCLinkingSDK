using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;

namespace VRCLinking
{
    public static class VRCLinkingAPI
    {
        private const string ApiBaseUrl = "http://localhost:7720/";
        private static readonly HttpClient Client = new HttpClient();

        static VRCLinkingAPI()
        {
            LoadSession();
        }

        private static void LoadSession()
        {
            var session = EditorPrefs.GetString("VRCLinking_Session", "");
            if (!string.IsNullOrEmpty(session))
            {
                Client.DefaultRequestHeaders.Clear();
                Client.DefaultRequestHeaders.Add("Cookie", $"session={session}");
            }
        }

        private static bool EnsureSessionLoaded()
        {
            var session = EditorPrefs.GetString("VRCLinking_Session", "");
            if (string.IsNullOrEmpty(session))
            {
                Console.WriteLine("No session found. Please log in first.");
                return false;
            }

            // Ensure headers are set
            LoadSession();
            return true;
        }

        public static async Task<List<Guild>> GetUserGuildsAsync()
        {
            if (!EnsureSessionLoaded())
            {
                return new List<Guild>();
            }

            try
            {
                var response = await Client.GetStringAsync($"{ApiBaseUrl}guilds");
                var guildsResponse = JsonConvert.DeserializeObject<GuildsResponse>(response);
                return guildsResponse.Guilds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching guilds: {ex.Message}");
                return new List<Guild>();
            }
        }

        [Serializable]
        private class GuildsResponse
        {
            [JsonProperty("guilds")]
            public List<Guild> Guilds { get; set; }
        }

        [Serializable]
        public class Guild
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("icon")]
            public string Icon { get; set; }
        }
    }
}
