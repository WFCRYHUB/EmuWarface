using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Shops;
using System;
using System.Threading;

namespace EmuWarface
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            Log.Info("Starting...");

            SQL.Init();
            CommandHandler.Init();
            QueryBinder.Init();
            QueryCache.Init();
            GameData.Init();
            Shop.Init();
            Server.Init();
            Clan.GenerateClanList();

            Thread.Sleep(-1);
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error(e.ExceptionObject.ToString());
            Environment.Exit(1);
        }
    }
}
