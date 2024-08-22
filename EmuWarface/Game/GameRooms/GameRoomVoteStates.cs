using EmuWarface.Core;
using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRoomVotes;
using EmuWarface.Game.Missions;
using EmuWarface.Xmpp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Xml;

namespace EmuWarface.Game.GameRooms
{
    public class GameRoomVoteStates : GameRoomExtension
    {
        public List<Vote> Votes = new List<Vote>();
        public MissionVote MissionVote;

        public void StartVote(Vote vote)
        {
            Votes.Add(vote);

            Utils.Delay(60).ContinueWith(task => EndVote(vote));
        }

        private void EndVote(Vote vote)
        {
            lock (vote)
            {
                if (!vote.Timeout)
                    vote.OnFinish("2");

                vote.Timeout = true;
                Votes.Remove(vote);
            }
        }

        public override XmlElement Serialize()
        {
            //<vote_states revision='71'>
            //<KickVote room_cooldown='1547833550'/>
            //</vote_states>

            return Xml.Element("vote_states")
                .Attr("revision", Revision);
        }
    }
}
