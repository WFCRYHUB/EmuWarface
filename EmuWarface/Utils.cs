using EmuWarface.Game.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmuWarface
{
    public static class Utils
    {
        public static string Base64Encode(string data) => Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        public static string Base64Decode(string data) => Encoding.UTF8.GetString(Convert.FromBase64String(data));

        public static int GetHash(string data)
        {
            MD5 md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToInt32(hashed, 0);
        }

        public static void UpdateOnline()
        {
            lock (Server.Clients)
            {
                File.WriteAllText(Config.Settings.OnlinePath, Server.Clients.Where(x => !x.IsDedicated && x.Presence.HasFlag(PlayerStatus.Online)).Count().ToString());
            }
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static long GetTotalSeconds(string Value)
        {
            try
            {
                long num = long.Parse(new Regex("[0-9]*").Matches(Value)[0].ToString());
                char result = 's';
                char.TryParse(new Regex("[a-z]").Matches(Value)[0].Value, out result);
                switch (result)
                {
                    case 'm':
                        return (long)TimeSpan.FromMinutes(num).TotalSeconds;
                    case 'h':
                        return (long)TimeSpan.FromHours(num).TotalSeconds;
                    case 'd':
                        return (long)TimeSpan.FromDays(num).TotalSeconds;
                    default:
                        return num;
                }
            }
            catch
            {
                return -1;
            }
        }

        public static bool MatFilter(string input)
        {
            List<char> combined = new List<char>();

            foreach (var symbol in input.ToLower().ToCharArray())
            {
                if (symbol != 95 && symbol != 46 && symbol != 45 && !(symbol > 47 && symbol < 58))
                {
                    if (symbol > 1072)
                    {
                        combined.Add(symbol);
                    }
                }
            }

            input = new string(combined.ToArray());

            foreach (string word in Config.ObsceneWords)
            {
                if (input.Contains(word))
                    return false;
            }
            return true;
        }

        public static Task Delay(double seconds)
        {
            return Task.Delay(TimeSpan.FromSeconds(seconds));
        }
    }
}
