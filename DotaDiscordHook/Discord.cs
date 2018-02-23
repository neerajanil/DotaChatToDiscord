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
        static bool enabledOnly = false;

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
                case "#muteteamchat":
                    DisabledTypes.Add("DOTA_Chat_Team");
                    break;
                case "#mute":
                    if (splitMessage.Length < 2)
                        return false;
                    DisabledUsers.Add(splitMessage[1]);
                    break;
                case "#unmute":
                    if (splitMessage.Length < 2)
                        return false;
                    EnabledUsers.Add(splitMessage[1]);
                    break;
                case "#enabledonlymode":
                    enabledOnly = true;
                    break;
                case "#clear":
                    DisabledTypes.Clear();
                    DisabledUsers.Clear();
                    EnabledUsers.Clear();
                    enabledOnly = false;
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

        public static void Send(DotaChatHook.ChatMessage dotaChat)
        {
            if (ProcessCommand(dotaChat))
                return;

            if (DoNotSend(dotaChat))
                return;

            var discordmessage = new DiscordMessage() { content = dotaChat.Message, tts = true, username = dotaChat.Username };
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
