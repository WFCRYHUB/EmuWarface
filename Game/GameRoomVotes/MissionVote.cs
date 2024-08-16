using EmuWarface.Core;
using EmuWarface.Game.Missions;
using EmuWarface.Xmpp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmuWarface.Game.GameRoomVotes
{
    public class MissionVote
    {
        public List<Client> Voters      { get; private set; }
        public List<Mission> Missions   { get; private set; }

        public Dictionary<Client, Mission> Votes = new Dictionary<Client, Mission>();

        public bool Timeout;

        public MissionVote(List<Client> voters, List<Mission> missions)
        {
            Voters      = voters;
            Missions    = missions;
        }

        public void VotingStarted()
        {
            var map_voting_started = Xml.Element("map_voting_started")
                .Attr("voting_time", "15");

            var missions_el = Xml.Element("missions");

            foreach (var mission in Missions)
            {
                missions_el.Child(Xml.Element("mission").Attr("uid", mission.Uid));
            }

            map_voting_started.Child(missions_el);

            foreach (var voter in Voters)
            {
                voter.QueryGet(map_voting_started);
            }
        }

        public void OnVote(Client client, string mission_uid)
        {
            if (!Voters.Contains(client))
                return;

            var vote_mission = Missions.FirstOrDefault(x => x.Uid == mission_uid);

            if (vote_mission == null)
                return;

            lock (Votes)
            {
                if (Votes.ContainsKey(client))
                {
                    Votes[client] = vote_mission;
                }
                else
                {
                    Votes.Add(client, vote_mission);
                }

                VotingState();
            }
        }

        public void VotingState()
        {
            var map_voting_state = Xml.Element("map_voting_state");

            foreach (var mission in Missions)
            {
                var mission_el = Xml.Element("mission")
                    .Attr("uid", mission.Uid)
                    .Attr("votes_num", Votes.Where(x => x.Value == mission).Count());

                map_voting_state.Child(mission_el);
            }

            foreach (var voter in Voters)
            {
                voter.QueryGet(map_voting_state);
            }
        }
    }
}
