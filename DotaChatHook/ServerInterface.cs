using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotaChatHook
{
    /// <summary>
    /// Provides an interface for communicating from the client (target) to the server (injector)
    /// </summary>
    /// 
    public class ServerInterface : MarshalByRefObject
    {
        public ServerInterface() : base()
        {
            ChatMessageAction += DefaultChatMessageAction;
        }

        public ServerInterface(Action<ChatMessage> chatMessageAction) : this()
        {
            ChatMessageAction += chatMessageAction;
        }

        public event Action<ChatMessage> ChatMessageAction;

        public void HookIsInstalled(int clientPID)
        {
            Console.WriteLine("DotaDiscordHook has injected ChatHook into process {0}.\r\n", clientPID);
        }

        public void DefaultChatMessageAction(ChatMessage chat)
        {
            Console.WriteLine(chat.Type + " - " + chat.Username + " - " + chat.Message);
        }

        /// <summary>
        /// Output messages to the console.
        /// </summary>
        /// <param name="clientPID"></param>
        /// <param name="fileNames"></param>
        //public void ReportChatMessages(ChatMessage[] messages)
        //{
        //    for (int i = 0; i < messages.Length; i++)
        //    {
        //        Console.WriteLine(messages[i].Type + " - " + messages[i].UserName + " - " + messages[i].Message);
        //        //ChatMessageAction(messages[i]);
        //    }
        //}

        public void ReportChatMessages(string[] messages)
        {
            for (int i = 0; i < messages.Length; i++)
            {
                ChatMessageAction(Jil.JSON.Deserialize<ChatMessage>(messages[i]));
                //ChatMessageAction(messages[i]);
            }
        }


        public void ReportMessage(int clientPID, string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Report exception
        /// </summary>
        /// <param name="e"></param>
        public void ReportException(Exception e)
        {
            Console.WriteLine("The target process has reported an error:\r\n" + e.ToString());
        }

        /// <summary>
        /// Called to confirm that the IPC channel is still open / host application has not closed
        /// </summary>
        public void Ping()
        {
        }
    }
}
