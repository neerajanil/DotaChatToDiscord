using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Discord.WebSocket;

namespace DotaDiscordHook
{
    public class User
    {
        public User()
        {
            MangoMode = true;
            LastUsed = DateTime.MinValue;
            Admin = false;
        }

        public string Name { get; set; }
        public bool MangoMode { get; set; }
        public DateTime LastUsed { get; set; }
        public bool Admin { get; set; }
    }

    public static class DiscordWrapper
    {
        
        public static Dictionary<string, User> EnabledUsers = new Dictionary<string, User>();
        public static bool AlliesEnabled = false;
        public static bool AllEnabled = false;

        private static bool CheckTime(User user)
        {
            return user.LastUsed < DateTime.Now.AddSeconds(-10);
        }

        private static bool ProcessChat(DotaChatHook.ChatMessage dotaChat)
        {
            var user = EnabledUsers[dotaChat.Username];

            

            if (dotaChat.Message.StartsWith("@"))
            {
                if (!CheckTime(user) && user.Admin == false)
                {
                    return true;
                }
                string message = dotaChat.Message.Substring(1);
                var discordMessage = new DiscordMessage() { content = message, tts = false, username = dotaChat.Username };
                user.LastUsed = DateTime.Now;
                Send(discordMessage);
                return true;
            }

            if (dotaChat.Message.StartsWith("!"))
            {
                if (!CheckTime(user))
                {
                    return true;
                }
                user.LastUsed = DateTime.Now;
                string message = dotaChat.Message.Substring(1);
                if (EnabledUsers[dotaChat.Username].MangoMode)
                {
                    var discordMessage = new DiscordMessage() { content = "?smarttts " + message, tts = false, username = dotaChat.Username };
                    Send(discordMessage);
                }
                else
                {
                    var discordMessage = new DiscordMessage() { content = message, tts = true, username = dotaChat.Username };
                    Send(discordMessage);
                }
                return true;
            }

            if (dotaChat.Message.StartsWith("~"))
            {
                if (!CheckTime(user))
                {
                    return true;
                }
                user.LastUsed = DateTime.Now;
                string message = dotaChat.Message.Substring(1);
                var discordMessage = new DiscordMessage() { content = "?dota " + message, tts = false, username = dotaChat.Username };
                Send(discordMessage);
                return true;
            }


            if (dotaChat.Message.StartsWith("#") && user.Admin == true)
            {
                try
                {

                    var splitMessage = dotaChat.Message.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                    switch (splitMessage[0])
                    {
                        case "#mute":
                            if (splitMessage.Length < 2)
                                return false;
                            if (splitMessage[1] == "myteam")
                                AlliesEnabled = true;
                            else if (splitMessage[1] == "enemyteam")
                                AllEnabled = true;
                            else
                                EnabledUsers.Remove(splitMessage[1]);
                            break;

                        case "#unmute":
                            if (splitMessage.Length < 2)
                                return false;
                            if (splitMessage[1] == "myteam")
                                AlliesEnabled = false;
                            else if (splitMessage[1] == "enemyteam")
                                AllEnabled = false;
                            else
                                EnabledUsers.Add(splitMessage[1], new User { Name = splitMessage[1] });
                            break;
                        case "#mango":
                            if (splitMessage.Length < 2)
                                return false;
                            EnabledUsers[splitMessage[1]].MangoMode = true;
                            break;

                        case "#unmango":
                            if (splitMessage.Length < 2)
                                return false;
                            EnabledUsers[splitMessage[1]].MangoMode = false;
                            break;
                        default:
                            return false;
                    }
                    return true;
                }
                catch
                {

                }
            }

            return false;
        }

        public static bool DoNotSend(DotaChatHook.ChatMessage dotaChat)
        {
            if (dotaChat.Type == "DOTA_Chat_All" && AllEnabled == true)
                return false;

            if (dotaChat.Type == "DOTA_Chat_Team" && AlliesEnabled == true)
                return false;

            if (EnabledUsers.ContainsKey(dotaChat.Username))
                return false;
            else
                return true;

        }

        public static void Send(DotaChatHook.ChatMessage dotaChat)
        {
            if (DoNotSend(dotaChat))
                return;

            ProcessChat(dotaChat);
        }

        public static void Send(DiscordMessage discordmessage)
        {
            if (Config.UserConnectMode == false)
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 1, 0, 0);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var body = new StringContent(Jil.JSON.Serialize<DiscordMessage>(discordmessage));
                    body.Headers.ContentType.MediaType = "application/json";
                    client.PostAsync(Config.HookUrl, body).Wait();
                }
            }
            else
            {
                if (_client.ConnectionState == Discord.ConnectionState.Connected)
                {
                    if (serverChannel == null)
                    {
                        serverChannel = _client.GetGuild(Config.ServerId).GetTextChannel(Config.TextChannelId);

                        var voiceChannel = _client.GetGuild(Config.ServerId).GetVoiceChannel(Config.VoiceChannelId);
                        var s = voiceChannel.ConnectAsync().Result;

                    }

                    var result = serverChannel.SendMessageAsync(discordmessage.content, discordmessage.tts, null).Result;
                }
            }
        }

        public static DiscordSocketClient _client = null;
        public static SocketTextChannel serverChannel = null;

    }
}
