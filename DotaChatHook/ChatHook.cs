using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotaChatHook
{

    public class Native
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct ModuleInformation
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out ModuleInformation lpmodinfo, uint cb);


        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        //[DllImport("user32.dll")]
        //public static extern bool MessageBeep(uint uType);

        //public enum BeepType : uint
        //{
        //    /// <summary>
        //    /// A simple windows beep
        //    /// </summary>            
        //    SimpleBeep = 0xFFFFFFFF,
        //    /// <summary>
        //    /// A standard windows OK beep
        //    /// </summary>
        //    OK = 0x00,
        //    /// <summary>
        //    /// A standard windows Question beep
        //    /// </summary>
        //    Question = 0x20,
        //    /// <summary>
        //    /// A standard windows Exclamation beep
        //    /// </summary>
        //    Exclamation = 0x30,
        //    /// <summary>
        //    /// A standard windows Asterisk beep
        //    /// </summary>
        //    Asterisk = 0x40,
        //}

    }

    public class ChatHook : EasyHook.IEntryPoint
    {

        /// <summary>
        /// Reference to the server interface within FileMonitor
        /// </summary>
        ServerInterface _server = null;

        /// <summary>
        /// Message queue of all files accessed
        /// </summary>
        Queue<string> _messageQueue = new Queue<string>();

        public ChatHook(EasyHook.RemoteHooking.IContext context, string channelName)
        {
            // Connect to server object using provided channel name
            _server = EasyHook.RemoteHooking.IpcConnectClient<ServerInterface>(channelName);

            // If Ping fails then the Run method will be not be called
            _server.Ping();
        }


        readonly static byte[] messageSignature = { 0x40, 0x55, 0x41, 0x57, 0x48, 0x8d, 0xAC, 0x24, 0x98, 0xFD, 0xFF, 0xFF, 0x48, 0x81, 0xEC, 0x68 };

        unsafe public void Run(EasyHook.RemoteHooking.IContext context, string channelName)
        {
            string s = dllpurpose;
            int id = EasyHook.RemoteHooking.GetCurrentProcessId();
            _server.HookIsInstalled(id);
            EasyHook.LocalHook chatMessageFunctionHook = null;
            try
            {
                
                Native.ModuleInformation moduleInformation = new Native.ModuleInformation();
                var size = Convert.ToUInt32(Marshal.SizeOf(typeof(Native.ModuleInformation)));
                Native.GetModuleInformation(Process.GetCurrentProcess().Handle, Native.GetModuleHandle("client.dll"), out moduleInformation, size);
                var pointer = FindThePrintFunction(moduleInformation, messageSignature);
                originalMethod = Marshal.GetDelegateForFunctionPointer<DotaChatFunction_Delegate>(pointer);

                chatMessageFunctionHook = EasyHook.LocalHook.Create(
                    pointer,
                    new DotaChatFunction_Delegate(MyDotaChatFunction),
                    this);
                chatMessageFunctionHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                originalMethodByPass = Marshal.GetDelegateForFunctionPointer<DotaChatFunction_Delegate>(chatMessageFunctionHook.HookBypassAddress);

                
                _server.ReportMessage(id, "Local Hook Installation complete.");

                try
                {
                    // Loop until FileMonitor closes (i.e. IPC fails)
                    while (true)
                    {
                        System.Threading.Thread.Sleep(500);

                        string[] queued = null;

                        lock (_messageQueue)
                        {
                            queued = _messageQueue.ToArray();
                            _messageQueue.Clear();
                        }

                        // Send newly monitored file accesses to FileMonitor
                        if (queued != null && queued.Length > 0)
                        {
                            _server.ReportChatMessages(queued);
                        }
                        else
                        {
                            _server.Ping();
                        }
                    }
                }
                catch(Exception ex)
                {
                    // Ping() or ReportMessages() will raise an exception if host is unreachable
                    try
                    {
                        _server.ReportException(ex);
                    }
                    catch
                    {

                    }
                }

            }
            catch (Exception ex)
            {
                _server.ReportException(ex);
            }
            finally
            {
                try
                {
                    if (chatMessageFunctionHook != null)
                        chatMessageFunctionHook.Dispose();

                    EasyHook.LocalHook.Release();
                }
                catch (Exception ex)
                {
                    _server.ReportException(ex);
                }
            }
            
            
        }


       

        #region delegate

        [UnmanagedFunctionPointer(CallingConvention.Cdecl,
                CharSet = CharSet.Unicode,
                SetLastError = true)]
        unsafe delegate Int64 DotaChatFunction_Delegate(void* a1, void* a2);

        DotaChatFunction_Delegate originalMethod = null;
        DotaChatFunction_Delegate originalMethodByPass = null;

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct DotaMessage
        {
            public IntPtr padding;
            public IntPtr padding1;
            public IntPtr padding2;
            public char* type;
            public char* username;
            public InnerMessage* innermessage;
        }

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct InnerMessage
        {
            public IntPtr message;
            public IntPtr padding1;
            public IntPtr padding2;
            public UInt64 check;
        }


        public const string dllpurpose = "not a hack!(https://github.com/neerajanil/DotaChatToDiscord/)";
        unsafe Int64 MyDotaChatFunction(void* a1, void* a2)
        {
            //Native.MessageBeep((uint)Native.BeepType.Asterisk);
            string s = dllpurpose;
            DotaMessage chat = *(DotaMessage*)a2;
            string type = Marshal.PtrToStringAnsi((IntPtr)chat.type);
            string username = Marshal.PtrToStringAnsi((IntPtr)chat.username);
            string message = "";

            InnerMessage innermessage = *chat.innermessage;

            if (innermessage.check < 0x10)
            {
                message = Marshal.PtrToStringAnsi((IntPtr)(&innermessage.message));
            }
            else
            {
                message = Marshal.PtrToStringAnsi((IntPtr)innermessage.message);
            }
            string json = string.Format("{{ \"Type\" : \"{0}\",\"Username\" : \"{1}\",\"Message\" : \"{2}\" }}", type, username, message);
                        
            lock (this._messageQueue)
            {
                if (this._messageQueue.Count < 1000)
                {
                    // Add message to send to server
                    this._messageQueue.Enqueue(json);
                }
            }

            return originalMethod(a1, a2);
        }


        #endregion

        #region olddelegate

        //[UnmanagedFunctionPointer(CallingConvention.Cdecl,
        //        CharSet = CharSet.Unicode,
        //        SetLastError = true)]
        //unsafe delegate Int64 MessagePrintf_Delegate(void* a1, int ba2, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder message, int a3);

        //MessagePrintf_Delegate originalMethod = null;
        //MessagePrintf_Delegate originalMethodByPass = null;

        //unsafe Int64 MyMessagePrintf(void* a1, int a2, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder message, int a3)
        //{
        //    Native.MessageBeep((uint)Native.BeepType.Asterisk);
        //    //lock (this._messageQueue)
        //    //{
        //    //    if (this._messageQueue.Count < 1000)
        //    //    {
        //    //        // Add message to send to FileMonitor
        //    //        this._messageQueue.Enqueue(message.ToString());
        //    //    }
        //    //}
        //    _server.ReportMessage(0, "procaddress:" + message.ToString());
        //    return originalMethod(a1, a2, message, a3);
        //}


        #endregion


        unsafe IntPtr FindThePrintFunction(Native.ModuleInformation moduleInformation,byte[] signature)
        {
            byte* baseAddr = (byte*)moduleInformation.lpBaseOfDll;
            uint dllSize = moduleInformation.SizeOfImage;
            uint i = 0;
            for (; i < dllSize; i++)
            {
                for (int j = 0; j < (sizeof(byte) * signature.Length); j++)
                {
                    if (*(baseAddr + j) != signature[j])
                        break;

                    if (j == (sizeof(byte) * signature.Length) - 1)
                    {
                        return (IntPtr)baseAddr;
                    }
                }
                baseAddr++;
            }
            _server.ReportMessage(0, "failed!!");
            return IntPtr.Zero;
        }
        
    }

}
