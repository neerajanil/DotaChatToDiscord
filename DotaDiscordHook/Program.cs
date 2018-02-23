﻿using System;
using System.Diagnostics;
using System.IO;


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
            var server = EasyHook.RemoteHooking.IpcCreateServer<DotaChatHook.ServerInterface>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton, new DotaChatHook.ServerInterface(Discord.Send));
            
            // inject into existing process
            EasyHook.RemoteHooking.Inject(
                targetPID,          // ID of process to inject into
                injectionLibrary,   // 32-bit library to inject (if target is 32-bit)
                injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                channelName         // the parameters to pass into injected library
                                    // ...
            );
            Console.ReadLine();
            server.StopListening(null);
            Console.ReadLine();
        }
    }


    //class Program
    //{
    //    // The matching delegate for MessageBeep
    //    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    //    delegate bool MessageBeepDelegate(uint uType);

    //    // Import the method so we can call it
    //    [DllImport("user32.dll")]
    //    static extern bool MessageBeep(uint uType);

    //    /// <summary>
    //    /// Our MessageBeep hook handler
    //    /// </summary>
    //    static private bool MessageBeepHook(uint uType)
    //    {
    //        // We aren't going to call the original at all
    //        // but we could using: return MessageBeep(uType);
    //        Console.Write("...intercepted...");
    //        return false;
    //    }

    //    /// <summary>
    //    /// Plays a beep using the native MessageBeep method
    //    /// </summary>
    //    static private void PlayMessageBeep()
    //    {
    //        Console.Write("    MessageBeep(BeepType.Asterisk) return value: ");
    //        Console.WriteLine(MessageBeep((uint)BeepType.Asterisk));
    //    }

    //    static void Main(string[] args)
    //    {
    //        Console.WriteLine("Calling MessageBeep with no hook.");
    //        PlayMessageBeep();

    //        Console.Write("\nPress <enter> to call MessageBeep while hooked by MessageBeepHook:");
    //        Console.ReadLine();

    //        Console.WriteLine("\nInstalling local hook for user32!MessageBeep");
    //        // Create the local hook using our MessageBeepDelegate and MessageBeepHook function
    //        using (var hook = EasyHook.LocalHook.Create(
    //                EasyHook.LocalHook.GetProcAddress("user32.dll", "MessageBeep"),
    //                new MessageBeepDelegate(MessageBeepHook),
    //                null))
    //        {
    //            // Only hook this thread (threadId == 0 == GetCurrentThreadId)
    //            hook.ThreadACL.SetInclusiveACL(new int[] { 0 });

    //            PlayMessageBeep();

    //            Console.Write("\nPress <enter> to disable hook for current thread:");
    //            Console.ReadLine();
    //            Console.WriteLine("\nDisabling hook for current thread.");
    //            // Exclude this thread (threadId == 0 == GetCurrentThreadId)
    //            hook.ThreadACL.SetExclusiveACL(new int[] { 0 });
    //            PlayMessageBeep();

    //            Console.Write("\nPress <enter> to uninstall hook and exit.");
    //            Console.ReadLine();
    //        } // hook.Dispose() will uninstall the hook for us
    //    }

    //    public enum BeepType : uint
    //    {
    //        /// <summary>
    //        /// A simple windows beep
    //        /// </summary>            
    //        SimpleBeep = 0xFFFFFFFF,
    //        /// <summary>
    //        /// A standard windows OK beep
    //        /// </summary>
    //        OK = 0x00,
    //        /// <summary>
    //        /// A standard windows Question beep
    //        /// </summary>
    //        Question = 0x20,
    //        /// <summary>
    //        /// A standard windows Exclamation beep
    //        /// </summary>
    //        Exclamation = 0x30,
    //        /// <summary>
    //        /// A standard windows Asterisk beep
    //        /// </summary>
    //        Asterisk = 0x40,
    //    }

    //}
}