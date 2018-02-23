using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotaDiscordHook
{
    public class DiscordMessage
    {
        public string content { get; set; }
        public string username { get; set; }
        public bool tts { get; set; }
    }
}
