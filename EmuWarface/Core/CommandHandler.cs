using EmuWarface.Game.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EmuWarface.Core
{
    public static class CommandHandler
    {
        public static List<ICmd> Handlers = new List<ICmd>();

        public static void Init()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                if(type.GetInterfaces().Contains(typeof(ICmd)))
                {
                    var handler = (ICmd)Activator.CreateInstance(type);
                    Handlers.Add(handler);
                }
            }

            Log.Info("[CommandHandler] Loaded {0} commands", Handlers.Count);

            Task.Factory.StartNew(() => ReadConsole(), TaskCreationOptions.LongRunning);
        }

        public static void ReadConsole()
        {
            while (true)
            {
                var input = Console.ReadLine().Split(' ').ToList();
                input.RemoveAll(x => x == " " || x == string.Empty);

                if (input.Count == 0) continue;

                string cmdName = input[0];
                string[] args = input.Skip(1).ToArray();

                var cmd = Handlers.FirstOrDefault(c => c.Names.Contains(cmdName));

                string result = string.Empty;
                if (cmd == null)
                {
                    result = "Unknown command. Use 'help' for get command list.";
                }
                else
                {
                    try
                    {
                        result = cmd.OnCommand(Permission.Admin, args);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.ToString());
                    }
                }

                if(!string.IsNullOrEmpty(result))
                    Log.Info(result);
            }

        }
    }
}
