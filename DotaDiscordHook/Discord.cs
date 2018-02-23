using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DotaDiscordHook
{
    public static class Discord
    {

        static string _hookUrl = null;
        static string HookUrl
        {
            get
            {
                if (_hookUrl == null)
                {
                    _hookUrl = ConfigurationManager.AppSettings["DiscordWebHookUri"];
                    Uri hookUri = new Uri(HookUrl);
                }
                return _hookUrl;
            }
        }


        static List<string> DisabledTypes = new List<string>();
        static List<string> DisabledUsers = new List<string>();
        static List<string> EnabledUsers = new List<string>();
        static List<string> MangoByteUsers = new List<string>();
        static Dictionary<string, string> DiscordUserName = new Dictionary<string, string>();
        static bool enabledOnly = false;
        static bool mangoByteOnly = false;

        private static bool ProcessCommand(DotaChatHook.ChatMessage dotaChat)
        {
            if (!dotaChat.Message.StartsWith("#"))
                return false;

            var splitMessage = dotaChat.Message.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            switch (splitMessage[0])
            {
                case "#muteallchat":
                    DisabledTypes.Add("DOTA_Chat_All");
                    break;
                case "#unmuteallchat":
                    DisabledTypes.Remove("DOTA_Chat_All");
                    break;
                case "#muteteamchat":
                    DisabledTypes.Add("DOTA_Chat_Team");
                    break;
                case "#unmuteteamchat":
                    DisabledTypes.Remove("DOTA_Chat_Team");
                    break;
                case "#mute":
                    if (splitMessage.Length < 2)
                        return false;
                    DisabledUsers.Add(splitMessage[1]);
                    break;
                case "#unmute":
                    if (splitMessage.Length < 2)
                        return false;
                    DisabledUsers.Remove(splitMessage[1]);
                    EnabledUsers.Add(splitMessage[1]);
                    break;
                case "#voiceusername":
                    if (splitMessage.Length < 2)
                        return false;
                    if (splitMessage.Length < 3)
                        DiscordUserName.Add(dotaChat.Username, splitMessage[1]);
                    else
                        DiscordUserName.Add(splitMessage[1], splitMessage[2]);
                    break;
                case "#enabledonlymode":
                    enabledOnly = !enabledOnly;
                    break;
                case "#mangomode":
                    mangoByteOnly = !mangoByteOnly;
                    break;
                case "#mango":
                    if (splitMessage.Length < 2)
                        return false;
                    MangoByteUsers.Add(splitMessage[1]);
                    break;
                case "#unmango":
                    if (splitMessage.Length < 2)
                        return false;
                    MangoByteUsers.Remove(splitMessage[1]);
                    break;
                case "#pure":
                    if (splitMessage.Length < 2)
                        return false;
                    var discordMessage = new DiscordMessage() { content = splitMessage[1], tts = false, username = dotaChat.Username};
                    Send(discordMessage);
                    break;
                case "#clear":
                    DisabledTypes.Clear();
                    DisabledUsers.Clear();
                    EnabledUsers.Clear();
                    MangoByteUsers.Clear();
                    DiscordUserName.Clear();
                    enabledOnly = false;
                    mangoByteOnly = false;
                    break;
                default:
                    return false;
            }

            return true;
        }

        public static bool DoNotSend(DotaChatHook.ChatMessage dotaChat)
        {
            if (!enabledOnly)
            {
                if (DisabledTypes.Contains(dotaChat.Type))
                    return true;
                if (DisabledUsers.Contains(dotaChat.Username))
                    return true;
                return false;
            }
            else
            {
                if (EnabledUsers.Contains(dotaChat.Username))
                    return false;
                return true;
            }
            
        }

        public static DiscordMessage ModifySend(DotaChatHook.ChatMessage dotaChat)
        {
            string username = (DiscordUserName.ContainsKey(dotaChat.Username) ? DiscordUserName[dotaChat.Username] : dotaChat.Username);
            if (MangoByteUsers.Contains(dotaChat.Username) || mangoByteOnly)
            {
                string mangoMessage = "?smarttts " + (dotaChat.Message.StartsWith("?") ? dotaChat.Message.Substring(1) : username + " said " + dotaChat.Message);
                return new DiscordMessage() { content = mangoMessage, tts = false, username = dotaChat.Username };
            }
            else
                return new DiscordMessage() { content = dotaChat.Message, tts = true, username = username };
        }

        public static void Send(DotaChatHook.ChatMessage dotaChat)
        {
            if (ProcessCommand(dotaChat))
                return;

            if (DoNotSend(dotaChat))
                return;

            var discordmessage = ModifySend(dotaChat);
            Send(discordmessage);
        }

        public static void Send(DiscordMessage discordmessage)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 1, 0, 0);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var body = new StringContent(Jil.JSON.Serialize<DiscordMessage>(discordmessage));
                body.Headers.ContentType.MediaType = "application/json";
                client.PostAsync(HookUrl, body).Wait();
            }
        }

    }
}
