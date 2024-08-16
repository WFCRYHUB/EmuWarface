using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using EmuWarface.Game.GameRoomVotes;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmuWarface.Game.Requests
{
    public static class VotingVote
    {
        //<kick_voting_vote answer="1" team_id="1" />

        [Query(IqType.Get, "map_voting_vote", "kick_voting_vote", "pause_voting_vote", "surrender_voting_vote")]
        public static void Serializer(Client client, Iq iq)
        {
            var room = client.Profile?.Room;

            if (room == null)
                throw new InvalidOperationException("Room or profile is null");

            var q = iq.Query;

            var rVote = room.GetExtension<GameRoomVoteStates>();

            if (rVote == null)
            {
                Log.Warn("[VotingVote] Not found GameRoomVoteStates (type:{0})", q.Name);
                return;
            }

            if (q.Name == "map_voting_vote")
            {
                MissionVote missionVote = rVote.MissionVote;

                if (missionVote == null || missionVote.Timeout)
                    return;

                missionVote.OnVote(client, q.GetAttribute("mission_uid"));

                client.QueryResult(iq);
                return;
            }

            var answer  = q.GetAttribute("answer");

            Vote selVote = null;

            lock (rVote.Votes)
            {
                foreach (Vote vote in rVote.Votes)
                {
                    if (vote.Team == client.Profile.RoomPlayer.TeamId && !vote.Timeout)
                        selVote = vote;
                }
            }

            if (selVote == null)
            {
                Log.Warn("[VotingVote] Not found vote (team:{0}, type:{1})", client.Profile.RoomPlayer.TeamId, q.Name);
                return;
            }

            selVote.OnVote(client, answer);

            if (selVote.NoVoters.Count >= selVote.NoVotesRequired)
            {
                selVote.OnFinish("0");
            }
            else if (selVote.YesVoters.Count >= selVote.YesVotesRequired)
            {
                selVote.OnFinish("1");

                if (selVote.Type == VotingType.KickVote)
                    room.KickPlayer(selVote.Target, RoomPlayerRemoveReason.KickVote);
            }

            client.QueryResult(iq);
        }
    }
}
