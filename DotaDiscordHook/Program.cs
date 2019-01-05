using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DotaDiscordHook
{
    class Program
    {
        
        static void Main(string[] args)
        {
            int targetPID = 0;
            Process[] processes = Process.GetProcesses();
            for (int i = 0; i < processes.Length; i++)
            {
                try
                {
                    if (processes[i].MainWindowTitle == "Dota 2" && processes[i].HasExited == false)
                    {
                        targetPID = processes[i].Id;
                    }
                }
                catch { }
            }


            Console.WriteLine("Attempting to inject into process {0}", targetPID);

            // Get the full path to the assembly we want to inject into the target process
            string injectionLibrary = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DotaChatHook.dll");
            string channelName = null;
            // Create the IPC server using the FileMonitorIPC.ServiceInterface class as a singleton
            var server = EasyHook.RemoteHooking.IpcCreateServer<DotaChatHook.ServerInterface>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton, new DotaChatHook.ServerInterface(DiscordWrapper.Send));
            
            // inject into existing process
            EasyHook.RemoteHooking.Inject(
                targetPID,          // ID of process to inject into
                injectionLibrary,   // 32-bit library to inject (if target is 32-bit)
                injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                channelName         // the parameters to pass into injected library
                                    // ...
            );
            if (Config.UserConnectMode == true)
            {
                DiscordWrapper._client = new DiscordSocketClient();
                DiscordWrapper._client.Log += LogAsync;
                DiscordWrapper._client.Ready += ReadyAsync;
                DiscordWrapper._client.LoginAsync(Discord.TokenType.User, Config.UserToken);
                DiscordWrapper._client.StartAsync();
                DiscordWrapper.EnabledUsers.Add(Config.AdminName, new User { Name = Config.AdminName, Admin = true });
            }
            Console.ReadLine();
            server.StopListening(null);
            Console.ReadLine();
        }


        private static Task LogAsync(Discord.LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.FromResult(0);
        }

         // The Ready event indicates that the client has opened a
        // connection and it is now safe to access the cache.
        private static Task ReadyAsync()
        {
            Console.WriteLine($"{DiscordWrapper._client.CurrentUser} is connected!");

            return Task.FromResult(0);
        }
    }
}