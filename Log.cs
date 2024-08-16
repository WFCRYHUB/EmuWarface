using System;
using System.Diagnostics;
using System.IO;

namespace EmuWarface
{
    public static class Log
    {
        private static string mainLogPath;
        private static string chatLogPath;
        private static string xmppLogPath;

        static Log()
        {
            if (!Directory.Exists("Logs"))
                Directory.CreateDirectory("Logs");

            if (!Directory.Exists("Logs/chat"))
                Directory.CreateDirectory("Logs/chat");

            mainLogPath = string.Format("Logs/emuwarface_{0}.log", DateTime.Now.ToString("yyyy-MM-dd"));
            chatLogPath = string.Format("Logs/chat/chat_{0}.log", DateTime.Now.ToString("yyyy-MM-dd"));
            xmppLogPath = string.Format("Logs/chat/xmpp_{0}.log", DateTime.Now.ToString("yyyy-MM-dd"));
        }

        public static void Chat(string value) => Print(LogType.CHAT, value, chatLogPath);
        public static void Chat(string format, params object[] arg) => Chat(string.Format(format, arg));
        public static void Xmpp(string value) => Print(LogType.XMPP, value.Replace("><", ">\n<"), xmppLogPath);
        public static void Xmpp(string format, params object[] arg) => Xmpp(string.Format(format, arg));
        public static void Debug(string value) => Print(LogType.DEBUG, value, mainLogPath);
        public static void Debug(string format, params object[] arg) => Debug(string.Format(format, arg));
        public static void Info(string value) => Print(LogType.INFO, value, mainLogPath);
        public static void Info(string format, params object[] arg) => Info(string.Format(format, arg));
        public static void Warn(string value) => Print(LogType.WARN, value, mainLogPath);
        public static void Warn(string format, params object[] arg) => Warn(string.Format(format, arg));
        public static void Error(string value) => Print(LogType.ERROR, value, mainLogPath);
        public static void Error(string format, params object[] arg) => Error(string.Format(format, arg));

        private static void Print(LogType type, string text, string filename)
        {
            var color   = ConsoleColor.White;

            switch (type)
            {
                case LogType.XMPP:
                    color = ConsoleColor.Gray;
                    filename = string.Format("Logs/xmpp_{0}h.log", DateTime.Now.ToString("yyyy-MM-dd HH"));
                    break;
                case LogType.DEBUG:
                    color   = ConsoleColor.Gray;
                    break;
                case LogType.CHAT:
                case LogType.INFO:
                    color   = ConsoleColor.White;
                    break;
                case LogType.WARN:
                    color   = ConsoleColor.Yellow;
                    break;
                case LogType.ERROR:
                    color   = ConsoleColor.Red;
                    break;
            }

            string output = string.Format("{0} {1}\t{2}", DateTime.Now.ToString("HH:mm:ss"), type.ToString(), text);

            lock (Console.Out)
            {
                Console.ForegroundColor = color;

                if(EmuConfig.Settings.XmppDebugConsole)
                    Console.Out.WriteLine(output);

                try
                {
                    File.AppendAllText(filename, output + "\n");
                }
                catch
                {

                }
            }
        }
    }

    public enum LogType
    {
        XMPP,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        CHAT
    }
}
