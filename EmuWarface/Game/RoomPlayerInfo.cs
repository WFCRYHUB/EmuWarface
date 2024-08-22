using EmuWarface.Game.Enums;
using EmuWarface.Game.GameRooms;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmuWarface.Game
{
    public class RoomPlayerInfo
    {
        private GameRoom _room;
        public GameRoom Room
        {
            get
            {
                if (_room == null || _room.Disposed)
                {
                    return null;
                }

                return _room;
            }
            set
            {
                _room = value;
            }
        }
        public RoomPlayerStatus Status  = RoomPlayerStatus.Ready;
        public Team     TeamId          = Team.Warface;
        public bool     Observer        = false;
        public float    Skill           = 0;
        public string   GroupId         = "";
        public string   RegionId        = "global";

        public RoomPlayerInfo(GameRoom room, string groupId, RoomPlayerStatus status = RoomPlayerStatus.Ready)
        {
            //Room            = room;
            //TeamId          = team;
            Room    = room;
            Status  = status;
            GroupId = groupId;

            if (string.IsNullOrEmpty(GroupId))
                GroupId = Guid.NewGuid().ToString();
        }
    }
}
