using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace EmuWarface.Game
{
    public static class GameRestrictionSystem
    {
        //kind id
        private static List<Tuple<string, string, string>>                      _defaultRestrictions = new List<Tuple<string, string, string>>();
        //kind id
        private static List<Tuple<string, string, List<string>>>                _allowedRestrictions = new List<Tuple<string, string, List<string>>>();
        //mode room_type kind
        private static List<Tuple<string, string, Dictionary<string, string>>>  _gmModesRestrictions = new List<Tuple<string, string, Dictionary<string, string>>>();

        static GameRestrictionSystem()
        {
            foreach (XmlElement restriction in GameData.GameModesConfig["restriction_options"])
            {
                var kind = restriction.GetAttribute("kind");

                foreach (XmlElement option in restriction.ChildNodes)
                {
                    if (option.Name != "option")
                        continue;

                    var id              = option.GetAttribute("id");
                    var defaultValue    = option.GetAttribute("default");


                    if (option.HasAttribute("default"))
                        _defaultRestrictions.Add(Tuple.Create(kind, id, defaultValue));


                    if (option.HasChildNodes)
                    {
                        List<string> allowedValues = new List<string>();
                        foreach (XmlElement allowed in option.ChildNodes)
                        {
                            if (allowed.Name != "allowed")
                                continue;

                            allowedValues.Add(allowed.GetAttribute("value"));
                        }
                        _allowedRestrictions.Add(Tuple.Create(kind, id, allowedValues));
                    }
                }
            }

            foreach (var gm in GameData.GameModes)
            {
                Dictionary<string, string> restrictions = new Dictionary<string, string>();

                var mode = gm.Key;
                var elem = gm.Value;

                foreach (XmlElement restriction in elem["restrictions"].ChildNodes)
                {
                    if (restriction.Name != "restriction")
                        continue;

                    restrictions.Add(restriction.GetAttribute("kind"), restriction.GetAttribute("option"));
                }

                _gmModesRestrictions.Add(Tuple.Create(mode, string.Empty, restrictions));

                foreach (XmlElement room in elem.ChildNodes)
                {
                    if (room.Name != "room")
                        continue;

                    var type = room.GetAttribute("type");
                    restrictions = new Dictionary<string, string>();

                    foreach (XmlElement restriction in room.ChildNodes)
                    {
                        if (restriction.Name != "restriction")
                            continue;

                        restrictions.Add(restriction.GetAttribute("kind"), restriction.GetAttribute("option"));
                    }

                    _gmModesRestrictions.Add(Tuple.Create(mode, type, restrictions));
                }
            }

            //var s = _defaultRestrictions["auto_team_balance"]["force_enabled"];
            //var s = GetDefaultRestriction("auto_team_balance", "force_enabled");
            //var s2 = GetDefaultRestriction("preround_time", "default");
            //var s3 = GetDefaultRestriction("max_players", "pve_aren2a");
        }

        public static string GetDefaultRestrictionByOption(string kind, string option)      => _defaultRestrictions.FirstOrDefault(x => x.Item1 == kind && x.Item2 == option)?.Item3;
        public static string GetDefaultRestriction(string kind, string mode, string type)
        {
            var option = GetOption(kind, mode, type);

            if (option == null)
                return null;

            return GetDefaultRestrictionByOption(kind, option);
        }
        public static Dictionary<string, string> GetDefaultRestrictions(string mode, string type)
        {
            try
            {
                var options = new Dictionary<string, string>();

                var room = _gmModesRestrictions.FirstOrDefault(x => x.Item1 == mode && x.Item2 == string.Empty);
                foreach (var option in room.Item3)
                {
                    options[option.Key] = option.Value;
                }

                room = _gmModesRestrictions.FirstOrDefault(x => x.Item1 == mode && x.Item2 == type);
                if (room != null)
                {
                    foreach (var option in room.Item3)
                    {
                        options[option.Key] = option.Value;
                    }
                }

                var restrictions = new Dictionary<string, string>();

                foreach (var option in options)
                {
                    restrictions.Add(option.Key, GetDefaultRestrictionByOption(option.Key, option.Value));
                }

                return restrictions;
            }
            catch
            {
                return null;
            }
        }
        public static string GetOption(string kind, string mode, string type)
        {
            try
            {
                var restriction = _gmModesRestrictions.FirstOrDefault(x => x.Item1 == mode && x.Item2 == type);
                if (restriction != null && restriction.Item3 != null && restriction.Item3.Count != 0)
                {
                    return restriction.Item3.FirstOrDefault(x => x.Key == kind).Value;
                }

                return _gmModesRestrictions.FirstOrDefault(x => x.Item1 == mode && x.Item2 == string.Empty)?.Item3?.FirstOrDefault(x => x.Key == kind).Value;
            }
            catch
            {
                return null;
            }
        }
        public static bool ValidateRestriction(string mode, string type, string kind, string value)
        {
            var option = GetOption(kind, mode, type);
            if (option == null)
                return false;

            var restriction_value = _allowedRestrictions.FirstOrDefault(x => x.Item1 == kind && x.Item2 == option)?.Item3;
            if (restriction_value == null)
                return false;

            return restriction_value.Contains(value);
        }
    }
}
