using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.GameRoomVotes;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace EmuWarface.Xmpp.Query
{
    /*
        <kick_voting_start initiator_profile_id="19" initiator_team_id="1" target_profile_id="21">
        <voters>
        <voter profile_id="19" />
        <voter profile_id="22" />
        <voter profile_id="21" />
        </voters>
        </kick_voting_start>
     */

    public static class VotingStart
    {
        [Query(IqType.Get, "kick_voting_start", "pause_voting_start", "surrender_voting_start")]
        public static void Serializer(Client client, Iq iq)
        {
            if (!client.IsDedicated)
                throw new InvalidOperationException();

            var q = iq.Query;
            var room = client.Dedicated.Room;

            if (room == null)
            {
                client.Dedicated.MissionUnload();
                return;
            }

            var rCore = room.GetExtension<GameRoomCore>();
            var rVote = room.GetExtension<GameRoomVoteStates>();

            if (rVote == null)
                return;

            //TODO pause
            if (q.Name == "pause_voting_start" || q.Name == "surrender_voting_start")
                return;

            VotingType votingType = VotingType.KickVote;
            switch (q.Name)
            {
                case "pause_voting_start":
                    votingType = VotingType.PauseVote;
                    break;
                case "surrender_voting_start":
                    votingType = VotingType.SurrenderVote;
                    break;
            }

            var initiator_team_id = Utils.ParseEnum<Team>(q.GetAttribute("initiator_team_id"));
            var initiator_profile_id = ulong.Parse(q.GetAttribute("initiator_profile_id"));
            var target_profile_id = ulong.Parse(q.GetAttribute("target_profile_id"));

            //foreach(var adm in EmuConfig.API.Admins)
            //{
            //    if (adm.ProfileId == target_profile_id)
            //        throw new QueryException(1);
            //}

            lock (rVote.Votes)
            {
                foreach (Vote _vote in rVote.Votes)
                {
                    if (_vote.Team == initiator_team_id && !_vote.Timeout)
                        throw new QueryException(1);
                }
            }

            Client initiator = null;
            Client target = null;

            List<Client> voters = new List<Client>();
            foreach (XmlElement voter in q["voters"].ChildNodes)
            {
                var profile_id = ulong.Parse(voter.GetAttribute("profile_id"));
                Client cl = null;

                lock (Server.Clients)
                {
                    cl = Server.Clients.FirstOrDefault(x => x.ProfileId == profile_id);
                }

                if (initiator_profile_id == profile_id)
                    initiator = cl;

                if (target_profile_id == profile_id)
                    target = cl;

                voters.Add(cl);
            }

            var vote = new Vote(votingType, initiator_team_id, voters, initiator, target);

            rVote.StartVote(vote);

            var on_kick_voting_started = Xml.Element("on_kick_voting_started")
                .Attr("initiator", initiator.Profile.Nickname)
                .Attr("target", target?.Profile?.Nickname)
                .Attr("yes_votes_required", vote.YesVotesRequired)
                .Attr("no_votes_required", vote.NoVotesRequired);

            client.QueryResult(iq);

            foreach (var voter in voters)
            {
                if (voter == target)
                    continue;

                voter.QueryGet(on_kick_voting_started, voter.Channel.Jid);
            }

            vote.OnVote(initiator, "1");
        }
    }
}
