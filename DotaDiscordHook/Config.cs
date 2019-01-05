using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotaDiscordHook
{
    public static class Config
    {
        static string _hookUrl = null;
        public static string HookUrl
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

        public static ulong ServerId
        {
            get
            {
                string serverId = ConfigurationManager.AppSettings["ServerId"];
                return Convert.ToUInt64(serverId ?? "0");
            }
        }

        public static ulong TextChannelId
        {
            get
            {
                string serverId = ConfigurationManager.AppSettings["TextChannelId"];
                return Convert.ToUInt64(serverId ?? "0");
            }
        }

        public static ulong VoiceChannelId
        {
            get
            {
                string serverId = ConfigurationManager.AppSettings["VoiceChannelId"];
                return Convert.ToUInt64(serverId ?? "0");
            }
        }

        public static string UserToken
        {
            get
            {
                return ConfigurationManager.AppSettings["UserToken"];
            }
        }

        public static string AdminName
        {
            get
            {
                return ConfigurationManager.AppSettings["AdminName"];
            }
        }

        public static bool UserConnectMode
        {
            get
            {
                string flag = ConfigurationManager.AppSettings["UserConnectMode"];
                return Convert.ToBoolean(flag ?? "false");
            }
        }


    }
}
