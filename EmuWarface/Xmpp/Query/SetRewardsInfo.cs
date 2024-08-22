using EmuWarface.Core;
using EmuWarface.Game;
using EmuWarface.Game.Clans;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.Items;
using EmuWarface.Game.Shops;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    public static class SetRewardsInfo
    {
        /*
<set_rewards_info session_id="1" difficulty="128" room_type="2" mission_id="e5981b6a-325d-42eb-a3fe-e6eed0bc4bf2" incomplete_session="0" session_time="605.01617" session_kill_count="5" winning_team_id="1" passed_sublevels_count="1" passed_checkpoints_count="0" secondary_objectives_completed="0" last_boss_killed="" max_session_score="500">
<players_performance />
<team id="1">
<profile profile_id="5" score="500" in_session_from_start="1" player_session_time="605.01617" first_checkpoint="0" last_checkpoint="0" group_id="" player_finished_session="1" />
</team>
<team id="2">
<profile profile_id="3" score="100" in_session_from_start="1" player_session_time="605.01617" first_checkpoint="0" last_checkpoint="0" group_id="" player_finished_session="1" />
</team>
</set_rewards_info>
 */

        [Query(IqType.Get, "set_rewards_info")]
        public static void SetRewardsInfoSerializer(Client client, Iq iq)
        {
            if (!client.IsDedicated)
                throw new InvalidOperationException();

            if (client.Dedicated.Room == null)
                throw new InvalidOperationException();

            var q = iq.Query;

            var broadcast_session_result = Xml.Element("broadcast_session_result");

            var rewards_configuration = GameData.RewardsConfiguration;

            var room = client.Dedicated.Room;
            var rCore = client.Dedicated.Room.GetExtension<GameRoomCore>();
            var rSession = client.Dedicated.Room.GetExtension<GameRoomSession>();
            var rMission = client.Dedicated.Room.GetExtension<GameRoomMission>();

            var win_pool = int.Parse(GameData.RewardsConfiguration["Rewards"]["WinPoolDefault"].InnerText);
            var lose_pool = int.Parse(GameData.RewardsConfiguration["Rewards"]["LosePoolDefault"].InnerText);
            var draw_pool = int.Parse(GameData.RewardsConfiguration["Rewards"]["DrawPoolDefault"].InnerText);
            var score_pool = int.Parse(GameData.RewardsConfiguration["Rewards"]["ScorePoolDefault"].InnerText);

            /*if (mission_info_current.settings.Sublevels.attrs.win_pool != null)
            {
                //console.log("OneSublevel");
                win_pool = Number(mission_info_current.settings.Sublevels.attrs.win_pool);
                lose_pool = Number(mission_info_current.settings.Sublevels.attrs.lose_pool);
                draw_pool = Number(mission_info_current.settings.Sublevels.attrs.draw_pool);
                score_pool = Number(mission_info_current.settings.Sublevels.attrs.score_pool);
            }*/

            var secondary_objectives_completed = int.Parse(q.GetAttribute("secondary_objectives_completed")) * int.Parse(rewards_configuration["Rewards"]["SecondaryObjectiveBonus"].InnerText);


            var teams = q.GetElementsByTagName("team");

            //Только для пвп вычитание очков за маленькое количество игроков
            var players_count_all = 0;
            foreach (XmlElement team in teams)
            {
                players_count_all += team.ChildNodes.Count;
            }

            //Подсчёты учитывая игроков
            var player_count_reward_mults = rewards_configuration["Rewards"]["player_count_reward_mults"].GetElementsByTagName("Value");

            double player_count_reward_mult = 1;

            //TODO check 8 players
            if (player_count_reward_mults.Count >= players_count_all)
                player_count_reward_mult = double.Parse(player_count_reward_mults[player_count_reward_mults.Count - 1].InnerText, CultureInfo.InvariantCulture);

            var rewards = rewards_configuration["Rewards"];
            var mission_type = rMission.Mission.MissionType;

            //Счастливыве часы
            var dynamic_multiplier = 1;
            var dynamic_multipliers_info = "";

            var db = SQL.QueryRead($"SELECT * FROM emu_dynamic_multipliers");
            foreach (DataRow row in db.Rows)
            {
                dynamic_multiplier *= (int)row["multiplier"];
                if (string.IsNullOrEmpty(dynamic_multipliers_info))
                {
                    dynamic_multipliers_info = (string)row["name"];
                }
                else
                {
                    dynamic_multipliers_info += " + " + (string)row["name"];
                }
            }

            List<Client> receivers = new List<Client>();

            foreach (XmlElement team in teams)
            {
                //players_count_all += team.ChildNodes.Count;
                foreach (XmlElement profile in team.ChildNodes)
                {
                    Client target = null;

                    lock (Server.Clients)
                    {
                        target = Server.Clients.FirstOrDefault(x => x.ProfileId.ToString() == profile.GetAttribute("profile_id"));
                    }

                    if (target == null)
                        continue;

                    receivers.Add(target);

                    //Вычисление пула для текущего игрока
                    var current_pool = 0;
                    var score_popl_for_c_player = 0;
                    var in_session_from_start = int.Parse(profile.GetAttribute("in_session_from_start"));
                    var score = int.Parse(profile.GetAttribute("score"));
                    var max_session_score = int.Parse(q.GetAttribute("max_session_score"));
                    var session_time = double.Parse(q.GetAttribute("session_time"), CultureInfo.InvariantCulture);
                    var player_session_time = double.Parse(profile.GetAttribute("player_session_time"), CultureInfo.InvariantCulture);

                    if (score_pool != 0 && score != 0 && max_session_score != 0)
                    {
                        score_popl_for_c_player = score_pool / 100 * score / max_session_score * 100;
                    }

                    var is_winner = 0;
                    if (q.GetAttribute("winning_team_id") == team.GetAttribute("id"))
                    {
                        current_pool = win_pool + score_popl_for_c_player + secondary_objectives_completed;
                        is_winner = 1;
                    }
                    else
                    {
                        current_pool = lose_pool + score_popl_for_c_player + secondary_objectives_completed;
                    }
                    if (player_count_reward_mult != null && room.Type != RoomType.PvE_Private)
                    {
                        current_pool = (int)Math.Round(current_pool * player_count_reward_mult);
                    }

                    if (in_session_from_start == 0)
                    {
                        current_pool = (int)Math.Round(current_pool / 1.5);
                    }

                    current_pool = (int)Math.Round(current_pool * session_time / 15);

                    //Вычислекние варбаксов и остальной валюты на основании pool
                    var res_game_money = 0;
                    var res_exp = 0;
                    var res_sp_points = 0;
                    var res_crown = 0;
                    var res_clan_points = 0;

                    //Вычичление множителя для варбаксов еслие его нет в там то дефаулт
                    if (rewards["MoneyMultiplier"][rMission.Mission.MissionType] != null)
                    {
                        res_game_money = (int)Math.Round(current_pool * double.Parse(rewards["MoneyMultiplier"][rMission.Mission.MissionType].InnerText, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        res_game_money = (int)Math.Round(current_pool * double.Parse(rewards["MoneyMultiplier"]["default"].InnerText, CultureInfo.InvariantCulture));
                    }
                    //Вычичление множителя для опыта еслие его нет в там то дефаулт
                    if (rewards["ExperienceMultiplier"][mission_type] != null)
                    {
                        res_exp = (int)Math.Round(current_pool * double.Parse(rewards["ExperienceMultiplier"][rMission.Mission.MissionType].InnerText, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        res_exp = (int)Math.Round(current_pool * double.Parse(rewards["ExperienceMultiplier"]["default"].InnerText, CultureInfo.InvariantCulture));
                    }
                    //Вычичление множителя для опыта поставщиков еслие его нет в там то дефаулт
                    if (rewards["SponsorPointsMultiplier"][mission_type] != null)
                    {
                        res_sp_points = (int)Math.Round(current_pool * double.Parse(rewards["SponsorPointsMultiplier"][rMission.Mission.MissionType].InnerText, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        res_sp_points = (int)Math.Round(current_pool * double.Parse(rewards["SponsorPointsMultiplier"]["default"].InnerText, CultureInfo.InvariantCulture));
                    }

                    //Клановые очки 
                    /*if (q.GetAttribute("isClanWar") == "1")
                    {
                        res_clan_points = (int)Math.Round(current_pool * double.Parse(rewards["SponsorPointsMultiplier"]["ClanPointsClanWarMultiplier"].InnerText, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        res_clan_points = (int)Math.Round(current_pool * double.Parse(rewards["SponsorPointsMultiplier"]["ClanPointsMultiplier"].InnerText, CultureInfo.InvariantCulture));
                    }*/
                    //TODO clan points
                    if (target.Profile.ClanId != 0)
                    {
                        res_clan_points = (int)Math.Round(current_pool * 0.2);
                    }

                    if (res_game_money < 5)
                    {
                        res_game_money = (int)Math.Round(double.Parse(rewards["MinReward"].InnerText, CultureInfo.InvariantCulture));
                    }
                    if (res_exp < 5)
                    {
                        res_exp = (int)Math.Round(double.Parse(rewards["MinReward"].InnerText, CultureInfo.InvariantCulture));
                    }
                    if (res_sp_points < 5)
                    {
                        res_sp_points = (int)Math.Round(double.Parse(rewards["MinReward"].InnerText, CultureInfo.InvariantCulture));
                    }

                    if (res_clan_points < 1)
                    {
                        res_clan_points = 1;
                    }

                    //TODO випки
                    double xpBoost = 1;
                    double vpBoost = 1;
                    double gmBoost = 1;

                    byte isVip = 0;

                    foreach (var booster in target.Profile.Items)
                    {
                        if (!booster.Name.StartsWith("booster_"))
                            continue;

                        if (rSession.StartTime > booster.ExpirationTimeUtc)
                            continue;

                        var booster_info = Shop.ShopItems.FirstOrDefault(x => x.GetAttribute("name") == booster.Name);

                        foreach (XmlElement param in booster_info["GameParams"].ChildNodes)
                        {
                            switch (param.GetAttribute("name"))
                            {
                                case "xpBoost":
                                    xpBoost += double.Parse(param.GetAttribute("value"), CultureInfo.InvariantCulture);
                                    break;
                                case "vpBoost":
                                    vpBoost += double.Parse(param.GetAttribute("value"), CultureInfo.InvariantCulture);
                                    break;
                                case "gmBoost":
                                    gmBoost += double.Parse(param.GetAttribute("value"), CultureInfo.InvariantCulture);
                                    break;
                            }
                        }

                        foreach (XmlElement param in booster_info["mmo_stats"].ChildNodes)
                        {
                            if (param.GetAttribute("name") == "vip")
                                isVip = byte.Parse(param.GetAttribute("value"));
                        }
                    }

                    //Log.Info($"pid={target.ProfileId} exp={res_exp} xpBoost={xpBoost}");

                    //Подсчёт с учетеом випки
                    res_exp = (int)Math.Round(res_exp * xpBoost);
                    res_sp_points = (int)Math.Round(res_sp_points * vpBoost);
                    res_game_money = (int)Math.Round(res_game_money * gmBoost);

                    //Счастливыве часы
                    //if(dynamic_multiplier != 0)
                    //{
                    res_exp *= dynamic_multiplier;
                    res_sp_points *= dynamic_multiplier;
                    res_game_money *= dynamic_multiplier;
                    //}

                    //TODO test
                    /*if (rMission.Mode == "pve")
                    {
                        switch (mission_type)
                        {
                            case "trainingmission":
                                res_exp *= 2;
                                break;
                            case "easymission":
                                res_exp *= 3;
                                break;
                            case "normalmission":
                                res_exp *= 4;
                                break;
                            case "hardmission":
                                res_exp *= 5;
                                break;
                            case "survivalmission":
                                res_exp *= 6;
                                break;
                        }
                    }*/

                    if (target.Profile.IsExperienceFreezed)
                        res_exp = 0;

                    if (target.Profile.Experience + res_exp > 23046000)
                    {
                        res_exp = 23046000 - target.Profile.Experience;
                    }

                    target.Profile.Experience += res_exp;
                    target.Profile.GameMoney += res_game_money;

                    //БОНУС
                    target.Profile.CryMoney += 100;
                    target.Profile.GiveRandomBoxCards();
                    target.Profile.GiveItem("free_card", ItemType.Consumable, 50);

                    target.QueryGet(target.Profile.UpdateCryMoney());

                    target.Profile.Update();

                    if (target.Profile.ClanId != 0)
                    {
                        Clan.AddMemberPoints(target.Profile.ClanId, target.ProfileId, res_clan_points);
                    }

                    XmlElement player_result = Xml.Element("player_result")
                        .Attr("nickname", target.Profile.Nickname)
                        .Attr("money", res_game_money)
                        .Attr("experience", res_exp)
                        .Attr("sponsor_points",             /*res_sp_points*/ "0") //TODO
                        .Attr("clan_points", res_clan_points)
                        .Attr("gained_crown_money", res_crown)
                        .Attr("bonus_money",                /*res_bonus_money*/"0")             //TODO
                        .Attr("bonus_experience",           /*res_bonus_experience*/"0")        //TODO
                        .Attr("bonus_sponsor_points",       /*res_bonus_sponsor_points*/"0")    //TODO
                        .Attr("completed_stages", "0")
                        .Attr("money_boost", "0")    //TODO
                        .Attr("experience_boost", "0")    //TODO
                        .Attr("sponsor_points_boost", "0")    //TODO
                        .Attr("experience_boost_percent", (xpBoost - 1).ToString("F2").Replace(',', '.'))
                        .Attr("money_boost_percent", (gmBoost - 1).ToString("F2").Replace(',', '.'))
                        .Attr("sponsor_points_boost_percent", (vpBoost - 1).ToString("F2").Replace(',', '.'))
                        .Attr("is_vip", isVip)
                        .Attr("score", score)
                        .Attr("no_crown_rewards", "1") //TODO
                        .Attr("first_win", "0")
                        .Attr("dynamic_crown_multiplier", "1")
                        .Attr("dynamic_multipliers_info", Utils.Base64Encode(dynamic_multipliers_info));

                    if (room.Type == RoomType.PvP_Rating)
                        target.Profile.PvpRatingState.UpdateRating(is_winner == 1);

                    //<pvp_rating_outcome rank='9' game_result='1' games_history='' win_streak_bonus='0'/>
                    XmlElement pvp_rating_outcome = Xml.Element("pvp_rating_outcome")
                        .Attr("rank", target.Profile.PvpRatingState.Rank)
                        .Attr("game_result", is_winner)
                        .Attr("games_history", target.Profile.PvpRatingState.GamesHistory)
                        .Attr("win_streak_bonus", 0);

                    player_result.Child(pvp_rating_outcome);

                    broadcast_session_result.Child(player_result);

                    //<player_result nickname='..каштыр..' experience='0' money='338' gained_crown_money='0' no_crown_rewards='1' sponsor_points='0' clan_points='38' bonus_experience='0' bonus_money='0' bonus_sponsor_points='0' experience_boost='444' money_boost='160' sponsor_points_boost='0' experience_boost_percent='1.15' money_boost_percent='0.9' sponsor_points_boost_percent='0.65' completed_stages='0' is_vip='1' score='1310' first_win='0' dynamic_multipliers_info='' dynamic_crown_multiplier='1'>
                    //<profile_progression_update profile_id='13143324' mission_unlocked='none,trainingmission,easymission,normalmission,hardmission,survivalmission,zombieeasy,zombienormal,zombiehard,campaignsections,campaignsection1,campaignsection2,campaignsection3,volcanoeasy,volcanonormal,volcanohard,volcanosurvival,anubiseasy,anubisnormal,anubishard,zombietowereasy,zombietowernormal,zombietowerhard,icebreakereasy,icebreakernormal,icebreakerhard,anubiseasy2,anubisnormal2,anubishard2,chernobyleasy,chernobylnormal,japaneasy,japannormal,all' tutorial_unlocked='7' tutorial_passed='7' class_unlocked='29'/>
                    //<pvp_rating_outcome rank='9' game_result='1' games_history='' win_streak_bonus='0'/>
                    //</player_result>
                }
            }

            foreach (var target in receivers)
            {
                target.QueryGet(broadcast_session_result, target.Channel.Jid);
            }

            iq.SetQuery(Xml.Element("set_rewards_info"));
            client.QueryResult(iq);
        }
    }
}