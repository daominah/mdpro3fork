using System.Runtime.InteropServices;
using System.Threading;
using MDPro3.Duel.YGOSharp;

namespace MDPro3.Net
{
    internal static unsafe class Dll
    {
        [DllImport("ygoserver", CallingConvention = CallingConvention.Cdecl)]
        public static extern int start_server([MarshalAs(UnmanagedType.LPUTF8Str)] string args);
        [DllImport("ygoserver", CallingConvention = CallingConvention.Cdecl)]
        public static extern void stop_server();
    }

    public class YgoServer
    {
        public static Thread serverThread;
        public static void StartServer(string args)
        {
            if (ServerRunning())
                StopServer();

            serverThread = new Thread(() =>
            {
                try
                {
                    Dll.start_server(args);
                }
                finally
                {
                    BanlistManager.RestoreLocalServerLflist();
                }
            });
            serverThread.Start();
        }

        public static void StopServer()
        {
            try
            {
                Dll.stop_server();
            }
            finally
            {
                serverThread?.Abort();
                BanlistManager.RestoreLocalServerLflist();
            }
        }

        public static bool ServerRunning()
        {
            return serverThread != null && serverThread.IsAlive;
        }
    }
}
