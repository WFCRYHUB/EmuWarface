using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;

namespace EmuWarface.Game.GameRoomVotes
{
    public class Vote
    {
        public Team Team                { get; private set; }
        public VotingType Type          { get; private set; }
        public long Cooldown            { get; private set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60;

        public Client Initator          { get; private set; }
        public Client Target            { get; private set; }

        public List<Client> Voters      { get; private set; }
        public List<Client> YesVoters   { get; private set; } = new List<Client>();
        public List<Client> NoVoters    { get; private set; } = new List<Client>();


        public int YesVotesRequired => (int)Math.Round((double)Voters.Count / 2);
        public int NoVotesRequired  => Voters.Count - YesVotesRequired;

        public bool Timeout;

        public Vote(VotingType type, Team team, List<Client> voters, Client initator = null, Client target = null)
        {
            Type        = type;
            Team        = team;
            Initator    = initator;
            Target      = target;
            Voters      = voters;
        }

        public void OnVote(Client client, string answer)
        {
            if (!Voters.Contains(client) || YesVoters.Contains(client) || NoVoters.Contains(client))
                return;

            switch (answer)
            {
                case "0":
                    NoVoters.Add(client);
                    break;
                case "1":
                    YesVoters.Add(client);
                    break;
            }

            var on_voting_finished = Xml.Element("on_voting_vote")
                .Attr("yes",    YesVoters.Count)
                .Attr("no",     NoVoters.Count);

            foreach (var voter in Voters)
            {
                voter.QueryGet(on_voting_finished, voter.Channel.Jid);
            }
        }

        public void OnFinish(string result)
        {
            var on_voting_finished = Xml.Element("on_voting_finished")
                .Attr("result", result)
                .Attr("yes",    YesVoters.Count)
                .Attr("no",     NoVoters.Count);

            foreach (var voter in Voters)
            {
                voter.QueryGet(on_voting_finished, voter.Channel.Jid);
            }

            Timeout = true;

            //TODO result = 0x3 SomeTeamVotedYes
        }
    }
}
